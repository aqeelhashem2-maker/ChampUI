# UiFormField

Wrap custom input markup with help or error text. Check the accessible name of the nested control.

Namespace `ChampUI.Components.Forms`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Forms/UiFormField.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Label` | `string?` | optional | `null` |
| `HelpText` | `string?` | optional | `null` |
| `Error` | `string?` | optional | `null` |
| `Required` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: `ChildContent`.

## Example

```razor
<UiFormField Label="Employee code"><input aria-label="Employee code" /></UiFormField>
```
