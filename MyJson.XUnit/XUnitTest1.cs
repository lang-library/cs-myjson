using MyJson;
//using static MyJson.MyData;
using Xunit;
using Xunit.Abstractions;

public class XUnitTest1
{
    private readonly ITestOutputHelper Out;
    public XUnitTest1(ITestOutputHelper testOutputHelper)
    {
        Out = testOutputHelper;
        MyData.ClearAllSettings();
        Print("Setup() called");
    }
    private void Print(object x, string title = null)
    {
        Out.WriteLine(new MyTool().ToPrintable(x, title));
    }
    [Fact]
    public void Test01()
    {
        MyData
            .SetDecimalAsString(true) // decimal を文字列に変換
            .SetForceASCII(false)
            .SetNumberAsDecimal(false)
            //.SetXUnitOutput(Out)
            ;
        string json = MyData.ToJson(12345678901234567890123456789m);
        Print(json, "json");
        Assert.Equal("\"12345678901234567890123456789\"", json); // 文字列のJSON
    }
    [Fact]
    public void Test02()
    {
        MyData
            .SetForceASCII(true)
            ;
        string json = MyData.ToJson("helloハロー©");
        Print(json, "json");
        Print(json == @"""hello\u30CF\u30ED\u30FC\u00A9""");
        Assert.Equal(@"""hello\u30CF\u30ED\u30FC\u00A9""", json);
    }
    [Fact]
    public void Test03()
    {
        MyData
            .SetForceASCII(true)
            ;
        string json = MyData.FormatJson(@"""helloハロー©""");
        Print(json, "json");
        Print(json == @"""hello\u30CF\u30ED\u30FC\u00A9""");
        Assert.Equal(@"""hello\u30CF\u30ED\u30FC\u00A9""", json);
    }
    [Fact]
    public void Test04()
    {
        //MyData
        //    .SetForceASCII(true)
        //    ;
        string json = MyData.FormatJson("\"hello\\nハロー©\"");
        Print(json, "json");
        Assert.Equal("\"hello\\nハロー©\"", json);
    }
    [Fact]
    public void Test05()
    {
        string orig = """
            "hello\nハロー©"
            """;
        Print($"<{orig}>", "orig");
        string json = MyData.FormatJson(orig);
        Print($"<{json}>", "json");
        Assert.Equal("\"hello\\nハロー©\"", json);
    }
    [Fact]
    public void Test06()
    {
        string orig = """
            {msg: "hello\nハロー©"}
            """;
        Print($"<{orig}>", "orig");
        string json = MyData.FormatJson(orig);
        Print($"<{json}>", "json");
        Assert.Equal("""
            {"msg":"hello\nハロー©"}
            """, json);
    }
    [Fact]
    public void Test07()
    {
        string orig = """
            {msg: 123.45}
            """;
        Print($"<{orig}>", "orig");
        string json = MyData.FormatJson(orig);
        Print($"<{json}>", "json");
        Assert.Equal("""
            {"msg":123.45}
            """, json);
    }
    [Fact]
    public void Test08()
    {
        string orig = """
            12345678901234567890123456789
            """;
        Print($"<{orig}>", "orig");
        string json = MyData.FormatJson(orig);
        Print($"<{json}>", "json");
        Assert.Equal("""
            1.23456789012346E+28
            """, json);
    }
    [Fact]
    public void Test09()
    {
        MyData.SetNumberAsDecimal(true);
        string orig = """
            12345678901234567890123456789
            """;
        Print($"<{orig}>", "orig");
        string json = MyData.FormatJson(orig);
        Print($"<{json}>", "json");
        Assert.Equal("""
            12345678901234567890123456789
            """, json);
    }
}
