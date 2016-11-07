using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;

namespace DotnetFormatter
{
    public class FormatterAnalysisContext : AnalysisContext
    {
        public readonly List<Action<SyntaxTreeAnalysisContext>> RegisteredSyntaxTreeActions =
            new List<Action<SyntaxTreeAnalysisContext>>();

        public override void RegisterCodeBlockAction(Action<CodeBlockAnalysisContext> action)
        {
            throw new NotImplementedException();
        }

        public override void RegisterCodeBlockStartAction<TLanguageKindEnum>(Action<CodeBlockStartAnalysisContext<TLanguageKindEnum>> action)
        {
            throw new NotImplementedException();
        }

        public override void RegisterCompilationAction(Action<CompilationAnalysisContext> action)
        {
            throw new NotImplementedException();
        }

        public override void RegisterCompilationStartAction(Action<CompilationStartAnalysisContext> action)
        {
            throw new NotImplementedException();
        }

        public override void RegisterSemanticModelAction(Action<SemanticModelAnalysisContext> action)
        {
            throw new NotImplementedException();
        }

        public override void RegisterSymbolAction(Action<SymbolAnalysisContext> action, ImmutableArray<SymbolKind> symbolKinds)
        {
            throw new NotImplementedException();
        }

        public override void RegisterSyntaxNodeAction<TLanguageKindEnum>(Action<SyntaxNodeAnalysisContext> action, ImmutableArray<TLanguageKindEnum> syntaxKinds)
        {
            throw new NotImplementedException();
        }

        public override void RegisterSyntaxTreeAction(Action<SyntaxTreeAnalysisContext> action) =>
            RegisteredSyntaxTreeActions.Add(action);
    }
}
