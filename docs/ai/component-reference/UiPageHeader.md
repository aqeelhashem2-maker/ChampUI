# UiPageHeader

Title is required. Actions and Badge accept fragments and Breadcrumbs accepts data.

Namespace `ChampUI.Components.Layout`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Layout/UiPageHeader.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Title` | `string` | required | `""` |
| `Subtitle` | `string?` | optional | `null` |
| `Breadcrumbs` | `IReadOnlyList<BreadcrumbItem>?` | optional | `null` |
| `Badge` | `RenderFragment?` | optional | `null` |
| `Actions` | `RenderFragment?` | optional | `null` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: `Badge`, `Actions`.

## Example

```razor
<UiPageHeader Title="Appointments" Subtitle="Today's visits" />
```
