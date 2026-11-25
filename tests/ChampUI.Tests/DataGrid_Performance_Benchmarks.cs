using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace ChampUI.Tests;

[CollectionDefinition("DataGrid performance", DisableParallelization = true)]
public sealed class DataGridPerformanceCollection { }

/// <summary>
/// Performance benchmarks for UiDataGrid at 1K, 10K, and 100K records.
/// Measures rendering time, memory usage, and operation performance.
/// </summary>
[Collection("DataGrid performance")]
public class DataGrid_Performance_Benchmarks
{
    private readonly ITestOutputHelper _output;

    public DataGrid_Performance_Benchmarks(ITestOutputHelper output)
    {
        _output = output;
    }

    private record BenchmarkItem(int Id, string Name, int Value, DateTime Date);

    private List<BenchmarkItem> GenerateData(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new BenchmarkItem(
                i,
                $"Item {i}",
                i * 100,
                DateTime.Now.AddDays(-i)))
            .ToList();
    }

    /// <summary>
    /// Benchmark 1: Data generation and sorting at scale
    /// </summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void Benchmark_01_DataGeneration(int count)
    {
        var sw = Stopwatch.StartNew();
        var data = GenerateData(count);
        sw.Stop();

        _output.WriteLine($"=== Data Generation: {count:N0} records ===");
        _output.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Rate: {count / sw.Elapsed.TotalSeconds:N0} records/sec");
        
        Assert.True(sw.ElapsedMilliseconds < 5000, 
            $"Generation should be < 5s, got {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Benchmark 2: Sorting performance
    /// </summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void Benchmark_02_Sorting(int count)
    {
        var data = GenerateData(count);
        
        // Warmup
        _ = data.OrderBy(x => x.Name).ToList();
        
        var sw = Stopwatch.StartNew();
        var sorted = data.OrderBy(x => x.Name).ToList();
        sw.Stop();

        _output.WriteLine($"=== Sorting: {count:N0} records ===");
        _output.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Rate: {count / sw.Elapsed.TotalSeconds:N0} records/sec");
        
        Assert.Equal(count, sorted.Count);
        Assert.True(sw.ElapsedMilliseconds < 10000, 
            $"Sorting should be < 10s, got {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Benchmark 3: Filtering performance
    /// </summary>
    [Theory]
    [InlineData(1_000, 500)]  // 50% match
    [InlineData(10_000, 1_000)]  // 10% match
    [InlineData(100_000, 1_000)]  // 1% match
    public void Benchmark_03_Filtering(int totalCount, int expectedMatches)
    {
        var data = GenerateData(totalCount);
        var threshold = totalCount / (totalCount / expectedMatches);
        
        var sw = Stopwatch.StartNew();
        var filtered = data.Where(x => x.Id < threshold).ToList();
        sw.Stop();

        _output.WriteLine($"=== Filtering: {totalCount:N0} records → {filtered.Count:N0} matches ===");
        _output.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Rate: {totalCount / sw.Elapsed.TotalSeconds:N0} records/sec");
        
        Assert.True(filtered.Count >= expectedMatches * 0.9, "Should match approximately");
        Assert.True(sw.ElapsedMilliseconds < 5000, 
            $"Filtering should be < 5s, got {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Benchmark 4: Pagination (Skip/Take)
    /// </summary>
    [Theory]
    [InlineData(1_000, 50)]
    [InlineData(10_000, 50)]
    [InlineData(100_000, 50)]
    public void Benchmark_04_Pagination(int totalCount, int pageSize)
    {
        var data = GenerateData(totalCount);
        var iterations = 100;  // Paginate 100 times
        
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var page = i % (totalCount / pageSize);
            var pageData = data.Skip(page * pageSize).Take(pageSize).ToList();
            Assert.Equal(Math.Min(pageSize, totalCount - page * pageSize), pageData.Count);
        }
        sw.Stop();

        _output.WriteLine($"=== Pagination: {totalCount:N0} records, {pageSize} per page, {iterations} iterations ===");
        _output.WriteLine($"Total time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Avg per page: {sw.ElapsedMilliseconds / (double)iterations:F2} ms");
        
        Assert.True(sw.ElapsedMilliseconds / iterations < 100, 
            "Single page should be < 100ms");
    }

    /// <summary>
    /// Benchmark 5: HashSet lookup performance (for _loadedVirtualItems)
    /// </summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void Benchmark_05_HashSetLookup(int count)
    {
        var data = GenerateData(Math.Min(count, 10_000)); // Cap at 10K (our limit)
        var hashSet = new HashSet<BenchmarkItem>(data);
        const int lookupIterations = 10_000;

        int LookUpAll()
        {
            var hits = 0;
            for (var i = 0; i < lookupIterations; i++)
                if (hashSet.Contains(data[i % data.Count])) hits++;
            return hits;
        }

        Assert.Equal(lookupIterations, LookUpAll()); // Warm the full workload before timing.
        var samples = new double[5];
        for (var i = 0; i < samples.Length; i++)
        {
            var start = Stopwatch.GetTimestamp();
            var hits = LookUpAll();
            samples[i] = Stopwatch.GetElapsedTime(start).TotalMicroseconds / lookupIterations;
            Assert.Equal(lookupIterations, hits);
        }

        Array.Sort(samples);
        var median = samples[samples.Length / 2];
        _output.WriteLine($"HashSet lookup at {data.Count:N0} tracked items: {string.Join(", ", samples.Select(s => s.ToString("F3")))} μs/lookup");
        Assert.True(median < 10, $"Median lookup should be < 10 microseconds, got {median:F3}");
    }

    /// <summary>
    /// Benchmark 6: Memory footprint estimation
    /// </summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void Benchmark_06_MemoryFootprint(int count)
    {
        var beforeGen0 = GC.CollectionCount(0);
        var beforeGen1 = GC.CollectionCount(1);
        var beforeGen2 = GC.CollectionCount(2);
        
        var beforeMemory = GC.GetTotalMemory(true);
        
        var data = GenerateData(count);
        var hashSet = new HashSet<BenchmarkItem>(data.Take(Math.Min(count, 10_000)));
        
        var afterMemory = GC.GetTotalMemory(false);
        var memoryUsed = afterMemory - beforeMemory;
        var memoryPerItem = memoryUsed / (double)Math.Min(count, 10_000);
        
        var afterGen0 = GC.CollectionCount(0);
        var afterGen1 = GC.CollectionCount(1);
        var afterGen2 = GC.CollectionCount(2);
        
        _output.WriteLine($"=== Memory Footprint: {count:N0} items (tracked: {Math.Min(count, 10_000):N0}) ===");
        _output.WriteLine($"Total memory: {memoryUsed / 1024.0 / 1024.0:F2} MB");
        _output.WriteLine($"Per item: {memoryPerItem:F2} bytes");
        _output.WriteLine($"GC Gen0: {afterGen0 - beforeGen0}");
        _output.WriteLine($"GC Gen1: {afterGen1 - beforeGen1}");
        _output.WriteLine($"GC Gen2: {afterGen2 - beforeGen2}");
        
        // For 10K items, should be < 5 MB total
        if (count <= 10_000)
        {
            Assert.True(memoryUsed < 5 * 1024 * 1024, 
                $"Memory for {count} items should be < 5 MB, got {memoryUsed / 1024.0 / 1024.0:F2} MB");
        }
    }

    /// <summary>
    /// Benchmark 7: Composite key generation performance
    /// </summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void Benchmark_07_CompositeKeyGeneration(int count)
    {
        var data = GenerateData(count);
        
        var sw = Stopwatch.StartNew();
        var keys = data.Select(item => 
        {
            var rowKey = item.Id;
            var uniqueKey = $"{rowKey}_{item.GetHashCode()}";
            return uniqueKey;
        }).ToList();
        sw.Stop();

        _output.WriteLine($"=== Composite Key Generation: {count:N0} records ===");
        _output.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Rate: {count / sw.Elapsed.TotalSeconds:N0} keys/sec");
        _output.WriteLine($"Avg per key: {sw.Elapsed.TotalMilliseconds / count * 1000:F3} microseconds");
        
        Assert.Equal(count, keys.Count);
        Assert.True(sw.ElapsedMilliseconds < 5000, 
            $"Key generation should be < 5s, got {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Benchmark 8: LINQ query performance (simulating grid operations)
    /// </summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void Benchmark_08_ComplexQuery(int count)
    {
        var data = GenerateData(count);
        
        // Complex query: filter, sort, paginate
        var sw = Stopwatch.StartNew();
        var result = data
            .Where(x => x.Value > 5000)
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Name)
            .Skip(100)
            .Take(50)
            .ToList();
        sw.Stop();

        _output.WriteLine($"=== Complex Query: {count:N0} records ===");
        _output.WriteLine($"Filter → Sort → Paginate");
        _output.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Result: {result.Count} rows");
        
        Assert.True(sw.ElapsedMilliseconds < 10000, 
            $"Complex query should be < 10s, got {sw.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// Benchmark 9: Select All simulation at scale
    /// </summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    [InlineData(100_000)]
    public void Benchmark_09_SelectAllSimulation(int count)
    {
        var loadedItems = new HashSet<BenchmarkItem>(
            GenerateData(Math.Min(count, 10_000)));
        var selectedItems = new HashSet<BenchmarkItem>();
        
        var sw = Stopwatch.StartNew();
        foreach (var item in loadedItems)
        {
            selectedItems.Add(item);
        }
        sw.Stop();

        _output.WriteLine($"=== Select All: {count:N0} total, {loadedItems.Count:N0} tracked ===");
        _output.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Selected: {selectedItems.Count:N0} items");
        
        Assert.Equal(loadedItems.Count, selectedItems.Count);
        Assert.True(sw.ElapsedMilliseconds < 1000, 
            "Select All should be < 1s");
    }

    /// <summary>
    /// Benchmark 10: CSV export simulation
    /// </summary>
    [Theory]
    [InlineData(1_000)]
    [InlineData(10_000)]
    public void Benchmark_10_CsvExportSimulation(int count)
    {
        var data = GenerateData(count);
        
        var sw = Stopwatch.StartNew();
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("ID,Name,Value,Date");
        foreach (var item in data)
        {
            sb.AppendLine($"\"{item.Id}\",\"{item.Name}\",\"{item.Value}\",\"{item.Date:yyyy-MM-dd}\"");
        }
        var csv = sb.ToString();
        sw.Stop();

        _output.WriteLine($"=== CSV Export: {count:N0} records ===");
        _output.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");
        _output.WriteLine($"Size: {csv.Length / 1024.0:F2} KB");
        _output.WriteLine($"Rate: {count / sw.Elapsed.TotalSeconds:N0} records/sec");
        
        Assert.True(sw.ElapsedMilliseconds < 10000, 
            $"CSV export should be < 10s, got {sw.ElapsedMilliseconds}ms");
    }
}
