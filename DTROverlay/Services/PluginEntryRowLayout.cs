using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;

namespace DTROverlay.Services;

/// <summary>
/// One DTR plugin row: optional prefix, content, optional suffix (fixed collect order).
/// </summary>
internal static class PluginEntryRowLayout
{
    public static IEnumerable<VisibleDtrEntry> CollectPluginEntry(DtrOverlayGroup group, string title)
    {
        var entry = Svc.DtrBar.Entries.FirstOrDefault(e => e.Title == title);
        if (entry == null || !DtrEntryVisibility.ShouldShowInOverlay(entry, group) || entry.Text == null)
            yield break;

        var styleKey = GroupStyleKeys.PluginEntry(group.Id, entry.Title);
        var affixes = PluginEntryAffixSettings.Get(group, entry.Title);
        var tooltipData = EncodeDtrTooltip(entry.Tooltip);
        var onClick = entry.OnClick;

        if (affixes.Prefix.Length != 0)
        {
            yield return VisibleDtrEntry.FromPluginAffix(
                affixes.Prefix,
                entry.Title,
                PluginAffixRole.Prefix,
                onClick,
                tooltipData,
                styleKey);
        }

        yield return VisibleDtrEntry.FromSeString(entry.Text, onClick, entry.Title, styleKey, entry.Tooltip);

        if (affixes.Suffix.Length != 0)
        {
            yield return VisibleDtrEntry.FromPluginAffix(
                affixes.Suffix,
                entry.Title,
                PluginAffixRole.Suffix,
                onClick,
                tooltipData,
                styleKey);
        }
    }

    public static bool TryGetPluginRowSpan(
        IReadOnlyList<VisibleDtrEntry> entries,
        int index,
        out int rowStart,
        out int rowEnd)
    {
        rowStart = -1;
        rowEnd = -1;

        if (index < 0 || index >= entries.Count)
            return false;

        if (!TryResolveContentIndex(entries, index, out var contentIndex, out var title))
            return false;

        rowStart = contentIndex;
        rowEnd = contentIndex;

        for (var i = contentIndex - 1; i >= 0; i--)
        {
            if (IsPluginPrefix(entries[i], title))
                rowStart = i;
            else
                break;
        }

        for (var i = contentIndex + 1; i < entries.Count; i++)
        {
            if (IsPluginSuffix(entries[i], title))
                rowEnd = i;
            else
                break;
        }

        return index >= rowStart && index <= rowEnd;
    }

    public static bool IsPluginContent(VisibleDtrEntry entry) =>
        entry.AffixRole == PluginAffixRole.None
        && entry.Kind == VisibleDtrEntryKind.SeString
        && !string.IsNullOrEmpty(entry.DtrEntryTitle);

    public static bool IsPluginOverlayEntry(VisibleDtrEntry entry) =>
        IsPluginContent(entry)
        || entry.AffixRole is PluginAffixRole.Prefix or PluginAffixRole.Suffix && entry.HasVisibleContent;

    private static bool TryResolveContentIndex(
        IReadOnlyList<VisibleDtrEntry> entries,
        int index,
        out int contentIndex,
        out string title)
    {
        contentIndex = -1;
        title = string.Empty;

        if (IsPluginContent(entries[index]))
        {
            contentIndex = index;
            title = entries[index].DtrEntryTitle;
            return true;
        }

        if (!TryGetAffixTitle(entries[index], out title))
            return false;

        for (var i = index - 1; i >= 0; i--)
        {
            if (IsPluginContent(entries[i]) && entries[i].DtrEntryTitle == title)
            {
                contentIndex = i;
                return true;
            }

            if (IsPluginAffix(entries[i], out var otherTitle) && otherTitle != title)
                break;
        }

        for (var i = index + 1; i < entries.Count; i++)
        {
            if (IsPluginContent(entries[i]) && entries[i].DtrEntryTitle == title)
            {
                contentIndex = i;
                return true;
            }

            if (IsPluginAffix(entries[i], out var otherTitle) && otherTitle != title)
                break;
        }

        return false;
    }

    private static bool IsPluginPrefix(VisibleDtrEntry entry, string title) =>
        entry.AffixRole == PluginAffixRole.Prefix
        && entry.HasVisibleContent
        && entry.DtrEntryTitle == title;

    private static bool IsPluginSuffix(VisibleDtrEntry entry, string title) =>
        entry.AffixRole == PluginAffixRole.Suffix
        && entry.HasVisibleContent
        && entry.DtrEntryTitle == title;

    private static bool IsPluginAffix(VisibleDtrEntry entry, out string title)
    {
        if (entry.AffixRole is PluginAffixRole.Prefix or PluginAffixRole.Suffix && entry.HasVisibleContent)
        {
            title = entry.DtrEntryTitle;
            return !string.IsNullOrEmpty(title);
        }

        title = string.Empty;
        return false;
    }

    private static bool TryGetAffixTitle(VisibleDtrEntry entry, out string title) =>
        IsPluginAffix(entry, out title);

    private static byte[] EncodeDtrTooltip(SeString tooltip) =>
        tooltip != null && !string.IsNullOrEmpty(tooltip.TextValue) ? tooltip.Encode() : null;
}
