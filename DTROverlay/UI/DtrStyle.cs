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
    public const float TooltipBackgroundAlpha = 0.6f;
    public const float TooltipWindowRounding = 6f;
    public static readonly Vector2 TooltipWindowPadding = new(10f, 8f);

    public const ImGuiColorEditFlags ColorEditFlags = ImGuiColorEditFlags.NoInputs;

    public const float DefaultOverlayFontSizeScale = 1.1f;
    public const float DefaultFollowVanillaFontSizeScale = 1f;

    public static readonly Vector4 DefaultTextColor = new(1f, 1f, 1f, 1f);
    public static readonly Vector4 DefaultOutlineColor = new(0xC8 / 255f, 0x84 / 255f, 0x01 / 255f, 1f);

    public static readonly Vector2[] OutlineOffsets =
    [
        new(-0.1f, 0f),
        new(0.1f, 0f),
        new(0f, -0.1f),
        new(0f, 0.1f),
    ];
}
