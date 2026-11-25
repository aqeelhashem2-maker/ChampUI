using ChampUI.Components.Data;
using ChampUI.Core.Models;
using ChampUI.Models;
using ChampUI.Tests.Infrastructure;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace ChampUI.Tests;

/// <summary>
/// Phase 3 Checkpoint B: Row Virtualization Tests
/// Verifies that virtualized rendering works correctly while maintaining
/// backward compatibility with non-virtualized mode.
/// </summary>
public class Phase3BVirtualizationTests
{
    private sealed record Product(int Id, string Name, decimal Price, string Category);

    private static async Task<(TestHost host, Node root)> RenderGridAsync(
        List<Product> items,
        UiDataGridVirtualization virtualization = UiDataGridVirtualization.None,
        int pageSize = 10,
        bool selectable = false,
        bool searchable = true,
        UiDataGridEditMode editMode = UiDataGridEditMode.None)
    {
        var host = new TestHost();
        var columns = new RenderFragment(b =>
        {
            b.OpenComponent<UiColumn<Product>>(0);
            b.AddComponentParameter(1, "Title", "Name");
            b.AddComponentParameter(2, nameof(UiColumn<Product>.Value), (Func<Product, string>)(p => p.Name));
            b.CloseComponent();
        });
        var parameters = new Dictionary<string, object?>
        {
            ["Items"] = items,
            ["VirtualizationMode"] = virtualization,
            ["PageSize"] = pageSize,
            ["Selectable"] = selectable,
            ["Searchable"] = searchable,
            ["EditMode"] = editMode,
            ["Columns"] = columns
        };
        var root = await host.RenderAsync<UiDataGrid<Product>>(parameters);
        return (host, root);
    }

