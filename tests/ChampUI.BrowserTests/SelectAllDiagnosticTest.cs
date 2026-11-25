using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

[Collection("PlaywrightCollection")]
public class SelectAllDiagnosticTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";

    public SelectAllDiagnosticTest(ITestOutputHelper output)
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

    [Fact]
    public async Task DiagnoseSelectAllBehavior()
    {
        await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load
        });

        // Wait for grid
        await _page.WaitForSelectorAsync("tr[data-row-key]", new PageWaitForSelectorOptions { Timeout = 10000 });
        await Task.Delay(2000);

        _output.WriteLine("=== BEFORE SELECT ALL ===");
        
        // Count rows
        var rowsBefore = await _page.QuerySelectorAllAsync("tr[data-row-key]");
        _output.WriteLine($"Total rendered rows: {rowsBefore.Count}");
        
        var rowKeys = new List<string>();
        foreach (var row in rowsBefore)
        {
            var key = await row.GetAttributeAsync("data-row-key");
            if (key != null) rowKeys.Add(key);
        }
        _output.WriteLine($"Row keys: [{string.Join(", ", rowKeys)}]");
        
        // Count selected rows
        var selectedBefore = await _page.QuerySelectorAllAsync("tr[data-row-key].is-selected");
        _output.WriteLine($"Selected rows before: {selectedBefore.Count}");
        
        // Find Select All checkbox
        var selectAllCheckbox = await _page.QuerySelectorAsync("thead input[type='checkbox']");
        _output.WriteLine($"Select All checkbox found: {selectAllCheckbox != null}");
        
        if (selectAllCheckbox != null)
        {
            var isCheckedBefore = await selectAllCheckbox.IsCheckedAsync();
            _output.WriteLine($"Select All checked before: {isCheckedBefore}");
            
            // Click Select All using JS
            await selectAllCheckbox.EvaluateAsync("element => element.click()");
            _output.WriteLine("=== CLICKED SELECT ALL ===");
            
            // Wait for state change
            await Task.Delay(1000);
            
            var isCheckedAfter = await selectAllCheckbox.IsCheckedAsync();
            _output.WriteLine($"Select All checked after: {isCheckedAfter}");
            
            // Check selected rows after
            var selectedAfter = await _page.QuerySelectorAllAsync("tr[data-row-key].is-selected");
            _output.WriteLine($"Selected rows after: {selectedAfter.Count}");
            
            var selectedKeys = new List<string>();
            foreach (var row in selectedAfter)
            {
                var key = await row.GetAttributeAsync("data-row-key");
                if (key != null) selectedKeys.Add(key);
            }
            _output.WriteLine($"Selected row keys: [{string.Join(", ", selectedKeys)}]");
            
            // Check individual checkbox states
            var allCheckboxes = await _page.QuerySelectorAllAsync("tbody input[type='checkbox']");
            var checkedCount = 0;
            foreach (var cb in allCheckboxes)
            {
                if (await cb.IsCheckedAsync()) checkedCount++;
            }
            _output.WriteLine($"Individual checkboxes checked: {checkedCount} / {allCheckboxes.Count}");
            
            // Check for console errors
            var logs = await _page.EvaluateAsync<string>(@"
                () => {
                    const logs = window['__testLogs'] || [];
                    return logs.join('\n');
                }
            ");
            if (!string.IsNullOrEmpty(logs))
            {
                _output.WriteLine($"Console logs: {logs}");
            }
        }
        
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "/tmp/selectall_diagnostic.png",
            FullPage = true
        });
        _output.WriteLine("Screenshot saved to /tmp/selectall_diagnostic.png");
    }
}
