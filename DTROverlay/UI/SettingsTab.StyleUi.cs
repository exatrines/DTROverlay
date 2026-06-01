using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private readonly record struct StyleColorRow(string Label, string LayoutKey, string IdPrefix, bool Enabled = true);

    private static bool UsesFollowVanillaDefaultGroupSettings(DtrOverlayGroup group) =>
        C.FollowVanillaDtr && DtrOverlayGroups.IsDefaultGroup(group);

    private static bool ShouldShowDivisionSeparatorCheckbox(DtrOverlayGroup group) =>
        DtrOverlayGroups.IsDefaultGroup(group)
        && (C.FollowVanillaDtr || !DtrOverlayGroups.IsSplitNativeMode());

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
                new("Text", GroupStyleKeys.OverrideText(group.Id), "overrideText"),
                new("Separator", GroupStyleKeys.OverrideSeparator(group.Id), "overrideSeparator"),
                new(
                    "Division separator",
                    OverlayEntryIds.DivisionSeparatorColor,
                    "overrideDivisionSeparator"),
            ]);
            return;
        }

        if (DtrOverlayGroups.IsMergedDefaultPanelGroup(group))
        {
            var divisionEnabled = OverlayStyleKeys.IsOverrideStyleDivisionColorRowEnabled(group);
            DrawStyleColorRows(
            [
                new("Plugin text", GroupStyleKeys.OverrideText(group.Id), "overridePluginText"),
                new("Plugin separator", GroupStyleKeys.OverrideSeparator(group.Id), "overridePluginSeparator"),
                new("Native text", GroupStyleKeys.OverrideNativeText(group.Id), "overrideNativeText"),
                new("Native separator", GroupStyleKeys.OverrideNativeSeparator(group.Id), "overrideNativeSeparator"),
                new(
                    "Division separator",
                    GroupStyleKeys.OverrideDivisionSeparator(group.Id),
                    "overrideDivisionSeparator",
                    divisionEnabled),
            ]);
            return;
        }

        DrawStyleColorRows(
        [
            new("Text", GroupStyleKeys.OverrideText(group.Id), "overrideText"),
            new("Separator", GroupStyleKeys.OverrideSeparator(group.Id), "overrideSeparator"),
        ]);
    }
}
