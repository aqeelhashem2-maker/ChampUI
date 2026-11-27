# UiDonutChart

Pass labeled numeric slices. CenterContent allows custom center markup. Include a readable breakdown alongside a chart when exact values matter.

Namespace `ChampUI.Components.Charts`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Charts/UiDonutChart.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Segments` | `IReadOnlyList<ChartSegment>` | required | `Array.Empty<ChartSegment>()` |
| `Size` | `int` | optional | `188` |
| `Thickness` | `int` | optional | `22` |
| `CenterValue` | `string?` | optional | `null` |
| `CenterLabel` | `string?` | optional | `null` |
| `CenterContent` | `RenderFragment?` | optional | `null` |
| `ShowLegend` | `bool` | optional | `true` |
| `AriaLabel` | `string` | optional | `"Donut chart"` |
| `ValueFormatter` | `Func<double, string>` | optional | `DefaultValue` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: `CenterContent`.

## Example

```razor
<UiDonutChart Segments="@(new[] { new ChartSegment { Label = "Services", Value = 75 }, new ChartSegment { Label = "Retail", Value = 25 } })" AriaLabel="Revenue share" />
```

The center shows the formatted total unless CenterValue or CenterContent overrides it. The legend displays values and percentages. Supply nonnegative slice values and a thickness smaller than Size. The default formatter rounds displayed values to whole numbers.
