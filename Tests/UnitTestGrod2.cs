using GROD2;

namespace Tests;

public class UnitTestGrod2
{
    private Grod2 _grod2;

    private const string Level1 = "Base";
    private const string Level2 = "Overlay";
    private const string InvalidKey = "   ";

    [SetUp]
    public void Setup()
    {
        _grod2 = new Grod2();
        _grod2.AddLevel(Level1);
        _grod2.AddLevel(Level2, Level1);
        _grod2.SetCurrentLevel(Level2);
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
    public void TestGetItem()
    {
        // Test getting an item
        var item = _grod2.GetItem("TestKey");
        Assert.Multiple(() =>
        {
            Assert.That(item.Key, Is.EqualTo("TestKey"));
            Assert.That(item.Text, Is.Empty);
        });
        // Set the item and test again
        _grod2.Set(new Item { Key = "TestKey", Text = "TestValue" });
        item = _grod2.GetItem("TestKey");
        Assert.Multiple(() =>
        {
            Assert.That(item.Key, Is.EqualTo("TestKey"));
            Assert.That(item.Text, Is.EqualTo("TestValue"));
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
        Assert.Multiple(() =>
        {
            Assert.That(item.Key, Is.EqualTo("NonExistentKey"));
            Assert.That(item.Text, Is.Empty);
        });
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
}
