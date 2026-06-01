using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class EntryFixedWidth
{
    public static bool IsTextColorEnabled(string layoutKey) =>
        !string.IsNullOrEmpty(layoutKey) && C.FixedTextColorEnabledIds.Contains(layoutKey);

    public static bool IsEdgeStyleEnabled(string layoutKey)
    {
        C.FixedEdgeStyleEnabledIds ??= [];
        return !string.IsNullOrEmpty(layoutKey) && C.FixedEdgeStyleEnabledIds.Contains(layoutKey);
    }

    public static bool IsShadowStyleEnabled(string layoutKey)
    {
        C.FixedShadowStyleEnabledIds ??= [];
        return !string.IsNullOrEmpty(layoutKey) && C.FixedShadowStyleEnabledIds.Contains(layoutKey);
    }

    public static Vector4 GetStoredTextColor(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            return C.TextColor;

        if (C.FixedWidthTextColors.TryGetValue(layoutKey, out var color))
            return color;

        return DtrStyle.OriginTextColor;
    }

    public static Vector4 GetStoredOutlineColor(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            return C.OutlineColor;

        if (C.FixedWidthOutlineColors.TryGetValue(layoutKey, out var color))
            return color;

        return DtrStyle.OriginOutlineColor;
    }

    public static Vector4 GetStoredShadowColor(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            return C.ShadowColor;

        if (C.FixedWidthShadowColors.TryGetValue(layoutKey, out var color))
            return color;

        return DtrStyle.OriginShadowColor;
    }

    public static float GetStoredEdgeStrength(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            return C.EdgeStrength;

        if (C.FixedWidthEdgeStrengths.TryGetValue(layoutKey, out var strength))
            return strength;

        return DtrStyle.OriginEdgeStrength;
    }

    public static float GetStoredShadowThickness(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            return C.ShadowThickness;

        if (C.FixedWidthShadowThicknesses.TryGetValue(layoutKey, out var thickness))
            return thickness;

        return DtrStyle.OriginShadowThickness;
    }

    public static float ResolveWidth(string layoutKey, float measuredWidth)
    {
        if (DtrSeparatorStyle.IsSeparatorKey(layoutKey))
            return DtrSeparatorStyle.ResolveSlotWidth(layoutKey, measuredWidth);

        return DtrEntrySlotWidth.ResolveWidth(layoutKey, measuredWidth);
    }

    public static float GetContentOffsetX(string layoutKey, float slotWidth, float contentWidth)
    {
        var extra = MathF.Max(0f, slotWidth - contentWidth);
        if (extra <= 0f)
            return 0f;

        if (OverlayGroupLayout.GetLayoutMode() != OverlayLayoutMode.Vertical)
            return extra * 0.5f;

        return OverlayGroupLayout.GetVerticalAlignment() == OverlayVerticalAlignment.Right ? extra : 0f;
    }

    public static Vector4 GetTextColor(string layoutKey) =>
        OverlayStyleResolver.GetTextColor(layoutKey);

    public static Vector4 GetOutlineColor(string layoutKey) =>
        OverlayStyleResolver.GetOutlineColor(layoutKey);

    public static Vector4 GetShadowColor(string layoutKey) =>
        OverlayStyleResolver.GetShadowColor(layoutKey);

    public static float GetEdgeStrength(string layoutKey) =>
        OverlayStyleResolver.GetEdgeStrength(layoutKey);

    public static float GetShadowThickness(string layoutKey) =>
        OverlayStyleResolver.GetShadowThickness(layoutKey);

    public static bool IsEdgeEnabled(string layoutKey) =>
        OverlayStyleResolver.IsEdgeEnabled(layoutKey);

    public static bool IsShadowEnabled(string layoutKey) =>
        OverlayStyleResolver.IsShadowEnabled(layoutKey);

    public static void ResetTextStyleToDefault(string layoutKey)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        layoutKey = OverlayStyleResolver.NormalizeLayoutKey(layoutKey);

        if (OverlayEntryIds.IsDefaultText(layoutKey))
        {
            OverlayStyleResolver.ResetDefaultTextToOrigin();
            return;
        }

        if (OverlayEntryIds.IsDefaultSeparator(layoutKey))
        {
            OverlayStyleResolver.ResetDefaultSeparatorToOrigin();
            return;
        }

        if (GroupStyleKeys.IsOverridePluginTextKey(layoutKey))
        {
            var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetSelected();
            OverlayStyleResolver.ResetOverrideTextToDefault(group);
            return;
        }

        if (GroupStyleKeys.IsOverrideNativeTextKey(layoutKey))
        {
            var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetSelected();
            OverlayStyleResolver.ResetOverrideNativeTextToDefault(group);
            return;
        }

        if (GroupStyleKeys.IsOverridePluginSeparatorKey(layoutKey))
        {
            var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetSelected();
            OverlayStyleResolver.ResetOverrideSeparatorToDefault(group);
            return;
        }

        if (GroupStyleKeys.IsOverrideNativeSeparatorKey(layoutKey))
        {
            var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetSelected();
            OverlayStyleResolver.ResetOverrideNativeSeparatorToDefault(group);
            return;
        }

        if (GroupStyleKeys.IsOverrideDivisionSeparatorKey(layoutKey))
        {
            var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetSelected();
            OverlayStyleResolver.ResetOverrideDivisionSeparatorToDefault(group);
            return;
        }

        OverlayStyleResolver.ResetPluginTextToOverride(layoutKey);
    }

    public static void ResetEdgeStyleToDefault(string layoutKey)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        var (outline, edgeEnabled, edgeStrength) = GetParentEdgeStored(layoutKey);
        if (OverlayEntryIds.IsDefaultText(layoutKey))
        {
            C.OutlineColor = outline;
            C.EdgeEnabled = edgeEnabled;
            C.EdgeStrength = edgeStrength;
            return;
        }

        C.FixedWidthOutlineColors[layoutKey] = outline;
        C.FixedWidthEdgeEnabled[layoutKey] = edgeEnabled;
        C.FixedWidthEdgeStrengths[layoutKey] = edgeStrength;
    }

    public static void ResetShadowStyleToDefault(string layoutKey)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        var (shadow, shadowEnabled, shadowThickness) = GetParentShadowStored(layoutKey);
        if (OverlayEntryIds.IsDefaultText(layoutKey))
        {
            C.ShadowColor = shadow;
            C.ShadowEnabled = shadowEnabled;
            C.ShadowThickness = shadowThickness;
            return;
        }

        C.FixedWidthShadowColors[layoutKey] = shadow;
        C.FixedWidthShadowEnabled[layoutKey] = shadowEnabled;
        C.FixedWidthShadowThicknesses[layoutKey] = shadowThickness;
    }

    private static (Vector4 Outline, bool EdgeEnabled, float EdgeStrength) GetParentEdgeStored(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey) || OverlayEntryIds.IsDefaultSeparator(layoutKey))
            return (DtrStyle.OriginOutlineColor, DtrStyle.OriginEdgeEnabled, DtrStyle.OriginEdgeStrength);

        if (GroupStyleKeys.IsOverridePluginTextKey(layoutKey) || GroupStyleKeys.IsOverrideNativeTextKey(layoutKey))
            return (C.OutlineColor, C.EdgeEnabled, C.EdgeStrength);

        if (GroupStyleKeys.IsOverridePluginSeparatorKey(layoutKey) || GroupStyleKeys.IsOverrideNativeSeparatorKey(layoutKey))
        {
            var defaultSep = OverlayEntryIds.DefaultSeparator;
            return (
                GetStoredOutlineColor(defaultSep),
                GetEdgeEnabledStored(defaultSep),
                GetStoredEdgeStrength(defaultSep));
        }

        if (GroupStyleKeys.IsOverrideDivisionSeparatorKey(layoutKey))
        {
            var divisionSep = OverlayEntryIds.DivisionSeparatorColor;
            return (
                GetStoredOutlineColor(divisionSep),
                GetEdgeEnabledStored(divisionSep),
                GetStoredEdgeStrength(divisionSep));
        }

        var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetSelected();
        var overrideKey = GroupStyleKeys.OverrideText(group.Id);
        return (GetStoredOutlineColor(overrideKey), GetEdgeEnabledStored(overrideKey), GetStoredEdgeStrength(overrideKey));
    }

    private static (Vector4 Shadow, bool ShadowEnabled, float Thickness) GetParentShadowStored(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey) || OverlayEntryIds.IsDefaultSeparator(layoutKey))
            return (DtrStyle.OriginShadowColor, DtrStyle.OriginShadowEnabled, DtrStyle.OriginShadowThickness);

        if (GroupStyleKeys.IsOverridePluginTextKey(layoutKey) || GroupStyleKeys.IsOverrideNativeTextKey(layoutKey))
            return (C.ShadowColor, C.ShadowEnabled, C.ShadowThickness);

        if (GroupStyleKeys.IsOverridePluginSeparatorKey(layoutKey) || GroupStyleKeys.IsOverrideNativeSeparatorKey(layoutKey))
        {
            var defaultSep = OverlayEntryIds.DefaultSeparator;
            return (
                GetStoredShadowColor(defaultSep),
                GetShadowEnabledStored(defaultSep),
                GetStoredShadowThickness(defaultSep));
        }

        if (GroupStyleKeys.IsOverrideDivisionSeparatorKey(layoutKey))
        {
            var divisionSep = OverlayEntryIds.DivisionSeparatorColor;
            return (
                GetStoredShadowColor(divisionSep),
                GetShadowEnabledStored(divisionSep),
                GetStoredShadowThickness(divisionSep));
        }

        var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetSelected();
        var overrideKey = GroupStyleKeys.OverrideText(group.Id);
        return (GetStoredShadowColor(overrideKey), GetShadowEnabledStored(overrideKey), GetStoredShadowThickness(overrideKey));
    }

    private static bool GetEdgeEnabledStored(string layoutKey) =>
        OverlayEntryIds.IsDefaultText(layoutKey) ? C.EdgeEnabled : C.FixedWidthEdgeEnabled.GetValueOrDefault(layoutKey, C.EdgeEnabled);

    private static bool GetShadowEnabledStored(string layoutKey) =>
        OverlayEntryIds.IsDefaultText(layoutKey) ? C.ShadowEnabled : C.FixedWidthShadowEnabled.GetValueOrDefault(layoutKey, C.ShadowEnabled);

    public static void ResetColorsToDefault(string layoutKey) =>
        ResetTextStyleToDefault(layoutKey);

    public static void SetTextColorEnabled(string layoutKey, bool enabled)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        C.FixedTextColorEnabledIds ??= [];

        if (enabled)
        {
            C.FixedTextColorEnabledIds.Add(layoutKey);

            if (OverlayEntryIds.IsDefaultText(layoutKey))
                return;

            if (!C.FixedWidthTextColors.ContainsKey(layoutKey))
                C.FixedWidthTextColors[layoutKey] = OverlayStyleResolver.GetDefaultTextColorEffective();

            return;
        }

        C.FixedTextColorEnabledIds.Remove(layoutKey);
    }

    public static void SetEdgeStyleEnabled(string layoutKey, bool enabled)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        C.FixedEdgeStyleEnabledIds ??= [];

        if (enabled)
        {
            C.FixedEdgeStyleEnabledIds.Add(layoutKey);
            SeedEdgeStyle(layoutKey);
            return;
        }

        C.FixedEdgeStyleEnabledIds.Remove(layoutKey);
    }

    public static void SetShadowStyleEnabled(string layoutKey, bool enabled)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return;

        C.FixedShadowStyleEnabledIds ??= [];

        if (enabled)
        {
            C.FixedShadowStyleEnabledIds.Add(layoutKey);
            SeedShadowStyle(layoutKey);
            return;
        }

        C.FixedShadowStyleEnabledIds.Remove(layoutKey);
    }

    private static void SeedEdgeStyle(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            return;

        var parent = GroupStyleKeys.IsOverrideKey(layoutKey)
            ? OverlayStyleResolver.GetOverrideTextColorEffective()
            : OverlayStyleResolver.GetDefaultTextColorEffective();

        if (!C.FixedWidthOutlineColors.ContainsKey(layoutKey))
            C.FixedWidthOutlineColors[layoutKey] = parent;

        if (!C.FixedWidthEdgeEnabled.ContainsKey(layoutKey))
            C.FixedWidthEdgeEnabled[layoutKey] = OverlayStyleResolver.IsEdgeEnabled(OverlayEntryIds.DefaultText);

        if (!C.FixedWidthEdgeStrengths.ContainsKey(layoutKey))
            C.FixedWidthEdgeStrengths[layoutKey] = OverlayStyleResolver.GetEdgeStrength(OverlayEntryIds.DefaultText);
    }

    private static void SeedShadowStyle(string layoutKey)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
            return;

        var parent = GroupStyleKeys.IsOverrideKey(layoutKey)
            ? OverlayStyleResolver.GetOverrideTextColorEffective()
            : OverlayStyleResolver.GetDefaultTextColorEffective();

        if (!C.FixedWidthShadowColors.ContainsKey(layoutKey))
            C.FixedWidthShadowColors[layoutKey] = parent;

        if (!C.FixedWidthShadowEnabled.ContainsKey(layoutKey))
            C.FixedWidthShadowEnabled[layoutKey] = OverlayStyleResolver.IsShadowEnabled(OverlayEntryIds.DefaultText);

        if (!C.FixedWidthShadowThicknesses.ContainsKey(layoutKey))
            C.FixedWidthShadowThicknesses[layoutKey] = OverlayStyleResolver.GetShadowThickness(OverlayEntryIds.DefaultText);
    }
}
