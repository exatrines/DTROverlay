using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawDefaultStyleSection()
    {
        MirageUi.SubHeader("Default Style");

        using (MirageUi.DisabledIf(C.FollowNativeDtr))
            DrawOverlayFontScale("Font Size Scale", ref C.OverlayFontSizeScale, "defaultOverlayFontSizeScale");
        if (C.FollowNativeDtr)
            TooltipWhenDisabled("Follow native DTR uses its own Font Size Scale.");

        DrawDefaultSeparatorWidthSetting();
        DrawDefaultStyleColorTable();
    }

    private static void DrawDefaultSeparatorWidthSetting()
    {
        var width = C.SeparatorSlotWidthPx;
        if (!MirageUi.SliderInt("Separator width", ref width, 0, OverlaySlotWidthSettings.MaxWidth, "defaultSeparatorSlotWidth"))
            return;

        C.SeparatorSlotWidthPx = width;
        C.Save();
    }

    private static void DrawDefaultStyleColorTable()
    {
        MirageUi.Text("Font Colors", MirageUi.Color.Secondary);

        var defaultGroup = DtrOverlayGroups.GetDefaultOverlay();
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
    }

    private static bool BeginFontColorStyleTable(string id) =>
        SettingsTables.BeginDefaultTable(id, ["Label", "Text", "Edge", "Shadow"]);

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
