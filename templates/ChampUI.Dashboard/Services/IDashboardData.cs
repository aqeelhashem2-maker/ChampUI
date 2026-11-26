using ChampUI.Starter.Models;

namespace ChampUI.Starter.Services;

// A scoped, asynchronous boundary for replacing the fictional data with a backend.
public interface IDashboardData
{
    Task<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Customer>> GetCustomersAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Order>> GetOrdersAsync(CancellationToken cancellationToken);
    Task<AnalyticsSnapshot> GetAnalyticsAsync(CancellationToken cancellationToken);
}
