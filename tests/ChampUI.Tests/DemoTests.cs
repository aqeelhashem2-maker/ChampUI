using System.Text.RegularExpressions;
using ChampUI.Core.Models;
using ChampUI.Demo.Components;
using ChampUI.Demo.Data;
using ChampUI.Demo.Services;
using ChampUI.Models;
using ChampUI.Services;

namespace ChampUI.Tests;

public class DemoTests
{
    private static string RepoRoot()
    {
        var overrideRoot = Environment.GetEnvironmentVariable("CHAMPUI_REPO_ROOT");
        if (!string.IsNullOrEmpty(overrideRoot)) return overrideRoot;
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ChampUI.slnx"))) dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }

    [Fact]
    public void Every_showcase_label_has_text_in_both_languages()
    {
        var labels = new[] { Copy.Navigation, Copy.Foundations, Copy.Forms, Copy.Data, Copy.Charts, Copy.Theme };
        Assert.All(labels, label =>
        {
            Assert.Matches("[a-zA-Z]", label.English);
            Assert.Matches(@"\p{IsArabic}", label.Arabic);
        });
    }

    [Fact]
    public void Shell_binds_each_translation_and_selects_the_requested_language()
    {
        var layout = File.ReadAllText(Path.Combine(RepoRoot(), "demo", "ChampUI.Demo", "Components", "Layout", "MainLayout.razor"));
        var names = typeof(Copy).GetProperties().Select(property => property.Name).Order().ToArray();
        var used = Regex.Matches(layout, @"Copy\.(\w+)\.For\(_arabic\)")
            .Select(match => match.Groups[1].Value).Distinct().Order().ToArray();
        Assert.Equal(names, used);
        Assert.Equal("البيانات", Copy.Data.For(true));
        Assert.Equal("Data", Copy.Data.For(false));
        Assert.Equal("العربية", Copy.Language.For(false));
        Assert.Equal("English", Copy.Language.For(true));
    }

    [Fact]
    public async Task Paged_provider_returns_exact_sorted_and_filtered_rows()
    {
        var page = await new ProjectData().GetDataAsync(new DataGridRequest
        {
            Page = 2,
            PageSize = 4,
            Filters = [new GridFilterDescriptor("Category", GridFilterOperator.Equals, "Studio")],
            Sorts = [new("Id", true)]
        });
        Assert.Equal(60, page.TotalCount);
        Assert.Equal(new[] { 112, 110, 108, 106 }, page.Items.Select(row => row.Id));
    }

    [Fact]
    public async Task Range_provider_returns_the_requested_window()
    {
        var page = await new ProjectData().GetVirtualDataAsync(new DataGridVirtualRequest
        {
            StartIndex = 10,
            Count = 4,
            GlobalSearch = "Workshop"
        });
        Assert.Equal(60, page.TotalCount);
        Assert.Equal(new[] { 21, 23, 25, 27 }, page.Items.Select(row => row.Id));
    }

    [Fact]
    public void Fictional_project_data_is_unique_and_balanced()
    {
        var rows = ProjectData.CreateRows();
        Assert.Equal(120, rows.Count);
        Assert.Equal(Enumerable.Range(1, 120), rows.Select(row => row.Id));
        Assert.Equal(60, rows.Count(row => row.Category == "Studio"));
        Assert.Equal(60, rows.Count(row => row.Category == "Workshop"));
    }

    [Fact]
    public void Independent_showcase_data_is_deterministic()
    {
        var first = ProjectData.CreateRows();
        var second = ProjectData.CreateRows();
        Assert.Equal<ProjectRow>(first, second);
        Assert.All(first.Zip(second), pair => Assert.NotSame(pair.First, pair.Second));
    }

    [Fact]
    public void Csv_export_quotes_values_and_neutralises_formula_injection()
    {
        var csv = DownloadService.ToCsv(new[] { "Name", "Note" }, new[]
        {
            (IEnumerable<object?>)new object?[] { "=HYPERLINK(\"x\")", "He said \"hi\", ok" }
        });
        Assert.Contains("\"'=HYPERLINK(\"\"x\"\")\",\"He said \"\"hi\"\", ok\"", csv);
    }

    [Fact]
    public async Task Export_uses_full_query_and_rejects_cancellation()
    {
        var provider = new ProjectData();
        var request = new DataGridVirtualRequest { StartIndex = 4, Count = 2, GlobalSearch = "Project 01" };
        var export = await provider.GetAllForExportAsync(request);
        Assert.NotNull(export);
        Assert.Equal(Enumerable.Range(10, 10), export.Items.Select(row => row.Id));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => provider.GetVirtualDataAsync(
            new DataGridVirtualRequest { CancellationToken = new CancellationToken(true) }));
    }

    [Fact]
    public void The_library_is_independent_of_the_showcase_and_uses_neutral_defaults()
    {
        var library = Path.Combine(RepoRoot(), "src", "ChampUI");
        var project = File.ReadAllText(Path.Combine(library, "ChampUI.csproj"));
        Assert.DoesNotContain("ProjectReference", project);
        var options = new ChampUiOptions();
        var theme = new UiThemeState(options);
        var start = theme.Mode;
        theme.Toggle();
        Assert.NotEqual(start, theme.Mode);
        theme.Toggle();
        Assert.Equal(start, theme.Mode);
        Assert.Empty(options.DarkTokens);
        Assert.Empty(options.LightTokens);
    }
}
