using Dalamud.Game.Config;

namespace DTROverlay.Services;

internal readonly record struct DtrClockSettings(
    bool ShowLocal,
    bool ShowEorzea)
{
    public static DtrClockSettings Read() => new(
        IsEnabled(UiConfigOption.TimeLocal),
        IsEnabled(UiConfigOption.TimeEorzea));

    private static bool IsEnabled(UiConfigOption option)
    {
        if (Svc.GameConfig.TryGet(option, out uint value))
            return value != 0;

        return true;
    }
}
