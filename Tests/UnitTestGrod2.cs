using GROD2;

namespace Tests;

public class UnitTestGrod2
{
    private Grod2 _grod2;

    private const string Level1 = "Base";
    private const string Level2 = "Overlay";
    private const string InvalidKey = "   ";
    private static readonly string[] expectedValue1 = ["Value1"];
    private static readonly string[] expectedValue2 = ["Value2"];

    [SetUp]
    public void Setup()
    {
        _grod2 = new Grod2();
        _grod2.AddLevel(Level1);
        _grod2.AddLevel(Level2, Level1);
        _grod2.SetCurrentLevel(Level2);
    }

    [Test]
    public void TestInitialization()
    {
        // Test if Grod2 initializes correctly
        Assert.That(_grod2, Is.Not.Null);
        Assert.That(_grod2.GetCurrentLevel, Is.EqualTo(Level2));
        Assert.That(_grod2.LevelKeys, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(_grod2.LevelKeys, Does.Contain(Level1));
            Assert.That(_grod2.LevelKeys, Does.Contain(Level2));
        });
    }

    [Test]
    public void TestGetCurrentLevel()
    {
        // Test if the current level is set correctly
        Assert.That(_grod2.GetCurrentLevel, Is.EqualTo(Level2));
    }

    [Test]
    public void TestSetAndGet()
    {
        // Test setting a value
        _grod2.Set("TestKey", "TestValue");
        Assert.That(_grod2.Get("TestKey"), Is.EqualTo("TestValue"));
        // Test updating a value
        _grod2.Set("TestKey", "UpdatedValue");
        Assert.That(_grod2.Get("TestKey"), Is.EqualTo("UpdatedValue"));
        // Test removing a value
        _grod2.Remove("TestKey");
        Assert.That(_grod2.Get("TestKey"), Is.Null);
    }

    [Test]
    public void TestSetAndGetItemByLevel()
    {
        // Test setting an item by level name
        var item = new Item { Key = "TestKey", Text = "TestValue" };
        _grod2.Set(Level1, item);
        var retrievedItem = _grod2.GetItem(Level1, "TestKey");
        Assert.Multiple(() =>
        {
            Assert.That(retrievedItem, Is.Not.Null);
            Assert.That(retrievedItem!.Key, Is.EqualTo("TestKey"));
            Assert.That(retrievedItem!.Text, Is.EqualTo("TestValue"));
        });
    }

    [Test]
    public void TestGetItem()
    {
        // Test getting an item
        var item = _grod2.GetItem("TestKey");
        Assert.Multiple(() =>
        {
            Assert.That(item, Is.Null);
        });
        // Set the item and test again
        _grod2.Set(new Item { Key = "TestKey", Text = "TestValue" });
        item = _grod2.GetItem("TestKey");
        Assert.Multiple(() =>
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item!.Key, Is.EqualTo("TestKey"));
            Assert.That(item!.Text, Is.EqualTo("TestValue"));
        });
    }

    [Test]
    public void TestLevelManagement()
    {
        // Test adding a level
        _grod2.AddLevel("NewLevel", Level1);
        Assert.That(_grod2.GetCurrentLevel, Is.EqualTo(Level2));
        // Test setting current level to a different one
        _grod2.SetCurrentLevel(Level1);
        Assert.That(_grod2.GetCurrentLevel, Is.EqualTo(Level1));
    }

    [Test]
    public void TestRemoveLevel()
    {
        _grod2.SetCurrentLevel(Level2);
        // Test removing a level which is the current level
        Assert.Throws<InvalidOperationException>(() => _grod2.RemoveLevel(Level2));
        // Test removing a level which is a parent of some level
        Assert.Throws<InvalidOperationException>(() => _grod2.RemoveLevel(Level1));
        // Test if the level was not removed
        Assert.That(_grod2.LevelKeys, Has.Count.EqualTo(2));
        _grod2.SetCurrentLevel(null);
        _grod2.RemoveLevel(Level2);
        // Test if the level was removed
        Assert.That(_grod2.LevelKeys, Has.Count.EqualTo(1));
        // Test if the current level is null
        Assert.That(_grod2.GetCurrentLevel, Is.Null);
    }

    [Test]
    public void TestRemoveLevelInUse()
    {
        Assert.Throws<InvalidOperationException>(() => _grod2.RemoveLevel(Level1));
        // Test if the level was not removed
        Assert.That(_grod2.LevelKeys, Has.Count.EqualTo(2));
    }

    [Test]
    public void TestInvalidKey()
    {
        // Test setting a value with an invalid key
        Assert.Throws<ArgumentNullException>(() => _grod2.Set(InvalidKey, "Value"));
        // Test getting a value with an invalid key
        Assert.Throws<ArgumentNullException>(() => _grod2.Get(InvalidKey));
        // Test removing a value with an invalid key
        Assert.Throws<ArgumentNullException>(() => _grod2.Remove(InvalidKey));
    }

    [Test]
    public void TestClearData()
    {
        // Test clearing all data
        _grod2.Set("TestKey", "TestValue");
        Assert.That(_grod2.Get("TestKey"), Is.EqualTo("TestValue"));
        _grod2.Clear();
        Assert.That(_grod2.Get("TestKey"), Is.Null);
    }

    [Test]
    public void TestSetItemNull()
    {
        // Test setting a null item
        Assert.Throws<ArgumentNullException>(() => _grod2.Set((Item)null!));
    }

    [Test]
    public void TestGetNonExistentItem()
    {
        // Test getting a non-existent item
        var item = _grod2.GetItem("NonExistentKey");
        Assert.That(item, Is.Null);
    }

    [Test]
    public void TestGetWithNullKey()
    {
        // Test getting a value with a null key
        Assert.Throws<ArgumentNullException>(() => _grod2.Get(null!));
    }

    [Test]
    public void TestSetWithNullKey()
    {
        // Test setting a value with a null key
        Assert.Throws<ArgumentNullException>(() => _grod2.Set(null!, "Value"));
    }

    [Test]
    public void TestRemoveWithNullKey()
    {
        // Test removing a value with a null key
        Assert.Throws<ArgumentNullException>(() => _grod2.Remove(null!));
    }

    [Test]
    public void TestAddLevelWithNullName()
    {
        // Test adding a level with a null name
        Assert.Throws<ArgumentNullException>(() => _grod2.AddLevel(null!));
    }

    [Test]
    public void TestAddLevelWithInvalidParent()
    {
        // Test adding a level with an invalid parent
        Assert.Throws<KeyNotFoundException>(() => _grod2.AddLevel("InvalidParentLevel", "NonExistentParent"));
    }

    [Test]
    public void TestSetCurrentLevelWithInvalidName()
    {
        // Test setting current level to a non-existent level
        Assert.Throws<KeyNotFoundException>(() => _grod2.SetCurrentLevel("NonExistentLevel"));
    }

    [Test]
    public void TestAddDuplicateLevel()
    {
        // Test adding a duplicate level
        Assert.Throws<InvalidOperationException>(() => _grod2.AddLevel(Level1));
    }

    [Test]
    public void TestRemoveKeyFromDifferentLevels()
    {
        // Set a key in the base level
        _grod2.SetCurrentLevel(Level1);
        _grod2.Set("SharedKey", "BaseValue");
        // Set the same key in the overlay level
        _grod2.SetCurrentLevel(Level2);
        _grod2.Set("SharedKey", "OverlayValue");
        // Verify the value in the overlay level
        Assert.That(_grod2.Get("SharedKey"), Is.EqualTo("OverlayValue"));
        // Remove the key from the overlay level
        _grod2.Remove("SharedKey");
        // Verify that the key now resolves to the base level value
        Assert.That(_grod2.Get("SharedKey"), Is.EqualTo("BaseValue"));
        // Remove the key from the base level
        _grod2.SetCurrentLevel(Level1);
        _grod2.Remove("SharedKey");
        // Verify that the key is now completely removed
        Assert.That(_grod2.Get("SharedKey"), Is.Null);
    }

    [Test]
    public void TestSetItemWithEmptyText()
    {
        // Test setting an item with empty text
        _grod2.Set(new Item { Key = "EmptyTextKey", Text = string.Empty });
        var item = _grod2.GetItem("EmptyTextKey");
        Assert.Multiple(() =>
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item!.Key, Is.EqualTo("EmptyTextKey"));
            Assert.That(item!.Text, Is.Empty);
        });
    }

    [Test]
    public void TestSetItemWithWhitespaceKey()
    {
        // Test setting an item with a whitespace key
        Assert.Throws<ArgumentNullException>(() => _grod2.Set(new Item { Key = "   ", Text = "Value" }));
    }

    [Test]
    public void TestSetItemWithNullText()
    {
        // Test setting an item with null text
        var item = new Item { Key = "NullTextKey", Text = null! };
        _grod2.Set(item);
        var item2 = _grod2.GetItem("NullTextKey");
        Assert.Multiple(() =>
        {
            Assert.That(item2, Is.Null);
        });
    }

    [Test]
    public void TestGetItemByLevel()
    {
        // Test getting an item by level name
        _grod2.Set(Level1, "Level1Key", "Level1Value");
        var item = _grod2.GetItem(Level1, "Level1Key");
        Assert.Multiple(() =>
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item!.Key, Is.EqualTo("Level1Key"));
            Assert.That(item!.Text, Is.EqualTo("Level1Value"));
        });
        // Test getting an item from the current level
        _grod2.Set(Level2, "Level2Key", "Level2Value");
        item = _grod2.GetItem(Level2, "Level2Key");
        Assert.Multiple(() =>
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item!.Key, Is.EqualTo("Level2Key"));
            Assert.That(item!.Text, Is.EqualTo("Level2Value"));
        });
    }

    [Test]
    public void TestSetAndGetByLevelKey()
    {
        // Test setting a value by level name
        _grod2.Set(Level1, "Level1Key", "Level1Value");
        Assert.That(_grod2.Get(Level1, "Level1Key"), Is.EqualTo("Level1Value"));
        // Test setting a value in the current level
        _grod2.Set(Level2, "Level2Key", "Level2Value");
        Assert.Multiple(() =>
        {
            Assert.That(_grod2.Get(Level2, "Level2Key"), Is.EqualTo("Level2Value"));
            // Test getting a value from a parent level
            Assert.That(_grod2.Get(Level2, "Level1Key"), Is.EqualTo("Level1Value"));
        });
        Assert.That(_grod2.Get(Level1, "Level2Key"), Is.Null, "Should not find Level2Key in Level1");
    }

    [Test]
    public void TestRemoveKeyFromSpecificLevel()
    {
        // Set a key in the base level
        _grod2.Set(Level1, "SharedKey", "BaseValue");
        // Set the same key in the overlay level
        _grod2.Set(Level2, "SharedKey", "OverlayValue");
        // Verify the value in the overlay level
        Assert.That(_grod2.Get(Level2, "SharedKey"), Is.EqualTo("OverlayValue"));
        // Remove the key from the overlay level
        _grod2.Remove(Level2, "SharedKey");
        // Verify that the key now resolves to the base level value
        Assert.That(_grod2.Get(Level1, "SharedKey"), Is.EqualTo("BaseValue"));
        // Remove the key from the base level
        _grod2.Remove(Level1, "SharedKey");
        // Verify that the key is now completely removed
        Assert.That(_grod2.Get(Level1, "SharedKey"), Is.Null);
    }

    [Test]
    public void TestGetKeys()
    {
        // Test getting all keys in the current level
        _grod2.Set("Key1", "Value1");
        _grod2.Set("Key2", "Value2");
        var keys = _grod2.GetKeys();
        Assert.That(keys.Count, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(keys, Does.Contain("Key1"));
            Assert.That(keys, Does.Contain("Key2"));
        });
    }


    [Test]
    public void TestGetKeysRecursive()
    {
        // Test getting all keys recursively from the current level
        _grod2.Set(Level1, "Key1", "Value1");
        _grod2.Set(Level2, "Key2", "Value2");
        var keys = _grod2.GetKeysRecursive();
        Assert.That(keys, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(keys, Does.Contain("Key1"));
            Assert.That(keys, Does.Contain("Key2"));
        });
    }

    [Test]
    public void TestGetKeysSorted()
    {
        // Test getting all keys sorted
        _grod2.Set("KeyB", "ValueB");
        _grod2.Set("KeyA", "ValueA");
        var keys = _grod2.GetKeysSorted();
        Assert.That(keys, Is.EqualTo(new List<string> { "KeyA", "KeyB" }));
    }

    [Test]
    public void TestGetItems()
    {
        // Test getting all items in the current level
        _grod2.Set("Item1", expectedValue1[0]);
        _grod2.Set("Item2", expectedValue2[0]);
        var items = _grod2.GetItems();
        Assert.That(items, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items.Where(x => x.Key == "Item1").Select(x => x.Text), Is.EqualTo(expectedValue1));
            Assert.That(items.Where(x => x.Key == "Item2").Select(x => x.Text), Is.EqualTo(expectedValue2));
        });
    }

    [Test]
    public void TestGetItemsRecursive()
    {
        // Test getting all items recursively from the current level
        _grod2.Set(Level1, "Item1", expectedValue1[0]);
        _grod2.Set(Level2, "Item2", expectedValue2[0]);
        var items = _grod2.GetItemsRecursive(Level2);
        Assert.That(items, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(items.Where(x => x.Key == "Item1").Select(x => x.Text), Is.EqualTo(expectedValue1));
            Assert.That(items.Where(x => x.Key == "Item2").Select(x => x.Text), Is.EqualTo(expectedValue2));
        });
    }

    [Test]
    public void TestCaseInsensitiveKeyHandling()
    {
        // Test case-insensitive key handling
        _grod2.Set("TestKey", "TestValue");
        Assert.Multiple(() =>
        {
            Assert.That(_grod2.Get("testkey"), Is.EqualTo("TestValue"));
            Assert.That(_grod2.Get("TESTKEY"), Is.EqualTo("TestValue"));
        });
        // Test setting a value with different case
        _grod2.Set("TESTKEY", "UpdatedValue");
        Assert.Multiple(() =>
        {
            Assert.That(_grod2.Get("testkey"), Is.EqualTo("UpdatedValue"));
            Assert.That(_grod2.Get("TESTKEY"), Is.EqualTo("UpdatedValue"));
        });
    }
}
