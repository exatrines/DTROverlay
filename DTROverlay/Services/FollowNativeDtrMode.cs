namespace DTROverlay.Services;

internal static class FollowNativeDtrMode
{
    public static bool IsActive => C.FollowNativeDtr;

    public static bool AppliesTo(DtrOverlayGroup group) =>
        IsActive && group != null && DtrOverlayGroups.IsDefaultOverlay(group);

    public static bool IsVanillaDtrVisible => DtrVanillaBounds.IsAddonVisible();

    /// <summary>
    /// Inserts the division separator at the start of the plugin list when true, or at the end when false.
    /// Left side: overlay sits left of vanilla DTR — division on the right of the default group.
    /// Right side: overlay sits right of vanilla — division on the left of the group.
    /// LTR vs RTL chooses prepend vs append on that vanilla-adjacent edge.
    /// </summary>
    public static bool ShouldPrependDivisionSeparatorToPluginList()
    {
        var overlayOnLeftOfVanilla = C.FollowNativeDtrSide == FollowNativeDtrSide.Left;
        var pluginsFlowLeftToRight = OverlayPluginFlow.UseHorizontalLeftToRight(DtrOverlayGroups.GetDefaultOverlay());
        return overlayOnLeftOfVanilla != pluginsFlowLeftToRight;
    }

    public static void EnforceLayoutConstraints()
    {
        if (!C.FollowNativeDtr)
            return;

        if (C.Overlays == null)
            return;

        // このメソッドは毎フレーム PreDraw から呼ばれる。以前は無条件で C.Save() と
        // OverlayWindowHost.RequestRefresh() を実行していたため、毎フレーム設定の JSON 直列化・
        // ディスク書き込みとウィンドウ再構築が走り FPS が低下していた。
        // 制約適用は冪等なので、実際に値が変化したフレームのみ保存・再描画要求を行う。
        var changed = false;

        var defaultGroup = DtrOverlayGroups.GetDefaultOverlay();
        if (defaultGroup.OverlayEditMode)
        {
            defaultGroup.OverlayEditMode = false;
            changed = true;
        }

        if (defaultGroup.LayoutMode != OverlayLayoutMode.Horizontal)
        {
            defaultGroup.LayoutMode = OverlayLayoutMode.Horizontal;
            changed = true;
        }

        if (DtrOverlayGroups.ApplyFollowNativeConstraintsCore())
            changed = true;

        if (!changed)
            return;

        C.Save();
        OverlayWindowHost.RequestRefresh();
    }
}
