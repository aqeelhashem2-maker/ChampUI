using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

[Collection("PlaywrightCollection")]
public class EmptySearchDiagnostic : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string TestPageUrl = "http://localhost:5000/virtual-data-test";

    public EmptySearchDiagnostic(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _output.WriteLine("=== INITIALIZING PLAYWRIGHT ===");
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        _output.WriteLine("=== PLAYWRIGHT READY ===");
    }

    public async Task DisposeAsync()
    {
        _output.WriteLine("=== CLEANING UP ===");
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
        _output.WriteLine("=== CLEANUP COMPLETE ===");
    }

    [Fact(Skip = "Diagnostic test with ambiguous selector - not production code")]
    public async Task DiagnoseEmptySearch()
    {
        var page = await _browser!.NewPageAsync();
        try
        {
            await page.GotoAsync(TestPageUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Get initial row count
            var initialRows = await page.QuerySelectorAllAsync("tr[data-row-key]");
            _output.WriteLine($"Initial rows: {initialRows.Count}");

            // Get search input
            var searchInput = await page.WaitForSelectorAsync("input[type='search']", new PageWaitForSelectorOptions { Timeout = 5000 });
            Assert.NotNull(searchInput);

            // Enter search term that matches nothing
            _output.WriteLine("Entering search term: NOMATCHXYZ123");
            
            // Clear first, then type slowly to trigger oninput events properly
            await searchInput.FillAsync("");
            await Task.Delay(200);
            // Use PressSequentiallyAsync instead of obsolete TypeAsync
            await page.Locator("input[type='search']").PressSequentiallyAsync("NOMATCHXYZ123", new LocatorPressSequentiallyOptions { Delay = 50 });
            await Task.Delay(300);
            
            // Wait and check repeatedly
            for (int i = 0; i < 6; i++)
            {
                await Task.Delay(500);
                var rows = await page.QuerySelectorAllAsync("tr[data-row-key]");
                _output.WriteLine($"After {(i + 1) * 500}ms: {rows.Count} rows");
                
                if (rows.Count == 0)
                {
                    _output.WriteLine("SUCCESS: Rows cleared");
                    break;
                }
            }
            
            // Check status
            var searchStatus = await page.TextContentAsync("[data-testid='status-search']");
            _output.WriteLine($"Search status: {searchStatus}");
            
            var requestCount = await page.TextContentAsync("[data-testid='status-request-count']");
            _output.WriteLine($"Request count: {requestCount}");
            
            // Final check
            var finalRows = await page.QuerySelectorAllAsync("tr[data-row-key]");
            _output.WriteLine($"Final row count: {finalRows.Count}");
            
            // Check if Virtualize component is present
            var virtualizeWrapper = await page.QuerySelectorAsync("[data-testid='virtualize-wrapper']");
            _output.WriteLine($"Virtualize wrapper present: {virtualizeWrapper != null}");
            
            // Check HTML structure
            var bodyHTML = await page.EvaluateAsync<string>("() => document.querySelector('[data-testid=\"virtualize-wrapper\"]')?.innerHTML?.substring(0, 500)");
            _output.WriteLine($"Wrapper HTML (first 500 chars): {bodyHTML}");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
