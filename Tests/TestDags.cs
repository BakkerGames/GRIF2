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
    public async Task TestProcess()
    {
        Grod grod = new("testGrod");
        string script = "@write(abc)";
        var result = await Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagItem> { new(0, "abc") }));
    }
}
