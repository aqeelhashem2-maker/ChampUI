using ChampUI.Components.Actions;
using ChampUI.Components.Charts;
using ChampUI.Components.Display;
using ChampUI.Components.Feedback;
using ChampUI.Components.Layout;
using ChampUI.Components.Navigation;
using ChampUI.Models;
using ChampUI.Services;
using ChampUI.Tests.Infrastructure;
using Microsoft.AspNetCore.Components;

namespace ChampUI.Tests;

public class ComponentTests
{
    [Fact]
    public async Task Button_renders_variant_and_is_disabled_while_loading()
    {
        var host = new TestHost();
        var root = await host.RenderAsync<UiButton>(new Dictionary<string, object?> { ["Variant"] = UiVariant.Danger, ["Loading"] = true });
        var button = root.Elements("button").Single();
        Assert.True(button.HasClass("ui-btn--danger"));
        Assert.True(button.Attributes.ContainsKey("disabled"));
        Assert.Equal("true", button.Attr("aria-busy"));
    }

    [Fact]
    public async Task Badge_renders_variant_class()
    {
        var root = await new TestHost().RenderAsync<UiBadge>(new Dictionary<string, object?> { ["Variant"] = UiVariant.Success });
        Assert.True(root.Elements("span").First().HasClass("ui-badge--success"));
    }

    [Fact]
    public async Task Progress_clamps_to_the_valid_range_and_exposes_aria_values()
    {
        var root = await new TestHost().RenderAsync<UiProgress>(new Dictionary<string, object?> { ["Value"] = 250.0, ["Label"] = "Quota" });
        Assert.Contains("width:100%", root.First("ui-progress__bar").Attr("style"));
        Assert.Equal("progressbar", root.First("ui-progress__track").Attr("role"));
    }

    [Fact]
    public async Task Progress_can_hide_its_visible_label_but_keep_the_accessible_name()
    {
        var root = await new TestHost().RenderAsync<UiProgress>(new Dictionary<string, object?> { ["Value"] = 40.0, ["Label"] = "Quota", ["HideLabel"] = true });
        Assert.False(root.WithClass("ui-progress__head").Any());
        Assert.Equal("Quota", root.First("ui-progress__track").Attr("aria-label"));
    }

    [Fact]
    public async Task Avatar_derives_initials()
    {
        var root = await new TestHost().RenderAsync<UiAvatar>(new Dictionary<string, object?> { ["Name"] = "Omar Al-Rashid" });
        Assert.Contains("OA", root.First("ui-avatar").InnerText);
    }

    [Fact]
    public async Task Pagination_disables_previous_on_the_first_page_and_reports_clicks()
    {
        var page = 0;
        var host = new TestHost();
        var root = await host.RenderAsync<UiPagination>(new Dictionary<string, object?>
        {
            ["Page"] = 1, ["PageSize"] = 10, ["TotalItems"] = 45,
            ["PageChanged"] = EventCallback.Factory.Create<int>(this, p => page = p),
        });
        var prev = root.Elements("button").First(b => b.Attr("aria-label") == "Previous page");
        Assert.True(prev.Attributes.ContainsKey("disabled"));
        await host.ClickAsync(root.Elements("button").First(b => b.Attr("aria-label") == "Next page"));
        Assert.Equal(2, page);
    }

    [Fact]
    public async Task Tabs_switch_panels_when_a_tab_is_clicked()
    {
        var host = new TestHost();
        var root = await host.RenderFragmentAsync(b =>
        {
            b.OpenComponent<UiTabs>(0);
            b.AddAttribute(1, "ChildContent", (RenderFragment)(c =>
            {
                c.OpenComponent<UiTab>(0);
                c.AddAttribute(1, "Title", "One");
                c.AddAttribute(2, "ChildContent", (RenderFragment)(t => t.AddContent(0, "first-panel")));
                c.CloseComponent();
                c.OpenComponent<UiTab>(3);
                c.AddAttribute(4, "Title", "Two");
                c.AddAttribute(5, "ChildContent", (RenderFragment)(t => t.AddContent(0, "second-panel")));
                c.CloseComponent();
            }));
            b.CloseComponent();
        });
        Assert.Contains("first-panel", root.InnerText);
        Assert.DoesNotContain("second-panel", root.InnerText);

        var tabTwo = root.Elements("button").First(t => t.Attr("role") == "tab" && t.InnerText.Contains("Two"));
        await host.ClickAsync(tabTwo);
        var after = host.Snapshot();
        Assert.Contains("second-panel", after.InnerText);
        Assert.DoesNotContain("first-panel", after.InnerText);
    }

    [Fact]
    public async Task Dialog_renders_nothing_when_hidden_and_closes_on_escape()
    {
        var host = new TestHost();
        var hidden = await host.RenderAsync<UiDialog>(new Dictionary<string, object?> { ["Visible"] = false, ["Title"] = "Hello" });
        Assert.False(hidden.WithClass("ui-dialog").Any());

        bool? visible = null;
        var host2 = new TestHost();
        var shown = await host2.RenderAsync<UiDialog>(new Dictionary<string, object?>
        {
            ["Visible"] = true, ["Title"] = "Hello",
            ["VisibleChanged"] = EventCallback.Factory.Create<bool>(this, v => visible = v),
        });
        var dialog = shown.First("ui-dialog");
        Assert.Equal("dialog", dialog.Attr("role"));
        Assert.Equal("true", dialog.Attr("aria-modal"));
        await host2.KeyDownAsync(dialog, "Escape");
        Assert.Equal(false, visible);
    }

