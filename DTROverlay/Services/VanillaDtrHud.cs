using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace DTROverlay.Services;

internal static unsafe class VanillaDtrHud
{
    public static void Toggle()
    {
        var message = Utf8String.FromString("/hud dtr");
        UIModule.Instance()->ProcessChatBoxEntry(message);
        message->Dtor(true);
    }
}
