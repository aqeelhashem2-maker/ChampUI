using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.Tests.Export;

/// <summary>
/// Phase 3: Direct Export Method Tests
/// Tests the static CsvQuote method and validates CSV/XLSX structures.
/// Uses reflection to test actual production code without full component instantiation.
/// </summary>
public class DirectExportMethodTests
{
    private readonly ITestOutputHelper _output;

    public DirectExportMethodTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ═══════════════════════════════════════════════════════════════════
    // CSV QUOTE FORMULA INJECTION PROTECTION
    // ═══════════════════════════════════════════════════════════════════

    [Theory]
    [InlineData("=1+1", "\"'=1+1\"")]
    [InlineData("+cmd|calc", "\"'+cmd|calc\"")]
    [InlineData("-2+3", "\"'-2+3\"")]
    [InlineData("@SUM(A1)", "\"'@SUM(A1)\"")]
    [InlineData("\t=evil", "\"'\t=evil\"")]
    [InlineData("\r=bad", "\"'\r=bad\"")]
    [InlineData("   =spaced", "\"'   =spaced\"")]  // Leading whitespace trimmed, then formula detected
    public void CsvQuote_FormulaInjection_EscapedWithPrefix(string input, string expected)
    {
        // Act
        var result = CsvQuoteReflection(input);

        // Assert
        Assert.Equal(expected, result);
        _output.WriteLine($"✅ Formula Injection: '{input}' → {result}");
    }

    [Theory]
    [InlineData("Normal text", "\"Normal text\"")]
    [InlineData("Quote \"test\"", "\"Quote \"\"test\"\"\"")]  // Quotes doubled
    [InlineData("", "\"\"")]
    [InlineData("123", "\"123\"")]
    public void CsvQuote_NormalText_QuotedCorrectly(string input, string expected)
    {
        // Act
        var result = CsvQuoteReflection(input);

        // Assert
        Assert.Equal(expected, result);
        _output.WriteLine($"✅ Normal text: '{input}' → {result}");
    }

    [Fact]
    public void CsvQuote_WithComma_Quoted()
    {
        // Arrange
        var input = "Value, with comma";

        // Act
        var result = CsvQuoteReflection(input);

        // Assert
        Assert.StartsWith("\"", result);
        Assert.EndsWith("\"", result);
        Assert.Contains("with comma", result);
        _output.WriteLine($"✅ Comma handling: '{input}' → {result}");
    }

    [Fact]
    public void CsvQuote_WithNewline_Quoted()
    {
        // Arrange
        var input = "Line1\nLine2";

        // Act
        var result = CsvQuoteReflection(input);

        // Assert
        Assert.StartsWith("\"", result);
        Assert.EndsWith("\"", result);
        Assert.Contains("\n", result);
        _output.WriteLine($"✅ Newline handling: escaped properly");
    }

    // ═══════════════════════════════════════════════════════════════════
    // XLSX STRUCTURE VALIDATION
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void XLSX_MinimalPackage_ValidZipStructure()
    {
        // Arrange - Create minimal XLSX via reflection
        var sheetData = "<sheetData><row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>Test</t></is></c></row></sheetData>";
        var xlsx = WriteXlsxPackageReflection(sheetData);

        // Act - Write for inspection
        File.WriteAllBytes("/tmp/phase3_xlsx_minimal.xlsx", xlsx);

        // Assert - Valid ZIP
        Assert.True(xlsx.Length > 0);
        Assert.Equal(0x50, xlsx[0]); // 'P'
        Assert.Equal(0x4B, xlsx[1]); // 'K'

        // Verify ZIP entries
        using var ms = new MemoryStream(xlsx);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        Assert.NotNull(zip.GetEntry("[Content_Types].xml"));
        Assert.NotNull(zip.GetEntry("xl/workbook.xml"));
        Assert.NotNull(zip.GetEntry("xl/worksheets/sheet1.xml"));
        Assert.NotNull(zip.GetEntry("xl/_rels/workbook.xml.rels"));

        _output.WriteLine($"✅ XLSX ZIP Structure: {zip.Entries.Count} entries");
        _output.WriteLine($"File: /tmp/phase3_xlsx_minimal.xlsx");
    }

    [Fact]
    public void XLSX_Sheet_ContainsData()
    {
        // Arrange
        var sheetData = "<sheetData>" +
            "<row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>ID</t></is></c><c r=\"B1\" t=\"inlineStr\"><is><t>Name</t></is></c></row>" +
            "<row r=\"2\"><c r=\"A2\"><v>1</v></c><c r=\"B2\" t=\"inlineStr\"><is><t>Alice</t></is></c></row>" +
            "</sheetData>";
        var xlsx = WriteXlsxPackageReflection(sheetData);
        File.WriteAllBytes("/tmp/phase3_xlsx_data.xlsx", xlsx);

        // Act - Parse sheet
        using var ms = new MemoryStream(xlsx);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
        var sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml");
        using var stream = sheetEntry!.Open();
        var xml = new StreamReader(stream).ReadToEnd();

        // Assert
        Assert.Contains("ID", xml);
        Assert.Contains("Name", xml);
        Assert.Contains("Alice", xml);
        Assert.Contains("<v>1</v>", xml); // Numeric cell

        _output.WriteLine($"✅ XLSX Sheet Data: Headers and rows present");
        _output.WriteLine($"File: /tmp/phase3_xlsx_data.xlsx");
    }

