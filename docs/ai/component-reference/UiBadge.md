# UiBadge

Use text as well as color to communicate status.

Namespace `ChampUI.Components.Display`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Display/UiBadge.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Variant` | `UiVariant` | optional | `UiVariant.Neutral` |
| `Dot` | `bool` | optional | `false` |
| `Small` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Attributes` | `Dictionary<string, object>?` | captures HTML attributes | `null` |

Callbacks: none. Child slots: `ChildContent`.

## Example

```razor
<UiBadge Variant="UiVariant.Success" Dot="true">Paid</UiBadge>
```
