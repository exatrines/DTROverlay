using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class EntryFixedWidth
{
    public const float DefaultWidthPixels = 40f;

    public static bool IsWidthEnabled(string layoutKey) =>
        !string.IsNullOrEmpty(layoutKey) && C.FixedWidthEnabledIds.Contains(layoutKey);

    public static bool IsColorEnabled(string layoutKey) =>
        !string.IsNullOrEmpty(layoutKey) && C.FixedColorEnabledIds.Contains(layoutKey);

    public static Vector4 GetDefaultTextColor() =>
        IsColorEnabled(OverlayEntryIds.AppearanceText)
            ? C.TextColor
            : DtrStyle.DefaultTextColor;

    public static Vector4 GetDefaultOutlineColor() =>
        IsColorEnabled(OverlayEntryIds.AppearanceText)
            ? C.OutlineColor
            : DtrStyle.DefaultOutlineColor;

    public static float ResolveWidth(string layoutKey, float measuredWidth)
    {
        if (!IsWidthEnabled(layoutKey)
            || !C.FixedWidthPixels.TryGetValue(layoutKey, out var width)
            || width <= 0f)
            return measuredWidth;

        return MathF.Max(measuredWidth, width);
    }

    public static float GetContentOffsetX(string layoutKey, float slotWidth, float contentWidth)
    {
        if (!IsWidthEnabled(layoutKey))
            return 0f;

        var extra = MathF.Max(0f, slotWidth - contentWidth);
        if (extra <= 0f)
            return 0f;

        if (C.OverlayLayoutMode != OverlayLayoutMode.Vertical)
            return extra * 0.5f;

        return C.OverlayVerticalAlignment == OverlayVerticalAlignment.Right ? extra : 0f;
    }

    public static Vector4 GetTextColor(string layoutKey) =>
        DtrSeparatorStyle.IsSeparatorKey(layoutKey)
            ? DtrSeparatorStyle.GetTextColor(layoutKey)
            : IsColorEnabled(layoutKey) && C.FixedWidthTextColors.TryGetValue(layoutKey, out var color)
                ? color
                : GetDefaultTextColor();

    public static Vector4 GetOutlineColor(string layoutKey) =>
        DtrSeparatorStyle.IsSeparatorKey(layoutKey)
            ? DtrSeparatorStyle.GetOutlineColor(layoutKey)
            : IsColorEnabled(layoutKey) && C.FixedWidthOutlineColors.TryGetValue(layoutKey, out var color)
                ? color
                : GetDefaultOutlineColor();

    public static void SetWidthEnabled(string layoutKey, bool enabled)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        if (enabled)
        {
            C.FixedWidthEnabledIds.Add(layoutKey);
            if (!C.FixedWidthPixels.ContainsKey(layoutKey))
                C.FixedWidthPixels[layoutKey] = DefaultWidthPixels;

            return;
        }

        C.FixedWidthEnabledIds.Remove(layoutKey);
    }

    public static void ResetColorsToDefault(string layoutKey)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        if (OverlayEntryIds.IsAppearanceText(layoutKey))
        {
            C.TextColor = DtrStyle.DefaultTextColor;
            C.OutlineColor = DtrStyle.DefaultOutlineColor;
            return;
        }

        C.FixedWidthTextColors[layoutKey] = GetDefaultTextColor();
        C.FixedWidthOutlineColors[layoutKey] = GetDefaultOutlineColor();
    }

    public static void SetColorEnabled(string layoutKey, bool enabled)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        if (enabled)
        {
            C.FixedColorEnabledIds.Add(layoutKey);

            if (OverlayEntryIds.IsAppearanceText(layoutKey))
                return;

            if (!C.FixedWidthTextColors.ContainsKey(layoutKey))
                C.FixedWidthTextColors[layoutKey] = GetDefaultTextColor();

            if (!C.FixedWidthOutlineColors.ContainsKey(layoutKey))
                C.FixedWidthOutlineColors[layoutKey] = GetDefaultOutlineColor();

            return;
        }

        C.FixedColorEnabledIds.Remove(layoutKey);
    }

}
