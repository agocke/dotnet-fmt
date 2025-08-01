
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
                    // Normalize the using directive by reconstructing it
                    var normalizedUsing = usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
                        ? $"using static {usingDirective.Name?.ToString()};"
                        : usingDirective.Alias != null
                            ? $"using {usingDirective.Alias.Name.Identifier.ValueText} = {usingDirective.Name?.ToString()};"
                            : $"using {usingDirective.Name?.ToString()};";

                    _sb.AppendLine(normalizedUsing);
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
        var modifiers = SortModifiers(node.Modifiers);
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
        var modifiers = SortModifiers(node.Modifiers);
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
        var modifiers = SortModifiers(node.Modifiers);
        var declaration = node.Declaration.NormalizeWhitespace().ToString(); // Use NormalizeWhitespace() for better normalization

        var fieldDeclaration = string.IsNullOrEmpty(modifiers)
            ? $"{declaration};"
            : $"{modifiers} {declaration};";

        _sb.AppendLine(fieldDeclaration);
    }

    public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var modifiers = SortModifiers(node.Modifiers);
        var constructorName = node.Identifier.Text;
        var parameters = node.ParameterList.NormalizeWhitespace().ToString(); // Use NormalizeWhitespace() for better normalization

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
                _sb.AppendLine(NormalizeStatement(statement));
            }
        }
        _sb.Dedent();
        _sb.AppendLine("}");
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        // Build the method signature from its actual syntax
        var modifiers = SortModifiers(node.Modifiers);
        var returnType = node.ReturnType.NormalizeWhitespace().ToString(); // Use NormalizeWhitespace() for better normalization
        var methodName = node.Identifier.Text;
        var parameters = node.ParameterList.NormalizeWhitespace().ToString(); // Use NormalizeWhitespace() for better normalization

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
                _sb.AppendLine(NormalizeStatement(statement));
            }
        }
        _sb.Dedent();
        _sb.AppendLine("}");
    }

    private static string NormalizeStatement(SyntaxNode statement)
    {
        // Use NormalizeWhitespace() to get properly normalized text
        return statement.NormalizeWhitespace().ToString();
    }

    private static string SortModifiers(SyntaxTokenList modifiers)
    {
        if (modifiers.Count == 0) return string.Empty;

        // Define the order of modifiers according to C# conventions
        var modifierOrder = new Dictionary<SyntaxKind, int>
        {
            // Accessibility modifiers come first
            { SyntaxKind.PublicKeyword, 1 },
            { SyntaxKind.ProtectedKeyword, 2 },
            { SyntaxKind.InternalKeyword, 3 },
            { SyntaxKind.PrivateKeyword, 4 },
            // Then static
            { SyntaxKind.StaticKeyword, 5 },
            // Then other modifiers
            { SyntaxKind.AbstractKeyword, 6 },
            { SyntaxKind.VirtualKeyword, 7 },
            { SyntaxKind.OverrideKeyword, 8 },
            { SyntaxKind.SealedKeyword, 9 },
            { SyntaxKind.ExternKeyword, 10 },
            { SyntaxKind.PartialKeyword, 11 },
            { SyntaxKind.AsyncKeyword, 12 },
            { SyntaxKind.UnsafeKeyword, 13 },
            // Storage modifiers come last
            { SyntaxKind.ReadOnlyKeyword, 14 },
            { SyntaxKind.VolatileKeyword, 15 },
            { SyntaxKind.ConstKeyword, 16 }
        };

        var sortedModifiers = modifiers
            .Where(m => modifierOrder.ContainsKey(m.Kind()))
            .OrderBy(m => modifierOrder[m.Kind()])
            .Select(m => m.ValueText)
            .ToList();

        // Add any modifiers not in our order dictionary at the end
        var unknownModifiers = modifiers
            .Where(m => !modifierOrder.ContainsKey(m.Kind()))
            .Select(m => m.ValueText);

        sortedModifiers.AddRange(unknownModifiers);

        return string.Join(" ", sortedModifiers);
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