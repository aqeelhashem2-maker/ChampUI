using ChampUI.Starter.Models;

namespace ChampUI.Starter.Services;

public sealed class SampleDashboardData : IDashboardData
{
    private static readonly Customer[] Customers =
    [
        new(1, "Maya Chen", "Lumen Studio", "maya@example.com", CustomerStatus.Active, new(2026, 1, 12)),
        new(2, "Omar Nasser", "Northline Co.", "omar@example.com", CustomerStatus.Active, new(2026, 2, 8)),
        new(3, "Sofia Martin", "Fieldwork Supply", "sofia@example.com", CustomerStatus.Onboarding, new(2026, 6, 11)),
        new(4, "Daniel Brooks", "Juniper Works", "daniel@example.com", CustomerStatus.Active, new(2026, 3, 3)),
        new(5, "Leila Haddad", "Arc & Oak", "leila@example.com", CustomerStatus.Inactive, new(2026, 1, 24)),
        new(6, "Noah Williams", "Forma Collective", "noah@example.com", CustomerStatus.Active, new(2026, 3, 18)),
        new(7, "Amira Saleh", "Cedar House", "amira@example.com", CustomerStatus.Onboarding, new(2026, 6, 17)),
        new(8, "Elena Rossi", "Morrow Design", "elena@example.com", CustomerStatus.Active, new(2026, 4, 7)),
        new(9, "Samir Patel", "Madewell Labs", "samir@example.com", CustomerStatus.Inactive, new(2026, 2, 15)),
        new(10, "Ivy Tran", "Slate Office", "ivy@example.com", CustomerStatus.Active, new(2026, 4, 20)),
        new(11, "Adam Reed", "Bridgepoint", "adam@example.com", CustomerStatus.Active, new(2026, 5, 4)),
        new(12, "Noura Ali", "Opal Workshop", "noura@example.com", CustomerStatus.Onboarding, new(2026, 6, 19))
    ];

    private static readonly Order[] Orders =
    [
        new(1048, Customers[0].Company, "Workspace supplies", new(2026, 6, 24), 1240m, OrderStatus.Completed),
        new(1047, Customers[1].Company, "Equipment order", new(2026, 6, 23), 875m, OrderStatus.Processing),
        new(1046, Customers[2].Company, "Materials order", new(2026, 6, 22), 2160m, OrderStatus.Completed),
        new(1045, Customers[3].Company, "Office essentials", new(2026, 6, 20), 465m, OrderStatus.Pending),
        new(1044, Customers[4].Company, "Materials order", new(2026, 6, 19), 910m, OrderStatus.Completed),
        new(1043, Customers[5].Company, "Equipment order", new(2026, 6, 18), 780m, OrderStatus.Completed),
        new(1042, Customers[6].Company, "Workspace supplies", new(2026, 6, 18), 335m, OrderStatus.Processing),
        new(1041, Customers[7].Company, "Office essentials", new(2026, 6, 16), 560m, OrderStatus.Completed),
        new(1040, Customers[8].Company, "Materials order", new(2026, 6, 14), 1380m, OrderStatus.Pending),
        new(1039, Customers[9].Company, "Equipment order", new(2026, 6, 10), 610m, OrderStatus.Completed)
    ];

    private static readonly MonthMetrics[] Months =
    [
        new(new(2026, 1, 1), 31200m, 27400m, 124, 301),
        new(new(2026, 2, 1), 35600m, 30200m, 169, 348),
        new(new(2026, 3, 1), 32900m, 31500m, 184, 335),
        new(new(2026, 4, 1), 42100m, 35300m, 216, 412),
        new(new(2026, 5, 1), 39700m, 36700m, 238, 391),
        new(new(2026, 6, 1), 48250m, 42300m, 286, 476)
    ];

    private static readonly Activity[] RecentActivity =
    [
        new("Lumen Studio order completed", "اكتمل طلب لومين ستوديو", new(2026, 6, 24, 14, 0, 0, TimeSpan.Zero)),
        new("Northline Co. order moved to processing", "انتقل طلب نورث لاين إلى المعالجة", new(2026, 6, 23, 11, 0, 0, TimeSpan.Zero)),
        new("Fieldwork Supply order completed", "اكتمل طلب فيلد وورك", new(2026, 6, 22, 15, 0, 0, TimeSpan.Zero)),
        new("Opal Workshop joined", "انضمت أوبال وركشوب", new(2026, 6, 19, 9, 0, 0, TimeSpan.Zero))
    ];

    public Task<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new DashboardSnapshot(Months, Orders.Take(4).ToArray(), RecentActivity));
    }

    public Task<IReadOnlyList<Customer>> GetCustomersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Customer>>(Customers);
    }

    public Task<IReadOnlyList<Order>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<Order>>(Orders);
    }

    public Task<AnalyticsSnapshot> GetAnalyticsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var counts = Orders.GroupBy(order => order.Status).ToDictionary(group => group.Key, group => group.Count());
        return Task.FromResult(new AnalyticsSnapshot(Months, counts));
    }
}
