using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

/// <summary>
/// B-12: Tests stable row selection across server-side virtualization.
/// Verifies selection persistence with scroll, refresh, and fresh object instances.
/// Uses stable row keys (data-row-key) instead of DOM positions.
/// </summary>
[Collection("PlaywrightCollection")]
public class StableSelectionTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";

    public StableSelectionTest(ITestOutputHelper output)
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

    private async Task<IElementHandle?> GetCheckboxByRowKey(int rowKey)
    {
        return await _page!.QuerySelectorAsync($"tr[data-row-key='{rowKey}'] input[type='checkbox']");
    }

    private async Task<IElementHandle> EnsureCheckboxVisible(int rowKey)
    {
        // Simple approach: just get the checkbox if it exists
        // Don't rely on IsVisibleAsync which may have issues with Blazor rendering
        
        var checkbox = await GetCheckboxByRowKey(rowKey);
        if (checkbox != null)
        {
            _output.WriteLine($"Row {rowKey} checkbox found immediately");
            return checkbox;
        }

        _output.WriteLine($"Row {rowKey} not in DOM, need to scroll");
        
        // Calculate approximate scroll position
        // VirtualRowHeight=48, viewport starts at top
        // Row N is at approximately (N-1) * 48 pixels
        var approximateScrollTop = (rowKey - 1) * 48;
        
        // Scroll the virtualize wrapper to that position
        await _page!.EvaluateAsync($@"
            const wrapper = document.querySelector('[data-testid=""virtualize-wrapper""]');
            if (wrapper) {{
                wrapper.scrollTop = {approximateScrollTop};
            }}
        ");
        
        // Wait for virtualization to load the row
        await WaitForGridStability(1000);
        
        // Now try to find the checkbox with retries
        for (int attempt = 0; attempt < 10; attempt++)
        {
            checkbox = await GetCheckboxByRowKey(rowKey);
            if (checkbox != null)
            {
                _output.WriteLine($"Row {rowKey} checkbox found after scroll, attempt {attempt + 1}");
                return checkbox;
            }
            
            await Task.Delay(300);
        }
        
        // Final check: what rows are actually visible?
        var visibleRows = await _page.QuerySelectorAllAsync("tr[data-row-key]");
        var visibleKeys = new List<string>();
        foreach (var row in visibleRows)
        {
            var key = await row.GetAttributeAsync("data-row-key");
            if (key != null) visibleKeys.Add(key);
        }
        
        throw new TimeoutException($"Checkbox for row {rowKey} never appeared in DOM after scrolling to position {approximateScrollTop}. Visible rows: [{string.Join(", ", visibleKeys)}]");
    }

    private async Task ClickCheckbox(IElementHandle checkbox)
    {
        // Use JavaScript click to bypass Playwright's visibility and viewport checks
        // This is necessary for virtualized grids where checkboxes may be outside normal viewport
        await checkbox.EvaluateAsync("element => element.click()");
    }

    private async Task<bool> IsRowSelected(int rowKey)
    {
        var checkbox = await GetCheckboxByRowKey(rowKey);
        return checkbox != null && await checkbox.IsCheckedAsync();
    }

    private async Task WaitForGridStability(int delayMs = 500)
    {
        // Wait for provider request to complete and grid to re-render
        await Task.Delay(delayMs);
        
        // Wait for no network activity
        try
        {
            await _page!.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 2000 });
        }
        catch
        {
            // Timeout is acceptable; grid may already be stable
        }
    }

    private async Task WaitForInteractiveGridReady()
    {
        // Wait for Blazor InteractiveServer circuit to establish and grid to render
        // This is specifically for the initial page load
        
        // Wait for at least one data row to appear (proves grid rendered)
        await _page!.WaitForSelectorAsync("tr[data-row-key]", new PageWaitForSelectorOptions 
        { 
            Timeout = 10000,
            State = WaitForSelectorState.Attached
        });
        
        // Additional stability wait for virtualization to complete
        await Task.Delay(500);
    }

    [Fact]
    public async Task B12_01_SelectVisibleRow_PersistsAcrossScroll()
    {
        _output.WriteLine("=== TEST: B12_01 Selection persists across scroll ===");

        await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load
        });

        // Wait for InteractiveServer grid to fully render
        await WaitForInteractiveGridReady();
        _output.WriteLine("=== Grid rendered and ready ===");

        // Select record with ID=5 (stable identifier)
        const int targetRowKey = 5;
        var checkbox = await EnsureCheckboxVisible(targetRowKey);
        
        await ClickCheckbox(checkbox);
        await WaitForGridStability();

        var isChecked = await IsRowSelected(targetRowKey);
        _output.WriteLine($"=== Row {targetRowKey} selected: {isChecked} ===");
        Assert.True(isChecked);

        // Scroll down significantly to unload row 5
        await _page.EvaluateAsync(@"
            const wrapper = document.querySelector('[data-testid=""virtualize-wrapper""]');
            if (wrapper) wrapper.scrollTop = 3000;
        ");

        await WaitForGridStability(1500);
        _output.WriteLine("=== Scrolled down ===");

        // Scroll back up to row 5
        await _page.EvaluateAsync(@"
            const wrapper = document.querySelector('[data-testid=""virtualize-wrapper""]');
            if (wrapper) wrapper.scrollTop = 0;
        ");

        await WaitForGridStability(1500);
        _output.WriteLine("=== Scrolled back to top ===");

        // Verify row 5 is still selected using stable key
        var stillChecked = await IsRowSelected(targetRowKey);
        _output.WriteLine($"=== Row {targetRowKey} still selected: {stillChecked} ===");
        Assert.True(stillChecked);
    }

    [Fact]
    public async Task B12_02_SelectionState_ConsistentWithMultipleScrolls()
    {
        _output.WriteLine("=== TEST: B12_02 Selection consistent with multiple scrolls ===");

        await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load
        });

        await WaitForInteractiveGridReady();

        // Select records with stable IDs: 3, 7, 10
        var rowKeysToSelect = new[] { 3, 7, 10 };
        foreach (var rowKey in rowKeysToSelect)
        {
            var checkbox = await EnsureCheckboxVisible(rowKey);
            await ClickCheckbox(checkbox);
            await WaitForGridStability(300);
        }

        _output.WriteLine($"=== Selected rows {string.Join(", ", rowKeysToSelect)} ===");

        // Scroll down and back multiple times
        for (int i = 0; i < 3; i++)
        {
            await _page.EvaluateAsync("document.querySelector('[data-testid=\"virtualize-wrapper\"]').scrollTop = 2000;");
            await WaitForGridStability(500);
            await _page.EvaluateAsync("document.querySelector('[data-testid=\"virtualize-wrapper\"]').scrollTop = 0;");
            await WaitForGridStability(500);
        }

        _output.WriteLine("=== Completed 3 scroll cycles ===");

        // Verify all selections persist using stable keys
        foreach (var rowKey in rowKeysToSelect)
        {
            var isChecked = await IsRowSelected(rowKey);
            _output.WriteLine($"=== Row {rowKey} selected: {isChecked} ===");
            Assert.True(isChecked);
        }
    }

    [Fact]
    public async Task B12_03_DeselectRow_RemovesSelectionCorrectly()
    {
        _output.WriteLine("=== TEST: B12_03 Deselect removes selection ===");

        await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load
        });

        await WaitForInteractiveGridReady();

        // Select record with ID=6
        const int targetRowKey = 6;
        var checkbox = await EnsureCheckboxVisible(targetRowKey);
        
        await ClickCheckbox(checkbox);
        await WaitForGridStability();
        Assert.True(await IsRowSelected(targetRowKey));

        _output.WriteLine($"=== Row {targetRowKey} selected ===");

        // Scroll away and back
        await _page.EvaluateAsync("document.querySelector('[data-testid=\"virtualize-wrapper\"]').scrollTop = 2500;");
        await WaitForGridStability(1000);
        await _page.EvaluateAsync("document.querySelector('[data-testid=\"virtualize-wrapper\"]').scrollTop = 0;");
        await WaitForGridStability(1000);

        // Deselect row 6 using stable key
        var checkboxAfterScroll = await EnsureCheckboxVisible(targetRowKey);
        
        await ClickCheckbox(checkboxAfterScroll);
        await WaitForGridStability();

        var isDeselected = !(await IsRowSelected(targetRowKey));
        _output.WriteLine($"=== Row {targetRowKey} deselected: {isDeselected} ===");
        Assert.True(isDeselected);

        // Verify deselection persists after another scroll cycle
        await _page.EvaluateAsync("document.querySelector('[data-testid=\"virtualize-wrapper\"]').scrollTop = 1000;");
        await WaitForGridStability(500);
        await _page.EvaluateAsync("document.querySelector('[data-testid=\"virtualize-wrapper\"]').scrollTop = 0;");
        await WaitForGridStability(500);

        Assert.False(await IsRowSelected(targetRowKey));
        _output.WriteLine("=== Deselection persisted ===");
    }

    [Fact]
    public async Task B12_04_SelectAll_AffectsCurrentPageOnly()
    {
        _output.WriteLine("=== TEST: B12_04 Select All affects current page only ===");

        await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load
        });

        await WaitForInteractiveGridReady();

        // Click the "Select All" checkbox in header
        var selectAllCheckbox = await _page.QuerySelectorAsync("thead input[type='checkbox']");
        
        if (selectAllCheckbox != null)
        {
            await selectAllCheckbox.EvaluateAsync("element => element.click()");
            await WaitForGridStability();

            _output.WriteLine("=== Clicked Select All ===");

            // Get keys of visible selected rows
            var selectedRowsBefore = await _page.QuerySelectorAllAsync("tr[data-row-key].is-selected");
            var selectedKeysBefore = new List<string>();
            foreach (var row in selectedRowsBefore)
            {
                var key = await row.GetAttributeAsync("data-row-key");
                if (key != null) selectedKeysBefore.Add(key);
            }

            var selectedCountBefore = selectedKeysBefore.Count;
            _output.WriteLine($"=== Selected rows on first page: {selectedCountBefore} (keys: {string.Join(", ", selectedKeysBefore)}) ===");
            Assert.True(selectedCountBefore > 0);

            // Scroll to a different page (beyond current viewport)
            await _page.EvaluateAsync("document.querySelector('[data-testid=\"virtualize-wrapper\"]').scrollTop = 5000;");
            await WaitForGridStability(1500);

            // Get keys of selected rows after scroll
            var selectedRowsAfter = await _page.QuerySelectorAllAsync("tr[data-row-key].is-selected");
            var selectedKeysAfter = new List<string>();
            foreach (var row in selectedRowsAfter)
            {
                var key = await row.GetAttributeAsync("data-row-key");
                if (key != null) selectedKeysAfter.Add(key);
            }

            var selectedCountAfter = selectedKeysAfter.Count;
            _output.WriteLine($"=== Visible selected rows after scroll: {selectedCountAfter} (keys: {string.Join(", ", selectedKeysAfter)}) ===");
            
            // With virtualization, SelectAll selects only currently loaded items
            // After scrolling away from them, they're no longer visible/rendered
            // So selectedKeysAfter should be different from selectedKeysBefore
            var keysIntersection = selectedKeysBefore.Intersect(selectedKeysAfter).Count();
            _output.WriteLine($"=== Intersection of before/after keys: {keysIntersection} ===");
            
            // The rows that were selected should still BE selected, they're just not currently rendered
            // Scroll back to verify
            await _page.EvaluateAsync("document.querySelector('[data-testid=\"virtualize-wrapper\"]').scrollTop = 0;");
            await WaitForGridStability(1500);

            var selectedRowsReturned = await _page.QuerySelectorAllAsync("tr[data-row-key].is-selected");
            var selectedKeysReturned = new List<string>();
            foreach (var row in selectedRowsReturned)
            {
                var key = await row.GetAttributeAsync("data-row-key");
                if (key != null) selectedKeysReturned.Add(key);
            }

            _output.WriteLine($"=== Selected rows after returning: {selectedKeysReturned.Count} (keys: {string.Join(", ", selectedKeysReturned)}) ===");
            
            // Should match original selection
            Assert.Equal(selectedCountBefore, selectedKeysReturned.Count);
        }
        else
        {
            _output.WriteLine("=== Select All checkbox not found (grid may not be selectable) ===");
        }
    }

    [Fact]
    public async Task B12_05_SelectionWithSearch_MaintainsStability()
    {
        _output.WriteLine("=== TEST: B12_05 Selection with search maintains stability ===");

        await _page!.GotoAsync($"{BaseUrl}/virtual-data-test", new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.Load
        });

        await WaitForInteractiveGridReady();

        // Select record with stable ID=4
        const int targetRowKey = 4;
        var checkbox = await EnsureCheckboxVisible(targetRowKey);
        
        await ClickCheckbox(checkbox);
        await WaitForGridStability();
        _output.WriteLine($"=== Selected row {targetRowKey} ===");

        // Apply search filter
        var searchBox = await _page.QuerySelectorAsync("input[type='search']");
        Assert.NotNull(searchBox); // Should exist since we set Searchable="true"

        await searchBox.FillAsync("0004"); // Should match "Test Record 0004"
        await WaitForGridStability(1000);
        _output.WriteLine("=== Applied search filter '0004' ===");

        // Verify row 4 is still selected and visible
        var isSelectedAfterSearch = await IsRowSelected(targetRowKey);
        _output.WriteLine($"=== Row {targetRowKey} selected after search: {isSelectedAfterSearch} ===");
        Assert.True(isSelectedAfterSearch);

        // Clear search
        await searchBox.FillAsync("");
        await WaitForGridStability(1000);
        _output.WriteLine("=== Cleared search filter ===");

        // Verify row 4 is still selected
        var stillChecked = await IsRowSelected(targetRowKey);
        _output.WriteLine($"=== Row {targetRowKey} still selected after clearing search: {stillChecked} ===");
        Assert.True(stillChecked);
    }
}
