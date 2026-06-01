namespace DTROverlay.Services;

internal static class GroupStyleKeys
{
    private const string OverridePrefix = "@override:";
    private const string PluginPrefix = "@plugin:";

    public static string OverrideText(string groupId) => $"{OverridePrefix}{groupId}:text";

    public static string OverrideSeparator(string groupId) => $"{OverridePrefix}{groupId}:separator";

    public static string OverrideNativeText(string groupId) => $"{OverridePrefix}{groupId}:native-text";

    public static string OverrideNativeSeparator(string groupId) => $"{OverridePrefix}{groupId}:native-separator";

    public static string OverrideDivisionSeparator(string groupId) => $"{OverridePrefix}{groupId}:division-separator";

    public static string PluginEntry(string groupId, string pluginTitle) =>
        $"{PluginPrefix}{groupId}:{pluginTitle}";

    public static bool IsOverrideKey(string layoutKey) =>
        layoutKey.StartsWith(OverridePrefix, StringComparison.Ordinal);

    public static bool IsOverrideNativeTextKey(string layoutKey) =>
        layoutKey.EndsWith(":native-text", StringComparison.Ordinal);

    public static bool IsOverrideNativeSeparatorKey(string layoutKey) =>
        layoutKey.EndsWith(":native-separator", StringComparison.Ordinal);

    public static bool IsOverrideDivisionSeparatorKey(string layoutKey) =>
        layoutKey.EndsWith(":division-separator", StringComparison.Ordinal);

    public static bool IsOverridePluginTextKey(string layoutKey) =>
        IsOverrideKey(layoutKey)
        && layoutKey.EndsWith(":text", StringComparison.Ordinal)
        && !IsOverrideNativeTextKey(layoutKey);

    public static bool IsOverridePluginSeparatorKey(string layoutKey) =>
        IsOverrideKey(layoutKey)
        && layoutKey.EndsWith(":separator", StringComparison.Ordinal)
        && !IsOverrideNativeSeparatorKey(layoutKey)
        && !IsOverrideDivisionSeparatorKey(layoutKey);

    public static bool IsOverrideTextKey(string layoutKey) =>
        IsOverridePluginTextKey(layoutKey) || IsOverrideNativeTextKey(layoutKey);

    public static bool IsOverrideSeparatorKey(string layoutKey) =>
        IsOverridePluginSeparatorKey(layoutKey)
        || IsOverrideNativeSeparatorKey(layoutKey)
        || IsOverrideDivisionSeparatorKey(layoutKey);

    public static bool IsPluginEntryKey(string layoutKey) =>
        layoutKey.StartsWith(PluginPrefix, StringComparison.Ordinal);

    public static bool TryParsePluginEntryKey(string layoutKey, out string groupId, out string pluginTitle)
    {
        groupId = "";
        pluginTitle = "";

        if (!IsPluginEntryKey(layoutKey))
            return false;

        var body = layoutKey[PluginPrefix.Length..];
        var separator = body.IndexOf(':');
        if (separator <= 0 || separator >= body.Length - 1)
            return false;

        groupId = body[..separator];
        pluginTitle = body[(separator + 1)..];
        return !string.IsNullOrEmpty(groupId) && !string.IsNullOrEmpty(pluginTitle);
    }

    /// <summary>Resolves plugin row style key for the active or given group.</summary>
    public static string ResolvePluginEntryKey(string pluginTitle, DtrOverlayGroup group = null)
    {
        var resolvedGroup = group ?? OverlayStyleContext.Group ?? DtrOverlayGroups.GetDefaultGroup();
        return PluginEntry(resolvedGroup.Id, pluginTitle);
    }

    /// <summary>Plugin title for DTR lookup, or the key itself when not group-scoped.</summary>
    public static string GetPluginTitleFromLayoutKey(string layoutKey)
    {
        if (TryParsePluginEntryKey(layoutKey, out _, out var pluginTitle))
            return pluginTitle;

        return layoutKey;
    }
}
