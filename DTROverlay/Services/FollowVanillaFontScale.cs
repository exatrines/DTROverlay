using Dalamud.Interface.Utility;
using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class FollowVanillaFontScale
{
    private const float MinScale = 0.5f;
    private const float MaxScale = 3f;
    private const string ReferenceSample = "ET 00:00";
    private const float DefaultContentToLineRatio = 0.86f;

    private static float _cachedVanillaMatchScale = 1f;
    private static float _cachedUnitLineHeight;
    private static float _cachedContentToLineRatio = DefaultContentToLineRatio;
    private static float _cachedNativeRowHeight;

    public static float ActiveScale =>
        FollowVanillaDtrMode.IsActive
            ? Math.Clamp(_cachedVanillaMatchScale * C.FollowVanillaFontSizeScale, MinScale, MaxScale)
            : C.OverlayFontSizeScale;

    public static float NativeRowHeight => _cachedNativeRowHeight;

    public static void UpdateFromBounds(VanillaDtrBounds bounds)
    {
        if (!bounds.IsValid)
            return;

        var unitLineHeight = MeasureUnitScaleLineHeight();
        if (unitLineHeight <= 0f)
            return;

        _cachedUnitLineHeight = unitLineHeight;
        _cachedNativeRowHeight = bounds.RowHeight;
        _cachedContentToLineRatio = MeasureContentToLineHeightRatio(unitLineHeight);

        // Collision row height is stable; min visible glyph height drops in duty (ET/LT labels vs WorldInfo).
        var referenceHeight = bounds.RowHeight > 0f
            ? bounds.RowHeight
            : bounds.NativeTextLineHeight;
        var targetHeight = referenceHeight * DtrStyle.FollowVanillaFontRowHeightRatio;
        _cachedVanillaMatchScale = Math.Clamp(targetHeight / unitLineHeight, MinScale, MaxScale);
    }

    /// <summary>Line height at the default Dalamud overlay font size (before user scale multipliers).</summary>
    public static float MeasureUnitScaleLineHeight() => UiBuilder.DefaultFontSizePx;

    public static float EstimateLineHeight() => DtrOverlayFonts.GetTargetSizePx();

    public static float EstimateContentHeight() =>
        EstimateLineHeight() * _cachedContentToLineRatio;

    public static float EstimateTextDrawTopInset()
    {
        var lineHeight = EstimateLineHeight();
        return (lineHeight - EstimateContentHeight()) * 0.5f;
    }

    private static float MeasureContentToLineHeightRatio(float unitLineHeight)
    {
        if (unitLineHeight <= 0f)
            return DefaultContentToLineRatio;

        var contentHeight = ImGui.CalcTextSize(ReferenceSample).Y;
        if (contentHeight <= 0f)
            return DefaultContentToLineRatio;

        return contentHeight / unitLineHeight;
    }
}
