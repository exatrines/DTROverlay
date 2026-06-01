namespace DTROverlay.Services;

/// <summary>Centralizes layout-key selection for native/division styling (settings UI + runtime).</summary>
internal static class OverlayStyleKeys
{
    public static string GetNativeTextColorLayoutKey() =>
        DtrOverlayGroups.IsMergedDefaultMode()
            ? GroupStyleKeys.OverrideNativeText(DtrOverlayGroups.GetDefaultGroup().Id)
            : GroupStyleKeys.OverrideText(DtrOverlayGroups.GetNativeGroup().Id);

    public static string GetDivisionSeparatorLayoutKey(DtrOverlayGroup group)
    {
        if (C.FollowVanillaDtr && DtrOverlayGroups.IsDefaultGroup(group))
            return OverlayEntryIds.DivisionSeparatorColor;

        if (DtrOverlayGroups.IsMergedDefaultPanelGroup(group))
            return GroupStyleKeys.OverrideDivisionSeparator(group.Id);

        return OverlayEntryIds.DivisionSeparatorColor;
    }

    public static bool IsDefaultStyleDivisionColorRowEnabled(DtrOverlayGroup group) =>
        C.FollowVanillaDtr
        || (group.LayoutMode == OverlayLayoutMode.Horizontal && group.ShowDivisionSeparatorBar);

    public static bool IsOverrideStyleDivisionColorRowEnabled(DtrOverlayGroup group) =>
        group.LayoutMode == OverlayLayoutMode.Horizontal;
}
