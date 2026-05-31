using System.Collections.Generic;
using ECommons.Reflection;

namespace DTROverlay.Services;

/// <summary>
/// Dalamud general settings DTR visibility (<c>DtrIgnore</c> list).
/// </summary>
internal static class DtrDalamudConfig
{
    public static void HideAllEntriesInDalamudSettings()
    {
        try
        {
            var config = DalamudReflector.GetService("Dalamud.Configuration.Internal.DalamudConfiguration");
            var dtrIgnore = config.GetFoP<List<string>>("DtrIgnore");
            var changed = false;

            foreach (var entry in Svc.DtrBar.Entries)
            {
                if (dtrIgnore.Contains(entry.Title))
                    continue;

                dtrIgnore.Add(entry.Title);
                changed = true;
            }

            if (changed)
                config.Call("QueueSave", []);
        }
        catch (Exception e)
        {
            e.Log();
        }
    }
}
