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

        for (var i = groupStart; i <= groupEnd; i++)
        {
            if (needsSpacing)
                width += entries[i].SameLineSpacingBefore ?? DtrStyle.EntrySpacing;

            width += MeasureEntry(entries[i]).X;
            needsSpacing = true;
        }

        return width;
    }

    private static bool TryGetPluginEntryGroupForward(
        IReadOnlyList<VisibleDtrEntry> entries,
        int index,
        out int groupStart,
        out int contentIndex,
        out int groupEnd)
    {
        groupStart = index;
        contentIndex = -1;
        groupEnd = index;

        if (index >= entries.Count)
            return false;

        if (entries[index].AffixRole == PluginAffixRole.Prefix)
        {
            if (index + 1 >= entries.Count || !IsPluginContent(entries[index + 1]))
                return false;

            contentIndex = index + 1;
            groupEnd = contentIndex;
            if (contentIndex + 1 < entries.Count
                && entries[contentIndex + 1].AffixRole == PluginAffixRole.Suffix
                && SharesPluginEntryTitle(entries[contentIndex], entries[contentIndex + 1]))
                groupEnd = contentIndex + 1;

            return true;
        }

        if (!IsPluginContent(entries[index]))
            return false;

        contentIndex = index;
        groupEnd = index;
        if (index + 1 < entries.Count
            && entries[index + 1].AffixRole == PluginAffixRole.Suffix
            && SharesPluginEntryTitle(entries[index], entries[index + 1]))
            groupEnd = index + 1;

        return true;
    }

    private static bool TryGetPluginEntryGroupAt(
        IReadOnlyList<VisibleDtrEntry> entries,
        int index,
        out int groupStart,
        out int contentIndex,
        out int groupEnd)
    {
        if (TryGetPluginEntryGroupForward(entries, index, out groupStart, out contentIndex, out groupEnd))
            return true;

        if (IsPluginContent(entries[index]))
        {
            contentIndex = index;
            groupEnd = index;
            groupStart = index;
            if (index + 1 < entries.Count
                && entries[index + 1].AffixRole == PluginAffixRole.Suffix
                && SharesPluginEntryTitle(entries[index], entries[index + 1]))
                groupEnd = index + 1;

            if (index > 0
                && entries[index - 1].AffixRole == PluginAffixRole.Prefix
                && SharesPluginEntryTitle(entries[index - 1], entries[index]))
                groupStart = index - 1;

            return true;
        }

        if (entries[index].AffixRole == PluginAffixRole.Suffix
            && index > 0
            && IsPluginContent(entries[index - 1]))
        {
            contentIndex = index - 1;
            groupEnd = index;
            groupStart = contentIndex;
            if (contentIndex > 0
                && entries[contentIndex - 1].AffixRole == PluginAffixRole.Prefix
                && SharesPluginEntryTitle(entries[contentIndex - 1], entries[contentIndex]))
                groupStart = contentIndex - 1;

            return true;
        }

        groupStart = -1;
        contentIndex = -1;
        groupEnd = -1;
        return false;
    }

    private static bool SharesPluginEntryTitle(VisibleDtrEntry left, VisibleDtrEntry right) =>
        !string.IsNullOrEmpty(left.DtrEntryTitle)
        && left.DtrEntryTitle == right.DtrEntryTitle;

    private static List<VisibleDtrEntry> InsertPluginSeparators(IReadOnlyList<VisibleDtrEntry> entries)
    {
        if (!DtrSeparatorStyle.IsPluginVisible)
            return entries is List<VisibleDtrEntry> list ? list : [.. entries];

        var result = new List<VisibleDtrEntry>(entries.Count);
        VisibleDtrEntry? previousEntry = null;

        foreach (var entry in entries)
        {
            if (ShouldInsertSeparatorBefore(entry, previousEntry))
                result.Add(CreatePluginSeparator());

            result.Add(entry);
            previousEntry = entry;
        }

        return result;
    }

    private static bool ShouldInsertSeparatorBefore(
        VisibleDtrEntry entry,
        VisibleDtrEntry? previousEntry)
    {
        if (!IsPluginOverlayEntry(entry))
            return false;

        if (previousEntry == null)
            return false;

        if (AreGroupedPluginEntries(previousEntry.Value, entry))
            return false;

        if (!IsPluginContent(previousEntry.Value))
            return false;

        return true;
    }

    private static bool AreGroupedPluginEntries(VisibleDtrEntry previous, VisibleDtrEntry next) =>
        previous.AffixRole == PluginAffixRole.Prefix && IsPluginContent(next)
            && SharesPluginEntryTitle(previous, next)
        || IsPluginContent(previous) && next.AffixRole == PluginAffixRole.Suffix
            && SharesPluginEntryTitle(previous, next);

    private static bool IsPluginOverlayEntry(VisibleDtrEntry entry) =>
        entry.AffixRole != PluginAffixRole.None || IsPluginContent(entry);

    private static bool IsPluginContent(VisibleDtrEntry entry) =>
        entry.Kind == VisibleDtrEntryKind.SeString && !string.IsNullOrEmpty(entry.DtrEntryTitle);

    private static bool IsHorizontalDivisionLayout =>
        !FollowVanillaDtrMode.IsActive && C.OverlayLayoutMode == OverlayLayoutMode.Horizontal;

    private static bool UsesDivisionSeparator =>
        IsHorizontalDivisionLayout && C.NativePluginDivision == NativePluginDivisionMode.Separator;

    private static bool UsesNewLineDivision =>
        IsHorizontalDivisionLayout && C.NativePluginDivision == NativePluginDivisionMode.NewLine;

    private static bool ShouldShowDivisionSeparator(bool hasNativeEntries, bool hasPluginEntries) =>
        UsesDivisionSeparator && hasNativeEntries && hasPluginEntries;

    private static VisibleDtrEntry CreatePluginSeparator() =>
        VisibleDtrEntry.FromText("|", colorLayoutKey: OverlayEntryIds.PluginSeparatorColor);

    private static VisibleDtrEntry CreateDivisionSeparator() =>
        VisibleDtrEntry.FromText("|", colorLayoutKey: OverlayEntryIds.DivisionSeparatorColor);
}
