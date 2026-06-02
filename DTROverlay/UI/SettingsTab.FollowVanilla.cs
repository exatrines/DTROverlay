using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawFollowVanillaSection()
    {
        DtrImGui.SectionHeader("Follow Vanilla DTR");

        if (ImGui.Checkbox("Follow Vanilla DTR", ref C.FollowVanillaDtr))
        {
            DtrOverlayGroups.SyncDefaultGroupDisplayName();
            DtrOverlayGroups.ApplyFollowVanillaConstraints();
            OverlayWindowHost.RequestRefresh();
            EzConfig.Save();
        }

        if (!C.FollowVanillaDtr)
            return;

        FollowVanillaDtrMode.EnforceLayoutConstraints();
        DrawFollowVanillaLayoutSettings();
        DrawHorizontalPluginFlow(DtrOverlayGroups.GetDefaultGroup());
    }
}
