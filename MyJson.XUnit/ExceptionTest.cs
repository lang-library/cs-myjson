using MyJson;
//using static MyJson.MyData;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using System.Security.Authentication;

public class ExceptionTest
{
    private readonly ITestOutputHelper Out;
    public ExceptionTest(ITestOutputHelper testOutputHelper)
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
        var exception1 = Assert.Throws<ArgumentException>(() => { new MyNumber(null); });
        Assert.Equal("Argument is null", exception1.Message);
        var exception2 = Assert.Throws<ArgumentException>(() => { new MyNumber("abc"); });
        Assert.Equal("Argument is not numeric: System.String", exception2.Message);
    }
}
