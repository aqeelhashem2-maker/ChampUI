using ChampUI.Services;

namespace ChampUI.Themes.Tokens;

/// <summary>
/// Documents all available ChampUI design token names.
/// Use these string constants with <see cref="ChampUiOptions"/> to override tokens at runtime.
/// </summary>
public static class UiTokens
{
    // ── Colour ──────────────────────────────────────────
    public const string Accent          = "ui-accent";
    public const string AccentStrong    = "ui-accent-strong";
    public const string AccentSoft      = "ui-accent-soft";
    public const string AccentContrast  = "ui-accent-contrast";
    public const string Bg              = "ui-bg";
    public const string Surface         = "ui-surface";
    public const string SurfaceElevated = "ui-surface-elevated";
    public const string SurfaceHover    = "ui-surface-hover";
    public const string Sidebar         = "ui-sidebar";
    public const string Border          = "ui-border";
    public const string BorderStrong    = "ui-border-strong";
    public const string Text            = "ui-text";
    public const string TextMuted       = "ui-text-muted";
    public const string TextFaint       = "ui-text-faint";
    public const string Overlay         = "ui-overlay";

    // ── Semantic ─────────────────────────────────────────
    public const string Success         = "ui-success";
    public const string Warning         = "ui-warning";
    public const string Danger          = "ui-danger";
    public const string Info            = "ui-info";

    // ── Chart palette ────────────────────────────────────
    public const string Chart1          = "ui-chart-1";
    public const string Chart2          = "ui-chart-2";
    public const string Chart3          = "ui-chart-3";
    public const string Chart4          = "ui-chart-4";
    public const string Chart5          = "ui-chart-5";
    public const string Chart6          = "ui-chart-6";

    // ── Shape ────────────────────────────────────────────
    public const string RadiusSm        = "ui-radius-sm";
    public const string RadiusMd        = "ui-radius-md";
    public const string RadiusLg        = "ui-radius-lg";
    public const string RadiusXl        = "ui-radius-xl";
    public const string RadiusFull      = "ui-radius-full";

    // ── Motion ───────────────────────────────────────────
    public const string Duration        = "ui-duration";
    public const string Ease            = "ui-ease";

    // ── Typography ───────────────────────────────────────
    public const string FontSans        = "ui-font-sans";
    public const string FontMono        = "ui-font-mono";
    public const string WeightMedium    = "ui-weight-medium";
    public const string WeightSemibold  = "ui-weight-semibold";
    public const string WeightBold      = "ui-weight-bold";
}
