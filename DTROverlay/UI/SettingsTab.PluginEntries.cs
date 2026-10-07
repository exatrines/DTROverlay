using Dalamud.Game.Gui.Dtr;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static string _addPluginChoice = "";

    private static void DrawEntriesSection(DtrOverlayGroup group)
    {
        MirageUi.SubHeader(I18n.Get("entries.header"));

        DrawPluginAddControl(group);
        DrawDtrEntryTable(group);
    }

    private static void DrawPluginAddControl(DtrOverlayGroup group)
    {
        var available = DtrOverlayGroups.GetAvailablePluginTitles(group);
        if (MirageUi.Dropdown(
                I18n.Get("entries.add"),
                ref _addPluginChoice,
                available,
                I18n.Get("entries.add_placeholder"),
                "addPlugin",
                allowClear: false,
                emptyMessage: I18n.Get("entries.empty")))
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

        if (!SettingsTables.BeginDefaultTable("##dtrEntries", [
                "^",
                "",
                "~" + I18n.Get("entries.plugin"),
                I18n.Get("entries.affix"),
                I18n.Get("entries.min_width"),
                I18n.Get("style.column.text"),
                I18n.Get("style.column.edge"),
                I18n.Get("style.column.shadow"),
                "",
            ]))
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
            DrawPluginName(entry.Title);

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
        if (MirageUi.IconButton(FontAwesomeIcon.Trash, $"remove_{id}", iconSize, tooltip: I18n.Get("entries.remove")))
            DtrOverlayGroups.RemovePlugin(group, id);
    }

    private static void DrawPluginName(string title)
    {
        var shown = FitText(title, ImGui.GetContentRegionAvail().X);
        ImGui.TextUnformatted(shown);
        if (!string.Equals(shown, title, StringComparison.Ordinal))
            MirageUi.Tooltip(title);
    }

    private static string FitText(string text, float avail)
    {
        if (avail <= 0f || ImGui.CalcTextSize(text).X <= avail)
            return text;

        const string ellipsis = "…";
        var budget = avail - ImGui.CalcTextSize(ellipsis).X;
        if (budget <= 0f)
            return ellipsis;

        var count = text.Length;
        while (count > 0 && ImGui.CalcTextSize(text[..count]).X > budget)
            count--;

        return count == 0 ? ellipsis : text[..count] + ellipsis;
    }

    private static void DrawPluginAffixControls(DtrOverlayGroup group, string entryTitle)
    {
        var affixes = PluginEntryAffixSettings.GetOrCreate(group, entryTitle);

        var prefixChanged = MirageUi.InputText(
            "",
            ref affixes.Prefix,
            128,
            $"prefix_{entryTitle}",
            I18n.Get("entries.prefix"),
            88f);

        ImGui.SameLine();
        ImGui.TextUnformatted("/");
        ImGui.SameLine();

        var suffixChanged = MirageUi.InputText(
            "",
            ref affixes.Suffix,
            128,
            $"suffix_{entryTitle}",
            I18n.Get("entries.suffix"),
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

            MirageUi.Tooltip(I18n.Format(
                "entries.min_width_plugin",
                entry.MinimumWidth,
                DtrEntrySlotWidth.GetScaledFixedWidth(entry).ToString("0.##")));
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
            MirageUi.Tooltip(I18n.Format(
                "entries.slot_fixed",
                overlayMin,
                DtrEntrySlotWidth.GetScaledFixedWidth(entry).ToString("0.##")));
        }
        else
        {
            MirageUi.Tooltip(I18n.Get("entries.slot_auto"));
        }
    }
}
