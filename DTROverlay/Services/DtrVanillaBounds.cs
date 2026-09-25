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

    // _DTR の当たり判定は 1 フレーム内で変わらない。Follow 描画中の再走査を避ける。
    private static int _cachedFrame = -1;
    private static bool _computed;
    private static bool _success;
    private static VanillaDtrBounds _bounds;

    public static bool TryGet(out VanillaDtrBounds bounds)
    {
        var frame = ImGui.GetFrameCount();
        if (frame != _cachedFrame)
        {
            _cachedFrame = frame;
            _computed = false;
        }

        if (!_computed)
        {
            _success = TryGetUncached(out _bounds);
            _computed = true;
        }

        bounds = _bounds;
        return _success;
    }

    public static bool IsAddonVisible()
    {
        if (!TryGetAddon(out var addon))
            return false;

        return IsAddonReady(addon);
    }

    private static bool TryGetUncached(out VanillaDtrBounds bounds)
    {
        bounds = default;

        if (!TryGetAddon(out var addon))
            return false;

        if (!IsAddonReady(addon))
            return false;

        if (addon->RootNode == null || addon->UldManager.NodeList == null)
            return false;

        var scale = addon->RootNode->ScaleX;
        if (scale <= 0f)
            scale = 1f;

        if (!TryGetCollisionNode(addon, out var collision) || collision->Width <= 0)
            return false;

        var localLeft = GetRootRelativeX(collision, addon->RootNode);
        var screenLeft = addon->X + (localLeft * scale);
        var screenRight = screenLeft + (collision->Width * scale);
        if (screenRight <= screenLeft)
            return false;

        var rowHeight = collision->Height > 0 ? collision->Height * scale : addon->RootNode->Height * scale;

        if (!TryGetNativeTextMetrics(addon, scale, out var nativeTextCenterY, out var nativeTextLineHeight))
        {
            nativeTextLineHeight = rowHeight;
            nativeTextCenterY = addon->Y + (nativeTextLineHeight * 0.5f);
        }
        else if (collision->Height > 0)
        {
            nativeTextLineHeight = MathF.Min(nativeTextLineHeight, rowHeight);
        }

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

        var addonPtr = PluginServices.GameGui.GetAddonByName("_DTR");
        if (addonPtr == null)
            return false;

        addon = (AtkUnitBase*)addonPtr.Address;
        return addon != null;
    }

    private static bool IsAddonReady(AtkUnitBase* addon) =>
        addon->IsVisible
        && addon->UldManager.LoadedState == AtkLoadState.Loaded
        && addon->IsFullyLoaded();

    private static bool TryGetCollisionNode(AtkUnitBase* addon, out AtkResNode* node)
    {
        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            node = addon->UldManager.NodeList[i];
            if (node == null || !node->IsVisible() || node->Type != NodeType.Collision)
                continue;

            return true;
        }

        node = null;
        return false;
    }

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

    private static float GetRootRelativeX(AtkResNode* node, AtkResNode* root)
    {
        var x = 0f;
        for (var current = node; current != null && current != root; current = current->ParentNode)
            x += current->X;

        return x;
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
}
