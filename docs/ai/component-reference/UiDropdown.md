# UiDropdown

Trigger renders inside a button. Panel clicks close it by default. Use CloseOnSelect=false for multi-step interaction.

Namespace `ChampUI.Components.Navigation`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Navigation/UiDropdown.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Trigger` | `RenderFragment?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Align` | `UiAlign` | optional | `UiAlign.Start` |
| `TriggerClass` | `string` | optional | `"ui-btn ui-btn--secondary"` |
| `Label` | `string?` | optional | `null` |
| `Class` | `string?` | optional | `null` |
| `CloseOnSelect` | `bool` | optional | `true` |
| `OpenChanged` | `EventCallback<bool>` | optional | `default (no delegate)` |

Callbacks: `OpenChanged`. Child slots: `Trigger`, `ChildContent`.

## Example

```razor
<UiDropdown Label="Workspace links"><Trigger>Navigate</Trigger><ChildContent><a href="/orders" role="menuitem">Orders</a></ChildContent></UiDropdown>
```
