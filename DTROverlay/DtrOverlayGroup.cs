using DTROverlay.UI;

namespace DTROverlay;

public sealed class DtrOverlayGroup
{
    public string Id = Guid.NewGuid().ToString("N");

    public string Name = "Group 1";

    public DtrOverlayGroupKind Kind = DtrOverlayGroupKind.Custom;

    public bool Enabled = true;

    public Vector2 OverlayPosition = new(20f, 10f);

    public OverlayPositionOrigin OverlayPositionOrigin = OverlayPositionOrigin.TopRight;

    public bool OverlayEditMode;

    public List<string> EntryOrder = [];

    public HashSet<string> HiddenEntryTitles = [];

    public bool OverrideFontSizeScaleEnabled;
    public float OverrideFontSizeScale = DtrStyle.DefaultOverlayFontSizeScale;

    public bool OverrideSeparatorSlotWidthPxEnabled;
    public int OverrideSeparatorSlotWidthPx;

    public bool OverrideTooltipPositionEnabled;
    public TooltipPosition OverrideTooltipPosition = TooltipPosition.Lower;

    public bool OverrideTooltipFontSizePxEnabled;
    public float OverrideTooltipFontSizePx = DtrStyle.DefaultTooltipFontSizePx;

    public bool OverrideTooltipTextColorEnabled;
    public Vector4 OverrideTooltipTextColor = DtrStyle.DefaultTooltipTextColor;

    public bool OverrideTooltipBackgroundColorEnabled;
    public Vector4 OverrideTooltipBackgroundColor = DtrStyle.DefaultTooltipBackgroundColor;

    public OverlayLayoutMode LayoutMode = OverlayLayoutMode.Horizontal;

    public OverlayHorizontalFlow HorizontalPluginFlow = OverlayHorizontalFlow.RightToLeft;

    public OverlayVerticalFlow VerticalPluginFlow = OverlayVerticalFlow.TopToBottom;

    public OverlayVerticalAlignment VerticalAlignment = OverlayVerticalAlignment.Right;

    public NativePluginDivisionMode NativePluginDivision = NativePluginDivisionMode.Separator;

    public bool ShowServerInfo = true;

    public ServerInfoDisplayMode ServerInfoDisplayMode = ServerInfoDisplayMode.Icon;

    public HashSet<string> HiddenServerInfoParts = [];

    public bool ShowPluginEntrySeparators;

    public bool ShowNativeEntrySeparators;

    public bool ShowDivisionSeparatorBar = true;

    public Dictionary<string, PluginEntryAffixes> PluginEntryAffixesByTitle = [];

    public Dictionary<string, int> OverlaySlotMinWidthByTitle = [];
}
