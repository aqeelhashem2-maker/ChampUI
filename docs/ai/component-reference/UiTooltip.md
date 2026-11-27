# UiTooltip

Wrap the element being described. Keep important instructions visible independently of hover.

Namespace `ChampUI.Components.Display`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Display/UiTooltip.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Text` | `string` | required | `""` |
| `Class` | `string?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |

Callbacks: none. Child slots: `ChildContent`.

## Example

```razor
<UiTooltip Text="Open the customer record"><a href="/customers">Customers</a></UiTooltip>
```
