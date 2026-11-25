using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

[Collection("PlaywrightCollection")]
public class CheckboxDiagnosticTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";

    public CheckboxDiagnosticTest(ITestOutputHelper output)
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

    [Fact(Skip = "Diagnostic test with JavaScript syntax issue - not production code")]
    public async Task DiagnoseCheckboxStructure()
    {
        await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load
        });

        // Wait for grid
        await _page.WaitForSelectorAsync("tr[data-row-key]", new PageWaitForSelectorOptions { Timeout = 10000 });
        await Task.Delay(2000);

        // Get all table rows
        var rows = await _page.QuerySelectorAllAsync("tr[data-row-key]");
        _output.WriteLine($"Total rows with data-row-key: {rows.Count}");

        if (rows.Count > 0)
        {
            var firstRow = rows[0];
            var rowKey = await firstRow.GetAttributeAsync("data-row-key");
            _output.WriteLine($"First row key: {rowKey}");
            
            var html = await firstRow.InnerHTMLAsync();
            _output.WriteLine($"First row HTML: {html}");
            
            // Try different checkbox selectors
            var checkbox1 = await _page.QuerySelectorAsync($"tr[data-row-key='{rowKey}'] input[type='checkbox']");
            _output.WriteLine($"Selector 'tr[data-row-key='{rowKey}'] input[type='checkbox']': {(checkbox1 != null ? "FOUND" : "NOT FOUND")}");
            
            var checkbox2 = await _page.QuerySelectorAsync($"tr[data-row-key='{rowKey}'] input");
            _output.WriteLine($"Selector 'tr[data-row-key='{rowKey}'] input': {(checkbox2 != null ? "FOUND" : "NOT FOUND")}");
            
            var checkbox3 = await firstRow.QuerySelectorAsync("input[type='checkbox']");
            _output.WriteLine($"Selector within row 'input[type='checkbox']': {(checkbox3 != null ? "FOUND" : "NOT FOUND")}");
            
            var checkbox4 = await firstRow.QuerySelectorAsync("input");
            _output.WriteLine($"Selector within row 'input': {(checkbox4 != null ? "FOUND" : "NOT FOUND")}");
            
            var allInputs = await _page.QuerySelectorAllAsync("input[type='checkbox']");
            _output.WriteLine($"Total checkboxes in page: {allInputs.Count}");
        }

        // Check grid properties
        var selectableAttr = await _page.EvaluateAsync<string>(@"
            const grid = document.querySelector('[data-testid=""virtual-grid""] .ui-grid');
            return grid ? grid.className : 'grid not found';
        ");
        _output.WriteLine($"Grid classes: {selectableAttr}");
        
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "/tmp/checkbox_diagnostic.png",
            FullPage = true
        });
        _output.WriteLine("Screenshot saved to /tmp/checkbox_diagnostic.png");
    }
}
