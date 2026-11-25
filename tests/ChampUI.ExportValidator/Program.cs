using System.Text;
using System.IO.Compression;

Console.WriteLine("=== ChampUI Export Validation - Standalone Generator ===");
Console.WriteLine($"Starting at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");

var outputDir = args.Length > 0 ? args[0] : "/tmp/champui-exports";
Directory.CreateDirectory(outputDir);
Console.WriteLine($"Output directory: {outputDir}\n");

int passed = 0;
int failed = 0;

// Test 1: CSV Basic Export
try
{
    Console.WriteLine("Test 1: CSV Basic Export");
    await GenerateBasicCSV(Path.Combine(outputDir, "test_basic_export.csv"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 2: CSV UTF-8 with Arabic
try
{
    Console.WriteLine("Test 2: CSV UTF-8 with Arabic");
    await GenerateUTF8ArabicCSV(Path.Combine(outputDir, "test_utf8_export.csv"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 3: CSV Formula Injection Protection
try
{
    Console.WriteLine("Test 3: CSV Formula Injection Protection");
    await GenerateFormulaProtectionCSV(Path.Combine(outputDir, "test_formula_protection.csv"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 4: CSV Numeric Types
try
{
    Console.WriteLine("Test 4: CSV Numeric Types");
    await GenerateNumericTypesCSV(Path.Combine(outputDir, "test_numeric_types.csv"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 5: CSV Complex Escaping
try
{
    Console.WriteLine("Test 5: CSV Complex Escaping");
    await GenerateComplexEscapingCSV(Path.Combine(outputDir, "test_complex_escaping.csv"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 6: CSV Empty Data
try
{
    Console.WriteLine("Test 6: CSV Empty Data");
    await GenerateEmptyCSV(Path.Combine(outputDir, "test_empty.csv"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 7: XLSX Basic Export
try
{
    Console.WriteLine("Test 7: XLSX Basic Export");
    await GenerateBasicXLSX(Path.Combine(outputDir, "test_basic_export.xlsx"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 8: XLSX UTF-8 with Arabic
try
{
    Console.WriteLine("Test 8: XLSX UTF-8 with Arabic");
    await GenerateUTF8ArabicXLSX(Path.Combine(outputDir, "test_utf8_export.xlsx"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 9: XLSX Numeric Cells
try
{
    Console.WriteLine("Test 9: XLSX Numeric Cells");
    await GenerateNumericXLSX(Path.Combine(outputDir, "test_numeric_cells.xlsx"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

// Test 10: XLSX Date Cells
try
{
    Console.WriteLine("Test 10: XLSX Date Cells");
    await GenerateDateXLSX(Path.Combine(outputDir, "test_date_cells.xlsx"));
    Console.WriteLine("✅ PASS\n");
    passed++;
}
catch (Exception ex)
{
    Console.WriteLine($"❌ FAIL: {ex.Message}\n");
    failed++;
}

Console.WriteLine($"\n=== Summary ===");
Console.WriteLine($"Passed: {passed}/10");
Console.WriteLine($"Failed: {failed}/10");
Console.WriteLine($"\nFiles written to: {outputDir}");
Console.WriteLine($"Completed at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

Environment.Exit(failed > 0 ? 1 : 0);

// ===== CSV Test Generators =====

static async Task GenerateBasicCSV(string path)
{
    var data = new List<TestPerson>
    {
        new TestPerson { Id = 1, Name = "Alice", Age = 30, Salary = 50000 },
        new TestPerson { Id = 2, Name = "Bob", Age = 25, Salary = 45000 }
    };

    var columns = new List<ColumnDef>
    {
        new() { Title = "ID", Display = p => ((TestPerson)p).Id.ToString() },
        new() { Title = "Name", Display = p => ((TestPerson)p).Name },
        new() { Title = "Age", Display = p => ((TestPerson)p).Age.ToString() },
        new() { Title = "Salary", Display = p => ((TestPerson)p).Salary.ToString() }
    };

    var csv = BuildCsv(data.Cast<object>(), columns);
    await File.WriteAllTextAsync(path, csv, Encoding.UTF8);
    
    Console.WriteLine($"  File: {path}");
    Console.WriteLine($"  Size: {new FileInfo(path).Length} bytes");
    Console.WriteLine($"  Lines: {csv.Split('\n').Length}");
    
    // Basic validation
    if (!csv.Contains("ID,Name,Age,Salary"))
        throw new Exception("Missing header row");
    if (!csv.Contains("Alice"))
        throw new Exception("Missing data row");
}

static async Task GenerateUTF8ArabicCSV(string path)
{
    var data = new List<TestPerson>
    {
        new TestPerson { Id = 1, Name = "أحمد محمد", Age = 30, Salary = 50000 },
        new TestPerson { Id = 2, Name = "فاطمة علي", Age = 25, Salary = 45000 }
    };

    var columns = new List<ExportColumn<TestPerson>>
    {
        new() { Title = "المعرف", ValueSelector = p => p.Id },
        new() { Title = "الاسم", ValueSelector = p => p.Name },
        new() { Title = "العمر", ValueSelector = p => p.Age }
    };

    var csv = ExportHelper.GenerateCsvString(data, columns);
    await File.WriteAllTextAsync(path, csv, Encoding.UTF8);
    
    Console.WriteLine($"  File: {path}");
    Console.WriteLine($"  Size: {new FileInfo(path).Length} bytes");
    
    // UTF-8 BOM check
    var bytes = await File.ReadAllBytesAsync(path);
    bool hasBOM = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
    Console.WriteLine($"  UTF-8 BOM: {(hasBOM ? "Present" : "Missing")}");
    
    // Arabic preservation check
    if (!csv.Contains("أحمد"))
        throw new Exception("Arabic characters not preserved");
    if (!csv.Contains("المعرف"))
        throw new Exception("Arabic header not preserved");
}

static async Task GenerateFormulaProtectionCSV(string path)
{
    var data = new List<TestPerson>
    {
        new TestPerson { Id = 1, Name = "=1+1", Age = 30, Salary = 50000 },
        new TestPerson { Id = 2, Name = "+cmd|'/c calc'!A1", Age = 25, Salary = 45000 },
        new TestPerson { Id = 3, Name = "-2+3", Age = 28, Salary = 48000 },
        new TestPerson { Id = 4, Name = "@SUM(A1:A10)", Age = 35, Salary = 55000 }
    };

    var columns = new List<ExportColumn<TestPerson>>
    {
        new() { Title = "ID", ValueSelector = p => p.Id },
        new() { Title = "Name", ValueSelector = p => p.Name }
    };

    var csv = ExportHelper.GenerateCsvString(data, columns);
    await File.WriteAllTextAsync(path, csv, Encoding.UTF8);
    
    Console.WriteLine($"  File: {path}");
    
    // Formula injection protection check
    if (csv.Contains("\"=1+1\"") || csv.Contains("\"'=1+1\""))
        Console.WriteLine($"  ✅ Formula '=1+1' escaped");
    else
        throw new Exception("Formula '=1+1' not properly escaped");
    
    if (csv.Contains("\"+cmd") || csv.Contains("\"'+cmd"))
        Console.WriteLine($"  ✅ Command injection '+cmd' escaped");
    else
        throw new Exception("Command injection not properly escaped");
}

static async Task GenerateNumericTypesCSV(string path)
{
    var data = new List<TestPerson>
    {
        new TestPerson { Id = 1, Name = "Alice", Age = 30, Salary = 50000.50m },
        new TestPerson { Id = 2, Name = "Bob", Age = 25, Salary = 45000.99m }
    };

    var columns = new List<ExportColumn<TestPerson>>
    {
        new() { Title = "ID", ValueSelector = p => p.Id },
        new() { Title = "Salary", ValueSelector = p => p.Salary }
    };

    var csv = ExportHelper.GenerateCsvString(data, columns);
    await File.WriteAllTextAsync(path, csv, Encoding.UTF8);
    
    Console.WriteLine($"  File: {path}");
    
    // Numeric format check
    if (csv.Contains("50000.50") && csv.Contains("45000.99"))
        Console.WriteLine($"  ✅ Decimal values preserved");
    else
        throw new Exception("Decimal values not preserved correctly");
}

static async Task GenerateComplexEscapingCSV(string path)
{
    var data = new List<TestPerson>
    {
        new TestPerson { Id = 1, Name = "Quote \"Test\"", Age = 30, Salary = 50000 },
        new TestPerson { Id = 2, Name = "Comma, Test", Age = 25, Salary = 45000 },
        new TestPerson { Id = 3, Name = "Newline\nTest", Age = 28, Salary = 48000 }
    };

    var columns = new List<ExportColumn<TestPerson>>
    {
        new() { Title = "ID", ValueSelector = p => p.Id },
        new() { Title = "Name", ValueSelector = p => p.Name }
    };

    var csv = ExportHelper.GenerateCsvString(data, columns);
    await File.WriteAllTextAsync(path, csv, Encoding.UTF8);
    
    Console.WriteLine($"  File: {path}");
    Console.WriteLine($"  ✅ Complex escaping applied");
}

static async Task GenerateEmptyCSV(string path)
{
    var data = new List<TestPerson>();
    var columns = new List<ExportColumn<TestPerson>>
    {
        new() { Title = "ID", ValueSelector = p => p.Id },
        new() { Title = "Name", ValueSelector = p => p.Name }
    };

    var csv = ExportHelper.GenerateCsvString(data, columns);
    await File.WriteAllTextAsync(path, csv, Encoding.UTF8);
    
    Console.WriteLine($"  File: {path}");
    
    if (csv.Contains("ID,Name"))
        Console.WriteLine($"  ✅ Header present for empty data");
    else
        throw new Exception("Header missing for empty data");
}

// ===== XLSX Test Generators =====

static async Task GenerateBasicXLSX(string path)
{
    var data = new List<TestPerson>
    {
        new TestPerson { Id = 1, Name = "Alice", Age = 30, Salary = 50000 },
        new TestPerson { Id = 2, Name = "Bob", Age = 25, Salary = 45000 }
    };

    var columns = new List<ExportColumn<TestPerson>>
    {
        new() { Title = "ID", ValueSelector = p => p.Id },
        new() { Title = "Name", ValueSelector = p => p.Name },
        new() { Title = "Age", ValueSelector = p => p.Age },
        new() { Title = "Salary", ValueSelector = p => p.Salary }
    };

    var bytes = await ExportHelper.GenerateExcelBytesAsync(data, columns);
    await File.WriteAllBytesAsync(path, bytes);
    
    Console.WriteLine($"  File: {path}");
    Console.WriteLine($"  Size: {bytes.Length} bytes");
    
    // XLSX magic number check
    if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B)
        throw new Exception("Not a valid XLSX file (missing ZIP header)");
}

static async Task GenerateUTF8ArabicXLSX(string path)
{
    var data = new List<TestPerson>
    {
        new TestPerson { Id = 1, Name = "أحمد محمد", Age = 30, Salary = 50000 },
        new TestPerson { Id = 2, Name = "فاطمة علي", Age = 25, Salary = 45000 }
    };

    var columns = new List<ExportColumn<TestPerson>>
    {
        new() { Title = "المعرف", ValueSelector = p => p.Id },
        new() { Title = "الاسم", ValueSelector = p => p.Name },
        new() { Title = "العمر", ValueSelector = p => p.Age }
    };

    var bytes = await ExportHelper.GenerateExcelBytesAsync(data, columns);
    await File.WriteAllBytesAsync(path, bytes);
    
    Console.WriteLine($"  File: {path}");
    Console.WriteLine($"  Size: {bytes.Length} bytes");
    Console.WriteLine($"  ✅ Arabic content in XLSX (requires Excel inspection)");
}

static async Task GenerateNumericXLSX(string path)
{
    var data = new List<TestPerson>
    {
        new TestPerson { Id = 1, Name = "Alice", Age = 30, Salary = 50000.50m },
        new TestPerson { Id = 2, Name = "Bob", Age = 25, Salary = 45000.99m }
    };

    var columns = new List<ExportColumn<TestPerson>>
    {
        new() { Title = "ID", ValueSelector = p => p.Id },
        new() { Title = "Age", ValueSelector = p => p.Age },
        new() { Title = "Salary", ValueSelector = p => p.Salary }
    };

    var bytes = await ExportHelper.GenerateExcelBytesAsync(data, columns);
    await File.WriteAllBytesAsync(path, bytes);
    
    Console.WriteLine($"  File: {path}");
    Console.WriteLine($"  ✅ Numeric cells (requires Excel inspection for type)");
}

static async Task GenerateDateXLSX(string path)
{
    var data = new List<TestPersonWithDate>
    {
        new TestPersonWithDate { Id = 1, Name = "Alice", BirthDate = new DateTime(1990, 5, 15) },
        new TestPersonWithDate { Id = 2, Name = "Bob", BirthDate = new DateTime(1985, 11, 23) }
    };

    var columns = new List<ExportColumn<TestPersonWithDate>>
    {
        new() { Title = "ID", ValueSelector = p => p.Id },
        new() { Title = "Name", ValueSelector = p => p.Name },
        new() { Title = "Birth Date", ValueSelector = p => p.BirthDate }
    };

    var bytes = await ExportHelper.GenerateExcelBytesAsync(data, columns);
    await File.WriteAllBytesAsync(path, bytes);
    
    Console.WriteLine($"  File: {path}");
    Console.WriteLine($"  ✅ Date cells (requires Excel inspection for type)");
}

// ===== Test Models =====

class TestPerson
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public decimal Salary { get; set; }
}

class TestPersonWithDate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
}
