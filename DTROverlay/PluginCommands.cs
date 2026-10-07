namespace DTROverlay;

internal static class PluginCommands
{
    private static string Usage => I18n.Get("command.usage");

    public static void Handle(string command, string args)
    {
        args = args?.Trim() ?? "";
        if (args.Length == 0)
        {
            P.ToggleMain();
            return;
        }

        PluginServices.PrintNotice(Usage);
    }
}
