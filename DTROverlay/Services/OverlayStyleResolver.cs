using DTROverlay.UI;

namespace DTROverlay.Services;

internal static class OverlayStyleResolver
{
    /// <summary>
    /// Effective font scale for the current overlay group. Never stacks multiple tiers.
    /// Follow Vanilla (<see cref="Configuration.FollowVanillaDtr"/>) → vanilla match × <see cref="Configuration.FollowVanillaFontSizeScale"/>; group font override ignored.
    /// Group override enabled → <see cref="DtrOverlayGroup.OverrideFontSizeScale"/> only (default scale ignored).
    /// Otherwise → <see cref="Configuration.OverlayFontSizeScale"/> (Default Style).
    /// </summary>
    public static float GetEffectiveOverlayFontScale()
    {
        if (C.FollowVanillaDtr)
            return FollowVanillaFontScale.GetVanillaPluginScale();

        var group = OverlayStyleContext.Group;
        if (group is { OverrideFontSizeScaleEnabled: true })
            return group.OverrideFontSizeScale;

        return C.OverlayFontSizeScale;
    }

    public static int GetEffectiveSeparatorSlotWidthPx()
    {
        var group = OverlayStyleContext.Group;
        if (group is { OverrideSeparatorSlotWidthPxEnabled: true })
            return group.OverrideSeparatorSlotWidthPx;

        return C.SeparatorSlotWidthPx;
    }

    public static Vector4 GetTextColor(string layoutKey) =>
        ResolveStyle(layoutKey).Text;

    public static Vector4 GetOutlineColor(string layoutKey) =>
        ResolveStyle(layoutKey).Outline;

    public static Vector4 GetShadowColor(string layoutKey) =>
        ResolveStyle(layoutKey).Shadow;

    public static float GetEdgeStrength(string layoutKey) =>
        ResolveStyle(layoutKey).EdgeStrength;

    public static float GetShadowThickness(string layoutKey) =>
        ResolveStyle(layoutKey).ShadowThickness;

    public static bool IsEdgeEnabled(string layoutKey) =>
        ResolveStyle(layoutKey).EdgeEnabled;

    public static bool IsShadowEnabled(string layoutKey) =>
        ResolveStyle(layoutKey).ShadowEnabled;

    public static Vector4 GetDefaultTextColorEffective() =>
        BuildDefaultTextStyle().Text;

    public static Vector4 GetOverrideTextColorEffective()
    {
        var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetSelected();
        return BuildOverrideTextStyle(GroupStyleKeys.OverrideText(group.Id)).Text;
    }

    public static void ResetDefaultTextToOrigin()
    {
        C.TextColor = DtrStyle.OriginTextColor;
        C.OutlineColor = DtrStyle.OriginOutlineColor;
        C.ShadowColor = DtrStyle.OriginShadowColor;
        C.EdgeEnabled = DtrStyle.OriginEdgeEnabled;
        C.ShadowEnabled = DtrStyle.OriginShadowEnabled;
        C.EdgeStrength = DtrStyle.OriginEdgeStrength;
        C.ShadowThickness = DtrStyle.OriginShadowThickness;
    }

    public static void ResetDefaultSeparatorToOrigin() =>
        CopyStyleToStored(OverlayEntryIds.DefaultSeparator, BuildOriginStyle());

    public static void ResetOverrideTextToDefault(DtrOverlayGroup group)
    {
        var defaultStyle = BuildDefaultTextStyle();
        CopyStyleToStored(GroupStyleKeys.OverrideText(group.Id), defaultStyle);
    }

    public static void ResetOverrideSeparatorToDefault(DtrOverlayGroup group)
    {
        var defaultStyle = BuildDefaultSeparatorStyle();
        CopyStyleToStored(GroupStyleKeys.OverrideSeparator(group.Id), defaultStyle);
    }

    public static void ResetOverrideNativeTextToDefault(DtrOverlayGroup group)
    {
        var defaultStyle = BuildDefaultTextStyle();
        CopyStyleToStored(GroupStyleKeys.OverrideNativeText(group.Id), defaultStyle);
    }

