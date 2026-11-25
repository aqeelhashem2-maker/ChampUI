using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

/// <summary>
/// Diagnostic test to capture actual runtime state for B12_01 failure investigation.
/// </summary>
[Collection("PlaywrightCollection")]
public class DiagnosticSelectionTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";

    public DiagnosticSelectionTest(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Timeout = 30000
        });
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();

        // Capture console messages
        _page.Console += (_, msg) =>
        {
            _output.WriteLine($"[BROWSER CONSOLE {msg.Type}] {msg.Text}");
        };

        // Capture page errors
        _page.PageError += (_, error) =>
        {
            _output.WriteLine($"[BROWSER ERROR] {error}");
        };
    }

    public async Task DisposeAsync()
    {
        if (_page != null) await _page.CloseAsync();
        if (_context != null) await _context.CloseAsync();
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
    }

    [Fact]
    public async Task Diagnostic_CaptureGridState()
    {
        _output.WriteLine("=== DIAGNOSTIC TEST START ===");

        // Navigate to page
        _output.WriteLine("=== Navigating to /virtual-data-test ===");
        var response = await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load // Don't wait for NetworkIdle yet
        });

        _output.WriteLine($"=== HTTP Status: {response?.Status} ===");
        _output.WriteLine($"=== Final URL: {_page.Url} ===");

        // Wait for Blazor to initialize
        _output.WriteLine("=== Waiting for Blazor initialization ===");
        await Task.Delay(1000);

        // Check for Blazor circuit connection
        var blazorStatus = await _page.EvaluateAsync<string>(@"
            () => {
                const circuit = window['Blazor'];
                if (!circuit) return 'Blazor object not found';
                if (circuit._internal?.dotNetRuntime) return 'Circuit connected';
                return 'Circuit object exists but not connected';
            }
        ");
        _output.WriteLine($"=== Blazor Status: {blazorStatus} ===");

        // Wait longer for interactive rendering
        _output.WriteLine("=== Waiting for interactive rendering (5 seconds) ===");
        await Task.Delay(5000);

        // Check grid presence
        var gridElement = await _page.QuerySelectorAsync("[data-testid='virtual-grid']");
        _output.WriteLine($"=== Grid element found: {gridElement != null} ===");

        // Check virtualize wrapper
        var virtualizeWrapper = await _page.QuerySelectorAsync("[data-testid='virtualize-wrapper']");
        _output.WriteLine($"=== Virtualize wrapper found: {virtualizeWrapper != null} ===");

        // Check loading state
        var loadingText = await _page.TextContentAsync("[data-testid='status-loading']");
        _output.WriteLine($"=== Loading status: {loadingText} ===");

        // Check request count
        var requestCountText = await _page.TextContentAsync("[data-testid='status-request-count']");
        _output.WriteLine($"=== Request count: {requestCountText} ===");

        // Count total rendered rows
        var allRows = await _page.QuerySelectorAllAsync("tr[data-row]");
        _output.WriteLine($"=== Total tr[data-row] elements: {allRows.Count} ===");

        // Get all data-row-key values
        var rowKeys = await _page.EvaluateAsync<string[]>(@"
            () => {
                const rows = document.querySelectorAll('tr[data-row-key]');
                return Array.from(rows).map(r => r.getAttribute('data-row-key'));
            }
        ");
        _output.WriteLine($"=== Total tr[data-row-key] elements: {rowKeys.Length} ===");
        _output.WriteLine($"=== Row keys: [{string.Join(", ", rowKeys)}] ===");

        // Check if any checkboxes exist
        var allCheckboxes = await _page.QuerySelectorAllAsync("input[type='checkbox']");
        _output.WriteLine($"=== Total checkboxes: {allCheckboxes.Count} ===");

        // Check if row 5 checkbox exists anywhere
        var row5Checkbox = await _page.QuerySelectorAsync("tr[data-row-key='5'] input[type='checkbox']");
        _output.WriteLine($"=== Row 5 checkbox found: {row5Checkbox != null} ===");

        // Check table structure
        var tableHtml = await _page.EvaluateAsync<string>(@"
            () => {
                const table = document.querySelector('.ui-grid__table');
                if (!table) return 'No table found';
                const tbody = table.querySelector('tbody');
                if (!tbody) return 'No tbody found';
                const rows = tbody.querySelectorAll('tr');
                return `Table has ${rows.length} tr elements in tbody`;
            }
        ");
        _output.WriteLine($"=== Table structure: {tableHtml} ===");

        // Check if Virtualize component rendered
        var virtualizeInfo = await _page.EvaluateAsync<string>(@"
            () => {
                const wrapper = document.querySelector('[data-testid=""virtualize-wrapper""]');
                if (!wrapper) return 'No virtualize wrapper';
                const table = wrapper.querySelector('table');
                if (!table) return 'Wrapper has no table';
                const tbody = table.querySelector('tbody');
                if (!tbody) return 'Table has no tbody';
                return `Wrapper→Table→TBody found, tbody.children: ${tbody.children.length}`;
            }
        ");
        _output.WriteLine($"=== Virtualize structure: {virtualizeInfo} ===");

        // Get actual rendered HTML of first few rows
        var rowsHtml = await _page.EvaluateAsync<string>(@"
            () => {
                const rows = document.querySelectorAll('tr[data-row]');
                if (rows.length === 0) return 'No data rows found';
                const first3 = Array.from(rows).slice(0, 3);
                return first3.map((r, i) => {
                    const key = r.getAttribute('data-row-key');
                    const cells = r.querySelectorAll('td');
                    const checkbox = r.querySelector('input[type=""checkbox""]');
                    return `Row ${i}: key=${key}, cells=${cells.length}, hasCheckbox=${checkbox !== null}`;
                }).join('\n');
            }
        ");
        _output.WriteLine($"=== First 3 rows:\n{rowsHtml} ===");

        // Check for any error messages in grid
        var errorElement = await _page.QuerySelectorAsync(".ui-grid__error");
        if (errorElement != null)
        {
            var errorText = await errorElement.TextContentAsync();
            _output.WriteLine($"=== Grid error message: {errorText} ===");
        }

        // Check for empty state
        var emptyElement = await _page.QuerySelectorAsync(".ui-grid__empty");
        if (emptyElement != null)
        {
            var emptyText = await emptyElement.TextContentAsync();
            _output.WriteLine($"=== Grid empty state: {emptyText} ===");
        }

        // Take screenshot
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "/tmp/diagnostic_grid_state.png",
            FullPage = true
        });
        _output.WriteLine("=== Screenshot saved to /tmp/diagnostic_grid_state.png ===");

        // Save full HTML
        var html = await _page.ContentAsync();
        await File.WriteAllTextAsync("/tmp/diagnostic_page.html", html);
        _output.WriteLine("=== Full HTML saved to /tmp/diagnostic_page.html ===");

        // Final assertion - at minimum we should have SOME rows
        Assert.True(rowKeys.Length > 0, 
            $"Expected at least one row with data-row-key, but found {rowKeys.Length}. " +
            $"Check diagnostic output above for details.");
    }
}
