using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace ChampUI.BrowserTests;

[Collection("PlaywrightCollection")]
public class ShowcaseBrowserTests
{
    [Fact]
    public async Task Showcase_supports_themes_RTL_and_data_sources_without_console_errors()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        var errors = new System.Collections.Concurrent.ConcurrentQueue<string>();
        page.PageError += (_, error) => errors.Enqueue(error);
        page.Console += (_, message) => { if (message.Type == "error") errors.Enqueue(message.Text); };

        await page.GotoAsync("http://localhost:5000/");
        var shell = page.Locator(".ui-shell");
        var theme = page.GetByRole(AriaRole.Button, new() { Name = "Switch theme" });
        await Expect(theme).ToBeEnabledAsync();
        var initialTheme = await shell.GetAttributeAsync("data-theme");
        Assert.Contains(initialTheme, new[] { "light", "dark" });
        await theme.ClickAsync();
        await Expect(shell).ToHaveAttributeAsync("data-theme", initialTheme == "light" ? "dark" : "light");
        await theme.ClickAsync();
        await Expect(shell).ToHaveAttributeAsync("data-theme", initialTheme!);
        await page.GetByRole(AriaRole.Button, new() { Name = "العربية" }).ClickAsync();
        await Expect(shell).ToHaveAttributeAsync("dir", "rtl");
        await Expect(shell).ToHaveAttributeAsync("lang", "ar");
        await Expect(page.GetByRole(AriaRole.Link, new() { Name = "البيانات" })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "English" }).ClickAsync();
        await Expect(shell).ToHaveAttributeAsync("dir", "ltr");

        await page.GetByRole(AriaRole.Link, new() { Name = "Data", Exact = true }).ClickAsync();
        var source = page.GetByRole(AriaRole.Combobox, new() { Name = "Data source" });
        await source.SelectOptionAsync("Paged");
        await Expect(page.GetByText("Project 001", new() { Exact = true })).ToBeVisibleAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Next page" }).ClickAsync();
        await Expect(page.GetByText("Project 009", new() { Exact = true })).ToBeVisibleAsync();
        await source.SelectOptionAsync("Virtual");
        await Expect(page.GetByText("Project 001", new() { Exact = true })).ToBeVisibleAsync();
        Assert.Empty(errors);
    }
}
