using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawDefaultTooltipSection()
    {
        MirageUi.SubHeader("Default Tooltip");

        var position = C.TooltipPosition;
        if (DrawEnumDropdown("Position", ref position, TooltipPositionLabels, "defaultTooltipPosition"))
        {
            C.TooltipPosition = position;
            C.Save();
        }

        if (MirageUi.SliderFloat("Font Size (px)", ref C.TooltipFontSizePx, 8f, 48f, "%.0f", "defaultTooltipFontSizePx"))
        {
            DtrOverlayFonts.NotifyTooltipSizeChanged();
            C.Save();
        }

        if (MirageUi.ColorEdit4("Text", ref C.TooltipTextColor, DtrStyle.ColorEditFlags, "defaultTooltipTextColor"))
            C.Save();

        if (MirageUi.ColorEdit4("Background", ref C.TooltipBackgroundColor, DtrStyle.ColorEditFlags, "defaultTooltipBackgroundColor"))
            C.Save();
    }
}
