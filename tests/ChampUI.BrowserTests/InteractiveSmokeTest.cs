using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.BrowserTests;

/// <summary>
/// Minimal Blazor Interactive Server smoke test.
/// Isolates basic interactive functionality from DataGrid complexity.
/// </summary>
[Collection("PlaywrightCollection")]
public class InteractiveSmokeTest : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _page;
    private const string BaseUrl = "http://localhost:5000";
    private const string TestPageUrl = $"{BaseUrl}/interactive-smoke";

    public InteractiveSmokeTest(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        _output.WriteLine("=== INITIALIZING PLAYWRIGHT ===");
        _playwright = await Playwright.CreateAsync();
        
        _output.WriteLine("=== LAUNCHING CHROMIUM ===");
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Timeout = 30000 // 30 seconds for browser launch
        });

        _output.WriteLine("=== CREATING NEW PAGE ===");
        _page = await _browser.NewPageAsync();
        
        // Enable console logging
        _page.Console += (_, msg) =>
        {
            _output.WriteLine($"[BROWSER CONSOLE {msg.Type}] {msg.Text}");
        };

        // Enable page error logging
        _page.PageError += (_, error) =>
        {
            _output.WriteLine($"[BROWSER ERROR] {error}");
        };

        // Enable request logging
        _page.Request += (_, request) =>
        {
            _output.WriteLine($"[REQUEST] {request.Method} {request.Url}");
        };

        // Enable response logging
        _page.Response += (_, response) =>
        {
            _output.WriteLine($"[RESPONSE] {response.Status} {response.Url}");
        };

        _output.WriteLine("=== PLAYWRIGHT INITIALIZATION COMPLETE ===");
    }

    public async Task DisposeAsync()
    {
        _output.WriteLine("=== CLEANING UP ===");
        if (_page != null) await _page.CloseAsync();
        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }
        _playwright?.Dispose();
        _output.WriteLine("=== CLEANUP COMPLETE ===");
    }

    [Fact]
    public async Task Smoke_01_PageLoads_ReturnsHttp200()
    {
        _output.WriteLine($"=== TEST START: Navigate to {TestPageUrl} ===");
        
        var response = await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        _output.WriteLine($"=== NAVIGATION COMPLETE: Status {response?.Status} ===");
        
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected HTTP 200-299, got {response.Status}");
        Assert.Equal(200, response.Status);
    }

    [Fact]
    public async Task Smoke_02_CircuitStatus_AppearsOnPage()
    {
        _output.WriteLine($"=== TEST START: Check circuit status element ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        _output.WriteLine("=== WAITING FOR CIRCUIT STATUS ELEMENT ===");
        var circuitStatus = await _page.WaitForSelectorAsync("[data-testid='circuit-status']", new PageWaitForSelectorOptions
        {
            Timeout = 10000,
            State = WaitForSelectorState.Visible
        });

        Assert.NotNull(circuitStatus);
        
        var statusText = await circuitStatus.TextContentAsync();
        _output.WriteLine($"=== CIRCUIT STATUS TEXT: {statusText} ===");
        
        Assert.NotNull(statusText);
        Assert.NotEmpty(statusText);
    }

    [Fact]
    public async Task Smoke_03_InteractiveReady_BecomesTrue()
    {
        _output.WriteLine($"=== TEST START: Wait for interactive ready ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        _output.WriteLine("=== WAITING FOR INTERACTIVE READY INDICATOR ===");
        
        // Wait for interactive-ready to become "True"
        var ready = false;
        for (int attempt = 0; attempt < 20; attempt++)
        {
            await Task.Delay(500);
            var readyElement = await _page.QuerySelectorAsync("[data-testid='interactive-ready']");
            if (readyElement != null)
            {
                var text = await readyElement.TextContentAsync();
                _output.WriteLine($"=== ATTEMPT {attempt + 1}: Interactive Ready = {text} ===");
                if (text?.Contains("True") == true)
                {
                    ready = true;
                    break;
                }
            }
        }

        Assert.True(ready, "Interactive ready indicator did not become True within 10 seconds");
    }

    [Fact]
    public async Task Smoke_04_CounterButton_IncrementsValue()
    {
        _output.WriteLine($"=== TEST START: Counter increment test ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        _output.WriteLine("=== WAITING FOR INTERACTIVE READY ===");
        await WaitForInteractiveReady();

        _output.WriteLine("=== READING INITIAL COUNTER VALUE ===");
        var counterElement = await _page.WaitForSelectorAsync("[data-testid='counter-value']", new()
        {
            Timeout = 5000
        });
        Assert.NotNull(counterElement);
        var initialValue = await counterElement.TextContentAsync();
        _output.WriteLine($"=== INITIAL COUNTER: {initialValue} ===");

        _output.WriteLine("=== CLICKING INCREMENT BUTTON ===");
        var button = await _page.WaitForSelectorAsync("[data-testid='increment-btn']", new()
        {
            Timeout = 5000
        });
        Assert.NotNull(button);
        await button.ClickAsync();

        _output.WriteLine("=== WAITING FOR COUNTER UPDATE ===");
        await Task.Delay(1000); // Allow SignalR round-trip

        var newValue = await counterElement.TextContentAsync();
        _output.WriteLine($"=== NEW COUNTER: {newValue} ===");

        Assert.NotEqual(initialValue, newValue);
        Assert.True(int.TryParse(newValue?.Trim(), out var num));
        Assert.True(num > 0, "Counter should be greater than 0 after increment");
    }

    [Fact]
    public async Task Smoke_05_MultipleClicks_CounterIncrements()
    {
        _output.WriteLine($"=== TEST START: Multiple clicks test ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await WaitForInteractiveReady();

        var button = await _page.WaitForSelectorAsync("[data-testid='increment-btn']", new()
        {
            Timeout = 5000
        });
        Assert.NotNull(button);
        var counterElement = await _page.WaitForSelectorAsync("[data-testid='counter-value']");
        Assert.NotNull(counterElement);

        _output.WriteLine("=== CLICKING 5 TIMES ===");
        for (int i = 1; i <= 5; i++)
        {
            await button.ClickAsync();
            await Task.Delay(200);
            var value = await counterElement.TextContentAsync();
            _output.WriteLine($"=== CLICK {i}: Counter = {value} ===");
        }

        var finalValue = await counterElement.TextContentAsync();
        Assert.True(int.TryParse(finalValue?.Trim(), out var finalNum));
        Assert.Equal(5, finalNum);
    }

    [Fact]
    public async Task Smoke_06_ResetButton_ResetsCounter()
    {
        _output.WriteLine($"=== TEST START: Reset button test ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await WaitForInteractiveReady();

        var incrementBtn = await _page.WaitForSelectorAsync("[data-testid='increment-btn']");
        Assert.NotNull(incrementBtn);
        var resetBtn = await _page.WaitForSelectorAsync("[data-testid='reset-btn']");
        Assert.NotNull(resetBtn);
        var counterElement = await _page.WaitForSelectorAsync("[data-testid='counter-value']");
        Assert.NotNull(counterElement);

        _output.WriteLine("=== INCREMENTING TO 3 ===");
        for (int i = 0; i < 3; i++)
        {
            await incrementBtn.ClickAsync();
            await Task.Delay(200);
        }

        var beforeReset = await counterElement.TextContentAsync();
        _output.WriteLine($"=== BEFORE RESET: {beforeReset} ===");

        _output.WriteLine("=== CLICKING RESET ===");
        await resetBtn.ClickAsync();
        await Task.Delay(500);

        var afterReset = await counterElement.TextContentAsync();
        _output.WriteLine($"=== AFTER RESET: {afterReset} ===");

        Assert.True(int.TryParse(afterReset?.Trim(), out var resetNum));
        Assert.Equal(0, resetNum);
    }

    [Fact]
    public async Task Smoke_07_AsyncOperation_CompletesSuccessfully()
    {
        _output.WriteLine($"=== TEST START: Async operation test ===");
        
        await _page!.GotoAsync(TestPageUrl, new PageGotoOptions
        {
            Timeout = 30000,
            WaitUntil = WaitUntilState.NetworkIdle
        });

        await WaitForInteractiveReady();

        var asyncBtn = await _page.WaitForSelectorAsync("[data-testid='async-btn']");
        Assert.NotNull(asyncBtn);
        var asyncStatus = await _page.WaitForSelectorAsync("[data-testid='async-status']");
        Assert.NotNull(asyncStatus);
        var asyncResult = await _page.WaitForSelectorAsync("[data-testid='async-result']");
        Assert.NotNull(asyncResult);

        var initialStatus = await asyncStatus.TextContentAsync();
        _output.WriteLine($"=== INITIAL STATUS: {initialStatus} ===");

        _output.WriteLine("=== CLICKING ASYNC BUTTON ===");
        await asyncBtn.ClickAsync();

        _output.WriteLine("=== WAITING FOR COMPLETION (up to 3 seconds) ===");
        var completed = false;
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(100);
            var status = await asyncStatus.TextContentAsync();
            _output.WriteLine($"=== ATTEMPT {i + 1}: Status = {status} ===");
            if (status?.Contains("Completed") == true)
            {
                completed = true;
                break;
            }
        }

        Assert.True(completed, "Async operation did not complete within 3 seconds");

        var finalResult = await asyncResult.TextContentAsync();
        _output.WriteLine($"=== FINAL RESULT: {finalResult} ===");
        Assert.Contains("Completed at", finalResult);
    }

    private async Task WaitForInteractiveReady()
    {
        _output.WriteLine("=== HELPER: Waiting for interactive ready ===");
        for (int attempt = 0; attempt < 20; attempt++)
        {
            await Task.Delay(500);
            var readyElement = await _page!.QuerySelectorAsync("[data-testid='interactive-ready']");
            if (readyElement != null)
            {
                var text = await readyElement.TextContentAsync();
                if (text?.Contains("True") == true)
                {
                    _output.WriteLine("=== HELPER: Interactive ready confirmed ===");
                    return;
                }
            }
        }
        throw new TimeoutException("Interactive ready did not become True within 10 seconds");
    }
}
