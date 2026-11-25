using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

/// <summary>
/// Task B: Test isolation between concurrent browser sessions.
/// Verifies that multiple browser contexts do not contaminate each other's diagnostic state.
/// </summary>
[Collection("PlaywrightCollection")]
public class ConcurrentSessionIsolationTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private const string BaseUrl = "http://localhost:5000";

    public ConcurrentSessionIsolationTest(ITestOutputHelper output)
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
    }

    public async Task DisposeAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
    }

    [Fact]
    public async Task TwoContexts_IndependentCounters_DoNotInterfere()
    {
        _output.WriteLine("=== TEST: Concurrent session isolation ===");

        // Create two independent browser contexts (simulate two users)
        var context1 = await _browser!.NewContextAsync();
        var context2 = await _browser.NewContextAsync();

        var page1 = await context1.NewPageAsync();
        var page2 = await context2.NewPageAsync();

        try
        {
            _output.WriteLine("=== Context 1: Navigate to virtualize-smoke ===");
            await page1.GotoAsync($"{BaseUrl}/virtualize-smoke", new PageGotoOptions
            {
                Timeout = 30000,
                WaitUntil = WaitUntilState.NetworkIdle
            });

            await Task.Delay(2000);

            _output.WriteLine("=== Context 2: Navigate to virtualize-smoke ===");
            await page2.GotoAsync($"{BaseUrl}/virtualize-smoke", new PageGotoOptions
            {
                Timeout = 30000,
                WaitUntil = WaitUntilState.NetworkIdle
            });

            await Task.Delay(2000);

            // Read counter from context 1
            var counter1Text = await page1.TextContentAsync("[data-testid='provider-calls']");
            _output.WriteLine($"=== Context 1 counter: {counter1Text} ===");

            // Read counter from context 2
            var counter2Text = await page2.TextContentAsync("[data-testid='provider-calls']");
            _output.WriteLine($"=== Context 2 counter: {counter2Text} ===");

            // Scroll in context 1 to trigger more provider calls
            _output.WriteLine("=== Context 1: Scrolling ===");
            await page1.EvaluateAsync(@"
                const wrapper = document.querySelector('[data-testid=""virtualize-wrapper""]');
                if (wrapper) wrapper.scrollTop = 2000;
            ");

            await Task.Delay(1000);

            var counter1AfterScroll = await page1.TextContentAsync("[data-testid='provider-calls']");
            _output.WriteLine($"=== Context 1 after scroll: {counter1AfterScroll} ===");

            // Verify context 2 counter didn't change due to context 1 scroll
            var counter2AfterContext1Scroll = await page2.TextContentAsync("[data-testid='provider-calls']");
            _output.WriteLine($"=== Context 2 (should be unchanged): {counter2AfterContext1Scroll} ===");

            // Counters should be independent
            Assert.Contains("Provider Calls:", counter1Text);
            Assert.Contains("Provider Calls:", counter2Text);
            
            // Context 2 counter should NOT have increased due to context 1 activity
            Assert.Equal(counter2Text, counter2AfterContext1Scroll);
        }
        finally
        {
            await page1.CloseAsync();
            await page2.CloseAsync();
            await context1.CloseAsync();
            await context2.CloseAsync();
        }
    }
}
