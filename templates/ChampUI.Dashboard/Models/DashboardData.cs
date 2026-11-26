namespace ChampUI.Starter.Models;

public enum CustomerStatus { Active, Onboarding, Inactive }
public enum OrderStatus { Completed, Processing, Pending }

public sealed record Customer(int Id, string Name, string Company, string Email,
    CustomerStatus Status, DateOnly Joined);

public sealed record Order(int Id, string Customer, string Description, DateOnly Placed,
    decimal Total, OrderStatus Status)
{
    public string Number => $"ORD-{Id}";
}

public sealed record MonthMetrics(DateOnly Month, decimal Revenue, decimal PreviousRevenue,
    int Customers, int Orders);

public sealed record Activity(string English, string Arabic, DateTimeOffset Occurred);

public sealed record DashboardSnapshot(IReadOnlyList<MonthMetrics> Months,
    IReadOnlyList<Order> RecentOrders, IReadOnlyList<Activity> Activity);

public sealed record AnalyticsSnapshot(IReadOnlyList<MonthMetrics> Months,
    IReadOnlyDictionary<OrderStatus, int> OrderCounts);
