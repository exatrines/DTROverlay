using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawShortcutsSection()
    {
        DtrImGui.SectionHeader("Shortcuts");

        if (ImGui.Button("Toggle vanilla DTR bar"))
            VanillaDtrHud.Toggle();

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Runs /hud dtr to show or hide the game's Server Info Bar. "
                + "Avoid while Follow Vanilla DTR is enabled.");
        }

        ImGui.SameLine();
        if (ImGui.Button("Uncheck all in Dalamud DTR settings"))
            DtrDalamudConfig.HideAllEntriesInDalamudSettings();

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(
                "Disables every plugin DTR entry in Dalamud's general settings "
                + "(XIVLauncher → Dalamud Settings → Server Info Bar). "
                + "Use this so only the overlay shows plugin DTR text.");
        }
    }

    private static void DrawOptionSection()
    {
        DtrImGui.SectionHeader("Option");
        if (ImGui.Checkbox("Open plugin UI on middle-click", ref C.OpenPluginUiOnMiddleClick))
            EzConfig.Save();
    }
}
