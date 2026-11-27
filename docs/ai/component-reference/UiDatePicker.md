# UiDatePicker

A native date input uses DateOnly? with no time or timezone information.

Namespace `ChampUI.Components.Forms`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Forms/UiDatePicker.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Value` | `DateOnly?` | optional | `null` |
| `ValueChanged` | `EventCallback<DateOnly?>` | optional | `default (no delegate)` |
| `Min` | `DateOnly?` | optional | `null` |
| `Max` | `DateOnly?` | optional | `null` |
| `Label` | `string?` | optional | `null` |
| `HelpText` | `string?` | optional | `null` |
| `Disabled` | `bool` | optional | `false` |
| `Small` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |

Callbacks: `ValueChanged`. Child slots: none.

## Example

```razor
<UiDatePicker Label="Visit date" @bind-Value="day" />
@code { private DateOnly? day; }
```
