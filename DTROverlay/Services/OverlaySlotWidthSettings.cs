namespace DTROverlay.Services;

internal static class OverlaySlotWidthSettings
{
    public const int MaxWidth = 1000;

    public static int Get(DtrOverlayGroup group, string entryTitle) =>
        !string.IsNullOrEmpty(entryTitle)
        && group.OverlaySlotMinWidthByTitle.TryGetValue(entryTitle, out var width)
            ? Math.Clamp(width, 0, MaxWidth)
            : 0;

    public static void Set(DtrOverlayGroup group, string entryTitle, int width)
    {
        if (string.IsNullOrEmpty(entryTitle))
            return;

        width = Math.Clamp(width, 0, MaxWidth);
        if (width == 0)
            group.OverlaySlotMinWidthByTitle.Remove(entryTitle);
        else
            group.OverlaySlotMinWidthByTitle[entryTitle] = width;
    }
}
