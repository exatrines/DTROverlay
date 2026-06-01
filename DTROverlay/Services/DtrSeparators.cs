using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class DtrSeparators
{
    /// <summary>Visible glyph when separator bars are off (slot width still comes from separator width settings).</summary>
    public const string SlotGlyph = "";

    public static bool ShouldInsertDivision(bool hasNativeOverlayEntries, bool hasPluginOverlayEntries) =>
        OverlayGroupLayout.ShouldInsertDivisionSeparator(
            hasNativeOverlayEntries,
            hasPluginOverlayEntries);

    public static VisibleDtrEntry CreateNative(float opacity = 1f) =>
        Create(OverlayEntryIds.NativeSeparatorColor, opacity);

    public static VisibleDtrEntry CreatePlugin(float opacity = 1f) =>
        Create(OverlayEntryIds.PluginSeparatorColor, opacity);

    public static VisibleDtrEntry CreateDivision(float opacity = 1f) =>
        Create(OverlayEntryIds.DivisionSeparatorColor, opacity);

    private static VisibleDtrEntry Create(string layoutKey, float opacity) =>
        VisibleDtrEntry.FromText(
            SlotGlyph,
            layoutKey: layoutKey,
            colorLayoutKey: layoutKey,
            opacity: opacity);
}
