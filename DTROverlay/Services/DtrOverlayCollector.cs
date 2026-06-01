using System.Linq;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;

namespace DTROverlay.Services;

public static class DtrOverlayCollector
{
    public static DtrOverlayContent Collect(DtrOverlayGroup group)
    {
        if (!Svc.ClientState.IsLoggedIn)
            return new([], [], []);

        if (DtrOverlayGroups.IsNativeGroup(group))
        {
            if (!DtrOverlayGroups.IsSplitNativeMode())
                return new([], [], []);

            return CollectNativeGroup();
        }

        DtrOverlayGroups.SyncGroupOrder(group);

        if (FollowVanillaDtrMode.IsActive)
            return CollectFollowVanilla(group);

        if (DtrOverlayGroups.IsDefaultGroup(group) && !DtrOverlayGroups.IsSplitNativeMode())
            return CollectMergedDefault(group);

        return CollectPluginsOnly(group);
    }

    private static DtrOverlayContent CollectNativeGroup()
    {
        var nativeSegments = NativeDtrReader.BuildServerInfoSegments();
        if (nativeSegments.Count == 0)
            return new([], [], []);

        return new(nativeSegments, nativeSegments, []);
    }

    private static DtrOverlayContent CollectMergedDefault(DtrOverlayGroup group)
    {
        var orderedFlat = new List<VisibleDtrEntry>();
        var nativeSplit = new List<VisibleDtrEntry>();
        var pluginSplit = new List<VisibleDtrEntry>();

        var nativeSegments = NativeDtrReader.BuildServerInfoSegments();
        if (nativeSegments.Count > 0)
        {
            orderedFlat.AddRange(nativeSegments);
            nativeSplit.AddRange(nativeSegments);
        }

        foreach (var id in DtrEntryOrder.GetOrderedPluginIdsForDisplay(group))
        {
            foreach (var entry in CollectPluginEntry(group, id))
            {
                orderedFlat.Add(entry);
                pluginSplit.Add(entry);
            }
        }

        return new(orderedFlat, nativeSplit, pluginSplit);
    }

    private static DtrOverlayContent CollectPluginsOnly(DtrOverlayGroup group)
    {
        var orderedFlat = new List<VisibleDtrEntry>();
        var pluginSplit = new List<VisibleDtrEntry>();

        foreach (var id in DtrEntryOrder.GetOrderedPluginIdsForDisplay(group))
        {
            foreach (var entry in CollectPluginEntry(group, id))
            {
                orderedFlat.Add(entry);
                pluginSplit.Add(entry);
            }
        }

        return new(orderedFlat, [], pluginSplit);
    }

    private static DtrOverlayContent CollectFollowVanilla(DtrOverlayGroup group)
    {
        var pluginEntries = new List<VisibleDtrEntry>();

        foreach (var id in DtrEntryOrder.GetOrderedPluginIdsForDisplay(group))
        {
            foreach (var entry in CollectPluginEntry(group, id))
                pluginEntries.Add(entry);
        }

        return new(pluginEntries, [], pluginEntries);
    }

    private static IEnumerable<VisibleDtrEntry> CollectPluginEntry(DtrOverlayGroup group, string title)
    {
        var entry = Svc.DtrBar.Entries.FirstOrDefault(e => e.Title == title);
        if (entry == null || !DtrEntryVisibility.ShouldShowInOverlay(entry, group) || entry.Text == null)
            yield break;

        var styleKey = GroupStyleKeys.PluginEntry(group.Id, entry.Title);
        var affixes = PluginEntryAffixSettings.Get(group, entry.Title);
        var tooltipData = EncodeDtrTooltip(entry.Tooltip);
        var onClick = entry.OnClick;

        if (!string.IsNullOrEmpty(affixes.Prefix))
            yield return VisibleDtrEntry.FromPluginAffix(affixes.Prefix, entry.Title, PluginAffixRole.Prefix, onClick, tooltipData, styleKey);

        yield return VisibleDtrEntry.FromSeString(entry.Text, onClick, entry.Title, styleKey, entry.Tooltip);

        if (!string.IsNullOrEmpty(affixes.Suffix))
            yield return VisibleDtrEntry.FromPluginAffix(affixes.Suffix, entry.Title, PluginAffixRole.Suffix, onClick, tooltipData, styleKey);
    }

    private static byte[] EncodeDtrTooltip(SeString tooltip) =>
        tooltip != null && !string.IsNullOrEmpty(tooltip.TextValue) ? tooltip.Encode() : null;
}
