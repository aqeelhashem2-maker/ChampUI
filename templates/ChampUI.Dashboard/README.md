# ChampUI Dashboard Starter

A .NET 10 Blazor Interactive Server dashboard from Sadeem Systems. It includes a fictional operations overview, customers and orders grids, analytics, and session-scoped workspace preferences. The application requires ChampUI 0.3.3. Restore the library from NuGet.org.

## Get started

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) and check `dotnet --version`. Install the published template and create a project:

```sh
dotnet new install SadeemSystems.ChampUI.Templates@0.1.3
dotnet new list champui-dashboard
dotnet new champui-dashboard -n MyDashboard
cd MyDashboard
```

Restore and run the generated application:

```sh
dotnet restore
dotnet build --no-restore
dotnet run
```

Open the localhost address printed by `dotnet run`. The template package version (0.1.3) is independent of the component dependency (0.3.3).

To restore offline, put the approved `ChampUI.0.3.3.nupkg` and any SDK-required packages in a directory outside the generated project. Some .NET 10 SDKs restore `Microsoft.AspNetCore.App.Internal.Assets` for the Blazor script. Use the version requested by your SDK. With the offline feed prepared, run:

```sh
dotnet restore --source /absolute/path/to/feed --packages /absolute/path/to/isolated-cache
dotnet build --no-restore
dotnet run --no-build --no-restore
```

The ChampUI component library is available on NuGet.org. Use `dotnet new champui-dashboard -n Northwind.Ops-Portal` to choose a different project and assembly name at generation time. The display name in configuration is independent of the .NET project name.

## Project structure

| Path | Purpose |
| --- | --- |
| `Components/Pages` | Dashboard, Customers, Orders and order details, Analytics, Settings, Getting Started |
| `Components/Layout/MainLayout.razor` | Sidebar routes, top bar, direction, and locale |
| `Components/Shared` | Metric cards, order status, and asynchronous loading with retry |
| `Models` | Dashboard snapshots, customer records, and orders |
| `Services` | Data abstraction, fictional fixture data, and session preferences |
| `wwwroot` | Application layout CSS and document language sync |
| `Program.cs` | ChampUI configuration, dependency injection, and static asset endpoints |

To add a page, create a Razor file under `Components/Pages` with an `@page` route. Add a `UiSidebarItem` in `MainLayout.razor` and supply its labels in English and Arabic. Localized pages receive `WorkspacePreferences` via a cascading parameter. Keep identifiers and currency in directional isolation (`bdi`) when mixed with Arabic text.

The initial theme and language come from the Dashboard configuration. The top bar switches theme and language immediately. Settings validates the workspace name and updates the current Blazor circuit. Reloading starts a new session with the configured defaults. The app does not persist preferences in browser storage. To customize the theme, configure shared, light or dark tokens with `AddChampUI(options => …)` in `Program.cs`. Use existing `--ui-*` tokens for new page styles.

## Configure identity and appearance

Edit the `Dashboard` section of `appsettings.json`. Use `DisplayName` for the application name and the initial heading and workspace label when customized. `BrowserTitle` sets the suffix for browser tabs and defaults to the display name. Use `SidebarBrand` for the sidebar label and `Description` for page metadata. `SidebarSubtitle` and `SidebarSubtitleArabic` control the optional second line. For a custom logo, add a file at `wwwroot/images/brand.svg`, then set `LogoPath` to `images/brand.svg` or `/images/brand.svg`. The logo also becomes the browser icon. Missing files and unsupported paths, including external URLs, use the built-in icons. Include the file under `wwwroot` before publishing and restart after changing configuration.

```json
{
  "Dashboard": {
    "DisplayName": "Atlas Operations",
    "BrowserTitle": "Atlas Portal",
    "SidebarBrand": "Atlas",
    "Description": "Internal operations dashboard",
    "LogoPath": "images/brand.svg",
    "DefaultLanguage": "ar",
    "DefaultTheme": "Dark",
    "ShowDeveloperGuide": true
  }
}
```

`DefaultLanguage` accepts `en` or `ar`, and `DefaultTheme` accepts `Light` or `Dark`. These settings apply during server prerendering and after Blazor connects. Set Arabic copy for your custom pages and keep layout CSS logical, for example `margin-inline-start`.

The sidebar link **Getting Started** opens `/getting-started` with working page and REST client examples. To disable the link and return HTTP 404 for direct requests, set `Dashboard:ShowDeveloperGuide` to `false` in `appsettings.json` or override it with `Dashboard__ShowDeveloperGuide=false` in the deployment environment. Restart the app. Empty required identity values and unsupported language or theme values are startup configuration errors.

A new page can start with `@page "/inventory"` and `<UiPageHeader Title="Inventory" />`. Add `<UiSidebarItem Href="/inventory" Text="Inventory" Icon="inbox" />` in the layout and localize both labels as needed.

## Replace the sample data

Implement `IDashboardData` and replace the registration for `SampleDashboardData` in `Program.cs`. The interface is asynchronous and accepts cancellation tokens. For an HTTP backend, register a typed `HttpClient` and map its DTOs to the starter models. For a database, keep access in a service and use an EF Core context factory if each request needs its own context. Keep credentials in server configuration. The sample uses in-memory data only.

The Customers and Orders grids page, search and filter these small result sets in memory. For large datasets, use ChampUI's documented server data provider and move the query to the backend. Treat the data and export options shown in the starter as examples when adding authorization or tenant-aware filtering.

For a REST backend, replace the sample service registration with `builder.Services.AddHttpClient<IDashboardData, ApiDashboardData>()`. In your implementation, pass the method's cancellation token to `HttpClient.GetFromJsonAsync`, map the response to the starter models, and keep the HTTPS API base address in `Api:BaseUrl`. See Getting Started for the complete implementation and production checklist.

## Static assets and Production

`Components/App.razor` references ChampUI and application assets through `Assets[…]`. `Program.cs` maps these paths with `app.MapStaticAssets()`. The ChampUI script and the document-language script load before Blazor.

The included launch profile makes plain `dotnet run` a Development run. For a standalone Production run, publish and run the assembly from the publish directory:

```sh
dotnet publish -c Release -o ./publish
cd publish
ASPNETCORE_ENVIRONMENT=Production dotnet MyDashboard.dll --urls http://localhost:5080
```

Use the generated project's assembly name in that last command. When assets fail, confirm you deployed the full publish output, including `wwwroot` and the static asset endpoint manifest. Check the browser's Network and Console panels for stylesheet or script errors before editing asset paths.
