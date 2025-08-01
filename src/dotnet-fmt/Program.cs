using DotnetFmt;

var input = "using\t\tSystem;\n\n\nnamespace\t\tTestNamespace\n{\n\tclass\tTestClass\n\t{\n\t\tpublic\tvoid\tDoSomething()\n\t\t{\n\t\t\tConsole.WriteLine(\"Tabs and newlines\");\n\t\t}\n\t}\n}";

Console.WriteLine("=== INPUT (with visible whitespace) ===");
Console.WriteLine(input.Replace("\t", "[TAB]").Replace("\n", "[NEWLINE]\n"));
Console.WriteLine("\n=== FORMATTED OUTPUT ===");
var result = Fmt.Format(input);
Console.WriteLine(result);
Console.WriteLine("=== END ===");
