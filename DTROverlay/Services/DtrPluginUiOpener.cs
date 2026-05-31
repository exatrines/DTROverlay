using System.Linq;
using System.Reflection;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Plugin;

namespace DTROverlay.Services;

internal static class DtrPluginUiOpener
{
    public static void TryOpen(string entryTitle)
    {
        if (string.IsNullOrEmpty(entryTitle))
            return;

        var plugin = ResolvePlugin(entryTitle);
        if (plugin == null)
            return;

        if (plugin.HasMainUi)
            plugin.OpenMainUi();
        else if (plugin.HasConfigUi)
            plugin.OpenConfigUi();
    }

    private static IExposedPlugin ResolvePlugin(string entryTitle)
    {
        foreach (var plugin in Svc.PluginInterface.InstalledPlugins)
        {
            if (MatchesTitle(plugin, entryTitle))
                return plugin;
        }

        var entry = Svc.DtrBar.Entries.FirstOrDefault(e => e.Title == entryTitle);
        if (entry == null)
            return null;

        var ownerInternalName = GetOwnerInternalName(entry);
        if (ownerInternalName == null)
            return null;

        return Svc.PluginInterface.InstalledPlugins.FirstOrDefault(p =>
            string.Equals(p.InternalName, ownerInternalName, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesTitle(IExposedPlugin plugin, string entryTitle) =>
        string.Equals(plugin.InternalName, entryTitle, StringComparison.OrdinalIgnoreCase)
        || string.Equals(plugin.Name, entryTitle, StringComparison.OrdinalIgnoreCase);

    private static string GetOwnerInternalName(IReadOnlyDtrBarEntry entry)
    {
        var ownerProp = entry.GetType().GetProperty(
            "OwnerPlugin",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (ownerProp?.GetValue(entry) is not { } owner)
            return null;

        var internalNameProp = owner.GetType().GetProperty(
            "InternalName",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        return internalNameProp?.GetValue(owner) as string;
    }
}
