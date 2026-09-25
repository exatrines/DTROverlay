using System.Reflection;

namespace DTROverlay.Services;

/// <summary>
/// Dalamud general settings DTR visibility (<c>DtrIgnore</c> list).
/// </summary>
internal static class DtrDalamudConfig
{
    private const BindingFlags AllFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static void HideAllEntriesInDalamudSettings()
    {
        try
        {
            var config = GetDalamudConfig();
            var dtrIgnore = GetMember<List<string>>(config, "DtrIgnore");
            var changed = false;

            foreach (var entry in PluginServices.DtrBar.Entries)
            {
                if (dtrIgnore.Contains(entry.Title))
                    continue;

                dtrIgnore.Add(entry.Title);
                changed = true;
            }

            if (changed)
                config.GetType().GetMethod("QueueSave", AllFlags)?.Invoke(config, []);
        }
        catch (Exception e)
        {
            PluginServices.Log.Error(e, "Failed to update Dalamud DTR ignore list.");
        }
    }

    private static object GetDalamudConfig()
    {
        var assembly = PluginServices.PluginInterface.GetType().Assembly;
        var serviceType = assembly.GetType("Dalamud.Service`1", throwOnError: true);
        var configType = assembly.GetType("Dalamud.Configuration.Internal.DalamudConfiguration", throwOnError: true);
        var closed = serviceType.MakeGenericType(configType);
        return closed.GetMethod("Get")?.Invoke(null, [])
            ?? throw new InvalidOperationException("DalamudConfiguration service was not found.");
    }

    private static T GetMember<T>(object obj, string name)
    {
        var type = obj.GetType();
        while (type != null)
        {
            var field = type.GetField(name, AllFlags);
            if (field != null)
                return (T)field.GetValue(obj);

            var property = type.GetProperty(name, AllFlags);
            if (property != null)
                return (T)property.GetValue(obj);

            type = type.BaseType;
        }

        throw new InvalidOperationException($"Member '{name}' was not found.");
    }
}
