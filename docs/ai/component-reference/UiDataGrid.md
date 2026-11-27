# UiDataGrid

Choose a single data source and supply stable, unique, non-null domain keys. See the [provider and editing guide](../grid.md) for the three query modes and exact export scopes.

Namespace `ChampUI.Components.Data`. Generic parameters: TItem. [Razor source](../../../src/ChampUI/Components/Data/UiDataGrid.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Items` | `IEnumerable<TItem>?` | optional | `null` |
| `ServerProvider` | `IDataGridDataProvider<TItem>?` | optional | `null` |
| `VirtualProvider` | `IDataGridVirtualDataProvider<TItem>?` | optional | `null` |
| `Columns` | `RenderFragment?` | optional | `null` |
| `Toolbar` | `RenderFragment?` | optional | `null` |
| `EmptyTemplate` | `RenderFragment?` | optional | `null` |
| `PageSize` | `int` | optional | `10` |
| `Searchable` | `bool` | optional | `true` |
| `Selectable` | `bool` | optional | `false` |
| `Loading` | `bool` | optional | `false` |
| `ShowColumnToggle` | `bool` | optional | `true` |
| `AllowCsvExport` | `bool` | optional | `false` |
| `AllowXlsxExport` | `bool` | optional | `false` |
| `EditMode` | `UiDataGridEditMode` | optional | `UiDataGridEditMode.None` |
| `MultiSort` | `bool` | optional | `true` |
| `SelectedItems` | `IReadOnlyCollection<TItem>?` | optional | `null` |
| `SelectedItemsChanged` | `EventCallback<IReadOnlyCollection<TItem>>` | optional | `default (no delegate)` |
| `OnRowClick` | `EventCallback<TItem>` | optional | `default (no delegate)` |
| `OnRowSaved` | `EventCallback<TItem>` | optional | `default (no delegate)` |
| `Class` | `string?` | optional | `null` |
| `VirtualizationMode` | `UiDataGridVirtualization` | optional | `UiDataGridVirtualization.None` |
| `VirtualRowHeight` | `float` | optional | `48f` |
| `VirtualOverscan` | `int` | optional | `5` |
| `VirtualViewportHeight` | `string?` | optional | `null` |
| `RowKeySelector` | `Func<TItem, object>?` | optional | `null` |
| `ServerExportAll` | `Func<DataGridRequest, Task<IEnumerable<TItem>>>?` | optional | `null` |
| `SearchPlaceholder` | `string` | optional | `"Search…"` |
| `EmptyTitle` | `string` | optional | `"No results"` |
| `EmptyDescription` | `string` | optional | `"Try adjusting your search or filters."` |
| `SelectedFormat` | `string` | optional | `"{0} selected"` |
| `SummaryFormat` | `string` | optional | `"Showing {0}–{1} of {2}"` |
| `SelectAllLabel` | `string?` | optional | `null` |
| `SelectRowLabel` | `string` | optional | `"Select row"` |
| `PreviousLabel` | `string` | optional | `"Previous page"` |
| `NextLabel` | `string` | optional | `"Next page"` |
| `PageLabel` | `string` | optional | `"Page"` |
| `ColumnsLabel` | `string` | optional | `"Columns"` |
| `ExportLabel` | `string` | optional | `"Export"` |
| `ClearFilterLabel` | `string` | optional | `"Clear"` |
| `ClearAllFiltersLabel` | `string` | optional | `"Clear all"` |
| `FilterLabel` | `string` | optional | `"Filter"` |
| `FilterPlaceholder` | `string` | optional | `"Filter…"` |
| `ApplyFilterLabel` | `string` | optional | `"Apply"` |
| `SaveLabel` | `string` | optional | `"Save"` |
| `CancelLabel` | `string` | optional | `"Cancel"` |
| `EditLabel` | `string` | optional | `"Edit"` |
| `ActionsLabel` | `string` | optional | `"Actions"` |
| `RetryLabel` | `string` | optional | `"Retry"` |
| `ErrorMessage` | `string` | optional | `"Failed to load data."` |
| `ExportCsvCurrentPageLabel` | `string` | optional | `"CSV — current page"` |
| `ExportCsvAllLabel` | `string` | optional | `"CSV — all rows"` |
| `ExportXlsxCurrentPageLabel` | `string` | optional | `"Excel — current page"` |
| `ExportXlsxAllLabel` | `string` | optional | `"Excel — all rows"` |
| `ExportAllUnavailableLabel` | `string` | optional | `"Full export requires ServerExportAll or GetAllForExportAsync."` |
| `ExportErrorMessage` | `string` | optional | `"Export failed."` |
| `VirtualCurrentPageUnavailableLabel` | `string` | optional | `"Current page export is unavailable for virtual data."` |

Callbacks: `SelectedItemsChanged`, `OnRowClick`, `OnRowSaved`. Child slots: `Columns`, `Toolbar`, `EmptyTemplate`.

## Example

```razor
<UiDataGrid TItem="string" Items="names" RowKeySelector="s => s">
    <Columns><UiColumn TItem="string" Title="Customer" Value="s => s" Filterable="true" /></Columns>
</UiDataGrid>
@code { private readonly string[] names = ["Sara", "Noor"]; }
```
