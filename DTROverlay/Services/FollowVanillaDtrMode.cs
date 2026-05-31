namespace DTROverlay.Services;

internal static class FollowVanillaDtrMode
{
    public static bool IsActive => C.FollowVanillaDtr && C.OverlayEnabled;

    public static void EnforceLayoutConstraints()
    {
        if (!C.FollowVanillaDtr)
            return;

        C.OverlayLayoutMode = OverlayLayoutMode.Horizontal;
        C.OverlayEditMode = false;
    }
}
