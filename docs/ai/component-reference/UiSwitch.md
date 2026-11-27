# UiSwitch

Use Checked and CheckedChanged for a binary setting.

Namespace `ChampUI.Components.Forms`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Forms/UiSwitch.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Checked` | `bool` | optional | `false` |
| `CheckedChanged` | `EventCallback<bool>` | optional | `default (no delegate)` |
| `Label` | `string?` | optional | `null` |
| `Disabled` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |

Callbacks: `CheckedChanged`. Child slots: none.

## Example

```razor
<UiSwitch Label="Receive reminders" @bind-Checked="reminders" />
@code { private bool reminders; }
```
