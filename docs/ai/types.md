# Types used by the components

All component namespace imports are in [application setup](application-setup.md). RenderFragment supplies markup and `RenderFragment<TItem>` supplies a typed context. EventCallback and `EventCallback<T>` are Blazor callbacks. MouseEventArgs is in Microsoft.AspNetCore.Components.Web and NavLinkMatch in Microsoft.AspNetCore.Components.Routing.

## ChampUI.Models

| Type | Values or construction |
| --- | --- |
| UiVariant | Primary, Secondary, Ghost, Outline, Neutral, Success, Warning, Danger, Info |
| UiSize | Small, Medium, Large |
| UiTextDirection / UiThemeMode | Ltr, Rtl / Light, Dark |
| UiSortDirection | None, Ascending, Descending |
| UiTrend / UiAlign | Neutral, Up, Down / Start, Center, End |
| UiDataGridEditMode / UiDataGridVirtualization | None, Inline / None, Enabled |
| BreadcrumbItem | Record constructor takes string Text and optional string? Href. |
| SelectOption&lt;TValue&gt; | Record constructor takes TValue Value then string Text. |
| UiMenuItem | Initialize Text and optional Icon, Href or Func&lt;Task&gt; OnSelect. Danger, Disabled and SeparatorBefore are booleans. |
| ChartSeries | Set string Name and IReadOnlyList&lt;double&gt; Values. Optional string Color overrides the palette. |
| ChartSegment | Set string Label, double Value and optional string Color. |
| ToastKind / ToastMessage | Info, Success, Warning, Danger. Message holds a generated Guid Id, optional Title, Text, Kind and TimeSpan Duration (four seconds by default). |

[Model declarations](../../src/ChampUI/Models/Models.cs) · [Enums and CSS extensions](../../src/ChampUI/Models/Enums.cs)

## Grid contracts

Both provider interfaces are in ChampUI.Core.Interfaces. Their requests, descriptors and results are in ChampUI.Core.Models.

| Type | Contract |
| --- | --- |
| IDataGridDataProvider&lt;TItem&gt; | GetDataAsync(DataGridRequest) returns Task&lt;DataGridResponse&lt;TItem&gt;&gt;. |
| IDataGridVirtualDataProvider&lt;TItem&gt; | GetVirtualDataAsync(DataGridVirtualRequest) returns Task&lt;DataGridVirtualResponse&lt;TItem&gt;&gt;. |
| DataGridRequest / DataGridVirtualRequest | Page and PageSize for paging, StartIndex and Count for ranges. Both carry GlobalSearch, lists of Sorts and Filters and a CancellationToken. |
| GridSortDescriptor | Record constructed with string Field and bool Descending. |
| GridFilterDescriptor | Construct with string field, GridFilterOperator operator, object? value and optional object? value2. Use the second value for Between. |
| GridFilterOperator | Contains, NotContains, Equals, NotEquals, StartsWith, EndsWith, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Between, IsNull, IsNotNull, IsTrue, IsFalse, Before, After, True, False. Select operators appropriate to the column type. |
| DataGridResponse&lt;TItem&gt; / DataGridVirtualResponse&lt;TItem&gt; | Set IReadOnlyList&lt;TItem&gt; Items and int TotalCount. |
| DataGridExportResult&lt;TItem&gt; / DataGridExportScope | Full-query provider response contains Items. Export scopes are CurrentPage and AllFilteredRows. Both provider interfaces expose optional GetAllForExportAsync taking their own request type and returning Task&lt;DataGridExportResult&lt;TItem&gt;?&gt;. |

[Provider declarations](../../src/ChampUI/Core/Interfaces/IDataGridDataProvider.cs) · [Grid types](../../src/ChampUI/Core/Models/DataGridModels.cs)

## Services and display helpers

ChampUI.Services contains ChampUiOptions for token overrides and initial mode, the scoped UiThemeState for Mode, Set and Toggle, and the scoped ToastService. ToastService.Show accepts text, kind, title and duration. Success, Info, Warning and Error are shortcuts. Dismiss takes a message ID. The shell includes UiToastHost once per layout. Use an inline error for information the user must retain after the notification expires.

UiIcons in ChampUI.Components.Display supplies Get and Register. Register accepts inner SVG markup from trusted application code. Use an existing icon name from the registry whenever possible. The theme token constants are in ChampUI.Themes.Tokens.UiTokens.

[Services](../../src/ChampUI/Services) · [Icon registry](../../src/ChampUI/Components/Display/UiIcons.cs) · [Token names](../../src/ChampUI/Themes/Tokens/UiTokens.cs)
