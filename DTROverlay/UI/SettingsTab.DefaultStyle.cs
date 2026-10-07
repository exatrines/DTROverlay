using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawDefaultStyleSection()
    {
        MirageUi.SubHeader(I18n.Get("style.default"));

        using (MirageUi.DisabledIf(C.FollowNativeDtr))
            DrawOverlayFontScale(I18n.Get("layout.font_scale"), ref C.OverlayFontSizeScale, "defaultOverlayFontSizeScale");
        if (C.FollowNativeDtr)
            TooltipWhenDisabled(I18n.Get("layout.font_scale_follow"));

        DrawDefaultSeparatorWidthSetting();
        DrawDefaultStyleColorTable();
    }

    private static void DrawDefaultSeparatorWidthSetting()
    {
        var width = C.SeparatorSlotWidthPx;
        if (!MirageUi.SliderInt(I18n.Get("style.separator_width"), ref width, 0, OverlaySlotWidthSettings.MaxWidth, "defaultSeparatorSlotWidth"))
            return;

        C.SeparatorSlotWidthPx = width;
        C.Save();
    }

    private static void DrawDefaultStyleColorTable()
    {
        MirageUi.Text(I18n.Get("style.colors"), MirageUi.Color.Secondary);

        var defaultGroup = DtrOverlayGroups.GetDefaultOverlay();
        var divisionRowEnabled = OverlayStyleKeys.IsDefaultStyleDivisionColorRowEnabled(defaultGroup);

        if (!BeginFontColorStyleTable("##defaultStyleColors"))
            return;

        DrawStyleHierarchyColorRow(I18n.Get("style.text"), OverlayEntryIds.DefaultText, "defaultText");
        DrawStyleHierarchyColorRow(I18n.Get("style.separator"), OverlayEntryIds.DefaultSeparator, "defaultSeparator");
        DrawStyleHierarchyColorRow(
            I18n.Get("style.division"),
            OverlayEntryIds.DivisionSeparatorColor,
            "divisionSep",
            divisionRowEnabled);

        ImGui.EndTable();
    }

    private static bool BeginFontColorStyleTable(string id) =>
        SettingsTables.BeginDefaultTable(id, [
            I18n.Get("style.column.label"),
            I18n.Get("style.column.text"),
            I18n.Get("style.column.edge"),
            I18n.Get("style.column.shadow"),
        ]);

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
