using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

/// <summary>
/// Tests native Blazor Virtualize component behavior.
/// Isolates framework-level virtualization from ChampUI DataGrid integration.
/// </summary>
[Collection("PlaywrightCollection")]
public class VirtualizeSmokeTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";
    private const string TestPageUrl = $"{BaseUrl}/virtualize-smoke";

    public VirtualizeSmokeTest(ITestOutputHelper output)
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
    public async Task Virtualize_01_PageLoads_Successfully()
    {
        _output.WriteLine($"=== Navigate to {TestPageUrl} ===");
        
        var response = await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        Assert.NotNull(response);
        Assert.True(response.Ok);
        _output.WriteLine($"=== Page loaded: {response.Status} ===");
    }

    [Fact]
    public async Task Virtualize_02_ProviderCalled_OnInitialLoad()
    {
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        _output.WriteLine("=== Waiting for provider call indicator ===");
        await Task.Delay(1000);

        var providerCallsElement = await _page.WaitForSelectorAsync("[data-testid='provider-calls']", new()
        {
            Timeout = 10000
        });
        Assert.NotNull(providerCallsElement);

        var callsText = await providerCallsElement.TextContentAsync();
        _output.WriteLine($"=== Provider calls: {callsText} ===");

        Assert.NotNull(callsText);
        Assert.Contains("Provider Calls:", callsText);
        
        // Extract number
        var match = System.Text.RegularExpressions.Regex.Match(callsText, @"Provider Calls: (\d+)");
        Assert.True(match.Success);
        var callCount = int.Parse(match.Groups[1].Value);
        Assert.True(callCount > 0, "Provider should be called at least once on initial load");
    }

    [Fact]
    public async Task Virtualize_03_ItemsRender_InViewport()
    {
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        _output.WriteLine("=== Waiting for virtual items to render ===");
        await Task.Delay(1500);

        var items = await _page.QuerySelectorAllAsync("[data-testid^='virtual-item-']");
        _output.WriteLine($"=== Rendered items: {items.Count} ===");

        Assert.NotEmpty(items);
        Assert.True(items.Count > 0 && items.Count < 1000, 
            $"Expected a subset of items to render (not all 1000). Found: {items.Count}");
    }

    [Fact]
    public async Task Virtualize_04_Scroll_TriggersNewProviderRequest()
    {
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await Task.Delay(1000);

        var providerCallsElement = await _page.WaitForSelectorAsync("[data-testid='provider-calls']");
        Assert.NotNull(providerCallsElement);
        var initialCallsText = await providerCallsElement.TextContentAsync();
        var initialCallsMatch = System.Text.RegularExpressions.Regex.Match(initialCallsText!, @"Provider Calls: (\d+)");
        var initialCalls = int.Parse(initialCallsMatch.Groups[1].Value);
        _output.WriteLine($"=== Initial provider calls: {initialCalls} ===");

        _output.WriteLine("=== Scrolling virtualize container ===");
        await _page.EvaluateAsync(@"
            const wrapper = document.querySelector('[data-testid=""virtualize-wrapper""]');
            if (wrapper) {
                wrapper.scrollTop = 2000; // Scroll down
            }
        ");

        await Task.Delay(1000);

        var newCallsText = await providerCallsElement.TextContentAsync();
        var newCallsMatch = System.Text.RegularExpressions.Regex.Match(newCallsText!, @"Provider Calls: (\d+)");
        var newCalls = int.Parse(newCallsMatch.Groups[1].Value);
        _output.WriteLine($"=== After scroll provider calls: {newCalls} ===");

        Assert.True(newCalls > initialCalls, 
            $"Scroll should trigger new provider request. Initial: {initialCalls}, After: {newCalls}");
    }

    [Fact]
    public async Task Virtualize_05_TotalItemCount_IsCorrect()
    {
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await Task.Delay(1000);

        var totalItemsElement = await _page.WaitForSelectorAsync("[data-testid='total-items']");
        Assert.NotNull(totalItemsElement);
        var totalText = await totalItemsElement.TextContentAsync();
        _output.WriteLine($"=== Total items: {totalText} ===");

        Assert.Contains("Total Items: 1000", totalText);
    }

    [Fact]
    public async Task Virtualize_06_ResetStats_Works()
    {
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await Task.Delay(1000);

        var providerCallsElement = await _page.WaitForSelectorAsync("[data-testid='provider-calls']");
        Assert.NotNull(providerCallsElement);
        var initialText = await providerCallsElement.TextContentAsync();
        _output.WriteLine($"=== Before reset: {initialText} ===");

        var resetBtn = await _page.WaitForSelectorAsync("[data-testid='reset-stats']");
        Assert.NotNull(resetBtn);
        await resetBtn.ClickAsync();
        await Task.Delay(500);

        var afterResetText = await providerCallsElement.TextContentAsync();
        _output.WriteLine($"=== After reset: {afterResetText} ===");

        Assert.Contains("Provider Calls: 0", afterResetText);
    }
}
