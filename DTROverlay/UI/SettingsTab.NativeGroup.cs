using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawPositionSection()
    {
        DtrImGui.SectionHeader("Position");

        if (ImGui.Checkbox("Edit mode", ref C.OverlayEditMode) && !C.OverlayEditMode)
            EzConfig.Save();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Drag the overlay in-game while enabled.");

        DrawOverlayPositionOriginSettings();

        ImGuiSettingControls.LabeledIndented("Position on screen :", () =>
        {
            ImGui.SetNextItemWidth(100f);
            ImGui.DragFloat("X##overlayPos", ref C.OverlayPosition.X, 1f, 0f, 5000f, "%.0f");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            ImGui.DragFloat("Y##overlayPos", ref C.OverlayPosition.Y, 1f, 0f, 5000f, "%.0f");
        });
    }

    private static void DrawServerInfoSection()
    {
        DtrImGui.SectionHeader("Native Group");

        if (ImGui.Checkbox("Show Native Group", ref C.ShowServerInfo))
            EzConfig.Save();

        ImGui.BeginDisabled(!C.ShowServerInfo);
        DrawServerInfoDisplayModeSettings();
        DrawServerInfoPartSettings();
        ImGui.EndDisabled();
    }

    private static void DrawServerInfoDisplayModeSettings()
    {
        ImGuiSettingControls.LabeledIndented("Mode :", () =>
        {
            var displayMode = (int)C.ServerInfoDisplayMode;
            ImGuiSettingControls.RadioPair(
                "Icon mode",
                "Text mode",
                ref displayMode,
                (int)ServerInfoDisplayMode.Icon,
                (int)ServerInfoDisplayMode.Text);
            C.ServerInfoDisplayMode = (ServerInfoDisplayMode)displayMode;
        });
    }

    private static void DrawServerInfoPartSettings()
    {
        ImGui.TextUnformatted("Enable display parts :");
        var serverInfoHidden = C.HiddenEntryTitles.Contains(OverlayEntryIds.ServerInfo);
        ImGui.BeginDisabled(serverInfoHidden);

        ImGuiSettingControls.Indented(() =>
        {
            foreach (var partId in OverlayEntryIds.ServerInfoParts)
            {
                var visible = OverlayEntryIds.IsServerInfoPartVisible(partId);
                if (ImGui.Checkbox(OverlayEntryIds.GetPartDisplayName(partId), ref visible))
                {
                    if (visible)
                        C.HiddenServerInfoParts.Remove(partId);
                    else
                        C.HiddenServerInfoParts.Add(partId);
                }
            }
        });

        ImGui.EndDisabled();
    }

    private static void DrawOverlayPositionOriginSettings()
    {
        var previousOrigin = C.OverlayPositionOrigin;

        ImGuiSettingControls.LabeledIndented("Overlay origin :", () =>
        {
            var origin = (int)C.OverlayPositionOrigin;
            ImGuiSettingControls.RadioPair(
                "Top left",
                "Top right",
                ref origin,
                (int)OverlayPositionOrigin.TopLeft,
                (int)OverlayPositionOrigin.TopRight);
            C.OverlayPositionOrigin = (OverlayPositionOrigin)origin;
        });

        if (C.OverlayPositionOrigin != previousOrigin)
            OverlayPositioning.OnOriginChanged(previousOrigin);
    }
}
