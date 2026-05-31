using ECommons.Logging;
using ECommons.SimpleGui;

namespace DTROverlay;

internal static class PluginCommands
{
    private const string Usage = "/dtroverlay — toggle settings UI. Subcommands: on|off|toggle";

    public static void Handle(string command, string args)
    {
        args = args?.Trim() ?? "";
        if (args.Length == 0)
        {
            EzConfigGui.Toggle();
            return;
        }

        var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var sub = parts[0].ToLowerInvariant();

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
}
