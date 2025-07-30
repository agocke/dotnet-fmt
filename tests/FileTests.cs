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
}
