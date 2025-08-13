namespace Tests;

public class TestGrod
{
    [SetUp]
    public void Setup()
    {
        // This method is called before each test.
        // You can initialize any shared resources here.
    }

    [Test]
    public void TestGrodSetAndGet()
    {
        var grod = new Grif.Grod("grod1");
        grod.Set("key1", "value1");
        Assert.That(grod.Get("key1", false), Is.EqualTo("value1"));
    }

    [Test]
    public void TestGrodSetAndGetRecursive()
    {
        var parentGrod = new Grif.Grod("parent");
        parentGrod.Set("key2", "value2");
        var childGrod = new Grif.Grod("child", parentGrod);
        Assert.That(childGrod.Get("key2", true), Is.EqualTo("value2"));
    }

    [Test]
    public void TestGrodRemove()
    {
        var grod = new Grif.Grod("grod3");
        grod.Set("key3", "value3");
        grod.Remove("key3");
        Assert.That(grod.Get("key3", false), Is.Null);
    }

    [Test]
    public void TestGrodClear()
    {
        var grod = new Grif.Grod("grod4");
        grod.Set("key4", "value4");
        grod.Clear(false);
        Assert.That(grod.Get("key4", false), Is.Null);
    }
}
