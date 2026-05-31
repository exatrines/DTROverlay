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
}
