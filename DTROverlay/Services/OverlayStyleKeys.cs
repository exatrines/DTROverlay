namespace DTROverlay.Services;

/// <summary>Centralizes layout-key selection for native/division styling (settings UI + runtime).</summary>
internal static class OverlayStyleKeys
{
    public static string GetNativeTextColorLayoutKey() =>
        DtrOverlayGroups.IsMergedDefaultMode()
            ? GroupStyleKeys.OverrideNativeText(DtrOverlayGroups.GetDefaultOverlay().Id)
            : GroupStyleKeys.OverrideText(DtrOverlayGroups.GetNativeOverlay().Id);

    public static string GetDivisionSeparatorLayoutKey(DtrOverlayGroup group)
    {
        if (C.FollowNativeDtr && DtrOverlayGroups.IsDefaultOverlay(group))
            return OverlayEntryIds.DivisionSeparatorColor;

        if (DtrOverlayGroups.IsMergedDefaultPanelOverlay(group))
            return GroupStyleKeys.OverrideDivisionSeparator(group.Id);

        return OverlayEntryIds.DivisionSeparatorColor;
    }

    public static bool IsDefaultStyleDivisionColorRowEnabled(DtrOverlayGroup group) =>
        C.FollowNativeDtr
        || (group.LayoutMode == OverlayLayoutMode.Horizontal && group.ShowDivisionSeparatorBar);

    public static bool IsOverrideStyleDivisionColorRowEnabled(DtrOverlayGroup group) =>
        group.LayoutMode == OverlayLayoutMode.Horizontal;
}
