# UiNumberField

Value is a nullable double. Convert and validate currency in domain code before saving.

Namespace `ChampUI.Components.Forms`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Forms/UiNumberField.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Value` | `double?` | optional | `null` |
| `ValueChanged` | `EventCallback<double?>` | optional | `default (no delegate)` |
| `Min` | `double?` | optional | `null` |
| `Max` | `double?` | optional | `null` |
| `Step` | `double` | optional | `1` |
| `Label` | `string?` | optional | `null` |
| `HelpText` | `string?` | optional | `null` |
| `Disabled` | `bool` | optional | `false` |
| `DecreaseLabel` | `string` | optional | `"Decrease"` |
| `IncreaseLabel` | `string` | optional | `"Increase"` |
| `Class` | `string?` | optional | `null` |

Callbacks: `ValueChanged`. Child slots: none.

## Example

```razor
<UiNumberField Label="Hours" Min="0" Step="0.5" @bind-Value="hours" />
@code { private double? hours; }
```
