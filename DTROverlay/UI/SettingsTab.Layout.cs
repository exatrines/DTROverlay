using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static void DrawLayoutSection()
    {
        DtrImGui.SectionHeader("Layout");

        if (ImGui.Checkbox("Follow Vanilla DTR", ref C.FollowVanillaDtr))
            FollowVanillaDtrMode.EnforceLayoutConstraints();

        if (C.FollowVanillaDtr)
        {
            FollowVanillaDtrMode.EnforceLayoutConstraints();
            DrawFollowVanillaLayoutSettings();
            DrawHorizontalPluginFlow();
            return;
        }

        ImGuiSettingControls.LabeledIndented("Line direction :", () =>
        {
            var layoutMode = (int)C.OverlayLayoutMode;
            ImGuiSettingControls.RadioPair(
                "Horizontal",
                "Vertical",
                ref layoutMode,
                (int)OverlayLayoutMode.Horizontal,
                (int)OverlayLayoutMode.Vertical);
            C.OverlayLayoutMode = (OverlayLayoutMode)layoutMode;
        });

        if (C.OverlayLayoutMode == OverlayLayoutMode.Horizontal)
            DrawNativePluginDivisionSettings();

        DrawPluginFlowSettings();
    }

    private static void DrawNativePluginDivisionSettings()
    {
        ImGuiSettingControls.LabeledIndented("Between native and plugins :", () =>
        {
            var division = (int)C.NativePluginDivision;
            ImGuiSettingControls.RadioPair(
                "Separator",
                "Native / plugins: New line",
                ref division,
                (int)NativePluginDivisionMode.Separator,
                (int)NativePluginDivisionMode.NewLine);
            C.NativePluginDivision = (NativePluginDivisionMode)division;
        });
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
            C.FollowVanillaDtrSide = (FollowVanillaDtrSide)side;

            ImGui.SetNextItemWidth(88f);
            ImGui.DragFloat("X Offset##followVanilla", ref C.FollowVanillaHorizontalOffset, 0.1f, -100f, 100f, "%.1f");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(88f);
            ImGui.DragFloat("Y Offset##followVanilla", ref C.FollowVanillaVerticalOffset, 0.1f, -30f, 30f, "%.1f");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(88f);
            if (ImGui.DragFloat("Font Size Scale##followVanilla", ref C.FollowVanillaFontSizeScale, 0.01f, 0.5f, 3f, "%.2f"))
                DtrOverlayFonts.NotifyScaleChanged();
        });
    }

    private static void DrawPluginFlowSettings()
    {
        if (C.OverlayLayoutMode == OverlayLayoutMode.Horizontal)
            DrawHorizontalPluginFlow();
        else
            DrawVerticalPluginFlow();
    }

    private static void DrawHorizontalPluginFlow()
    {
        ImGuiSettingControls.LabeledIndented("Plugin order :", () =>
        {
            var flow = (int)C.HorizontalPluginFlow;
            ImGuiSettingControls.RadioPair(
                "Left to right",
                "Right to left",
                ref flow,
                (int)OverlayHorizontalFlow.LeftToRight,
                (int)OverlayHorizontalFlow.RightToLeft);
            C.HorizontalPluginFlow = (OverlayHorizontalFlow)flow;
        });
    }

    private static void DrawVerticalPluginFlow()
    {
        ImGuiSettingControls.LabeledIndented("Plugin order :", () =>
        {
            var verticalFlow = (int)C.VerticalPluginFlow;
            ImGuiSettingControls.RadioPair(
                "Top to bottom",
                "Bottom to top",
                ref verticalFlow,
                (int)OverlayVerticalFlow.TopToBottom,
                (int)OverlayVerticalFlow.BottomToTop);
            C.VerticalPluginFlow = (OverlayVerticalFlow)verticalFlow;
        });

        ImGuiSettingControls.LabeledIndented("Plugin alignment :", () =>
        {
            var alignment = (int)C.OverlayVerticalAlignment;
            ImGuiSettingControls.RadioPair(
                "Left align",
                "Right align",
                ref alignment,
                (int)OverlayVerticalAlignment.Left,
                (int)OverlayVerticalAlignment.Right);
            C.OverlayVerticalAlignment = (OverlayVerticalAlignment)alignment;
        });
    }
}
