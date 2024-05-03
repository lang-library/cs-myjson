using System;
using static MyJson.MyData;
namespace Main;


class MyClass
{
    public int abc = 123;
    public static int Add2(int x, int y)
    {
        return x + y;
    }
}
static class Program
{
    [STAThread]
    static void Main(string[] originalArgs)
    {
        //StrictParsing = true;
        var o1 = FromJson("""
            { "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789": 123, "b": /* commentハロー© */ "ハロー©"
            // line
            }
            """);
        Echo(o1, "o1");
        var o2 = FromJson("""
            { "a": "abc" }
            """);
        Echo(o2, "o2");
        var o3 = FromJson("""
            [11, /*c1*/22, /*c2*/true, /*c3*/false, /*c4*/null]
            """);
        Echo(o3, "o3");
        var o4 = FromJson("""
            { a: 123, "b": "ハロー©" }
            """);
        Echo(o4, "o1");
    }
}