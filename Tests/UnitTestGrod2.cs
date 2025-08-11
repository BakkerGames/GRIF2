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
        Assert.That(_grod2.GetLevelKeys, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_grod2.GetLevelKeys, Does.Contain(Level1));
            Assert.That(_grod2.GetLevelKeys, Does.Contain(Level2));
        }
    }

    [Test]
    public void TestLevelGetCurrentLevel()
    {
        // Test if the current level is set correctly
        Assert.That(_grod2.GetCurrentLevel, Is.EqualTo(Level2));
    }

    [Test]
    public void TestSetGetRemove()
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
    public void TestLevelNew()
    {
        // Test adding a level
        _grod2.AddLevel("NewLevel", Level1);
        Assert.That(_grod2.GetLevelKeys, Has.Count.EqualTo(3));
        Assert.That(_grod2.GetLevelKeys, Does.Contain("NewLevel"));
    }

    [Test]
    public void TestLevelRemove()
    {
        _grod2.SetCurrentLevel(Level2);
        // Test removing a level which is the current level
        Assert.Throws<InvalidOperationException>(() => _grod2.RemoveLevel(Level2));
        // Test removing a level which is a parent of some level
        Assert.Throws<InvalidOperationException>(() => _grod2.RemoveLevel(Level1));
        // Test if the level was not removed
        Assert.That(_grod2.GetLevelKeys, Has.Count.EqualTo(2));
        _grod2.SetCurrentLevel(null);
        Assert.That(_grod2.GetCurrentLevel, Is.Null);
        _grod2.RemoveLevel(Level2);
        // Test if the level was removed
        Assert.That(_grod2.GetLevelKeys, Has.Count.EqualTo(1));
        // Test if the current level is null
        Assert.That(_grod2.GetCurrentLevel, Is.Null);
    }

    [Test]
    public void TestLevelRemoveInUse()
    {
        Assert.Throws<InvalidOperationException>(() => _grod2.RemoveLevel(Level1));
        // Test if the level was not removed
        Assert.That(_grod2.GetLevelKeys, Has.Count.EqualTo(2));
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
    public void TestDataClear()
    {
        // Test clearing all data
        _grod2.Set("TestKey", "TestValue");
        Assert.That(_grod2.Get("TestKey"), Is.EqualTo("TestValue"));
        _grod2.Clear();
        Assert.That(_grod2.Get("TestKey"), Is.Null);
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
    public void TestLevelAddWithNullName()
    {
        // Test adding a level with a null name
        Assert.Throws<ArgumentNullException>(() => _grod2.AddLevel(null!));
    }

    [Test]
    public void TestLevelAddWithInvalidParent()
    {
        // Test adding a level with an invalid parent
        Assert.Throws<KeyNotFoundException>(() => _grod2.AddLevel("InvalidParentLevel", "NonExistentParent"));
    }

    [Test]
    public void TestLevelSetCurrentWithInvalidName()
    {
        // Test setting current level to a non-existent level
        Assert.Throws<KeyNotFoundException>(() => _grod2.SetCurrentLevel("NonExistentLevel"));
    }

    [Test]
    public void TestLevelAddDuplicate()
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
    public void TestSetGetByLevelKey()
    {
        // Test setting a value by level name
        _grod2.Set(Level1, "Level1Key", "Level1Value");
        Assert.That(_grod2.Get(Level1, "Level1Key"), Is.EqualTo("Level1Value"));
        // Test setting a value in the current level
        _grod2.Set(Level2, "Level2Key", "Level2Value");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_grod2.Get(Level2, "Level2Key"), Is.EqualTo("Level2Value"));
            // Test getting a value from a parent level
            Assert.That(_grod2.Get(Level2, "Level1Key"), Is.EqualTo("Level1Value"));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(keys, Does.Contain("Key1"));
            Assert.That(keys, Does.Contain("Key2"));
        }
    }


    [Test]
    public void TestGetKeysRecursive()
    {
        // Test getting all keys recursively from the current level
        _grod2.Set(Level1, "Key1", "Value1");
        _grod2.Set(Level2, "Key2", "Value2");
        var keys = _grod2.GetKeysRecursive();
        Assert.That(keys, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(keys, Does.Contain("Key1"));
            Assert.That(keys, Does.Contain("Key2"));
        }
    }

    [Test]
    public void TestGetItems()
    {
        // Test getting all items in the current level
        _grod2.Set("Item1", expectedValue1[0]);
        _grod2.Set("Item2", expectedValue2[0]);
        var items = _grod2.GetItems();
        Assert.That(items, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(items.Where(x => x.Key == "Item1").Select(x => x.Text), Is.EqualTo(expectedValue1));
            Assert.That(items.Where(x => x.Key == "Item2").Select(x => x.Text), Is.EqualTo(expectedValue2));
        }
    }

    [Test]
    public void TestGetItemsRecursive()
    {
        // Test getting all items recursively from the current level
        _grod2.Set(Level1, "Item1", expectedValue1[0]);
        _grod2.Set(Level2, "Item2", expectedValue2[0]);
        var items = _grod2.GetItemsRecursive(Level2);
        Assert.That(items, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(items.Where(x => x.Key == "Item1").Select(x => x.Text), Is.EqualTo(expectedValue1));
            Assert.That(items.Where(x => x.Key == "Item2").Select(x => x.Text), Is.EqualTo(expectedValue2));
        }
    }

    [Test]
    public void TestCaseInsensitiveKeyHandling()
    {
        // Test case-insensitive key handling
        _grod2.Set("TestKey", "TestValue");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_grod2.Get("testkey"), Is.EqualTo("TestValue"));
            Assert.That(_grod2.Get("TESTKEY"), Is.EqualTo("TestValue"));
        }
        // Test setting a value with different case
        _grod2.Set("TESTKEY", "UpdatedValue");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_grod2.Get("testkey"), Is.EqualTo("UpdatedValue"));
            Assert.That(_grod2.Get("TESTKEY"), Is.EqualTo("UpdatedValue"));
        }
    }
}
