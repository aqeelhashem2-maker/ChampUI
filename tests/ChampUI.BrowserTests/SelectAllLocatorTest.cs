using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

[Collection("PlaywrightCollection")]
public class SelectAllLocatorTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";

    public SelectAllLocatorTest(ITestOutputHelper output)
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

    [Fact(Skip = "Checkbox visibility issue in headless Chrome - works in headed mode")]
    public async Task TestSelectAllWithLocator()
    {
        await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load
        });

        // Wait for grid to render
        await _page.WaitForSelectorAsync("tr[data-row-key]", new PageWaitForSelectorOptions { Timeout = 10000 });
        await Task.Delay(2000);

        _output.WriteLine("=== BEFORE SELECT ALL ===");
        
        var rowsBefore = await _page.Locator("tr[data-row-key]").CountAsync();
        _output.WriteLine($"Total rendered rows: {rowsBefore}");
        
        var selectedBefore = await _page.Locator("tr[data-row-key].is-selected").CountAsync();
        _output.WriteLine($"Selected rows before: {selectedBefore}");
        
        // Try different selectors to find Select All checkbox
        var selector1 = "thead .ui-grid__check input[type='checkbox']";
        var selector2 = "thead input[type='checkbox']";
        var selector3 = "th input[type='checkbox']";
        
        var count1 = await _page.Locator(selector1).CountAsync();
        var count2 = await _page.Locator(selector2).CountAsync();
        var count3 = await _page.Locator(selector3).CountAsync();
        
        _output.WriteLine($"Selector '{selector1}': {count1} found");
        _output.WriteLine($"Selector '{selector2}': {count2} found");
        _output.WriteLine($"Selector '{selector3}': {count3} found");
        
        // Use the most specific selector that works
        var selectAllLocator = _page.Locator("thead input[type='checkbox']").First;
        
        var exists = await selectAllLocator.CountAsync();
        _output.WriteLine($"Select All checkbox exists: {exists > 0}");
        
        if (exists > 0)
        {
            try
            {
                var isCheckedBefore = await selectAllLocator.IsCheckedAsync(new LocatorIsCheckedOptions { Timeout = 2000 });
                _output.WriteLine($"Select All checked before: {isCheckedBefore}");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Cannot check checkbox state: {ex.Message}");
            }
            
            // Click using Locator with shorter timeout
            try
            {
                await selectAllLocator.ClickAsync(new LocatorClickOptions { Timeout = 10000 });
                _output.WriteLine("=== CLICKED SELECT ALL WITH LOCATOR ===");
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Click failed: {ex.Message}");
                throw;
            }
            
            // Wait for Blazor to process
            await Task.Delay(2000);
            
            var isCheckedAfter = await selectAllLocator.IsCheckedAsync(new LocatorIsCheckedOptions { Timeout = 2000 });
            _output.WriteLine($"Select All checked after: {isCheckedAfter}");
            
            // Check selected rows after
            var selectedAfter = await _page.Locator("tr[data-row-key].is-selected").CountAsync();
            _output.WriteLine($"Selected rows after: {selectedAfter}");
            
            // Check individual checkboxes
            var totalCheckboxes = await _page.Locator("tbody input[type='checkbox']").CountAsync();
            var checkedCheckboxes = await _page.Locator("tbody input[type='checkbox']:checked").CountAsync();
            _output.WriteLine($"Individual checkboxes checked: {checkedCheckboxes} / {totalCheckboxes}");
            
            // Get selected row keys
            if (selectedAfter > 0)
            {
                var selectedRows = await _page.Locator("tr[data-row-key].is-selected").AllAsync();
                var keys = new List<string>();
                foreach (var row in selectedRows)
                {
                    var key = await row.GetAttributeAsync("data-row-key");
                    if (key != null) keys.Add(key);
                }
                _output.WriteLine($"Selected row keys: [{string.Join(", ", keys)}]");
            }
        }
        
        await _page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = "/tmp/selectall_locator.png",
            FullPage = true
        });
        _output.WriteLine("Screenshot saved to /tmp/selectall_locator.png");
    }
}
