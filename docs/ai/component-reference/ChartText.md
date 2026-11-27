# ChartText

SVG axis-label helper used by the chart components. Place it inside an SVG coordinate system.

Namespace `ChampUI.Components.Charts`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Charts/ChartText.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `X` | `string` | optional | `"0"` |
| `Y` | `string` | optional | `"0"` |
| `Anchor` | `string` | optional | `"middle"` |
| `ChildContent` | `RenderFragment?` | optional | `null` |

Callbacks: none. Child slots: `ChildContent`.

## Example

```razor
<svg viewBox="0 0 100 30"><ChartText X="50" Y="20">Q1</ChartText></svg>
```
