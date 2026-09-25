using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawGroupTooltipSection(DtrOverlayGroup group)
    {
        MirageUi.SubHeader("Tooltip");

        DrawOverrideField(
            "Position",
            $"groupTooltipPos_{group.Id}",
            ref group.OverrideTooltipPositionEnabled,
            width =>
            {
                var position = group.OverrideTooltipPosition;
                if (!DrawEnumDropdown(
                        string.Empty,
                        ref position,
                        TooltipPositionLabels,
                        $"groupTooltipPos_{group.Id}",
                        width))
                    return;

                group.OverrideTooltipPosition = position;
                C.Save();
            });

        DrawOverrideField(
            "Font Size (px)",
            $"groupTooltipFontSize_{group.Id}",
            ref group.OverrideTooltipFontSizePxEnabled,
            width =>
            {
                if (!MirageUi.SliderFloat(
                        string.Empty,
                        ref group.OverrideTooltipFontSizePx,
                        8f,
                        48f,
                        "%.0f",
                        $"groupTooltipFontSize_{group.Id}",
                        width))
                    return;

                DtrOverlayFonts.NotifyTooltipSizeChanged();
                C.Save();
            });

        DrawOverrideField(
            "Text",
            $"groupTooltipText_{group.Id}",
            ref group.OverrideTooltipTextColorEnabled,
            width =>
            {
                if (MirageUi.ColorEdit4(
                        string.Empty,
                        ref group.OverrideTooltipTextColor,
                        DtrStyle.ColorEditFlags,
                        $"groupTooltipText_{group.Id}",
                        width))
                    C.Save();
            });

        DrawOverrideField(
            "Background",
            $"groupTooltipBg_{group.Id}",
            ref group.OverrideTooltipBackgroundColorEnabled,
            width =>
            {
                if (MirageUi.ColorEdit4(
                        string.Empty,
                        ref group.OverrideTooltipBackgroundColor,
                        DtrStyle.ColorEditFlags,
                        $"groupTooltipBg_{group.Id}",
                        width))
                    C.Save();
            });
    }
}
