using Dalamud.Interface.Utility;
using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class OverlayPositionOriginHelper
{
    public static bool IsRight(OverlayPositionOrigin origin) =>
        origin is OverlayPositionOrigin.TopRight or OverlayPositionOrigin.BottomRight;

    public static bool IsBottom(OverlayPositionOrigin origin) =>
        origin is OverlayPositionOrigin.BottomLeft or OverlayPositionOrigin.BottomRight;

    public static Vector2 GetPivot(OverlayPositionOrigin origin) =>
        origin switch
        {
            OverlayPositionOrigin.TopRight => new(1f, 0f),
            OverlayPositionOrigin.BottomLeft => new(0f, 1f),
            OverlayPositionOrigin.BottomRight => new(1f, 1f),
            _ => Vector2.Zero,
        };

    public static Vector2 GetAnchorScreenPosition(DtrOverlayGroup group, OverlayPositionOrigin origin)
    {
        var viewport = ImGuiHelpers.MainViewport;
        var x = IsRight(origin)
            ? viewport.Pos.X + viewport.Size.X - group.OverlayPosition.X
            : viewport.Pos.X + group.OverlayPosition.X;

        var y = IsBottom(origin)
            ? viewport.Pos.Y + viewport.Size.Y - group.OverlayPosition.Y
            : viewport.Pos.Y + group.OverlayPosition.Y + DtrStyle.VerticalOffset;

        return new Vector2(x, y);
    }

    public static Vector2 GetWindowTopLeft(Vector2 anchor, OverlayPositionOrigin origin, float width, float height)
    {
        var pivot = GetPivot(origin);
        return new Vector2(anchor.X - pivot.X * width, anchor.Y - pivot.Y * height);
    }

    public static void SetOverlayPositionFromWindowTopLeft(
        DtrOverlayGroup group,
        OverlayPositionOrigin origin,
        Vector2 windowTopLeft,
        float width,
        float height)
    {
        var viewport = ImGuiHelpers.MainViewport;
        var left = viewport.Pos.X;
        var top = viewport.Pos.Y;
        var right = left + viewport.Size.X;
        var bottom = top + viewport.Size.Y;

        group.OverlayPosition = origin switch
        {
            OverlayPositionOrigin.TopRight => new(
                right - windowTopLeft.X - width,
                windowTopLeft.Y - top - DtrStyle.VerticalOffset),
            OverlayPositionOrigin.BottomLeft => new(
                windowTopLeft.X - left,
                bottom - windowTopLeft.Y - height),
            OverlayPositionOrigin.BottomRight => new(
                right - windowTopLeft.X - width,
                bottom - windowTopLeft.Y - height),
            _ => new(
                windowTopLeft.X - left,
                windowTopLeft.Y - top - DtrStyle.VerticalOffset),
        };
    }
}
