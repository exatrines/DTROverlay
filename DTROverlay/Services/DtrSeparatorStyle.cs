using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class DtrSeparatorStyle
{
    public static bool IsSeparatorKey(string layoutKey) =>
        layoutKey is OverlayEntryIds.PluginSeparatorColor
            or OverlayEntryIds.NativeSeparatorColor
            or OverlayEntryIds.DivisionSeparatorColor;

    public static bool ShowsBar(string layoutKey) =>
        layoutKey switch
        {
            OverlayEntryIds.PluginSeparatorColor => OverlayGroupSettings.ShowsPluginSeparatorBar(),
            OverlayEntryIds.NativeSeparatorColor => OverlayGroupSettings.ShowsNativeSeparatorBar(),
            OverlayEntryIds.DivisionSeparatorColor => OverlayGroupSettings.ShowsDivisionSeparatorBar(),
            _ => false,
        };

    public static string GetDisplayGlyph(string layoutKey) =>
        ShowsBar(layoutKey) ? "|" : DtrSeparators.SlotGlyph;

    public static float ResolveSlotWidth(string layoutKey, float measuredWidth)
    {
        var widthPx = OverlayStyleResolver.GetEffectiveSeparatorSlotWidthPx();
        if (!IsSeparatorKey(layoutKey) || widthPx <= 0)
            return measuredWidth;

        return DtrEntrySlotWidth.ScaleToOverlayPixels(widthPx);
    }

    public static Vector4 GetTextColor(string layoutKey) =>
        EntryFixedWidth.GetTextColor(layoutKey);

    public static Vector4 GetOutlineColor(string layoutKey) =>
        EntryFixedWidth.GetOutlineColor(layoutKey);

    public static Vector4 GetShadowColor(string layoutKey) =>
        EntryFixedWidth.GetShadowColor(layoutKey);

    public static float GetEdgeStrength(string layoutKey) =>
        EntryFixedWidth.GetEdgeStrength(layoutKey);

    public static float GetShadowThickness(string layoutKey) =>
        EntryFixedWidth.GetShadowThickness(layoutKey);

    public static bool IsEdgeEnabled(string layoutKey) =>
        EntryFixedWidth.IsEdgeEnabled(layoutKey);

    public static bool IsShadowEnabled(string layoutKey) =>
        EntryFixedWidth.IsShadowEnabled(layoutKey);

    public static void ResetColors(string layoutKey) =>
        EntryFixedWidth.ResetColorsToDefault(layoutKey);

    public static void MigrateVisibilitySplit()
    {
        if (C.SeparatorVisibilitySplitMigrated)
            return;

        C.ShowNativeEntrySeparators = C.ShowPluginEntrySeparators;
        C.SeparatorVisibilitySplitMigrated = true;
    }

    public static void MigrateDivisionSeparatorBar()
    {
        if (C.DivisionSeparatorBarMigrated)
            return;

        C.ShowDivisionSeparatorBar = C.ShowPluginEntrySeparators;
        C.DivisionSeparatorBarMigrated = true;
    }
}
