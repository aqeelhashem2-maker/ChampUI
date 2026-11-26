using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ChampUI.Models;
using Microsoft.Extensions.FileProviders;

namespace ChampUI.Starter.Models;

public sealed class DashboardOptions
{
    public const string Section = "Dashboard";
    private const string DefaultDisplayName = "ChampUI Dashboard Starter";
    private static readonly Regex LocalImage = new(
        @"^/?(?<path>(?:[a-z0-9_-]+/)*[a-z0-9_-]+\.(?:svg|png|webp|ico|gif|jpeg|jpg|avif))\z",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    [Required]
    public string DisplayName { get; init; } = DefaultDisplayName;
    public string? BrowserTitle { get; init; }

    [Required]
    public string SidebarBrand { get; init; } = "ChampUI";

    public string SidebarSubtitle { get; init; } = "Dashboard Starter";
    public string SidebarSubtitleArabic { get; init; } = "قالب لوحة المعلومات";

    [Required]
    public string Description { get; init; } = "A workspace dashboard for revenue, orders and customer activity.";

    public string? LogoPath { get; init; }

    [RegularExpression("en|ar", ErrorMessage = "Dashboard:DefaultLanguage must be en or ar.")]
    [Required]
    public string DefaultLanguage { get; init; } = "en";

    [EnumDataType(typeof(UiThemeMode))]
    public UiThemeMode DefaultTheme { get; init; } = UiThemeMode.Light;

    public bool ShowDeveloperGuide { get; init; } = true;
    public bool IsArabic => DefaultLanguage == "ar";
    public string? CustomDisplayName => DisplayName == DefaultDisplayName ? null : DisplayName;
    public string DocumentTitle => string.IsNullOrWhiteSpace(BrowserTitle) ? DisplayName : BrowserTitle;
    public string? AvailableLogo { get; private set; }

    internal void ResolveLogo(IFileProvider webRoot)
    {
        var match = LocalImage.Match(LogoPath?.Trim() ?? "");
        if (match.Success && webRoot.GetFileInfo(match.Groups["path"].Value) is { Exists: true, IsDirectory: false })
            AvailableLogo = match.Groups["path"].Value;
    }
}
