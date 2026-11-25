# ChampUI

**Developed by Sadeem Systems.** ChampUI is an open-source UI component library for Blazor on .NET 10, licensed under [MIT](LICENSE).

ChampUI includes themed layout and navigation components, forms, alerts and dialogs, SVG charts, and a DataGrid with local, paged-provider and virtual-range modes. It uses the ASP.NET Core shared framework and has no third-party NuGet dependencies.

## Try the showcase

```sh
dotnet run --project demo/ChampUI.Demo
```

Explore **Foundations**, **Forms**, **Data** and **Charts** in ChampUI Showcase. The shell supports light and dark themes and an Arabic right-to-left demonstration. The Data page uses fictional project records. Its data-source picker switches between local records, a paged provider and a virtual range provider.

## Use ChampUI in a .NET 10 app

For a local release candidate, put the package in a local feed and restore it with:

```sh
dotnet add package ChampUI --version 0.3.1 --source /path/to/local-feed
```

The public package will be installable without the local source flag after publication. The package ID is `ChampUI`.

Register the library and map static assets in the host:

```csharp
using ChampUI.Services;

builder.Services.AddChampUI();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// After building the host:
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
```

Load the assets alongside the Blazor script in the app's root component:

```html
<link rel="stylesheet" href="_content/ChampUI/champui.css" />
<script src="_content/ChampUI/js/champui.js"></script>
```

Wrap content in `UiAppShell` to apply theme tokens. Use `UiThemeState` to switch appearance and the shell's `Direction` and `Language` parameters for RTL.

## DataGrid behavior

Use stable and unique typed keys with `RowKeySelector`. A paged grid can export its loaded page. Virtual range providers use full-query exports when their provider implements `GetAllForExportAsync`; current-page export is disabled in that mode. Changing a virtual query invalidates a pending export. Select All for a virtual query acts on loaded row identities and retains at most 10,000 of them.

## Verification

Use .NET 10 and the Chromium build pinned to the Playwright test project.

```sh
dotnet build tests/ChampUI.BrowserTests/ChampUI.BrowserTests.csproj -c Release
pwsh tests/ChampUI.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium
bash verify_fixes.sh
```

On Linux, use `install --with-deps chromium`. The gate runs the complete unit and browser suites with a local demo server. See the [showcase and verification guide](docs/showcase.md) and [contributing guide](CONTRIBUTING.md).

## License

ChampUI is released under the MIT License. See [LICENSE](LICENSE) and [third-party notices](THIRD_PARTY_NOTICES.md).
