using System.Linq;
using Dalamud.Game.Gui.Dtr;

namespace DTROverlay.Services;

internal static class DtrOverlayGroups
{
    public const string DefaultGroupName = "Default";
    public const string MergedDefaultGroupName = "Default+Native";
    public const string NativeGroupName = "Native";

    public static void EnsureInitialized()
    {
        C.Overlays ??= [];
        MigrateLegacySettings();
        OverlayEntryIds.MigrateStyleHierarchy();
        EnsureSystemGroups();
        OverlayEntryIds.MigrateMergedDefaultOverrideStyles();
        MigrateGroupLayout();
        MigrateGroupScopedSettings();
        MigrateAutoOverlayNames();

        if (string.IsNullOrEmpty(C.SelectedOverlayId)
            || GetById(C.SelectedOverlayId) == null)
            C.SelectedOverlayId = GetDefaultOverlay().Id;

        if (C.FollowNativeDtr)
            ApplyFollowNativeConstraints();

        EnsureDisplayOrder();
        SyncDefaultOverlayDisplayName();
        PluginEntryAffixSettings.NormalizeAllGroups();

        if (!IsOverlayListedInSettings(GetSelected()))
            Select(GetDefaultOverlay().Id);

        foreach (var group in C.Overlays)
        {
            if (!IsNativeOverlay(group))
                SyncOverlayOrder(group);
        }
    }

    public static DtrOverlayGroup GetDefaultOverlay() =>
        C.Overlays.First(g => g.Kind == DtrOverlayGroupKind.Default);

    public static DtrOverlayGroup GetNativeOverlay() =>
        C.Overlays.First(g => g.Kind == DtrOverlayGroupKind.Native);

    public static bool IsDefaultOverlay(DtrOverlayGroup group) =>
        group.Kind == DtrOverlayGroupKind.Default;

    public static bool IsNativeOverlay(DtrOverlayGroup group) =>
        group.Kind == DtrOverlayGroupKind.Native;

    public static bool IsSystemOverlay(DtrOverlayGroup group) =>
        group.Kind != DtrOverlayGroupKind.Custom;

    public static bool IsSplitNativeMode() =>
        C.SplitNativeDtr && !C.FollowNativeDtr;

    public static bool IsMergedDefaultMode() =>
        !IsSplitNativeMode() && !C.FollowNativeDtr;

    public static bool IsMergedDefaultPanelOverlay(DtrOverlayGroup group) =>
        IsMergedDefaultMode() && IsDefaultOverlay(group);

    public static void SyncDefaultOverlayDisplayName()
    {
        if (C.Overlays == null || C.Overlays.Count == 0)
            return;

        GetDefaultOverlay().Name = IsMergedDefaultMode()
            ? MergedDefaultGroupName
            : DefaultGroupName;
    }

    /// <summary>Settings group list: Native is listed only in Split Native DTR.</summary>
    public static bool IsOverlayListedInSettings(DtrOverlayGroup group)
    {
        if (!IsNativeOverlay(group))
            return true;

        return IsSplitNativeMode();
    }

    /// <summary>Overlay windows: Native only when Split Native DTR is active.</summary>
    public static bool IsOverlayHosted(DtrOverlayGroup group) =>
        !IsNativeOverlay(group) || IsSplitNativeMode();

    public static IEnumerable<DtrOverlayGroup> EnumerateGroupsForSettings() =>
        C.Overlays.Where(IsOverlayListedInSettings);

    public static DtrOverlayGroup GetSelected() =>
        GetById(C.SelectedOverlayId) ?? GetDefaultOverlay();

    public static DtrOverlayGroup GetById(string groupId) =>
        string.IsNullOrEmpty(groupId)
            ? null
            : C.Overlays.FirstOrDefault(g => g.Id == groupId);

    public static void Select(string groupId)
    {
        if (GetById(groupId) is not { } group)
            return;

        if (!IsOverlayListedInSettings(group))
            return;

        C.SelectedOverlayId = groupId;
        C.Save();
    }

    public static void ApplyFollowNativeConstraints()
    {
        if (!ApplyFollowNativeConstraintsCore())
            return;

        C.Save();
        OverlayWindowHost.RequestRefresh();
    }

