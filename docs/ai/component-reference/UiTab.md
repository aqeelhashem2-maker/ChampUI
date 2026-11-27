# UiTab

Registers with the enclosing UiTabs and renders its panel when active. Standalone usage throws.

Namespace `ChampUI.Components.Navigation`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Navigation/UiTab.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Title` | `string` | required | `""` |
| `Icon` | `string?` | optional | `null` |
| `Badge` | `string?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |

Callbacks: none. Child slots: `ChildContent`.

## Example

```razor
<UiTabs Label="Customer"><UiTab Title="History">Visit history</UiTab></UiTabs>
```
