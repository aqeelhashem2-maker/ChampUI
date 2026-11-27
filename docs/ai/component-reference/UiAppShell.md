# UiAppShell

Places the page in main, supplies the navigation cascade, and renders theme overrides and a toast host. Language and Direction are independent.

Namespace `ChampUI.Components.Layout`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Layout/UiAppShell.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Sidebar` | `RenderFragment?` | optional | `null` |
| `Topbar` | `RenderFragment?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Direction` | `UiTextDirection` | optional | `UiTextDirection.Ltr` |
| `Language` | `string` | optional | `"en"` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: `Sidebar`, `Topbar`, `ChildContent`.

## Example

```razor
<UiAppShell Language="ar" Direction="UiTextDirection.Rtl"><ChildContent>المواعيد</ChildContent></UiAppShell>
```
