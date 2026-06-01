using Dalamud.Interface.Windowing;
using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class OverlayWindowHost
{
    private static WindowSystem _windowSystem;
    private static bool _refreshRequested;

    public static void Initialize(WindowSystem windowSystem)
    {
        _windowSystem = windowSystem;
        RefreshWindows();
    }

    public static void RequestRefresh() => _refreshRequested = true;

    public static void Draw()
    {
        if (_windowSystem == null)
            return;

        if (_refreshRequested)
        {
            RefreshWindows();
            _refreshRequested = false;
        }

        _windowSystem.Draw();
    }

    private static void RefreshWindows()
    {
        if (_windowSystem == null)
            return;

        DtrOverlayGroups.EnsureInitialized();

        var activeIds = C.OverlayGroups.Select(g => g.Id).ToHashSet();

        foreach (var window in _windowSystem.Windows.ToList())
        {
            if (window is OverlayWindow overlay && !activeIds.Contains(overlay.GroupId))
                _windowSystem.RemoveWindow(window);
        }

        var groupsToHost = C.FollowVanillaDtr
            ? [DtrOverlayGroups.GetDefaultGroup()]
            : C.OverlayGroups.Where(DtrOverlayGroups.IsGroupHostedAsOverlay).ToList();

        foreach (var group in groupsToHost)
        {
            if (_windowSystem.Windows.OfType<OverlayWindow>().Any(w => w.GroupId == group.Id))
                continue;

            _windowSystem.AddWindow(new OverlayWindow(group.Id));
        }
    }
}
