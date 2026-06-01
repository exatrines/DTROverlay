using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawDefaultStyleSection()
    {
        DtrImGui.SectionHeader("Default Style");

        if (!C.FollowVanillaDtr)
        {
            ImGui.Text("Font Size :");
            ImGuiSettingControls.Indented(() =>
            {
                ImGui.TextUnformatted("Scale");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(72f);
                ImGuiSettingControls.DrawOverlayFontScaleDrag("##defaultOverlayFontSizeScale", ref C.OverlayFontSizeScale);
            });
        }

        DrawDefaultSeparatorWidthSetting();
        DrawDefaultStyleColorTable();
    }

    private static void DrawDefaultSeparatorWidthSetting()
    {
        ImGui.TextUnformatted("Separator width :");
        ImGuiSettingControls.Indented(() =>
        {
            var width = C.SeparatorSlotWidthPx;
            ImGui.SetNextItemWidth(88f);
            if (ImGuiSettingControls.DrawSeparatorSlotWidthDrag("##defaultSeparatorSlotWidth", ref width))
            {
                C.SeparatorSlotWidthPx = width;
                EzConfig.Save();
            }
        });
    }

    private static void DrawDefaultStyleColorTable()
    {
        ImGui.TextUnformatted("Font Colors :");
        ImGuiSettingControls.Indented(() =>
        {
            var defaultGroup = DtrOverlayGroups.GetDefaultGroup();
            var divisionRowEnabled = OverlayStyleKeys.IsDefaultStyleDivisionColorRowEnabled(defaultGroup);

            if (!BeginFontColorStyleTable("##defaultStyleColors"))
                return;

            DrawStyleHierarchyColorRow("Text", OverlayEntryIds.DefaultText, "defaultText");
            DrawStyleHierarchyColorRow("Separator", OverlayEntryIds.DefaultSeparator, "defaultSeparator");
            DrawStyleHierarchyColorRow(
                "Division",
                OverlayEntryIds.DivisionSeparatorColor,
                "divisionSep",
                divisionRowEnabled);

            ImGui.EndTable();
        });
    }

    private static bool BeginFontColorStyleTable(string id) =>
        ImGuiEx.BeginDefaultTable(id, ["Label", "Text", "Edge", "Shadow"]);

    private static void DrawStyleHierarchyColorRow(
        string label,
        string layoutKey,
        string idPrefix,
        bool rowEnabled = true)
    {
        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ImGui.TextUnformatted(label);

        ImGui.TableSetColumnIndex(1);
        OverlayColorPicker.DrawTextColumn(layoutKey, idPrefix, rowEnabled);

        ImGui.TableSetColumnIndex(2);
        OverlayColorPicker.DrawEdgeColumn(layoutKey, idPrefix, rowEnabled);

        ImGui.TableSetColumnIndex(3);
        OverlayColorPicker.DrawShadowColumn(layoutKey, idPrefix, rowEnabled);
    }
}
