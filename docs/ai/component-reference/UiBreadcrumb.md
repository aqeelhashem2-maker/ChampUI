# UiBreadcrumb

Pass ordered BreadcrumbItem records. Supply Href for links in the path.

Namespace `ChampUI.Components.Navigation`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Navigation/UiBreadcrumb.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Items` | `IReadOnlyList<BreadcrumbItem>` | required | `Array.Empty<BreadcrumbItem>()` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: none.

## Example

```razor
<UiBreadcrumb Items="@(new BreadcrumbItem[] { new("Home", "/"), new("Customers") })" />
```