    // Follow Vanilla の制約（グループ名固定）を適用する。
    // 実際に値が変化した場合のみ true を返す。保存・再描画要求は呼び出し側に委ねるため、
    // 毎フレーム呼ばれても変化が無ければ何もしない（FPS 低下対策）。
    internal static bool ApplyFollowNativeConstraintsCore()
    {
        if (!C.FollowNativeDtr || C.Overlays.Count == 0)
            return false;

        var changed = false;

        var def = GetDefaultOverlay();
        if (def.Name != DefaultGroupName)
        {
            def.Name = DefaultGroupName;
            changed = true;
        }

        var native = GetNativeOverlay();
        if (native.Name != NativeGroupName)
        {
            native.Name = NativeGroupName;
            changed = true;
        }

        if (IsNativeOverlay(GetSelected()))
        {
            C.SelectedOverlayId = def.Id;
            changed = true;
        }

        return changed;
    }

    public static bool TryAddOverlay() =>
        TryAddOverlay(AllocateCustomGroupName());

    public static bool TryAddOverlay(string name)
    {
        EnsureInitialized();
        var trimmed = name.Trim();
        if (!IsUsableCustomGroupName(trimmed))
            return false;

        var group = CreateGroup(trimmed);
        OverlayGroupLayout.CopyLayoutFrom(group, GetDefaultOverlay());
        C.Overlays.Add(group);
        EnsureDisplayOrder();
        C.SelectedOverlayId = group.Id;
        C.Save();
        OverlayWindowHost.RequestRefresh();
        return true;
    }

    public static bool TryRemoveOverlay(string groupId)
    {
        EnsureInitialized();
        if (GetById(groupId) is not { } group || IsSystemOverlay(group))
            return false;

        var index = C.Overlays.FindIndex(g => g.Id == groupId);
        if (index < 0)
            return false;

        C.Overlays.RemoveAt(index);
        DtrOverlayFonts.ReleaseGroup(groupId);
        EnsureDisplayOrder();
        if (C.SelectedOverlayId == groupId)
            C.SelectedOverlayId = GetDefaultOverlay().Id;

        C.Save();
        OverlayWindowHost.RequestRefresh();
        return true;
    }

    public static bool CanRemoveOverlay(DtrOverlayGroup group) =>
        !IsSystemOverlay(group);

    public static bool TryRenameOverlay(DtrOverlayGroup group, string name)
    {
        if (IsSystemOverlay(group))
            return false;

        var trimmed = name.Trim();
        if (!IsUsableCustomGroupName(trimmed) || group.Name == trimmed)
            return false;

        group.Name = trimmed;
        C.Save();
        return true;
    }

    private static void EnsureDisplayOrder()
    {
        if (C.Overlays.Count == 0)
            return;

        DtrOverlayGroup native = null;
        DtrOverlayGroup def = null;
        var customs = new List<DtrOverlayGroup>();

        foreach (var group in C.Overlays)
        {
            switch (group.Kind)
            {
                case DtrOverlayGroupKind.Native:
                    native = group;
                    break;
                case DtrOverlayGroupKind.Default:
                    def = group;
                    break;
                default:
                    customs.Add(group);
                    break;
            }
        }

        if (native == null || def == null)
            return;

        var ordered = new List<DtrOverlayGroup> { native, def };
        ordered.AddRange(customs);

        if (IsSameOrder(C.Overlays, ordered))
            return;

        C.Overlays.Clear();
        C.Overlays.AddRange(ordered);
    }

    private static bool IsSameOrder(IReadOnlyList<DtrOverlayGroup> current, List<DtrOverlayGroup> ordered)
    {
        if (current.Count != ordered.Count)
            return false;

        for (var i = 0; i < current.Count; i++)
        {
            if (current[i].Id != ordered[i].Id)
                return false;
        }

        return true;
    }

    public static bool AddPlugin(DtrOverlayGroup group, string entryTitle)
    {
        if (IsNativeOverlay(group))
            return false;

        if (string.IsNullOrEmpty(entryTitle) || group.EntryOrder.Contains(entryTitle))
            return false;

        if (PluginServices.DtrBar.Entries.All(e => e.Title != entryTitle))
            return false;

        group.EntryOrder.Add(entryTitle);
        C.Save();
        return true;
    }

    public static bool RemovePlugin(DtrOverlayGroup group, string entryTitle)
    {
        if (IsNativeOverlay(group))
            return false;

        if (!group.EntryOrder.Remove(entryTitle))
            return false;

        C.Save();
        return true;
    }

    public static IReadOnlyList<string> GetAvailablePluginTitles(DtrOverlayGroup group) =>
        IsNativeOverlay(group)
            ? []
            : PluginServices.DtrBar.Entries
                .Select(e => e.Title)
                .Where(title => !group.EntryOrder.Contains(title))
                .ToList();

