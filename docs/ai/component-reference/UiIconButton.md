# UiIconButton

Supply the action name for screen readers even when the icon is familiar.

Namespace `ChampUI.Components.Actions`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Actions/UiIconButton.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Icon` | `string` | required | `""` |
| `Label` | `string` | required | `""` |
| `Outline` | `bool` | optional | `false` |
| `Primary` | `bool` | optional | `false` |
| `Small` | `bool` | optional | `false` |
| `Disabled` | `bool` | optional | `false` |
| `Indicator` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |
| `OnClick` | `EventCallback<MouseEventArgs>` | optional | `default (no delegate)` |
| `Attributes` | `Dictionary<string, object>?` | captures HTML attributes | `null` |

Callbacks: `OnClick`. Child slots: none.

## Example

```razor
<UiIconButton Icon="edit" Label="Edit customer" />
```
