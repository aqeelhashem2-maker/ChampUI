using System.Globalization;

namespace ChampUI.Components.Charts;

internal static class ChartHelpers
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string F(double v) => v.ToString("0.##", Inv);

    /// <summary>Picks a "nice" tick step so the axis ends on a round number.</summary>
    public static (double step, double max) NiceScale(double dataMax, int intervals = 4)
    {
        if (dataMax <= 0) return (1, intervals);
        var raw = dataMax / intervals;
        var pow = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        var norm = raw / pow;
        var nice = norm <= 1 ? 1 : norm <= 2 ? 2 : norm <= 2.5 ? 2.5 : norm <= 5 ? 5 : 10;
        var step = nice * pow;
        return (step, step * intervals);
    }

    public static string Color(string? explicitColor, int index) =>
        !string.IsNullOrWhiteSpace(explicitColor) ? explicitColor : $"var(--ui-chart-{index % 6 + 1})";

    public static string CompactAxis(double v)
    {
        var abs = Math.Abs(v);
        if (abs >= 1_000_000) return (v / 1_000_000).ToString("0.#", Inv) + "M";
        if (abs >= 1_000) return (v / 1_000).ToString("0.#", Inv) + "k";
        return v.ToString("0.#", Inv);
    }

    public static string DefaultValue(double v) => v.ToString("N0", CultureInfo.CurrentCulture);
}
