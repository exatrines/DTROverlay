namespace DTROverlay;

using DTROverlay.UI;

public sealed class Configuration
{
    public bool OverlayEnabled = true;
    public bool FollowVanillaDtr;
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
    public bool OpenPluginUiOnRightClick = true;
    public bool ShowPluginEntrySeparators;
    public bool ShowNativeEntrySeparators;
    public NativePluginDivisionMode NativePluginDivision = NativePluginDivisionMode.NewLine;
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
    public HashSet<string> HiddenEntryTitles = [];
    public HashSet<string> HiddenServerInfoParts = [];
    public bool PluginAffixMigrated;
    public Dictionary<string, PluginEntryAffixes> PluginEntryAffixesByTitle = [];

    /// <summary>Legacy; migrated to <see cref="PluginEntryAffixesByTitle"/>.</summary>
    public HashSet<string> ShowPluginNameEntryTitles = [];
    public HashSet<string> FixedWidthEnabledIds = [];
    public HashSet<string> FixedColorEnabledIds = [];
    public Dictionary<string, float> FixedWidthPixels = [];
    public Dictionary<string, Vector4> FixedWidthTextColors = [];
    public Dictionary<string, Vector4> FixedWidthOutlineColors = [];
    public List<string> EntryOrder = [];
}
