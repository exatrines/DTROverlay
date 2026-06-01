namespace DTROverlay.Services;

internal static class PluginEntryAffixSettings
{
    public static PluginEntryAffixes Get(DtrOverlayGroup group, string entryTitle) =>
        group.PluginEntryAffixesByTitle.TryGetValue(entryTitle, out var affixes)
            ? affixes
            : Empty;

    public static PluginEntryAffixes GetOrCreate(DtrOverlayGroup group, string entryTitle)
    {
        if (!group.PluginEntryAffixesByTitle.TryGetValue(entryTitle, out var affixes))
        {
            affixes = new PluginEntryAffixes();
            group.PluginEntryAffixesByTitle[entryTitle] = affixes;
        }

        return affixes;
    }

    public static bool HasAny(DtrOverlayGroup group, string entryTitle)
    {
        var affixes = Get(group, entryTitle);
        return !string.IsNullOrEmpty(affixes.Prefix) || !string.IsNullOrEmpty(affixes.Suffix);
    }

    private static readonly PluginEntryAffixes Empty = new();
}
