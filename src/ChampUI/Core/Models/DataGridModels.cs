namespace ChampUI.Core.Models;

// ============================================================
// DataGrid core request / response types
// All types are intentionally domain-free and reusable.
// ============================================================

/// <summary>
/// Describes how a single column should be sorted.
/// </summary>
public sealed record GridSortDescriptor(string Field, bool Descending);

/// <summary>
/// Operator used for column filters.
/// Text: Contains, NotContains, StartsWith, EndsWith, Equals, NotEquals
/// Numbers: Equals, NotEquals, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, Between
/// Dates: Equals, Before (LessThan), After (GreaterThan), Between
/// Boolean: IsTrue, IsFalse
/// </summary>
public enum GridFilterOperator
{
    Contains,
    NotContains,
    Equals,
    NotEquals,
    StartsWith,
    EndsWith,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,
    /// <summary>Range filter. Use <see cref="GridFilterDescriptor.Value"/> for lower and <see cref="GridFilterDescriptor.Value2"/> for upper bound.</summary>
    Between,
    IsNull,
    IsNotNull,
    IsTrue,
    IsFalse,
    /// <summary>Date semantic alias for LessThan.</summary>
    Before,
    After,
    True,
    False,
}

/// <summary>
/// Defines the scope of data to export from a DataGrid.
/// </summary>
public enum DataGridExportScope
{
    /// <summary>Exports the active loaded page, including rows outside its render window. Unavailable with a virtual range provider.</summary>
    CurrentPage,
    /// <summary>Exports all filtered and sorted items across all pages.</summary>
    AllFilteredRows,
}

/// <summary>
/// Describes a single active column filter.
/// For <see cref="GridFilterOperator.Between"/>, <see cref="Value"/> is the lower bound and <see cref="Value2"/> is the upper bound.
/// </summary>
public sealed class GridFilterDescriptor
{
    public GridFilterDescriptor(string field, GridFilterOperator @operator, object? value, object? value2 = null)
    {
        Field = field;
        Operator = @operator;
        Value = value;
        Value2 = value2;
    }

    public string Field { get; }
    public GridFilterOperator Operator { get; }
    public object? Value { get; }
    /// <summary>Upper bound for <see cref="GridFilterOperator.Between"/> filters. Null for all other operators.</summary>
    public object? Value2 { get; }
}

/// <summary>
/// The request sent to a server-side data provider.
/// </summary>
public sealed class DataGridRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? GlobalSearch { get; init; }
    public IReadOnlyList<GridSortDescriptor> Sorts { get; init; } = Array.Empty<GridSortDescriptor>();
    public IReadOnlyList<GridFilterDescriptor> Filters { get; init; } = Array.Empty<GridFilterDescriptor>();
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// The response returned by a server-side data provider.
/// </summary>
public sealed class DataGridResponse<TItem>
{
    public IReadOnlyList<TItem> Items { get; init; } = Array.Empty<TItem>();
    public int TotalCount { get; init; }

    public static DataGridResponse<TItem> Empty => new();
}

/// <summary>
/// Result of an export-all operation on a server-side grid.
/// </summary>
public sealed class DataGridExportResult<TItem>
{
    public IReadOnlyList<TItem> Items { get; init; } = Array.Empty<TItem>();
}

/// <summary>
/// Request for server-side virtual data provider.
/// Contains range information (StartIndex, Count) for true server-side virtualization.
/// </summary>
public sealed class DataGridVirtualRequest
{
    /// <summary>Zero-based start index of the requested range.</summary>
    public int StartIndex { get; init; }
    /// <summary>Number of items requested (may be adjusted by provider if exceeds limits).</summary>
    public int Count { get; init; } = 50;
    /// <summary>Global search term applied across searchable columns.</summary>
    public string? GlobalSearch { get; init; }
    /// <summary>Sort descriptors (supports multi-column sorting).</summary>
    public IReadOnlyList<GridSortDescriptor> Sorts { get; init; } = Array.Empty<GridSortDescriptor>();
    /// <summary>Column filter descriptors.</summary>
    public IReadOnlyList<GridFilterDescriptor> Filters { get; init; } = Array.Empty<GridFilterDescriptor>();
    /// <summary>Cancellation token for request cancellation.</summary>
    public CancellationToken CancellationToken { get; init; }
}

/// <summary>
/// Response from server-side virtual data provider.
/// Contains the requested range of items and total filtered count.
/// </summary>
public sealed class DataGridVirtualResponse<TItem>
{
    /// <summary>Items for the requested range (may be fewer than requested if at end of dataset).</summary>
    public IReadOnlyList<TItem> Items { get; init; } = Array.Empty<TItem>();
    /// <summary>Total count of filtered records (not just this range).</summary>
    public int TotalCount { get; init; }

    public static DataGridVirtualResponse<TItem> Empty => new();
}
