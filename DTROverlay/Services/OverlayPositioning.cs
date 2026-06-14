using Dalamud.Interface.Utility;
using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class OverlayPositioning
{
    public static void MigrateLegacyTopLeftAnchor(DtrOverlayGroup group)
    {
        if (C.OverlayPositionOriginMigrated)
            return;

        var viewport = ImGuiHelpers.MainViewport;
        group.OverlayPosition.X = MathF.Max(0f, viewport.Size.X - group.OverlayPosition.X);
        group.OverlayPositionOrigin = OverlayPositionOrigin.TopRight;
        C.OverlayPositionOriginMigrated = true;
    }

    public static void ApplyWindowPosition(DtrOverlayGroup group)
    {
        if (FollowVanillaDtrMode.IsActive && TryApplyFollowVanillaPosition())
            return;

        var anchor = OverlayPositionOriginHelper.GetAnchorScreenPosition(group, group.OverlayPositionOrigin);
        var pivot = OverlayPositionOriginHelper.GetPivot(group.OverlayPositionOrigin);
        ImGui.SetNextWindowPos(anchor, ImGuiCond.Always, pivot);
    }

    private static bool TryApplyFollowVanillaPosition(bool inFrame = false)
    {
        if (!TryGetFollowVanillaWindowPos(out var pos, out var pivot))
            return false;

        if (inFrame)
            ImGui.SetWindowPos(new Vector2(ImGui.GetWindowPos().X, pos.Y));
        else
            ImGui.SetNextWindowPos(pos, ImGuiCond.Always, pivot);

        return true;
    }

    public static void RefineFollowVanillaPositionInFrame()
    {
        if (!FollowVanillaDtrMode.IsActive || !DtrVanillaBounds.TryGet(out var bounds, useScreenCoordinates: true))
            return;

        var lineHeight = DtrImGui.GetHorizontalRowLineHeight();
        var windowY = GetFollowVanillaOverlayY(bounds, lineHeight, contentRegionMinY: 0f);
        var windowSize = ImGui.GetWindowSize();
        var xOffset = C.FollowVanillaHorizontalOffset;

        var pos = C.FollowVanillaDtrSide == FollowVanillaDtrSide.Left
            ? new Vector2(bounds.ScreenLeft - windowSize.X + xOffset, windowY)
            : new Vector2(bounds.BarScreenRight + xOffset, windowY);

        ImGui.SetWindowPos(pos);
        ImGui.SetCursorPos(Vector2.Zero);
    }

    private static bool TryGetFollowVanillaWindowPos(out Vector2 pos, out Vector2 pivot)
    {
        pos = default;
        pivot = Vector2.Zero;

        if (!DtrVanillaBounds.TryGet(out var bounds))
            return false;

        var overlayY = GetFollowVanillaOverlayY(bounds, FollowVanillaFontScale.EstimateLineHeight());
        var xOffset = C.FollowVanillaHorizontalOffset;
        if (C.FollowVanillaDtrSide == FollowVanillaDtrSide.Left)
        {
            pos = new Vector2(bounds.ScreenLeft + xOffset, overlayY);
            pivot = new Vector2(1f, 0f);
        }
        else
        {
            pos = new Vector2(bounds.BarScreenRight + xOffset, overlayY);
            pivot = Vector2.Zero;
        }

        return true;
    }

    private static float GetFollowVanillaOverlayY(
        VanillaDtrBounds bounds,
        float overlayLineHeight,
        float contentRegionMinY = 0f) =>
        bounds.GetOverlayWindowY(overlayLineHeight, contentRegionMinY) + C.FollowVanillaVerticalOffset;

    public static void ApplyDragDelta(DtrOverlayGroup group, Vector2 delta)
    {
        if (OverlayPositionOriginHelper.IsRight(group.OverlayPositionOrigin))
            group.OverlayPosition.X -= delta.X;
        else
            group.OverlayPosition.X += delta.X;

        if (OverlayPositionOriginHelper.IsBottom(group.OverlayPositionOrigin))
            group.OverlayPosition.Y -= delta.Y;
        else
            group.OverlayPosition.Y += delta.Y;

        EzConfig.Save();
    }

    public static void OnOriginChanged(
        DtrOverlayGroup group,
        OverlayPositionOrigin previousOrigin,
        float lastWindowWidth,
        float lastWindowHeight)
    {
        if (previousOrigin == group.OverlayPositionOrigin || lastWindowWidth <= 0f || lastWindowHeight <= 0f)
            return;

        var anchor = OverlayPositionOriginHelper.GetAnchorScreenPosition(group, previousOrigin);
        var topLeft = OverlayPositionOriginHelper.GetWindowTopLeft(
            anchor,
            previousOrigin,
            lastWindowWidth,
            lastWindowHeight);

        OverlayPositionOriginHelper.SetOverlayPositionFromWindowTopLeft(
            group,
            group.OverlayPositionOrigin,
            topLeft,
            lastWindowWidth,
            lastWindowHeight);

        EzConfig.Save();
    }
}
