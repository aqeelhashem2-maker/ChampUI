# UiAreaChart

Compare filled trends. Provide one value per label for each series. Labels and Values align by index. Use AriaLabel and provide a textual summary.

Namespace `ChampUI.Components.Charts`. Generic parameters: none. [Razor source](../../../src/ChampUI/Components/Charts/UiAreaChart.razor).

| Parameter | C# type | Supply | Initial value |
| --- | --- | --- | --- |
| `Series` | `IReadOnlyList<ChartSeries>` | required | `Array.Empty<ChartSeries>()` |
| `Labels` | `IReadOnlyList<string>` | required | `Array.Empty<string>()` |
| `Height` | `double` | optional | `300` |
| `ShowLegend` | `bool` | optional | `true` |
| `AriaLabel` | `string` | optional | `"Area chart"` |
| `ValueFormatter` | `Func<double, string>` | optional | `DefaultValue` |
| `AxisFormatter` | `Func<double, string>` | optional | `CompactAxis` |
| `Class` | `string?` | optional | `null` |

Callbacks: none. Child slots: none.

## Example

```razor
<UiAreaChart Series="sales" Labels="months" AriaLabel="Monthly sales" />
@code {
    private readonly string[] months = ["Jan", "Feb"];
    private readonly ChartSeries[] sales = [new() { Name = "Revenue", Values = [1200, 1700] }];
}
```

## Formatting and scale

The default value formatter displays whole numbers in the current culture. The compact axis formatter abbreviates thousands and millions with k and M. Supply ValueFormatter and AxisFormatter for fractional values or localized currency. Set ChartSeries.Color to override the cycling six-color palette. Include units in the accessible label.
