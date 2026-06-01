using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawSettingsSubTabs()
    {
        DtrOverlayGroups.EnsureInitialized();

        if (!ImGui.BeginTabBar("##settingsStyleGroupTabs"))
            return;

        if (ImGui.BeginTabItem("Default Style"))
        {
            DrawDefaultStyleSection();
            ImGui.Spacing();
            DrawDefaultTooltipSection();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Group"))
        {
            DrawGroupTabSection();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }
}
