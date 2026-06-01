using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawGroupTooltipSection(DtrOverlayGroup group)
    {
        DtrImGui.SectionHeader("Tooltip");

        ImGuiSettingControls.LabeledIndented("Position :", () =>
        {
            ImGui.BeginDisabled(!group.OverrideTooltipPositionEnabled);
            var position = group.OverrideTooltipPosition;
            if (ImGuiSettingControls.DrawTooltipPositionRadios(ref position))
            {
                group.OverrideTooltipPosition = position;
                EzConfig.Save();
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Checkbox("Override##tooltipPos", ref group.OverrideTooltipPositionEnabled))
                EzConfig.Save();
        });

        ImGui.TextUnformatted("Font Size :");
        ImGuiSettingControls.Indented(() =>
        {
            ImGui.BeginDisabled(!group.OverrideTooltipFontSizePxEnabled);
            ImGui.SetNextItemWidth(72f);
            if (ImGui.DragFloat("##groupTooltipFontSize", ref group.OverrideTooltipFontSizePx, 0.5f, 8f, 48f, "%.0f"))
            {
                DtrOverlayFonts.NotifyTooltipSizeChanged();
                EzConfig.Save();
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Checkbox("Override##tooltipSize", ref group.OverrideTooltipFontSizePxEnabled))
                EzConfig.Save();
        });

        ImGui.TextUnformatted("Colors :");
        ImGuiSettingControls.Indented(() =>
        {
            ImGui.BeginDisabled(!group.OverrideTooltipTextColorEnabled);
            ImGui.TextUnformatted("Text");
            ImGui.SameLine();
            if (ImGui.ColorEdit4("##groupTooltipText", ref group.OverrideTooltipTextColor, DtrStyle.ColorEditFlags))
                EzConfig.Save();
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Checkbox("Override##tooltipText", ref group.OverrideTooltipTextColorEnabled))
                EzConfig.Save();

            ImGui.BeginDisabled(!group.OverrideTooltipBackgroundColorEnabled);
            ImGui.TextUnformatted("Background");
            ImGui.SameLine();
            if (ImGui.ColorEdit4("##groupTooltipBg", ref group.OverrideTooltipBackgroundColor, DtrStyle.ColorEditFlags))
                EzConfig.Save();
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Checkbox("Override##tooltipBg", ref group.OverrideTooltipBackgroundColorEnabled))
                EzConfig.Save();
        });
    }
}
