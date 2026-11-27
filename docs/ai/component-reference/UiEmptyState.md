# UiEmptyState

Place a recovery action in ChildContent when the user can resolve an empty result.

Namespace `ChampUI.Components.Display`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Display/UiEmptyState.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Icon` | `string` | optional | `"inbox"` |
| `Title` | `string` | optional | `"Nothing here yet"` |
| `Description` | `string?` | optional | `null` |
| `Class` | `string?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |

Callbacks: none. Child slots: `ChildContent`.

## Example

```razor
<UiEmptyState Title="No appointments" Description="Choose another day." />
```
