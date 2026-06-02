using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    public static bool UsesGroupLayout(DtrOverlayGroup group) =>
        !C.FollowVanillaDtr
        && (group.Kind == DtrOverlayGroupKind.Custom
            || DtrOverlayGroups.IsDefaultGroup(group)
            || (DtrOverlayGroups.IsNativeGroup(group) && DtrOverlayGroups.IsSplitNativeMode()));

    /// <summary>
    /// Native group server-info settings on the group details panel (Native row, or Default when merged).
    /// Hidden for Default while <see cref="Configuration.FollowVanillaDtr"/> is on — vanilla DTR is used instead.
    /// </summary>
    public static bool ShouldShowNativeGroupSettings(DtrOverlayGroup panelGroup)
    {
        if (C.FollowVanillaDtr && DtrOverlayGroups.IsDefaultGroup(panelGroup))
            return false;

        return DtrOverlayGroups.IsNativeGroup(panelGroup)
            || (DtrOverlayGroups.IsDefaultGroup(panelGroup) && !DtrOverlayGroups.IsSplitNativeMode());
    }

    public static bool ShouldShowGroupSeparatorSettings(DtrOverlayGroup group)
    {
        if (C.FollowVanillaDtr)
            return DtrOverlayGroups.IsDefaultGroup(group);

        if (DtrOverlayGroups.IsNativeGroup(group))
            return true;

        if (DtrOverlayGroups.IsDefaultGroup(group))
            return true;

        return group.Kind == DtrOverlayGroupKind.Custom;
    }

    public static void DrawGroupSeparatorSettings(DtrOverlayGroup group)
    {
        DtrImGui.SectionHeader("Separators");

        if (!DtrOverlayGroups.IsNativeGroup(group))
        {
            if (ImGui.Checkbox("Show plugin separator bars", ref group.ShowPluginEntrySeparators))
                EzConfig.Save();

            if (ShouldShowDivisionSeparatorCheckbox(group))
                DrawDivisionSeparatorCheckbox(group);
        }

        if (C.FollowVanillaDtr && DtrOverlayGroups.IsDefaultGroup(group))
            return;

        var showNativeSeparators = ShouldShowNativeGroupSettings(group)
            && (DtrOverlayGroups.IsNativeGroup(group)
                || (DtrOverlayGroups.IsDefaultGroup(group) && !DtrOverlayGroups.IsSplitNativeMode()));

        if (showNativeSeparators)
        {
            var native = DtrOverlayGroups.IsNativeGroup(group) ? group : DtrOverlayGroups.GetNativeGroup();
            if (ImGui.Checkbox("Show native separator bars", ref native.ShowNativeEntrySeparators))
                EzConfig.Save();

            if (!DtrOverlayGroups.IsNativeGroup(group) && ImGui.IsItemHovered())
                ImGui.SetTooltip("Stored on the Native group.");
        }
    }

    public static void DrawGroupLayoutSection(DtrOverlayGroup group)
    {
        DtrImGui.SectionHeader("Layout");
        DrawGroupLayoutContent(group);
    }

    private static void DrawGroupLayoutContent(DtrOverlayGroup group)
    {
        ImGuiSettingControls.LabeledIndented("Line direction :", () =>
        {
            var layoutMode = (int)group.LayoutMode;
            ImGuiSettingControls.RadioPair(
                "Horizontal",
                "Vertical",
                ref layoutMode,
                (int)OverlayLayoutMode.Horizontal,
                (int)OverlayLayoutMode.Vertical);

            var newMode = (OverlayLayoutMode)layoutMode;
            if (group.LayoutMode != newMode)
            {
                group.LayoutMode = newMode;
                EzConfig.Save();
            }
        });

        if (!DtrOverlayGroups.IsNativeGroup(group))
            DrawPluginFlowSettings(group);
    }

    private static void DrawFollowVanillaLayoutSettings()
    {
        ImGuiSettingControls.LabeledIndented("Overlay relative position :", () =>
        {
            var side = (int)C.FollowVanillaDtrSide;
            ImGuiSettingControls.RadioPair(
                "Left side",
                "Right side",
                ref side,
                (int)FollowVanillaDtrSide.Left,
                (int)FollowVanillaDtrSide.Right);
            var newSide = (FollowVanillaDtrSide)side;
            if (C.FollowVanillaDtrSide != newSide)
            {
                C.FollowVanillaDtrSide = newSide;
                EzConfig.Save();
            }

            ImGui.SetNextItemWidth(88f);
            if (ImGui.DragFloat("X Offset##followVanilla", ref C.FollowVanillaHorizontalOffset, 0.1f, -100f, 100f, "%.1f")
                && ImGui.IsItemDeactivatedAfterEdit())
                EzConfig.Save();
            ImGui.SameLine();
            ImGui.SetNextItemWidth(88f);
            if (ImGui.DragFloat("Y Offset##followVanilla", ref C.FollowVanillaVerticalOffset, 0.1f, -30f, 30f, "%.1f")
                && ImGui.IsItemDeactivatedAfterEdit())
                EzConfig.Save();
            ImGui.SameLine();
            ImGui.SetNextItemWidth(88f);
            ImGuiSettingControls.DrawOverlayFontScaleDrag("Font Size Scale##followVanilla", ref C.FollowVanillaFontSizeScale);
        });
    }

    private static void DrawDivisionSeparatorCheckbox(DtrOverlayGroup group)
    {
        if (ImGui.Checkbox("Show division separator", ref group.ShowDivisionSeparatorBar))
            EzConfig.Save();
    }

    private static void DrawPluginFlowSettings(DtrOverlayGroup group)
    {
        if (group.LayoutMode == OverlayLayoutMode.Horizontal)
            DrawHorizontalPluginFlow(group);
        else
            DrawVerticalPluginFlow(group);
    }

    private static void DrawHorizontalPluginFlow(DtrOverlayGroup group)
    {
        ImGuiSettingControls.LabeledIndented("Plugin order :", () =>
        {
            var flow = (int)group.HorizontalPluginFlow;
            ImGuiSettingControls.RadioPair(
                "Left to right",
                "Right to left",
                ref flow,
                (int)OverlayHorizontalFlow.LeftToRight,
                (int)OverlayHorizontalFlow.RightToLeft);

            var newFlow = (OverlayHorizontalFlow)flow;
            if (group.HorizontalPluginFlow != newFlow)
            {
                group.HorizontalPluginFlow = newFlow;
                EzConfig.Save();
            }
        });
    }

    private static void DrawVerticalPluginFlow(DtrOverlayGroup group)
    {
        ImGuiSettingControls.LabeledIndented("Plugin order :", () =>
        {
            var verticalFlow = (int)group.VerticalPluginFlow;
            ImGuiSettingControls.RadioPair(
                "Top to bottom",
                "Bottom to top",
                ref verticalFlow,
                (int)OverlayVerticalFlow.TopToBottom,
                (int)OverlayVerticalFlow.BottomToTop);

            var newVerticalFlow = (OverlayVerticalFlow)verticalFlow;
            if (group.VerticalPluginFlow != newVerticalFlow)
            {
                group.VerticalPluginFlow = newVerticalFlow;
                EzConfig.Save();
            }
        });

        ImGuiSettingControls.LabeledIndented("Plugin alignment :", () =>
        {
            var alignment = (int)group.VerticalAlignment;
            ImGuiSettingControls.RadioPair(
                "Left align",
                "Right align",
                ref alignment,
                (int)OverlayVerticalAlignment.Left,
                (int)OverlayVerticalAlignment.Right);

            var newAlignment = (OverlayVerticalAlignment)alignment;
            if (group.VerticalAlignment != newAlignment)
            {
                group.VerticalAlignment = newAlignment;
                EzConfig.Save();
            }
        });
    }
}
