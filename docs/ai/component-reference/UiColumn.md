# UiColumn

Place inside the Columns slot with the same TItem. Template formats the cell while Value supplies sorting, searching and export. Visible is a public property, not a Razor parameter.

Namespace `ChampUI.Components.Data`. Generic parameters: TItem. [Razor source](../../../src/ChampUI/Components/Data/UiColumn.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Grid` | `UiDataGrid<TItem>?` | cascade | `null` |
| `Title` | `string` | required | `""` |
| `Value` | `Func<TItem, object?>?` | optional | `null` |
| `Format` | `string?` | optional | `null` |
| `Template` | `RenderFragment<TItem>?` | optional | `null` |
| `HeaderTemplate` | `RenderFragment?` | optional | `null` |
| `EditTemplate` | `RenderFragment<TItem>?` | optional | `null` |
| `Sortable` | `bool` | optional | `true` |
| `Searchable` | `bool` | optional | `true` |
| `Filterable` | `bool` | optional | `false` |
| `FilterType` | `string` | optional | `"text"` |
| `Width` | `string?` | optional | `null` |
| `MinWidth` | `string?` | optional | `null` |
| `Align` | `UiAlign` | optional | `UiAlign.Start` |
| `HideOnMobile` | `bool` | optional | `false` |
| `Field` | `string?` | optional | `null` |

Callbacks: none. Child slots: `Template`, `HeaderTemplate`, `EditTemplate`.

## Example

```razor
<UiDataGrid TItem="string" Items="names" RowKeySelector="s => s">
    <Columns><UiColumn TItem="string" Title="Customer" Value="s => s" Filterable="true" /></Columns>
</UiDataGrid>
@code { private readonly string[] names = ["Sara", "Noor"]; }
```
