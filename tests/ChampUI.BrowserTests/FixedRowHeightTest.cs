using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

/// <summary>
/// B-03: Fixed row height and viewport configuration browser tests.
/// Verifies actual DOM measurements match configured virtualization parameters.
/// </summary>
[Collection("PlaywrightCollection")]
public class FixedRowHeightTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";
    private const string TestPageUrl = $"{BaseUrl}/virtual-data-test";

    public FixedRowHeightTest(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _output.WriteLine("=== INITIALIZING PLAYWRIGHT ===");
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Timeout = 30000
        });
        _page = await _browser.NewPageAsync();
        
        _page.Console += (_, msg) => _output.WriteLine($"[BROWSER CONSOLE {msg.Type}] {msg.Text}");
        _page.PageError += (_, error) => _output.WriteLine($"[BROWSER ERROR] {error}");
        
        _output.WriteLine("=== PLAYWRIGHT READY ===");
    }

    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
    }

    [Fact]
    public async Task B03_01_RowHeight_MatchesConfiguration()
    {
        _output.WriteLine("=== TEST START: B-03 Row height verification ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await Task.Delay(2000); // Allow interactive render

        _output.WriteLine("=== Measuring actual row heights ===");
        
        // Get first 5 visible data rows
        var rows = await _page.QuerySelectorAllAsync("tbody tr[data-row]");
        _output.WriteLine($"=== Found {rows.Count} data rows ===");
        
        Assert.True(rows.Count > 0, "Should have at least one rendered row");
        Assert.True(rows.Count <= 200, "Should have bounded row count (not all 1000)");

        // Measure heights of first 5 rows
        for (int i = 0; i < Math.Min(5, rows.Count); i++)
        {
            var height = await rows[i].EvaluateAsync<double>("el => el.getBoundingClientRect().height");
            _output.WriteLine($"=== Row {i} actual height: {height}px ===");
            
            // Default VirtualRowHeight is 48px (from UiDataGrid parameter)
            // Allow small tolerance for borders/rendering
            Assert.InRange(height, 46, 50);
        }

        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "/tmp/b03_row_height_test.png",
            FullPage = true
        });
        _output.WriteLine("=== Screenshot saved to /tmp/b03_row_height_test.png ===");
    }

    [Fact]
    public async Task B03_02_RenderedRowCount_IsBounded()
    {
        _output.WriteLine("=== TEST START: B-03 Bounded row count ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await Task.Delay(2000);

        var rows = await _page.QuerySelectorAllAsync("tbody tr[data-row]");
        var spacers = await _page.QuerySelectorAllAsync("tbody tr[aria-hidden='true']");
        
        _output.WriteLine($"=== Data rows: {rows.Count} ===");
        _output.WriteLine($"=== Spacer rows: {spacers.Count} ===");

        // With VirtualRowHeight=48, VirtualOverscan=5, typical viewport renders ~20-30 rows
        // Dataset is 1000 records, so should NOT render all 1000
        Assert.True(rows.Count < 200, $"Rendered {rows.Count} rows, expected < 200 (dataset is 1000)");
        
        // Should have spacers for virtualization
        Assert.True(spacers.Count >= 2, "Expected at least top and bottom spacers");
    }

    [Fact]
    public async Task B03_03_SpacerDimensions_AreCorrect()
    {
        _output.WriteLine("=== TEST START: B-03 Spacer dimensions ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await Task.Delay(2000);

        var spacers = await _page.QuerySelectorAllAsync("tbody tr[aria-hidden='true']");
        _output.WriteLine($"=== Found {spacers.Count} spacers ===");

        foreach (var spacer in spacers)
        {
            var height = await spacer.EvaluateAsync<double>("el => el.getBoundingClientRect().height");
            _output.WriteLine($"=== Spacer height: {height}px ===");
            
            // Spacers should have non-zero height representing skipped rows
            // For 1000 records with ~30 visible, spacers should total ~900+ rows worth
            if (height > 0)
            {
                Assert.True(height > 100, "Non-zero spacers should represent multiple rows");
            }
        }
    }

    [Fact]
    public async Task B03_04_RowHeight_RemainsStable_OnScroll()
    {
        _output.WriteLine("=== TEST START: B-03 Stable row height on scroll ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await Task.Delay(2000);

        // Measure initial row height
        var rows = await _page.QuerySelectorAllAsync("tbody tr[data-row]");
        var initialHeight = await rows[0].EvaluateAsync<double>("el => el.getBoundingClientRect().height");
        _output.WriteLine($"=== Initial row height: {initialHeight}px ===");

        // Scroll down
        _output.WriteLine("=== Scrolling down ===");
        await _page.EvaluateAsync(@"
            const grid = document.querySelector('.ui-grid__scroll, .ui-grid__scroll--virtualized');
            if (grid) {
                grid.scrollTop = 2000;
            }
        ");

        await Task.Delay(1000);

        // Measure row height after scroll
        var rowsAfterScroll = await _page.QuerySelectorAllAsync("tbody tr[data-row]");
        if (rowsAfterScroll.Count > 0)
        {
            var heightAfterScroll = await rowsAfterScroll[0].EvaluateAsync<double>("el => el.getBoundingClientRect().height");
            _output.WriteLine($"=== Row height after scroll: {heightAfterScroll}px ===");

            Assert.Equal(initialHeight, heightAfterScroll);
        }
    }

    [Fact]
    public async Task B03_05_LongText_DoesNotExpandRowHeight()
    {
        _output.WriteLine("=== TEST START: B-03 Long text truncation ===");
        
        // This test assumes VirtualDataTest generates names like "Test Record 0001"
        // With fixed height + overflow:hidden, long text should be clipped
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await Task.Delay(2000);

        var rows = await _page.QuerySelectorAllAsync("tbody tr[data-row]");
        _output.WriteLine($"=== Checking {rows.Count} rows for stable height ===");

        var heights = new List<double>();
        for (int i = 0; i < Math.Min(10, rows.Count); i++)
        {
            var height = await rows[i].EvaluateAsync<double>("el => el.getBoundingClientRect().height");
            heights.Add(height);
        }

        _output.WriteLine($"=== Row heights: {string.Join(", ", heights)} ===");

        // All rows should have same height (with small tolerance)
        var avgHeight = heights.Average();
        foreach (var h in heights)
        {
            Assert.InRange(h, avgHeight - 2, avgHeight + 2);
        }
    }
}
