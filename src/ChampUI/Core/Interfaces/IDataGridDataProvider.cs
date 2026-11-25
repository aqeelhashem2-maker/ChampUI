using ChampUI.Core.Models;

namespace ChampUI.Core.Interfaces;

/// <summary>
/// Server-side data provider for <see cref="ChampUI.Components.Data.UiDataGrid{TItem}"/>.
/// Implement this interface to support paging, sorting and filtering on the server.
/// </summary>
public interface IDataGridDataProvider<TItem>
{
    /// <summary>
    /// Called by the grid whenever the user changes the page, sort or filter state.
    /// </summary>
    Task<DataGridResponse<TItem>> GetDataAsync(DataGridRequest request);

    /// <summary>
    /// Optional: Returns all filtered items for export purposes.
    /// If not implemented or returns null, AllFilteredRows export scope will be unavailable.
    /// An attempted full export displays an availability message without downloading a partial result.
    /// </summary>
    /// <param name="request">Contains search, filters, and sorts (paging is ignored for full export).</param>
    /// <returns>All items matching the filters, or null if full export is not supported.</returns>
    Task<DataGridExportResult<TItem>?> GetAllForExportAsync(DataGridRequest request) => Task.FromResult<DataGridExportResult<TItem>?>(null);
}

/// <summary>
/// Server-side virtual data provider for <see cref="ChampUI.Components.Data.UiDataGrid{TItem}"/>.
/// Implement this interface to support true server-side row virtualization where only
/// the visible range of records is fetched from the server.
/// This is distinct from page-level virtualization, which virtualizes rendering of an already-loaded page.
/// </summary>
public interface IDataGridVirtualDataProvider<TItem>
{
    /// <summary>
    /// Called when the grid needs to fetch a specific range of records.
    /// The provider must return exactly the requested range based on StartIndex and Count,
    /// applying the specified search, sort, and filter criteria.
    /// </summary>
    /// <param name="request">Contains the range (StartIndex, Count), search, filters, sorts, and cancellation token.</param>
    /// <returns>The requested range of items and the total count of filtered records.</returns>
    Task<DataGridVirtualResponse<TItem>> GetVirtualDataAsync(DataGridVirtualRequest request);

    /// <summary>
    /// Returns all rows matching the supplied search and filters, in the requested sort order.
    /// The grid sends StartIndex and Count as zero. The range used for rendering is independent.
    /// Return null when unsupported. The grid displays an availability message without downloading.
    /// </summary>
    Task<DataGridExportResult<TItem>?> GetAllForExportAsync(DataGridVirtualRequest request) => Task.FromResult<DataGridExportResult<TItem>?>(null);
}
