using System.Linq;
using Dalamud.Game.Gui.Dtr;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawEntriesSection(DtrOverlayGroup group)
    {
        DtrImGui.SectionHeader(
            "Plugin entries",
            () => DtrEntryOrder.ResetToNativeOrder(group),
            "##dtrEntryOrderReset",
            "Reset entry order from Dalamud DTR settings.");

        ImGui.Spacing();
        DrawPluginAddControl(group);
        ImGui.Spacing();
        DrawDtrEntryTable(group);
    }

    private static void DrawPluginAddControl(DtrOverlayGroup group)
    {
        var available = DtrOverlayGroups.GetAvailablePluginTitles(group);

        ImGui.SetNextItemWidth(200f);
        if (ImGui.BeginCombo("##addPlugin", "Add plugin..."))
        {
            foreach (var title in available)
            {
                if (ImGui.Selectable(title))
                    DtrOverlayGroups.AddPlugin(group, title);
            }

            ImGui.EndCombo();
        }
    }

    private static void DrawDtrEntryTable(DtrOverlayGroup group)
    {
        DtrOverlayGroups.SyncGroupOrder(group);

        ImGui.TextUnformatted("Plugin entries :");
        ImGuiSettingControls.Indented(() =>
        {
            if (!ImGuiEx.BeginDefaultTable("##dtrEntries", ["^", "", "Plugin", "prefix / suffix", "Min Width", "Text", "Edge", "Shadow", ""]))
                return;

            var pluginIds = DtrEntryOrder.GetOrderedPluginIdsForDisplay(group);
            for (var i = 0; i < pluginIds.Count; i++)
                DrawDtrEntryTableRow(group, pluginIds, i);

            ImGui.EndTable();
        });
    }

    private static void DrawDtrEntryTableRow(DtrOverlayGroup group, IReadOnlyList<string> pluginIds, int displayIndex)
    {
        var id = pluginIds[displayIndex];
        var entry = Svc.DtrBar.Entries.FirstOrDefault(e => e.Title == id);
        if (entry == null)
            return;

        var orderIndex = group.EntryOrder.IndexOf(id);
        if (orderIndex < 0)
            return;

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.PushID(orderIndex);
        ImGui.BeginDisabled(displayIndex == 0);
        if (ImGuiEx.SmallIconButton(FontAwesomeIcon.ArrowUp) && orderIndex > 0)
            DtrEntryOrder.MoveUp(group.EntryOrder, orderIndex);
        ImGui.EndDisabled();

        ImGui.SameLine();
        ImGui.BeginDisabled(orderIndex >= group.EntryOrder.Count - 1);
        if (ImGuiEx.SmallIconButton(FontAwesomeIcon.ArrowDown) && orderIndex < group.EntryOrder.Count - 1)
            DtrEntryOrder.MoveDown(group.EntryOrder, orderIndex);
        ImGui.EndDisabled();
        ImGui.PopID();

        ImGui.TableNextColumn();
        var showInOverlay = !group.HiddenEntryTitles.Contains(id);
        if (ImGui.Checkbox($"##overlay_{id}", ref showInOverlay))
        {
            if (showInOverlay)
                group.HiddenEntryTitles.Remove(id);
            else
                group.HiddenEntryTitles.Add(id);
            EzConfig.Save();
        }

        ImGui.BeginDisabled(!showInOverlay);

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

        ImGui.EndDisabled();

        ImGui.TableNextColumn();
        ImGui.PushID($"remove_{id}");
        if (ImGuiEx.SmallIconButton(FontAwesomeIcon.Trash))
            DtrOverlayGroups.RemovePlugin(group, id);
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Remove from group");
        ImGui.PopID();
    }

    private static void DrawPluginAffixControls(DtrOverlayGroup group, string entryTitle)
    {
        var affixes = PluginEntryAffixSettings.GetOrCreate(group, entryTitle);

        ImGui.SetNextItemWidth(88f);
        ImGui.InputTextWithHint($"##prefix_{entryTitle}", "prefix", ref affixes.Prefix, 128);

        ImGui.SameLine();
        ImGui.TextUnformatted("/");

        ImGui.SameLine();
        ImGui.SetNextItemWidth(88f);
        ImGui.InputTextWithHint($"##suffix_{entryTitle}", "suffix", ref affixes.Suffix, 128);
    }

    private static void DrawSlotWidthControls(DtrOverlayGroup group, IReadOnlyDtrBarEntry entry)
    {
        if (entry.MinimumWidth > 0)
        {
            ImGui.BeginDisabled();
            ImGui.TextUnformatted(entry.MinimumWidth.ToString());

            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip(
                    $"Plugin DTR MinimumWidth ({entry.MinimumWidth} px). "
                    + $"Overlay uses {DtrEntrySlotWidth.GetScaledFixedWidth(entry):0.##} px after font scale.");
            }

            ImGui.EndDisabled();
            return;
        }

        var overlayMin = OverlaySlotWidthSettings.Get(group, entry.Title);
        ImGui.SetNextItemWidth(88f);
        if (ImGuiSettingControls.DragInt($"##overlayMinWidth_{entry.Title}", ref overlayMin, 1f, 0, OverlaySlotWidthSettings.MaxWidth))
        {
            OverlaySlotWidthSettings.Set(group, entry.Title, overlayMin);
            EzConfig.Save();
        }

        if (ImGui.IsItemHovered())
        {
            if (overlayMin > 0)
            {
                ImGui.SetTooltip(
                    $"Fixed overlay slot: {overlayMin} px × font scale "
                    + $"({DtrEntrySlotWidth.GetScaledFixedWidth(entry):0.##} px). "
                    + "0 = follow measured text width.");
            }
            else
            {
                ImGui.SetTooltip(
                    "Minimum overlay slot width (0–1000 px). 0 follows measured text (may flicker).");
            }
        }
    }
}
