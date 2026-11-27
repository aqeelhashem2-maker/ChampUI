---
name: champui
description: Build, extend and test ChampUI dashboards in .NET 10 Blazor Interactive Server applications. Use for ChampUI pages, forms, grids, charts, dialogs, navigation, bilingual layouts and application data integration.
---

# Build with ChampUI

**Choose the starting point.** Inspect the consuming project, package references, routes, layout and render mode. For a new dashboard use the published Starter. From a new parent directory run:

```sh
dotnet new install SadeemSystems.ChampUI.Templates@0.1.3 --nuget-source https://api.nuget.org/v3/index.json
dotnet new champui-dashboard -n SkillDashboard
cd SkillDashboard
dotnet restore --source https://api.nuget.org/v3/index.json
dotnet build -c Release --no-restore
```

For an existing .NET 10 Blazor app, add the public ChampUI package at 0.3.3. Follow application-setup.md for AddChampUI(), Interactive Server registration and render boundaries, MapStaticAssets(), the ChampUI stylesheet and the ChampUI JS before Blazor. Preserve the app's existing layout and services when extending it.

**Resolve the API before writing markup.** Read the relevant pages in the checkout's docs/ai/ or use the [pinned catalog](https://github.com/aqeelhashem2-maker/ChampUI/tree/3d4c98bfbc12b0d23dbd1266ecaefccd3a39e9ff/docs/ai). Start with README.md, then component-reference/ComponentName.md and types.md. Check namespace, generic arguments, required parameters, exact callback and slot types, and enum members. Follow the source link for behavior. If a requested feature is absent, compose verified components in application code or explain the gap. Build success alone can miss a misspelled parameter captured as an HTML attribute.

**Implement the user action.**

- Add a routed Razor page and a UiSidebarItem in the application's actual layout. Follow a neighboring page for localization and navigation. Use application-setup.md and the recipes directory for starting examples.
- For tables, use grid.md to choose Items, ServerProvider or VirtualProvider. Use matching TItem on grid and columns, explicit server Field names and immutable unique typed keys. Apply filtering and sorting before paging. A virtual range has no current-page export.
- For forms, bind Value or Checked as specified by the component. Set UiTextField's bound string before awaiting asynchronous work. Display validation errors beside the input and repeat domain validation in the server service. Scope edits to a draft until save succeeds.
- Bind UiDialog.Visible and use its accessible Title. Test Escape and focus restoration. Show success through the scoped ToastService after a confirmed save. UiAppShell includes a toast host.
- For KPIs, calculate monetary values with decimal in an application service. Pass formatted strings to UiStatCard and adapt display data to ChartSeries or ChartSegment. Provide a text equivalent and units.
- For bilingual pages, translate visible text and accessible names, synchronize document and shell language/direction, and test long Arabic text and mixed-direction identifiers. Respect the existing UiThemeState and token overrides. Use logical CSS for layout.
- For EF Core or REST reads and writes, follow the [application service patterns](references/application-services.md). Keep tenant checks, authorization, persistence and financial operations in the application's server layer.

**Verify the delivered behavior.** Restore and build the consuming app against public packages. Exercise the actual route and the requested interaction after the circuit connects. Check keyboard focus, a narrow viewport, both themes, and RTL where applicable. For a provider, test an empty query, sorting and cancellation. Report the package versions, checks run, actual outcomes, and any missing environment or assistive-technology coverage.
