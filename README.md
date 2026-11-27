# ChampUI

**Developed by Sadeem Systems.** ChampUI is an open-source UI component library for Blazor on .NET 10, licensed under [MIT](LICENSE).

ChampUI includes themed layout and navigation components, forms, alerts and dialogs, SVG charts, and a DataGrid with local, paged-provider and virtual-range modes. It uses the ASP.NET Core shared framework and has no third-party NuGet dependencies.

## Try the showcase

```sh
dotnet run --project demo/ChampUI.Demo
```

Explore **Foundations**, **Forms**, **Data** and **Charts** in ChampUI Showcase. The shell supports light and dark themes and an Arabic right-to-left demonstration. The Data page uses fictional project records. Its data-source picker switches between local records, a paged provider and a virtual range provider.

## Use ChampUI in a .NET 10 app

Install the published package from NuGet.org:

```sh
dotnet add package ChampUI --version 0.3.3
```

See the [agent documentation and component inventory](docs/ai/README.md) for compiled examples and complete Interactive Server setup.

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

## Dashboard Starter

Generate a .NET 10 Blazor dashboard with fictional data, English and Arabic layouts, and theme and identity settings. Install the published template with `dotnet new install SadeemSystems.ChampUI.Templates@0.1.3`, then run `dotnet new champui-dashboard -n MyDashboard`. Follow the [Dashboard Starter guide](templates/ChampUI.Dashboard/README.md).

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
