using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawOverrideStyleSection(DtrOverlayGroup group)
    {
        DtrImGui.SectionHeader("Override Style");

        if (!UsesFollowVanillaDefaultGroupSettings(group))
        {
            ImGui.Text("Font Size :");
            ImGuiSettingControls.Indented(() =>
            {
                ImGui.BeginDisabled(C.FollowVanillaDtr);
                if (ImGui.Checkbox("Override##fontScale", ref group.OverrideFontSizeScaleEnabled))
                {
                    DtrOverlayFonts.NotifyScaleChanged();
                    EzConfig.Save();
                }
                ImGui.SameLine();
                ImGui.BeginDisabled(!group.OverrideFontSizeScaleEnabled);
                ImGui.SetNextItemWidth(72f);
                if (ImGuiSettingControls.DrawOverlayFontScaleDrag("##overrideFontSizeScale", ref group.OverrideFontSizeScale))
                    EzConfig.Save();
                ImGui.EndDisabled();
                ImGui.EndDisabled();
                if (C.FollowVanillaDtr && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    ImGui.SetTooltip("Font size uses Follow Vanilla DTR scale while Follow Vanilla is enabled.");
            });
        }

        ImGui.TextUnformatted("Separator width :");
        ImGuiSettingControls.Indented(() =>
        {
            if (ImGui.Checkbox("Override##sepWidth", ref group.OverrideSeparatorSlotWidthPxEnabled))
                EzConfig.Save();
            ImGui.SameLine();
            ImGui.BeginDisabled(!group.OverrideSeparatorSlotWidthPxEnabled);
            var width = group.OverrideSeparatorSlotWidthPx;
            ImGui.SetNextItemWidth(88f);
            if (ImGuiSettingControls.DrawSeparatorSlotWidthDrag("##overrideSeparatorWidth", ref width))
            {
                group.OverrideSeparatorSlotWidthPx = width;
                EzConfig.Save();
            }
            ImGui.EndDisabled();
        });

        ImGui.TextUnformatted("Font Colors :");
        ImGuiSettingControls.Indented(() =>
        {
            if (!BeginFontColorStyleTable("##overrideStyleColors"))
                return;

            DrawOverrideStyleColorRows(group);

            ImGui.EndTable();
        });
    }
}
