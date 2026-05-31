using ECommons.Automation;

namespace DTROverlay.Services;

internal static class VanillaDtrHud
{
    public static void Toggle() => Chat.ExecuteCommand("/hud dtr");
}
