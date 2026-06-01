using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawDefaultTooltipSection()
    {
        DtrImGui.SectionHeader("Default Tooltip");

        ImGuiSettingControls.LabeledIndented("Position :", () =>
        {
            var position = C.TooltipPosition;
            if (ImGuiSettingControls.DrawTooltipPositionRadios(ref position))
            {
                C.TooltipPosition = position;
                EzConfig.Save();
            }
        });

        ImGui.TextUnformatted("Font Size :");
        ImGuiSettingControls.Indented(() =>
        {
            ImGui.TextUnformatted("Size (px)");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(72f);
            if (ImGui.DragFloat("##defaultTooltipFontSizePx", ref C.TooltipFontSizePx, 0.5f, 8f, 48f, "%.0f"))
            {
                DtrOverlayFonts.NotifyTooltipSizeChanged();
                EzConfig.Save();
            }
        });

        ImGui.TextUnformatted("Colors :");
        ImGuiSettingControls.Indented(() =>
        {
            ImGui.TextUnformatted("Text");
            ImGui.SameLine();
            if (ImGui.ColorEdit4("##defaultTooltipTextColor", ref C.TooltipTextColor, DtrStyle.ColorEditFlags))
                EzConfig.Save();

            ImGui.TextUnformatted("Background");
            ImGui.SameLine();
            if (ImGui.ColorEdit4("##defaultTooltipBackgroundColor", ref C.TooltipBackgroundColor, DtrStyle.ColorEditFlags))
                EzConfig.Save();
        });
    }
}
