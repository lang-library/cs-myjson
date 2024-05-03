using MyJson;
//using static MyJson.MyData;
using Xunit;
using Xunit.Abstractions;

public class DynamicTest
{
    private readonly ITestOutputHelper Out;
    public DynamicTest(ITestOutputHelper testOutputHelper)
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
        MyData d1 = MyData.FromObject(new { x = 123 });
        Print((double)d1["x"]);
        Print((float)d1["x"]);
        Print((decimal)d1["x"]);
        Print((int)d1["x"]);
        Print((uint)d1["x"]);
        Print((long)d1["x"]);
        Print((ulong)d1["x"]);
        Print((bool)d1["x"]);
        Print((byte)d1["x"]);
    }
}
