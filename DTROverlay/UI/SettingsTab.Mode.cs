using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    public static void DrawModePage()
    {
        MirageUi.SubHeader(I18n.Get("page.mode"));

        var followNative = C.FollowNativeDtr;
        if (ImGui.RadioButton(I18n.Get("mode.follow"), followNative))
            SetFollowNativeMode(true);
        if (ImGui.RadioButton(I18n.Get("mode.manual"), !followNative))
            SetFollowNativeMode(false);

        if (followNative)
        {
            FollowNativeDtrMode.EnforceLayoutConstraints();
            MirageUi.SubHeader(I18n.Get("mode.follow_settings"));
            DrawFollowNativeLayoutSettings();
            DrawHorizontalPluginFlow(DtrOverlayGroups.GetDefaultOverlay());
            return;
        }

        MirageUi.SubHeader(I18n.Get("mode.manual_settings"));
        if (MirageUi.Checkbox(I18n.Get("mode.split"), ref C.SplitNativeDtr))
        {
            DtrOverlayGroups.SyncDefaultOverlayDisplayName();

            if (!C.SplitNativeDtr && DtrOverlayGroups.IsNativeOverlay(DtrOverlayGroups.GetSelected()))
                DtrOverlayGroups.Select(DtrOverlayGroups.GetDefaultOverlay().Id);

            OverlayWindowHost.RequestRefresh();
            C.Save();
        }
    }

    private static void SetFollowNativeMode(bool followNative)
    {
        if (C.FollowNativeDtr == followNative)
            return;

        C.FollowNativeDtr = followNative;
        DtrOverlayGroups.SyncDefaultOverlayDisplayName();
        DtrOverlayGroups.ApplyFollowNativeConstraints();
        OverlayWindowHost.RequestRefresh();
        C.Save();
    }
}
