# UiMenu

Supply menu actions as UiMenuItem objects. Use Href for navigation or OnSelect for an asynchronous action.

Namespace `ChampUI.Components.Actions`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Actions/UiMenu.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Items` | `IEnumerable<UiMenuItem>` | required | `Array.Empty<UiMenuItem>()` |
| `Trigger` | `RenderFragment?` | optional | `null` |
| `Icon` | `string` | optional | `"dots"` |
| `Label` | `string` | optional | `"More actions"` |
| `Header` | `string?` | optional | `null` |
| `TriggerClass` | `string` | optional | `"ui-iconbtn"` |
| `Align` | `UiAlign` | optional | `UiAlign.End` |

Callbacks: none. Child slots: `Trigger`.

## Example

```razor
<UiMenu Label="Customer actions" Items="@(new[] { new UiMenuItem { Text = "Customers", Href = "/customers" } })" />
```
