using System.Linq;
using Dalamud.Game.Gui.Dtr;

namespace DTROverlay.Services;

internal enum DtrSlotWidthSource
{
    None,
    /// <summary>Fixed slot from plugin <see cref="IReadOnlyDtrBarEntry.MinimumWidth"/>.</summary>
    PluginMinimumWidth,
    /// <summary>Fixed slot from DTROverlay overlay settings.</summary>
    OverlayMinWidth,
}

internal static class DtrEntrySlotWidth
{
    /// <summary>
    /// Overlay font scale applied to DTR width values for the current <see cref="OverlayStyleContext"/> group.
    /// </summary>
    public static float ScaleFactor => OverlayStyleResolver.GetEffectiveOverlayFontScale();

    /// <summary>
    /// Resolves overlay slot width. Plugin or overlay minimum widths use a fixed scaled slot (ignores measured width).
    /// </summary>
    public static float ResolveWidth(string layoutKey, float measuredWidth)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return measuredWidth;

        var pluginTitle = GroupStyleKeys.GetPluginTitleFromLayoutKey(layoutKey);
        var entry = Svc.DtrBar.Entries.FirstOrDefault(e => e.Title == pluginTitle);
        if (entry == null)
            return measuredWidth;

        if (TryGetFixedRawWidth(entry, out var rawWidth))
            return ScaleToOverlayPixels(rawWidth);

        return measuredWidth;
    }

    public static bool TryGetFixedRawWidth(IReadOnlyDtrBarEntry entry, out int rawWidthPx)
    {
        rawWidthPx = 0;

        if (entry.MinimumWidth > 0)
        {
            rawWidthPx = entry.MinimumWidth;
            return true;
        }

        var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetDefaultGroup();
        var overlayMin = OverlaySlotWidthSettings.Get(group, entry.Title);
        if (overlayMin > 0)
        {
            rawWidthPx = overlayMin;
            return true;
        }

        return false;
    }

    public static DtrSlotWidthSource GetWidthSource(IReadOnlyDtrBarEntry entry)
    {
        if (entry.MinimumWidth > 0)
            return DtrSlotWidthSource.PluginMinimumWidth;

        var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetDefaultGroup();
        return OverlaySlotWidthSettings.Get(group, entry.Title) > 0
            ? DtrSlotWidthSource.OverlayMinWidth
            : DtrSlotWidthSource.None;
    }

    public static float GetScaledFixedWidth(IReadOnlyDtrBarEntry entry) =>
        TryGetFixedRawWidth(entry, out var raw) ? ScaleToOverlayPixels(raw) : 0f;

    public static float ScaleToOverlayPixels(float rawWidth) =>
        rawWidth * ScaleFactor;
}
