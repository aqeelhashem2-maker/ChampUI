# UiIcon

Unknown names produce an empty SVG. Omit Label for a decorative icon alongside visible text.

Namespace `ChampUI.Components.Display`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Display/UiIcon.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Name` | `string` | required | `""` |
| `Size` | `int` | optional | `18` |
| `Flip` | `bool` | optional | `false` |
| `Label` | `string?` | optional | `null` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: none.

## Example

```razor
<UiIcon Name="calendar" Label="Appointment" />
```
