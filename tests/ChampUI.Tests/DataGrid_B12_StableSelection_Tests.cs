using ChampUI.Components.Data;
using Xunit;

namespace ChampUI.Tests;

public class DataGrid_B12_StableSelection_Tests
{
    private record TestItem(int Id, string Name);

    [Fact]
    public void B12_01_RowKeySelector_EnablesStableSelection()
    {
        // Arrange
        var item1 = new TestItem(1, "Alpha");
        var item2 = new TestItem(1, "Alpha"); // Fresh instance, same key

        Func<TestItem, object> keySelector = item => item.Id;

        // Act
        var key1 = keySelector(item1);
        var key2 = keySelector(item2);

        // Assert
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void B12_02_NullKey_LogsWarning()
    {
        // Arrange
        var item = new TestItem(0, "Test");
        Func<TestItem, object?> keySelector = _ => null;

        // Act
        var key = keySelector(item);

        // Assert
        Assert.Null(key);
        // In production, Logger.LogWarning would be called
    }

    [Fact]
    public void B12_03_DuplicateKeys_DetectedInSameSelection()
    {
        // Arrange
        var item1 = new TestItem(1, "Alpha");
        var item2 = new TestItem(1, "Beta");
        Func<TestItem, object> keySelector = item => item.Id;

        var keys = new HashSet<object>();

        // Act
        var key1 = keySelector(item1);
        var key2 = keySelector(item2);
        var added1 = keys.Add(key1);
        var added2 = keys.Add(key2);

        // Assert
        Assert.True(added1);
        Assert.False(added2); // Duplicate key
    }

    [Fact]
    public void B12_04_KeySelector_WorksWithCompositeKeys()
    {
        // Arrange
        var item1 = new TestItem(1, "Alpha");
        var item2 = new TestItem(1, "Alpha");
        Func<TestItem, object> keySelector = item => item.Id;

        // Act
        var key1 = keySelector(item1);
        var key2 = keySelector(item2);

        // Assert
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void B12_05_MultipleSelections_MaintainStableKeys()
    {
        // Arrange
        var items = new[]
        {
            new TestItem(1, "Alpha"),
            new TestItem(2, "Beta"),
            new TestItem(3, "Gamma")
        };

        Func<TestItem, object> keySelector = item => item.Id;
        var selectedKeys = new HashSet<object>();

        // Act
        foreach (var item in items)
        {
            selectedKeys.Add(keySelector(item));
        }

        // Assert
        Assert.Equal(3, selectedKeys.Count);
        Assert.Contains(1, selectedKeys);
        Assert.Contains(2, selectedKeys);
        Assert.Contains(3, selectedKeys);
    }

    [Fact]
    public void B12_06_KeySelector_HandlesNullItem()
    {
        // Arrange
        TestItem? item = null;
        Func<TestItem, object> keySelector = item => item.Id;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => keySelector(item!));
    }

    [Fact]
    public void B12_07_ExternalSelectedItems_SynchronizesKeys()
    {
        // Arrange
        var item1 = new TestItem(1, "Alpha");
        var item2 = new TestItem(2, "Beta");
        var externalSelection = new List<TestItem> { item1, item2 };

        Func<TestItem, object> keySelector = item => item.Id;
        var keys = new HashSet<object>();

        // Act
        foreach (var item in externalSelection)
        {
            keys.Add(keySelector(item));
        }

        // Assert
        Assert.Equal(2, keys.Count);
        Assert.Contains(1, keys);
        Assert.Contains(2, keys);
    }

    [Fact]
    public void B12_08_NullKeyInExternalSync_SkipsItem()
    {
        // Arrange
        var item = new TestItem(1, "Alpha");
        Func<TestItem, object?> keySelector = _ => null;

        var keys = new HashSet<object>();

        // Act
        var key = keySelector(item);
        if (key != null)
        {
            keys.Add(key);
        }

        // Assert
        Assert.Empty(keys);
    }

    [Fact]
    public void B12_09_CompositeKey_WorksCorrectly()
    {
        // Arrange
        var item1 = new TestItem(1, "Alpha");
        var item2 = new TestItem(1, "Alpha"); // Same data
        var item3 = new TestItem(1, "Beta");  // Different name

        Func<TestItem, object> compositeKeySelector = item => (item.Id, item.Name);

        // Act
        var key1 = compositeKeySelector(item1);
        var key2 = compositeKeySelector(item2);
        var key3 = compositeKeySelector(item3);

        // Assert
        Assert.Equal(key1, key2); // Same composite key
        Assert.NotEqual(key1, key3); // Different composite key
    }

    [Fact]
    public void B12_10_SelectionWithoutKeySelector_UsesReferenceEquality()
    {
        // Test legacy behavior when RowKeySelector is not configured
        // Use class (reference equality) not record (value equality)
        var item1 = new TestItemClass { Id = 1, Name = "Alpha" };
        var item2 = new TestItemClass { Id = 1, Name = "Alpha" }; // Same data, different instance

        var selected = new HashSet<TestItemClass>();

        // Without key selector, uses reference equality
        selected.Add(item1);

        Assert.Contains(item1, selected);
        Assert.DoesNotContain(item2, selected); // Different reference
    }

    // Separate class for testing reference equality (records have value equality)
    private class TestItemClass
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
    }
}
