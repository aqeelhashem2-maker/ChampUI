using ChampUI.Components.Data;
using ChampUI.Core.Interfaces;
using ChampUI.Core.Models;
using ChampUI.Tests.Infrastructure;
using Microsoft.AspNetCore.Components;

namespace ChampUI.Tests;

public class DataGrid_B03_VirtualizationValidation_Tests
{
    [Theory]
    [InlineData(0f)]
    [InlineData(-10f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public async Task VirtualRowHeight_RejectsInvalidValues(float height)
    {
        var exception = await RejectAsync(nameof(UiDataGrid<int>.VirtualRowHeight), height);
        Assert.Contains("VirtualRowHeight must be a positive finite number", exception.Message);
    }

    [Fact]
    public async Task VirtualOverscan_RejectsNegativeValues()
    {
        var exception = await RejectAsync(nameof(UiDataGrid<int>.VirtualOverscan), -5);
        Assert.Contains("VirtualOverscan must be >= 0", exception.Message);
        Assert.Contains("Current value: -5", exception.Message);
    }

    [Fact]
    public async Task VirtualViewportHeight_RejectsInvalidSyntax()
    {
        var exception = await RejectAsync(nameof(UiDataGrid<int>.VirtualViewportHeight), "invalid-height");
        Assert.Contains("VirtualViewportHeight must be a valid CSS height value", exception.Message);
        Assert.Contains("invalid-height", exception.Message);
    }

    [Theory]
    [InlineData("400px")]
    [InlineData("50vh")]
    [InlineData("20rem")]
    [InlineData("80%")]
    [InlineData("15em")]
    public Task VirtualViewportHeight_AcceptsValidSyntax(string height) =>
        AcceptAsync(nameof(UiDataGrid<int>.VirtualViewportHeight), height);

    [Theory]
    [InlineData(32f)]
    [InlineData(48f)]
    [InlineData(64f)]
    [InlineData(80f)]
    public Task VirtualRowHeight_AcceptsPositiveValues(float height) =>
        AcceptAsync(nameof(UiDataGrid<int>.VirtualRowHeight), height);

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public Task VirtualOverscan_AcceptsNonnegativeValues(int overscan) =>
        AcceptAsync(nameof(UiDataGrid<int>.VirtualOverscan), overscan);

    private static async Task<Exception> RejectAsync(string name, object value)
    {
        var grid = new UiDataGrid<int>();
        return await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await grid.SetParametersAsync(ParameterView.FromDictionary(Parameters(name, value))));
    }

    private static async Task AcceptAsync(string name, object value)
    {
        using var host = new TestHost();
        var node = await host.RenderAsync<UiDataGrid<int>>(Parameters(name, value));
        Assert.Single(node.WithClass("ui-grid--virtualized"));
    }

    private static Dictionary<string, object?> Parameters(string name, object value) => new()
    {
        [nameof(UiDataGrid<int>.VirtualProvider)] = new MockVirtualProvider(), [name] = value
    };

    private sealed class MockVirtualProvider : IDataGridVirtualDataProvider<int>
    {
        public Task<DataGridVirtualResponse<int>> GetVirtualDataAsync(DataGridVirtualRequest request) =>
            Task.FromResult(DataGridVirtualResponse<int>.Empty);
    }
}
