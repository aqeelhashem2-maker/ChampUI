# Start an Interactive Server application

Use the .NET 10 SDK. For a dashboard with routing, bilingual sample pages, theme controls and data grids already configured, install the published Starter template and generate an application.

```sh
dotnet new install SadeemSystems.ChampUI.Templates@0.1.3 --nuget-source https://api.nuget.org/v3/index.json
dotnet new champui-dashboard -n MyDashboard
cd MyDashboard
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet run
```

For a minimal host, generate the standard Blazor Web App and add Core to the generated project.

```sh
dotnet new blazor -n DocsConsumer --interactivity Server --all-interactive
cd DocsConsumer
dotnet add package ChampUI --version 0.3.3 --source https://api.nuget.org/v3/index.json
```

## Program.cs

The project name below determines the generated Components namespace. Register ChampUI once. AddInteractiveServerComponents registers circuit services and AddInteractiveServerRenderMode maps their endpoints. Scope UI state to the circuit. Keep business services and authorization in the host.

```csharp
using ChampUI.Services;
using DocsConsumer.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddChampUI();
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
```

## Components/App.razor

Set the interactive boundary on Routes so that events, dialog JS interop and notifications work after connection. Keep the library script ahead of Blazor. The asset map resolves fingerprinted URLs in a published app.

```razor
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <base href="/" />
    <link rel="stylesheet" href="@Assets["_content/ChampUI/champui.css"]" />
    <ImportMap />
    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="@Assets["_content/ChampUI/js/champui.js"]"></script>
    <script src="@Assets["_framework/blazor.web.js"]"></script>
</body>
</html>
```

## Components/_Imports.razor

Append these imports to those supplied by the Blazor template. The examples use these namespaces.

```razor
@using ChampUI.Components.Actions
@using ChampUI.Components.Charts
@using ChampUI.Components.Data
@using ChampUI.Components.Display
@using ChampUI.Components.Feedback
@using ChampUI.Components.Forms
@using ChampUI.Components.Layout
@using ChampUI.Components.Navigation
@using ChampUI.Core.Interfaces
@using ChampUI.Core.Models
@using ChampUI.Models
@using ChampUI.Services
```

## Components/Layout/MainLayout.razor

```razor
@inherits LayoutComponentBase
@inject UiThemeState Theme
<UiAppShell Direction="UiTextDirection.Ltr" Language="en">
    <Topbar><UiTopbar><End><UiButton OnClick="Theme.Toggle">Switch theme</UiButton></End></UiTopbar></Topbar>
    <ChildContent>@Body</ChildContent>
</UiAppShell>
```

The shell emits theme overrides and hosts notifications. Set the starting mode through ChampUiOptions.DefaultMode in AddChampUI's configuration delegate. Shared, Light and Dark accept token names and CSS values. UiThemeState.Toggle switches the scoped mode. Persist user preferences in application code if needed across circuits.

For Arabic set Language to ar and Direction to UiTextDirection.Rtl, and synchronize the root document's lang and dir with the active locale. Supply localized labels, ARIA names, grid format strings and chart summaries. Choose an explicit culture for dates, money and numbers. Use logical CSS properties and bidirectional isolation for mixed Arabic text and identifiers. The [Starter layout](../../templates/ChampUI.Dashboard/Components/Layout/MainLayout.razor) demonstrates switching at runtime.
