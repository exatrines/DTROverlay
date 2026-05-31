using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using DTROverlay.Services;
using DTROverlay.UI;
using ECommons.SimpleGui;

namespace DTROverlay;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "DTR Overlay";

    internal static Configuration C = null!;
    internal static Plugin P = null!;

    private readonly WindowSystem _overlayWindows = new("DTROverlayOverlay");
    private readonly OverlayWindow _overlay = new();

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        P = this;
        ECommonsMain.Init(pluginInterface, this);

        C = EzConfig.Init<Configuration>();
        EzConfigGui.Init(UI.ConfigWindow.Draw, windowType: EzConfigGui.WindowType.Config);
        ConfigureConfigWindow();

        _overlayWindows.AddWindow(_overlay);
        DtrNativePluginHider.Register();
        Svc.PluginInterface.UiBuilder.Draw += DrawOverlay;

        const string help = "on|off|toggle|edit — Control overlay. No args: open settings.";
        EzCmd.Add("/dtroverlay", PluginCommands.Handle, help);
    }

    private static void ConfigureConfigWindow()
    {
        if (EzConfigGui.Window == null)
            return;

        EzConfigGui.Window.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = ConfigUiConstants.MinimumWindowSize,
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    private void DrawOverlay() => _overlayWindows.Draw();

    public void Dispose()
    {
        Svc.PluginInterface.UiBuilder.Draw -= DrawOverlay;
        DtrNativePluginHider.Unregister();
        DtrOverlayFonts.Dispose();
        _overlayWindows.RemoveAllWindows();
        ECommonsMain.Dispose();
        P = null!;
        C = null!;
    }
}
