using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using DTROverlay.Services;
using DTROverlay.UI;

namespace DTROverlay;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "DTR Overlay";

    internal static Configuration C = null!;
    internal static Plugin P = null!;

    private readonly WindowSystem _overlayWindows = new("DTROverlayOverlay");
    private readonly WindowSystem _uiWindows = new("DTROverlay");
    private readonly MainWindow _mainWindow;
    private readonly SettingsWindow _settingsWindow;

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IObjectTable objectTable,
        IGameGui gameGui,
        IGameConfig gameConfig,
        IAddonLifecycle addonLifecycle,
        IDtrBar dtrBar,
        ITextureProvider textureProvider,
        IPluginLog log,
        IChatGui chatGui)
    {
        P = this;
        PluginServices.Init(
            pluginInterface,
            commandManager,
            clientState,
            objectTable,
            gameGui,
            gameConfig,
            addonLifecycle,
            dtrBar,
            textureProvider,
            log,
            chatGui);

        C = Configuration.Load(pluginInterface);
        MigrateConfiguration(C);
        C.ThemeColors ??= MirageColorSettings.CreateDefault();
        DtrOverlayGroups.EnsureInitialized();

        MirageUi.ConfigureTheme(() => C.ThemeColors ?? MirageColorSettings.CreateDefault());
        MirageUi.Init(pluginInterface, textureProvider, log);
        MirageUi.ConfigurePluginInfo(info =>
        {
            info.DiscordUrl = "https://discord.gg/gRfxXNZWMs";
            info.SupportUrl = "https://exatrines.github.io/support/";
        });

        _settingsWindow = new SettingsWindow();
        _mainWindow = new MainWindow(ToggleSettings);
        _uiWindows.AddWindow(_mainWindow);
        _uiWindows.AddWindow(_settingsWindow);

        OverlayWindowHost.Initialize(_overlayWindows);
        DtrNativePluginHider.Register();
        pluginInterface.UiBuilder.Draw += DrawUi;
        pluginInterface.UiBuilder.OpenConfigUi += ToggleSettings;
        pluginInterface.UiBuilder.OpenMainUi += ToggleMain;

        commandManager.AddHandler("/dtroverlay", new CommandInfo(PluginCommands.Handle)
        {
            HelpMessage = "Toggle the overlay editor. Settings open from the gear icon.",
        });
    }

    internal void ToggleMain() => _mainWindow.Toggle();

    internal void ToggleSettings() => _settingsWindow.Toggle();

    private static void MigrateConfiguration(Configuration config)
    {
        if (config.TooltipPositionMigrated)
            return;

        config.TooltipPosition = config.CenterTooltipBelowHoveredEntry
            ? TooltipPosition.Lower
            : TooltipPosition.FollowCursor;
        config.TooltipPositionMigrated = true;
    }

    private void DrawUi()
    {
        _uiWindows.Draw();
        OverlayWindowHost.Draw();
    }

    public void Dispose()
    {
        PluginServices.PluginInterface.UiBuilder.Draw -= DrawUi;
        PluginServices.PluginInterface.UiBuilder.OpenConfigUi -= ToggleSettings;
        PluginServices.PluginInterface.UiBuilder.OpenMainUi -= ToggleMain;
        PluginServices.CommandManager.RemoveHandler("/dtroverlay");
        DtrNativePluginHider.Unregister();
        DtrOverlayFonts.Dispose();
        MirageUi.Dispose();
        _uiWindows.RemoveAllWindows();
        _overlayWindows.RemoveAllWindows();
        PluginServices.Clear();
        P = null!;
        C = null!;
    }
}
