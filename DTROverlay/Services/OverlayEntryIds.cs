using System.Linq;
using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class OverlayEntryIds
{
    public const string NativePrefix = "@native:";

    public const string ServerInfo = "@native:server-info";
    public const string ServerInfoTextGroup = "@native:server-info-text-group";

    public const string AppearanceText = "@appearance:text";

    public const string DefaultText = AppearanceText;

    public const string DefaultSeparator = "@style:default-separator";

    public const string PluginSeparatorColor = "@separator:plugin";

    public const string NativeSeparatorColor = "@separator:native";

    public const string DivisionSeparatorColor = "@separator:division";

    public static bool IsAppearanceText(string layoutKey) =>
        layoutKey == AppearanceText;

    public static bool IsDefaultText(string layoutKey) =>
        layoutKey == DefaultText;

    public static bool IsDefaultSeparator(string layoutKey) =>
        layoutKey == DefaultSeparator;

    public static bool IsNativeGroupColor(string layoutKey) =>
        layoutKey == ServerInfoTextGroup;

    public const string WorldIcon = "@native:world-icon";
    public const string WorldName = "@native:world-name";
    public const string WalkMode = "@native:walk-mode";
    public const string Network = "@native:network";
    public const string LocalClock = "@native:clock-lt";
    public const string EorzeaClock = "@native:clock-et";

    public static readonly string[] ServerInfoParts =
    [
        WorldIcon,
        WorldName,
        WalkMode,
        Network,
        LocalClock,
        EorzeaClock,
    ];

    private static readonly string[] LegacyNativeIds = ServerInfoParts;

    public static bool IsNative(string id) =>
        id.StartsWith(NativePrefix, StringComparison.Ordinal);

    public static bool IsServerInfoPart(string id) =>
        id == WorldIcon
            || id == WorldName
            || id == WalkMode
            || id == Network
            || id == LocalClock
            || id == EorzeaClock;

    public static bool IsLegacyNative(string id) =>
        IsServerInfoPart(id);

    public static bool IsServerInfoPartVisible(string partId) =>
        OverlayGroupSettings.IsServerInfoPartVisible(partId);

    public static void MigrateMiddleClickUi()
    {
        if (C.OpenPluginUiMiddleClickMigrated)
            return;

        C.OpenPluginUiOnMiddleClick = C.OpenPluginUiOnRightClick;
        C.OpenPluginUiMiddleClickMigrated = true;
    }

    public static void MigratePluginAffixes()
    {
        if (C.PluginAffixMigrated)
            return;

        foreach (var title in C.ShowPluginNameEntryTitles)
        {
            if (!C.PluginEntryAffixesByTitle.ContainsKey(title))
            {
                var affixes = new PluginEntryAffixes { Prefix = $"[{title}] " };
                affixes.Normalize();
                C.PluginEntryAffixesByTitle[title] = affixes;
            }
        }

        C.ShowPluginNameEntryTitles.Clear();
        C.PluginAffixMigrated = true;
    }

    public static void MigrateFontSizeScales()
    {
        if (C.FontSizeScaleSplitMigrated)
            return;

        if (C.FontScale > 0f)
            C.OverlayFontSizeScale = C.FontScale;

        C.FontSizeScaleSplitMigrated = true;
    }

    public static void MigrateServerInfoTableRow()
    {
        if (C.ServerInfoTableRowMigrated)
            return;

        if (C.HiddenEntryTitles.Contains(ServerInfo))
            C.ShowServerInfo = false;

        C.HiddenEntryTitles.Remove(ServerInfo);
        C.ServerInfoTableRowMigrated = true;
    }

    public static void MigrateLegacyNativeIds(
        IList<string> entryOrder,
        ISet<string> hiddenEntryTitles,
        ISet<string> hiddenServerInfoParts)
    {
        var hadLegacyInOrder = false;
        for (var i = entryOrder.Count - 1; i >= 0; i--)
        {
            if (!IsLegacyNative(entryOrder[i]))
                continue;

            hadLegacyInOrder = true;
            entryOrder.RemoveAt(i);
        }

        var migratedParts = false;
        foreach (var partId in LegacyNativeIds)
        {
            if (!hiddenEntryTitles.Contains(partId))
                continue;

            hiddenEntryTitles.Remove(partId);
            hiddenServerInfoParts.Add(partId);
            migratedParts = true;
        }

        _ = hadLegacyInOrder;
        _ = migratedParts;
    }

    public static void MigrateTextModeColorSettings()
    {
        var group = ServerInfoTextGroup;

        foreach (var partId in ServerInfoParts)
        {
            if (C.FixedWidthTextColors.Remove(partId, out var textColor))
                C.FixedWidthTextColors.TryAdd(group, textColor);

            if (C.FixedWidthOutlineColors.Remove(partId, out var outlineColor))
                C.FixedWidthOutlineColors.TryAdd(group, outlineColor);

            if (C.FixedWidthShadowColors.Remove(partId, out var shadowColor))
                C.FixedWidthShadowColors.TryAdd(group, shadowColor);

            if (C.FixedWidthEdgeStrengths.Remove(partId, out var edgeStrength))
                C.FixedWidthEdgeStrengths.TryAdd(group, edgeStrength);

            if (C.FixedWidthShadowThicknesses.Remove(partId, out var shadowThickness))
                C.FixedWidthShadowThicknesses.TryAdd(group, shadowThickness);
        }
    }

    public static void EnsureColorStyleCollections()
    {
        C.FixedTextColorEnabledIds ??= [];
        C.FixedEdgeStyleEnabledIds ??= [];
        C.FixedShadowStyleEnabledIds ??= [];
    }

    public static void MigrateTextColorEnabled()
    {
        if (C.TextColorEnabledMigrated)
            return;

        EnsureColorStyleCollections();

        foreach (var layoutKey in C.FixedColorEnabledIds)
            C.FixedTextColorEnabledIds.Add(layoutKey);

        foreach (var layoutKey in C.FixedWidthTextColors.Keys)
            C.FixedTextColorEnabledIds.Add(layoutKey);

        C.TextColorEnabledMigrated = true;
    }

    public static void MigrateEdgeShadowStyleEnabled()
    {
        if (C.EdgeShadowStyleEnabledMigrated)
            return;

        EnsureColorStyleCollections();

        foreach (var layoutKey in C.FixedWidthOutlineColors.Keys)
            C.FixedEdgeStyleEnabledIds.Add(layoutKey);

        foreach (var layoutKey in C.FixedWidthEdgeStrengths.Keys)
            C.FixedEdgeStyleEnabledIds.Add(layoutKey);

        foreach (var layoutKey in C.FixedWidthShadowColors.Keys)
            C.FixedShadowStyleEnabledIds.Add(layoutKey);

        foreach (var layoutKey in C.FixedWidthShadowThicknesses.Keys)
            C.FixedShadowStyleEnabledIds.Add(layoutKey);

        C.EdgeShadowStyleEnabledMigrated = true;
    }

    /// <summary>Splits legacy single-set custom flags into separate width and color sets.</summary>
    public static void MigrateAppearanceColorFlags()
    {
        if (C.AppearanceColorFlagsMigrated)
            return;

        if (C.TextColor != DtrStyle.DefaultTextColor)
            C.FixedTextColorEnabledIds.Add(AppearanceText);

        if (C.TextColor != DtrStyle.DefaultTextColor
            || C.OutlineColor != DtrStyle.DefaultOutlineColor
            || C.ShadowColor != DtrStyle.DefaultShadowColor
            || C.EdgeEnabled != DtrStyle.DefaultEdgeEnabled
            || C.ShadowEnabled != DtrStyle.DefaultShadowEnabled
            || C.EdgeStrength != DtrStyle.DefaultEdgeStrength
            || C.ShadowThickness != DtrStyle.DefaultShadowThickness)
            C.FixedColorEnabledIds.Add(AppearanceText);

        foreach (var layoutKey in new[]
                 {
                     PluginSeparatorColor,
                     NativeSeparatorColor,
                     DivisionSeparatorColor,
                     ServerInfoTextGroup,
                 })
        {
            if (C.FixedWidthTextColors.ContainsKey(layoutKey))
                C.FixedTextColorEnabledIds.Add(layoutKey);

            if (C.FixedWidthTextColors.ContainsKey(layoutKey)
                || C.FixedWidthOutlineColors.ContainsKey(layoutKey)
                || C.FixedWidthShadowColors.ContainsKey(layoutKey)
                || C.FixedWidthEdgeEnabled.ContainsKey(layoutKey)
                || C.FixedWidthShadowEnabled.ContainsKey(layoutKey)
                || C.FixedWidthEdgeStrengths.ContainsKey(layoutKey)
                || C.FixedWidthShadowThicknesses.ContainsKey(layoutKey))
                C.FixedColorEnabledIds.Add(layoutKey);
        }

        C.AppearanceColorFlagsMigrated = true;
    }

    public static void MigrateWidthColorSettings()
    {
        if (C.FixedColorEnabledIds.Count > 0)
            return;

        foreach (var layoutKey in C.FixedWidthEnabledIds.ToList())
        {
            var hasCustomColors = C.FixedWidthTextColors.ContainsKey(layoutKey)
                || C.FixedWidthOutlineColors.ContainsKey(layoutKey)
                || C.FixedWidthShadowColors.ContainsKey(layoutKey)
                || C.FixedWidthEdgeEnabled.ContainsKey(layoutKey)
                || C.FixedWidthShadowEnabled.ContainsKey(layoutKey)
                || C.FixedWidthEdgeStrengths.ContainsKey(layoutKey)
                || C.FixedWidthShadowThicknesses.ContainsKey(layoutKey);
            var hasCustomWidth = C.FixedWidthPixels.ContainsKey(layoutKey);

            if (hasCustomColors)
                C.FixedColorEnabledIds.Add(layoutKey);

            if (!hasCustomWidth && hasCustomColors)
                C.FixedWidthEnabledIds.Remove(layoutKey);
        }
    }

    public static string GetDisplayName(string id) => id switch
    {
        ServerInfo => "Server Info",
        _ => id,
    };

    public static void MigrateStyleHierarchy()
    {
        if (C.StyleHierarchyMigrated)
            return;

        EnsureColorStyleCollections();

        MigrateSeparatorStored(OverlayEntryIds.PluginSeparatorColor, OverlayEntryIds.DefaultSeparator);
        MigrateSeparatorStored(OverlayEntryIds.NativeSeparatorColor, OverlayEntryIds.DefaultSeparator);

        if (C.FixedTextColorEnabledIds.Remove(OverlayEntryIds.ServerInfoTextGroup))
            C.FixedTextColorEnabledIds.Add(DefaultText);

        CopyStyleStored(ServerInfoTextGroup, DefaultText);

        C.StyleHierarchyMigrated = true;
        EzConfig.Save();
    }

    /// <summary>Copies legacy Native-group override colors into Default+Native merged override keys.</summary>
    public static void MigrateMergedDefaultOverrideStyles()
    {
        if (C.MergedDefaultOverrideStylesMigrated)
            return;

        var def = DtrOverlayGroups.GetDefaultGroup();
        var native = DtrOverlayGroups.GetNativeGroup();

        CopyOverrideStyleIfMissing(GroupStyleKeys.OverrideNativeText(def.Id), GroupStyleKeys.OverrideText(native.Id));
        CopyOverrideStyleIfMissing(
            GroupStyleKeys.OverrideNativeSeparator(def.Id),
            GroupStyleKeys.OverrideSeparator(native.Id));

        C.MergedDefaultOverrideStylesMigrated = true;
        EzConfig.Save();
    }

    private static void CopyOverrideStyleIfMissing(string toKey, string fromKey)
    {
        if (toKey == fromKey || HasStoredStyle(toKey))
            return;

        if (!HasStoredStyle(fromKey))
            return;

        CopyStyleStored(fromKey, toKey);

        if (C.FixedTextColorEnabledIds.Remove(fromKey))
            C.FixedTextColorEnabledIds.Add(toKey);

        if (C.FixedColorEnabledIds.Remove(fromKey))
            C.FixedColorEnabledIds.Add(toKey);

        if (C.FixedEdgeStyleEnabledIds.Remove(fromKey))
            C.FixedEdgeStyleEnabledIds.Add(toKey);

        if (C.FixedShadowStyleEnabledIds.Remove(fromKey))
            C.FixedShadowStyleEnabledIds.Add(toKey);
    }

    private static bool HasStoredStyle(string layoutKey) =>
        C.FixedTextColorEnabledIds.Contains(layoutKey)
        || C.FixedColorEnabledIds.Contains(layoutKey)
        || C.FixedWidthTextColors.ContainsKey(layoutKey)
        || C.FixedWidthOutlineColors.ContainsKey(layoutKey)
        || C.FixedWidthShadowColors.ContainsKey(layoutKey)
        || C.FixedWidthEdgeStrengths.ContainsKey(layoutKey)
        || C.FixedWidthShadowThicknesses.ContainsKey(layoutKey)
        || C.FixedWidthEdgeEnabled.ContainsKey(layoutKey)
        || C.FixedWidthShadowEnabled.ContainsKey(layoutKey);

    private static void MigrateSeparatorStored(string fromKey, string toKey)
    {
        if (C.FixedTextColorEnabledIds.Remove(fromKey))
            C.FixedTextColorEnabledIds.Add(toKey);

        CopyStyleStored(fromKey, toKey);
    }

    private static void CopyStyleStored(string fromKey, string toKey)
    {
        if (fromKey == DefaultText || toKey == DefaultText)
            return;

        if (C.FixedWidthTextColors.TryGetValue(fromKey, out var text))
            C.FixedWidthTextColors[toKey] = text;

        if (C.FixedWidthOutlineColors.TryGetValue(fromKey, out var outline))
            C.FixedWidthOutlineColors[toKey] = outline;

        if (C.FixedWidthShadowColors.TryGetValue(fromKey, out var shadow))
            C.FixedWidthShadowColors[toKey] = shadow;

        if (C.FixedWidthEdgeStrengths.TryGetValue(fromKey, out var edgeStrength))
            C.FixedWidthEdgeStrengths[toKey] = edgeStrength;

        if (C.FixedWidthShadowThicknesses.TryGetValue(fromKey, out var shadowThickness))
            C.FixedWidthShadowThicknesses[toKey] = shadowThickness;

        if (C.FixedWidthEdgeEnabled.TryGetValue(fromKey, out var edgeEnabled))
            C.FixedWidthEdgeEnabled[toKey] = edgeEnabled;

        if (C.FixedWidthShadowEnabled.TryGetValue(fromKey, out var shadowEnabled))
            C.FixedWidthShadowEnabled[toKey] = shadowEnabled;
    }

    public static string GetPartDisplayName(string partId) => partId switch
    {
        WorldIcon => "Travel status",
        WorldName => "World name",
        WalkMode => "Walk mode",
        Network => "Network strength",
        LocalClock => "Local time (LT)",
        EorzeaClock => "Eorzea time (ET)",
        _ => partId,
    };
}
