namespace DTROverlay.Services;

internal static class OverlayPluginFlow
{
    public static bool UseHorizontalLeftToRight(DtrOverlayGroup group = null) =>
        OverlayGroupLayout.GetLayoutMode(group) == OverlayLayoutMode.Horizontal
        && OverlayGroupLayout.GetHorizontalPluginFlow(group) == OverlayHorizontalFlow.LeftToRight;

    public static bool UseVerticalBottomToTop(DtrOverlayGroup group = null) =>
        OverlayGroupLayout.GetLayoutMode(group) == OverlayLayoutMode.Vertical
        && OverlayGroupLayout.GetVerticalPluginFlow(group) == OverlayVerticalFlow.BottomToTop;

    public static IReadOnlyList<VisibleDtrEntry> OrderForVertical(IReadOnlyList<VisibleDtrEntry> entries) =>
        OrderForVertical(entries, null);

    public static IReadOnlyList<VisibleDtrEntry> OrderForVertical(
        IReadOnlyList<VisibleDtrEntry> entries,
        DtrOverlayGroup group)
    {
        if (!UseVerticalBottomToTop(group) || entries.Count <= 1)
            return entries;

        var groups = SplitPluginEntryGroups(entries);
        groups.Reverse();

        var ordered = new List<VisibleDtrEntry>(entries.Count);
        foreach (var pluginGroup in groups)
            ordered.AddRange(pluginGroup);

        return ordered;
    }

    /// <summary>
    /// One plugin row (prefix + content + suffix) per group. Used so bottom-to-top reverses rows, not affixes.
    /// </summary>
    private static List<List<VisibleDtrEntry>> SplitPluginEntryGroups(IReadOnlyList<VisibleDtrEntry> entries)
    {
        var groups = new List<List<VisibleDtrEntry>>();

        foreach (var entry in entries)
        {
            var title = entry.DtrEntryTitle ?? string.Empty;
            if (groups.Count == 0
                || string.IsNullOrEmpty(title)
                || groups[^1][0].DtrEntryTitle != title)
                groups.Add([entry]);
            else
                groups[^1].Add(entry);
        }

        return groups;
    }
}
