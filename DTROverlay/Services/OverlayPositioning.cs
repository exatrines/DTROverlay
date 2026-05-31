using Dalamud.Interface.Utility;
using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class OverlayPositioning
{
    public static float LastWindowWidth { get; private set; }

    public static float LastWindowHeight { get; private set; }

    public static void SetLastWindowSize(Vector2 size)
    {
        if (size.X > 0f)
            LastWindowWidth = size.X;

        if (size.Y > 0f)
            LastWindowHeight = size.Y;
    }

    public static void MigrateLegacyTopLeftAnchor()
    {
        if (C.OverlayPositionOriginMigrated)
            return;

        var viewport = ImGuiHelpers.MainViewport;
        C.OverlayPosition.X = MathF.Max(0f, viewport.Size.X - C.OverlayPosition.X);
        C.OverlayPositionOrigin = OverlayPositionOrigin.TopRight;
        C.OverlayPositionOriginMigrated = true;
    }

    public static void ApplyWindowPosition()
    {
        if (FollowVanillaDtrMode.IsActive && TryApplyFollowVanillaPosition())
            return;

        var viewport = ImGuiHelpers.MainViewport;
        var y = viewport.Pos.Y + C.OverlayPosition.Y + DtrStyle.VerticalOffset;

        Vector2 pos;
        Vector2 pivot;
        if (C.OverlayPositionOrigin == OverlayPositionOrigin.TopRight)
        {
            pos = new Vector2(
                viewport.Pos.X + viewport.Size.X - C.OverlayPosition.X,
                y);
            pivot = new Vector2(1f, 0f);
        }
        else
        {
            pos = new Vector2(viewport.Pos.X + C.OverlayPosition.X, y);
            pivot = Vector2.Zero;
        }

        ImGui.SetNextWindowPos(pos, ImGuiCond.Always, pivot);
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
        // WindowPadding is zeroed before Begin (PreDraw); do not read stale content-region padding here.
        var windowY = GetFollowVanillaOverlayY(bounds, lineHeight, contentRegionMinY: 0f);
        var windowSize = ImGui.GetWindowSize();
        var xOffset = C.FollowVanillaHorizontalOffset;

        var pos = C.FollowVanillaDtrSide == FollowVanillaDtrSide.Left
            ? new Vector2(bounds.ScreenLeft - windowSize.X + xOffset, windowY)
            : new Vector2(bounds.BarScreenRight + xOffset, windowY);

        ImGui.SetWindowPos(pos);
        // Right side keeps PreDraw X, so ImGui may not reset the cursor after Y-only SetWindowPos.
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

    public static void ApplyDragDelta(Vector2 delta)
    {
        if (C.OverlayPositionOrigin == OverlayPositionOrigin.TopRight)
            C.OverlayPosition.X -= delta.X;
        else
            C.OverlayPosition.X += delta.X;

        C.OverlayPosition.Y += delta.Y;
    }

    public static void OnOriginChanged(OverlayPositionOrigin previousOrigin)
    {
        if (previousOrigin == C.OverlayPositionOrigin || LastWindowWidth <= 0f)
            return;

        var viewport = ImGuiHelpers.MainViewport;
        if (previousOrigin == OverlayPositionOrigin.TopLeft
            && C.OverlayPositionOrigin == OverlayPositionOrigin.TopRight)
        {
            var rightEdge = viewport.Pos.X + C.OverlayPosition.X + LastWindowWidth;
            C.OverlayPosition.X = viewport.Pos.X + viewport.Size.X - rightEdge;
        }
        else if (previousOrigin == OverlayPositionOrigin.TopRight
            && C.OverlayPositionOrigin == OverlayPositionOrigin.TopLeft)
        {
            var rightEdge = viewport.Pos.X + viewport.Size.X - C.OverlayPosition.X;
            C.OverlayPosition.X = rightEdge - LastWindowWidth - viewport.Pos.X;
        }
    }
}
