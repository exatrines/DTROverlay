using Dalamud.Interface;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;

namespace DTROverlay.Services;

/// <summary>
/// Per-group overlay font atlases so rebuilding one group's raster size never invalidates other groups.
/// </summary>
internal static class DtrOverlayFonts
{
    private const float MinSizePx = 8f;
    private const float MaxSizePx = 48f;
    private const float SizeQuantizePx = 0.25f;

    private static readonly Dictionary<string, GroupFontResources> _resourcesByGroupId = new(StringComparer.Ordinal);
    private static bool _disposed;

    public static void Dispose()
    {
        _disposed = true;

        foreach (var resources in _resourcesByGroupId.Values)
            resources.Dispose();

        _resourcesByGroupId.Clear();
    }

    public static float GetTargetSizePx() =>
        QuantizeSize(UiBuilder.DefaultFontSizePx * OverlayStyleResolver.GetEffectiveOverlayFontScale());

    public static void NotifyScaleChanged()
    {
        // Sizes are applied lazily per group on the next Push; no shared atlas rebuild.
    }

    public static void ReleaseGroup(string groupId)
    {
        if (string.IsNullOrEmpty(groupId))
            return;

        if (_resourcesByGroupId.Remove(groupId, out var resources))
            resources.Dispose();
    }

    public static float GetTooltipTargetSizePx() =>
        QuantizeSize(OverlayTooltipResolver.GetEffectiveFontSizePx(OverlayStyleContext.Group));

    public static void NotifyTooltipSizeChanged() => NotifyScaleChanged();

    public static IDisposable PushTooltip()
    {
        if (_disposed)
            return PushTooltipFallback();

        var resources = GetResourcesForCurrentGroup();
        var targetPx = GetTooltipTargetSizePx();
        resources.EnsureTooltip(targetPx);

        if (resources.TooltipHandle == null || !resources.TooltipHandle.Available)
            return PushTooltipFallback();

        return resources.TooltipHandle.Push();
    }

    public static IDisposable PushActive()
    {
        if (_disposed)
            return PushScaledDefaultFontFallback();

        var resources = GetResourcesForCurrentGroup();
        var targetPx = GetTargetSizePx();
        resources.EnsureOverlay(targetPx);

        if (resources.OverlayHandle == null || !resources.OverlayHandle.Available)
            return PushScaledDefaultFontFallback();

        return resources.OverlayHandle.Push();
    }

    private static GroupFontResources GetResourcesForCurrentGroup()
    {
        var groupId = OverlayStyleContext.Group?.Id;
        if (string.IsNullOrEmpty(groupId))
            groupId = "_none";

        if (!_resourcesByGroupId.TryGetValue(groupId, out var resources))
        {
            resources = new GroupFontResources(groupId);
            _resourcesByGroupId[groupId] = resources;
        }

        return resources;
    }

    /// <summary>Used while the raster font is building or after plugin dispose.</summary>
    private static IDisposable PushScaledDefaultFontFallback()
    {
        ImGui.PushFont(UiBuilder.DefaultFont);
        var scale = OverlayStyleResolver.GetEffectiveOverlayFontScale();
        var scaled = Math.Abs(scale - 1f) > 0.001f;
        if (scaled)
            ImGui.SetWindowFontScale(scale);

        return new ScaledDefaultFontPopScope(scaled);
    }

    private static IDisposable PushTooltipFallback()
    {
        ImGui.PushFont(UiBuilder.DefaultFont);
        var scale = OverlayTooltipResolver.GetEffectiveFontSizePx(OverlayStyleContext.Group)
                    / UiBuilder.DefaultFontSizePx;
        var scaled = Math.Abs(scale - 1f) > 0.001f;
        if (scaled)
            ImGui.SetWindowFontScale(scale);

        return new ScaledDefaultFontPopScope(scaled);
    }

    private sealed class GroupFontResources : IDisposable
    {
        private readonly IFontAtlas _atlas;
        private float _overlaySizePx = -1f;
        private float _tooltipSizePx = -1f;

        public IFontHandle OverlayHandle { get; private set; }
        public IFontHandle TooltipHandle { get; private set; }

        public GroupFontResources(string groupId)
        {
            _atlas = Svc.PluginInterface.UiBuilder.CreateFontAtlas(
                FontAtlasAutoRebuildMode.Async,
                isGlobalScaled: true,
                debugName: $"DTROverlay.Overlay.{groupId}");
        }

        public void EnsureOverlay(float targetPx)
        {
            targetPx = QuantizeSize(targetPx);
            if (OverlayHandle != null && _overlaySizePx == targetPx)
                return;

            OverlayHandle?.Dispose();
            _overlaySizePx = targetPx;
            OverlayHandle = CreateHandle(targetPx);
            RequestBuild();
        }

        public void EnsureTooltip(float targetPx)
        {
            targetPx = QuantizeSize(targetPx);
            if (TooltipHandle != null && _tooltipSizePx == targetPx)
                return;

            TooltipHandle?.Dispose();
            _tooltipSizePx = targetPx;
            TooltipHandle = CreateHandle(targetPx);
            RequestBuild();
        }

        private IFontHandle CreateHandle(float bakedSizePx) =>
            _atlas.NewDelegateFontHandle(e =>
                e.OnPreBuild(tk =>
                {
                    var config = new SafeFontConfig
                    {
                        SizePx = bakedSizePx,
                        OversampleH = 3,
                        OversampleV = 2,
                        PixelSnapH = false,
                    };
                    DalamudDefaultFontAndFamilyId.Instance.AddToBuildToolkit(tk, in config);
                }));

        private void RequestBuild()
        {
            if (_atlas.BuildTask.IsCompleted)
                _ = _atlas.BuildFontsAsync();
        }

        public void Dispose()
        {
            OverlayHandle?.Dispose();
            TooltipHandle?.Dispose();
            // Atlas lifetime is tied to the plugin via UiBuilder's scoped finalizer.
        }
    }

    private sealed class ScaledDefaultFontPopScope(bool resetScale) : IDisposable
    {
        public void Dispose()
        {
            if (resetScale)
                ImGui.SetWindowFontScale(1f);

            ImGui.PopFont();
        }
    }

    private static float QuantizeSize(float sizePx) =>
        Math.Clamp(MathF.Round(sizePx / SizeQuantizePx) * SizeQuantizePx, MinSizePx, MaxSizePx);
}
