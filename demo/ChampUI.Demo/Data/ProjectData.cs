using ChampUI.Core.Interfaces;
using ChampUI.Core.Models;

namespace ChampUI.Demo.Data;

public sealed record ProjectRow(int Id, string Name, string Category)
{
    public string Name { get; set; } = Name;
}

public sealed class ProjectData : IDataGridDataProvider<ProjectRow>, IDataGridVirtualDataProvider<ProjectRow>
{
    public const int TotalRecords = 120;

    public static List<ProjectRow> CreateRows() => Enumerable.Range(1, TotalRecords)
        .Select(id => new ProjectRow(id, $"Project {id:D3}", id % 2 == 0 ? "Studio" : "Workshop"))
        .ToList();

    private readonly List<ProjectRow> _rows = CreateRows();

    public Task<DataGridResponse<ProjectRow>> GetDataAsync(DataGridRequest request)
    {
        var rows = Match(request.GlobalSearch, request.Filters, request.Sorts, request.CancellationToken);
        return Task.FromResult(new DataGridResponse<ProjectRow>
        {
            TotalCount = rows.Count,
            Items = rows.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList()
        });
    }

    public Task<DataGridVirtualResponse<ProjectRow>> GetVirtualDataAsync(DataGridVirtualRequest request)
    {
        var rows = Match(request.GlobalSearch, request.Filters, request.Sorts, request.CancellationToken);
        return Task.FromResult(new DataGridVirtualResponse<ProjectRow>
        {
            TotalCount = rows.Count,
            Items = rows.Skip(request.StartIndex).Take(request.Count).ToList()
        });
    }

    public Task<DataGridExportResult<ProjectRow>?> GetAllForExportAsync(DataGridRequest request) =>
        Export(request.GlobalSearch, request.Filters, request.Sorts, request.CancellationToken);

    public Task<DataGridExportResult<ProjectRow>?> GetAllForExportAsync(DataGridVirtualRequest request) =>
        Export(request.GlobalSearch, request.Filters, request.Sorts, request.CancellationToken);

    private Task<DataGridExportResult<ProjectRow>?> Export(string? search,
        IReadOnlyList<GridFilterDescriptor> filters, IReadOnlyList<GridSortDescriptor> sorts, CancellationToken token) =>
        Task.FromResult<DataGridExportResult<ProjectRow>?>(new()
        {
            Items = Match(search, filters, sorts, token)
        });

    private List<ProjectRow> Match(string? search, IReadOnlyList<GridFilterDescriptor> filters,
        IReadOnlyList<GridSortDescriptor> sorts, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        IEnumerable<ProjectRow> rows = _rows;
        if (!string.IsNullOrWhiteSpace(search))
            rows = rows.Where(row => row.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                     row.Category.Contains(search, StringComparison.OrdinalIgnoreCase));
        foreach (var filter in filters.Where(filter => filter.Field == nameof(ProjectRow.Category)))
        {
            var value = filter.Value?.ToString() ?? "";
            rows = rows.Where(row => filter.Operator switch
            {
                GridFilterOperator.Equals => row.Category.Equals(value, StringComparison.OrdinalIgnoreCase),
                GridFilterOperator.StartsWith => row.Category.StartsWith(value, StringComparison.OrdinalIgnoreCase),
                GridFilterOperator.EndsWith => row.Category.EndsWith(value, StringComparison.OrdinalIgnoreCase),
                _ => row.Category.Contains(value, StringComparison.OrdinalIgnoreCase)
            });
        }

        var sort = sorts.FirstOrDefault();
        return (sort?.Field, sort?.Descending) switch
        {
            (nameof(ProjectRow.Name), true) => rows.OrderByDescending(row => row.Name).ToList(),
            (nameof(ProjectRow.Name), false) => rows.OrderBy(row => row.Name).ToList(),
            (_, true) => rows.OrderByDescending(row => row.Id).ToList(),
            _ => rows.OrderBy(row => row.Id).ToList()
        };
    }
}
