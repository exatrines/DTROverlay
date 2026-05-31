using System.Numerics;

namespace DTROverlay.UI;

public static class ConfigWindow
{
    public static void Draw()
    {
        ImGui.SetNextWindowSize(ConfigUiConstants.DefaultWindowSize, ImGuiCond.FirstUseEver);

        var footerHeight = ConfigFooter.GetReservedHeight();
        var bodyHeight = Math.Max(80f, ImGui.GetContentRegionAvail().Y - footerHeight);

        ImGui.BeginChild("##DtrBody", new Vector2(0, bodyHeight));
        if (ImGui.BeginTabBar("DtrOverlayTabs"))
        {
            DrawTab("General", GeneralTab.Draw);
            DrawTab("Settings", SettingsTab.Draw);
            ImGui.EndTabBar();
        }

        ImGui.EndChild();

        ConfigFooter.Draw();
    }

    private static void DrawTab(string name, Action draw)
    {
        if (!ImGui.BeginTabItem(name))
            return;

        var contentHeight = ImGui.GetContentRegionAvail().Y;
        ImGui.BeginChild(name + "child", new Vector2(0, contentHeight));
        try
        {
            draw();
        }
        catch (Exception e)
        {
            e.Log();
        }

        ImGui.EndChild();
        ImGui.EndTabItem();
    }
}
