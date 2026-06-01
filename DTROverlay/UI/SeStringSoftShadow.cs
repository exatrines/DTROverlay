using System;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;

namespace DTROverlay.UI;

internal static class SeStringSoftShadow
{
    private static readonly Dictionary<uint, KernelSample[]> KernelCache = new();

    private readonly record struct KernelSample(Vector2 Offset, float Weight);

    public static void DrawPlainText(
        ImDrawListPtr drawList,
        ImFontPtr font,
        float fontSize,
        Vector2 pos,
        uint shadowColor,
        float radius,
        string text)
    {
        if (string.IsNullOrEmpty(text) || radius <= 0f)
            return;

        var kernel = GetKernel(radius);
        if (kernel.Length == 0)
            return;

        var sumWeight = 0f;
        foreach (var sample in kernel)
            sumWeight += sample.Weight;

        if (sumWeight <= 0f)
            return;

        var accumulation = DtrStyle.SoftShadowAccumulation * MathF.Min(kernel.Length, 12f);

        foreach (var sample in kernel)
        {
            var alphaFactor = sample.Weight / sumWeight * accumulation;
            if (alphaFactor <= 0.001f)
                continue;

            drawList.AddText(
                font,
                fontSize,
                pos + sample.Offset,
                ScaleAlpha(shadowColor, alphaFactor),
                text);
        }
    }

    public static void Draw(
        byte[] data,
        in SeStringDrawParams template,
        Vector2 pos,
        float radius)
    {
        if (radius <= 0f)
            return;

        var kernel = GetKernel(radius);
        if (kernel.Length == 0)
            return;

        var sumWeight = 0f;
        foreach (var sample in kernel)
            sumWeight += sample.Weight;

        if (sumWeight <= 0f)
            return;

        var builtInShadowDelta = new Vector2(0f, 1f);
        var baseShadowColor = template.ShadowColor ?? 0xFF000000;
        var accumulation = DtrStyle.SoftShadowAccumulation * MathF.Min(kernel.Length, 12f);

        foreach (var sample in kernel)
        {
            var alphaFactor = sample.Weight / sumWeight * accumulation;
            if (alphaFactor <= 0.001f)
                continue;

            var shadowParams = template;
            shadowParams.Edge = false;
            shadowParams.Shadow = true;
            shadowParams.Color = 0;
            shadowParams.ScreenOffset = pos + sample.Offset - builtInShadowDelta;
            shadowParams.ShadowColor = ScaleAlpha(baseShadowColor, alphaFactor);
            ImGuiHelpers.SeStringWrapped(data, shadowParams);
        }
    }

    private static KernelSample[] GetKernel(float radius)
    {
        var maxRadius = DtrStyle.MaxSoftShadowRadius;
        var radiusPx = Math.Clamp(radius, 0.5f, maxRadius);
        var key = BitConverter.SingleToUInt32Bits(radiusPx);
        if (KernelCache.TryGetValue(key, out var cached))
            return cached;

        var extent = (int)MathF.Ceiling(radiusPx);
        var samples = new List<KernelSample>();

        for (var dy = 0; dy <= extent; dy++)
        {
            for (var dx = -extent; dx <= extent; dx++)
            {
                var dist = MathF.Sqrt(dx * dx + dy * dy);
                if (dist > radiusPx || dist < 0.01f)
                    continue;

                var weight = 1f - dist / radiusPx;
                samples.Add(new KernelSample(new Vector2(dx, dy), weight));
            }
        }

        var kernel = samples.ToArray();
        KernelCache[key] = kernel;
        return kernel;
    }

    private static uint ScaleAlpha(uint color, float alphaFactor)
    {
        var c = ImGui.ColorConvertU32ToFloat4(color);
        c.W *= Math.Clamp(alphaFactor, 0f, 1f);
        return ImGui.ColorConvertFloat4ToU32(c);
    }
}
