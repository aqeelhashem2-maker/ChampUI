# UiTextField

Updates on each input event. With explicit ValueChanged, assign the new string before awaiting other work. For asynchronous search, cancel or ignore obsolete responses. OnEnter is a separate callback.

Namespace `ChampUI.Components.Forms`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Forms/UiTextField.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Value` | `string?` | optional | `null` |
| `ValueChanged` | `EventCallback<string?>` | optional | `default (no delegate)` |
| `Label` | `string?` | optional | `null` |
| `Placeholder` | `string?` | optional | `null` |
| `Type` | `string` | optional | `"text"` |
| `Icon` | `string?` | optional | `null` |
| `HelpText` | `string?` | optional | `null` |
| `Error` | `string?` | optional | `null` |
| `Disabled` | `bool` | optional | `false` |
| `Small` | `bool` | optional | `false` |
| `AutoComplete` | `string` | optional | `"off"` |
| `Class` | `string?` | optional | `null` |
| `OnEnter` | `EventCallback<string?>` | optional | `default (no delegate)` |
| `Attributes` | `Dictionary<string, object>?` | captures HTML attributes | `null` |

Callbacks: `ValueChanged`, `OnEnter`. Child slots: none.

## Example

```razor
<UiTextField Label="Find customer" Value="@query" ValueChanged="UpdateQuery" />
@code {
    private string? query;
    private void UpdateQuery(string? value) => query = value;
}
```

Use @bind-Value for simple forms. For a remote lookup, track the request separately from the text currently displayed. An older search result should never reassign the current Value. Exercise rapid typing, deletion, paste and Enter with a slow server connection. The control uses Blazor input-event binding and reports the current string through OnEnter.
