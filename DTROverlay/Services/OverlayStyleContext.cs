using System.Threading;

namespace DTROverlay.Services;

internal static class OverlayStyleContext
{
    private static readonly AsyncLocal<DtrOverlayGroup> CurrentGroup = new();

    public static DtrOverlayGroup Group => CurrentGroup.Value;

    public static IDisposable Push(DtrOverlayGroup group) =>
        new Scope(group);

    private sealed class Scope : IDisposable
    {
        private readonly DtrOverlayGroup _previous = CurrentGroup.Value;

        public Scope(DtrOverlayGroup group) =>
            CurrentGroup.Value = group;

        public void Dispose() =>
            CurrentGroup.Value = _previous;
    }
}
