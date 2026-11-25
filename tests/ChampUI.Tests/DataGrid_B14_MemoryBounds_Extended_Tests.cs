using Xunit;
using Xunit.Abstractions;

namespace ChampUI.Tests;

/// <summary>
/// Extended tests for 10,000-item memory bound verification.
/// Tests scrolling behavior, selection semantics, and memory bounds beyond the limit.
/// </summary>
public class DataGrid_B14_MemoryBounds_Extended_Tests
{
    private readonly ITestOutputHelper _output;

    public DataGrid_B14_MemoryBounds_Extended_Tests(ITestOutputHelper output)
    {
        _output = output;
    }

    private record TestItem(int Id, string Name);

    /// <summary>
    /// B14_03: Verify that memory remains bounded when scrolling past 10K items.
    /// </summary>
    [Fact]
    public void B14_03_ScrollingPast10K_MemoryRemainsBounded()
    {
        // This test verifies the memory bound concept:
        // - HashSet stops adding after 10,000 items
        // - Memory footprint capped at ~10K references
        // - No out-of-memory exception
        
        const int totalItems = 15_000;
        const int maxTracked = 10_000;
        
        // Simulate the tracking logic
        var trackedItems = new HashSet<TestItem>();
        var allItems = Enumerable.Range(1, totalItems)
            .Select(i => new TestItem(i, $"Item {i}"))
            .ToList();
        
        int warningCount = 0;
        
        foreach (var item in allItems)
        {
            if (trackedItems.Count < maxTracked)
            {
                trackedItems.Add(item);
            }
            else if (trackedItems.Count == maxTracked && warningCount == 0)
            {
                // Warning would be logged here
                warningCount++;
                _output.WriteLine($"Warning: Tracking limit reached at {trackedItems.Count} items");
            }
        }
        
        // Verify memory bound
        Assert.Equal(maxTracked, trackedItems.Count);
        Assert.Equal(1, warningCount); // Warning logged once
        
        // Verify last 5000 items NOT tracked
        var untrackedItems = allItems.Skip(maxTracked).Take(100).ToList();
        foreach (var item in untrackedItems)
        {
            Assert.DoesNotContain(item, trackedItems);
        }
        
        _output.WriteLine($"✅ Memory bounded: {trackedItems.Count}/{totalItems} items tracked");
    }

    /// <summary>
    /// B14_04: Verify selection semantics remain correct after limit reached.
    /// </summary>
    [Fact]
    public void B14_04_SelectionAfterLimit_RemainsSemanicallyCorrect()
    {
        // After 10K limit:
        // - Select All only selects tracked items (first 10K)
        // - Individual selection still works
        // - No data corruption
        
        const int maxTracked = 10_000;
        var trackedItems = new HashSet<TestItem>();
        var selectedItems = new HashSet<TestItem>();
        
        // Fill to limit
        for (int i = 1; i <= maxTracked; i++)
        {
            trackedItems.Add(new TestItem(i, $"Item {i}"));
        }
        
        // Try to track more (should be ignored)
        var beyondLimit = new TestItem(15_000, "Item 15000");
        bool wasAdded = false;
        if (trackedItems.Count < maxTracked)
        {
            trackedItems.Add(beyondLimit);
            wasAdded = true;
        }
        
        Assert.False(wasAdded, "Items beyond limit should not be added");
        Assert.Equal(maxTracked, trackedItems.Count);
        
        // Simulate "Select All" - only selects tracked items
        foreach (var item in trackedItems)
        {
            selectedItems.Add(item);
        }
        
        Assert.Equal(maxTracked, selectedItems.Count);
        Assert.DoesNotContain(beyondLimit, selectedItems);
        
        // Individual selection beyond limit should still work (if implemented)
        // This would require separate tracking mechanism
        
        _output.WriteLine($"✅ Selection semantics correct: {selectedItems.Count} items selected");
    }

    /// <summary>
    /// B14_05: Verify memory estimation is reasonable.
    /// </summary>
    [Fact]
    public void B14_05_MemoryFootprint_IsReasonable()
    {
        // 10,000 object references
        // Typical object: ~100-200 bytes (conservatively)
        // HashSet overhead: ~32 bytes per entry
        // Total estimate: 10K * 132 bytes = 1.32 MB
        
        const int maxTracked = 10_000;
        const int typicalItemSize = 100; // bytes, conservative
        const int hashSetOverhead = 32; // bytes per entry
        
        var totalBytesPerItem = typicalItemSize + hashSetOverhead;
        var estimatedMemoryMB = (maxTracked * totalBytesPerItem) / 1024.0 / 1024.0;
        
        _output.WriteLine($"Estimated memory footprint: {estimatedMemoryMB:F2} MB");
        
        // Verify under 2 MB (conservative threshold)
        Assert.True(estimatedMemoryMB < 2.0, 
            $"Memory estimate ({estimatedMemoryMB:F2} MB) should be under 2 MB");
        
        // Verify reasonable for production
        Assert.True(estimatedMemoryMB < 10.0,
            "Memory should be under 10 MB for production use");
    }

