using System.IO;
using Newtonsoft.Json;

namespace DTROverlay;

internal static class I18n
{
    public const string FollowClient = "client";

    private static IDalamudPluginInterface? _pluginInterface;
    private static Dictionary<string, string> _strings = new(StringComparer.Ordinal);

    public static event Action Reloaded;

    public static void Init(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
        pluginInterface.LanguageChanged += OnLanguageChanged;
        Apply();
    }

    public static void Dispose()
    {
        if (_pluginInterface != null)
            _pluginInterface.LanguageChanged -= OnLanguageChanged;

        _pluginInterface = null;
        Reloaded = null;
        _strings = new(StringComparer.Ordinal);
    }

    public static void Apply()
    {
        var lang = C.UiLanguage;
        if (lang is not ("en" or "ja"))
            lang = _pluginInterface?.UiLanguage ?? "en";

        Load(lang);
        Reloaded?.Invoke();
    }

    public static string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (_strings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
            return value;

        return key;
    }

    public static string Format(string key, params object[] args)
    {
        var template = Get(key);
        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    private static void OnLanguageChanged(string _)
    {
        if (C.UiLanguage is not ("en" or "ja"))
            Apply();
    }

    private static void Load(string langCode)
    {
        var lang = Normalize(langCode);
        var map = Read(lang);
        if (map.Count == 0 && lang != "en")
            map = Read("en");

        _strings = map;
    }

    private static string Normalize(string langCode)
    {
        var lang = string.IsNullOrWhiteSpace(langCode) ? "en" : langCode.Trim().ToLowerInvariant();
        return lang.Length > 2 ? lang[..2] : lang;
    }

    private static Dictionary<string, string> Read(string lang)
    {
        var pi = _pluginInterface ?? PluginServices.PluginInterface;
        var dir = pi.AssemblyLocation.DirectoryName ?? AppContext.BaseDirectory;
        var path = Path.Combine(dir, "Data", "I18n", $"{lang}.json");
        if (!File.Exists(path))
            return new Dictionary<string, string>(StringComparer.Ordinal);

        try
        {
            var parsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
            return parsed == null
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : new Dictionary<string, string>(parsed, StringComparer.Ordinal);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }
}
