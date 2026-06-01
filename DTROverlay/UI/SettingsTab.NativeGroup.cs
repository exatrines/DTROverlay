using Dalamud.Interface.Utility;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawPositionSection(DtrOverlayGroup group)
    {
        DtrImGui.SectionHeader("Position");

        if (ImGui.Checkbox("Edit mode", ref group.OverlayEditMode))
        {
            if (group.OverlayEditMode)
            {
                foreach (var other in C.OverlayGroups)
                {
                    if (other.Id != group.Id)
                        other.OverlayEditMode = false;
                }
            }

            EzConfig.Save();
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Drag the overlay in-game while enabled.");

        DrawOverlayPositionOriginSettings(group);

        ImGuiSettingControls.LabeledIndented("Position on screen :", () =>
        {
            var viewport = ImGuiHelpers.MainViewport;
            var maxX = MathF.Max(0f, viewport.Size.X);
            var maxY = MathF.Max(0f, viewport.Size.Y);

            ImGui.SetNextItemWidth(100f);
            if (ImGui.DragFloat("X##overlayPos", ref group.OverlayPosition.X, 1f, 0f, maxX, "%.0f"))
                EzConfig.Save();
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100f);
            if (ImGui.DragFloat("Y##overlayPos", ref group.OverlayPosition.Y, 1f, 0f, maxY, "%.0f"))
                EzConfig.Save();
        });
    }

    private static DtrOverlayGroup GetServerInfoSettingsGroup(DtrOverlayGroup panelGroup) =>
        DtrOverlayGroups.IsDefaultGroup(panelGroup) && !DtrOverlayGroups.IsSplitNativeMode()
            ? DtrOverlayGroups.GetNativeGroup()
            : panelGroup;

    private static void DrawServerInfoSection(DtrOverlayGroup panelGroup)
    {
        var group = GetServerInfoSettingsGroup(panelGroup);

        DtrImGui.SectionHeader("Native Group");

        if (panelGroup.Id != group.Id && ImGui.IsItemHovered())
            ImGui.SetTooltip("Server info is stored on the Native group.");

        DrawServerInfoDisplayModeSettings(group);
        DrawServerInfoPartSettings(group);
    }

    private static void DrawServerInfoDisplayModeSettings(DtrOverlayGroup group)
    {
        ImGuiSettingControls.LabeledIndented("Mode :", () =>
        {
            var displayMode = (int)group.ServerInfoDisplayMode;
            ImGuiSettingControls.RadioPair(
                "Icon mode",
                "Text mode",
                ref displayMode,
                (int)ServerInfoDisplayMode.Icon,
                (int)ServerInfoDisplayMode.Text);

            var newMode = (ServerInfoDisplayMode)displayMode;
            if (group.ServerInfoDisplayMode != newMode)
            {
                group.ServerInfoDisplayMode = newMode;
                EzConfig.Save();
            }
        });
    }

    private static void DrawServerInfoPartSettings(DtrOverlayGroup group)
    {
        ImGui.TextUnformatted("Enable display parts :");

        ImGuiSettingControls.Indented(() =>
        {
            foreach (var partId in OverlayEntryIds.ServerInfoParts)
            {
                var visible = !group.HiddenServerInfoParts.Contains(partId);

                if (ImGui.Checkbox(OverlayEntryIds.GetPartDisplayName(partId), ref visible))
                {
                    if (visible)
                        group.HiddenServerInfoParts.Remove(partId);
                    else
                        group.HiddenServerInfoParts.Add(partId);

                    EzConfig.Save();
                }
            }
        });
    }

    private static void DrawOverlayPositionOriginSettings(DtrOverlayGroup group)
    {
        var previousOrigin = group.OverlayPositionOrigin;

        ImGuiSettingControls.LabeledIndented("Overlay origin :", () =>
        {
            var origin = (int)group.OverlayPositionOrigin;
            ImGuiSettingControls.RadioPair(
                "Top left",
                "Top right",
                ref origin,
                (int)OverlayPositionOrigin.TopLeft,
                (int)OverlayPositionOrigin.TopRight);
            group.OverlayPositionOrigin = (OverlayPositionOrigin)origin;
        });

        if (group.OverlayPositionOrigin != previousOrigin)
        {
            OverlayPositioning.OnOriginChanged(
                group,
                previousOrigin,
                OverlayWindow.GetLastWidthForGroup(group.Id));
            EzConfig.Save();
        }
    }
}
