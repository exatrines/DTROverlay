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
        try
        {
            SettingsTab.Draw();
        }
        catch (Exception e)
        {
            e.Log();
        }

        ImGui.EndChild();

        ConfigFooter.Draw();
    }
}
