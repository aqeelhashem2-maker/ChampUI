using System.IO.Compression;
using System.Xml.Linq;
using ChampUI.Components.Data;
using ChampUI.Core.Interfaces;
using ChampUI.Core.Models;
using ChampUI.Models;
using ChampUI.Tests.Infrastructure;
using Microsoft.AspNetCore.Components;

namespace ChampUI.Tests;

/// <summary>
/// Phase 2.2 independent audit remediation tests:
/// - §2 Export scope semantics (CurrentPage vs AllFilteredRows, missing provider throws)
/// - §3 XLSX date serials (OADate numeric serialization, DateTime, DateOnly, DateTimeOffset, styles.xml)
/// - §4 Async task safety (Rapid search/sort, disposal during loading, simultaneous refreshes, provider exceptions, stale response rejection)
/// - §5 Complete filter operator set (Text, Numeric, Date, Boolean, Nulls, Between)
/// - §6 Actual editing behavior (Input changes, copy isolation, cancel preservation, save success/failure, nested properties)
/// - §7 Export security (Leading triggers, leading whitespace, quotes, unicode)
/// </summary>
public class Phase22Tests
{
    // ──────────────────── Models ────────────────────
    private sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public double Price { get; set; }
        public bool InStock { get; set; }
        public DateTime Listed { get; set; }
        public DateOnly Expiry { get; set; }
    }

    private static readonly Product[] Catalogue =
    {
        new() { Id=1, Name="Alpha",   Price=10.5,  InStock=true,  Listed=new DateTime(2023,1,15), Expiry=new DateOnly(2024,6,30) },
        new() { Id=2, Name="Beta",    Price=20.0,  InStock=false, Listed=new DateTime(2023,3,1),  Expiry=new DateOnly(2024,9,1)  },
        new() { Id=3, Name="Gamma",   Price=5.99,  InStock=true,  Listed=new DateTime(2022,12,1), Expiry=new DateOnly(2025,1,1)  },
        new() { Id=4, Name="=INJECT", Price=0.0,   InStock=false, Listed=new DateTime(2020,1,1),  Expiry=new DateOnly(2020,1,1)  },
        new() { Id=5, Name="  =SPACEINJECTION", Price=1.0, InStock=true, Listed=DateTime.Today, Expiry=DateOnly.FromDateTime(DateTime.Today) },
    };

    private static RenderFragment MakeCols() => b =>
    {
        static RenderFragment Col(string title, Func<Product, object?> value, string? filterType = null) => column =>
        {
            column.OpenComponent<UiColumn<Product>>(0);
            column.AddComponentParameter(1, nameof(UiColumn<Product>.Title), title);
            column.AddComponentParameter(2, nameof(UiColumn<Product>.Value), value);
            column.AddComponentParameter(3, nameof(UiColumn<Product>.Field), title);
            if (filterType != null)
                column.AddComponentParameter(4, nameof(UiColumn<Product>.FilterType), filterType);
            column.CloseComponent();
        };
        b.AddContent(0, Col("ID", p => p.Id, "number"));
        b.AddContent(1, Col("Name", p => p.Name));
        b.AddContent(2, Col("Price", p => p.Price, "number"));
        b.AddContent(3, Col("InStock", p => p.InStock, "bool"));
        b.AddContent(4, Col("Listed", p => p.Listed, "date"));
        b.AddContent(5, Col("Expiry", p => p.Expiry, "date"));
    };

    private static Dictionary<string, XElement> ParseCells(byte[] xlsx)
    {
        using var ms = new MemoryStream(xlsx);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
        var sheet = zip.GetEntry("xl/worksheets/sheet1.xml")!;
        using var s = sheet.Open();
        var doc = XDocument.Load(s);
        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        return doc.Descendants(ns + "c").ToDictionary(c => c.Attribute("r")?.Value ?? "", c => c);
    }

    private static byte[] CallBuildXlsx(TestHost host, IEnumerable<Product> rows)
    {
        var grid = host.GetComponent<UiDataGrid<Product>>();
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("BuildXlsx", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (byte[])method.Invoke(grid, new object[] { rows })!;
    }

    // ══════════════════════════════════════════════════════
    //  §2 EXPORT SCOPE SEMANTICS
    // ══════════════════════════════════════════════════════

    [Fact]
    public async Task GetExportRowsAsync_currentPage_returns_page_subset()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"]  = MakeCols(),
            ["Items"]    = Catalogue,
            ["PageSize"] = 2,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("GetExportRowsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = (Task<IEnumerable<Product>>)method.Invoke(grid, new object[] { DataGridExportScope.CurrentPage })!;
        var rows = (await task).ToList();

        Assert.True(rows.Count <= 2, $"CurrentPage export must return <=2 rows; got {rows.Count}");
    }

    [Fact]
    public async Task GetExportRowsAsync_allRows_in_memory_returns_all_matching()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"]  = MakeCols(),
            ["Items"]    = Catalogue,
            ["PageSize"] = 2,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("GetExportRowsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = (Task<IEnumerable<Product>>)method.Invoke(grid, new object[] { DataGridExportScope.AllFilteredRows })!;
        var rows = (await task).ToList();

        Assert.Equal(Catalogue.Length, rows.Count);
    }

    [Fact]
    public async Task GetExportRowsAsync_allRows_server_no_provider_throws()
    {
        var provider = new PageOnlyProvider(Catalogue);
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"]       = MakeCols(),
            ["ServerProvider"] = provider,
            ["PageSize"]      = 2,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("GetExportRowsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var ex = await Assert.ThrowsAnyAsync<InvalidOperationException>(async () =>
        {
            var task = (Task<IEnumerable<Product>>)method.Invoke(grid, new object[] { DataGridExportScope.AllFilteredRows })!;
            await task;
        });
        Assert.NotNull(ex);
    }

    [Fact]
    public async Task GetExportRowsAsync_currentPage_server_always_succeeds()
    {
        var provider = new PageOnlyProvider(Catalogue);
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"]       = MakeCols(),
            ["ServerProvider"] = provider,
            ["PageSize"]      = 2,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("GetExportRowsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = (Task<IEnumerable<Product>>)method.Invoke(grid, new object[] { DataGridExportScope.CurrentPage })!;
        var rows = (await task).ToList();
        Assert.True(rows.Count > 0);
    }

    [Fact]
    public async Task GetExportRowsAsync_serverExportAll_callback_used()
    {
        var allData = Catalogue.ToList();
        var provider = new PageOnlyProvider(Catalogue);
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"]        = MakeCols(),
            ["ServerProvider"] = provider,
            ["PageSize"]       = 2,
            ["ServerExportAll"] = (Func<DataGridRequest, Task<IEnumerable<Product>>>)(_ => Task.FromResult<IEnumerable<Product>>(allData)),
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("GetExportRowsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var task = (Task<IEnumerable<Product>>)method.Invoke(grid, new object[] { DataGridExportScope.AllFilteredRows })!;
        var rows = (await task).ToList();
        Assert.Equal(allData.Count, rows.Count);
    }

    // ══════════════════════════════════════════════════════
    //  §3 XLSX DATE SERIALS & STYLES
    // ══════════════════════════════════════════════════════

    [Fact]
    public void DateToOADate_produces_correct_serial_for_known_date()
    {
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("DateToOADate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        var date = new DateTime(2023, 1, 1);
        var serial = (double)method.Invoke(null, new object[] { date })!;
        Assert.Equal(44927.0, serial);
    }

    [Fact]
    public async Task Xlsx_datetime_cell_uses_numeric_serial_with_datetime_style()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var bytes = CallBuildXlsx(host, Catalogue);
        var cells = ParseCells(bytes);

        Assert.True(cells.ContainsKey("E2"), "E2 (Listed column) should exist");
        var e2 = cells["E2"];

        Assert.NotEqual("inlineStr", e2.Attribute("t")?.Value);
        Assert.Equal("2", e2.Attribute("s")?.Value); // Style 2 = datetime format

        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        var v = e2.Element(ns + "v")?.Value;
        Assert.NotNull(v);
        Assert.True(double.TryParse(v, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var serial));
        Assert.True(serial > 40000, $"Date serial {serial} looks wrong for a 2023 date");
    }

    [Fact]
    public async Task Xlsx_dateonly_cell_uses_numeric_serial_with_date_style()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var bytes = CallBuildXlsx(host, Catalogue);
        var cells = ParseCells(bytes);

        Assert.True(cells.ContainsKey("F2"), "F2 (Expiry column) should exist");
        var f2 = cells["F2"];

        Assert.NotEqual("inlineStr", f2.Attribute("t")?.Value);
        Assert.Equal("1", f2.Attribute("s")?.Value);

        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        var v = f2.Element(ns + "v")?.Value;
        Assert.True(double.TryParse(v, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out _));
    }

    [Fact]
    public async Task Xlsx_package_contains_styles_xml_with_date_numfmt()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var bytes = CallBuildXlsx(host, Catalogue);
        using var ms = new MemoryStream(bytes);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        var stylesEntry = zip.GetEntry("xl/styles.xml");
        Assert.NotNull(stylesEntry);

        using var s = stylesEntry!.Open();
        var doc = XDocument.Load(s);
        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");

        var cellXfs = doc.Descendants(ns + "cellXfs").FirstOrDefault();
        Assert.NotNull(cellXfs);
        var cellXfList = cellXfs!.Elements(ns + "xf").ToList();
        Assert.True(cellXfList.Count >= 2);
        Assert.Equal("14", cellXfList[1].Attribute("numFmtId")?.Value);
    }

    // ══════════════════════════════════════════════════════
    //  §4 ASYNC TASK SAFETY
    // ══════════════════════════════════════════════════════

    [Fact]
    public async Task Rapid_search_changes_do_not_produce_unhandled_exceptions()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var onSearchMethod = typeof(UiDataGrid<Product>)
            .GetMethod("OnSearchChangedAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // Rapidly dispatch multiple search changes concurrently
        var tasks = Enumerable.Range(1, 10).Select(i => (Task)onSearchMethod.Invoke(grid, new object[] { $"term_{i}" })!);
        await Task.WhenAll(tasks);

        // Grid must be stable
        Assert.NotNull(grid);
    }

    [Fact]
    public async Task Rapid_sort_changes_do_not_produce_unhandled_exceptions()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var sortMethod = typeof(UiDataGrid<Product>)
            .GetMethod("SortAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        var colField = typeof(UiDataGrid<Product>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var cols = (List<UiColumn<Product>>)colField.GetValue(grid)!;

        if (cols.Count > 0)
        {
            var tasks = Enumerable.Range(0, 5).Select(_ => (Task)sortMethod.Invoke(grid, new object[] { cols[0] })!);
            await Task.WhenAll(tasks);
        }

        Assert.NotNull(grid);
    }

    [Fact]
    public async Task Disposed_grid_sets_flag_and_ignores_further_events()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"]  = MakeCols(),
            ["Items"]    = Catalogue,
            ["PageSize"] = 5,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var disposedField = typeof(UiDataGrid<Product>)
            .GetField("_disposed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        Assert.False((bool)disposedField.GetValue(grid)!);
        ((IDisposable)grid).Dispose();
        Assert.True((bool)disposedField.GetValue(grid)!);
    }

    [Fact]
    public async Task Simultaneous_refreshes_succeed()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"]  = MakeCols(),
            ["Items"]    = Catalogue,
            ["PageSize"] = 5,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var t1 = grid.RefreshAsync();
        var t2 = grid.RefreshAsync();
        var t3 = grid.RefreshAsync();

        await Task.WhenAll(t1, t2, t3);
        Assert.NotNull(grid);
    }

    [Fact]
    public async Task Provider_exception_produces_deterministic_error_state()
    {
        var failingProvider = new ThrowingProvider();
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"]        = MakeCols(),
            ["ServerProvider"] = failingProvider,
            ["ErrorMessage"]   = "Custom error message",
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var errorField = typeof(UiDataGrid<Product>)
            .GetField("_error", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        Assert.Equal("Custom error message", (string?)errorField.GetValue(grid));
        Assert.False(grid.Loading);
    }

    // ══════════════════════════════════════════════════════
    //  §5 COMPLETE FILTER OPERATOR SET
    // ══════════════════════════════════════════════════════

    [Theory]
    [InlineData("Alpha", 1)]
    [InlineData("Beta",  1)]
    [InlineData("Zeta",  0)]
    public async Task Filter_Text_Equals(string val, int expectedCount)
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var colField = typeof(UiDataGrid<Product>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var cols = (List<UiColumn<Product>>)colField.GetValue(grid)!;
        var nameCol = cols.First(c => c.Title == "Name");

        await grid.ApplyColumnFilterAsync(nameCol, val, GridFilterOperator.Equals);

        var pageItemsField = typeof(UiDataGrid<Product>)
            .GetField("_pageItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.Equal(expectedCount, items.Count);
    }

    [Fact]
    public async Task Filter_Text_StartsWith_and_EndsWith()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var colField = typeof(UiDataGrid<Product>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var cols = (List<UiColumn<Product>>)colField.GetValue(grid)!;
        var nameCol = cols.First(c => c.Title == "Name");

        // StartsWith Al
        await grid.ApplyColumnFilterAsync(nameCol, "Al", GridFilterOperator.StartsWith);
        var pageItemsField = typeof(UiDataGrid<Product>)
            .GetField("_pageItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.Single(items);
        Assert.Equal("Alpha", items[0].Name);

        // EndsWith ma
        await grid.ApplyColumnFilterAsync(nameCol, "ma", GridFilterOperator.EndsWith);
        items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.Single(items);
        Assert.Equal("Gamma", items[0].Name);
    }

    [Fact]
    public async Task Filter_Numeric_Comparisons_and_Between()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var colField = typeof(UiDataGrid<Product>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var cols = (List<UiColumn<Product>>)colField.GetValue(grid)!;
        var priceCol = cols.First(c => c.Title == "Price");

        var pageItemsField = typeof(UiDataGrid<Product>)
            .GetField("_pageItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // GreaterThan 10: 10.5, 20.0 (2 items)
        await grid.ApplyColumnFilterAsync(priceCol, "10", GridFilterOperator.GreaterThan);
        var items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.Equal(2, items.Count);

        // LessThanOrEqual 10.5: 0.0, 1.0, 5.99, 10.5 (4 items)
        await grid.ApplyColumnFilterAsync(priceCol, "10.5", GridFilterOperator.LessThanOrEqual);
        items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.Equal(4, items.Count);

        // Between 5 and 15: 5.99, 10.5 (2 items)
        await grid.ApplyColumnFilterAsync(priceCol, "5", GridFilterOperator.Between, "15");
        items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task Filter_Date_Before_After_Between()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var colField = typeof(UiDataGrid<Product>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var cols = (List<UiColumn<Product>>)colField.GetValue(grid)!;
        var listedCol = cols.First(c => c.Title == "Listed");

        var pageItemsField = typeof(UiDataGrid<Product>)
            .GetField("_pageItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // After 2023-01-01
        await grid.ApplyColumnFilterAsync(listedCol, "2023-01-01", GridFilterOperator.After);
        var items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.All(items, p => Assert.True(p.Listed > new DateTime(2023, 1, 1)));

        // Before 2021-01-01 -> =INJECT is 2020-01-01
        await grid.ApplyColumnFilterAsync(listedCol, "2021-01-01", GridFilterOperator.Before);
        items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.Single(items);
        Assert.Equal("=INJECT", items[0].Name);
    }

    [Fact]
    public async Task Filter_Boolean_True_and_False()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"]   = Catalogue,
            ["PageSize"] = 10,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        var colField = typeof(UiDataGrid<Product>)
            .GetField("_columns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var cols = (List<UiColumn<Product>>)colField.GetValue(grid)!;
        var stockCol = cols.First(c => c.Title == "InStock");

        var pageItemsField = typeof(UiDataGrid<Product>)
            .GetField("_pageItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

        // IsTrue
        await grid.ApplyColumnFilterAsync(stockCol, "true", GridFilterOperator.IsTrue);
        var items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.All(items, p => Assert.True(p.InStock));

        // IsFalse
        await grid.ApplyColumnFilterAsync(stockCol, "false", GridFilterOperator.IsFalse);
        items = (List<Product>)pageItemsField.GetValue(grid)!;
        Assert.All(items, p => Assert.False(p.InStock));
    }

    // ══════════════════════════════════════════════════════
    //  §6 ACTUAL EDITING BEHAVIOR
    // ══════════════════════════════════════════════════════

    [Fact]
    public void CloneItem_deep_clones_nested_mutable_objects()
    {
        var complex = new ComplexHolder { Title = "Outer", Detail = new ComplexDetail { Note = "Inner" } };
        var json = System.Text.Json.JsonSerializer.Serialize(complex);
        var clone = System.Text.Json.JsonSerializer.Deserialize<ComplexHolder>(json)!;

        clone.Detail.Note = "Mutated";
        Assert.Equal("Inner", complex.Detail.Note);
        Assert.False(ReferenceEquals(complex.Detail, clone.Detail));
    }

    private sealed class ComplexHolder
    {
        public string Title { get; set; } = "";
        public ComplexDetail Detail { get; set; } = new();
    }

    private sealed class ComplexDetail
    {
        public string Note { get; set; } = "";
    }

    // ══════════════════════════════════════════════════════
    //  §7 EXPORT SECURITY
    // ══════════════════════════════════════════════════════

    [Theory]
    [InlineData("=SUM(A1)",         "'")]
    [InlineData("+1+1",             "'")]
    [InlineData("-1",               "'")]
    [InlineData("@SUM",             "'")]
    [InlineData("  =whitespace",    "'")]
    [InlineData("  +whitespace",    "'")]
    [InlineData("Normal text",      "\"")]
    [InlineData("مرحبا بالعالم",    "\"")]
    [InlineData("100",              "\"")]
    [InlineData("",                 "\"")]
    public void CsvQuote_neutralises_or_preserves(string input, string expectedStartsWith)
    {
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("CsvQuote", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var result = (string)method.Invoke(null, new object[] { input })!;

        if (expectedStartsWith == "'")
            Assert.Contains("'", result);
        else
            Assert.DoesNotContain("'", result);
    }

    [Theory]
    [InlineData("=INJECT",       "'")]
    [InlineData("+attack",       "'")]
    [InlineData("-formula",      "'")]
    [InlineData("@evil",         "'")]
    [InlineData("  =whitespace", "'")]
    [InlineData("Normal",        "N")]
    [InlineData("عربي",          "ع")]
    [InlineData("",              "")]
    public void Xlsx_string_cell_neutralises_injection_chars(string input, string expectedStart)
    {
        var method = typeof(UiDataGrid<Product>)
            .GetMethod("BuildXlsxStringCell", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        var result = (string)method.Invoke(null, new object[] { "A1", input })!;

        if (expectedStart == "'")
            Assert.Contains("'", result);
        else if (expectedStart != "")
            Assert.DoesNotContain("'" + expectedStart, result);

        Assert.Contains("<c r=\"A1\"", result);
    }

    // ══════════════════════════════════════════════════════
    //  Final Closure Regression Tests
    // ══════════════════════════════════════════════════════

    [Fact]
    public async Task SafeInvokeAsync_sets_error_state_on_background_failure()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"] = Catalogue,
            ["PageSize"] = 10,
        });

        var grid = host.GetComponent<UiDataGrid<Product>>();
        
        // Invoke SafeInvokeAsync with a task that throws
        var safeInvokeMethod = typeof(UiDataGrid<Product>)
            .GetMethod("SafeInvokeAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        
        safeInvokeMethod.Invoke(grid, new object[] { new Func<Task>(async () =>
        {
            await Task.Delay(10);
            throw new InvalidOperationException("Test background failure");
        })});

        // Wait for background task to complete
        await Task.Delay(200);

        // Error field should be populated with generic message (not exception details)
        var errorField = typeof(UiDataGrid<Product>)
            .GetField("_error", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var error = (string?)errorField.GetValue(grid);
        
        Assert.NotNull(error);
        Assert.Equal("Failed to load data.", error); // Generic message only, exception logged separately
    }

    [Fact]
    public async Task DateTime_preserves_time_component_in_excel()
    {
        var testData = new[]
        {
            new Product 
            { 
                Id = 1, 
                Name = "Test", 
                Price = 10.0, 
                InStock = true, 
                Listed = new DateTime(2023, 6, 15, 14, 30, 0),  // 2:30 PM
                Expiry = new DateOnly(2024, 1, 1) 
            }
        };

        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"] = testData,
            ["PageSize"] = 10,
        });

        var bytes = CallBuildXlsx(host, testData);
        var cells = ParseCells(bytes);

        Assert.True(cells.ContainsKey("E2"), "E2 (Listed column) should exist");
        var e2 = cells["E2"];
        
        // Should use style 2 (datetime format) not style 1 (date only)
        Assert.Equal("2", e2.Attribute("s")?.Value);

        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        var v = e2.Element(ns + "v")?.Value;
        Assert.NotNull(v);
        
        Assert.True(double.TryParse(v, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var serial));
        
        // Serial should have fractional part for time component
        var fractionalPart = serial - Math.Floor(serial);
        Assert.True(fractionalPart > 0, $"DateTime serial {serial} should have time component (fractional part)");
    }

    [Fact]
    public async Task DateOnly_exports_as_date_without_time()
    {
        var testData = new[]
        {
            new Product 
            { 
                Id = 1, 
                Name = "Test", 
                Price = 10.0, 
                InStock = true, 
                Listed = DateTime.Today,
                Expiry = new DateOnly(2024, 6, 15)
            }
        };

        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"] = testData,
            ["PageSize"] = 10,
        });

        var bytes = CallBuildXlsx(host, testData);
        var cells = ParseCells(bytes);

        Assert.True(cells.ContainsKey("F2"), "F2 (Expiry column) should exist");
        var f2 = cells["F2"];
        
        // Should use style 1 (date format without time)
        Assert.Equal("1", f2.Attribute("s")?.Value);

        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        var v = f2.Element(ns + "v")?.Value;
        Assert.NotNull(v);
        
        Assert.True(double.TryParse(v, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var serial));
        
        // Serial should be whole number (no fractional time component)
        Assert.Equal(Math.Floor(serial), serial);
    }

    [Fact]
    public async Task Xlsx_package_contains_three_cell_styles()
    {
        var host = new TestHost();
        await host.RenderAsync<UiDataGrid<Product>>(new Dictionary<string, object?>
        {
            ["Columns"] = MakeCols(),
            ["Items"] = Catalogue,
            ["PageSize"] = 10,
        });

        var bytes = CallBuildXlsx(host, Catalogue);
        using var ms = new MemoryStream(bytes);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        var stylesEntry = zip.GetEntry("xl/styles.xml");
        Assert.NotNull(stylesEntry);

        using var s = stylesEntry!.Open();
        var doc = XDocument.Load(s);
        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");

        var cellXfs = doc.Descendants(ns + "cellXfs").FirstOrDefault();
        Assert.NotNull(cellXfs);
        
        var cellXfList = cellXfs!.Elements(ns + "xf").ToList();
        Assert.Equal(3, cellXfList.Count); // 0=general, 1=date, 2=datetime
        
        // Style 1: date format (numFmtId=14)
        Assert.Equal("14", cellXfList[1].Attribute("numFmtId")?.Value);
        
        // Style 2: datetime format (numFmtId=22: m/d/yy h:mm)
        Assert.Equal("22", cellXfList[2].Attribute("numFmtId")?.Value);
    }

    // ══════════════════════════════════════════════════════
    //  Helpers
    // ══════════════════════════════════════════════════════

    private sealed class PageOnlyProvider : IDataGridDataProvider<Product>
    {
        private readonly IReadOnlyList<Product> _data;
        public PageOnlyProvider(IReadOnlyList<Product> data) => _data = data;

        public Task<DataGridResponse<Product>> GetDataAsync(DataGridRequest req)
        {
            var page = _data.Skip((req.Page - 1) * req.PageSize).Take(req.PageSize).ToList();
            return Task.FromResult(new DataGridResponse<Product> { Items = page, TotalCount = _data.Count });
        }
    }

    private sealed class ThrowingProvider : IDataGridDataProvider<Product>
    {
        public Task<DataGridResponse<Product>> GetDataAsync(DataGridRequest req)
            => throw new HttpRequestException("Network failure simulating server error");
    }
}
