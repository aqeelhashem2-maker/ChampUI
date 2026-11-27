# UiPagination

Page numbers start at one. Supply TotalItems as the result count and update Page when the user navigates.

Namespace `ChampUI.Components.Navigation`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Navigation/UiPagination.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Page` | `int` | optional | `1` |
| `PageChanged` | `EventCallback<int>` | optional | `default (no delegate)` |
| `PageSize` | `int` | optional | `10` |
| `TotalItems` | `int` | optional | `0` |
| `SummaryFormat` | `string` | optional | `"Showing {0}–{1} of {2}"` |
| `PreviousLabel` | `string` | optional | `"Previous page"` |
| `NextLabel` | `string` | optional | `"Next page"` |
| `PageLabel` | `string` | optional | `"Page"` |
| `Class` | `string?` | optional | `null` |

Callbacks: `PageChanged`. Child slots: none.

## Example

```razor
<UiPagination @bind-Page="page" PageSize="10" TotalItems="32" />
@code { private int page = 1; }
```
