# UiSidebarItem

Href is matched with Blazor NavLinkMatch.Prefix by default. Use All for an exact route.

Namespace `ChampUI.Components.Layout`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Layout/UiSidebarItem.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Href` | `string` | required | `""` |
| `Text` | `string` | required | `""` |
| `Icon` | `string` | optional | `"layers"` |
| `Badge` | `string?` | optional | `null` |
| `Match` | `NavLinkMatch` | optional | `NavLinkMatch.Prefix` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: none.

## Example

```razor
<UiSidebarItem Href="/customers" Text="Customers" />
```
