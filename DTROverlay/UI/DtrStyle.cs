using System.Numerics;

namespace DTROverlay.UI;

internal static class DtrStyle
{
    public const float EntrySpacing = 5f;
    /// <summary>SameLine gap between a native icon/image and its paired text (matches tight native DTR grouping).</summary>
    public const float IconAdjacentSpacing = 0f;
    /// <summary>Gap between ET/LT icon and the clock time digits (slightly wider than other icon pairs).</summary>
    public const float ClockIconTimeSpacing = 3f;
    public const float IconHeightRatio = 1.3f;
    /// <summary>Icon height vs text line when following vanilla DTR (native icons match row, not 1.3x).</summary>
    public const float FollowVanillaIconHeightRatio = 1f;
    public const float WalkIconScale = 1.15f;
    public const float EditModeBackgroundAlpha = 0.45f;
    public const float VerticalOffset = 1f;
    /// <summary>Default Y nudge (screen pixels) when Follow Vanilla DTR aligns the plugin row.</summary>
    public const float DefaultFollowVanillaVerticalOffset = 2f;
    /// <summary>Scales native glyph target down so ImGui text visually matches game fonts.</summary>
    public const float FollowVanillaFontVisualMatch = 0.88f;
    /// <summary>
    /// ImGui font cap height as a fraction of the native DTR collision row height (~24px).
    /// Min-glyph sizing (see <see cref="FollowVanillaFontVisualMatch"/>) shrinks in duty when WorldInfo hides.
    /// Calibrated as FollowVanillaFontVisualMatch × (typical digit cap height / row height).
    /// </summary>
    public const float FollowVanillaFontRowHeightRatio = FollowVanillaFontVisualMatch * (17f / 24f);
    public const float TooltipBackgroundAlpha = 0.6f;
    public const float TooltipWindowRounding = 6f;
    public const float TooltipEntrySpacing = 4f;
    public static readonly Vector2 TooltipWindowPadding = new(6f, 4f);
    public const float DefaultTooltipFontSizePx = 18f;
    public static readonly Vector4 DefaultTooltipTextColor = new(1f, 1f, 1f, 1f);
    public static readonly Vector4 DefaultTooltipBackgroundColor = new(0f, 0f, 0f, TooltipBackgroundAlpha);

    public const ImGuiColorEditFlags ColorEditFlags = ImGuiColorEditFlags.NoInputs;

    public const float DefaultOverlayFontSizeScale = 1.1f;
    public const float DefaultFollowVanillaFontSizeScale = 1f;

    /// <summary>Origin palette (final fallback when Default style columns are off).</summary>
    public static readonly Vector4 OriginTextColor = new(1f, 1f, 1f, 1f);
    public static readonly Vector4 OriginOutlineColor = new(1f, 1f, 0f, 1f);
    public static readonly Vector4 OriginShadowColor = new(0xB0 / 255f, 0x6F / 255f, 0f, 0x78 / 255f);

    public const bool OriginEdgeEnabled = true;
    public const bool OriginShadowEnabled = true;
    public const float OriginEdgeStrength = 0.02f;
    public const float OriginShadowThickness = 2.5f;

    /// <summary>Legacy name for <see cref="OriginTextColor"/>.</summary>
    public static readonly Vector4 DefaultTextColor = OriginTextColor;
    public static readonly Vector4 DefaultOutlineColor = OriginOutlineColor;
    public static readonly Vector4 DefaultShadowColor = OriginShadowColor;
    public const bool DefaultEdgeEnabled = OriginEdgeEnabled;
    public const bool DefaultShadowEnabled = OriginShadowEnabled;
    public const float DefaultEdgeStrength = OriginEdgeStrength;
    public const float DefaultShadowThickness = OriginShadowThickness;
    public const float MaxEdgeStrength = 1f;
    public const float MaxShadowThickness = 4f;
    public const float MaxSoftShadowRadius = 4f;
    /// <summary>Per-sample alpha scale so stacked soft-shadow passes match a single crisp shadow.</summary>
    public const float SoftShadowAccumulation = 0.45f;

    public static readonly Vector2[] OutlineOffsets =
    [
        new(-0.1f, 0f),
        new(0.1f, 0f),
        new(0f, -0.1f),
        new(0f, 0.1f),
    ];
}