    public static void ResetOverrideNativeSeparatorToDefault(DtrOverlayGroup group)
    {
        var defaultStyle = BuildDefaultSeparatorStyle();
        CopyStyleToStored(GroupStyleKeys.OverrideNativeSeparator(group.Id), defaultStyle);
    }

    public static void ResetOverrideDivisionSeparatorToDefault(DtrOverlayGroup group)
    {
        var defaultStyle = BuildStoredOrInherit(
            OverlayEntryIds.DivisionSeparatorColor,
            BuildDefaultSeparatorStyle());
        CopyStyleToStored(GroupStyleKeys.OverrideDivisionSeparator(group.Id), defaultStyle);
    }

    public static void ResetPluginTextToOverride(string layoutKey) =>
        CopyStyleToStored(NormalizeLayoutKey(layoutKey), BuildOverrideTextStyleForCurrentGroup());

    private static StyleSnapshot ResolveStyle(string layoutKey)
    {
        layoutKey = NormalizeLayoutKey(layoutKey);

        if (DtrSeparatorStyle.IsSeparatorKey(layoutKey))
            return ResolveSeparatorStyle(layoutKey);

        if (OverlayEntryIds.IsDefaultText(layoutKey))
            return BuildDefaultTextStyle();

        if (OverlayEntryIds.IsDefaultSeparator(layoutKey))
            return BuildDefaultSeparatorStyle();

        if (GroupStyleKeys.IsOverrideKey(layoutKey))
        {
            return GroupStyleKeys.IsOverrideSeparatorKey(layoutKey)
                ? BuildOverrideSeparatorStyle(layoutKey)
                : BuildOverrideTextStyle(layoutKey);
        }

        if (OverlayEntryIds.IsNativeGroupColor(layoutKey))
            return BuildNativeTextStyleForCurrentGroup();

        if (GroupStyleKeys.IsPluginEntryKey(layoutKey))
            return BuildPluginStyle(layoutKey);

        return BuildDefaultTextStyle();
    }

    internal static string NormalizeLayoutKey(string layoutKey)
    {
        if (string.IsNullOrEmpty(layoutKey))
            return layoutKey;

        if (GroupStyleKeys.IsPluginEntryKey(layoutKey)
            || GroupStyleKeys.IsOverrideKey(layoutKey)
            || OverlayEntryIds.IsDefaultText(layoutKey)
            || OverlayEntryIds.IsDefaultSeparator(layoutKey)
            || DtrSeparatorStyle.IsSeparatorKey(layoutKey)
            || OverlayEntryIds.IsNative(layoutKey))
            return layoutKey;

        return GroupStyleKeys.ResolvePluginEntryKey(layoutKey);
    }

    private static StyleSnapshot ResolveSeparatorStyle(string physicalKey)
    {
        var group = OverlayStyleContext.Group;

        if (physicalKey == OverlayEntryIds.DivisionSeparatorColor)
        {
            if (group != null && DtrOverlayGroups.IsMergedDefaultPanelGroup(group))
                return BuildOverrideSeparatorStyle(GroupStyleKeys.OverrideDivisionSeparator(group.Id));

            return BuildStoredOrInherit(physicalKey, BuildDefaultSeparatorStyle());
        }

        if (group != null)
        {
            if (physicalKey == OverlayEntryIds.NativeSeparatorColor)
            {
                if (DtrOverlayGroups.IsNativeGroup(group))
                    return BuildOverrideSeparatorStyle(GroupStyleKeys.OverrideSeparator(group.Id));

                if (DtrOverlayGroups.IsMergedDefaultPanelGroup(group))
                    return BuildOverrideSeparatorStyle(GroupStyleKeys.OverrideNativeSeparator(group.Id));
            }

            if (physicalKey == OverlayEntryIds.PluginSeparatorColor && !DtrOverlayGroups.IsNativeGroup(group))
                return BuildOverrideSeparatorStyle(GroupStyleKeys.OverrideSeparator(group.Id));
        }

        return BuildDefaultSeparatorStyle();
    }

