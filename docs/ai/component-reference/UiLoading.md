# UiLoading

Use Overlay inside the region whose loading state it describes.

Namespace `ChampUI.Components.Feedback`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Feedback/UiLoading.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Label` | `string?` | optional | `"Loading…"` |
| `Size` | `UiSize` | optional | `UiSize.Medium` |
| `Overlay` | `bool` | optional | `false` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: none.

## Example

```razor
<UiLoading Label="Loading appointments…" />
```
