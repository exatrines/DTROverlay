using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    public static void DrawModePage()
    {
        MirageUi.SubHeader("Mode");

        var followNative = C.FollowNativeDtr;
        if (ImGui.RadioButton("Follow native DTR", followNative))
            SetFollowNativeMode(true);
        if (ImGui.RadioButton("Manual", !followNative))
            SetFollowNativeMode(false);

        if (followNative)
        {
            FollowNativeDtrMode.EnforceLayoutConstraints();
            MirageUi.SubHeader("Follow native DTR settings");
            DrawFollowNativeLayoutSettings();
            DrawHorizontalPluginFlow(DtrOverlayGroups.GetDefaultOverlay());
            return;
        }

        MirageUi.SubHeader("Manual settings");
        if (MirageUi.Checkbox("Split Native DTR", ref C.SplitNativeDtr))
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
