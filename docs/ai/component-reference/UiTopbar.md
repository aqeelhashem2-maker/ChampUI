# UiTopbar

The menu toggle works with the surrounding shell. Supply trailing controls in End.

Namespace `ChampUI.Components.Layout`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Layout/UiTopbar.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Start` | `RenderFragment?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `End` | `RenderFragment?` | optional | `null` |
| `MenuLabel` | `string` | optional | `"Toggle navigation"` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: `Start`, `ChildContent`, `End`.

## Example

```razor
<UiTopbar><Start>Operations</Start><End>Sales team</End></UiTopbar>
```
