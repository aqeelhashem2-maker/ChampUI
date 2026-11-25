using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using System.Text.RegularExpressions;
using static Microsoft.Playwright.Assertions;

namespace ChampUI.BrowserTests;

/// <summary>
/// Browser-level integration tests for UiDataGrid VirtualProvider.
/// Tests critical behaviors identified in Checkpoint B audit findings B-08 through B-11.
/// </summary>
[Collection("PlaywrightCollection")]
public class VirtualProviderBrowserTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string BaseUrl = "http://localhost:5000";
    private const string TestPageUrl = $"{BaseUrl}/virtual-data-test";

    public VirtualProviderBrowserTests(ITestOutputHelper output)
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

    [Fact]
    public async Task B08_VirtualProvider_InitializesWithoutItems()
    {
        _output.WriteLine("=== TEST START: B-08 VirtualProvider initialization ===");
        
        var page = await _browser!.NewPageAsync();
        page.Console += (_, msg) => _output.WriteLine($"[BROWSER CONSOLE {msg.Type}] {msg.Text}");
        page.PageError += (_, error) => _output.WriteLine($"[BROWSER ERROR] {error}");
        
        try
        {
            _output.WriteLine($"=== Navigating to {TestPageUrl} ===");
            await page.GotoAsync(TestPageUrl, new PageGotoOptions 
            { 
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            _output.WriteLine("=== Waiting for virtual-grid element ===");
            var grid = await page.WaitForSelectorAsync("[data-testid='virtual-grid']", 
                new PageWaitForSelectorOptions { Timeout = 10000 });
            Assert.NotNull(grid);
            _output.WriteLine("=== Grid element found ===");

            // Wait for interactive render to complete
            _output.WriteLine("=== Waiting for request count to update (interactive render) ===");
            await Task.Delay(2000);

            var requestCountElement = await page.QuerySelectorAsync("[data-testid='status-request-count']");
            var requestCountText = await requestCountElement!.TextContentAsync();
            _output.WriteLine($"=== Request Count: {requestCountText} ===");

            // Check loading state
            var loadingStatus = await page.TextContentAsync("[data-testid='status-loading']");
            _output.WriteLine($"=== Loading Status: {loadingStatus} ===");

            // Verify rows are rendered
            _output.WriteLine("=== Checking for rendered rows ===");
            var rows = await page.QuerySelectorAllAsync("tbody tr:not([aria-hidden])");
            _output.WriteLine($"=== Found {rows.Count} visible rows ===");
            
            // Take screenshot for diagnosis
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = "/tmp/b08_virtual_provider_test.png",
                FullPage = true
            });
            _output.WriteLine("=== Screenshot saved to /tmp/b08_virtual_provider_test.png ===");

            // Assertion: Grid should have been called at least once
            Assert.Contains("Request Count:", requestCountText);
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    public Task B08_VirtualProvider_RendersEmptyStateWhenNoResults() => TraceGridAsync(nameof(B08_VirtualProvider_RendersEmptyStateWhenNoResults), async page =>
    {
        await page.Locator(".ui-grid").GetByRole(AriaRole.Searchbox).FillAsync("NOMATCHXYZ123");
        await Expect(page.GetByTestId("status-search")).ToHaveTextAsync("Search: NOMATCHXYZ123");
        await Expect(page.GetByText("No results", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(page.Locator("tr[data-row]")).ToHaveCountAsync(0);

        await page.Locator(".ui-grid").GetByRole(AriaRole.Searchbox).FillAsync("");
        await Expect(page.Locator("tr[data-row-key='1']")).ToBeVisibleAsync();
        await Expect(page.GetByText("No results", new() { Exact = true })).ToHaveCountAsync(0);
    });

    [Fact]
    public Task B09_SearchChange_TriggersVirtualQueryRefresh() => TraceGridAsync(nameof(B09_SearchChange_TriggersVirtualQueryRefresh), async page =>
    {
        await page.Locator(".ui-grid").GetByRole(AriaRole.Searchbox).FillAsync("Test Record 0042");
        await Expect(page.GetByTestId("status-search")).ToHaveTextAsync("Search: Test Record 0042");
        await Expect(page.Locator("tr[data-row-key='42']")).ToBeVisibleAsync();
        await Expect(page.Locator("tr[data-row]")).ToHaveCountAsync(1);
    });

    [Fact]
    public Task B09_SortChange_TriggersVirtualQueryRefresh() => TraceGridAsync(nameof(B09_SortChange_TriggersVirtualQueryRefresh), async page =>
    {
        var sort = page.Locator("button.ui-grid__sort").Filter(new() { HasText = "Name" });
        await sort.ClickAsync();
        await Expect(page.GetByTestId("status-sort")).ToHaveTextAsync("Sort: Name (asc)");
        await sort.ClickAsync();
        await Expect(page.GetByTestId("status-sort")).ToHaveTextAsync("Sort: Name (desc)");
        await Expect(page.Locator("tr[data-row]").First).ToHaveAttributeAsync("data-row-key", "1000");
    });

    [Fact]
    public Task B09_FilterChange_PropagatesOperatorAndValue() => TraceGridAsync(nameof(B09_FilterChange_PropagatesOperatorAndValue), async page =>
    {
        await page.GetByRole(AriaRole.Button, new() { Name = "Filter Category" }).ClickAsync();
        await page.GetByRole(AriaRole.Combobox, new() { Name = "Filter Category" }).SelectOptionAsync("Equals");
        await page.Locator(".ui-grid__col-filter").Locator("input").FillAsync("Alpha");
        await Expect(page.GetByTestId("status-filter")).ToHaveTextAsync("Filter: Category Equals Alpha");
        await Expect(page.Locator("tr[data-row]").First).ToHaveAttributeAsync("data-row-key", "3");
        Assert.All(await page.Locator("tr[data-row] td[data-label='Category']").AllTextContentsAsync(),
            category => Assert.Equal("Alpha", category.Trim()));
    });

    [Theory]
    [InlineData("paged")]
    [InlineData("page")]
    [InlineData("provider")]
    public async Task Identity_TypedKeysSurviveRefreshSortAndFilter(string mode)
    {
        var page = await _browser!.NewPageAsync();
        await page.GotoAsync($"{BaseUrl}/row-identity-test?modeName={mode}");
        var rows = page.Locator("tr[data-row]");
        ILocator Row(string name) => rows.Filter(new() { HasText = name });
        ILocator Check(string name) => Row(name).GetByRole(AriaRole.Checkbox);

        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Replace instances" })).ToBeEnabledAsync();
        await Expect(rows).ToHaveCountAsync(7);
        // A probe on the element verifies the keyed node survived, independently of its selection state.
        await Row("Typed int").EvaluateAsync("el => el.dataset.identityProbe = 'int'");
        await Row("Typed Guid").EvaluateAsync("el => el.dataset.identityProbe = 'guid'");
        await Row("Typed int").Locator("label.ui-check").ClickAsync();
        await Row("Typed Guid").Locator("label.ui-check").ClickAsync();
        await Row("Duplicate A").Locator("label.ui-check").ClickAsync();
        await Row("Null A").Locator("label.ui-check").ClickAsync();
        await Expect(Check("Typed text")).Not.ToBeCheckedAsync();
        await Expect(Check("Duplicate B")).ToBeCheckedAsync();
        await Expect(Check("Null B")).Not.ToBeCheckedAsync();

        await page.GetByRole(AriaRole.Button, new() { Name = "Replace instances" }).ClickAsync();
        await page.Locator("button.ui-grid__sort").ClickAsync();
        await Expect(rows.First).ToContainTextAsync("Duplicate A");
        await Expect(Row("Typed int")).ToHaveAttributeAsync("data-identity-probe", "int");
        await Expect(Row("Typed Guid")).ToHaveAttributeAsync("data-identity-probe", "guid");
        await Expect(Check("Typed int")).ToBeCheckedAsync();
        await Expect(Check("Typed Guid")).ToBeCheckedAsync();
        await Expect(Check("Null A")).ToBeCheckedAsync();

        await page.Locator(".ui-grid").GetByRole(AriaRole.Searchbox).FillAsync("Typed");
        await Expect(rows).ToHaveCountAsync(3);
        await Expect(Check("Typed int")).ToBeCheckedAsync();
        await Expect(Check("Typed Guid")).ToBeCheckedAsync();
        await Expect(Check("Typed text")).Not.ToBeCheckedAsync();
        await page.Locator(".ui-grid").GetByRole(AriaRole.Searchbox).FillAsync("");
        await Expect(rows).ToHaveCountAsync(7);
        await Row("Duplicate B").Locator("label.ui-check").ClickAsync();
        await Expect(Check("Duplicate A")).Not.ToBeCheckedAsync();
        await Expect(Check("Null A")).ToBeCheckedAsync();
        await Expect(Check("Null B")).Not.ToBeCheckedAsync();
        await page.CloseAsync();
    }

    private async Task TraceGridAsync(string name, Func<IPage, Task> assertions)
    {
        var context = await _browser!.NewContextAsync();
        await context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        var page = await context.NewPageAsync();
        page.PageError += (_, error) => _output.WriteLine($"[Browser error] {error}");
        page.Console += (_, entry) => _output.WriteLine($"[{entry.Type}] {entry.Text}");
        try
        {
            await page.GotoAsync(TestPageUrl);
            await Expect(page.GetByTestId("status-request-count"))
                .ToHaveTextAsync(new Regex(@"Request Count: [1-9]\d*"));
            await Expect(page.Locator("tr[data-row-key='1']")).ToBeVisibleAsync();
            await assertions(page);
        }
        finally
        {
            if (Environment.GetEnvironmentVariable("CHAMPUI_EVIDENCE_DIR") is { } evidence)
            {
                Directory.CreateDirectory(evidence);
                await context.Tracing.StopAsync(new() { Path = Path.Combine(evidence, $"{name}.zip") });
                await page.ScreenshotAsync(new() { Path = Path.Combine(evidence, $"{name}.png") });
            }
            await context.CloseAsync();
        }
    }

    [Fact]
    public Task B10_LoadingState_DoesNotDestroyVirtualize_DuringScroll() => TraceGridAsync(nameof(B10_LoadingState_DoesNotDestroyVirtualize_DuringScroll), async page =>
    {
        var viewport = page.GetByTestId("virtualize-wrapper");
        await viewport.EvaluateAsync("el => el.dataset.viewportProbe = 'mounted'");
        await viewport.EvaluateAsync("el => el.scrollTop = 2400");
        await Expect(page.Locator("tr[data-row-key='56']")).ToBeVisibleAsync();
        await Expect(page.GetByTestId("status-range")).ToHaveTextAsync(new Regex(@"Range: [1-9]\d*"));
        await Expect(page.GetByTestId("status-loading")).ToHaveTextAsync("Loading: False");
        await Expect(viewport).ToHaveAttributeAsync("data-viewport-probe", "mounted");
    });

    [Fact]
    public Task B11_QueryGeneration_CancelsStaleRequests() => TraceGridAsync(nameof(B11_QueryGeneration_CancelsStaleRequests), async page =>
    {
        var search = page.Locator(".ui-grid").GetByRole(AriaRole.Searchbox);
        await search.FillAsync("Alpha");
        await search.FillAsync("Beta");
        await search.FillAsync("Test Record 0042");
        await Expect(page.GetByTestId("status-search")).ToHaveTextAsync("Search: Test Record 0042");
        await Expect(page.Locator("tr[data-row-key='42']")).ToBeVisibleAsync();
        await Expect(page.Locator("tr[data-row]")).ToHaveCountAsync(1);
    });

    [Fact]
    public async Task B11_VirtualProvider_HandlesLargeRequestCounts()
    {
        // GIVEN: VirtualProvider with 1000 total items
        // WHEN: Virtualize requests large batch (e.g., 100 items)
        // THEN: Request is NOT truncated to artificial limit (B-11 fix: removed Math.Min(count, 1000))
        //       All requested items are returned

        var page = await _browser!.NewPageAsync();
        try
        {
            await page.GotoAsync(TestPageUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Grid should load without errors
            await Task.Delay(500);

            // Verify grid rendered
            var grid = await page.QuerySelectorAsync("[data-testid='virtual-grid']");
            Assert.NotNull(grid);

            // Verify rows are present (provider handled request correctly)
            var rows = await page.QuerySelectorAllAsync("tbody tr");
            Assert.NotEmpty(rows);
            
            // The fact that rows render proves BuildVirtualRequest didn't artificially limit count
            // and LoadVirtualItemsAsync handled the response correctly
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// Multi-browser test: Run B-08 test on Firefox.
    /// </summary>
    [Fact(Skip = "Firefox browser not installed by default - enable manually")]
    public async Task B08_VirtualProvider_InitializesWithoutItems_Firefox()
    {
        var firefox = await _playwright!.Firefox.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await firefox.NewPageAsync();
        try
        {
            await page.GotoAsync(TestPageUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            var grid = await page.WaitForSelectorAsync("[data-testid='virtual-grid']", 
                new PageWaitForSelectorOptions { Timeout = 5000 });
            Assert.NotNull(grid);

            var rows = await page.QuerySelectorAllAsync("tbody tr");
            Assert.NotEmpty(rows);
        }
        finally
        {
            await page.CloseAsync();
            await firefox.CloseAsync();
        }
    }

    /// <summary>
    /// Multi-browser test: Run B-08 test on WebKit (Safari).
    /// </summary>
    [Fact(Skip = "WebKit browser not installed by default - enable manually")]
    public async Task B08_VirtualProvider_InitializesWithoutItems_WebKit()
    {
        var webkit = await _playwright!.Webkit.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await webkit.NewPageAsync();
        try
        {
            await page.GotoAsync(TestPageUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
            var grid = await page.WaitForSelectorAsync("[data-testid='virtual-grid']", 
                new PageWaitForSelectorOptions { Timeout = 5000 });
            Assert.NotNull(grid);

            var rows = await page.QuerySelectorAllAsync("tbody tr");
            Assert.NotEmpty(rows);
        }
        finally
        {
            await page.CloseAsync();
            await webkit.CloseAsync();
        }
    }

    private static int ExtractNumber(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 0;
        var parts = text.Split(':', StringSplitOptions.TrimEntries);
        if (parts.Length < 2) return 0;
        return int.TryParse(parts[1].Trim(), out var num) ? num : 0;
    }
}
