using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawDefaultTooltipSection()
    {
        MirageUi.SubHeader(I18n.Get("style.default_tooltip"));

        var position = C.TooltipPosition;
        if (DrawEnumDropdown(I18n.Get("style.position"), ref position, TooltipPositionLabels(), "defaultTooltipPosition"))
        {
            C.TooltipPosition = position;
            C.Save();
        }

        if (MirageUi.SliderFloat(I18n.Get("style.font_px"), ref C.TooltipFontSizePx, 8f, 48f, "%.0f", "defaultTooltipFontSizePx"))
        {
            DtrOverlayFonts.NotifyTooltipSizeChanged();
            C.Save();
        }

        if (MirageUi.ColorEdit4(I18n.Get("style.text"), ref C.TooltipTextColor, DtrStyle.ColorEditFlags, "defaultTooltipTextColor"))
            C.Save();

        if (MirageUi.ColorEdit4(I18n.Get("style.background"), ref C.TooltipBackgroundColor, DtrStyle.ColorEditFlags, "defaultTooltipBackgroundColor"))
            C.Save();
    }
}
