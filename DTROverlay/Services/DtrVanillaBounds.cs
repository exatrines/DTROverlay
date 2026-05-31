using DTROverlay.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace DTROverlay.Services;

public readonly record struct VanillaDtrBounds(
    float ScreenLeft,
    float NativeTextCenterY,
    float NativeTextLineHeight,
    float ScreenRight,
    float Width,
    float RowHeight)
{
    public bool IsValid => Width > 0f && NativeTextCenterY > 0f && NativeTextLineHeight > 0f;

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

    public static bool TryGet(out VanillaDtrBounds bounds)
    {
        bounds = default;

        var addonPtr = Svc.GameGui.GetAddonByName("_DTR");
        if (addonPtr == null)
            return false;

        var addon = (AtkUnitBase*)addonPtr.Address;
        if (addon == null || addon->RootNode == null || addon->UldManager.NodeList == null)
            return false;

        var scale = addon->RootNode->ScaleX;
        if (scale <= 0f)
            scale = 1f;

        var minX = (float)addon->RootNode->Width;
        var foundNative = false;

        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            var node = addon->UldManager.NodeList[i];
            if (node == null || !node->IsVisible() || node->NodeId >= DalamudNodeIdBase)
                continue;

            var nodeType = node->Type;
            if (nodeType == NodeType.Collision)
                continue;

            if (nodeType == NodeType.Res)
            {
                if (node->ChildNode == null || !node->ChildNode->IsVisible())
                    continue;

                minX = MathF.Min(minX, node->X);
                foundNative = true;
                continue;
            }

            if (nodeType != NodeType.Text
                && nodeType != NodeType.Image
                && (ushort)nodeType < 1000)
                continue;

            minX = MathF.Min(minX, node->X);
            foundNative = true;
        }

        if (!foundNative || !TryGetNativeTextMetrics(addon, scale, out var nativeTextCenterY, out var nativeTextLineHeight))
            return false;

        var nativeWidth = addon->RootNode->Width - minX;
        if (nativeWidth <= 0f)
            return false;

        var screenLeft = addon->X + (minX * scale);
        var screenRight = addon->X + ((minX + nativeWidth) * scale);
        TryGetCollisionRowHeight(addon, scale, out var rowHeight);
        if (rowHeight > 0f)
            nativeTextLineHeight = MathF.Min(nativeTextLineHeight, rowHeight);

        bounds = new VanillaDtrBounds(
            screenLeft,
            nativeTextCenterY,
            nativeTextLineHeight,
            screenRight,
            screenRight - screenLeft,
            rowHeight);
        return bounds.IsValid;
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
            if (!node->IsVisible() || node->NodeId >= DalamudNodeIdBase)
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
            && node->IsVisible()
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
