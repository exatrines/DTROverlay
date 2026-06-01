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

        // このメソッドは毎フレーム PreDraw から呼ばれる。以前は無条件で EzConfig.Save() と
        // OverlayWindowHost.RequestRefresh() を実行していたため、毎フレーム設定の JSON 直列化・
        // ディスク書き込みとウィンドウ再構築が走り FPS が低下していた。
        // 制約適用は冪等なので、実際に値が変化したフレームのみ保存・再描画要求を行う。
        var changed = false;

        foreach (var group in C.OverlayGroups)
        {
            if (group.OverlayEditMode)
            {
                group.OverlayEditMode = false;
                changed = true;
            }

            if (group.OverrideFontSizeScaleEnabled)
            {
                group.OverrideFontSizeScaleEnabled = false;
                changed = true;
            }
        }

        var defaultGroup = DtrOverlayGroups.GetDefaultGroup();
        if (defaultGroup.LayoutMode != OverlayLayoutMode.Horizontal)
        {
            defaultGroup.LayoutMode = OverlayLayoutMode.Horizontal;
            changed = true;
        }

        if (DtrOverlayGroups.ApplyFollowVanillaConstraintsCore())
            changed = true;

        if (!changed)
            return;

        EzConfig.Save();
        OverlayWindowHost.RequestRefresh();
    }
}
