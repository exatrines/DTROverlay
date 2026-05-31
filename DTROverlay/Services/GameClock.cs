using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace DTROverlay.Services;

internal static unsafe class GameClock
{
    public static DateTime GetEorzeaTime()
    {
        var framework = Framework.Instance();
        if (framework != null)
        {
            var clientTime = framework->ClientTime;
            if (TryFromClientTime(clientTime.EorzeaTime, out var fromClientTime))
                return fromClientTime;

            if (clientTime.EorzeaTimeMilliseconds > 0
                && TryFromClientTime((long)clientTime.EorzeaTimeMilliseconds / 1000, out fromClientTime))
                return fromClientTime;
        }

        return ComputeEorzeaFromUtc(DateTime.UtcNow);
    }

    public static DateTime GetLocalTime() => DateTime.Now;

    public static string FormatTime(DateTime time) => $"{time.Hour}:{time.Minute:D2}";

    private static bool TryFromClientTime(long eorzeaSeconds, out DateTime time)
    {
        time = default;
        if (eorzeaSeconds <= 0)
            return false;

        var secondsOfDay = eorzeaSeconds % 86400;
        if (secondsOfDay < 0)
            secondsOfDay += 86400;

        var hour = (int)(secondsOfDay / 3600);
        var minute = (int)(secondsOfDay % 3600 / 60);
        time = new DateTime(1, 1, 1, hour, minute, 0);
        return true;
    }

    private static DateTime ComputeEorzeaFromUtc(DateTime utcNow)
    {
        const double multiplier = 3600d / 175d;
        var seconds = (utcNow - DateTime.UnixEpoch).TotalSeconds;
        return DateTime.UnixEpoch.AddSeconds(seconds * multiplier);
    }
}
