namespace DTROverlay;

using DTROverlay.UI;

public sealed class Configuration
{
    public bool OverlayEnabled = true;
    public bool FollowVanillaDtr;
    public bool SplitNativeDtr = true;
    public FollowVanillaDtrSide FollowVanillaDtrSide = FollowVanillaDtrSide.Left;
    public float FollowVanillaHorizontalOffset;
    public float FollowVanillaVerticalOffset = DtrStyle.DefaultFollowVanillaVerticalOffset;
    public float FollowVanillaFontSizeScale = DtrStyle.DefaultFollowVanillaFontSizeScale;
    public Vector2 OverlayPosition = new(20f, 10f);
    public OverlayPositionOrigin OverlayPositionOrigin = OverlayPositionOrigin.TopRight;
    public bool OverlayPositionOriginMigrated;
    public OverlayLayoutMode OverlayLayoutMode = OverlayLayoutMode.Horizontal;
    public OverlayHorizontalFlow HorizontalPluginFlow = OverlayHorizontalFlow.RightToLeft;
    public OverlayVerticalFlow VerticalPluginFlow = OverlayVerticalFlow.TopToBottom;
    public OverlayVerticalAlignment OverlayVerticalAlignment = OverlayVerticalAlignment.Right;
    public bool OverlayEditMode;
    public bool OpenPluginUiOnMiddleClick = true;
    public bool OpenPluginUiMiddleClickMigrated;
    public bool CenterTooltipBelowHoveredEntry = true;
    public bool TooltipPositionMigrated;
    public TooltipPosition TooltipPosition = TooltipPosition.Lower;
    public float TooltipFontSizePx = DtrStyle.DefaultTooltipFontSizePx;
    public Vector4 TooltipTextColor = DtrStyle.DefaultTooltipTextColor;
    public Vector4 TooltipBackgroundColor = DtrStyle.DefaultTooltipBackgroundColor;

    /// <summary>Legacy; migrated to <see cref="OpenPluginUiOnMiddleClick"/>.</summary>
    public bool OpenPluginUiOnRightClick = true;
    /// <summary>Draw | in plugin separator slots (off = blank slot using separator width).</summary>
    public bool ShowPluginEntrySeparators;
    /// <summary>Draw | in native separator slots (off = blank slot using separator width).</summary>
    public bool ShowNativeEntrySeparators;
    /// <summary>Draw | in the native/plugin division separator slot.</summary>
    public bool ShowDivisionSeparatorBar = true;
    public bool DivisionSeparatorBarMigrated;
    /// <summary>Fixed slot width (px) for all separator slots. 0 = no reserved width when the bar is hidden.</summary>
    public int SeparatorSlotWidthPx;
    public NativePluginDivisionMode NativePluginDivision = NativePluginDivisionMode.Separator;
    public bool SeparatorVisibilitySplitMigrated;
    public bool AppearanceColorFlagsMigrated;
    public bool FontSizeScaleSplitMigrated;
    public bool ShowServerInfo = true;
    public bool ServerInfoTableRowMigrated;
    public ServerInfoDisplayMode ServerInfoDisplayMode = ServerInfoDisplayMode.Icon;
    public float OverlayFontSizeScale = DtrStyle.DefaultOverlayFontSizeScale;

    /// <summary>Legacy; migrated to <see cref="OverlayFontSizeScale"/>.</summary>
    public float FontScale;
    public Vector4 TextColor = DtrStyle.DefaultTextColor;
    public Vector4 OutlineColor = DtrStyle.DefaultOutlineColor;
    public Vector4 ShadowColor = DtrStyle.DefaultShadowColor;
    public bool EdgeEnabled = DtrStyle.DefaultEdgeEnabled;
    public bool ShadowEnabled = DtrStyle.DefaultShadowEnabled;
    public float EdgeStrength = DtrStyle.DefaultEdgeStrength;
    public float ShadowThickness = DtrStyle.DefaultShadowThickness;
    public HashSet<string> HiddenEntryTitles = [];
    public HashSet<string> HiddenServerInfoParts = [];
    public bool PluginAffixMigrated;
    public Dictionary<string, PluginEntryAffixes> PluginEntryAffixesByTitle = [];
    /// <summary>Per-plugin fixed overlay slot width (px) when the plugin does not set DTR MinimumWidth. 0 = use measured text.</summary>
    public Dictionary<string, int> OverlaySlotMinWidthByTitle = [];

    /// <summary>Legacy; migrated to <see cref="PluginEntryAffixesByTitle"/>.</summary>
    public HashSet<string> ShowPluginNameEntryTitles = [];
    public HashSet<string> FixedWidthEnabledIds = [];
    public HashSet<string> FixedColorEnabledIds = [];
    public HashSet<string> FixedTextColorEnabledIds = [];
    public HashSet<string> FixedEdgeStyleEnabledIds = [];
    public HashSet<string> FixedShadowStyleEnabledIds = [];
    public bool TextColorEnabledMigrated;
    public bool EdgeShadowStyleEnabledMigrated;
    public Dictionary<string, float> FixedWidthPixels = [];
    public Dictionary<string, Vector4> FixedWidthTextColors = [];
    public Dictionary<string, Vector4> FixedWidthOutlineColors = [];
    public Dictionary<string, Vector4> FixedWidthShadowColors = [];
    public Dictionary<string, float> FixedWidthEdgeStrengths = [];
    public Dictionary<string, float> FixedWidthShadowThicknesses = [];
    public Dictionary<string, bool> FixedWidthEdgeEnabled = [];
    public Dictionary<string, bool> FixedWidthShadowEnabled = [];
    public List<string> EntryOrder = [];

    public List<DtrOverlayGroup> OverlayGroups = [];

    public string SelectedOverlayGroupId = "";

    public bool OverlayGroupsMigrated;

    public bool NativeGroupMigrated;

    public bool StyleHierarchyMigrated;
    public bool MergedDefaultOverrideStylesMigrated;

    public bool GroupLayoutMigrated;

    public bool GroupScopedSettingsMigrated;
}
