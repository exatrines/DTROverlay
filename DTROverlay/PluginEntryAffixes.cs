namespace DTROverlay;

public sealed class PluginEntryAffixes
{
    public string Prefix = string.Empty;
    public string Suffix = string.Empty;

    /// <summary>Coerces unset values to <see cref="string.Empty"/> (never null).</summary>
    public void Normalize()
    {
        Prefix ??= string.Empty;
        Suffix ??= string.Empty;
    }
}
