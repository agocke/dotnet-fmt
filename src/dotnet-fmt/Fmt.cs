
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotnetFmt;

public static class Fmt
{
    public static string? Format(string input)
    {
        var tree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(input);
        if (tree.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
        {
            return null; // Return null if there are syntax errors
        }

        return Formatter.Format(tree);
    }
}