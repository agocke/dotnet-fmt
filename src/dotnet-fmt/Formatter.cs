
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotnetFmt;

internal sealed class Formatter : CSharpSyntaxVisitor
{
    private IndentingBuilder _sb;

    private Formatter(IndentingBuilder sb)
    {
        _sb = sb;
    }

    public static string Format(CSharpSyntaxTree tree)
    {
        var sb = new IndentingBuilder();
        var formatter = new Formatter(sb);
        formatter.Visit(tree.GetRoot());
        return sb.ToString();
    }

    public override void DefaultVisit(SyntaxNode node)
    {
        _sb.Append(node.ToFullString());
    }

    public override void VisitCompilationUnit(CompilationUnitSyntax node)
    {
        if (node.Externs.Count > 0)
        {
            _sb.AppendLine();

            bool first = true;
            foreach (var @extern in node.Externs)
            {
                if (!first)
                {
                    _sb.AppendLine();
                }
                VisitExternAliasDirective(@extern);
                first = false;
            }
        }

        if (node.Usings.Count > 0)
        {
            _sb.AppendLine();

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

        _sb.AppendLine();
        FormatMembers(node.Members);
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        _sb.AppendLine($"namespace {node.Name.ToString()}");
        _sb.AppendLine("{");
        _sb.Indent();
        FormatMembers(node.Members);
        _sb.Dedent();
        _sb.AppendLine("}");
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var modifiers = node.Modifiers.ToFullString().Trim();
        var className = node.Identifier.Text;
        
        var declaration = string.IsNullOrEmpty(modifiers)
            ? $"class {className}"
            : $"{modifiers} class {className}";
            
        _sb.AppendLine(declaration);
        _sb.AppendLine("{");
        _sb.Indent();
        FormatMembers(node.Members);
        _sb.Dedent();
        _sb.AppendLine("}");
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var modifiers = node.Modifiers.ToFullString().Trim();
        var structName = node.Identifier.Text;
        
        var declaration = string.IsNullOrEmpty(modifiers)
            ? $"struct {structName}"
            : $"{modifiers} struct {structName}";
            
        _sb.AppendLine(declaration);
        _sb.AppendLine("{");
        _sb.Indent();
        FormatMembers(node.Members);
        _sb.Dedent();
        _sb.AppendLine("}");
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        var modifiers = node.Modifiers.ToFullString().Trim();
        var declaration = node.Declaration.ToFullString().Trim();
        
        var fieldDeclaration = string.IsNullOrEmpty(modifiers)
            ? $"{declaration};"
            : $"{modifiers} {declaration};";
            
        _sb.AppendLine(fieldDeclaration);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var modifiers = node.Modifiers.ToFullString().Trim();
        var constructorName = node.Identifier.Text;
        var parameters = node.ParameterList.ToFullString().Trim();
        
        var signature = string.IsNullOrEmpty(modifiers)
            ? $"{constructorName}{parameters}"
            : $"{modifiers} {constructorName}{parameters}";
            
        _sb.AppendLine(signature);
        _sb.AppendLine("{");
        _sb.Indent();
        if (node.Body != null)
        {
            foreach (var statement in node.Body.Statements)
            {
                _sb.AppendLine(statement.ToFullString().Trim());
            }
        }
        _sb.Dedent();
        _sb.AppendLine("}");
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        // Build the method signature from its actual syntax
        var modifiers = node.Modifiers.ToFullString().Trim();
        var returnType = node.ReturnType.ToFullString().Trim();
        var methodName = node.Identifier.Text;
        var parameters = node.ParameterList.ToFullString().Trim();
        
        var signature = string.IsNullOrEmpty(modifiers) 
            ? $"{returnType} {methodName}{parameters}"
            : $"{modifiers} {returnType} {methodName}{parameters}";
            
        _sb.AppendLine(signature);
        _sb.AppendLine("{");
        _sb.Indent();
        if (node.Body != null)
        {
            foreach (var statement in node.Body.Statements)
            {
                _sb.AppendLine(statement.ToFullString().Trim());
            }
        }
        _sb.Dedent();
        _sb.AppendLine("}");
    }

    private void FormatMembers(SyntaxList<MemberDeclarationSyntax> members)
    {
        bool first = true;
        MemberDeclarationSyntax? previousMember = null;
        
        foreach (var member in members)
        {
            if (!first)
            {
                // Add blank line between different types of members, but not between consecutive fields
                bool shouldAddBlankLine = ShouldAddBlankLineBetweenMembers(previousMember, member);
                if (shouldAddBlankLine)
                {
                    _sb.AppendLine();
                }
            }
            first = false;
            previousMember = member;
            Visit(member);
        }
    }

    private static bool ShouldAddBlankLineBetweenMembers(MemberDeclarationSyntax? previous, MemberDeclarationSyntax current)
    {
        if (previous == null) return false;
        
        // Don't add blank line between consecutive field declarations
        if (previous is FieldDeclarationSyntax && current is FieldDeclarationSyntax)
        {
            return false;
        }
        
        // Add blank line between different types of members
        return true;
    }
}