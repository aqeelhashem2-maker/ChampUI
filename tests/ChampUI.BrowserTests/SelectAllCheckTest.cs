using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

[Collection("PlaywrightCollection")]
public class SelectAllCheckTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";

    public SelectAllCheckTest(ITestOutputHelper output)
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
        _page.SetDefaultTimeout(15000);
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
    public async Task TestSelectAllWithCheckAsync()
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
        
        // Find Select All checkbox label - UiCheckbox hides the real input, so click the label
        var labelSelector = "th.ui-grid__check label.ui-check";
        var labelCount = await _page.Locator(labelSelector).CountAsync();
        _output.WriteLine($"Select All label found: {labelCount}");
        
        if (labelCount == 0)
        {
            _output.WriteLine("ERROR: No Select All label found!");
            throw new Exception("Select All label not found");
        }
        
        var selectAllLabel = _page.Locator(labelSelector).First;
        
        // Also check the input state via the input element
        var inputSelector = "thead input[type='checkbox']";
        var inputLocator = _page.Locator(inputSelector).First;
        
        try
        {
            var isCheckedBefore = await inputLocator.IsCheckedAsync();
            _output.WriteLine($"Select All checked before: {isCheckedBefore}");
            
            // Click the label (which triggers the input) using Locator API
            _output.WriteLine("Clicking label with ClickAsync...");
            await selectAllLabel.ClickAsync(new LocatorClickOptions { Timeout = 10000 });
            _output.WriteLine("=== Label Click COMPLETED ===");
            
            // Wait for Blazor to process
            await Task.Delay(2000);
            
            var isCheckedAfter = await inputLocator.IsCheckedAsync();
            _output.WriteLine($"Select All checked after: {isCheckedAfter}");
            
            // Check selected rows after
            var selectedAfter = await _page.Locator("tr[data-row-key].is-selected").CountAsync();
            _output.WriteLine($"Selected rows after: {selectedAfter}");
            
            // Check individual checkboxes
            var totalCheckboxes = await _page.Locator("tbody input[type='checkbox']").CountAsync();
            var checkedCheckboxes = await _page.Locator("tbody input[type='checkbox']:checked").CountAsync();
            _output.WriteLine($"Individual checkboxes checked: {checkedCheckboxes} / {totalCheckboxes}");
            
            Assert.True(selectedAfter > 0, "Expected at least some rows to be selected after checking Select All");
        }
        catch (Exception ex)
        {
            _output.WriteLine($"Exception during test: {ex.Message}");
            _output.WriteLine($"Stack trace: {ex.StackTrace}");
            
            await _page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "/tmp/selectall_error.png",
                FullPage = true
            });
            _output.WriteLine("Error screenshot saved to /tmp/selectall_error.png");
            throw;
        }
    }
}
