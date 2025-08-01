using System.Runtime.InteropServices;
using DotnetFmt;

namespace tests;

public class FileTests
{
    [Fact]
    public void HelloWorld()
    {
        var src = """
        using System;

        namespace HelloWorld { class Program { static void Main() { Console.WriteLine("Hello, World!"); } } }
        """;
        var expected = """

        using System;

        namespace HelloWorld
        {
            class Program
            {
                static void Main()
                {
                    Console.WriteLine("Hello, World!");
                }
            }
        }

        """;

        Assert.Equal(expected, Fmt.Format(src));
    }

    [Fact]
    public void NonStaticMethod()
    {
        var src = """
        using System;

        namespace TestNamespace { class TestClass { public void DoSomething() { Console.WriteLine("Non-static method"); } } }
        """;
        var expected = """

        using System;

        namespace TestNamespace
        {
            class TestClass
            {
                public void DoSomething()
                {
                    Console.WriteLine("Non-static method");
                }
            }
        }

        """;

        Assert.Equal(expected, Fmt.Format(src));
    }

    [Fact]
    public void StructDeclaration()
    {
        var src = """
        using System;

        namespace TestNamespace { public struct Point { public int X; public int Y; public Point(int x, int y) { X = x; Y = y; } } }
        """;
        var expected = """

        using System;

        namespace TestNamespace
        {
            public struct Point
            {
                public int X;
                public int Y;

                public Point(int x, int y)
                {
                    X = x;
                    Y = y;
                }
            }
        }

        """;

        Assert.Equal(expected, Fmt.Format(src));
    }
}
