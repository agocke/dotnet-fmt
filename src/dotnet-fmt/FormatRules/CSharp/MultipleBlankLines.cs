
using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotnetFormatter.FormatRules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MultipleBlankLines : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MultipleBlankLines";
        private const string Title = "Files should not contain multiple consecutive blank lines";
        private const string MessageFormat = "Multiple blank lines";
        private const string Category = "Formatting";

        private readonly static DiagnosticDescriptor Rule =
            new DiagnosticDescriptor(DiagnosticId,
                                     Title,
                                     MessageFormat,
                                     Category,
                                     DiagnosticSeverity.Error,
                                     isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
        }

        private static void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            var root = context.Tree.GetRoot();

            foreach (var trivia in root.DescendantTrivia())
            {
                var length = trivia.FullSpan.Length;
            }
        }
    }
}