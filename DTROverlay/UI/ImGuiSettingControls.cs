using DTROverlay.Services;

namespace DTROverlay.UI;

internal static class ImGuiSettingControls
{
    public static void Indented(Action draw)
    {
        ImGui.Indent();
        draw();
        ImGui.Unindent();
    }

    public static void LabeledIndented(string label, Action draw)
    {
        ImGui.TextUnformatted(label);
        Indented(draw);
    }

    public static void RadioPair(string labelA, string labelB, ref int selected, int valueA, int valueB)
    {
        if (ImGui.RadioButton(labelA, ref selected, valueA))
            selected = valueA;

        ImGui.SameLine();
        if (ImGui.RadioButton(labelB, ref selected, valueB))
            selected = valueB;
    }

    public static bool DragInt(string id, ref int value, float speed, int min, int max)
    {
        var dragValue = (float)value;
        if (!ImGui.DragFloat(id, ref dragValue, speed, min, max, "%.0f"))
            return false;

        value = (int)MathF.Round(Math.Clamp(dragValue, min, max));
        return true;
    }

    public static bool DrawOverlayFontScaleDrag(string id, ref float scale)
    {
        if (!ImGui.DragFloat(id, ref scale, 0.01f, 0.5f, 3f, "%.2f"))
            return false;

        DtrOverlayFonts.NotifyScaleChanged();
        return true;
    }

    public static bool DrawSeparatorSlotWidthDrag(string id, ref int widthPx) =>
        DragInt(id, ref widthPx, 1f, 0, OverlaySlotWidthSettings.MaxWidth);

    public static bool DrawTooltipPositionRadios(ref TooltipPosition position)
    {
        var selected = (int)position;
        if (ImGui.RadioButton("Follow cursor", ref selected, (int)TooltipPosition.FollowCursor))
            selected = (int)TooltipPosition.FollowCursor;

        ImGui.SameLine();
        if (ImGui.RadioButton("Upper", ref selected, (int)TooltipPosition.Upper))
            selected = (int)TooltipPosition.Upper;

        ImGui.SameLine();
        if (ImGui.RadioButton("Lower", ref selected, (int)TooltipPosition.Lower))
            selected = (int)TooltipPosition.Lower;

        if (selected == (int)position)
            return false;

        position = (TooltipPosition)selected;
        return true;
    }
}
