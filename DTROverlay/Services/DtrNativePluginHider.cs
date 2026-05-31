using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace DTROverlay.Services;

internal static unsafe class DtrNativePluginHider
{
    private const uint DalamudNodeIdBase = 1000;

    public static void Register() =>
        Svc.AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "_DTR", OnDtrPostDraw);

    public static void Unregister() =>
        Svc.AddonLifecycle.UnregisterListener(AddonEvent.PostDraw, "_DTR", OnDtrPostDraw);

    private static void OnDtrPostDraw(AddonEvent type, AddonArgs args)
    {
        if (!FollowVanillaDtrMode.IsActive)
            return;

        var addon = (AtkUnitBase*)args.Addon.Address;
        if (addon == null || addon->RootNode == null || addon->UldManager.NodeList == null)
            return;

        var hideY = -addon->RootNode->Height * addon->RootNode->ScaleX;

        for (var i = 0; i < addon->UldManager.NodeListCount; i++)
        {
            var node = addon->UldManager.NodeList[i];
            if (node == null || node->NodeId < DalamudNodeIdBase || !node->IsVisible())
                continue;

            node->SetYFloat(hideY);
            node->ToggleVisibility(false);
        }
    }
}
