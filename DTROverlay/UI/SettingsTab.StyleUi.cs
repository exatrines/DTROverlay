using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private readonly record struct StyleColorRow(string Label, string LayoutKey, string IdPrefix, bool Enabled = true);

    private static bool UsesFollowVanillaDefaultGroupSettings(DtrOverlayGroup group) =>
        C.FollowNativeDtr && DtrOverlayGroups.IsDefaultOverlay(group);

    private static bool ShouldShowDivisionSeparatorCheckbox(DtrOverlayGroup group) =>
        DtrOverlayGroups.IsDefaultOverlay(group)
        && (C.FollowNativeDtr || !DtrOverlayGroups.IsSplitNativeMode());

    private static void DrawStyleColorRows(IEnumerable<StyleColorRow> rows)
    {
        foreach (var row in rows)
            DrawStyleHierarchyColorRow(row.Label, row.LayoutKey, row.IdPrefix, row.Enabled);
    }

    private static void DrawOverrideStyleColorRows(DtrOverlayGroup group)
    {
        if (UsesFollowVanillaDefaultGroupSettings(group))
        {
            DrawStyleColorRows(
            [
                new(I18n.Get("style.text"), GroupStyleKeys.OverrideText(group.Id), "overrideText"),
                new(I18n.Get("style.separator"), GroupStyleKeys.OverrideSeparator(group.Id), "overrideSeparator"),
                new(
                    I18n.Get("style.division_separator"),
                    OverlayEntryIds.DivisionSeparatorColor,
                    "overrideDivisionSeparator"),
            ]);
            return;
        }

        if (DtrOverlayGroups.IsMergedDefaultPanelOverlay(group))
        {
            var divisionEnabled = OverlayStyleKeys.IsOverrideStyleDivisionColorRowEnabled(group);
            DrawStyleColorRows(
            [
                new(I18n.Get("style.plugin_text"), GroupStyleKeys.OverrideText(group.Id), "overridePluginText"),
                new(I18n.Get("style.plugin_separator"), GroupStyleKeys.OverrideSeparator(group.Id), "overridePluginSeparator"),
                new(I18n.Get("style.native_text"), GroupStyleKeys.OverrideNativeText(group.Id), "overrideNativeText"),
                new(I18n.Get("style.native_separator"), GroupStyleKeys.OverrideNativeSeparator(group.Id), "overrideNativeSeparator"),
                new(
                    I18n.Get("style.division_separator"),
                    GroupStyleKeys.OverrideDivisionSeparator(group.Id),
                    "overrideDivisionSeparator",
                    divisionEnabled),
            ]);
            return;
        }

        DrawStyleColorRows(
        [
            new(I18n.Get("style.text"), GroupStyleKeys.OverrideText(group.Id), "overrideText"),
            new(I18n.Get("style.separator"), GroupStyleKeys.OverrideSeparator(group.Id), "overrideSeparator"),
        ]);
    }
}
