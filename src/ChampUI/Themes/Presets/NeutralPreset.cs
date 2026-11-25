using ChampUI.Services;

namespace ChampUI.Themes.Presets;

/// <summary>
/// The neutral default theme for ChampUI.
/// Uses an indigo accent with a neutral slate surface palette.
/// No CHAMP-specific naming or colors.
/// </summary>
public static class NeutralPreset
{
    /// <summary>Applies the neutral (default) theme tokens to <see cref="ChampUiOptions"/>.</summary>
    public static ChampUiOptions Apply(ChampUiOptions options) => options; // defaults are already neutral

    /// <summary>Convenience extension method.</summary>
    public static void UseNeutralPreset(this ChampUiOptions options) { /* defaults are neutral, no-op */ }
}
