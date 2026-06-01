namespace DTROverlay.Services;

internal static class FollowVanillaDtrMode
{
    public static bool IsActive => C.FollowVanillaDtr && C.OverlayEnabled;

    public static bool IsVanillaDtrVisible => DtrVanillaBounds.IsAddonVisible();

    public static bool ShouldRenderOverlay => IsActive && IsVanillaDtrVisible;

    /// <summary>
    /// Inserts the division separator at the start of the plugin list when true, or at the end when false.
    /// Left side: overlay sits left of vanilla DTR — division on the right of the default group.
    /// Right side: overlay sits right of vanilla — division on the left of the group.
    /// LTR vs RTL chooses prepend vs append on that vanilla-adjacent edge.
    /// </summary>
    public static bool ShouldPrependDivisionSeparatorToPluginList()
    {
        var overlayOnLeftOfVanilla = C.FollowVanillaDtrSide == FollowVanillaDtrSide.Left;
        var pluginsFlowLeftToRight = OverlayPluginFlow.UseHorizontalLeftToRight(DtrOverlayGroups.GetDefaultGroup());
        return overlayOnLeftOfVanilla != pluginsFlowLeftToRight;
    }

    public static void EnforceLayoutConstraints()
    {
        if (!C.FollowVanillaDtr)
            return;

        if (C.OverlayGroups == null)
            return;

        foreach (var group in C.OverlayGroups)
        {
            group.OverlayEditMode = false;
            group.OverrideFontSizeScaleEnabled = false;
        }

        DtrOverlayGroups.GetDefaultGroup().LayoutMode = OverlayLayoutMode.Horizontal;

        DtrOverlayGroups.ApplyFollowVanillaConstraints();
    }
}
