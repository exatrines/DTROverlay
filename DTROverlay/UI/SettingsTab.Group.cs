using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static string _newGroupNameInput = "";

    private static void DrawGroupTabSection()
    {
        DtrImGui.SectionHeader("Groups");

        var followVanilla = C.FollowVanillaDtr;
        if (!followVanilla)
        {
            if (ImGui.Checkbox("Split Native DTR", ref C.SplitNativeDtr))
            {
                DtrOverlayGroups.SyncDefaultGroupDisplayName();

                if (!C.SplitNativeDtr && DtrOverlayGroups.IsNativeGroup(DtrOverlayGroups.GetSelected()))
                    DtrOverlayGroups.Select(DtrOverlayGroups.GetDefaultGroup().Id);

                OverlayWindowHost.RequestRefresh();
                EzConfig.Save();
            }

            ImGui.Spacing();
        }
        else
        {
            DtrOverlayGroups.ApplyFollowVanillaConstraints();
            ImGui.TextUnformatted("Follow Vanilla DTR: configure plugins on the Default group. Native is not used.");
            ImGui.Spacing();
        }

        var avail = ImGui.GetContentRegionAvail();
        var columnHeight = Math.Max(120f, avail.Y);
        var leftWidth = 220f;
        var spacing = ImGui.GetStyle().ItemSpacing.X;

        if (ImGui.BeginChild("##groupList", new Vector2(leftWidth, columnHeight), true))
        {
            DrawGroupListColumn(followVanilla, leftWidth);
            ImGui.EndChild();
        }

        ImGui.SameLine(0f, spacing);

        if (ImGui.BeginChild("##groupDetails", new Vector2(avail.X - leftWidth - spacing, columnHeight), true))
        {
            DrawGroupDetailsPanel(followVanilla);
            ImGui.EndChild();
        }
    }

    private static void DrawGroupDetailsPanel(bool followVanilla)
    {
        var group = DtrOverlayGroups.GetSelected();
        using var _ = OverlayStyleContext.Push(group);

        var isNative = DtrOverlayGroups.IsNativeGroup(group);

        if (followVanilla && isNative)
            return;

        if (followVanilla && group.Kind == DtrOverlayGroupKind.Custom)
        {
            ImGui.TextUnformatted("Custom groups cannot be edited while Follow Vanilla DTR is enabled.");
            return;
        }

        DrawGroupEnabledControl(group);

        if (!followVanilla)
        {
            ImGui.Spacing();
            DrawPositionSection(group);

            if (UsesGroupLayout(group))
            {
                ImGui.Spacing();
                DrawGroupLayoutSection(group);
            }
        }

        if (ShouldShowGroupSeparatorSettings(group))
        {
            ImGui.Spacing();
            DrawGroupSeparatorSettings(group);
        }

        ImGui.Spacing();
        DrawOverrideStyleSection(group);

        if (ShouldShowNativeGroupSettings(group))
        {
            ImGui.Spacing();
            DrawServerInfoSection(group);
        }

        if (!isNative)
        {
            ImGui.Spacing();
            DrawEntriesSection(group);
        }

        ImGui.Spacing();
        DrawGroupTooltipSection(group);
    }

    private static void DrawGroupEnabledControl(DtrOverlayGroup group)
    {
        if (ImGui.Checkbox("Enable", ref group.Enabled))
            EzConfig.Save();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Show this group's overlay when the plugin is enabled.");
    }

    private static void DrawGroupListColumn(bool followVanilla, float columnWidth)
    {
        var selectedGroup = DtrOverlayGroups.GetSelected();
        var canDelete = DtrOverlayGroups.CanRemoveGroup(selectedGroup);
        var iconButtonWidth = ImGui.GetFrameHeight();
        var innerSpacing = ImGui.GetStyle().ItemInnerSpacing.X;
        var inputWidth = columnWidth - iconButtonWidth * 2f - innerSpacing * 4f - ImGui.GetStyle().WindowPadding.X * 2f;

        ImGui.BeginDisabled(followVanilla);
        ImGui.SetNextItemWidth(MathF.Max(48f, inputWidth));
        ImGui.InputTextWithHint("##newGroupName", "Group name", ref _newGroupNameInput, 64);
        ImGui.SameLine(0f, innerSpacing);
        if (ImGuiEx.SmallIconButton(FontAwesomeIcon.Plus))
        {
            if (DtrOverlayGroups.TryAddGroup(_newGroupNameInput))
                _newGroupNameInput = "";
        }

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Add group");

        ImGui.SameLine(0f, innerSpacing);
        ImGui.EndDisabled();

        ImGui.BeginDisabled(!canDelete);
        if (ImGuiEx.SmallIconButton(FontAwesomeIcon.Trash))
            DtrOverlayGroups.TryRemoveGroup(C.SelectedOverlayGroupId);
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(canDelete ? "Delete selected group" : "Default and Native cannot be deleted");
        ImGui.EndDisabled();

        ImGui.Spacing();

        foreach (var listGroup in DtrOverlayGroups.EnumerateGroupsForSettings())
        {
            var selected = listGroup.Id == C.SelectedOverlayGroupId;
            var selectable = DtrOverlayGroups.IsGroupSelectableInSettings(listGroup);
            ImGui.PushID(listGroup.Id);
            ImGui.BeginDisabled(!selectable);
            if (ImGui.Selectable(listGroup.Name, selected))
                DtrOverlayGroups.Select(listGroup.Id);
            if (!selectable && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.SetTooltip(
                    DtrOverlayGroups.IsNativeGroup(listGroup)
                        ? "Native group is not used while Follow Vanilla DTR is enabled."
                        : "Not available while Follow Vanilla DTR is enabled.");
            }
            ImGui.EndDisabled();
            ImGui.PopID();
        }
    }
}
