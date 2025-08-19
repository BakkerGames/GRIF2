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
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "abc") }));
    }

    [Test]
    public void TestTwoCommands()
    {
        Grod grod = new("testGrod");
        string script = "@write(abc)@write(def)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "abc"), new(DagsType.Text, "def") }));
    }

    [Test]
    public void TestConcatenate()
    {
        Grod grod = new("testGrod");
        string script = "@write(abc,def)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "abc"), new(DagsType.Text, "def") }));
    }

    [Test]
    public void TestWriteError()
    {
        Grod grod = new("testGrod");
        string script = "@write(abc";
        var result = Dags.Process(script, grod);
        var expected = new List<DagsItem> { new(DagsType.Error, "Error processing command at index 2:\r\nMissing closing parenthesis\r\n0: @write(\r\n1: abc\r\n") };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestNoParams()
    {
        Grod grod = new("testGrod");
        string script = "@write()";
        var result = Dags.Process(script, grod);
        var expected = new List<DagsItem> { new(DagsType.Error, "Error processing command at index 2:\r\nExpected at least one parameter, but got 0\r\n0: @write(\r\n1: )\r\n") };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestGetAndSet()
    {
        Grod grod = new("testGrod");
        grod.Set("key1", "value1");
        string script = "@get(key1)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "value1") }));
    }

    [Test]
    public void TestIfCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if true @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfNotCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if @not false @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfWithAndCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if true @and true @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfWithOrCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if false @or true @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfWithOrShortCircuitCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if true @or false @then @write(\"Condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Condition met\"") }));
    }

    [Test]
    public void TestIfWithElseCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if false @then @write(\"Condition met\") @else @write(\"Condition not met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Condition not met\"") }));
    }

    [Test]
    public void TestIfWithAndFailsToElseCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if true @and false @then @write(\"Condition met\") @else @write(\"Condition not met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Condition not met\"") }));
    }

    [Test]
    public void TestIfWithAndShortCircuitToElseCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if false @and true @then @write(\"Condition met\") @else @write(\"Condition not met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Condition not met\"") }));
    }

    [Test]
    public void TestIfWithElseIfCondition()
    {
        Grod grod = new("testGrod");
        string script = "@if false @then @write(\"Condition met\") @elseif true @then @write(\"Second condition met\") @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Second condition met\"") }));
    }

    [Test]
    public void TestIfNestedAnswer1()
    {
        Grod grod = new("testGrod");
        string script = "@if true @then @if true @then @write(Answer1) @else @write(Answer2) @endif @elseif false @then @if true @then @write(Answer3) @else @write(Answer4) @endif @else @write(Answer5) @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "Answer1") }));
    }

    [Test]
    public void TestIfNestedAnswer2()
    {
        Grod grod = new("testGrod");
        string script = "@if true @then @if false @then @write(Answer1) @else @write(Answer2) @endif @elseif false @then @if true @then @write(Answer3) @else @write(Answer4) @endif @else @write(Answer5) @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "Answer2") }));
    }

    [Test]
    public void TestIfNestedAnswer3()
    {
        Grod grod = new("testGrod");
        string script = "@if false @then @if true @then @write(Answer1) @else @write(Answer2) @endif @elseif true @then @if true @then @write(Answer3) @else @write(Answer4) @endif @else @write(Answer5) @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "Answer3") }));
    }

    [Test]
    public void TestIfNestedAnswer4()
    {
        Grod grod = new("testGrod");
        string script = "@if false @then @if true @then @write(Answer1) @else @write(Answer2) @endif @elseif true @then @if false @then @write(Answer3) @else @write(Answer4) @endif @else @write(Answer5) @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "Answer4") }));
    }

    [Test]
    public void TestIfNestedAnswer5()
    {
        Grod grod = new("testGrod");
        string script = "@if false @then @if true @then @write(Answer1) @else @write(Answer2) @endif @elseif false @then @if true @then @write(Answer3) @else @write(Answer4) @endif @else @write(Answer5) @endif";
        grod.Set("key1", "value1");
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "Answer5") }));
    }

    [Test]
    public void TestIfEQ()
    {
        Grod grod = new("testGrod");
        string script = "@if @eq(1,1) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfEQNull()
    {
        Grod grod = new("testGrod");
        string script = "@if @eq(null,null) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfEQString()
    {
        Grod grod = new("testGrod");
        string script = "@if @eq(abc,abc) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfNE()
    {
        Grod grod = new("testGrod");
        string script = "@if @ne(1,2) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfNENull()
    {
        Grod grod = new("testGrod");
        string script = "@if @ne(null,2) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfNEString()
    {
        Grod grod = new("testGrod");
        string script = "@if @ne(abc,xyz) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfGT()
    {
        Grod grod = new("testGrod");
        string script = "@if @gt(2,1) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfGTNull()
    {
        Grod grod = new("testGrod");
        string script = "@if @gt(2,null) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfGTString()
    {
        Grod grod = new("testGrod");
        string script = "@if @gt(xyz,abc) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfGE()
    {
        Grod grod = new("testGrod");
        string script = "@if @ge(1,1) @and @ge(2,1) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfGENull()
    {
        Grod grod = new("testGrod");
        string script = "@if @ge(null,null) @and @ge(2,null) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfGEString()
    {
        Grod grod = new("testGrod");
        string script = "@if @ge(abc,abc) @and @ge(xyz,abc) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfLT()
    {
        Grod grod = new("testGrod");
        string script = "@if @lt(1,2) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfLTNull()
    {
        Grod grod = new("testGrod");
        string script = "@if @lt(null,2) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfLTString()
    {
        Grod grod = new("testGrod");
        string script = "@if @lt(abc,xyz) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfLE()
    {
        Grod grod = new("testGrod");
        string script = "@if @le(1,1) @and @le(1,2) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfLENull()
    {
        Grod grod = new("testGrod");
        string script = "@if @le(null,null) @and @le(null,2) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestIfLEString()
    {
        Grod grod = new("testGrod");
        string script = "@if @le(abc,abc) @and @le(abc,xyz) @then @write(answer) @endif";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "answer") }));
    }

    [Test]
    public void TestMsg()
    {
        Grod grod = new("testGrod");
        grod.Set("Hello", "Hello, World!");
        string script = "@msg(Hello)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "Hello, World!"), new(DagsType.Text, "\\n") }));
    }

    [Test]
    public void TestParameterWithFunction()
    {
        Grod grod = new("testGrod");
        grod.Set("key1", "value1");
        string script = "@write(@get(key1))";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "value1") }));
    }

    [Test]
    public void TestParameterWithNestedFunction()
    {
        Grod grod = new("testGrod");
        grod.Set("key1", "value1");
        grod.Set("key2", "key1");
        string script = "@write(@get(@get(key2)))";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "value1") }));
    }

    [Test]
    public void TestUnknownToken()
    {
        Grod grod = new("testGrod");
        string script = "@unknown()";
        var result = Dags.Process(script, grod);
        var expected = new List<DagsItem> { new(DagsType.Error, "Error processing command at index 2:\r\nUnknown token: @unknown(\r\n0: @unknown(\r\n1: )\r\n") };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestUserDefinedScript()
    {
        Grod grod = new("testGrod");
        grod.Set("@myScript", "@write(\"Hello from user-defined script!\")");
        string script = "@myScript";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Text, "\"Hello from user-defined script!\"") }));
    }

    [Test]
    public void TestAddTo()
    {
        Grod grod = new("testGrod");
        grod.Set("counter", "5");
        string script = "@addto(counter,3)@get(counter)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "8") }));
    }

    [Test]
    public void TestSubTo()
    {
        Grod grod = new("testGrod");
        grod.Set("counter", "5");
        string script = "@subto(counter,2)@get(counter)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "3") }));
    }

    [Test]
    public void TestMulTo()
    {
        Grod grod = new("testGrod");
        grod.Set("counter", "5");
        string script = "@multo(counter,4)@get(counter)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "20") }));
    }

    [Test]
    public void TestDivideBy()
    {
        Grod grod = new("testGrod");
        grod.Set("counter", "20");
        string script = "@divto(counter,4)@get(counter)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "5") }));
    }

    [Test]
    public void TestDivideByZero()
    {
        Grod grod = new("testGrod");
        grod.Set("counter", "20");
        string script = "@divto(counter,0)@get(counter)";
        var result = Dags.Process(script, grod);
        var expected = new List<DagsItem> { new(DagsType.Error, "Error processing command at index 5:\r\nDivision by zero is not allowed.\r\n0: @divto(\r\n1: counter\r\n2: ,\r\n3: 0\r\n4: )\r\n5: @get(\r\n6: counter\r\n7: )\r\n") };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestAddToNonInteger()
    {
        Grod grod = new("testGrod");
        grod.Set("counter", "five");
        string script = "@addto(counter,3)@get(counter)";
        var result = Dags.Process(script, grod);
        var expected = new List<DagsItem> { new(DagsType.Error, "Error processing command at index 5:\r\nInvalid integer: five\r\n0: @addto(\r\n1: counter\r\n2: ,\r\n3: 3\r\n4: )\r\n5: @get(\r\n6: counter\r\n7: )\r\n") };
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestModTo()
    {
        Grod grod = new("testGrod");
        grod.Set("counter", "20");
        string script = "@modto(counter,6)@get(counter)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "2") }));
    }

    [Test]
    public void TestAdd()
    {
        Grod grod = new("testGrod");
        string script = "@add(5,3)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "8") }));
    }

    [Test]
    public void TestSub()
    {
        Grod grod = new("testGrod");
        string script = "@sub(5,3)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "2") }));
    }

    [Test]
    public void TestMul()
    {
        Grod grod = new("testGrod");
        string script = "@mul(5,3)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "15") }));
    }

    [Test]
    public void TestDiv()
    {
        Grod grod = new("testGrod");
        string script = "@div(6,3)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "2") }));
    }

    [Test]
    public void TestMod()
    {
        Grod grod = new("testGrod");
        string script = "@mod(20,6)";
        var result = Dags.Process(script, grod);
        Assert.That(result, Is.EqualTo(new List<DagsItem> { new(DagsType.Internal, "2") }));
    }
}