    /// <summary>
    /// Merges live DTR bar entries into the group's saved order.
    /// Does not remove titles that are not registered yet — plugins often register DTR entries
    /// after login, and pruning early would drop the user's saved order (see issue with Follow Vanilla).
    /// </summary>
    public static void SyncOverlayOrder(DtrOverlayGroup group)
    {
        if (IsNativeOverlay(group) || !PluginServices.ClientState.IsLoggedIn)
            return;

        group.EntryOrder ??= [];

        foreach (var entry in PluginServices.DtrBar.Entries)
        {
            if (!group.EntryOrder.Contains(entry.Title))
                group.EntryOrder.Add(entry.Title);
        }
    }

    private static DtrOverlayGroup CreateGroup(string name) =>
        new() { Name = name, Kind = DtrOverlayGroupKind.Custom };

    private static bool IsUsableCustomGroupName(string name) =>
        !string.IsNullOrEmpty(name)
        && !name.Equals(DefaultGroupName, StringComparison.OrdinalIgnoreCase)
        && !name.Equals(MergedDefaultGroupName, StringComparison.OrdinalIgnoreCase)
        && !name.Equals(NativeGroupName, StringComparison.OrdinalIgnoreCase);

    private static void MigrateAutoOverlayNames()
    {
        var changed = false;
        foreach (var group in C.Overlays)
        {
            if (group.Kind != DtrOverlayGroupKind.Custom || !group.Name.StartsWith("Group "))
                continue;

            var suffix = group.Name["Group ".Length..];
            if (suffix.Length == 0 || !suffix.All(char.IsDigit))
                continue;

            var next = "Overlay " + suffix;
            if (C.Overlays.Any(other =>
                    other.Id != group.Id && other.Name.Equals(next, StringComparison.OrdinalIgnoreCase)))
                continue;

            group.Name = next;
            changed = true;
        }

        if (changed)
            C.Save();
    }

