using Dalamud.Interface.Utility;
using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawPositionSection(DtrOverlayGroup group)
    {
        MirageUi.SubHeader("Position");

        if (MirageUi.Checkbox("Edit mode", ref group.OverlayEditMode))
        {
            if (group.OverlayEditMode)
            {
                foreach (var other in C.Overlays)
                {
                    if (other.Id != group.Id)
                        other.OverlayEditMode = false;
                }
            }

            C.Save();
        }

        MirageUi.Tooltip("Drag the overlay in-game while enabled.");

        DrawOverlayPositionOriginSettings(group);

        var viewport = ImGuiHelpers.MainViewport;
        var maxX = MathF.Max(0f, viewport.Size.X);
        var maxY = MathF.Max(0f, viewport.Size.Y);

        if (MirageUi.SliderFloat("X", ref group.OverlayPosition.X, 0f, maxX, "%.0f", $"overlayPosX_{group.Id}"))
            C.Save();

        if (MirageUi.SliderFloat("Y", ref group.OverlayPosition.Y, 0f, maxY, "%.0f", $"overlayPosY_{group.Id}"))
            C.Save();
    }

    private static DtrOverlayGroup GetServerInfoSettingsGroup(DtrOverlayGroup panelGroup) =>
        DtrOverlayGroups.IsDefaultOverlay(panelGroup) && !DtrOverlayGroups.IsSplitNativeMode()
            ? DtrOverlayGroups.GetNativeOverlay()
            : panelGroup;

    private static void DrawServerInfoSection(DtrOverlayGroup panelGroup)
    {
        var group = GetServerInfoSettingsGroup(panelGroup);

        MirageUi.SubHeader("Native Overlay");
        if (panelGroup.Id != group.Id)
            MirageUi.Tooltip("Server info is stored on the Native overlay.");

        DrawServerInfoDisplayModeSettings(group);
        DrawServerInfoPartSettings(group);
    }

    private static void DrawServerInfoDisplayModeSettings(DtrOverlayGroup group)
    {
        var displayMode = group.ServerInfoDisplayMode;
        if (!DrawEnumDropdown("Mode", ref displayMode, ServerInfoModeLabels, $"serverInfoMode_{group.Id}"))
            return;

        group.ServerInfoDisplayMode = displayMode;
        C.Save();
    }

    private static void DrawServerInfoPartSettings(DtrOverlayGroup group)
    {
        MirageUi.Text("Enable display parts", MirageUi.Color.Secondary);

        foreach (var partId in OverlayEntryIds.ServerInfoParts)
        {
            var visible = !group.HiddenServerInfoParts.Contains(partId);
            if (!MirageUi.Checkbox(OverlayEntryIds.GetPartDisplayName(partId), ref visible))
                continue;

            if (visible)
                group.HiddenServerInfoParts.Remove(partId);
            else
                group.HiddenServerInfoParts.Add(partId);

            C.Save();
        }
    }

    private static void DrawOverlayPositionOriginSettings(DtrOverlayGroup group)
    {
        var previousOrigin = group.OverlayPositionOrigin;
        var origin = group.OverlayPositionOrigin;
        if (DrawEnumDropdown("Overlay origin", ref origin, OverlayOriginLabels, $"overlayOrigin_{group.Id}"))
            group.OverlayPositionOrigin = origin;

        MirageUi.Tooltip(
            "Top origins: X/Y offset from the top edge. "
            + "Bottom origins: X/Y offset from the bottom edge.");

        if (group.OverlayPositionOrigin == previousOrigin)
            return;

        OverlayPositioning.OnOriginChanged(
            group,
            previousOrigin,
            OverlayWindow.GetLastWidthForGroup(group.Id),
            OverlayWindow.GetLastHeightForGroup(group.Id));
        C.Save();
    }
}
