using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawShortcutsSection()
    {
        MirageUi.SubHeader("Shortcuts");

        if (MirageUi.SecondaryButton("Toggle vanilla DTR bar", id: "toggleVanillaDtr"))
            VanillaDtrHud.Toggle();
        MirageUi.Tooltip(
            "Runs /hud dtr to show or hide the game's Server Info Bar. "
            + "Avoid while Follow native DTR is selected.");

        if (MirageUi.SecondaryButton("Uncheck all in Dalamud DTR settings", id: "hideDalamudDtr"))
            DtrDalamudConfig.HideAllEntriesInDalamudSettings();
        MirageUi.Tooltip(
            "Disables every plugin DTR entry in Dalamud's general settings "
            + "(XIVLauncher → Dalamud Settings → Server Info Bar). "
            + "Use this so only the overlay shows plugin DTR text.");
    }

    private static void DrawOptionSection()
    {
        MirageUi.SubHeader("Option");
        if (MirageUi.Checkbox("Open plugin UI on middle-click", ref C.OpenPluginUiOnMiddleClick))
            C.Save();
    }
}
