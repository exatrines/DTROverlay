using Dalamud.Game.Text;

namespace DTROverlay.Services;

internal static class DtrTimeIcons
{
    public static SeIconChar LocalTime => SeIconChar.LocalTimeEn;

    public static SeIconChar EorzeaTime => SeIconChar.EorzeaTimeEn;

    public static bool IsTimeIcon(string text)
    {
        if (text.Length != 1)
            return false;

        return text[0] is >= (char)SeIconChar.LocalTimeEn and <= (char)SeIconChar.EorzeaTimeJa;
    }
}
