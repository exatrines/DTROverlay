using System.Linq;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawEntriesSection()
    {
        DtrImGui.SectionHeader(
            "Plugin entries",
            DtrEntryOrder.ResetToNativeOrder,
            "##dtrEntryOrderReset",
            "Reset entry order from Dalamud DTR settings.");

        ImGui.Spacing();
        DrawDtrEntryTable();
    }

    private static void DrawDtrEntryTable()
    {
        DtrEntryOrder.SyncOrder();

        ImGui.TextUnformatted("Plugin entries :");
        ImGuiSettingControls.Indented(() =>
        {
            if (!ImGuiEx.BeginDefaultTable("##dtrEntries", ["^", "", "Plugin", "prefix / suffix", "Min Width", "Color"]))
                return;

            var pluginIds = DtrEntryOrder.GetOrderedPluginIdsForDisplay();
            for (var i = 0; i < pluginIds.Count; i++)
                DrawDtrEntryTableRow(pluginIds, i);

            ImGui.EndTable();
        });
    }

    private static void DrawDtrEntryTableRow(IReadOnlyList<string> pluginIds, int displayIndex)
    {
        var id = pluginIds[displayIndex];
        var entry = Svc.DtrBar.Entries.FirstOrDefault(e => e.Title == id);
        if (entry == null)
            return;

        var orderIndex = C.EntryOrder.IndexOf(id);
        if (orderIndex < 0)
            return;

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.PushID(orderIndex);
        ImGui.BeginDisabled(displayIndex == 0);
        if (ImGuiEx.SmallIconButton(FontAwesomeIcon.ArrowUp) && orderIndex > 0)
            DtrEntryOrder.MoveUp(C.EntryOrder, orderIndex);
        ImGui.EndDisabled();

        ImGui.SameLine();
        ImGui.BeginDisabled(orderIndex >= C.EntryOrder.Count - 1);
        if (ImGuiEx.SmallIconButton(FontAwesomeIcon.ArrowDown) && orderIndex < C.EntryOrder.Count - 1)
            DtrEntryOrder.MoveDown(C.EntryOrder, orderIndex);
        ImGui.EndDisabled();
        ImGui.PopID();

        ImGui.TableNextColumn();
        var showInOverlay = !C.HiddenEntryTitles.Contains(id);
        if (ImGui.Checkbox($"##overlay_{id}", ref showInOverlay))
        {
            if (showInOverlay)
                C.HiddenEntryTitles.Remove(id);
            else
                C.HiddenEntryTitles.Add(id);
        }

        ImGui.BeginDisabled(!showInOverlay);

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(entry.Title);

        ImGui.TableNextColumn();
        DrawPluginAffixControls(entry.Title);

        ImGui.TableNextColumn();
        DrawMinWidthControls(entry.Title, $"table_{entry.Title}");

        ImGui.TableNextColumn();
        OverlayColorPicker.DrawPluginEntryColors(entry.Title, $"table_{entry.Title}");

        ImGui.EndDisabled();
    }

    private static void DrawPluginAffixControls(string entryTitle)
    {
        var affixes = PluginEntryAffixSettings.GetOrCreate(entryTitle);

        ImGui.SetNextItemWidth(88f);
        ImGui.InputTextWithHint($"##prefix_{entryTitle}", "prefix", ref affixes.Prefix, 128);

        ImGui.SameLine();
        ImGui.TextUnformatted("/");

        ImGui.SameLine();
        ImGui.SetNextItemWidth(88f);
        ImGui.InputTextWithHint($"##suffix_{entryTitle}", "suffix", ref affixes.Suffix, 128);
    }

    private static void DrawMinWidthControls(string layoutKey, string idPrefix)
    {
        var widthEnabled = EntryFixedWidth.IsWidthEnabled(layoutKey);
        if (ImGui.Checkbox($"##width_{idPrefix}", ref widthEnabled))
            EntryFixedWidth.SetWidthEnabled(layoutKey, widthEnabled);

        if (!C.FixedWidthPixels.TryGetValue(layoutKey, out var width))
            width = EntryFixedWidth.DefaultWidthPixels;

        ImGui.SameLine();
        ImGui.BeginDisabled(!widthEnabled);
        ImGui.SetNextItemWidth(52f);
        if (ImGui.DragFloat($"Width##{idPrefix}", ref width, 1f, 1f, 500f, "%.0f") && widthEnabled)
            C.FixedWidthPixels[layoutKey] = width;

        ImGui.EndDisabled();
    }
}
