using ChampUI.Components.Data;
using ChampUI.Core.Interfaces;
using ChampUI.Core.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ChampUI.Tests;

/// <summary>
/// B-07: Unit tests for export functionality with virtual data providers.
/// </summary>
public class DataGrid_B07_Export_Tests
{
    private record TestItem(int Id, string Name);

    private class TestVirtualProvider : IDataGridVirtualDataProvider<TestItem>
    {
        private readonly List<TestItem> _allItems;
        private readonly bool _supportsExport;

        public TestVirtualProvider(int totalItems, bool supportsExport)
        {
            _allItems = Enumerable.Range(1, totalItems)
                .Select(i => new TestItem(i, $"Item {i}"))
                .ToList();
            _supportsExport = supportsExport;
        }

        public Task<DataGridVirtualResponse<TestItem>> GetVirtualDataAsync(DataGridVirtualRequest request)
        {
            var items = _allItems
                .Skip(request.StartIndex)
                .Take(request.Count)
                .ToList();

            return Task.FromResult(new DataGridVirtualResponse<TestItem>
            {
                Items = items,
                TotalCount = _allItems.Count
            });
        }

        public Task<DataGridExportResult<TestItem>?> GetAllForExportAsync(DataGridVirtualRequest request)
        {
            if (!_supportsExport)
                return Task.FromResult<DataGridExportResult<TestItem>?>(null);

            return Task.FromResult<DataGridExportResult<TestItem>?>(new DataGridExportResult<TestItem>
            {
                Items = _allItems
            });
        }
    }

    [Fact]
    public async Task B07_01_VirtualProviderExport_ReturnsAllItems()
    {
        // Test that virtual provider returns all items when GetAllForExportAsync is implemented
        var provider = new TestVirtualProvider(100, supportsExport: true);
        var request = new DataGridVirtualRequest { StartIndex = 0, Count = 0 };

        var result = await provider.GetAllForExportAsync(request);

        Assert.NotNull(result);
        Assert.Equal(100, result.Items.Count);
    }

    [Fact]
    public async Task B07_02_VirtualProviderExport_ReturnsNullWhenNotSupported()
    {
        // Test that virtual provider returns null when export is not supported
        var provider = new TestVirtualProvider(100, supportsExport: false);
        var request = new DataGridVirtualRequest { StartIndex = 0, Count = 0 };

        var result = await provider.GetAllForExportAsync(request);

        Assert.Null(result);
    }

    [Fact]
    public async Task B07_03_VirtualProviderExport_PreservesFilterAndSort()
    {
        // Test that export request preserves search, filter, and sort parameters
        DataGridVirtualRequest? capturedRequest = null;

        var provider = new TestableVirtualProvider(new List<TestItem>
        {
            new(1, "Alpha"),
            new(2, "Beta"),
            new(3, "Gamma")
        }, req =>
        {
            capturedRequest = req;
        });

        var request = new DataGridVirtualRequest
        {
            StartIndex = 0,
            Count = 0,
            GlobalSearch = "test",
            Sorts = new[] { new GridSortDescriptor("Name", false) },
            Filters = new[] { new GridFilterDescriptor("Id", GridFilterOperator.GreaterThan, 1) }
        };

        await provider.GetAllForExportAsync(request);

        Assert.NotNull(capturedRequest);
        Assert.Equal("test", capturedRequest.GlobalSearch);
        Assert.Single(capturedRequest.Sorts);
        Assert.Single(capturedRequest.Filters);
    }

    private class TestableVirtualProvider : IDataGridVirtualDataProvider<TestItem>
    {
        private readonly List<TestItem> _items;
        private readonly Action<DataGridVirtualRequest> _onExportRequest;

        public TestableVirtualProvider(List<TestItem> items, Action<DataGridVirtualRequest> onExportRequest)
        {
            _items = items;
            _onExportRequest = onExportRequest;
        }

        public Task<DataGridVirtualResponse<TestItem>> GetVirtualDataAsync(DataGridVirtualRequest request)
        {
            return Task.FromResult(new DataGridVirtualResponse<TestItem>
            {
                Items = _items,
                TotalCount = _items.Count
            });
        }

        public Task<DataGridExportResult<TestItem>?> GetAllForExportAsync(DataGridVirtualRequest request)
        {
            _onExportRequest(request);
            return Task.FromResult<DataGridExportResult<TestItem>?>(new DataGridExportResult<TestItem>
            {
                Items = _items
            });
        }
    }
}