    [Fact]
    public async Task StatCard_renders_label_value_and_trend()
    {
        var root = await new TestHost().RenderAsync<UiStatCard>(new Dictionary<string, object?>
        {
            ["Label"] = "Total", ["Value"] = "24,860", ["Delta"] = "+12.4%", ["Trend"] = UiTrend.Up,
        });
        Assert.Contains("24,860", root.First("ui-stat__value").InnerText);
        Assert.True(root.WithClass("ui-trend--up").Any());
    }

    [Fact]
    public async Task EmptyState_renders_title_and_description()
    {
        var root = await new TestHost().RenderAsync<UiEmptyState>(new Dictionary<string, object?> { ["Title"] = "Nothing", ["Description"] = "Add something" });
        Assert.Contains("Nothing", root.First("ui-empty__title").InnerText);
        Assert.Contains("Add something", root.First("ui-empty__text").InnerText);
    }

    // ----- charts

    [Fact]
    public async Task LineChart_renders_every_axis_label_in_order()
    {
        // Regression test: x-axis labels must not capture the (already advanced) loop variable.
        var labels = new[] { "Jan", "Feb", "Mar", "Apr" };
        var root = await new TestHost().RenderAsync<UiLineChart>(new Dictionary<string, object?>
        {
            ["Labels"] = labels,
            ["Series"] = new[] { new ChartSeries { Name = "A", Values = new double[] { 1, 2, 3, 4 } } },
        });
        var texts = root.Elements("text").Select(t => t.InnerText.Trim()).ToList();
        foreach (var l in labels) Assert.Contains(l, texts);
        Assert.True(root.WithClass("ui-chart__line").Any());
    }

    [Fact]
    public async Task BarChart_renders_one_bar_per_value_and_correct_axis_ticks()
    {
        var root = await new TestHost().RenderAsync<UiBarChart>(new Dictionary<string, object?>
        {
            ["Labels"] = new[] { "A", "B", "C" },
            ["Series"] = new[] { new ChartSeries { Name = "S", Values = new double[] { 10, 20, 40 } } },
            ["AxisFormatter"] = (Func<double, string>)(v => $"<{v}>"),
        });
        Assert.Equal(3, root.WithClass("ui-chart__bar").Count());
        var ticks = root.Elements("text").Select(t => t.InnerText.Trim()).Where(t => t.StartsWith("<")).ToList();
        Assert.Equal(5, ticks.Distinct().Count()); // five different tick values, not one repeated value
    }

    [Fact]
    public async Task DonutChart_renders_a_ring_and_legend_entry_per_segment()
    {
        var root = await new TestHost().RenderAsync<UiDonutChart>(new Dictionary<string, object?>
        {
            ["Segments"] = new[]
            {
                new ChartSegment { Label = "A", Value = 50 }, new ChartSegment { Label = "B", Value = 30 }, new ChartSegment { Label = "C", Value = 20 },
            },
            ["CenterValue"] = "100",
        });
        Assert.Equal(3, root.WithClass("ui-donut__ring").Count());
        Assert.Equal(3, root.Elements("li").Count());
        Assert.Contains("50", root.InnerText);
    }

    // ----- services & theming

    [Fact]
    public async Task Theme_style_emits_application_tokens_for_both_modes()
    {
        var host = new TestHost(o => o.Dark("ui-accent", "#123456").Light("ui-accent", "#abcdef"));
        var root = await host.RenderAsync<UiThemeStyle>();
        var css = root.InnerText;
        Assert.Contains("--ui-accent:#123456", css);
        Assert.Contains("--ui-accent:#abcdef", css);
    }

    [Fact]
    public async Task Theme_style_neutralises_values_that_try_to_escape_the_declaration()
    {
        var host = new TestHost(o => o.Shared("ui-accent", "red;}</style><script>alert(1)</script>"));
        var root = await host.RenderAsync<UiThemeStyle>();
        Assert.DoesNotContain("<script", root.InnerText);
    }

    [Fact]
    public void Theme_state_toggles_and_notifies()
    {
        var theme = new UiThemeState(new ChampUiOptions { DefaultMode = UiThemeMode.Dark });
        var raised = 0;
        theme.Changed += () => raised++;
        theme.Toggle();
        Assert.Equal(UiThemeMode.Light, theme.Mode);
        theme.Set(UiThemeMode.Light); // no change, no event
        Assert.Equal(1, raised);
    }

    [Fact]
    public void Toasts_can_be_shown_and_dismissed()
    {
        var toasts = new ToastService();
        toasts.Show("hello", ToastKind.Success);
        Assert.Single(toasts.Toasts);
        toasts.Dismiss(toasts.Toasts[0].Id);
        Assert.Empty(toasts.Toasts);
    }

    [Fact]
    public void Framework_defaults_are_neutral_and_contain_no_brand_colours()
    {
        var options = new ChampUiOptions();
        Assert.Equal(0, options.DarkTokens.Count + options.LightTokens.Count + options.SharedTokens.Count);
    }
}
