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

    [Test]
    public void TestIfCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @true @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfNotCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @not @false @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfWithAndCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @true @and @true @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfWithOrCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @false @or @true @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfWithOrShortCircuitCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @true @or @false @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfWithElseCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @false @then @write(\"Condition met\") @else @write(\"Condition not met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Condition not met\"") }));
    }

    [Test]
    public void TestIfWithAndFailsToElseCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @true @and @false @then @write(\"Condition met\") @else @write(\"Condition not met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Condition not met\"") }));
    }

    [Test]
    public void TestIfWithAndShortCircuitToElseCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @false @and @true @then @write(\"Condition met\") @else @write(\"Condition not met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Condition not met\"") }));
    }

    [Test]
    public void TestIfWithElseIfCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @false @then @write(\"Condition met\") @elseif @true @then @write(\"Second condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(0, "\"Second condition met\"") }));
    }
}
