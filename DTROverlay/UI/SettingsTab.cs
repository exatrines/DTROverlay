namespace DTROverlay.UI;

public static partial class SettingsTab
{
    public static void Draw()
    {
        ImGui.Checkbox("Enable", ref C.OverlayEnabled);
        ImGui.Spacing();

        DrawShortcutsSection();
        DrawLayoutSection();
        DrawAppearanceSection();

        if (!C.FollowVanillaDtr)
        {
            DrawPositionSection();
            DrawServerInfoSection();
        }

        DrawEntriesSection();
        DrawTooltipSection();
        DrawOptionSection();
    }
}
