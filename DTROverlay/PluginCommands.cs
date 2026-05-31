using ECommons.Logging;

namespace DTROverlay;

internal static class PluginCommands
{
    private const string Usage = "/dtroverlay on|off|toggle|edit — Control overlay (no args: open settings)";

    public static void Handle(string command, string args)
    {
        var parts = args.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sub = parts.Length > 0 ? parts[0].ToLowerInvariant() : null;

        switch (sub)
        {
            case "on":
            case "enable":
                SetOverlayEnabled(true);
                return;
            case "off":
            case "disable":
                SetOverlayEnabled(false);
                return;
            case "toggle":
                SetOverlayEnabled(!C.OverlayEnabled);
                return;
            case "edit":
                ToggleEditMode();
                return;
            case null:
            case "":
                EzConfigGui.Open();
                return;
            default:
                DuoLog.Information(Usage);
                return;
        }
    }

    private static void SetOverlayEnabled(bool enabled)
    {
        if (C.OverlayEnabled == enabled)
        {
            DuoLog.Information($"DTR Overlay is already {(enabled ? "enabled" : "disabled")}.");
            return;
        }

        C.OverlayEnabled = enabled;
        EzConfig.Save();
        DuoLog.Information($"DTR Overlay {(enabled ? "enabled" : "disabled")}.");
    }

    private static void ToggleEditMode()
    {
        if (C.FollowVanillaDtr)
        {
            DuoLog.Information("Edit mode is unavailable while Follow Vanilla DTR is enabled.");
            return;
        }

        C.OverlayEditMode = !C.OverlayEditMode;
        if (!C.OverlayEditMode)
            EzConfig.Save();

        DuoLog.Information($"DTR Overlay edit mode {(C.OverlayEditMode ? "enabled" : "disabled")}.");
    }
}
