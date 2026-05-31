namespace DTROverlay.Services;

internal static class PluginEntryAffixSettings
{
    public static PluginEntryAffixes Get(string entryTitle) =>
        C.PluginEntryAffixesByTitle.TryGetValue(entryTitle, out var affixes)
            ? affixes
            : Empty;

    public static PluginEntryAffixes GetOrCreate(string entryTitle)
    {
        if (!C.PluginEntryAffixesByTitle.TryGetValue(entryTitle, out var affixes))
        {
            affixes = new PluginEntryAffixes();
            C.PluginEntryAffixesByTitle[entryTitle] = affixes;
        }

        return affixes;
    }

    public static bool HasAny(string entryTitle)
    {
        var affixes = Get(entryTitle);
        return !string.IsNullOrEmpty(affixes.Prefix) || !string.IsNullOrEmpty(affixes.Suffix);
    }

    private static readonly PluginEntryAffixes Empty = new();
}
