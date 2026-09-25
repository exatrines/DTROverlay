using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static readonly string[] LayoutModeLabels = ["Horizontal", "Vertical"];
    private static readonly string[] FollowVanillaSideLabels = ["Left side", "Right side"];
    private static readonly string[] HorizontalFlowLabels = ["Left to right", "Right to left"];
    private static readonly string[] VerticalFlowLabels = ["Top to bottom", "Bottom to top"];
    private static readonly string[] VerticalAlignmentLabels = ["Left align", "Right align"];
    private static readonly string[] ServerInfoModeLabels = ["Icon mode", "Text mode"];
    private static readonly string[] OverlayOriginLabels = ["Top left", "Top right", "Bottom left", "Bottom right"];
    private static readonly string[] TooltipPositionLabels = ["Follow cursor", "Upper", "Lower"];

    public static void DrawStylePage()
    {
        DrawDefaultStyleSection();
        DrawDefaultTooltipSection();
    }

    public static void DrawOptionsPage()
    {
        DrawShortcutsSection();
        DrawOptionSection();
    }

    private static bool DrawEnumDropdown<T>(
        string label,
        ref T value,
        string[] labels,
        string id,
        float width = MirageUi.InputWidthFill)
        where T : struct, Enum
    {
        var index = Convert.ToInt32(value);
        if (index < 0 || index >= labels.Length)
            index = 0;

        var selected = labels[index];
        if (!MirageUi.Dropdown(label, ref selected, labels, allowClear: false, id: id, width: width))
            return false;

        var next = Array.IndexOf(labels, selected);
        if (next < 0 || next == index)
            return false;

        value = (T)Enum.ToObject(typeof(T), next);
        return true;
    }

    private static bool DrawOverlayFontScale(
        string label,
        ref float scale,
        string id,
        float width = MirageUi.InputWidthFill)
    {
        if (!MirageUi.SliderFloat(label, ref scale, 0.5f, 3f, "%.2f", id, width))
            return false;

        DtrOverlayFonts.NotifyScaleChanged();
        C.Save();
        return true;
    }

    private static void DrawFiftyFiftyColumns(string id, Action drawLeft, Action drawRight)
    {
        var gutter = ImGui.GetStyle().ItemSpacing.X;
        var lineThickness = MathF.Max(1f, ImGui.GetIO().FontGlobalScale);
        var midWidth = gutter * 2f + lineThickness;
        var top = ImGui.GetCursorScreenPos();
        var splitX = top.X + ImGui.GetContentRegionAvail().X * 0.5f;

        if (!ImGui.BeginTable(
                $"##{id}",
                3,
                ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.NoPadOuterX,
                new Vector2(-1f, 0f)))
            return;

        ImGui.TableSetupColumn("##left", ImGuiTableColumnFlags.WidthStretch, 0.5f);
        ImGui.TableSetupColumn("##mid", ImGuiTableColumnFlags.WidthFixed, midWidth);
        ImGui.TableSetupColumn("##right", ImGuiTableColumnFlags.WidthStretch, 0.5f);
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        drawLeft();

        ImGui.TableNextColumn();
        ImGui.Dummy(new Vector2(midWidth, 1f));

        ImGui.TableNextColumn();
        drawRight();

        ImGui.EndTable();

        var bottom = ImGui.GetCursorScreenPos();
        if (bottom.Y <= top.Y)
            return;

        ImGui.GetWindowDrawList().AddLine(
            new Vector2(splitX, top.Y),
            new Vector2(splitX, bottom.Y),
            ImGui.GetColorU32(ImGuiCol.Separator),
            lineThickness);
    }

    private static void DrawOverrideField(
        string label,
        string id,
        ref bool enabled,
        Action<float> drawControl,
        Action onEnabledChanged = null)
    {
        if (!ImGui.BeginTable(
                $"##{id}OverrideRow",
                2,
                ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.NoPadOuterX,
                new Vector2(-1f, 0f)))
            return;

        ImGui.TableSetupColumn("##lbl", ImGuiTableColumnFlags.WidthFixed, MirageUi.FieldLabelColumnWidth);
        ImGui.TableSetupColumn("##fld", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        MirageUi.Text(label, wrap: false);

        ImGui.TableNextColumn();
        if (MirageUi.Checkbox($"##{id}Enabled", ref enabled))
        {
            C.Save();
            onEnabledChanged?.Invoke();
        }

        MirageUi.Tooltip("Override the default setting.");

        ImGui.SameLine(0f, ImGui.GetStyle().ItemInnerSpacing.X);
        var controlWidth = MathF.Max(48f, ImGui.GetContentRegionAvail().X);
        using (MirageUi.DisabledIf(!enabled))
            drawControl(controlWidth);

        ImGui.EndTable();
    }

    private static void TooltipWhenDisabled(string text)
    {
        if (!ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) || string.IsNullOrEmpty(text))
            return;

        ImGui.BeginTooltip();
        ImGui.TextUnformatted(text);
        ImGui.EndTooltip();
    }

    private static Vector2 SmallIconSize()
    {
        var size = ImGui.GetFrameHeight();
        return new Vector2(size, size);
    }
}
