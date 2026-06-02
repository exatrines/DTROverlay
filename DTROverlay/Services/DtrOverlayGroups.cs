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
        C.OverlayGroups ??= [];
        MigrateLegacySettings();
        OverlayEntryIds.MigrateStyleHierarchy();
        EnsureSystemGroups();
        OverlayEntryIds.MigrateMergedDefaultOverrideStyles();
        MigrateGroupLayout();
        MigrateGroupScopedSettings();

        if (string.IsNullOrEmpty(C.SelectedOverlayGroupId)
            || GetById(C.SelectedOverlayGroupId) == null)
            C.SelectedOverlayGroupId = GetDefaultGroup().Id;

        if (C.FollowVanillaDtr)
            ApplyFollowVanillaConstraints();

        EnsureDisplayOrder();
        SyncDefaultGroupDisplayName();
        PluginEntryAffixSettings.NormalizeAllGroups();

        if (!IsSplitNativeMode() && IsNativeGroup(GetSelected()))
            Select(GetDefaultGroup().Id);

        foreach (var group in C.OverlayGroups)
        {
            if (!IsNativeGroup(group))
                SyncGroupOrder(group);
        }
    }

    public static DtrOverlayGroup GetDefaultGroup() =>
        C.OverlayGroups.First(g => g.Kind == DtrOverlayGroupKind.Default);

    public static DtrOverlayGroup GetNativeGroup() =>
        C.OverlayGroups.First(g => g.Kind == DtrOverlayGroupKind.Native);

    public static bool IsDefaultGroup(DtrOverlayGroup group) =>
        group.Kind == DtrOverlayGroupKind.Default;

    public static bool IsNativeGroup(DtrOverlayGroup group) =>
        group.Kind == DtrOverlayGroupKind.Native;

    public static bool IsSystemGroup(DtrOverlayGroup group) =>
        group.Kind != DtrOverlayGroupKind.Custom;

    public static bool IsSplitNativeMode() =>
        C.SplitNativeDtr && !C.FollowVanillaDtr;

    public static bool IsMergedDefaultMode() =>
        !IsSplitNativeMode() && !C.FollowVanillaDtr;

    public static bool IsMergedDefaultPanelGroup(DtrOverlayGroup group) =>
        IsMergedDefaultMode() && IsDefaultGroup(group);

    public static void SyncDefaultGroupDisplayName()
    {
        if (C.OverlayGroups == null || C.OverlayGroups.Count == 0)
            return;

        GetDefaultGroup().Name = IsMergedDefaultMode()
            ? MergedDefaultGroupName
            : DefaultGroupName;
    }

    /// <summary>Settings group list: Native is listed (disabled) under Follow Vanilla, hidden when merged into Default.</summary>
    public static bool IsGroupListedInSettings(DtrOverlayGroup group)
    {
        if (!IsNativeGroup(group))
            return true;

        return C.FollowVanillaDtr || IsSplitNativeMode();
    }

    /// <summary>Overlay windows: Native only when Split Native DTR is active.</summary>
    public static bool IsGroupHostedAsOverlay(DtrOverlayGroup group) =>
        !IsNativeGroup(group) || IsSplitNativeMode();

    public static bool IsGroupSelectableInSettings(DtrOverlayGroup group)
    {
        if (C.FollowVanillaDtr)
            return IsDefaultGroup(group);

        if (IsNativeGroup(group))
            return IsSplitNativeMode();

        return true;
    }

    public static IEnumerable<DtrOverlayGroup> EnumerateGroupsForSettings() =>
        C.OverlayGroups.Where(IsGroupListedInSettings);

    public static DtrOverlayGroup GetSelected() =>
        GetById(C.SelectedOverlayGroupId) ?? GetDefaultGroup();

    public static DtrOverlayGroup GetById(string groupId) =>
        string.IsNullOrEmpty(groupId)
            ? null
            : C.OverlayGroups.FirstOrDefault(g => g.Id == groupId);

    public static void Select(string groupId)
    {
        if (GetById(groupId) is not { } group)
            return;

        if (C.FollowVanillaDtr && !IsDefaultGroup(group))
            return;

        if (!IsGroupListedInSettings(group))
            return;

        C.SelectedOverlayGroupId = groupId;
        EzConfig.Save();
    }

    public static void ApplyFollowVanillaConstraints()
    {
        if (!ApplyFollowVanillaConstraintsCore())
            return;

        EzConfig.Save();
        OverlayWindowHost.RequestRefresh();
    }

    // Follow Vanilla の制約（グループ名固定・Native 選択の解除）を適用する。
    // 実際に値が変化した場合のみ true を返す。保存・再描画要求は呼び出し側に委ねるため、
    // 毎フレーム呼ばれても変化が無ければ何もしない（FPS 低下対策）。
    internal static bool ApplyFollowVanillaConstraintsCore()
    {
        if (!C.FollowVanillaDtr || C.OverlayGroups.Count == 0)
            return false;

        var changed = false;

        var def = GetDefaultGroup();
        if (def.Name != DefaultGroupName)
        {
            def.Name = DefaultGroupName;
            changed = true;
        }

        var native = GetNativeGroup();
        if (native.Name != NativeGroupName)
        {
            native.Name = NativeGroupName;
            changed = true;
        }

        if (IsNativeGroup(GetSelected()))
        {
            // Select() は内部で EzConfig.Save を呼ぶため、ここでは選択 ID のみ更新し保存はまとめて行う。
            C.SelectedOverlayGroupId = def.Id;
            changed = true;
        }

        return changed;
    }

    public static bool TryAddGroup(string name)
    {
        EnsureInitialized();
        var trimmed = name.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return false;

        if (trimmed.Equals(DefaultGroupName, StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(MergedDefaultGroupName, StringComparison.OrdinalIgnoreCase)
            || trimmed.Equals(NativeGroupName, StringComparison.OrdinalIgnoreCase))
            return false;

        var group = CreateGroup(trimmed);
        OverlayGroupLayout.CopyLayoutFrom(group, GetDefaultGroup());
        C.OverlayGroups.Add(group);
        EnsureDisplayOrder();
        C.SelectedOverlayGroupId = group.Id;
        EzConfig.Save();
        OverlayWindowHost.RequestRefresh();
        return true;
    }

    public static bool TryRemoveGroup(string groupId)
    {
        EnsureInitialized();
        if (GetById(groupId) is not { } group || IsSystemGroup(group))
            return false;

        var index = C.OverlayGroups.FindIndex(g => g.Id == groupId);
        if (index < 0)
            return false;

        C.OverlayGroups.RemoveAt(index);
        DtrOverlayFonts.ReleaseGroup(groupId);
        EnsureDisplayOrder();
        if (C.SelectedOverlayGroupId == groupId)
            C.SelectedOverlayGroupId = GetDefaultGroup().Id;

        EzConfig.Save();
        OverlayWindowHost.RequestRefresh();
        return true;
    }

    public static bool CanRemoveGroup(DtrOverlayGroup group) =>
        !IsSystemGroup(group);

    private static void EnsureDisplayOrder()
    {
        if (C.OverlayGroups.Count == 0)
            return;

        DtrOverlayGroup native = null;
        DtrOverlayGroup def = null;
        var customs = new List<DtrOverlayGroup>();

        foreach (var group in C.OverlayGroups)
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

        if (IsSameOrder(C.OverlayGroups, ordered))
            return;

        C.OverlayGroups.Clear();
        C.OverlayGroups.AddRange(ordered);
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
        if (IsNativeGroup(group))
            return false;

        if (string.IsNullOrEmpty(entryTitle) || group.EntryOrder.Contains(entryTitle))
            return false;

        if (Svc.DtrBar.Entries.All(e => e.Title != entryTitle))
            return false;

        group.EntryOrder.Add(entryTitle);
        EzConfig.Save();
        return true;
    }

    public static bool RemovePlugin(DtrOverlayGroup group, string entryTitle)
    {
        if (IsNativeGroup(group))
            return false;

        if (!group.EntryOrder.Remove(entryTitle))
            return false;

        EzConfig.Save();
        return true;
    }

    public static IReadOnlyList<string> GetAvailablePluginTitles(DtrOverlayGroup group) =>
        IsNativeGroup(group)
            ? []
            : Svc.DtrBar.Entries
                .Select(e => e.Title)
                .Where(title => !group.EntryOrder.Contains(title))
                .ToList();

    /// <summary>
    /// Merges live DTR bar entries into the group's saved order.
    /// Does not remove titles that are not registered yet — plugins often register DTR entries
    /// after login, and pruning early would drop the user's saved order (see issue with Follow Vanilla).
    /// </summary>
    public static void SyncGroupOrder(DtrOverlayGroup group)
    {
        if (IsNativeGroup(group) || !Svc.ClientState.IsLoggedIn)
            return;

        group.EntryOrder ??= [];

        foreach (var entry in Svc.DtrBar.Entries)
        {
            if (!group.EntryOrder.Contains(entry.Title))
                group.EntryOrder.Add(entry.Title);
        }
    }

    public static void ResetGroupToNativeOrder(DtrOverlayGroup group)
    {
        if (IsNativeGroup(group))
            return;

        group.EntryOrder.Clear();
        group.EntryOrder.AddRange(Svc.DtrBar.Entries.Select(e => e.Title));
        EzConfig.Save();
    }

    private static DtrOverlayGroup CreateGroup(string name) =>
        new() { Name = name, Kind = DtrOverlayGroupKind.Custom };

    private static void EnsureSystemGroups()
    {
        if (C.OverlayGroups.Count == 0)
            C.OverlayGroups.Add(CreateDefaultGroup());
        else if (C.OverlayGroups.All(g => g.Kind != DtrOverlayGroupKind.Default))
            C.OverlayGroups[0].Kind = DtrOverlayGroupKind.Default;

        var defaultGroup = GetDefaultGroup();
        defaultGroup.Kind = DtrOverlayGroupKind.Default;
        SyncDefaultGroupDisplayName();

        if (C.OverlayGroups.All(g => g.Kind != DtrOverlayGroupKind.Native))
        {
            C.OverlayGroups.Insert(0, CreateNativeGroup());
            EzConfig.Save();
        }
        else
        {
            GetNativeGroup().Kind = DtrOverlayGroupKind.Native;
            GetNativeGroup().Name = NativeGroupName;
        }

        MigrateNativeGroup();
    }

    private static void MigrateNativeGroup()
    {
        if (C.NativeGroupMigrated)
            return;

        C.NativeGroupMigrated = true;
        EzConfig.Save();
    }

    private static void MigrateGroupLayout()
    {
        if (C.GroupLayoutMigrated)
            return;

        foreach (var group in C.OverlayGroups)
            OverlayGroupLayout.CopyLayoutFromConfiguration(group);

        C.GroupLayoutMigrated = true;
        EzConfig.Save();
    }

    private static void MigrateGroupScopedSettings()
    {
        if (C.GroupScopedSettingsMigrated)
            return;

        var native = GetNativeGroup();
        native.ShowServerInfo = C.ShowServerInfo;
        native.ServerInfoDisplayMode = C.ServerInfoDisplayMode;
        native.HiddenServerInfoParts = [.. C.HiddenServerInfoParts];

        foreach (var group in C.OverlayGroups)
        {
            group.ShowPluginEntrySeparators = C.ShowPluginEntrySeparators;
            group.ShowNativeEntrySeparators = C.ShowNativeEntrySeparators;
            group.ShowDivisionSeparatorBar = C.ShowDivisionSeparatorBar;

            foreach (var title in group.EntryOrder)
                MigratePluginScopedSettings(group, title);
        }

        foreach (var legacyTitle in CollectLegacyPluginTitles())
        {
            foreach (var group in C.OverlayGroups)
            {
                if (group.EntryOrder.Contains(legacyTitle))
                    MigratePluginScopedSettings(group, legacyTitle);
            }
        }

        C.GroupScopedSettingsMigrated = true;
        EzConfig.Save();
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

        C.OverlayGroups = [group];
        C.SelectedOverlayGroupId = group.Id;
        C.OverlayGroupsMigrated = true;
        EzConfig.Save();
    }
}
