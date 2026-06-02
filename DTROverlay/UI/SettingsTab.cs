namespace DTROverlay.UI;

public static partial class SettingsTab
{
    public static void Draw()
    {
        if (ImGui.Checkbox("Enable", ref C.OverlayEnabled))
            EzConfig.Save();
        ImGui.Spacing();

        DrawShortcutsSection();
        DrawFollowVanillaSection();
        DrawOptionSection();
        ImGui.Spacing();
        DrawSettingsSubTabs();
    }
}
