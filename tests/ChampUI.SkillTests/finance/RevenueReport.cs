namespace SkillDashboard.Services;

public sealed record MonthRevenue(DateOnly Month, decimal Amount);

public static class RevenueReport
{
    public static IReadOnlyList<MonthRevenue> Months { get; } =
    [
        new(new(2026, 1, 1), 1200.25m),
        new(new(2026, 2, 1), 850.40m),
        new(new(2026, 3, 1), 1200.10m)
    ];

    public static decimal Total => Months.Sum(month => month.Amount);
}
