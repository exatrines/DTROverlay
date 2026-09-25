using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Command;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;

namespace DTROverlay.Services;

/// <summary>Dalamud services injected at plugin startup.</summary>
internal static class PluginServices
{
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    internal static ICommandManager CommandManager { get; private set; } = null!;
    internal static IClientState ClientState { get; private set; } = null!;
    internal static IObjectTable ObjectTable { get; private set; } = null!;
    internal static IGameGui GameGui { get; private set; } = null!;
    internal static IGameConfig GameConfig { get; private set; } = null!;
    internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    internal static IDtrBar DtrBar { get; private set; } = null!;
    internal static ITextureProvider Texture { get; private set; } = null!;
    internal static IPluginLog Log { get; private set; } = null!;
    internal static IChatGui Chat { get; private set; } = null!;

    internal static void Init(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IObjectTable objectTable,
        IGameGui gameGui,
        IGameConfig gameConfig,
        IAddonLifecycle addonLifecycle,
        IDtrBar dtrBar,
        ITextureProvider texture,
        IPluginLog log,
        IChatGui chat)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        ObjectTable = objectTable;
        GameGui = gameGui;
        GameConfig = gameConfig;
        AddonLifecycle = addonLifecycle;
        DtrBar = dtrBar;
        Texture = texture;
        Log = log;
        Chat = chat;
    }

    internal static void Clear()
    {
        PluginInterface = null!;
        CommandManager = null!;
        ClientState = null!;
        ObjectTable = null!;
        GameGui = null!;
        GameConfig = null!;
        AddonLifecycle = null!;
        DtrBar = null!;
        Texture = null!;
        Log = null!;
        Chat = null!;
    }

    internal static void PrintNotice(string message)
    {
        var text = $"[DTR Overlay] {message}";
        Log.Information(text);
        Chat.Print(new XivChatEntry
        {
            Message = new SeStringBuilder().AddUiForeground(text, 3).Build(),
            Type = PluginInterface.GeneralChatType,
        });
    }
}
