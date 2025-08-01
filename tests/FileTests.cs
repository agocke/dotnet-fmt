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

    [Fact]
    public void MultipleModifiers()
    {
        var src = """
        using System;

        namespace TestNamespace { class TestClass { static readonly public int StaticField; public static void StaticMethod() { } private static readonly string privateField = "test"; protected internal virtual async Task<string> ComplexMethod() { return "test"; } } }
        """;
        var expected = """

        using System;

        namespace TestNamespace
        {
            class TestClass
            {
                public static readonly int StaticField;

                public static void StaticMethod()
                {
                }

                private static readonly string privateField = "test";

                protected internal virtual async Task<string> ComplexMethod()
                {
                    return "test";
                }
            }
        }

        """;

        Assert.Equal(expected, Fmt.Format(src));
    }

    [Fact]
    public void RandomWhitespace()
    {
        var src = """
        using    System;

        namespace    TestNamespace   {   class    TestClass   {   public    void    DoSomething   (   )   {   Console   .   WriteLine   (   "Random whitespace"   )   ;   }   }   }
        """;
        var expected = """

        using System;

        namespace TestNamespace
        {
            class TestClass
            {
                public void DoSomething()
                {
                    Console.WriteLine("Random whitespace");
                }
            }
        }

        """;

        Assert.Equal(expected, Fmt.Format(src));
    }

    [Fact]
    public void TabsAndNewlines()
    {
        var src = "using\t\tSystem;\n\n\nnamespace\t\tTestNamespace\n{\n\tclass\tTestClass\n\t{\n\t\tpublic\tvoid\tDoSomething()\n\t\t{\n\t\t\tConsole.WriteLine(\"Tabs and newlines\");\n\t\t}\n\t}\n}";
        var expected = """

        using System;

        namespace TestNamespace
        {
            class TestClass
            {
                public void DoSomething()
                {
                    Console.WriteLine("Tabs and newlines");
                }
            }
        }

        """;

        Assert.Equal(expected, Fmt.Format(src));
    }

    [Fact]
    public void MixedWhitespaceCharacters()
    {
        // Test with mixed whitespace characters: spaces, tabs, newlines, and carriage returns
        var src = "using \t \t System;\r\n\r\n\r\n\t \t namespace \t TestNamespace \r\n{\r\n \t class \t TestClass\r\n \t {\r\n \t \t public \t void \t DoSomething( \t )\r\n \t \t {\r\n \t \t \t Console . WriteLine ( \"Mixed whitespace\" ) ;\r\n \t \t }\r\n \t }\r\n}";
        var expected = """

        using System;

        namespace TestNamespace
        {
            class TestClass
            {
                public void DoSomething()
                {
                    Console.WriteLine("Mixed whitespace");
                }
            }
        }

        """;

        Assert.Equal(expected, Fmt.Format(src));
    }
}
