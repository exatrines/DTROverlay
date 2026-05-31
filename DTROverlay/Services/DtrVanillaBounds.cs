using DTROverlay.UI;
using ECommons;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace DTROverlay.Services;

public readonly record struct VanillaDtrBounds(
    float ScreenLeft,
    float NativeTextCenterY,
    float NativeTextLineHeight,
    float ScreenRight,
    float Width,
    float RowHeight,
    float BarScreenLeft,
    float BarWidth)
{
    public bool IsValid => Width > 0f && NativeTextCenterY > 0f && NativeTextLineHeight > 0f;

    /// <summary>Right edge of the native DTR root node (BarScreenLeft + BarWidth).</summary>
    public float BarScreenRight => BarScreenLeft + BarWidth;

    /// <summary>
    /// Window Y so the overlay row center (ImGui line height) matches native text cluster center.
    /// </summary>
    public float GetOverlayWindowY(float overlayLineHeight, float contentRegionMinY = 0f) =>
        NativeTextCenterY
        - (overlayLineHeight * 0.5f)
        - contentRegionMinY;
}

internal static unsafe class DtrVanillaBounds
{
    private const uint DalamudNodeIdBase = 1000;

    public static bool IsAddonVisible()
    {
        if (!TryGetAddon(out var addon))
            return false;

        return GenericHelpers.IsAddonReady(addon);
    }

    public static bool TryGet(out VanillaDtrBounds bounds, bool useScreenCoordinates = false)
    {
        bounds = default;

        if (!TryGetAddon(out var addon))
            return false;

        if (!GenericHelpers.IsAddonReady(addon))
            return false;

        if (addon->RootNode == null || addon->UldManager.NodeList == null)
            return false;

        var scale = addon->RootNode->ScaleX;
        if (scale <= 0f)
            scale = 1f;

        if (!TryGetDisplayedNativeHorizontalBounds(addon, scale, useScreenCoordinates, out var screenLeft, out var screenRight))
            return false;

        if (!TryGetNativeTextMetrics(addon, scale, out var nativeTextCenterY, out var nativeTextLineHeight))
            ApplyFallbackNativeTextMetrics(addon, scale, out nativeTextCenterY, out nativeTextLineHeight);

        TryGetCollisionRowHeight(addon, scale, out var rowHeight);
        if (rowHeight > 0f)
            nativeTextLineHeight = MathF.Min(nativeTextLineHeight, rowHeight);

        var barScreenLeft = addon->X;
        var barWidth = addon->RootNode->Width * scale;

        bounds = new VanillaDtrBounds(
            screenLeft,
            nativeTextCenterY,
            nativeTextLineHeight,
            screenRight,
            screenRight - screenLeft,
            rowHeight,
            barScreenLeft,
            barWidth);
        return bounds.IsValid;
    }

    private static bool TryGetAddon(out AtkUnitBase* addon)
    {
        addon = null;

        var addonPtr = Svc.GameGui.GetAddonByName("_DTR");
        if (addonPtr == null)
            return false;

        addon = (AtkUnitBase*)addonPtr.Address;
        return addon != null;
    }

    private static bool TryGetDisplayedNativeHorizontalBounds(
        AtkUnitBase* addon,
        float scale,
        bool preferScreenCoordinates,
        out float screenLeft,
        out float screenRight)
    {
        screenLeft = float.MaxValue;
        screenRight = float.MinValue;
        var minLocalX = float.MaxValue;
        var maxLocalX = float.MinValue;
        var foundScreen = false;
        var foundLocal = false;
        var root = addon->RootNode;

        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            var node = addon->UldManager.NodeList[i];
            if (node == null || node->NodeId >= DalamudNodeIdBase || !IsEffectivelyVisible(node))
                continue;

            if (node->Type == NodeType.Collision)
                continue;

            if (node->Type == NodeType.Res)
            {
                if (node == root)
                    continue;

                AccumulateSubtreeHorizontalBounds(
                    addon,
                    scale,
                    root,
                    preferScreenCoordinates,
                    node,
                    ref screenLeft,
                    ref screenRight,
                    ref foundScreen,
                    ref minLocalX,
                    ref maxLocalX,
                    ref foundLocal);
                continue;
            }

            if (!IsDisplayLeafNode(node))
                continue;

            if (HasVisibleResAncestor(node, root))
                continue;

            ExtendHorizontalBounds(
                addon,
                scale,
                root,
                preferScreenCoordinates,
                node,
                ref screenLeft,
                ref screenRight,
                ref foundScreen,
                ref minLocalX,
                ref maxLocalX,
                ref foundLocal);
        }

        if (preferScreenCoordinates && foundScreen && screenRight > screenLeft)
            return true;

