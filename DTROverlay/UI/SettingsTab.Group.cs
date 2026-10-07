using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    public static void DrawOverlaysPage()
    {
        var followVanilla = C.FollowNativeDtr;
        if (followVanilla)
            DtrOverlayGroups.ApplyFollowNativeConstraints();

        if (ImGui.BeginChild("##groupDetails"))
            DrawGroupDetailsPanel(followVanilla);
        ImGui.EndChild();
    }

    private static void DrawGroupDetailsPanel(bool followVanilla)
    {
        var group = DtrOverlayGroups.GetSelected();
        using var _ = OverlayStyleContext.Push(group);

        var isNative = DtrOverlayGroups.IsNativeOverlay(group);

        DrawGroupStyleColumns(group, followVanilla);

        if (isNative)
            return;

        DrawEntriesSection(group);
    }

    private static void DrawGroupStyleColumns(DtrOverlayGroup group, bool followVanilla)
    {
        DrawFiftyFiftyColumns(
            "groupStyleSplit",
            () =>
            {
                MirageUi.SubHeader(I18n.Get("general.header"), pushDown: false);
                DrawGroupNameControl(group);
                DrawGroupEnabledControl(group);

                if (!followVanilla || group.Kind == DtrOverlayGroupKind.Custom)
                {
                    DrawPositionSection(group);

                    if (UsesGroupLayout(group))
                        DrawGroupLayoutSection(group);
                }

                DrawOverrideStyleSection(group);
            },
            () =>
            {
                if (ShouldShowGroupSeparatorSettings(group))
                    DrawGroupSeparatorSettings(group);

                if (ShouldShowNativeGroupSettings(group))
                    DrawServerInfoSection(group);

                DrawGroupTooltipSection(group);
            });
    }

    private static void DrawGroupEnabledControl(DtrOverlayGroup group)
    {
        if (MirageUi.Checkbox(I18n.Get("overlay.enable"), ref group.Enabled))
            C.Save();

        MirageUi.Tooltip(I18n.Get("overlay.enable_hint"));
    }

    private static void DrawGroupNameControl(DtrOverlayGroup group)
    {
        var locked = DtrOverlayGroups.IsSystemOverlay(group);
        var name = group.Name;
        using (MirageUi.DisabledIf(locked))
        {
            if (MirageUi.InputText(I18n.Get("overlay.name"), ref name, 64, $"groupName_{group.Id}"))
                DtrOverlayGroups.TryRenameOverlay(group, name);
        }

        if (locked)
            TooltipWhenDisabled(I18n.Get("overlay.rename_locked"));
    }
}
