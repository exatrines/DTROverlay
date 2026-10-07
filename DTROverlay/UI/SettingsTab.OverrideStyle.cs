using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawOverrideStyleSection(DtrOverlayGroup group)
    {
        MirageUi.SubHeader(I18n.Get("style.override"));

        DrawOverrideField(
            I18n.Get("layout.font_scale"),
            $"overrideFontSizeScale_{group.Id}",
            ref group.OverrideFontSizeScaleEnabled,
            width => DrawOverlayFontScale(
                string.Empty,
                ref group.OverrideFontSizeScale,
                $"overrideFontSizeScale_{group.Id}",
                width),
            DtrOverlayFonts.NotifyScaleChanged);

        DrawOverrideField(
            I18n.Get("style.separator_width"),
            $"overrideSeparatorWidth_{group.Id}",
            ref group.OverrideSeparatorSlotWidthPxEnabled,
            width =>
            {
                var separatorWidth = group.OverrideSeparatorSlotWidthPx;
                if (!MirageUi.SliderInt(
                        string.Empty,
                        ref separatorWidth,
                        0,
                        OverlaySlotWidthSettings.MaxWidth,
                        $"overrideSeparatorWidth_{group.Id}",
                        width))
                    return;

                group.OverrideSeparatorSlotWidthPx = separatorWidth;
                C.Save();
            });

        MirageUi.Text(I18n.Get("style.colors"), MirageUi.Color.Secondary);

        if (!BeginFontColorStyleTable("##overrideStyleColors"))
            return;

        DrawOverrideStyleColorRows(group);
        ImGui.EndTable();
    }
}
