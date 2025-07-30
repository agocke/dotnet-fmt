
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotnetFmt;

internal sealed class Formatter : CSharpSyntaxVisitor
{
    const string Indent = "    "; // 4 spaces for indentation

    private readonly StringBuilder _sb;
    private int _indentLevel;

    private Formatter(StringBuilder sb)
    {
        _sb = sb;
    }

    public static bool Format(CSharpSyntaxTree tree, StringBuilder sb)
    {
        var formatter = new Formatter(sb);
        formatter.Visit(tree.GetRoot());
        return true;
    }

    public override void DefaultVisit(SyntaxNode node)
    {
        _sb.Append(node.ToFullString());
    }

    public override void VisitCompilationUnit(CompilationUnitSyntax node)
    {
        if (node.Externs.Count > 0)
        {
            AppendLine();

            bool first = true;
            foreach (var @extern in node.Externs)
            {
                if (!first)
                {
                    AppendLine();
                }
                VisitExternAliasDirective(@extern);
                first = false;
            }
        }

        if (node.Usings.Count > 0)
        {
            AppendLine();

            // Separate usings into four groups:
            // 1. System.*
            // 2. Other namespaces
            // 3. Static usings
            // 4. Using aliases

            var systemUsings = new List<UsingDirectiveSyntax>();
            var otherUsings = new List<UsingDirectiveSyntax>();
            var staticUsings = new List<UsingDirectiveSyntax>();
            var aliasUsings = new List<UsingDirectiveSyntax>();

            foreach (var usingDirective in node.Usings)
            {
                if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                {
                    staticUsings.Add(usingDirective);
                }
                else if (usingDirective.Alias != null)
                {
                    aliasUsings.Add(usingDirective);
                }
                else if (usingDirective.Name?.ToString().StartsWith("System.", StringComparison.OrdinalIgnoreCase) == true)
                {
                    systemUsings.Add(usingDirective);
                }
                else
                {
                    otherUsings.Add(usingDirective);
                }
            }

            // Sort the usings
            Comparison<UsingDirectiveSyntax> nameComparison = (a, b) =>
            {
                return string.Compare(a.Name?.ToString(), b.Name?.ToString(), StringComparison.OrdinalIgnoreCase);
            };
            systemUsings.Sort(nameComparison);
            otherUsings.Sort(nameComparison);
            staticUsings.Sort(nameComparison);
            aliasUsings.Sort(nameComparison);

            void WriteUsings(List<UsingDirectiveSyntax> usings)
            {
                foreach (var usingDirective in usings)
                {
                    _sb.AppendLine(usingDirective.ToFullString().TrimEnd());
                }
            }

            WriteUsings(systemUsings);
            WriteUsings(otherUsings);
            WriteUsings(staticUsings);
            WriteUsings(aliasUsings);
        }

        AppendLine();
        FormatMembers(node.Members);
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        AppendLine($"namespace {node.Name.ToString()}");
        AppendLine("{");
        _indentLevel++;
        FormatMembers(node.Members);
        _indentLevel--;
        AppendLine("}");
    }

    private void FormatMembers(SyntaxList<MemberDeclarationSyntax> members)
    {
        bool first = true;
        foreach (var member in members)
        {
            if (!first)
            {
                AppendLine();
            }
            first = false;
            Visit(member);
        }
    }
}