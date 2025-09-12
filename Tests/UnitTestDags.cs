using Grif;
using System.Text;
using static Grif.Common;

namespace Tests;

public class UnitTestDags
{
    private readonly Grod grod = new("base");
    private List<DagsItem> result = [];

    private static string Squash(List<DagsItem> result)
    {
        var sb = new StringBuilder();
        foreach (var item in result)
        {
            if (item.Type == DagsType.Text || item.Type == DagsType.Internal)
            {
                sb.Append(item.Value);
            }
        }
        return sb.ToString();
    }

    [SetUp]
    public void Setup()
    {
        grod.Clear(true);
        result.Clear();
    }

    [Test]
    public void Test_Passing()
    {
        Assert.Pass();
    }

    [Test]
    public void Test_Get()
    {
        var key = "abc";
        var value = "123";
        grod.Set(key, value);
        result = Dags.Process(grod, $"@get({key})");
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Value, Is.EqualTo(value));
    }

    [Test]
    public void Test_Set()
    {
        var key = "abc";
        var value = "123";
        Dags.Process(grod, $"@set({key},{value})");
        result = Dags.Process(grod, $"@get({key})");
        Assert.That(Squash(result), Is.EqualTo(value));
    }

    [Test]
    public void Test_Set_Script()
    {
        var key = "abc";
        var answer = "@comment(\"this is a comment\")";
        var value = "\"" + answer.Replace("\"", "\\\"") + "\"";
        Dags.Process(grod, $"@set({key},{value})");
        result = Dags.Process(grod, $"@get({key})");
        Assert.That(Squash(result), Is.EqualTo(answer));
    }

    [Test]
    public void Test_SetArray()
    {
        var key = "abc";
        var value = "123";
        Dags.Process(grod, $"@setarray({key},2,3,{value})");
        result = Dags.Process(grod, $"@getarray({key},2,3)");
        Assert.That(Squash(result), Is.EqualTo(value));
    }

    [Test]
    public void Test_SetArray_Null()
    {
        var key = "abc";
        var value = "";
        Dags.Process(grod, $"@setarray({key},2,3,{value})");
        result = Dags.Process(grod, $"@getarray({key},2,3)");
        Assert.That(Squash(result), Is.EqualTo(value));
    }

    [Test]
    public void Test_ClearArray()
    {
        var key = "abc";
        var value = "123";
        Dags.Process(grod, $"@setarray({key},2,3,{value})");
        Dags.Process(grod, $"@cleararray({key})");
        result = Dags.Process(grod, $"@getarray({key},2,3)");
        Assert.That(Squash(result), Is.EqualTo(""));
    }

    [Test]
    public void Test_SetList()
    {
        var key = "abc";
        var value = "123";
        Dags.Process(grod, $"@setlist({key},1,{value})");
        result = Dags.Process(grod, $"@getlist({key},1)");
        Assert.That(Squash(result), Is.EqualTo(value));
    }

    [Test]
    public void Test_SetList_Null()
    {
        var key = "abc";
        var value = "";
        Dags.Process(grod, $"@setlist({key},1,{value})");
        result = Dags.Process(grod, $"@getlist({key},1)");
        Assert.That(Squash(result), Is.EqualTo(value));
    }

    [Test]
    public void Test_SetList_TabCRLF()
    {
        var key = "abc";
        var value = "abc\t\r\n123";
        Dags.Process(grod, $"@setlist({key},1,\"{value}\")");
        result = Dags.Process(grod, $"@getlist({key},1)");
        Assert.That(Squash(result), Is.EqualTo(value));
    }

    [Test]
    public void Test_InsertAtList()
    {
        var key = "abc";
        var value = "123";
        Dags.Process(grod, $"@addlist({key},0)");
        Dags.Process(grod, $"@addlist({key},1)");
        Dags.Process(grod, $"@addlist({key},2)");
        Dags.Process(grod, $"@addlist({key},3)");
        Dags.Process(grod, $"@insertatlist({key},1,{value})");
        result = Dags.Process(grod, $"@getlist({key},1)");
        Assert.That(Squash(result), Is.EqualTo(value));
        result = Dags.Process(grod, $"@getlist({key},4)");
        Assert.That(Squash(result), Is.EqualTo("3"));
    }

    [Test]
    public void Test_RemoveAtList()
    {
        var key = "abc";
        var value = "123";
        Dags.Process(grod, $"@setlist({key},3,{value})");
        Dags.Process(grod, $"@removeatlist({key},0)");
        result = Dags.Process(grod, $"@getlist({key},2)");
        Assert.That(Squash(result), Is.EqualTo(value));
    }

    [Test]
    public void Test_Function()
    {
        Dags.Process(grod, "@set(\"@boo\",\"@write(eek!)\")");
        result = Dags.Process(grod, "@boo");
        Assert.That(Squash(result), Is.EqualTo("eek!"));
    }

    [Test]
    public void Test_FunctionParameters()
    {
        Dags.Process(grod, "@set(\"@boo(x)\",\"@write($x)\")");
        result = Dags.Process(grod, "@boo(eek!)");
        Assert.That(Squash(result), Is.EqualTo("eek!"));
    }

    [Test]
    public void Test_Abs()
    {
        result = Dags.Process(grod, "@write(@abs(1))");
        Assert.That(Squash(result), Is.EqualTo("1"));
        result = Dags.Process(grod, "@write(@abs(-1))");
        Assert.That(Squash(result), Is.EqualTo("1"));
    }

    [Test]
    public void Test_Add()
    {
        result = Dags.Process(grod, "@write(@add(1,3))");
        Assert.That(Squash(result), Is.EqualTo("4"));
    }

    [Test]
    public void Test_AddTo()
    {
        result = Dags.Process(grod, "@set(value,12) @addto(value,7) @write(@get(value))");
        Assert.That(Squash(result), Is.EqualTo("19"));
    }

    [Test]
    public void Test_Comment()
    {
        result = Dags.Process(grod, "@comment(\"this is a comment\")");
        Assert.That(Squash(result), Is.EqualTo(""));
    }

    [Test]
    public void Test_Concat()
    {
        result = Dags.Process(grod, "@write(@concat(abc,def,123))");
        Assert.That(Squash(result), Is.EqualTo("abcdef123"));
    }

    [Test]
    public void Test_Debug()
    {
        grod.Set("system.debug", "true");
        result = Dags.Process(grod, "@debug(\"this is a comment\")");
        Assert.That(Squash(result), Is.EqualTo("### this is a comment" + NL));
        result = Dags.Process(grod, "@debug(@add(123,456))");
        Assert.That(Squash(result), Is.EqualTo("### 579" + NL));
        grod.Set("system.debug", "false");
        result = Dags.Process(grod, "@debug(\"this is a comment\")");
        Assert.That(Squash(result), Is.EqualTo(""));
    }

    [Test]
    public void Test_Div()
    {
        result = Dags.Process(grod, "@write(@div(42,6))");
        Assert.That(Squash(result), Is.EqualTo("7"));
    }

    [Test]
    public void Test_DivTo()
    {
        result = Dags.Process(grod, "@set(value,12) @divto(value,3) @write(@get(value))");
        Assert.That(Squash(result), Is.EqualTo("4"));
    }

    [Test]
    public void Test_EQ()
    {
        result = Dags.Process(grod, "@write(@eq(42,6))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@write(@eq(42,42))");
        Assert.That(Squash(result), Is.EqualTo("true"));
    }

    [Test]
    public void Test_Exec()
    {
        result = Dags.Process(grod, "@exec(\"@set(value,23)\") @write(@get(value))");
        Assert.That(Squash(result), Is.EqualTo("23"));
    }

    [Test]
    public void Test_False()
    {
        result = Dags.Process(grod, "@write(@false(\"\"))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@false(0))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@false(1))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@write(@false(abc))");
        Assert.That(Squash(result), Is.EqualTo("false"));
    }

    [Test]
    public void Test_For()
    {
        result = Dags.Process(grod, "@for(x,1,3) @write($x) @endfor");
        Assert.That(Squash(result), Is.EqualTo("123"));
    }

    [Test]
    public void Test_ForEachKey()
    {
        Dags.Process(grod, "@set(value.1,100) @set(value.2,200)");
        result = Dags.Process(grod, "@foreachkey(x,\"value.\") @write($x) @endforeachkey");
        Assert.That(Squash(result), Is.EqualTo("12"));
        result = Dags.Process(grod, "@foreachkey(x,\"value.\") @get(value.$x) @endforeachkey");
        Assert.That(Squash(result), Is.EqualTo("100200"));
    }

    [Test]
    public void Test_ForEachList()
    {
        Dags.Process(grod, "@setlist(value,1,10)");
        Dags.Process(grod, "@setlist(value,2,20)");
        Dags.Process(grod, "@setlist(value,3,30)");
        result = Dags.Process(grod, "@foreachlist(x,value) @write($x) @endforeachlist");
        Assert.That(Squash(result), Is.EqualTo("102030"));
    }

    [Test]
    public void Test_Format()
    {
        result = Dags.Process(grod, "@write(@format(\"{0}-{1}-{2}\",1,2,3))");
        Assert.That(Squash(result), Is.EqualTo("1-2-3"));
        result = Dags.Process(grod, "@write(@format(\"{2}-{1}-{0}\",1,2,3))");
        Assert.That(Squash(result), Is.EqualTo("3-2-1"));
        result = Dags.Process(grod, "@write(@format(\"{0}-{1}-{2}\",1,2))");
        Assert.That(Squash(result), Is.EqualTo("1-2-{2}"));
    }

    [Test]
    public void Test_GE()
    {
        result = Dags.Process(grod, "@write(@ge(42,6))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@ge(42,42))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@ge(1,42))");
        Assert.That(Squash(result), Is.EqualTo("false"));
    }

    [Test]
    public void Test_GetInChannel()
    {
        grod.Set(INCHANNEL, "abc");
        result = Dags.Process(grod, "@write(@getinchannel)");
        Assert.That(Squash(result), Is.EqualTo("abc"));
        result = Dags.Process(grod, "@write(@getinchannel)");
        Assert.That(Squash(result), Is.EqualTo(""));
    }

    [Test]
    public void Test_GetValue()
    {
        Dags.Process(grod, "@set(v1,\"@get(v2)\") @set(v2,123)");
        result = Dags.Process(grod, "@get(v1)");
        Assert.That(Squash(result), Is.EqualTo("@get(v2)"));
        result = Dags.Process(grod, "@write(@getvalue(v1))");
        Assert.That(Squash(result), Is.EqualTo("123"));
    }

    [Test]
    public void Test_GoLabel()
    {
        result = Dags.Process(grod, "@write(abc) @golabel(1) @write(def) @label(1) @write(xyz)");
        Assert.That(Squash(result), Is.EqualTo("abcxyz"));
    }

    [Test]
    public void Test_GT()
    {
        result = Dags.Process(grod, "@write(@gt(42,6))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@gt(42,42))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@write(@gt(1,42))");
        Assert.That(Squash(result), Is.EqualTo("false"));
    }

    [Test]
    public void Test_If()
    {
        result = Dags.Process(grod, "@if true @then @write(abc) @else @write(def) @endif");
        Assert.That(Squash(result), Is.EqualTo("abc"));
        result = Dags.Process(grod, "@if false @then @write(abc) @else @write(def) @endif");
        Assert.That(Squash(result), Is.EqualTo("def"));
        result = Dags.Process(grod, "@if true @or false @then @write(abc) @else @write(def) @endif");
        Assert.That(Squash(result), Is.EqualTo("abc"));
        result = Dags.Process(grod, "@if true @and false @then @write(abc) @else @write(def) @endif");
        Assert.That(Squash(result), Is.EqualTo("def"));
        result = Dags.Process(grod, "@if null @then @write(abc) @else @write(def) @endif");
        Assert.That(Squash(result), Is.EqualTo("def"));
    }

    [Test]
    public void Test_IsBool()
    {
        result = Dags.Process(grod, "@write(@isbool(0))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@isbool(1))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@isbool(notboolean))");
        Assert.That(Squash(result), Is.EqualTo("false"));
    }

    [Test]
    public void Test_Null()
    {
        result = Dags.Process(grod, "@write(@null(null))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@null(abc))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@write(@null(@get(value)))");
        Assert.That(Squash(result), Is.EqualTo("true"));
    }

    [Test]
    public void Test_Exists()
    {
        result = Dags.Process(grod, "@write(@exists(test.value))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@set(test.value,null) @write(@exists(test.value))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@set(test.value,abc) @write(@exists(test.value))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@set(test.value,\"\") @write(@exists(test.value))");
        Assert.That(Squash(result), Is.EqualTo("false"));
    }

    [Test]
    public void Test_IsScript()
    {
        result = Dags.Process(grod, "@set(test.value,abc) @write(@isscript(test.value))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@set(test.value,\"@get(value)\") @write(@isscript(test.value))");
        Assert.That(Squash(result), Is.EqualTo("true"));
    }

    [Test]
    public void Test_LE()
    {
        result = Dags.Process(grod, "@write(@le(42,6))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@write(@le(42,42))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@le(1,42))");
        Assert.That(Squash(result), Is.EqualTo("true"));
    }

    [Test]
    public void Test_Lower()
    {
        result = Dags.Process(grod, "@lower(ABC)");
        Assert.That(Squash(result), Is.EqualTo("abc"));
        result = Dags.Process(grod, "@lower(DEF)");
        Assert.That(Squash(result), Is.EqualTo("def"));
    }

    [Test]
    public void Test_LT()
    {
        result = Dags.Process(grod, "@write(@lt(42,6))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@write(@lt(42,42))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@write(@lt(1,42))");
        Assert.That(Squash(result), Is.EqualTo("true"));
    }

    [Test]
    public void Test_Mod()
    {
        result = Dags.Process(grod, "@write(@mod(13,4))");
        Assert.That(Squash(result), Is.EqualTo("1"));
        result = Dags.Process(grod, "@write(@mod(12,4))");
        Assert.That(Squash(result), Is.EqualTo("0"));
    }

    [Test]
    public void Test_ModTo()
    {
        result = Dags.Process(grod, "@set(value,13) @modto(value,4) @write(@get(value))");
        Assert.That(Squash(result), Is.EqualTo("1"));
    }

    [Test]
    public void Test_Msg()
    {
        result = Dags.Process(grod, "@set(value,abcdef) @msg(value)");
        Assert.That(Squash(result), Is.EqualTo("abcdef" + NL));
    }

    [Test]
    public void Test_Mul()
    {
        result = Dags.Process(grod, "@write(@mul(3,4))");
        Assert.That(Squash(result), Is.EqualTo("12"));
    }

    [Test]
    public void Test_MulTo()
    {
        result = Dags.Process(grod, "@set(value,3) @multo(value,4) @write(@get(value))");
        Assert.That(Squash(result), Is.EqualTo("12"));
    }

    [Test]
    public void Test_NE()
    {
        result = Dags.Process(grod, "@write(@ne(42,6))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@ne(42,42))");
        Assert.That(Squash(result), Is.EqualTo("false"));
    }

    [Test]
    public void Test_Neg()
    {
        result = Dags.Process(grod, "@write(@neg(3))");
        Assert.That(Squash(result), Is.EqualTo("-3"));
    }

    [Test]
    public void Test_NegTo()
    {
        result = Dags.Process(grod, "@set(value,3) @negto(value) @write(@get(value))");
        Assert.That(Squash(result), Is.EqualTo("-3"));
    }

    [Test]
    public void Test_NL()
    {
        result = Dags.Process(grod, "@nl");
        Assert.That(Squash(result), Is.EqualTo(NL));
    }

    [Test]
    public void Test_Not()
    {
        result = Dags.Process(grod, "@if @not false @then @write(abc) @else @write(def) @endif");
        Assert.That(Squash(result), Is.EqualTo("abc"));
    }

    [Test]
    public void Test_Or()
    {
        result = Dags.Process(grod, "@if true @or false @then @write(abc) @else @write(def) @endif");
        Assert.That(Squash(result), Is.EqualTo("abc"));
    }

    [Test]
    public void Test_Rand()
    {
        result = Dags.Process(grod, "@rand(30)");
        Assert.That(result[0].Value == "true" || result[0].Value == "false");
    }

    [Test]
    public void Test_Replace()
    {
        result = Dags.Process(grod, "@write(@replace(abcdef,d,x))");
        Assert.That(Squash(result), Is.EqualTo("abcxef"));
    }

    [Test]
    public void Test_Rnd()
    {
        result = Dags.Process(grod, "@set(value,@rnd(20))");
        result = Dags.Process(grod, "@get(value)");
        var r1 = int.Parse(Squash(result));
        Assert.That(r1 >= 0 && r1 < 20);
    }

    [Test]
    public void Test_Script()
    {
        result = Dags.Process(grod, "@set(script1,\"@write(abc)\")");
        result = Dags.Process(grod, "@script(script1)");
        Assert.That(Squash(result), Is.EqualTo("abc"));
    }

    [Test]
    public void Test_SetOutChannel()
    {
        result = Dags.Process(grod, "@setoutchannel(abc)");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0].Type, Is.EqualTo(DagsType.OutChannel));
            Assert.That(result[0].Value, Is.EqualTo("abc"));
        }
    }

    [Test]
    public void Test_Sub()
    {
        result = Dags.Process(grod, "@write(@sub(1,3))");
        Assert.That(Squash(result), Is.EqualTo("-2"));
    }

    [Test]
    public void Test_Substring()
    {
        result = Dags.Process(grod, "@write(@substring(abcdef,1,4))");
        Assert.That(Squash(result), Is.EqualTo("bcde"));
    }

    [Test]
    public void Test_SubTo()
    {
        result = Dags.Process(grod, "@set(value,12) @subto(value,7) @write(@get(value))");
        Assert.That(Squash(result), Is.EqualTo("5"));
    }

    [Test]
    public void Test_Swap()
    {
        result = Dags.Process(grod, "@set(value1,abc) @set(value2,def) @swap(value1,value2) @write(@get(value1),@get(value2))");
        Assert.That(Squash(result), Is.EqualTo("defabc"));
    }

    [Test]
    public void Test_Trim()
    {
        result = Dags.Process(grod, "@set(value,\"   abc   \") @write(@trim(@get(value)))");
        Assert.That(Squash(result), Is.EqualTo("abc"));
    }

    [Test]
    public void Test_True()
    {
        result = Dags.Process(grod, "@write(@true(0))");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, "@write(@true(1))");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, "@write(@true(abc))");
        Assert.That(Squash(result), Is.EqualTo("false"));
    }

    [Test]
    public void Test_Upper()
    {
        result = Dags.Process(grod, "@write(@upper(abc))");
        Assert.That(Squash(result), Is.EqualTo("ABC"));
    }

    [Test]
    public void Test_PrettyScript()
    {
        var script = "@if @eq(@get(value),0) @then @write(\"zero\") @else @write(\"not zero\") @endif";
        var expected = "@if @eq(@get(value),0) @then\r\n\t@write(\"zero\")\r\n@else\r\n\t@write(\"not zero\")\r\n@endif";
        var actual = Dags.PrettyScript(script);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Test_PrettyScript_Min()
    {
        var script = "@if@eq(@get(value),0)@then@write(\"zero\")@else@write(\"not zero\")@endif";
        var expected = "@if @eq(@get(value),0) @then\r\n\t@write(\"zero\")\r\n@else\r\n\t@write(\"not zero\")\r\n@endif";
        var actual = Dags.PrettyScript(script);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Test_PrettyScript_Same()
    {
        var script = "@write(\"hello \\\"wonderful\\\" world.\")";
        var actual = Dags.PrettyScript(script);
        Assert.That(actual, Is.EqualTo(script));
    }

    [Test]
    public void Test_IfThenNoStatements()
    {
        var script = "@if @eq(1,1) @then @endif";
        result = Dags.Process(grod, script);
        Assert.That(Squash(result), Is.EqualTo(""));
    }

    [Test]
    public void Test_IfThenElseNoStatements()
    {
        var script = "@if @eq(1,2) @then @write(abc) @else @endif";
        result = Dags.Process(grod, script);
        Assert.That(Squash(result), Is.EqualTo(""));
    }

    [Test]
    public void Test_Return()
    {
        var key = "abc";
        var value1 = "123";
        var value2 = "456";
        result = Dags.Process(grod, $"@set({key},{value1}) @return @set({key},{value2})");
        Assert.That(result, Is.Empty);
        result = Dags.Process(grod, $"@get({key})");
        Assert.That(Squash(result), Is.EqualTo(value1));
    }

    [Test]
    public void Test_AddList()
    {
        var key = "abc";
        var value1 = "123";
        var value2 = "456";
        Dags.Process(grod, $"@addlist({key},{value1}) @addlist({key},{value2})");
        result = Dags.Process(grod, $"@get({key})");
        Assert.That(Squash(result), Is.EqualTo(value1 + ',' + value2));
    }

    [Test]
    public void Test_ClearList()
    {
        var key = "abc";
        var value1 = "123";
        var value2 = "456";
        Dags.Process(grod, $"@addlist({key},{value1}) @addlist({key},{value2})");
        Dags.Process(grod, $"@clearlist({key})");
        result = Dags.Process(grod, $"@get({key})");
        Assert.That(Squash(result), Is.EqualTo(""));
    }

    [Test]
    public void Test_GetArray()
    {
        var key = "abc";
        var value = "123";
        Dags.Process(grod, $"@setarray({key},2,3,{value})");
        result = Dags.Process(grod, $"@getarray({key},2,3)");
        Assert.That(Squash(result), Is.EqualTo(value));
    }

    [Test]
    public void Test_GetList()
    {
        var key = "abc";
        var value1 = "123";
        var value2 = "456";
        Dags.Process(grod, $"@addlist({key},{value1}) @addlist({key},{value2})");
        result = Dags.Process(grod, $"@getlist({key},0)");
        Assert.That(Squash(result), Is.EqualTo(value1));
        result = Dags.Process(grod, $"@getlist({key},1)");
        Assert.That(Squash(result), Is.EqualTo(value2));
        result = Dags.Process(grod, $"@getlist({key},2)");
        Assert.That(Squash(result), Is.EqualTo(""));
    }

    [Test]
    public void Test_IsNumber()
    {
        var value1 = "123";
        var value2 = "abc";
        var value3 = "";
        result = Dags.Process(grod, $"@isnumber({value1})");
        Assert.That(Squash(result), Is.EqualTo("true"));
        result = Dags.Process(grod, $"@isnumber({value2})");
        Assert.That(Squash(result), Is.EqualTo("false"));
        result = Dags.Process(grod, $"@isnumber({value3})");
        Assert.That(result[0].Type, Is.EqualTo(DagsType.Error));
    }

    [Test]
    public void Test_ListLength()
    {
        var key = "abc";
        var value1 = "123";
        var value2 = "456";
        Dags.Process(grod, $"@addlist({key},{value1}) @addlist({key},{value2})");
        result = Dags.Process(grod, $"@listlength({key})");
        Assert.That(Squash(result), Is.EqualTo("2"));
    }

    [Test]
    public void Test_Write()
    {
        var value1 = "123";
        result = Dags.Process(grod, $"@write({value1})");
        Assert.That(Squash(result), Is.EqualTo(value1));
    }

    [Test]
    public void Test_WriteLine()
    {
        var value1 = "123";
        result = Dags.Process(grod, $"@writeline({value1})");
        // @writeline result ends with two characters, '\' and 'n'.
        // This is the expected behavior. See Test_NL().
        Assert.That(Squash(result), Is.EqualTo(value1 + NL));
    }

    [Test]
    public void Test_GetBit()
    {
        result = Dags.Process(grod, $"@getbit(4,2)");
        Assert.That(Squash(result), Is.EqualTo("1"));
        result = Dags.Process(grod, $"@getbit(8,2)");
        Assert.That(Squash(result), Is.EqualTo("0"));
        result = Dags.Process(grod, $"@getbit(1073741824,30)");
        Assert.That(Squash(result), Is.EqualTo("1"));
    }

    [Test]
    public void Test_SetBit()
    {
        result = Dags.Process(grod, $"@setbit(0,2)");
        Assert.That(Squash(result), Is.EqualTo("4"));
        result = Dags.Process(grod, $"@setbit(0,0)");
        Assert.That(Squash(result), Is.EqualTo("1"));
        result = Dags.Process(grod, $"@setbit(0,30)");
        Assert.That(Squash(result), Is.EqualTo("1073741824"));
    }

    [Test]
    public void Test_ClearBit()
    {
        result = Dags.Process(grod, $"@clearbit(7,2)");
        Assert.That(Squash(result), Is.EqualTo("3"));
        result = Dags.Process(grod, $"@clearbit(7,0)");
        Assert.That(Squash(result), Is.EqualTo("6"));
        result = Dags.Process(grod, $"@clearbit(1073741824,30)");
        Assert.That(Squash(result), Is.EqualTo("0"));
    }

    [Test]
    public void Test_BitwiseAnd()
    {
        result = Dags.Process(grod, $"@bitwiseand(7,2)");
        Assert.That(Squash(result), Is.EqualTo("2"));
        result = Dags.Process(grod, $"@bitwiseand(8,2)");
        Assert.That(Squash(result), Is.EqualTo("0"));
    }

    [Test]
    public void Test_BitwiseOr()
    {
        result = Dags.Process(grod, $"@bitwiseor(7,2)");
        Assert.That(Squash(result), Is.EqualTo("7"));
        result = Dags.Process(grod, $"@bitwiseor(8,2)");
        Assert.That(Squash(result), Is.EqualTo("10"));
    }

    [Test]
    public void Test_BitwiseXor()
    {
        result = Dags.Process(grod, $"@bitwisexor(7,2)");
        Assert.That(Squash(result), Is.EqualTo("5"));
        result = Dags.Process(grod, $"@bitwisexor(8,7)");
        Assert.That(Squash(result), Is.EqualTo("15"));
    }

    [Test]
    public void Test_ToBinary()
    {
        result = Dags.Process(grod, $"@tobinary(7)");
        Assert.That(Squash(result), Is.EqualTo("111"));
    }

    [Test]
    public void Test_ToInteger()
    {
        result = Dags.Process(grod, $"@tointeger(111)");
        Assert.That(Squash(result), Is.EqualTo("7"));
    }
}
