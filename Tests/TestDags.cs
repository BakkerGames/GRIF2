using Grif;

namespace Tests;

public class TestDags
{
    [SetUp]
    public void Setup()
    {
        // This method is called before each test.
        // You can initialize any shared resources here.
    }

    [Test]
    public void TestProcess()
    {
        Grod grod = new("testGrod");
        string script = "@write(abc)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "abc") }));
    }

    [Test]
    public void TestTwoCommands()
    {
        Grod grod = new("testGrod");
        string script = "@write(abc)@write(def)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "abc"), new(0, "def") }));
    }

    [Test]
    public void TestConcatenate()
    {
        Grod grod = new("testGrod");
        string script = "@write(abc,def)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "abc"), new(0, "def") }));
    }

    [Test]
    public void TestWriteError()
    {
        Grod grod = new("testGrod");
        string script = "@write(abc";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(-1, "Missing closing parenthesis") }));
    }

    [Test]
    public void TestNoParams()
    {
        Grod grod = new("testGrod");
        string script = "@write()";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { }));
    }

    [Test]
    public void TestGetAndSet()
    {
        Grod grod = new("testGrod");
        grod.Set("key1", "value1");
        string script = "@get(key1)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(1, "value1") }));
    }
}
