# UiProgress

Max sets the scale. The displayed percentage is clamped to the range 0–100.

Namespace `ChampUI.Components.Display`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Display/UiProgress.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Value` | `double` | optional | `0` |
| `Max` | `double` | optional | `100` |
| `Variant` | `UiVariant` | optional | `UiVariant.Primary` |
| `Label` | `string?` | optional | `null` |
| `ShowValue` | `bool` | optional | `false` |
| `HideLabel` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: none.

## Example

```razor
<UiProgress Label="Booked capacity" Value="7" Max="10" ShowValue="true" />
```
