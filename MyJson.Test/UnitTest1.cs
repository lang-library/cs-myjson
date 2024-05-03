namespace MyData.Test;

using MyJson;
using static MyJson.MyData;
using System;

public class Tests
{
    MyTool tool = new MyTool();
    [SetUp]
    public void Setup()
    {
        tool.Echo("Setup() called");
        MyData.ClearAllSettings();
    }

    [Test]
    public void Test0A()
    {
        var myTool = new MyTool()
            .SetDecimalAsString(false) // decimal を文字列に変換
            .SetForceASCII(true)
            .SetNumberAsDecimal(false);
        string json = ObjectToJson(myTool, "helloハロー©");
        myTool.Echo(json);
        Console.WriteLine($"json={json}");
        Console.WriteLine(json == @"""hello\u30CF\u30ED\u30FC\u00A9""");
        Assert.That(json, Is.EqualTo(@"""hello\u30CF\u30ED\u30FC\u00A9""")); // 文字列のJSON
    }

    [Test]
    public void Test1A()
    {
        var myTool = new MyTool()
            .SetDecimalAsString(true) // decimal を文字列に変換
            .SetForceASCII(false)
            .SetNumberAsDecimal(false); // オブジェクトの読み込み時には、false でも decimal のまま読み込まれる
        string json = myTool.ToJson(12345678901234567890123456789m);
        tool.Echo(json, "json");
        Assert.That(json, Is.EqualTo("\"12345678901234567890123456789\"")); // 文字列のJSON
    }
    [Test]
    public void Test1B()
    {
        var myTool = new MyTool()
            .SetDecimalAsString(true)
            .SetForceASCII(false)
            .SetNumberAsDecimal(false);
        string json = ReformatJson(myTool, "12345678901234567890123456789");
        tool.Echo(json, "json");
        Assert.That(json, Is.EqualTo("""1.23456789012346E+28"""));
    }
    [Test]
    public void Test1C()
    {
        var myTool = new MyTool()
            .SetShowDetail(true)
            .SetDecimalAsString(true)
            .SetForceASCII(false)
            .SetNumberAsDecimal(true);
        var o = ObjectToObject(myTool, 12345678901234567890123456789m);
        tool.Echo(o, "o");
        Assert.That(o, Is.EqualTo("12345678901234567890123456789")); // 文字列
        string p = myTool.ToPrintable(o);
        Assert.That(p, Is.EqualTo("`12345678901234567890123456789`"));
    }

    [Test]
    public void Test2()
    {
        var myTool = new MyTool()
            .SetShowDetail(true)
            .SetDecimalAsString(true)
            .SetForceASCII(false)
            .SetNumberAsDecimal(true);
        string json = ObjectToJson(tool, new { a = true });
        Echo(json, "json");
        Assert.That(json, Is.EqualTo("""{"a":true}"""));
        string p = myTool.ToPrintable(new { a = true });
        Assert.That(p, Is.EqualTo("<<>f__AnonymousType0> {\n  \"a\" : true\n}"));
    }
    [Test]
    public void Test3()
    {
        string json = ObjectToJson(tool, new object[] { true, false, null });
        tool.Echo(json, "json");
        Assert.That(json, Is.EqualTo("""[true,false,null]"""));
        MyData array = MyData.FromJson(json);
        tool.Echo($"array.IsArray: {array.IsArray}");
        Assert.True(array.IsArray);
        var array2 = array.AsStringArray;
        CheckObjectJson(array2, """["True","False","null"]""");
    }
    [Test]
    public void Test4()
    {
        string json = ObjectToJson(tool, 123);
        Assert.That(json, Is.EqualTo("""123"""));
        MyData array = MyData.FromJson(json);
        Assert.False(array.IsArray);
        var array2 = array.AsStringArray;
        CheckObjectJson(array2, """null""");
    }
    [Test]
    public void Test5()
    {
        var n1 = new MyNumber(777);
        CheckObjectJson(n1, "777");
        Assert.That(() => new MyNumber(null), Throws.TypeOf<ArgumentException>()
        .With.Message.EqualTo("Argument is null"));
        Assert.That(() => new MyNumber("abc"), Throws.TypeOf<ArgumentException>()
        .With.Message.EqualTo("Argument is not numeric: System.String"));
    }
    [Test]
    public void Test6()
    {
        string json = ObjectToJson(tool, new { a = 123 }, true);
        Assert.That(json, Is.EqualTo("{\n  \"a\" : 123\n}"));
    }
    protected string ObjectToJson(MyTool myTool, object x, bool indent = false)
    {
        MyData mj = myTool.FromObject(x);
        string json = myTool.ToJson(mj, indent);
        Console.WriteLine(json);
        return json;
    }
    protected dynamic ObjectToObject(MyTool myTool, object x)
    {
        MyData mj = myTool.FromObject(x);
        return myTool.ToObject(mj);
    }
    protected string ReformatJson(MyTool myTool, string json, bool indent = false)
    {
        MyData mj = myTool.FromJson(json);
        return myTool.ToJson(mj, indent);
    }
    protected void CheckObjectJson(object x, string expectedJson)
    {
        string actualJson = ObjectToJson(tool, x);
        Assert.That(actualJson, Is.EqualTo(expectedJson));
    }
}