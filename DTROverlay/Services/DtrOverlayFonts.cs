using Dalamud.Interface;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;

namespace DTROverlay.Services;

/// <summary>
/// Overlay-only font atlas. Uses a dedicated <see cref="UiBuilder.CreateFontAtlas"/> so rebuilds
/// never touch <see cref="UiBuilder.FontAtlas"/> (config UI and other plugin ImGui windows).
/// </summary>
internal static class DtrOverlayFonts
{
    private const float MinSizePx = 8f;
    private const float MaxSizePx = 48f;
    private const float SizeQuantizePx = 0.25f;

    private static IFontAtlas _overlayAtlas = null!;
    private static IFontHandle _fontHandle = null!;
    private static bool _hasHandle;
    private static float _builtSizePx;
    private static bool _disposed;

    public static void Dispose()
    {
        _disposed = true;
        if (_hasHandle)
        {
            _fontHandle.Dispose();
            _hasHandle = false;
        }

        // Atlas lifetime is tied to the plugin via UiBuilder's scoped finalizer — do not Dispose it here.
        _overlayAtlas = null!;
    }

    public static float GetTargetSizePx() =>
        QuantizeSize(UiBuilder.DefaultFontSizePx * FollowVanillaFontScale.ActiveScale);

    public static void NotifyScaleChanged() => RequestRebuildIfNeeded(GetTargetSizePx());

    public static IDisposable PushActive()
    {
        if (_disposed)
            return PushScaledDefaultFontFallback();

        EnsureHandle();
        RequestRebuildIfNeeded(GetTargetSizePx());

        if (!_hasHandle || !_fontHandle.Available)
            return PushScaledDefaultFontFallback();

        return _fontHandle.Push();
    }

    private static IFontAtlas GetOverlayAtlas() =>
        _overlayAtlas ??= Svc.PluginInterface.UiBuilder.CreateFontAtlas(
            FontAtlasAutoRebuildMode.Async,
            isGlobalScaled: true,
            debugName: "DTROverlay.Overlay");

    private static void EnsureHandle()
    {
        if (_hasHandle)
            return;

        _builtSizePx = QuantizeSize(GetTargetSizePx());
        _fontHandle = GetOverlayAtlas().NewDelegateFontHandle(e =>
            e.OnPreBuild(tk =>
            {
                var config = new SafeFontConfig
                {
                    SizePx = _builtSizePx,
                    OversampleH = 3,
                    OversampleV = 2,
                    PixelSnapH = false,
                };
                DalamudDefaultFontAndFamilyId.Instance.AddToBuildToolkit(tk, in config);
            }));

        _hasHandle = true;
        _ = GetOverlayAtlas().BuildFontsAsync();
    }

    private static void RequestRebuildIfNeeded(float targetPx)
    {
        if (!_hasHandle || targetPx == _builtSizePx)
            return;

        _builtSizePx = targetPx;

        var atlas = GetOverlayAtlas();
        if (!atlas.BuildTask.IsCompleted)
            return;

        _ = atlas.BuildFontsAsync();
    }

    /// <summary>Used while the raster font is building or after plugin dispose.</summary>
    private static IDisposable PushScaledDefaultFontFallback()
    {
        ImGui.PushFont(UiBuilder.DefaultFont);
        var scale = FollowVanillaFontScale.ActiveScale;
        var scaled = Math.Abs(scale - 1f) > 0.001f;
        if (scaled)
            ImGui.SetWindowFontScale(scale);

        return new ScaledDefaultFontPopScope(scaled);
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