    private static StyleSnapshot BuildNativeTextStyleForCurrentGroup() =>
        BuildOverrideTextStyle(OverlayStyleKeys.GetNativeTextColorLayoutKey());

    private static StyleSnapshot BuildPluginStyle(string layoutKey)
    {
        var overrideStyle = BuildOverrideTextStyleForCurrentGroup();
        if (!HasAnyPluginStyleEnabled(layoutKey))
            return overrideStyle;

        return new StyleSnapshot
        {
            Text = EntryFixedWidth.IsTextColorEnabled(layoutKey)
                && C.FixedWidthTextColors.TryGetValue(layoutKey, out var text)
                    ? text
                    : overrideStyle.Text,
            Outline = EntryFixedWidth.IsEdgeStyleEnabled(layoutKey)
                && C.FixedWidthOutlineColors.TryGetValue(layoutKey, out var outline)
                    ? outline
                    : overrideStyle.Outline,
            Shadow = EntryFixedWidth.IsShadowStyleEnabled(layoutKey)
                && C.FixedWidthShadowColors.TryGetValue(layoutKey, out var shadow)
                    ? shadow
                    : overrideStyle.Shadow,
            EdgeStrength = EntryFixedWidth.IsEdgeStyleEnabled(layoutKey)
                && C.FixedWidthEdgeStrengths.TryGetValue(layoutKey, out var edgeStrength)
                    ? edgeStrength
                    : overrideStyle.EdgeStrength,
            ShadowThickness = EntryFixedWidth.IsShadowStyleEnabled(layoutKey)
                && C.FixedWidthShadowThicknesses.TryGetValue(layoutKey, out var shadowThickness)
                    ? shadowThickness
                    : overrideStyle.ShadowThickness,
            EdgeEnabled = EntryFixedWidth.IsEdgeStyleEnabled(layoutKey)
                && C.FixedWidthEdgeEnabled.TryGetValue(layoutKey, out var edgeEnabled)
                    ? edgeEnabled
                    : overrideStyle.EdgeEnabled,
            ShadowEnabled = EntryFixedWidth.IsShadowStyleEnabled(layoutKey)
                && C.FixedWidthShadowEnabled.TryGetValue(layoutKey, out var shadowEnabled)
                    ? shadowEnabled
                    : overrideStyle.ShadowEnabled,
        };
    }

    private static bool HasAnyPluginStyleEnabled(string layoutKey) =>
        EntryFixedWidth.IsTextColorEnabled(layoutKey)
            || EntryFixedWidth.IsEdgeStyleEnabled(layoutKey)
            || EntryFixedWidth.IsShadowStyleEnabled(layoutKey);

    private static StyleSnapshot BuildOverrideTextStyleForCurrentGroup()
    {
        var group = OverlayStyleContext.Group ?? DtrOverlayGroups.GetDefaultGroup();
        return BuildOverrideTextStyle(GroupStyleKeys.OverrideText(group.Id));
    }

    private static StyleSnapshot BuildOverrideTextStyle(string layoutKey) =>
        BuildStoredOrInherit(layoutKey, BuildDefaultTextStyle());

    private static StyleSnapshot BuildOverrideSeparatorStyle(string layoutKey) =>
        BuildStoredOrInherit(layoutKey, GetOverrideSeparatorParent(layoutKey));

    private static StyleSnapshot GetOverrideSeparatorParent(string layoutKey)
    {
        if (GroupStyleKeys.IsOverrideDivisionSeparatorKey(layoutKey))
        {
            return BuildStoredOrInherit(
                OverlayEntryIds.DivisionSeparatorColor,
                BuildDefaultSeparatorStyle());
        }

        return BuildDefaultSeparatorStyle();
    }

    private static StyleSnapshot BuildDefaultTextStyle() =>
        BuildStoredOrInherit(OverlayEntryIds.DefaultText, BuildOriginStyle());

    private static StyleSnapshot BuildDefaultSeparatorStyle() =>
        BuildStoredOrInherit(OverlayEntryIds.DefaultSeparator, BuildOriginStyle());