        if (foundLocal && maxLocalX > minLocalX)
        {
            screenLeft = addon->X + (minLocalX * scale);
            screenRight = addon->X + (maxLocalX * scale);
            return true;
        }

        if (foundScreen && screenRight > screenLeft)
            return true;

        return false;
    }

    private static bool IsDisplayLeafNode(AtkResNode* node) =>
        node->Type == NodeType.Text || node->Type == NodeType.Image;

    private static bool IsEffectivelyVisible(AtkResNode* node)
    {
        if (node == null || !node->IsVisible())
            return false;

        for (var ancestor = node->ParentNode; ancestor != null; ancestor = ancestor->ParentNode)
        {
            if (!ancestor->IsVisible())
                return false;
        }

        return true;
    }

    private static bool HasVisibleResAncestor(AtkResNode* node, AtkResNode* root)
    {
        for (var ancestor = node->ParentNode; ancestor != null && ancestor != root; ancestor = ancestor->ParentNode)
        {
            if (ancestor->Type == NodeType.Res && IsEffectivelyVisible(ancestor))
                return true;
        }

        return false;
    }

    private static void AccumulateSubtreeHorizontalBounds(
        AtkUnitBase* addon,
        float scale,
        AtkResNode* root,
        bool preferScreenCoordinates,
        AtkResNode* node,
        ref float screenLeft,
        ref float screenRight,
        ref bool foundScreen,
        ref float minLocalX,
        ref float maxLocalX,
        ref bool foundLocal)
    {
        if (node == null || !IsEffectivelyVisible(node))
            return;

        if (node->Type == NodeType.Res)
            ExtendContainerHorizontalBounds(addon, scale, root, node, ref screenLeft, ref screenRight, ref foundScreen, ref minLocalX, ref maxLocalX, ref foundLocal);

        if (IsDisplayLeafNode(node))
        {
            ExtendHorizontalBounds(
                addon,
                scale,
                root,
                preferScreenCoordinates,
                node,
                ref screenLeft,
                ref screenRight,
                ref foundScreen,
                ref minLocalX,
                ref maxLocalX,
                ref foundLocal);
        }

        for (var child = node->ChildNode; child != null; child = child->NextSiblingNode)
        {
            AccumulateSubtreeHorizontalBounds(
                addon,
                scale,
                root,
                preferScreenCoordinates,
                child,
                ref screenLeft,
                ref screenRight,
                ref foundScreen,
                ref minLocalX,
                ref maxLocalX,
                ref foundLocal);
        }
    }

    private static float GetRootRelativeX(AtkResNode* node, AtkResNode* root)
    {
        var x = 0f;
        for (var current = node; current != null && current != root; current = current->ParentNode)
            x += current->X;

        return x;
    }

    private static void ExtendContainerHorizontalBounds(
        AtkUnitBase* addon,
        float scale,
        AtkResNode* root,
        AtkResNode* container,
        ref float screenLeft,
        ref float screenRight,
        ref bool foundScreen,
        ref float minLocalX,
        ref float maxLocalX,
        ref bool foundLocal)
    {
        if (container->Width <= 0)
            return;

        var localLeft = GetRootRelativeX(container, root);
        var localRight = localLeft + container->Width;
        if (localRight <= localLeft)
            return;

        minLocalX = MathF.Min(minLocalX, localLeft);
        maxLocalX = MathF.Max(maxLocalX, localRight);
        foundLocal = true;

        var containerScreenLeft = addon->X + (localLeft * scale);
        var containerScreenRight = addon->X + (localRight * scale);
        screenLeft = MathF.Min(screenLeft, containerScreenLeft);
        screenRight = MathF.Max(screenRight, containerScreenRight);
        foundScreen = true;
    }

    private static void ExtendHorizontalBounds(
        AtkUnitBase* addon,
        float scale,
        AtkResNode* root,
        bool preferScreenCoordinates,
        AtkResNode* node,
        ref float screenLeft,
        ref float screenRight,
        ref bool foundScreen,
        ref float minLocalX,
        ref float maxLocalX,
        ref bool foundLocal)
    {
        var width = GetNodeWidth(node, scale);
        if (width <= 0f)
            return;

        if (preferScreenCoordinates && TryGetNodeScreenLeft(node, out var nodeScreenLeft))
        {
            var nodeScreenRight = nodeScreenLeft + width;
            if (nodeScreenRight <= nodeScreenLeft)
                return;

            screenLeft = MathF.Min(screenLeft, nodeScreenLeft);
            screenRight = MathF.Max(screenRight, nodeScreenRight);
            foundScreen = true;
            return;
        }

        var localLeft = GetRootRelativeX(node, root);
        var localRight = localLeft + (width / scale);
        if (localRight <= localLeft)
            return;

        minLocalX = MathF.Min(minLocalX, localLeft);
        maxLocalX = MathF.Max(maxLocalX, localRight);
        foundLocal = true;
    }

    private static bool TryGetNodeScreenLeft(AtkResNode* node, out float screenLeft)
    {
        screenLeft = 0f;

        if (node->Type == NodeType.Text)
        {
            var textNode = node->GetAsAtkTextNode();
            if (textNode != null && textNode->ScreenX > 0f)
            {
                screenLeft = textNode->ScreenX;
                return true;
            }
        }

        if (node->ScreenX > 0f)
        {
            screenLeft = node->ScreenX;
            return true;
        }

        return false;
    }

    private static float GetNodeWidth(AtkResNode* node, float scale)
    {
        if (node->Type == NodeType.Text)
        {
            var textNode = node->GetAsAtkTextNode();
            if (textNode == null)
                return node->Width * scale;

            ushort drawWidth = 0;
            ushort drawHeight = 0;
            textNode->GetTextDrawSize(&drawWidth, &drawHeight, null, 0, -1, true);
            if (drawWidth > 0)
                return drawWidth * scale;
        }

        if (node->Type == NodeType.Image)
        {
            var imageNode = node->GetAsAtkImageNode();
            if (imageNode != null)
            {
                if (imageNode->Width > 0)
                    return imageNode->Width * scale;

                if (DtrNativeImage.TryCreate(imageNode, out var info))
                {
                    var width = info.NodeWidth > 0 ? info.NodeWidth : info.PartWidth;
                    if (width > 0)
                        return width * scale;
                }
            }
        }

        if (node->Width > 0)
            return node->Width * scale;

        return 0f;
    }

    private static bool TryGetNativeTextMetrics(
        AtkUnitBase* addon,
        float scale,
        out float centerY,
        out float lineHeight)
    {
        centerY = 0f;
        lineHeight = 0f;
        var minTop = float.MaxValue;
        var maxBottom = float.MinValue;
        var glyphHeights = new List<float>();
        var found = false;

        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            var node = addon->UldManager.NodeList[i];
            if (node == null || node->NodeId >= DalamudNodeIdBase || !IsEffectivelyVisible(node))
                continue;

            AccumulateNativeTextMetrics(node, scale, ref minTop, ref maxBottom, glyphHeights, ref found);
        }

        if (!found || minTop >= maxBottom || glyphHeights.Count == 0)
            return false;

        centerY = (minTop + maxBottom) * 0.5f;
        glyphHeights.Sort();
        lineHeight = glyphHeights[0];
        return lineHeight > 0f;
    }

    private static void ApplyFallbackNativeTextMetrics(
        AtkUnitBase* addon,
        float scale,
        out float centerY,
        out float lineHeight)
    {
        TryGetCollisionRowHeight(addon, scale, out var rowHeight);
        lineHeight = rowHeight > 0f ? rowHeight : addon->RootNode->Height * scale;
        centerY = addon->Y + (lineHeight * 0.5f);
    }

    private static void AccumulateNativeTextMetrics(
        AtkResNode* node,
        float scale,
        ref float minTop,
        ref float maxBottom,
        List<float> glyphHeights,
        ref bool found)
    {
        if (node == null)
            return;

        if (node->NodeId < DalamudNodeIdBase
            && IsEffectivelyVisible(node)
            && node->Type == NodeType.Text)
        {
            var textNode = node->GetAsAtkTextNode();
            if (textNode != null && textNode->Height > 0)
            {
                var top = textNode->ScreenY;
                var boxHeight = textNode->Height * scale;
                minTop = MathF.Min(minTop, top);
                maxBottom = MathF.Max(maxBottom, top + boxHeight);

                ushort drawWidth = 0;
                ushort drawHeight = 0;
                textNode->GetTextDrawSize(&drawWidth, &drawHeight, null, 0, -1, true);
                var glyphHeight = drawHeight > 0 ? drawHeight * scale : boxHeight;
                glyphHeights.Add(glyphHeight);
                found = true;
            }
        }

        if (node->ChildNode != null)
            AccumulateNativeTextMetrics(node->ChildNode, scale, ref minTop, ref maxBottom, glyphHeights, ref found);

        if (node->NextSiblingNode != null)
            AccumulateNativeTextMetrics(node->NextSiblingNode, scale, ref minTop, ref maxBottom, glyphHeights, ref found);
    }

    private static void TryGetCollisionRowHeight(AtkUnitBase* addon, float scale, out float rowHeight)
    {
        rowHeight = addon->RootNode->Height * scale;

        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            var node = addon->UldManager.NodeList[i];
            if (node == null || !node->IsVisible() || node->Type != NodeType.Collision)
                continue;

            rowHeight = node->Height * scale;
            return;
        }
    }
}
