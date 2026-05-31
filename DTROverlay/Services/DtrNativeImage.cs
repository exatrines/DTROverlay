using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace DTROverlay.Services;

public readonly record struct DtrImageInfo(
    string TexturePath,
    ushort PartU,
    ushort PartV,
    ushort PartWidth,
    ushort PartHeight,
    uint TextureWidth,
    uint TextureHeight,
    ImageNodeFlags Flags,
    ushort NodeWidth = 0,
    ushort NodeHeight = 0);

internal static unsafe class DtrNativeImage
{
    public const string HomeworldIconText = "[Homeworld]";
    public const string TravelingIconText = "[Traveling]";

    public static bool TryCreate(AtkImageNode* node, out DtrImageInfo info, bool requireVisible = true)
    {
        info = default;
        if (node == null || (requireVisible && !node->IsVisible()))
            return false;

        if (node->PartsList == null || node->PartsList->Parts == null)
            return false;

        if (node->PartId >= node->PartsList->PartCount)
            return false;

        var part = node->PartsList->Parts[node->PartId];
        if (part.UldAsset == null)
            return false;

        var atkTexture = part.UldAsset->AtkTexture;
        if (atkTexture.TextureType != TextureType.Resource)
            return false;

        if (!atkTexture.IsTextureReady())
            return false;

        if (atkTexture.Resource == null || atkTexture.Resource->TexFileResourceHandle == null)
            return false;

        var path = atkTexture.Resource->TexFileResourceHandle->FileName.ToString();
        if (string.IsNullOrEmpty(path))
            return false;

        var textureWidth = atkTexture.GetTextureWidth();
        var textureHeight = atkTexture.GetTextureHeight();
        if (textureWidth == 0 || textureHeight == 0)
            return false;

        info = new DtrImageInfo(
            path,
            part.U,
            part.V,
            part.Width,
            part.Height,
            textureWidth,
            textureHeight,
            node->Flags,
            node->Width,
            node->Height);

        return true;
    }

    public static bool TryCreateFromContainer(AtkResNode* container, out DtrImageInfo info)
    {
        info = default;
        if (container == null || !container->IsVisible())
            return false;

        return TryFindVisibleImage(container, out info);
    }

    public static bool TryCreateWorldIcon(AddonDtr* dtr, out DtrImageInfo info)
    {
        info = default;
        if (dtr->WorldInfoContainer == null || !dtr->WorldInfoContainer->IsVisible())
            return false;

        if (TryCreateFromContainer(dtr->WorldInfoContainer, out info))
            return true;

        return dtr->WorldVisitImage != null
            && TryCreate(dtr->WorldVisitImage, out info, requireVisible: false);
    }

    private static bool TryFindVisibleImage(AtkResNode* node, out DtrImageInfo info)
    {
        info = default;
        if (node == null || !node->IsVisible())
            return false;

        if (node->Type == NodeType.Image && TryCreate(node->GetAsAtkImageNode(), out info))
            return true;

        for (var child = node->ChildNode; child != null; child = child->NextSiblingNode)
        {
            if (TryFindVisibleImage(child, out info))
                return true;
        }

        return false;
    }

    public static (Vector2 Uv0, Vector2 Uv1) GetUvCoords(in DtrImageInfo image)
    {
        var texWidth = image.TextureWidth;
        var texHeight = image.TextureHeight;

        var uv0 = new Vector2(image.PartU / (float)texWidth, image.PartV / (float)texHeight);
        var uv1 = new Vector2(
            (image.PartU + image.PartWidth) / (float)texWidth,
            (image.PartV + image.PartHeight) / (float)texHeight);

        if ((image.Flags & ImageNodeFlags.FlipH) != 0)
            (uv0.X, uv1.X) = (uv1.X, uv0.X);

        if ((image.Flags & ImageNodeFlags.FlipV) != 0)
            (uv0.Y, uv1.Y) = (uv1.Y, uv0.Y);

        return (uv0, uv1);
    }

    public static Vector2 GetDisplaySize(in DtrImageInfo image, float iconHeight)
    {
        if (image.NodeWidth > 0 && image.NodeHeight > 0)
            return new Vector2(iconHeight * image.NodeWidth / (float)image.NodeHeight, iconHeight);

        var partHeight = Math.Max(1, (int)image.PartHeight);
        var aspect = image.PartWidth / (float)partHeight;
        return new Vector2(iconHeight * aspect, iconHeight);
    }
}
