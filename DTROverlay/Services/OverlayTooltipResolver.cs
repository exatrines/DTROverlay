using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class OverlayTooltipResolver
{
    public static TooltipPosition GetEffectivePosition(DtrOverlayGroup group) =>
        group is { OverrideTooltipPositionEnabled: true }
            ? group.OverrideTooltipPosition
            : C.TooltipPosition;

    public static float GetEffectiveFontSizePx(DtrOverlayGroup group) =>
        group is { OverrideTooltipFontSizePxEnabled: true }
            ? group.OverrideTooltipFontSizePx
            : C.TooltipFontSizePx;

    public static Vector4 GetEffectiveTextColor(DtrOverlayGroup group) =>
        group is { OverrideTooltipTextColorEnabled: true }
            ? group.OverrideTooltipTextColor
            : C.TooltipTextColor;

    public static Vector4 GetEffectiveBackgroundColor(DtrOverlayGroup group) =>
        group is { OverrideTooltipBackgroundColorEnabled: true }
            ? group.OverrideTooltipBackgroundColor
            : C.TooltipBackgroundColor;
}
