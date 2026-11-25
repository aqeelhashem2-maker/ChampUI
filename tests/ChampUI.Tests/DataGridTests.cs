using ChampUI.Components.Data;
using ChampUI.Tests.Infrastructure;
using Microsoft.AspNetCore.Components;

namespace ChampUI.Tests;

public class DataGridTests
{
    private sealed record Person(string Name, int Age, string City);

    private static readonly Person[] People =
        Enumerable.Range(1, 25).Select(i => new Person($"Person {i:D2}", 20 + (i * 7) % 31, i % 2 == 0 ? "Riyadh" : "Cairo")).ToArray();

    private static RenderFragment Columns() => b =>
    {
        b.OpenComponent<UiColumn<Person>>(0);
        b.AddAttribute(1, "Title", "Name");
        b.AddAttribute(2, "Value", (Func<Person, object?>)(p => p.Name));
        b.CloseComponent();
        b.OpenComponent<UiColumn<Person>>(3);
        b.AddAttribute(4, "Title", "Age");
        b.AddAttribute(5, "Value", (Func<Person, object?>)(p => p.Age));
        b.CloseComponent();
        b.OpenComponent<UiColumn<Person>>(6);
        b.AddAttribute(7, "Title", "City");
        b.AddAttribute(8, "Value", (Func<Person, object?>)(p => p.City));
        b.CloseComponent();
    };

    private static async Task<(TestHost host, Node root)> RenderGridAsync(IEnumerable<Person> items, int pageSize = 10, bool loading = false, bool selectable = false, EventCallback<IReadOnlyCollection<Person>>? selected = null)
    {
        var host = new TestHost();
        var parameters = new Dictionary<string, object?>
        {
            ["Items"] = items, ["Columns"] = Columns(), ["PageSize"] = pageSize, ["Loading"] = loading, ["Selectable"] = selectable,
        };
        if (selected is { } cb) parameters["SelectedItemsChanged"] = cb;
        var root = await host.RenderAsync<UiDataGrid<Person>>(parameters);
        return (host, root);
    }

    private static List<string> FirstColumn(Node root) =>
        root.Elements("tr").Where(r => r.Attributes.ContainsKey("data-row")).Select(r => r.Elements("td").First(c => c.Attr("data-label") == "Name").InnerText.Trim()).ToList();

    [Fact]
    public async Task Renders_one_page_of_rows_and_a_summary()
    {
        var (_, root) = await RenderGridAsync(People, pageSize: 10);
        var names = FirstColumn(root);
        Assert.Equal(10, names.Count);
        Assert.Equal("Person 01", names[0]);
        Assert.Contains("1–10 of 25", root.First("ui-pagination__summary").InnerText);
    }

    [Fact]
    public async Task Clicking_a_header_sorts_ascending_then_descending_then_clears()
    {
        var (host, root) = await RenderGridAsync(People, pageSize: 25);
        var ageHeader = root.WithClass("ui-grid__sort").First(b => b.InnerText.Contains("Age"));

        await host.ClickAsync(ageHeader);
        var asc = host.Snapshot();
        var ages = AgeColumn(asc);
        Assert.Equal(ages.OrderBy(a => a).ToList(), ages);

        ageHeader = asc.WithClass("ui-grid__sort").First(b => b.InnerText.Contains("Age"));
        await host.ClickAsync(ageHeader);
        var desc = AgeColumn(host.Snapshot());
        Assert.Equal(desc.OrderByDescending(a => a).ToList(), desc);

        ageHeader = host.Snapshot().WithClass("ui-grid__sort").First(b => b.InnerText.Contains("Age"));
        await host.ClickAsync(ageHeader);
        Assert.Equal("Person 01", FirstColumn(host.Snapshot())[0]); // third click restores source order
    }

    private static List<int> AgeColumn(Node root) =>
        root.Elements("tr").Where(r => r.Attributes.ContainsKey("data-row")).Select(r => int.Parse(r.Elements("td").First(c => c.Attr("data-label") == "Age").InnerText.Trim())).ToList();

    [Fact]
    public async Task Search_filters_rows_across_searchable_columns_and_resets_to_page_one()
    {
        var (host, root) = await RenderGridAsync(People, pageSize: 5);
        var input = root.Elements("input").First(i => i.Attr("type") == "search");
        await host.InputAsync(input, "person 07");
        var snap = host.Snapshot();
        Assert.Equal(new[] { "Person 07" }, FirstColumn(snap));
        Assert.Contains("1–1 of 1", snap.First("ui-pagination__summary").InnerText);

        await host.InputAsync(snap.Elements("input").First(i => i.Attr("type") == "search"), "riyadh");
        Assert.Equal(12, host.Snapshot().First("ui-pagination__summary").InnerText.Contains("of 12") ? 12 : -1);
    }

    [Fact]
    public async Task Pagination_moves_to_the_next_page()
    {
        var (host, root) = await RenderGridAsync(People, pageSize: 10);
        var next = root.Elements("button").First(b => b.Attr("aria-label") == "Next page");
        await host.ClickAsync(next);
        var snap = host.Snapshot();
        Assert.Equal("Person 11", FirstColumn(snap)[0]);
        Assert.Contains("11–20 of 25", snap.First("ui-pagination__summary").InnerText);

        await host.ClickAsync(snap.Elements("button").First(b => b.Attr("aria-label") == "Next page"));
        Assert.Equal(5, FirstColumn(host.Snapshot()).Count); // last, partial page
    }

    [Fact]
    public async Task Shows_the_empty_state_when_nothing_matches()
    {
        var (host, root) = await RenderGridAsync(People);
        await host.InputAsync(root.Elements("input").First(i => i.Attr("type") == "search"), "zzz-no-match");
        var snap = host.Snapshot();
        Assert.Empty(FirstColumn(snap));
        Assert.Contains("No results", snap.First("ui-empty").InnerText);
    }

    [Fact]
    public async Task Shows_skeleton_rows_while_loading()
    {
        var (_, root) = await RenderGridAsync(People, loading: true);
        Assert.Empty(FirstColumn(root));
        Assert.True(root.WithClass("ui-skeleton").Any(), "skeleton placeholders expected");
    }

    [Fact]
    public async Task Row_selection_reports_the_selected_items()
    {
        IReadOnlyCollection<Person>? reported = null;
        var cb = EventCallback.Factory.Create<IReadOnlyCollection<Person>>(this, v => reported = v);
        var (host, root) = await RenderGridAsync(People, selectable: true, selected: cb);

        var rowCheckbox = root.Elements("tr").First(r => r.Attributes.ContainsKey("data-row")).Elements("input").First(i => i.Attr("type") == "checkbox");
        await host.ChangeAsync(rowCheckbox, true);

        Assert.Equal("Person 01", Assert.Single(reported!).Name);
        Assert.Contains("1 selected", host.Snapshot().First("ui-grid__selected").InnerText);
    }
}
