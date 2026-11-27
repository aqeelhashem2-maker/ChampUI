# UiCheckbox

Bind Checked to retain the selected boolean on subsequent renders.

Namespace `ChampUI.Components.Forms`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Forms/UiCheckbox.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Checked` | `bool` | optional | `false` |
| `CheckedChanged` | `EventCallback<bool>` | optional | `default (no delegate)` |
| `Label` | `string?` | optional | `null` |
| `Disabled` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Attributes` | `Dictionary<string, object>?` | captures HTML attributes | `null` |

Callbacks: `CheckedChanged`. Child slots: `ChildContent`.

## Example

```razor
<UiCheckbox Label="Active employee" @bind-Checked="active" />
@code { private bool active; }
```
