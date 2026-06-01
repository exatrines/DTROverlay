using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace DTROverlay.Services;

public readonly record struct DtrOverlayContent(
    IReadOnlyList<VisibleDtrEntry> OrderedEntries,
    IReadOnlyList<VisibleDtrEntry> NativeEntries,
    IReadOnlyList<VisibleDtrEntry> PluginEntries)
{
    public bool IsEmpty => OrderedEntries.Count == 0;
}

public enum VisibleDtrEntryKind
{
    Text,
    SeString,
    Image,
}

public readonly record struct VisibleDtrEntry
{
    public VisibleDtrEntryKind Kind { get; init; }

    public string Text { get; init; }

    public byte[] SeStringData { get; init; }

    public DtrImageInfo Image { get; init; }

    public float? SameLineSpacingBefore { get; init; }

    public Action<DtrInteractionEvent> OnClick { get; init; }

    public float Opacity { get; init; }

    public float ImageScale { get; init; }

    public string DtrEntryTitle { get; init; }

    public string LayoutKey { get; init; }

    public string ColorLayoutKey { get; init; }

    public string HoverTooltip { get; init; }

    public byte[] HoverTooltipSeStringData { get; init; }

    public PluginAffixRole AffixRole { get; init; }

    public bool HasVisibleContent =>
        Kind switch
        {
            VisibleDtrEntryKind.SeString => SeStringData is { Length: > 0 },
            VisibleDtrEntryKind.Text => !string.IsNullOrEmpty(Text),
            VisibleDtrEntryKind.Image => true,
            _ => false,
        };

    public static VisibleDtrEntry FromPluginAffix(
        string text,
        string entryTitle,
        PluginAffixRole role,
        Action<DtrInteractionEvent> onClick = null,
        byte[] hoverTooltipSeStringData = null,
        string colorLayoutKey = null)
    {
        text ??= string.Empty;

        return new()
        {
            Kind = VisibleDtrEntryKind.SeString,
            SeStringData = text.Length == 0 ? [] : new SeString(new TextPayload(text)).Encode(),
            Opacity = 1f,
            LayoutKey = colorLayoutKey ?? string.Empty,
            ColorLayoutKey = colorLayoutKey ?? entryTitle,
            DtrEntryTitle = entryTitle,
            AffixRole = role,
            OnClick = onClick,
            HoverTooltipSeStringData = hoverTooltipSeStringData ?? [],
        };
    }

    public static VisibleDtrEntry FromText(
        string text,
        float? sameLineSpacingBefore = null,
        float opacity = 1f,
        string layoutKey = null,
        string colorLayoutKey = null,
        string hoverTooltip = null) =>
        new()
        {
            Kind = VisibleDtrEntryKind.Text,
            Text = text,
            SameLineSpacingBefore = sameLineSpacingBefore,
            Opacity = opacity,
            LayoutKey = layoutKey ?? string.Empty,
            ColorLayoutKey = colorLayoutKey ?? string.Empty,
            HoverTooltip = hoverTooltip ?? string.Empty,
            AffixRole = PluginAffixRole.None,
        };

    public static VisibleDtrEntry FromSeString(
        SeString seString,
        Action<DtrInteractionEvent> onClick = null,
        string dtrEntryTitle = null,
        string layoutKey = null,
        SeString tooltip = null) =>
        new()
        {
            Kind = VisibleDtrEntryKind.SeString,
            SeStringData = seString.Encode(),
            OnClick = onClick,
            DtrEntryTitle = dtrEntryTitle ?? string.Empty,
            Opacity = 1f,
            LayoutKey = layoutKey ?? string.Empty,
            ColorLayoutKey = string.Empty,
            AffixRole = PluginAffixRole.None,
            HoverTooltipSeStringData = EncodeTooltip(tooltip),
        };

    public static VisibleDtrEntry FromIcon(
        SeIconChar icon,
        float? sameLineSpacingBefore = null,
        float opacity = 1f,
        string layoutKey = null,
        string colorLayoutKey = null,
        string hoverTooltip = null) =>
        FromText(icon.ToIconString(), sameLineSpacingBefore, opacity, layoutKey, colorLayoutKey, hoverTooltip);

    public static VisibleDtrEntry FromImage(
        DtrImageInfo image,
        float? sameLineSpacingBefore = null,
        float opacity = 1f,
        float imageScale = 1f,
        string layoutKey = null,
        string hoverTooltip = null) =>
        new()
        {
            Kind = VisibleDtrEntryKind.Image,
            Image = image,
            SameLineSpacingBefore = sameLineSpacingBefore,
            Opacity = opacity,
            ImageScale = imageScale,
            LayoutKey = layoutKey ?? string.Empty,
            ColorLayoutKey = string.Empty,
            HoverTooltip = hoverTooltip ?? string.Empty,
            AffixRole = PluginAffixRole.None,
        };

    private static byte[] EncodeTooltip(SeString tooltip) =>
        tooltip != null && !string.IsNullOrEmpty(tooltip.TextValue) ? tooltip.Encode() : [];
}
