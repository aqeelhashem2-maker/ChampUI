# UiCard

Actions and Footer are named fragments. Interactive changes presentation. Wire an actual accessible action when navigation is needed.

Namespace `ChampUI.Components.Layout`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Layout/UiCard.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Title` | `string?` | optional | `null` |
| `Subtitle` | `string?` | optional | `null` |
| `Flush` | `bool` | optional | `false` |
| `Interactive` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |
| `BodyClass` | `string?` | optional | `null` |
| `Actions` | `RenderFragment?` | optional | `null` |
| `Footer` | `RenderFragment?` | optional | `null` |
| `ChildContent` | `RenderFragment?` | optional | `null` |
| `Attributes` | `Dictionary<string, object>?` | captures HTML attributes | `null` |

Callbacks: none. Child slots: `Actions`, `Footer`, `ChildContent`.

## Example

```razor
<UiCard Title="Customer"><ChildContent>Sara Ali</ChildContent><Footer>Active account</Footer></UiCard>
```
