using Newtonsoft.Json.Linq;

namespace DTROverlay;

public static class ConfigV2Migrator
{
    public const int CurrentVersion = 2;

    public static void Apply(JObject root)
    {
        var version = root["ConfigVersion"]?.Value<int>() ?? 0;
        if (version >= CurrentVersion)
            return;

        Move(root, "FollowVanillaDtr", "FollowNativeDtr");
        Move(root, "FollowVanillaDtrSide", "FollowNativeDtrSide");
        Move(root, "FollowVanillaHorizontalOffset", "FollowNativeHorizontalOffset");
        Move(root, "FollowVanillaVerticalOffset", "FollowNativeVerticalOffset");
        Move(root, "FollowVanillaFontSizeScale", "FollowNativeFontSizeScale");
        Move(root, "OverlayGroups", "Overlays");
        Move(root, "SelectedOverlayGroupId", "SelectedOverlayId");
        root["ConfigVersion"] = CurrentVersion;
    }

    private static void Move(JObject root, string from, string to)
    {
        if (root[from] == null)
            return;

        root[to] = root[from];
        root.Remove(from);
    }
}
