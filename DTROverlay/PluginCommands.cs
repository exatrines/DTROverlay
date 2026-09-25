namespace DTROverlay;

internal static class PluginCommands
{
    private const string Usage = "/dtroverlay — toggle the overlay editor";

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
