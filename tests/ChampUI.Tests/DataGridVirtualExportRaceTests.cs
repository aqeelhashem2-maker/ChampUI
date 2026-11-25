using System.Collections;
using System.Reflection;
using System.Text;
using ChampUI.Components.Data;
using ChampUI.Core.Interfaces;
using ChampUI.Core.Models;
using ChampUI.Tests.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;

namespace ChampUI.Tests;

public sealed class DataGridVirtualExportRaceTests
{
    [Fact]
    public async Task LateRangeResponsesCannotUpdateSelectionAcrossRangesOrQueries()
    {
        var provider = new ControlledProvider();
        using var host = new TestHost();
        await RenderAsync(host, provider);
        var grid = host.GetComponent<UiDataGrid<int>>();

        var first = await host.Dispatcher.InvokeAsync(() => Task.FromResult(LoadRange(grid, 10)));
        var second = await host.Dispatcher.InvokeAsync(() => Task.FromResult(LoadRange(grid, 20)));
        provider.SecondRange.SetResult(Response(201, 202));
        await second;
        Assert.Equal([201, 202], RangeRows(second));

        provider.FirstRange.SetResult(Response(101, 102));
        await first;
        Assert.Empty(RangeRows(first));

        var previousQuery = await host.Dispatcher.InvokeAsync(() => Task.FromResult(LoadRange(grid, 30)));
        await SearchAsync(host, "new query");
        await host.Dispatcher.InvokeAsync(() => LoadRange(grid, 40));
        provider.ObsoleteRange.SetResult(Response(301, 302));
        await previousQuery;
        Assert.Empty(RangeRows(previousQuery));

        var checkbox = host.Snapshot().Elements("input").Single(node =>
            node.Attr("aria-label")?.StartsWith("Select all", StringComparison.OrdinalIgnoreCase) == true);
        await host.ChangeAsync(checkbox, true);
        Assert.Contains("2 selected", host.Snapshot().InnerText);
    }

    [Fact]
    public async Task VirtualExportDropsObsoleteQueryAndDownloadsExactFreshIds()
    {
        var provider = new ControlledProvider();
        var js = new RecordingJs();
        using var host = new TestHost(js: js);
        await RenderAsync(host, provider);

        var oldExport = await InvokeExportAsync(host);
        await provider.ExportStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await SearchAsync(host, "new query");
        await oldExport.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Empty(js.Downloads);

        var newExport = await InvokeExportAsync(host);
        await newExport;
        provider.ObsoleteExport.SetResult(new() { Items = [101, 102] });
        Assert.Equal(new[] { (null, 0, 0), ("new query", 0, 0) }, provider.ExportRequests);
        var csv = Assert.Single(js.Downloads);
        Assert.Equal(new[] { "\"ID\"", "\"401\"", "\"402\"" }, csv.TrimStart('\uFEFF')
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries));
    }

    private static Task<Node> RenderAsync(TestHost host, ControlledProvider provider)
    {
        RenderFragment columns = builder =>
        {
            builder.OpenComponent<UiColumn<int>>(0);
            builder.AddComponentParameter(1, nameof(UiColumn<int>.Title), "ID");
            builder.AddComponentParameter(2, nameof(UiColumn<int>.Value), (Func<int, object?>)(value => value));
            builder.CloseComponent();
        };
        return host.RenderAsync<UiDataGrid<int>>(new Dictionary<string, object?>
        {
            ["VirtualProvider"] = provider, ["Columns"] = columns, ["Selectable"] = true, ["AllowCsvExport"] = true
        });
    }

    private static Task SearchAsync(TestHost host, string query) =>
        host.InputAsync(host.Snapshot().Elements("input").Single(node => node.Attr("type") == "search"), query);

    private static Task<Task> InvokeExportAsync(TestHost host) => host.Dispatcher.InvokeAsync(() =>
    {
        var grid = host.GetComponent<UiDataGrid<int>>();
        var export = (Task)Method("ExportCsvAsync").Invoke(grid, [DataGridExportScope.AllFilteredRows])!;
        return Task.FromResult(export);
    });

    private static MethodInfo Method(string name) => typeof(UiDataGrid<int>)
        .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

    private static Task LoadRange(UiDataGrid<int> grid, int offset)
    {
        var result = Method("LoadVirtualItemsAsync").Invoke(grid, [new ItemsProviderRequest(offset, 2, default)])!;
        return (Task)result.GetType().GetMethod("AsTask")!.Invoke(result, null)!;
    }

    private static IEnumerable<int> RangeRows(Task task)
    {
        var result = task.GetType().GetProperty("Result")!.GetValue(task)!;
        var rows = (IEnumerable)result.GetType().GetProperty("Items")!.GetValue(result)!;
        return rows.Cast<object>().Select(row => (int)row.GetType().GetProperty("Item")!.GetValue(row)!);
    }

    private static DataGridVirtualResponse<int> Response(params int[] items) => new() { Items = items, TotalCount = 500 };

    private sealed class ControlledProvider : IDataGridVirtualDataProvider<int>
    {
        public TaskCompletionSource<DataGridVirtualResponse<int>> FirstRange { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<DataGridVirtualResponse<int>> SecondRange { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<DataGridVirtualResponse<int>> ObsoleteRange { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<DataGridExportResult<int>?> ObsoleteExport { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ExportStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public List<(string? Search, int Start, int Count)> ExportRequests { get; } = [];

        // The delayed responses deliberately ignore cancellation.
        public Task<DataGridVirtualResponse<int>> GetVirtualDataAsync(DataGridVirtualRequest request) =>
            request.StartIndex switch
            {
                10 => FirstRange.Task,
                20 => SecondRange.Task,
                30 => ObsoleteRange.Task,
                40 when request.GlobalSearch == "new query" => Task.FromResult(Response(401, 402)),
                _ => Task.FromResult(Response())
            };

        public Task<DataGridExportResult<int>?> GetAllForExportAsync(DataGridVirtualRequest request)
        {
            ExportRequests.Add((request.GlobalSearch, request.StartIndex, request.Count));
            ExportStarted.TrySetResult();
            return request.GlobalSearch == null
                ? ObsoleteExport.Task
                : Task.FromResult<DataGridExportResult<int>?>(new() { Items = [401, 402] });
        }
    }

    private sealed class RecordingJs : IJSRuntime
    {
        public List<string> Downloads { get; } = [];

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, default, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "ChampUI.downloadFile")
                Downloads.Add(Encoding.UTF8.GetString(Convert.FromBase64String((string)args![2]!)));
            return ValueTask.FromResult(default(TValue)!);
        }
    }
}
