using System.Linq;
using Dalamud.Game.Gui.Dtr;

namespace DTROverlay.Services;

internal static class DtrEntryOrder
{
    public static void SyncOrder()
    {
        C.EntryOrder ??= [];

        OverlayEntryIds.MigratePluginAffixes();
        OverlayEntryIds.MigrateMiddleClickUi();
        OverlayEntryIds.MigrateFontSizeScales();
        OverlayEntryIds.MigrateServerInfoTableRow();
        OverlayEntryIds.MigrateLegacyNativeIds(C.EntryOrder, C.HiddenEntryTitles, C.HiddenServerInfoParts);
        OverlayEntryIds.MigrateTextModeColorSettings();
        OverlayEntryIds.MigrateWidthColorSettings();
        OverlayEntryIds.MigrateAppearanceColorFlags();
        DtrSeparatorStyle.MigrateVisibilitySplit();

        C.EntryOrder.RemoveAll(OverlayEntryIds.IsNative);

        var validPlugins = Svc.DtrBar.Entries.Select(entry => entry.Title).ToHashSet();
        C.EntryOrder.RemoveAll(id => !validPlugins.Contains(id));

        foreach (var title in Svc.DtrBar.Entries.Select(entry => entry.Title))
        {
            if (!C.EntryOrder.Contains(title))
                C.EntryOrder.Add(title);
        }
    }

    public static void ResetToNativeOrder()
    {
        C.EntryOrder.Clear();
        C.EntryOrder.AddRange(Svc.DtrBar.Entries.Select(entry => entry.Title));
    }

    public static void MoveUp(IList<string> order, int index)
    {
        if (index <= 0 || index >= order.Count)
            return;

        (order[index - 1], order[index]) = (order[index], order[index - 1]);
    }

    public static void MoveDown(IList<string> order, int index)
    {
        if (index < 0 || index >= order.Count - 1)
            return;

        (order[index + 1], order[index]) = (order[index], order[index + 1]);
    }

    public static IReadOnlyList<string> GetOrderedIdsForDisplay()
    {
        SyncOrder();
        return C.EntryOrder;
    }

    public static IReadOnlyList<string> GetOrderedPluginIdsForDisplay()
    {
        SyncOrder();
        return C.EntryOrder.Where(id => !OverlayEntryIds.IsNative(id)).ToList();
    }

    public static IReadOnlyList<IReadOnlyDtrBarEntry> GetOrderedPluginEntries()
    {
        var entriesByTitle = Svc.DtrBar.Entries.ToDictionary(entry => entry.Title);
        var orderedEntries = new List<IReadOnlyDtrBarEntry>();

        foreach (var id in GetOrderedPluginIdsForDisplay())
        {
            if (entriesByTitle.TryGetValue(id, out var entry))
                orderedEntries.Add(entry);
        }

        return orderedEntries;
    }
}
