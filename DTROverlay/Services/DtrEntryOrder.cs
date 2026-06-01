using System.Linq;
using Dalamud.Game.Gui.Dtr;

namespace DTROverlay.Services;

internal static class DtrEntryOrder
{
    public static void SyncOrder() => DtrOverlayGroups.EnsureInitialized();

    public static void ResetToNativeOrder(DtrOverlayGroup group) =>
        DtrOverlayGroups.ResetGroupToNativeOrder(group);

    public static void MoveUp(IList<string> order, int index)
    {
        if (index <= 0 || index >= order.Count)
            return;

        (order[index - 1], order[index]) = (order[index], order[index - 1]);
        EzConfig.Save();
    }

    public static void MoveDown(IList<string> order, int index)
    {
        if (index < 0 || index >= order.Count - 1)
            return;

        (order[index + 1], order[index]) = (order[index], order[index + 1]);
        EzConfig.Save();
    }

    public static IReadOnlyList<string> GetOrderedPluginIdsForDisplay(DtrOverlayGroup group)
    {
        DtrOverlayGroups.SyncGroupOrder(group);
        return group.EntryOrder;
    }

    public static IReadOnlyList<IReadOnlyDtrBarEntry> GetOrderedPluginEntries(DtrOverlayGroup group)
    {
        var entriesByTitle = Svc.DtrBar.Entries.ToDictionary(entry => entry.Title);
        var orderedEntries = new List<IReadOnlyDtrBarEntry>();

        foreach (var id in GetOrderedPluginIdsForDisplay(group))
        {
            if (entriesByTitle.TryGetValue(id, out var entry))
                orderedEntries.Add(entry);
        }

        return orderedEntries;
    }
}
