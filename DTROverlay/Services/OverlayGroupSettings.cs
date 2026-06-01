namespace DTROverlay.Services;

internal static class OverlayGroupSettings
{
    public static DtrOverlayGroup NativeServerInfoGroup => DtrOverlayGroups.GetNativeGroup();

    public static bool IsServerInfoPartVisible(string partId) =>
        !C.FollowVanillaDtr && !NativeServerInfoGroup.HiddenServerInfoParts.Contains(partId);

    public static ServerInfoDisplayMode GetServerInfoDisplayMode() =>
        NativeServerInfoGroup.ServerInfoDisplayMode;

    public static bool ShowsPluginSeparatorBar(DtrOverlayGroup group = null)
    {
        var resolved = OverlayGroupLayout.Resolve(group);
        return resolved.ShowPluginEntrySeparators;
    }

    public static bool ShowsNativeSeparatorBar() =>
        NativeServerInfoGroup.ShowNativeEntrySeparators;

    public static bool ShowsDivisionSeparatorBar(DtrOverlayGroup group = null)
    {
        var resolved = OverlayGroupLayout.Resolve(group);
        return resolved.ShowDivisionSeparatorBar;
    }
}
