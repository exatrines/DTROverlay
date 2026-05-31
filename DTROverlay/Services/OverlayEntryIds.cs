using System.Linq;
using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class OverlayEntryIds
{
    public const string NativePrefix = "@native:";

    public const string ServerInfo = "@native:server-info";
    public const string ServerInfoTextGroup = "@native:server-info-text-group";

    public const string AppearanceText = "@appearance:text";

    public const string PluginSeparatorColor = "@separator:plugin";

    public const string NativeSeparatorColor = "@separator:native";

    public const string DivisionSeparatorColor = "@separator:division";

    public static bool IsAppearanceText(string layoutKey) =>
        layoutKey == AppearanceText;

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
        !C.HiddenServerInfoParts.Contains(partId);

    public static bool IsServerInfoShownInOverlay() =>
        C.ShowServerInfo;

    public static void MigratePluginAffixes()
    {
        if (C.PluginAffixMigrated)
            return;

        foreach (var title in C.ShowPluginNameEntryTitles)
        {
            if (!C.PluginEntryAffixesByTitle.ContainsKey(title))
                C.PluginEntryAffixesByTitle[title] = new PluginEntryAffixes { Prefix = $"[{title}] " };
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
        }
    }

    /// <summary>Splits legacy single-set custom flags into separate width and color sets.</summary>
    public static void MigrateAppearanceColorFlags()
    {
        if (C.AppearanceColorFlagsMigrated)
            return;

        if (C.TextColor != DtrStyle.DefaultTextColor || C.OutlineColor != DtrStyle.DefaultOutlineColor)
            C.FixedColorEnabledIds.Add(AppearanceText);

        foreach (var layoutKey in new[]
                 {
                     PluginSeparatorColor,
                     NativeSeparatorColor,
                     DivisionSeparatorColor,
                     ServerInfoTextGroup,
                 })
        {
            if (C.FixedWidthTextColors.ContainsKey(layoutKey) || C.FixedWidthOutlineColors.ContainsKey(layoutKey))
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
                || C.FixedWidthOutlineColors.ContainsKey(layoutKey);
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
