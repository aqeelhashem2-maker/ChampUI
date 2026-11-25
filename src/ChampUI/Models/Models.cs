namespace ChampUI.Models;

public sealed record BreadcrumbItem(string Text, string? Href = null);

public sealed record SelectOption<TValue>(TValue Value, string Text);

/// <summary>A single entry of a <c>UiMenu</c>.</summary>
public sealed class UiMenuItem
{
    public string Text { get; init; } = "";
    public string? Icon { get; init; }
    public string? Href { get; init; }
    public Func<Task>? OnSelect { get; init; }
    public bool Danger { get; init; }
    public bool Disabled { get; init; }
    public bool SeparatorBefore { get; init; }
}

/// <summary>A named series of values for line and bar charts. Values align with the chart's labels.</summary>
public sealed class ChartSeries
{
    public string Name { get; init; } = "";
    public IReadOnlyList<double> Values { get; init; } = Array.Empty<double>();
    /// <summary>Any CSS colour. When null, the chart palette tokens (--ui-chart-1..6) are used.</summary>
    public string? Color { get; init; }
}

/// <summary>A labelled slice of a donut chart.</summary>
public sealed class ChartSegment
{
    public string Label { get; init; } = "";
    public double Value { get; init; }
    public string? Color { get; init; }
}

public enum ToastKind { Info, Success, Warning, Danger }

public sealed class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public string? Title { get; init; }
    public string Text { get; init; } = "";
    public ToastKind Kind { get; init; } = ToastKind.Info;
    public TimeSpan Duration { get; init; } = TimeSpan.FromSeconds(4);
}
