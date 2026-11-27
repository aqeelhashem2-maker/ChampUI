# Financial KPI dashboard

Keep accounting values as decimal and format the KPI with an explicit currency. Convert only the chart display series to double. Pair the chart with a readable summary.

```razor
<UiStatCard Label="Quarter revenue (USD)" Value="@Total" Trend="UiTrend.Up" />
<UiBarChart Labels="months" Series="series" AriaLabel="Revenue by month in US dollars" />
<p>Revenue increased from $1,200 in January to $1,700 in March.</p>
@code {
    private static readonly decimal[] revenue = [1200m, 1400m, 1700m];
    private readonly string[] months = ["January", "February", "March"];
    private string Total => revenue.Sum().ToString("C0", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
    private readonly ChartSeries[] series = [new() { Name = "USD", Values = revenue.Select(v => (double)v).ToArray() }];
}
```
