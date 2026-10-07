using DTROverlay.Services;

namespace DTROverlay.UI;

public static partial class SettingsTab
{
    private static string[] LayoutModeLabels() => [I18n.Get("layout.horizontal"), I18n.Get("layout.vertical")];
    private static string[] FollowVanillaSideLabels() => [I18n.Get("layout.side.left"), I18n.Get("layout.side.right")];
    private static string[] HorizontalFlowLabels() => [I18n.Get("layout.ltr"), I18n.Get("layout.rtl")];
    private static string[] VerticalFlowLabels() => [I18n.Get("layout.ttb"), I18n.Get("layout.btt")];
    private static string[] VerticalAlignmentLabels() => [I18n.Get("layout.align_left"), I18n.Get("layout.align_right")];
    private static string[] ServerInfoModeLabels() => [I18n.Get("server.icon"), I18n.Get("server.text")];
    private static string[] OverlayOriginLabels() =>
    [
        I18n.Get("position.top_left"),
        I18n.Get("position.top_right"),
        I18n.Get("position.bottom_left"),
        I18n.Get("position.bottom_right"),
    ];
    private static string[] TooltipPositionLabels() => [I18n.Get("tooltip.follow"), I18n.Get("tooltip.upper"), I18n.Get("tooltip.lower")];

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

        MirageUi.Tooltip(I18n.Get("style.override_hint"));

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