    [Fact]
    public void XLSX_NumericCell_NoTypeAttribute()
    {
        // Arrange - Numeric cells omit t attribute
        var sheetData = "<sheetData>" +
            "<row r=\"1\"><c r=\"A1\"><v>42</v></c></row>" +  // Numeric - no t attribute
            "<row r=\"2\"><c r=\"A2\" t=\"inlineStr\"><is><t>Text</t></is></c></row>" +  // String - has t attribute
            "</sheetData>";
        var xlsx = WriteXlsxPackageReflection(sheetData);

        // Act - Parse
        using var ms = new MemoryStream(xlsx);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
        var sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml");
        using var stream = sheetEntry!.Open();
        var xml = new StreamReader(stream).ReadToEnd();
        var doc = XDocument.Parse(xml);

        // Assert - Find cells
        var ns = XNamespace.Get("http://schemas.openxmlformats.org/spreadsheetml/2006/main");
        var cells = doc.Descendants(ns + "c").ToList();

        var numericCell = cells.First(c => c.Attribute("r")?.Value == "A1");
        var stringCell = cells.First(c => c.Attribute("r")?.Value == "A2");

        // Numeric cell should NOT have t attribute
        Assert.Null(numericCell.Attribute("t"));
        Assert.Equal("42", numericCell.Element(ns + "v")?.Value);

        // String cell SHOULD have t="inlineStr"
        Assert.Equal("inlineStr", stringCell.Attribute("t")?.Value);

        _output.WriteLine($"✅ XLSX Numeric Cell: No t attribute, value=42");
        _output.WriteLine($"✅ XLSX String Cell: t=\"inlineStr\"");
    }

    [Fact]
    public void XLSX_ArabicText_PreservedInXML()
    {
        // Arrange
        var sheetData = "<sheetData>" +
            "<row r=\"1\"><c r=\"A1\" t=\"inlineStr\"><is><t>أحمد محمد</t></is></c></row>" +
            "</sheetData>";
        var xlsx = WriteXlsxPackageReflection(sheetData);
        File.WriteAllBytes("/tmp/phase3_xlsx_arabic_simple.xlsx", xlsx);

        // Act - Parse
        using var ms = new MemoryStream(xlsx);
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);
        var sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml");
        using var stream = sheetEntry!.Open();
        var xml = new StreamReader(stream).ReadToEnd();

        // Assert
        Assert.Contains("أحمد محمد", xml);

        _output.WriteLine($"✅ XLSX Arabic Unicode: أحمد محمد preserved in XML");
        _output.WriteLine($"File: /tmp/phase3_xlsx_arabic_simple.xlsx");
    }

    // ═══════════════════════════════════════════════════════════════════
    // CSV STRUCTURE VALIDATION
    // ═══════════════════════════════════════════════════════════════════

    [Fact]
    public void CSV_Structure_HeaderAndRows()
    {
        // Arrange
        var rows = new[] { "Alice", "Bob", "Charlie" };
        var csv = BuildSimpleCsv(rows);
        File.WriteAllText("/tmp/phase3_csv_structure.csv", csv, Encoding.UTF8);

        // Assert
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, lines.Length); // Header + 3 rows

        Assert.Contains("Name", lines[0]);
        Assert.Contains("Alice", csv);
        Assert.Contains("Bob", csv);
        Assert.Contains("Charlie", csv);

        _output.WriteLine($"✅ CSV Structure: {lines.Length} lines");
        _output.WriteLine($"File: /tmp/phase3_csv_structure.csv");
    }

    [Fact]
    public void CSV_UTF8_WithBOM()
    {
        // Arrange
        var rows = new[] { "أحمد", "فاطمة" };
        var csv = BuildSimpleCsv(rows);

        // Act - Write with UTF-8 BOM
        var utf8WithBom = new UTF8Encoding(true);
        File.WriteAllText("/tmp/phase3_csv_utf8_bom.csv", csv, utf8WithBom);

        // Assert - Verify BOM
        var bytes = File.ReadAllBytes("/tmp/phase3_csv_utf8_bom.csv");
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]);

        // Verify Arabic text
        Assert.Contains("أحمد", csv);
        Assert.Contains("فاطمة", csv);

        _output.WriteLine($"✅ CSV UTF-8 BOM: Present (EF BB BF)");
        _output.WriteLine($"✅ Arabic text: أحمد, فاطمة");
        _output.WriteLine($"File: /tmp/phase3_csv_utf8_bom.csv");
    }

    // ═══════════════════════════════════════════════════════════════════
    // HELPER METHODS - REFLECTION ACCESS TO PRODUCTION CODE
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calls the static CsvQuote method via reflection.
    /// This tests the ACTUAL production code, not a test duplicate.
    /// </summary>
    private static string CsvQuoteReflection(string value)
    {
        var gridType = typeof(ChampUI.Components.Data.UiDataGrid<object>);
        var method = gridType.GetMethod("CsvQuote",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
            throw new InvalidOperationException("CsvQuote method not found");

        var result = method.Invoke(null, new object[] { value });
        return (string)result!;
    }

    /// <summary>
    /// Calls the static WriteXlsxPackage method via reflection.
    /// This tests the ACTUAL production code.
    /// </summary>
    private static byte[] WriteXlsxPackageReflection(string sheetDataXml)
    {
        var gridType = typeof(ChampUI.Components.Data.UiDataGrid<object>);
        var method = gridType.GetMethod("WriteXlsxPackage",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
            throw new InvalidOperationException("WriteXlsxPackage method not found");

        var result = method.Invoke(null, new object[] { sheetDataXml });
        return (byte[])result!;
    }

    /// <summary>
    /// Builds a simple CSV using the production CsvQuote method.
    /// </summary>
    private static string BuildSimpleCsv(string[] names)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{CsvQuoteReflection("Name")}");

        foreach (var name in names)
        {
            sb.AppendLine($"{CsvQuoteReflection(name)}");
        }

        return sb.ToString();
    }
}
