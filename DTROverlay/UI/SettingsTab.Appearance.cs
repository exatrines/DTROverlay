using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawAppearanceSection()
    {
        DtrImGui.SectionHeader("Appearance");

        if (!C.FollowVanillaDtr)
        {
            ImGui.Text("Font Size :");
            ImGuiSettingControls.Indented(() =>
            {
                ImGui.TextUnformatted("Scale");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(72f);
                if (ImGui.DragFloat("##overlayFontSizeScale", ref C.OverlayFontSizeScale, 0.01f, 0.5f, 3f, "%.2f"))
                    DtrOverlayFonts.NotifyScaleChanged();
            });
        }

        DrawAppearanceTable();
    }

    private static void DrawAppearanceTable()
    {
        ImGui.TextUnformatted("Font Colors :");
        ImGuiSettingControls.Indented(() =>
        {
            if (!ImGuiEx.BeginDefaultTable("##appearance", ["", "Label", "Color"]))
                return;

            DrawAppearanceTextRow();
            DrawAppearanceNativeGroupRow();
            DrawAppearanceDivisionSeparatorRow();
            DrawAppearanceSeparatorRow(
                OverlayEntryIds.PluginSeparatorColor,
                "Plugin separator",
                ref C.ShowPluginEntrySeparators);
            DrawAppearanceSeparatorRow(
                OverlayEntryIds.NativeSeparatorColor,
                "Native separator",
                ref C.ShowNativeEntrySeparators);
            ImGui.EndTable();
        });
    }

    private static void DrawAppearanceTextRow()
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Default text");
        ImGui.TableNextColumn();
        OverlayColorPicker.DrawRow(OverlayEntryIds.AppearanceText, "defaultText", rowEnabled: true);
    }

    private static void DrawAppearanceNativeGroupRow()
    {
        var enabled = !C.FollowVanillaDtr && C.ShowServerInfo;

        ImGui.TableNextRow();
        ImGui.BeginDisabled(!enabled);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Native group");
        ImGui.TableNextColumn();
        OverlayColorPicker.DrawRow(OverlayEntryIds.ServerInfoTextGroup, "nativeGroup", enabled);
        ImGui.EndDisabled();

        if (!enabled && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(C.FollowVanillaDtr
                ? "Server info is drawn on the vanilla DTR bar in this mode."
                : "Enable Show Server Info to style native overlay text.");
        }
    }

    private static void DrawAppearanceDivisionSeparatorRow()
    {
        var enabled = !C.FollowVanillaDtr
            && C.OverlayLayoutMode == OverlayLayoutMode.Horizontal
            && C.NativePluginDivision == NativePluginDivisionMode.Separator;

        ImGui.TableNextRow();
        ImGui.BeginDisabled(!enabled);
        ImGui.TableNextColumn();
        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Division separator");
        ImGui.TableNextColumn();
        OverlayColorPicker.DrawRow(OverlayEntryIds.DivisionSeparatorColor, "divisionSep", enabled);
        ImGui.EndDisabled();

        if (!enabled && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip(C.FollowVanillaDtr
                ? "Not used while Follow Vanilla DTR is enabled."
                : C.OverlayLayoutMode != OverlayLayoutMode.Horizontal
                    ? "Only applies in Horizontal layout."
                    : "Set Native / plugins to Separator in Layout to show this divider.");
        }
    }

    private static void DrawAppearanceSeparatorRow(
        string layoutKey,
        string label,
        ref bool visible,
        bool enabled = true)
    {
        ImGui.TableNextRow();
        ImGui.BeginDisabled(!enabled);

        ImGui.TableNextColumn();
        if (ImGui.Checkbox($"##sepVisible_{layoutKey}", ref visible))
            EzConfig.Save();

        ImGui.TableNextColumn();
        ImGui.TextUnformatted(label);

        ImGui.TableNextColumn();
        OverlayColorPicker.DrawRow(layoutKey, $"sep_{layoutKey}", enabled);

        ImGui.EndDisabled();
    }
}