    private static string AllocateCustomGroupName()
    {
        for (var n = 1; ; n++)
        {
            var name = $"Overlay {n}";
            if (C.Overlays.All(g => !g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                return name;
        }
    }

    private static void EnsureSystemGroups()
    {
        if (C.Overlays.Count == 0)
            C.Overlays.Add(CreateDefaultGroup());
        else if (C.Overlays.All(g => g.Kind != DtrOverlayGroupKind.Default))
            C.Overlays[0].Kind = DtrOverlayGroupKind.Default;

        var defaultGroup = GetDefaultOverlay();
        defaultGroup.Kind = DtrOverlayGroupKind.Default;
        SyncDefaultOverlayDisplayName();

        if (C.Overlays.All(g => g.Kind != DtrOverlayGroupKind.Native))
        {
            C.Overlays.Insert(0, CreateNativeGroup());
            C.Save();
        }
        else
        {
            GetNativeOverlay().Kind = DtrOverlayGroupKind.Native;
            GetNativeOverlay().Name = NativeGroupName;
        }

        MigrateNativeGroup();
    }

    private static void MigrateNativeGroup()
    {
        if (C.NativeGroupMigrated)
            return;

        C.NativeGroupMigrated = true;
        C.Save();
    }

    private static void MigrateGroupLayout()
    {
        if (C.GroupLayoutMigrated)
            return;

        foreach (var group in C.Overlays)
            OverlayGroupLayout.CopyLayoutFromConfiguration(group);

        C.GroupLayoutMigrated = true;
        C.Save();
    }

    private static void MigrateGroupScopedSettings()
    {
        if (C.GroupScopedSettingsMigrated)
            return;

        var native = GetNativeOverlay();
        native.ShowServerInfo = C.ShowServerInfo;
        native.ServerInfoDisplayMode = C.ServerInfoDisplayMode;
        native.HiddenServerInfoParts = [.. C.HiddenServerInfoParts];

        foreach (var group in C.Overlays)
        {
            group.ShowPluginEntrySeparators = C.ShowPluginEntrySeparators;
            group.ShowNativeEntrySeparators = C.ShowNativeEntrySeparators;
            group.ShowDivisionSeparatorBar = C.ShowDivisionSeparatorBar;

            foreach (var title in group.EntryOrder)
                MigratePluginScopedSettings(group, title);
        }

        foreach (var legacyTitle in CollectLegacyPluginTitles())
        {
            foreach (var group in C.Overlays)
            {
                if (group.EntryOrder.Contains(legacyTitle))
                    MigratePluginScopedSettings(group, legacyTitle);
            }
        }

        C.GroupScopedSettingsMigrated = true;
        C.Save();
    }

    private static IEnumerable<string> CollectLegacyPluginTitles()
    {
        var titles = new HashSet<string>(StringComparer.Ordinal);

        foreach (var title in C.EntryOrder)
            titles.Add(title);

        foreach (var key in C.FixedWidthTextColors.Keys)
        {
            if (IsLegacyPluginTitleKey(key))
                titles.Add(key);
        }

        foreach (var key in C.PluginEntryAffixesByTitle.Keys)
            titles.Add(key);

        foreach (var key in C.OverlaySlotMinWidthByTitle.Keys)
            titles.Add(key);

        return titles;
    }

    private static bool IsLegacyPluginTitleKey(string key) =>
        !string.IsNullOrEmpty(key)
        && !key.StartsWith("@", StringComparison.Ordinal)
        && !OverlayEntryIds.IsNative(key)
        && !GroupStyleKeys.IsPluginEntryKey(key)
        && !GroupStyleKeys.IsOverrideKey(key)
        && key != OverlayEntryIds.DefaultText
        && key != OverlayEntryIds.DefaultSeparator
        && !DtrSeparatorStyle.IsSeparatorKey(key);

    private static void MigratePluginScopedSettings(DtrOverlayGroup group, string pluginTitle)
    {
        var scopedKey = GroupStyleKeys.PluginEntry(group.Id, pluginTitle);

        if (C.PluginEntryAffixesByTitle.TryGetValue(pluginTitle, out var affixes))
        {
            affixes.Normalize();
            group.PluginEntryAffixesByTitle[pluginTitle] = affixes;
        }

        if (C.OverlaySlotMinWidthByTitle.TryGetValue(pluginTitle, out var width))
            group.OverlaySlotMinWidthByTitle[pluginTitle] = width;

        MigrateStyleDictEntry(C.FixedWidthTextColors, pluginTitle, scopedKey);
        MigrateStyleDictEntry(C.FixedWidthOutlineColors, pluginTitle, scopedKey);
        MigrateStyleDictEntry(C.FixedWidthShadowColors, pluginTitle, scopedKey);
        MigrateStyleDictEntry(C.FixedWidthEdgeStrengths, pluginTitle, scopedKey);
        MigrateStyleDictEntry(C.FixedWidthShadowThicknesses, pluginTitle, scopedKey);
        MigrateStyleDictEntry(C.FixedWidthPixels, pluginTitle, scopedKey);

        MigrateStyleDictFlag(C.FixedTextColorEnabledIds, pluginTitle, scopedKey);
        MigrateStyleDictFlag(C.FixedEdgeStyleEnabledIds, pluginTitle, scopedKey);
        MigrateStyleDictFlag(C.FixedShadowStyleEnabledIds, pluginTitle, scopedKey);
        MigrateStyleDictFlag(C.FixedWidthEnabledIds, pluginTitle, scopedKey);
        MigrateStyleDictFlag(C.FixedColorEnabledIds, pluginTitle, scopedKey);

        if (C.FixedWidthEdgeEnabled.Remove(pluginTitle, out var edgeEnabled))
            C.FixedWidthEdgeEnabled[scopedKey] = edgeEnabled;

        if (C.FixedWidthShadowEnabled.Remove(pluginTitle, out var shadowEnabled))
            C.FixedWidthShadowEnabled[scopedKey] = shadowEnabled;
    }

    private static void MigrateStyleDictEntry<T>(Dictionary<string, T> dict, string fromKey, string toKey)
    {
        if (dict.TryGetValue(fromKey, out var value))
            dict[toKey] = value;
    }

    private static void MigrateStyleDictFlag(HashSet<string> set, string fromKey, string toKey)
    {
        if (set.Remove(fromKey))
            set.Add(toKey);
    }

    private static DtrOverlayGroup CreateDefaultGroup() =>
        new()
        {
            Kind = DtrOverlayGroupKind.Default,
            Name = DefaultGroupName,
        };

    private static DtrOverlayGroup CreateNativeGroup() =>
        new()
        {
            Kind = DtrOverlayGroupKind.Native,
            Name = NativeGroupName,
            OverlayPosition = new(20f, 40f),
        };

    private static void MigrateLegacySettings()
    {
        if (C.OverlayGroupsMigrated)
            return;

        var group = new DtrOverlayGroup
        {
            Kind = DtrOverlayGroupKind.Default,
            Name = DefaultGroupName,
            OverlayPosition = C.OverlayPosition,
            OverlayPositionOrigin = C.OverlayPositionOrigin,
            OverlayEditMode = C.OverlayEditMode,
            EntryOrder = [.. C.EntryOrder],
            HiddenEntryTitles = [.. C.HiddenEntryTitles],
        };

        C.Overlays = [group];
        C.SelectedOverlayId = group.Id;
        C.OverlayGroupsMigrated = true;
        C.Save();
    }
}
