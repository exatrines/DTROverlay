using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawTooltipSection()
    {
        DtrImGui.SectionHeader("Tooltip");

        ImGuiSettingControls.LabeledIndented("Position :", () =>
        {
            var selected = (int)C.TooltipPosition;
            if (ImGui.RadioButton("Follow cursor", ref selected, (int)TooltipPosition.FollowCursor))
                selected = (int)TooltipPosition.FollowCursor;

            ImGui.SameLine();
            if (ImGui.RadioButton("Upper", ref selected, (int)TooltipPosition.Upper))
                selected = (int)TooltipPosition.Upper;

            ImGui.SameLine();
            if (ImGui.RadioButton("Lower", ref selected, (int)TooltipPosition.Lower))
                selected = (int)TooltipPosition.Lower;

            if (selected != (int)C.TooltipPosition)
            {
                C.TooltipPosition = (TooltipPosition)selected;
                EzConfig.Save();
            }
        });

        ImGui.TextUnformatted("Font Size :");
        ImGuiSettingControls.Indented(() =>
        {
            ImGui.TextUnformatted("Size (px)");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(72f);
            if (ImGui.DragFloat("##tooltipFontSizePx", ref C.TooltipFontSizePx, 0.5f, 8f, 48f, "%.0f"))
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
            if (ImGui.ColorEdit4("##tooltipTextColor", ref C.TooltipTextColor, DtrStyle.ColorEditFlags))
                EzConfig.Save();

            ImGui.TextUnformatted("Background");
            ImGui.SameLine();
            if (ImGui.ColorEdit4("##tooltipBackgroundColor", ref C.TooltipBackgroundColor, DtrStyle.ColorEditFlags))
                EzConfig.Save();
        });
    }
}
