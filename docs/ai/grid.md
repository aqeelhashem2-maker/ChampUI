# DataGrid queries, edits and exports

Use [UiDataGrid](component-reference/UiDataGrid.md) and nested [UiColumn](component-reference/UiColumn.md) with the same TItem. Use Value for typed data and Field for a stable server field independent of a translated Title. Enable Filterable per column and choose FilterType for the data. Searchable on the grid controls the global search UI.

| Input | Query contract | Export behavior |
| --- | --- | --- |
| Items | The grid searches, filters, sorts and pages an in-memory collection. | CurrentPage exports the loaded page. AllFilteredRows spans the filtered result. |
| ServerProvider | Implement IDataGridDataProvider and return one 1-based page plus the total filtered count. | CurrentPage exports the loaded page. Supply ServerExportAll or GetAllForExportAsync for all filtered rows. |
| VirtualProvider | Implement IDataGridVirtualDataProvider. Apply the request's search, Filters and Sorts before slicing StartIndex and Count. Honor cancellation. Return the filtered total. | CurrentPage is unavailable. GetAllForExportAsync receives a full-query request with zero range. Returning null reports unavailability. |

For a working paged and range provider, follow the [showcase data implementation](../../demo/ChampUI.Demo/Data/ProjectData.cs). Filter before counting and apply stable tie-breaking before paging. Range requests can overlap or finish out of order. A response may contain overscan. VirtualizationMode.Enabled on loaded page data virtualizes rendering only. A changed virtual query invalidates pending export work. AllowCsvExport and AllowXlsxExport enable the respective formats.

Selectable enables selection. Bind SelectedItems and supply RowKeySelector so freshly materialized records preserve identity across queries. Use immutable unique keys and preserve their type. Duplicate or null selectors cannot distinguish logical rows. Virtual select-all covers loaded identities with a 10,000-key bound. The provider owns any whole-query bulk operation.

The example edits a copy. In inline mode EditTemplate receives a JSON-cloned row. OnRowSaved must persist or replace the record; if the callback throws, the grid retains the edit for retry. Use DTOs that round-trip through System.Text.Json. Keep persistence and concurrency checks in your application.

```razor
<UiDataGrid TItem="ServiceRow" Items="rows" RowKeySelector="row => row.Id"
            EditMode="UiDataGridEditMode.Inline" OnRowSaved="Save"
            Selectable="true" @bind-SelectedItems="selected" AllowCsvExport="true">
    <Columns>
        <UiColumn TItem="ServiceRow" Title="Service" Field="Name" Value="row => row.Name" Filterable="true">
            <EditTemplate Context="draft"><UiTextField Label="Service name" @bind-Value="draft.Name" /></EditTemplate>
        </UiColumn>
    </Columns>
</UiDataGrid>
@code {
    public sealed class ServiceRow {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
    private ServiceRow[] rows = [new() { Id = 1, Name = "Haircut" }];
    private IReadOnlyCollection<ServiceRow> selected = [];
    private void Save(ServiceRow draft) {
        rows = rows.Select(row => row.Id == draft.Id ? draft : row).ToArray();
    }
}
```

## Provider examples

Copy the [Showcase ProjectData.cs](../../demo/ChampUI.Demo/Data/ProjectData.cs) into the consumer to run these two examples. ProjectData and ProjectRow are application examples. Core supplies the provider interfaces. This fixture implements single-column sorting and a limited set of category-filter operators. A real provider must implement every operator its UI exposes.

```razor
@using ChampUI.Demo.Data
<UiDataGrid TItem="ProjectRow" ServerProvider="provider" RowKeySelector="row => row.Id"
            MultiSort="false" PageSize="10" AllowCsvExport="true">
    <Columns>
        <UiColumn TItem="ProjectRow" Title="Project" Field="Name" Value="row => row.Name" />
    </Columns>
</UiDataGrid>
@code { private readonly ProjectData provider = new(); }
```

```razor
@using ChampUI.Demo.Data
<UiDataGrid TItem="ProjectRow" VirtualProvider="provider" RowKeySelector="row => row.Id"
            MultiSort="false" VirtualViewportHeight="320px" VirtualRowHeight="48" AllowXlsxExport="true">
    <Columns>
        <UiColumn TItem="ProjectRow" Title="Project" Field="Name" Value="row => row.Name" />
    </Columns>
</UiDataGrid>
@code { private readonly ProjectData provider = new(); }
```
