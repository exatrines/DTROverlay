using Dalamud.Game.Gui.Dtr;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static string _addPluginChoice = "";

    private static void DrawEntriesSection(DtrOverlayGroup group)
    {
        MirageUi.SubHeader("DTR entries");

        DrawPluginAddControl(group);
        DrawDtrEntryTable(group);
    }

    private static void DrawPluginAddControl(DtrOverlayGroup group)
    {
        var available = DtrOverlayGroups.GetAvailablePluginTitles(group);
        if (MirageUi.Dropdown(
                "Add plugin",
                ref _addPluginChoice,
                available,
                "Add plugin...",
                "addPlugin",
                allowClear: false,
                emptyMessage: "No plugins available"))
        {
            if (string.IsNullOrEmpty(_addPluginChoice))
                return;

            DtrOverlayGroups.AddPlugin(group, _addPluginChoice);
            _addPluginChoice = "";
        }
    }

    private static void DrawDtrEntryTable(DtrOverlayGroup group)
    {
        DtrOverlayGroups.SyncOverlayOrder(group);

        if (!SettingsTables.BeginDefaultTable("##dtrEntries", ["^", "", "Plugin", "prefix / suffix", "Min Width", "Text", "Edge", "Shadow", ""]))
            return;

        var pluginIds = DtrEntryOrder.GetOrderedPluginIdsForDisplay(group);
        for (var i = 0; i < pluginIds.Count; i++)
            DrawDtrEntryTableRow(group, pluginIds, i);

        ImGui.EndTable();
    }

    private static void DrawDtrEntryTableRow(DtrOverlayGroup group, IReadOnlyList<string> pluginIds, int displayIndex)
    {
        var id = pluginIds[displayIndex];
        var entry = PluginServices.DtrBar.Entries.FirstOrDefault(e => e.Title == id);
        if (entry == null)
            return;

        var orderIndex = group.EntryOrder.IndexOf(id);
        if (orderIndex < 0)
            return;

        var iconSize = SmallIconSize();
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.PushID(orderIndex);
        if (MirageUi.IconButton(FontAwesomeIcon.ArrowUp, "moveUp", iconSize, enabled: displayIndex > 0)
            && orderIndex > 0)
            DtrEntryOrder.MoveUp(group.EntryOrder, orderIndex);

        ImGui.SameLine();
        if (MirageUi.IconButton(
                FontAwesomeIcon.ArrowDown,
                "moveDown",
                iconSize,
                enabled: orderIndex < group.EntryOrder.Count - 1)
            && orderIndex < group.EntryOrder.Count - 1)
            DtrEntryOrder.MoveDown(group.EntryOrder, orderIndex);
        ImGui.PopID();

        ImGui.TableNextColumn();
        var showInOverlay = !group.HiddenEntryTitles.Contains(id);
        if (MirageUi.Checkbox($"##overlay_{id}", ref showInOverlay))
        {
            if (showInOverlay)
                group.HiddenEntryTitles.Remove(id);
            else
                group.HiddenEntryTitles.Add(id);
            C.Save();
        }

        using (MirageUi.DisabledIf(!showInOverlay))
        {
            ImGui.TableNextColumn();
            ImGui.TextUnformatted(entry.Title);

            ImGui.TableNextColumn();
            DrawPluginAffixControls(group, entry.Title);

            ImGui.TableNextColumn();
            DrawSlotWidthControls(group, entry);

            var styleKey = GroupStyleKeys.PluginEntry(group.Id, entry.Title);
            ImGui.TableNextColumn();
            OverlayColorPicker.DrawTextColumn(styleKey, $"table_{entry.Title}", rowEnabled: true);

            ImGui.TableNextColumn();
            OverlayColorPicker.DrawEdgeColumn(styleKey, $"table_{entry.Title}", rowEnabled: true);

            ImGui.TableNextColumn();
            OverlayColorPicker.DrawShadowColumn(styleKey, $"table_{entry.Title}", rowEnabled: true);
        }

        ImGui.TableNextColumn();
        if (MirageUi.IconButton(FontAwesomeIcon.Trash, $"remove_{id}", iconSize, tooltip: "Remove from overlay"))
            DtrOverlayGroups.RemovePlugin(group, id);
    }

    private static void DrawPluginAffixControls(DtrOverlayGroup group, string entryTitle)
    {
        var affixes = PluginEntryAffixSettings.GetOrCreate(group, entryTitle);

        var prefixChanged = MirageUi.InputText(
            "",
            ref affixes.Prefix,
            128,
            $"prefix_{entryTitle}",
            "prefix",
            88f);

        ImGui.SameLine();
        ImGui.TextUnformatted("/");
        ImGui.SameLine();

        var suffixChanged = MirageUi.InputText(
            "",
            ref affixes.Suffix,
            128,
            $"suffix_{entryTitle}",
            "suffix",
            88f);

        if (!prefixChanged && !suffixChanged)
            return;

        affixes.Normalize();
        C.Save();
    }

    private static void DrawSlotWidthControls(DtrOverlayGroup group, IReadOnlyDtrBarEntry entry)
    {
        if (entry.MinimumWidth > 0)
        {
            using (MirageUi.DisabledIf(true))
                ImGui.TextUnformatted(entry.MinimumWidth.ToString());

            MirageUi.Tooltip(
                $"Plugin DTR MinimumWidth ({entry.MinimumWidth} px). "
                + $"Overlay uses {DtrEntrySlotWidth.GetScaledFixedWidth(entry):0.##} px after font scale.");
            return;
        }

        var overlayMin = OverlaySlotWidthSettings.Get(group, entry.Title);
        if (MirageUi.SliderInt("", ref overlayMin, 0, OverlaySlotWidthSettings.MaxWidth, $"overlayMinWidth_{entry.Title}", 88f))
        {
            OverlaySlotWidthSettings.Set(group, entry.Title, overlayMin);
            C.Save();
        }

        if (overlayMin > 0)
        {
            MirageUi.Tooltip(
                $"Fixed overlay slot: {overlayMin} px × font scale "
                + $"({DtrEntrySlotWidth.GetScaledFixedWidth(entry):0.##} px). "
                + "0 = follow measured text width.");
        }
        else
        {
            MirageUi.Tooltip("Minimum overlay slot width (0–1000 px). 0 follows measured text (may flicker).");
        }
    }
}
