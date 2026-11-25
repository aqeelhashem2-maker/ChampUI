using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Playwright;
using Microsoft.VisualBasic.FileIO;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace ChampUI.BrowserTests;

[Collection("PlaywrightCollection")]
public class EndToEndExportTests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;
    private string _scenario = "default";
    private readonly string _temp = Path.Combine(Path.GetTempPath(), $"champui-downloads-{Guid.NewGuid():N}");

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_temp);
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        _context = await _browser.NewContextAsync(new() { AcceptDownloads = true });
        await _context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        var evidence = Environment.GetEnvironmentVariable("CHAMPUI_EVIDENCE_DIR");
        if (evidence != null)
        {
            Directory.CreateDirectory(evidence);
            await _context.Tracing.StopAsync(new() { Path = Path.Combine(evidence, $"export-{_scenario}.zip") });
            await _page.ScreenshotAsync(new() { Path = Path.Combine(evidence, $"export-{_scenario}.png") });
        }
        await _browser.DisposeAsync();
        _playwright.Dispose();
        Directory.Delete(_temp, recursive: true);
    }

    private async Task OpenAsync(string? scenario = null)
    {
        _scenario = scenario ?? "scroll";
        await _page.GotoAsync("http://localhost:5000/virtual-data-test" +
            (scenario == null ? "" : $"?scenario={scenario}"));
        await Expect(_page.GetByTestId("status-interactive")).ToHaveAttributeAsync("data-ready", "true");
        await Expect(_page.Locator("tr[data-row-key='1']")).ToBeVisibleAsync();
        if (scenario != "legacy")
            await Expect(_page.GetByTestId("status-request-count")).ToHaveTextAsync(new Regex(@"Request Count: [1-9]\d*"));
    }

    private async Task OpenExportAsync() =>
        await _page.GetByTestId("export-toolbar").GetByRole(AriaRole.Button, new() { Name = "Export" }).ClickAsync();

    private ILocator Action(string label) =>
        _page.GetByRole(AriaRole.Menuitem, new() { Name = label, Exact = true });

    private async Task<string> DownloadAsync(string label, string fileName)
    {
        await OpenExportAsync();
        var download = await _page.RunAndWaitForDownloadAsync(() => Action(label).ClickAsync());
        Assert.Null(await download.FailureAsync());
        var path = Path.Combine(_temp, fileName);
        await download.SaveAsAsync(path);
        if (Environment.GetEnvironmentVariable("CHAMPUI_EVIDENCE_DIR") is { } evidence)
            File.Copy(path, Path.Combine(evidence, fileName), overwrite: true);
        return path;
    }

    private static IEnumerable<string[]> CsvData(string path)
    {
        using var csv = new TextFieldParser(path) { TrimWhiteSpace = false };
        csv.SetDelimiters(",");
        Assert.Equal("ID", csv.ReadFields()![0]);
        while (!csv.EndOfData)
            yield return csv.ReadFields()!;
    }

    private static IEnumerable<int> CsvIds(string path) => CsvData(path).Select(row => int.Parse(row[0]));

    [Fact]
    public async Task E2E_LegacyCurrentPage_DownloadsExactIdsAndProtectsCsv()
    {
        await OpenAsync("legacy");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Next page" }).ClickAsync();
        await Expect(_page.Locator("tr[data-row-key='11']")).ToBeVisibleAsync();

        var csv = await DownloadAsync("CSV — current page", "legacy-page-2.csv");
        Assert.Equal(Enumerable.Range(11, 10), CsvIds(csv));
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, File.ReadAllBytes(csv)[..3]);
        Assert.Equal("'=1+1", CsvData(csv).First()[1]);

        var xlsx = await DownloadAsync("Excel — current page", "legacy-page-2.xlsx");
        var rows = ReadSheet(xlsx);
        Assert.Equal(Enumerable.Range(11, 10), rows.Select(row => int.Parse(CellValue(row[0]))));
        Assert.Equal("inlineStr", rows[0][1].Attribute("t")?.Value);
        Assert.Null(rows[0][3].Attribute("t"));
        Assert.Equal("2", rows[0][4].Attribute("s")?.Value);
        Assert.Equal(new DateTime(2026, 1, 11).ToOADate(), double.Parse(CellValue(rows[0][4]),
            System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task E2E_VirtualExport_ExactRecordsAcrossScrollRanges()
    {
        await OpenAsync();
        var viewport = _page.GetByTestId("virtualize-wrapper");
        foreach (var (offset, visibleId) in new[] { (2400, 56), (4800, 106) })
        {
            await viewport.EvaluateAsync("el => el.scrollTop = " + offset);
            await Expect(_page.Locator($"tr[data-row-key='{visibleId}']")).ToBeVisibleAsync();
        }

        await OpenExportAsync();
        await Expect(Action("CSV — current page")).ToBeDisabledAsync();
        await Expect(Action("Excel — current page")).ToBeDisabledAsync();
        await _page.GetByTestId("export-toolbar").GetByRole(AriaRole.Button, new() { Name = "Export" }).ClickAsync();

        var csv = await DownloadAsync("CSV — all rows", "virtual-after-scroll.csv");
        Assert.Equal(Enumerable.Range(1, 1000), CsvIds(csv));
    }

    [Fact]
    public async Task E2E_VirtualExport_UsesNewQueryWhileRangeRefreshIsPending()
    {
        await OpenAsync("refresh");
        await _page.GetByTestId("virtual-grid").GetByRole(AriaRole.Searchbox).FillAsync("Gamma");
        await Expect(_page.GetByTestId("status-search")).ToHaveTextAsync("Search: Gamma");
        await Expect(_page.GetByTestId("status-pending")).ToHaveTextAsync("Pending: yes");

        var expected = Enumerable.Range(1, 1000).Where(id => id % 3 == 2).ToArray();
        var csv = await DownloadAsync("CSV — all rows", "virtual-during-refresh.csv");
        Assert.Equal(expected, CsvIds(csv));
        await Expect(_page.GetByTestId("status-pending")).ToHaveTextAsync("Pending: yes");
        var xlsx = await DownloadAsync("Excel — all rows", "virtual-during-refresh.xlsx");
        Assert.Equal(expected, ReadSheet(xlsx).Select(row => int.Parse(CellValue(row[0]))));
        if (Environment.GetEnvironmentVariable("CHAMPUI_EVIDENCE_DIR") is { } evidence)
            await _page.ScreenshotAsync(new() { Path = Path.Combine(evidence, "export-while-range-pending.png"), FullPage = true });

        await _page.GetByTestId("release-refresh").ClickAsync();
        await Expect(_page.GetByTestId("status-pending")).ToHaveTextAsync("Pending: no");
        await Expect(_page.Locator("tr[data-row-key='2']")).ToBeVisibleAsync();
        await Expect(_page.Locator("tr[data-row-key='1']")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task E2E_Export_UnsupportedScope_ShowsError()
    {
        await OpenAsync("unsupported");
        var downloads = 0;
        _page.Download += (_, _) => downloads++;
        await OpenExportAsync();
        await Action("CSV — all rows").ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Alert))
            .ToHaveTextAsync("Full export requires ServerExportAll or GetAllForExportAsync.");
        await Expect(_page.Locator("tr[data-row-key='1']")).ToBeVisibleAsync();
        await OpenExportAsync();
        await Action("Excel — all rows").ClickAsync();
        await Expect(_page.GetByRole(AriaRole.Alert))
            .ToHaveTextAsync("Full export requires ServerExportAll or GetAllForExportAsync.");
        Assert.Equal(0, downloads);
    }

    private static XElement[][] ReadSheet(string path)
    {
        using var zip = ZipFile.OpenRead(path);
        using var stream = zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();
        var doc = XDocument.Load(stream);
        var ns = doc.Root!.Name.Namespace;
        return doc.Descendants(ns + "row").Skip(1).Select(row => row.Elements(ns + "c").ToArray()).ToArray();
    }

    private static string CellValue(XElement cell) => cell.Value;
}
