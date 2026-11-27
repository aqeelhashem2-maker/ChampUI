# UiTextArea

Rows controls visible height. Use MaxLength when showing a character budget.

Namespace `ChampUI.Components.Forms`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Forms/UiTextArea.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Value` | `string?` | optional | `null` |
| `ValueChanged` | `EventCallback<string?>` | optional | `default (no delegate)` |
| `Label` | `string?` | optional | `null` |
| `Placeholder` | `string?` | optional | `null` |
| `HelpText` | `string?` | optional | `null` |
| `Error` | `string?` | optional | `null` |
| `Disabled` | `bool` | optional | `false` |
| `ReadOnly` | `bool` | optional | `false` |
| `Required` | `bool` | optional | `false` |
| `Rows` | `int` | optional | `4` |
| `MaxLength` | `int` | optional | `0` |
| `ShowCharCount` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |
| `Attributes` | `Dictionary<string, object>?` | captures HTML attributes | `null` |

Callbacks: `ValueChanged`. Child slots: none.

## Example

```razor
<UiTextArea Label="Notes" Rows="3" MaxLength="200" ShowCharCount="true" @bind-Value="notes" />
@code { private string? notes; }
```
