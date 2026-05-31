namespace DTROverlay.UI;

public static class GeneralTab
{
    public static void Draw()
    {
        DtrImGui.SectionHeader("DTR Overlay");

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudWhite);
        ImGui.TextWrapped(
            "Create customizable DTR overlay. "
            + "Please check the Settings tab. "
            + "Start by trying `Layout` > `Follow Vanilla DTR` setting.");
        ImGui.PopStyleColor();

    }
}
