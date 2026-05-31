using System.Linq;
using Dalamud.Game.Gui.Dtr;

namespace DTROverlay.Services;

public static class DtrOverlayCollector
{
    public static DtrOverlayContent Collect()
    {
        if (!Svc.ClientState.IsLoggedIn)
            return new([], [], []);

        DtrEntryOrder.SyncOrder();

        if (FollowVanillaDtrMode.IsActive)
            return CollectFollowVanilla();

        var nativeSegments = NativeDtrReader.BuildServerInfoSegments();
        var orderedFlat = new List<VisibleDtrEntry>();
        var nativeSplit = new List<VisibleDtrEntry>();
        var pluginSplit = new List<VisibleDtrEntry>();

        if (OverlayEntryIds.IsServerInfoShownInOverlay() && nativeSegments.Count > 0)
        {
            orderedFlat.AddRange(nativeSegments);
            nativeSplit.AddRange(nativeSegments);
        }

        foreach (var id in DtrEntryOrder.GetOrderedPluginIdsForDisplay())
        {
            foreach (var entry in CollectPluginEntry(id))
            {
                orderedFlat.Add(entry);
                pluginSplit.Add(entry);
            }
        }

        return new(orderedFlat, nativeSplit, pluginSplit);
    }

    private static DtrOverlayContent CollectFollowVanilla()
    {
        var pluginEntries = new List<VisibleDtrEntry>();

        foreach (var id in DtrEntryOrder.GetOrderedPluginIdsForDisplay())
        {
            foreach (var entry in CollectPluginEntry(id))
                pluginEntries.Add(entry);
        }

        return new(pluginEntries, [], pluginEntries);
    }

    private static IEnumerable<VisibleDtrEntry> CollectPluginEntry(string title)
    {
        var entry = Svc.DtrBar.Entries.FirstOrDefault(e => e.Title == title);
        if (entry == null || !DtrEntryVisibility.ShouldShowInOverlay(entry) || entry.Text == null)
            yield break;

        var affixes = PluginEntryAffixSettings.Get(entry.Title);
        if (!string.IsNullOrEmpty(affixes.Prefix))
            yield return VisibleDtrEntry.FromPluginAffix(affixes.Prefix, entry.Title, PluginAffixRole.Prefix);

        yield return VisibleDtrEntry.FromSeString(entry.Text, entry.OnClick, entry.Title, entry.Title);

        if (!string.IsNullOrEmpty(affixes.Suffix))
            yield return VisibleDtrEntry.FromPluginAffix(affixes.Suffix, entry.Title, PluginAffixRole.Suffix);
    }
}
