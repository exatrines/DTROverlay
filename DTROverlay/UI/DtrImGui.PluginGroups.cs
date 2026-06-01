using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class DtrImGui
{
    private static float MeasurePluginEntryGroupWidth(
        IReadOnlyList<VisibleDtrEntry> entries,
        int groupStart,
        int groupEnd)
    {
        var width = 0f;
        var needsSpacing = false;

        foreach (var index in EnumeratePluginRowDrawOrder(entries, groupStart, groupEnd))
        {
            if (needsSpacing)
                width += entries[index].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

            width += MeasureEntry(entries[index]).X;
            needsSpacing = true;
        }

        return width;
    }

    private static void DrawPluginEntryRow(
        IReadOnlyList<VisibleDtrEntry> entries,
        int rowStart,
        int rowEnd,
        bool sameLineBefore)
    {
        var needsSpacing = sameLineBefore;

        foreach (var index in EnumeratePluginRowDrawOrder(entries, rowStart, rowEnd))
        {
            if (needsSpacing)
                ImGui.SameLine(0f, entries[index].SameLineSpacingBefore ?? DtrStyle.EntrySpacing);

            DrawEntry(entries[index]);
            needsSpacing = true;
        }
    }

    /// <summary>Always prefix → content → suffix, regardless of list index order.</summary>
    private static IEnumerable<int> EnumeratePluginRowDrawOrder(
        IReadOnlyList<VisibleDtrEntry> entries,
        int rowStart,
        int rowEnd)
    {
        int? prefix = null;
        int? content = null;
        int? suffix = null;

        for (var i = rowStart; i <= rowEnd; i++)
        {
            if (!entries[i].HasVisibleContent)
                continue;

            switch (entries[i].AffixRole)
            {
                case PluginAffixRole.Prefix:
                    prefix = i;
                    break;
                case PluginAffixRole.Suffix:
                    suffix = i;
                    break;
                default:
                    if (PluginEntryRowLayout.IsPluginContent(entries[i]))
                        content = i;
                    break;
            }
        }

        if (prefix is { } prefixIndex)
            yield return prefixIndex;

        if (content is { } contentIndex)
            yield return contentIndex;

        if (suffix is { } suffixIndex)
            yield return suffixIndex;
    }

    private static List<VisibleDtrEntry> InsertPluginSeparators(IReadOnlyList<VisibleDtrEntry> entries)
    {
        var result = new List<VisibleDtrEntry>(entries.Count);
        VisibleDtrEntry? previousEntry = null;

        foreach (var entry in entries)
        {
            if (ShouldInsertSeparatorBefore(entry, previousEntry))
                result.Add(DtrSeparators.CreatePlugin());

            result.Add(entry);
            previousEntry = entry;
        }

        return result;
    }

    private static IReadOnlyList<VisibleDtrEntry> InsertDivisionSeparatorIfNeeded(
        IReadOnlyList<VisibleDtrEntry> pluginEntries)
    {
        if (!DtrSeparators.ShouldInsertDivision(hasNativeOverlayEntries: false, pluginEntries.Count > 0))
            return pluginEntries;

        var division = DtrSeparators.CreateDivision();

        if (FollowVanillaDtrMode.IsActive)
        {
            if (FollowVanillaDtrMode.ShouldPrependDivisionSeparatorToPluginList())
            {
                var prepended = new List<VisibleDtrEntry>(pluginEntries.Count + 1) { division };
                prepended.AddRange(pluginEntries);
                return prepended;
            }

            var appended = new List<VisibleDtrEntry>(pluginEntries.Count + 1);
            appended.AddRange(pluginEntries);
            appended.Add(division);
            return appended;
        }

        var result = new List<VisibleDtrEntry>(pluginEntries.Count + 1) { division };
        result.AddRange(pluginEntries);
        return result;
    }

    private static bool ShouldInsertSeparatorBefore(
        VisibleDtrEntry entry,
        VisibleDtrEntry? previousEntry)
    {
        if (!PluginEntryRowLayout.IsPluginOverlayEntry(entry))
            return false;

        if (previousEntry == null)
            return false;

        if (AreGroupedPluginEntries(previousEntry.Value, entry))
            return false;

        if (!PluginEntryRowLayout.IsPluginOverlayEntry(previousEntry.Value))
            return false;

        return entry.AffixRole == PluginAffixRole.Prefix && entry.HasVisibleContent
            || PluginEntryRowLayout.IsPluginContent(entry);
    }

    private static bool AreGroupedPluginEntries(VisibleDtrEntry previous, VisibleDtrEntry next) =>
        !string.IsNullOrEmpty(previous.DtrEntryTitle)
        && previous.DtrEntryTitle == next.DtrEntryTitle
        && (
            previous.AffixRole == PluginAffixRole.Prefix
                && previous.HasVisibleContent
                && PluginEntryRowLayout.IsPluginContent(next)
            || PluginEntryRowLayout.IsPluginContent(previous)
                && next.AffixRole == PluginAffixRole.Suffix
                && next.HasVisibleContent);

    private static bool IsPluginOverlayEntry(VisibleDtrEntry entry) =>
        PluginEntryRowLayout.IsPluginOverlayEntry(entry);
}