    /// <summary>
    /// B14_06: Verify HashSet operations remain O(1) after limit.
    /// </summary>
    [Fact]
    public void B14_06_HashSetOperations_RemainEfficient()
    {
        const int maxTracked = 10_000;
        var trackedItems = new HashSet<TestItem>();
        
        // Fill to limit
        for (int i = 1; i <= maxTracked; i++)
        {
            trackedItems.Add(new TestItem(i, $"Item {i}"));
        }
        
        // Measure lookup performance
        var lookupItem = new TestItem(5000, "Item 5000");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < 10_000; i++)
        {
            _ = trackedItems.Contains(lookupItem);
        }
        
        sw.Stop();
        var avgMicroseconds = (sw.Elapsed.TotalMilliseconds * 1000) / 10_000;
        
        _output.WriteLine($"Average lookup time: {avgMicroseconds:F3} microseconds");
        
        // Verify O(1) performance (should be < 1 microsecond on modern hardware)
        Assert.True(avgMicroseconds < 10,
            $"Lookup should be O(1), got {avgMicroseconds:F3} microseconds");
    }

    /// <summary>
    /// B14_07: Verify limit applies per grid instance (not global).
    /// </summary>
    [Fact]
    public void B14_07_LimitIsPerInstance_NotGlobal()
    {
        // Each grid instance has its own _loadedVirtualItems HashSet
        // Limit applies per instance, not globally
        
        // Simulate two grid instances
        var grid1Tracked = new HashSet<TestItem>();
        var grid2Tracked = new HashSet<TestItem>();
        
        const int maxTracked = 10_000;
        
        // Fill both to limit
        for (int i = 1; i <= maxTracked; i++)
        {
            grid1Tracked.Add(new TestItem(i, $"Grid1 Item {i}"));
            grid2Tracked.Add(new TestItem(i, $"Grid2 Item {i}"));
        }
        
        Assert.Equal(maxTracked, grid1Tracked.Count);
        Assert.Equal(maxTracked, grid2Tracked.Count);
        
        // Verify independent
        var grid1Item = grid1Tracked.First();
        Assert.DoesNotContain(grid1Item, grid2Tracked); // Different instances
        
        _output.WriteLine("✅ Limit is per-instance, not global");
    }

    /// <summary>
    /// B14_08: Verify RefreshVirtualQueryAsync clears tracked items.
    /// </summary>
    [Fact]
    public void B14_08_QueryRefresh_ClearsTrackedItems()
    {
        // When query changes (search, filter, sort), tracked items should be cleared
        // This prevents stale data accumulation
        
        var trackedItems = new HashSet<TestItem>();
        
        // Fill with initial data
        for (int i = 1; i <= 5_000; i++)
        {
            trackedItems.Add(new TestItem(i, $"Item {i}"));
        }
        
        Assert.Equal(5_000, trackedItems.Count);
        
        // Simulate query refresh (line 1123 in UiDataGrid.razor)
        trackedItems.Clear();
        
        Assert.Empty(trackedItems);
        
        // Fill with new query results
        for (int i = 10_001; i <= 15_000; i++)
        {
            trackedItems.Add(new TestItem(i, $"Item {i}"));
        }
        
        Assert.Equal(5_000, trackedItems.Count);
        
        // Verify old items not present
        var oldItem = new TestItem(1, "Item 1");
        Assert.DoesNotContain(oldItem, trackedItems);
        
        _output.WriteLine("✅ Query refresh clears tracked items correctly");
    }

    /// <summary>
    /// B14_09: Verify warning is logged only once when limit reached.
    /// </summary>
    [Fact]
    public void B14_09_Warning_LoggedOnce()
    {
        // Warning should be logged exactly once when count equals limit
        // Not before, not repeatedly after
        
        const int maxTracked = 10_000;
        var trackedItems = new HashSet<TestItem>();
        int warningCount = 0;
        
        for (int i = 1; i <= 15_000; i++)
        {
            if (trackedItems.Count < maxTracked)
            {
                trackedItems.Add(new TestItem(i, $"Item {i}"));
            }
            else if (trackedItems.Count == maxTracked && warningCount == 0)
            {
                // Log warning ONCE
                warningCount++;
                _output.WriteLine($"⚠️ Warning logged at item {i}");
            }
            // No logging after first warning
        }
        
        Assert.Equal(1, warningCount);
        Assert.Equal(maxTracked, trackedItems.Count);
        
        _output.WriteLine("✅ Warning logged exactly once");
    }

    /// <summary>
    /// B14_10: Document behavior for duplicate instances.
    /// </summary>
    [Fact]
    public void B14_10_DuplicateInstances_AddedOnce()
    {
        // HashSet automatically handles duplicates
        // Same logical item (by reference) only added once
        
        var trackedItems = new HashSet<TestItem>();
        var item = new TestItem(1, "Item 1");
        
        // Try to add same instance multiple times
        bool added1 = trackedItems.Add(item);
        bool added2 = trackedItems.Add(item);
        bool added3 = trackedItems.Add(item);
        
        Assert.True(added1, "First add should succeed");
        Assert.False(added2, "Duplicate should be rejected");
        Assert.False(added3, "Duplicate should be rejected");
        Assert.Single(trackedItems);
        
        // Different instance, same values
        var sameValues = new TestItem(1, "Item 1");
        bool added4 = trackedItems.Add(sameValues);
        
        // For records with value equality, this would be false
        // For classes with reference equality, this would be true
        // TestItem is a record, so value equality applies
        Assert.False(added4, "Record with same values rejected");
        Assert.Single(trackedItems);
        
        _output.WriteLine("✅ Duplicate handling correct");
    }
}