    private static StyleSnapshot BuildStoredOrInherit(string layoutKey, StyleSnapshot parent)
    {
        var textEnabled = EntryFixedWidth.IsTextColorEnabled(layoutKey);
        var edgeEnabled = EntryFixedWidth.IsEdgeStyleEnabled(layoutKey);
        var shadowEnabled = EntryFixedWidth.IsShadowStyleEnabled(layoutKey);

        return new StyleSnapshot
        {
            Text = textEnabled && TryGetStoredText(layoutKey, out var text) ? text : parent.Text,
            Outline = edgeEnabled && TryGetStoredOutline(layoutKey, out var outline) ? outline : parent.Outline,
            Shadow = shadowEnabled && TryGetStoredShadow(layoutKey, out var shadow) ? shadow : parent.Shadow,
            EdgeStrength = edgeEnabled && C.FixedWidthEdgeStrengths.TryGetValue(layoutKey, out var edgeStrength)
                ? edgeStrength
                : parent.EdgeStrength,
            ShadowThickness = shadowEnabled && C.FixedWidthShadowThicknesses.TryGetValue(layoutKey, out var thickness)
                ? thickness
                : parent.ShadowThickness,
            EdgeEnabled = edgeEnabled && C.FixedWidthEdgeEnabled.TryGetValue(layoutKey, out var edgeOn)
                ? edgeOn
                : parent.EdgeEnabled,
            ShadowEnabled = shadowEnabled && C.FixedWidthShadowEnabled.TryGetValue(layoutKey, out var shadowOn)
                ? shadowOn
                : parent.ShadowEnabled,
        };
    }

    private static StyleSnapshot BuildOriginStyle() =>
        new()
        {
            Text = DtrStyle.OriginTextColor,
            Outline = DtrStyle.OriginOutlineColor,
            Shadow = DtrStyle.OriginShadowColor,
            EdgeStrength = DtrStyle.OriginEdgeStrength,
            ShadowThickness = DtrStyle.OriginShadowThickness,
            EdgeEnabled = DtrStyle.OriginEdgeEnabled,
            ShadowEnabled = DtrStyle.OriginShadowEnabled,
        };

    private static bool TryGetStoredText(string layoutKey, out Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
        {
            color = C.TextColor;
            return true;
        }

        return C.FixedWidthTextColors.TryGetValue(layoutKey, out color);
    }

    private static bool TryGetStoredOutline(string layoutKey, out Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
        {
            color = C.OutlineColor;
            return true;
        }

        return C.FixedWidthOutlineColors.TryGetValue(layoutKey, out color);
    }

    private static bool TryGetStoredShadow(string layoutKey, out Vector4 color)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
        {
            color = C.ShadowColor;
            return true;
        }

        return C.FixedWidthShadowColors.TryGetValue(layoutKey, out color);
    }

    private static void CopyStyleToStored(string layoutKey, StyleSnapshot style)
    {
        if (OverlayEntryIds.IsDefaultText(layoutKey))
        {
            C.TextColor = style.Text;
            C.OutlineColor = style.Outline;
            C.ShadowColor = style.Shadow;
            C.EdgeEnabled = style.EdgeEnabled;
            C.ShadowEnabled = style.ShadowEnabled;
            C.EdgeStrength = style.EdgeStrength;
            C.ShadowThickness = style.ShadowThickness;
            return;
        }

        C.FixedWidthTextColors[layoutKey] = style.Text;
        C.FixedWidthOutlineColors[layoutKey] = style.Outline;
        C.FixedWidthShadowColors[layoutKey] = style.Shadow;
        C.FixedWidthEdgeStrengths[layoutKey] = style.EdgeStrength;
        C.FixedWidthShadowThicknesses[layoutKey] = style.ShadowThickness;
        C.FixedWidthEdgeEnabled[layoutKey] = style.EdgeEnabled;
        C.FixedWidthShadowEnabled[layoutKey] = style.ShadowEnabled;
    }

    private struct StyleSnapshot
    {
        public Vector4 Text;
        public Vector4 Outline;
        public Vector4 Shadow;
        public float EdgeStrength;
        public float ShadowThickness;
        public bool EdgeEnabled;
        public bool ShadowEnabled;
    }
}
