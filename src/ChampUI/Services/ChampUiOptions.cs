using ChampUI.Models;

namespace ChampUI.Services;

/// <summary>
/// Application-level configuration of the framework. Applications override design tokens here
/// (e.g. brand colours) without touching the library. Keys are CSS custom property names,
/// with or without the leading "--" (e.g. "ui-accent").
/// </summary>
public sealed class ChampUiOptions
{
    public UiThemeMode DefaultMode { get; set; } = UiThemeMode.Light;

    /// <summary>Tokens applied in both light and dark mode.</summary>
    public Dictionary<string, string> SharedTokens { get; } = new();
    public Dictionary<string, string> LightTokens { get; } = new();
    public Dictionary<string, string> DarkTokens { get; } = new();

    public ChampUiOptions Shared(string token, string value) { SharedTokens[token] = value; return this; }
    public ChampUiOptions Light(string token, string value) { LightTokens[token] = value; return this; }
    public ChampUiOptions Dark(string token, string value) { DarkTokens[token] = value; return this; }

    internal static string Normalize(string token) => token.StartsWith("--") ? token : "--" + token;
}
