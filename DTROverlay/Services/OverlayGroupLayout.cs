namespace DTROverlay.Services;

internal static class OverlayGroupLayout
{
    public static DtrOverlayGroup Resolve(DtrOverlayGroup group = null) =>
        group ?? OverlayStyleContext.Group ?? DtrOverlayGroups.GetDefaultGroup();

    public static OverlayLayoutMode GetLayoutMode(DtrOverlayGroup group = null) =>
        Resolve(group).LayoutMode;

    public static OverlayHorizontalFlow GetHorizontalPluginFlow(DtrOverlayGroup group = null) =>
        Resolve(group).HorizontalPluginFlow;

    public static OverlayVerticalFlow GetVerticalPluginFlow(DtrOverlayGroup group = null) =>
        Resolve(group).VerticalPluginFlow;

    public static OverlayVerticalAlignment GetVerticalAlignment(DtrOverlayGroup group = null) =>
        Resolve(group).VerticalAlignment;

    public static bool UsesNativePluginDivisionSettings(DtrOverlayGroup group = null)
    {
        var resolved = Resolve(group);
        return DtrOverlayGroups.IsDefaultGroup(resolved) && !DtrOverlayGroups.IsSplitNativeMode();
    }

    public static bool ShouldInsertDivisionSeparator(bool hasNativeOverlayEntries, bool hasPluginOverlayEntries) =>
        UsesNativePluginDivisionSettings()
        && GetLayoutMode() == OverlayLayoutMode.Horizontal
        && hasPluginOverlayEntries
        && (hasNativeOverlayEntries || FollowVanillaDtrMode.IsActive);

    public static void CopyLayoutFrom(DtrOverlayGroup target, DtrOverlayGroup source)
    {
        target.LayoutMode = source.LayoutMode;
        target.HorizontalPluginFlow = source.HorizontalPluginFlow;
        target.VerticalPluginFlow = source.VerticalPluginFlow;
        target.VerticalAlignment = source.VerticalAlignment;
        target.NativePluginDivision = source.NativePluginDivision;
    }

    public static void CopyLayoutFromConfiguration(DtrOverlayGroup target)
    {
        target.LayoutMode = C.OverlayLayoutMode;
        target.HorizontalPluginFlow = C.HorizontalPluginFlow;
        target.VerticalPluginFlow = C.VerticalPluginFlow;
        target.VerticalAlignment = C.OverlayVerticalAlignment;
        target.NativePluginDivision = C.NativePluginDivision;
    }
}
