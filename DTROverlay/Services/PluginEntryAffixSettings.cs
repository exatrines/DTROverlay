namespace DTROverlay.Services;

internal static class PluginEntryAffixSettings
{
    public static PluginEntryAffixes Get(DtrOverlayGroup group, string entryTitle)
    {
        if (!group.PluginEntryAffixesByTitle.TryGetValue(entryTitle, out var affixes))
            return Empty;

        affixes.Normalize();
        return affixes;
    }

    public static PluginEntryAffixes GetOrCreate(DtrOverlayGroup group, string entryTitle)
    {
        if (!group.PluginEntryAffixesByTitle.TryGetValue(entryTitle, out var affixes))
        {
            affixes = new PluginEntryAffixes();
            group.PluginEntryAffixesByTitle[entryTitle] = affixes;
        }

        affixes.Normalize();
        return affixes;
    }

    public static void NormalizeAllGroups()
    {
        if (C.OverlayGroups != null)
        {
            foreach (var group in C.OverlayGroups)
            {
                if (group.PluginEntryAffixesByTitle == null)
                    continue;

                foreach (var affixes in group.PluginEntryAffixesByTitle.Values)
                    affixes.Normalize();
            }
        }

        if (C.PluginEntryAffixesByTitle == null)
            return;

        foreach (var affixes in C.PluginEntryAffixesByTitle.Values)
            affixes.Normalize();
    }

    private static readonly PluginEntryAffixes Empty = new();
}
