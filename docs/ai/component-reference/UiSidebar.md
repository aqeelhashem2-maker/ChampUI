# UiSidebar

Place navigation links in ChildContent and branding in Brand.

Namespace `ChampUI.Components.Layout`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Layout/UiSidebar.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Brand` | `RenderFragment?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Footer` | `RenderFragment?` | optional | `null` |
| `Label` | `string` | optional | `"Main navigation"` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: `Brand`, `ChildContent`, `Footer`.

## Example

```razor
<UiSidebar Label="Workspace"><ChildContent><UiSidebarItem Href="/orders" Text="Orders" /></ChildContent></UiSidebar>
```