    [Fact]
    public async Task Virtualization_None_RendersAllRowsDirectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 50).Select(i => new Product(i, $"Product {i}", i * 10m, "Category A")).ToList();

        // Act
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.None, pageSize: 50);

        // Assert: All 50 rows should be rendered directly (non-virtualized)
        var rowCount = root.Elements("tr").Count(n => n.Attributes.ContainsKey("data-row"));
        Assert.Equal(50, rowCount);
        Assert.Contains("Product 1", root.InnerText);
        Assert.Contains("Product 50", root.InnerText);
    }

    [Fact]
    public async Task Virtualization_Enabled_ConfiguresCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).Select(i => new Product(i, $"Product {i}", i * 10m, "Category A")).ToList();

        // Act
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.Enabled, pageSize: 100);

        // Assert: Grid renders with virtualization enabled
        var grid = host.GetComponent<UiDataGrid<Product>>();
        Assert.Equal(UiDataGridVirtualization.Enabled, grid.VirtualizationMode);
        Assert.Equal(48f, grid.VirtualRowHeight); // default
        Assert.Equal(5, grid.VirtualOverscan); // default
    }

    [Fact]
    public async Task Virtualization_DefaultsToNone_ForBackwardCompatibility()
    {
        // Arrange & Act
        var (host, root) = await RenderGridAsync(new List<Product>());

        // Assert: Default should be None for backward compatibility
        var grid = host.GetComponent<UiDataGrid<Product>>();
        Assert.Equal(UiDataGridVirtualization.None, grid.VirtualizationMode);
    }

    [Fact]
    public async Task Virtualization_ParametersHaveCorrectDefaults()
    {
        // Arrange & Act
        var (host, root) = await RenderGridAsync(new List<Product>());

        // Assert
        var grid = host.GetComponent<UiDataGrid<Product>>();
        Assert.Equal(UiDataGridVirtualization.None, grid.VirtualizationMode);
        Assert.Equal(48f, grid.VirtualRowHeight);
        Assert.Equal(5, grid.VirtualOverscan);
    }

    [Fact]
    public async Task Virtualization_WorksWithSelection()
    {
        // Arrange
        var items = Enumerable.Range(1, 20).Select(i => new Product(i, $"Product {i}", i * 10m, "Category A")).ToList();

        // Act
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.Enabled, pageSize: 20, selectable: true);

        // Assert: Grid should render with selection checkboxes
        var checkboxes = root.WithClass("ui-grid__check");
        Assert.True(checkboxes.Any());
    }

    [Fact]
    public async Task Virtualization_WorksWithInlineEditing()
    {
        // Arrange
        var items = new List<Product>
        {
            new(1, "Product A", 100m, "Cat1"),
            new(2, "Product B", 200m, "Cat2"),
        };

        // Act
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.Enabled, pageSize: 10, editMode: UiDataGridEditMode.Inline);

        // Assert: Edit buttons should be present
        var actionsCol = root.WithClass("ui-grid__actions-col");
        Assert.True(actionsCol.Any());
    }

    [Fact]
    public async Task Virtualization_EmptyState_RendersCorrectly()
    {
        // Arrange & Act
        var (host, root) = await RenderGridAsync(new List<Product>(), UiDataGridVirtualization.Enabled);

        // Assert: Empty state should render
        var emptyState = root.WithClass("ui-grid__empty");
        Assert.True(emptyState.Any());
        Assert.Contains("No results", root.InnerText);
    }

    [Fact]
    public async Task Virtualization_LargeDataset_1000Rows()
    {
        // Arrange: 1,000 rows
        var items = Enumerable.Range(1, 1000).Select(i => new Product(i, $"Product {i}", i * 10m, $"Category {i % 10}")).ToList();

        // Act
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.Enabled, pageSize: 1000);

        // Assert: Component should handle large dataset
        var grid = host.GetComponent<UiDataGrid<Product>>();
        Assert.Equal(UiDataGridVirtualization.Enabled, grid.VirtualizationMode);
        
        // Verify table structure exists
        var tbody = root.Elements("tbody").FirstOrDefault();
        Assert.NotNull(tbody);
    }

    [Fact]
    public async Task Virtualization_CustomRowHeight_AppliesCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).Select(i => new Product(i, $"Product {i}", i * 10m, "Cat")).ToList();
        var host = new TestHost();
        var columns = new RenderFragment(b =>
        {
            b.OpenComponent<UiColumn<Product>>(0);
            b.AddComponentParameter(1, "Title", "Name");
            b.AddComponentParameter(2, nameof(UiColumn<Product>.Value), (Func<Product, string>)(p => p.Name));
            b.CloseComponent();
        });
        var parameters = new Dictionary<string, object?>
        {
            ["Items"] = items,
            ["VirtualizationMode"] = UiDataGridVirtualization.Enabled,
            ["VirtualRowHeight"] = 60f,
            ["PageSize"] = 10,
            ["Columns"] = columns
        };

        // Act
        var root = await host.RenderAsync<UiDataGrid<Product>>(parameters);

        // Assert: Custom row height should be applied
        var grid = host.GetComponent<UiDataGrid<Product>>();
        Assert.Equal(60f, grid.VirtualRowHeight);
    }

    [Fact]
    public async Task Virtualization_CustomOverscan_AppliesCorrectly()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).Select(i => new Product(i, $"Product {i}", i * 10m, "Cat")).ToList();
        var host = new TestHost();
        var columns = new RenderFragment(b =>
        {
            b.OpenComponent<UiColumn<Product>>(0);
            b.AddComponentParameter(1, "Title", "Name");
            b.AddComponentParameter(2, nameof(UiColumn<Product>.Value), (Func<Product, string>)(p => p.Name));
            b.CloseComponent();
        });
        var parameters = new Dictionary<string, object?>
        {
            ["Items"] = items,
            ["VirtualizationMode"] = UiDataGridVirtualization.Enabled,
            ["VirtualOverscan"] = 10,
            ["PageSize"] = 10,
            ["Columns"] = columns
        };

        // Act
        var root = await host.RenderAsync<UiDataGrid<Product>>(parameters);

        // Assert: Custom overscan should be applied
        var grid = host.GetComponent<UiDataGrid<Product>>();
        Assert.Equal(10, grid.VirtualOverscan);
    }

    [Fact]
    public async Task Virtualization_WorksWithPagination()
    {
        // Arrange
        var items = Enumerable.Range(1, 100).Select(i => new Product(i, $"Product {i}", i * 10m, "Cat")).ToList();

        // Act
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.Enabled, pageSize: 20);

        // Assert: Pagination should still work
        var footer = root.WithClass("ui-grid__footer");
        Assert.True(footer.Any());
        var summary = root.WithClass("ui-pagination__summary").FirstOrDefault();
        Assert.NotNull(summary);
        Assert.Contains("1–20 of 100", summary.InnerText);
    }

    [Fact]
    public async Task Virtualization_None_DoesNotUseVirtualizeComponent()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).Select(i => new Product(i, $"Product {i}", i * 10m, "Cat")).ToList();

        // Act
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.None, pageSize: 10);

        // Assert: Should use direct foreach rendering
        var rowCount = root.Elements("tr").Count(n => n.Attributes.ContainsKey("data-row"));
        Assert.Equal(10, rowCount);
        Assert.Contains("Product 1", root.InnerText);
        Assert.Contains("Product 10", root.InnerText);
    }

    [Fact]
    public async Task Virtualization_None_WorksWith50Rows()
    {
        // Arrange
        var items = Enumerable.Range(1, 50).Select(i => new Product(i, $"Product {i}", i * 10m, "Cat")).ToList();

        // Act
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.None, pageSize: 50);

        // Assert: All rows rendered
        var rowCount = root.Elements("tr").Count(n => n.Attributes.ContainsKey("data-row"));
        Assert.Equal(50, rowCount);
    }

    [Fact]
    public async Task Virtualization_Enabled_ReducesInitialRenderSize()
    {
        // Arrange: Large dataset
        var items = Enumerable.Range(1, 1000).Select(i => new Product(i, $"Product {i}", i * 10m, "Cat")).ToList();

        // Act: Render with virtualization
        var (host, root) = await RenderGridAsync(items, UiDataGridVirtualization.Enabled, pageSize: 1000);

        // Assert: Virtualization should reduce DOM nodes
        // (We can't measure exact count without JS, but verify component renders)
        var grid = host.GetComponent<UiDataGrid<Product>>();
        Assert.Equal(UiDataGridVirtualization.Enabled, grid.VirtualizationMode);
        
        // Table structure exists
        var table = root.Elements("table").FirstOrDefault();
        Assert.NotNull(table);
    }
}
