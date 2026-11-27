# UiTabs

ActiveIndex is zero-based. Declare each panel with UiTab inside the group.

Namespace `ChampUI.Components.Navigation`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Navigation/UiTabs.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `ActiveIndex` | `int` | optional | `0` |
| `ActiveIndexChanged` | `EventCallback<int>` | optional | `default (no delegate)` |
| `Label` | `string?` | optional | `null` |
| `Class` | `string?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |

Callbacks: `ActiveIndexChanged`. Child slots: `ChildContent`.

## Example

```razor
<UiTabs Label="Account"><UiTab Title="Details">Customer details</UiTab></UiTabs>
```
