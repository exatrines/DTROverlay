using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class DtrSeparatorStyle
{
    public static bool IsPluginVisible => C.ShowPluginEntrySeparators;

    public static bool IsNativeVisible => C.ShowNativeEntrySeparators;

    public static bool IsSeparatorKey(string layoutKey) =>
        layoutKey is OverlayEntryIds.PluginSeparatorColor
            or OverlayEntryIds.NativeSeparatorColor
            or OverlayEntryIds.DivisionSeparatorColor;

    public static Vector4 GetTextColor(string layoutKey) =>
        EntryFixedWidth.IsColorEnabled(layoutKey)
        && C.FixedWidthTextColors.TryGetValue(layoutKey, out var color)
            ? color
            : EntryFixedWidth.GetDefaultTextColor();

    public static Vector4 GetOutlineColor(string layoutKey) =>
        EntryFixedWidth.IsColorEnabled(layoutKey)
        && C.FixedWidthOutlineColors.TryGetValue(layoutKey, out var color)
            ? color
            : EntryFixedWidth.GetDefaultOutlineColor();

    public static void ResetColors(string layoutKey)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        C.FixedWidthTextColors[layoutKey] = EntryFixedWidth.GetDefaultTextColor();
        C.FixedWidthOutlineColors[layoutKey] = EntryFixedWidth.GetDefaultOutlineColor();
    }

    public static void MigrateVisibilitySplit()
    {
        if (C.SeparatorVisibilitySplitMigrated)
            return;

        C.ShowNativeEntrySeparators = C.ShowPluginEntrySeparators;
        C.SeparatorVisibilitySplitMigrated = true;
    }
}
