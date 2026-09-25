using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    public static bool UsesGroupLayout(DtrOverlayGroup group) =>
        group.Kind == DtrOverlayGroupKind.Custom
        || (!C.FollowNativeDtr
            && (DtrOverlayGroups.IsDefaultOverlay(group)
                || (DtrOverlayGroups.IsNativeOverlay(group) && DtrOverlayGroups.IsSplitNativeMode())));

    /// <summary>
    /// Native group server-info settings on the group details panel (Native row, or Default when merged).
    /// Hidden for Default while <see cref="Configuration.FollowNativeDtr"/> is on — vanilla DTR is used instead.
    /// </summary>
    public static bool ShouldShowNativeGroupSettings(DtrOverlayGroup panelGroup)
    {
        if (C.FollowNativeDtr && DtrOverlayGroups.IsDefaultOverlay(panelGroup))
            return false;

        return DtrOverlayGroups.IsNativeOverlay(panelGroup)
            || (DtrOverlayGroups.IsDefaultOverlay(panelGroup) && !DtrOverlayGroups.IsSplitNativeMode());
    }

    public static bool ShouldShowGroupSeparatorSettings(DtrOverlayGroup group)
    {
        if (C.FollowNativeDtr)
            return DtrOverlayGroups.IsDefaultOverlay(group) || group.Kind == DtrOverlayGroupKind.Custom;

        if (DtrOverlayGroups.IsNativeOverlay(group))
            return true;

        if (DtrOverlayGroups.IsDefaultOverlay(group))
            return true;

        return group.Kind == DtrOverlayGroupKind.Custom;
    }

    public static void DrawGroupSeparatorSettings(DtrOverlayGroup group)
    {
        MirageUi.SubHeader("Separator", pushDown: false);

        if (!DtrOverlayGroups.IsNativeOverlay(group))
        {
            if (MirageUi.Checkbox("Show plugin separator bars", ref group.ShowPluginEntrySeparators))
                C.Save();

            if (ShouldShowDivisionSeparatorCheckbox(group))
                DrawDivisionSeparatorCheckbox(group);
        }

        if (C.FollowNativeDtr && DtrOverlayGroups.IsDefaultOverlay(group))
            return;

        var showNativeSeparators = ShouldShowNativeGroupSettings(group)
            && (DtrOverlayGroups.IsNativeOverlay(group)
                || (DtrOverlayGroups.IsDefaultOverlay(group) && !DtrOverlayGroups.IsSplitNativeMode()));

        if (!showNativeSeparators)
            return;

        var native = DtrOverlayGroups.IsNativeOverlay(group) ? group : DtrOverlayGroups.GetNativeOverlay();
        if (MirageUi.Checkbox("Show native separator bars", ref native.ShowNativeEntrySeparators))
            C.Save();

        if (!DtrOverlayGroups.IsNativeOverlay(group))
            MirageUi.Tooltip("Stored on the Native overlay.");
    }

    public static void DrawGroupLayoutSection(DtrOverlayGroup group)
    {
        MirageUi.SubHeader("Layout");
        DrawGroupLayoutContent(group);
    }

    private static void DrawGroupLayoutContent(DtrOverlayGroup group)
    {
        var layoutMode = group.LayoutMode;
        if (DrawEnumDropdown("Line direction", ref layoutMode, LayoutModeLabels, $"layoutMode_{group.Id}"))
        {
            group.LayoutMode = layoutMode;
            C.Save();
        }

        if (!DtrOverlayGroups.IsNativeOverlay(group))
            DrawPluginFlowSettings(group);
    }

    private static void DrawFollowNativeLayoutSettings()
    {
        var side = C.FollowNativeDtrSide;
        if (DrawEnumDropdown("Overlay relative position", ref side, FollowVanillaSideLabels, "followVanillaSide"))
        {
            C.FollowNativeDtrSide = side;
            C.Save();
        }

        if (MirageUi.SliderFloat("X Offset", ref C.FollowNativeHorizontalOffset, -100f, 100f, "%.1f", "followVanillaX"))
            C.Save();

        if (MirageUi.SliderFloat("Y Offset", ref C.FollowNativeVerticalOffset, -30f, 30f, "%.1f", "followVanillaY"))
            C.Save();

        DrawOverlayFontScale("Font Size Scale", ref C.FollowNativeFontSizeScale, "followVanillaFontScale");
    }

    private static void DrawDivisionSeparatorCheckbox(DtrOverlayGroup group)
    {
        if (MirageUi.Checkbox("Show division separator", ref group.ShowDivisionSeparatorBar))
            C.Save();
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
        var flow = group.HorizontalPluginFlow;
        if (!DrawEnumDropdown("Plugin order", ref flow, HorizontalFlowLabels, $"hFlow_{group.Id}"))
            return;

        group.HorizontalPluginFlow = flow;
        C.Save();
    }

    private static void DrawVerticalPluginFlow(DtrOverlayGroup group)
    {
        var verticalFlow = group.VerticalPluginFlow;
        if (DrawEnumDropdown("Plugin order", ref verticalFlow, VerticalFlowLabels, $"vFlow_{group.Id}"))
        {
            group.VerticalPluginFlow = verticalFlow;
            C.Save();
        }

        var alignment = group.VerticalAlignment;
        if (!DrawEnumDropdown("Plugin alignment", ref alignment, VerticalAlignmentLabels, $"vAlign_{group.Id}"))
            return;

        group.VerticalAlignment = alignment;
        C.Save();
    }
}
