# UiSelect

TValue is the domain value type. Each SelectOption has a typed Value and display Text. The native option carries a list index and raises ValueChanged with the selected typed value. `EqualityComparer<TValue>.Default` matches the current option.

Namespace `ChampUI.Components.Forms`. Generic parameters: TValue. [Razor source](../../../src/ChampUI/Components/Forms/UiSelect.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Options` | `IEnumerable<SelectOption<TValue>>` | required | `Array.Empty<SelectOption<TValue>>()` |
| `Value` | `TValue?` | optional | `default(TValue)` |
| `ValueChanged` | `EventCallback<TValue>` | optional | `default (no delegate)` |
| `Label` | `string?` | optional | `null` |
| `Icon` | `string?` | optional | `null` |
| `AriaLabel` | `string?` | optional | `null` |
| `HelpText` | `string?` | optional | `null` |
| `Disabled` | `bool` | optional | `false` |
| `Small` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |

Callbacks: `ValueChanged`. Child slots: none.

## Example

```razor
<UiSelect TValue="int" Label="Department" Options="departments" @bind-Value="department" />
@code {
    private int department = 1;
    private readonly SelectOption<int>[] departments = [new(1, "Sales"), new(2, "Support")];
}
```
