using DotnetFormatter.FormatRules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DotnetFormatter
{
    public class Program
    {
        private const int ExitFailed = 1;

        public static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet fmt",
                FullName = "dotnet fmt",
                Description = "Format dotnet code according to .NET coding guidelines"
            };

            var lang = app.Argument("[language]", "Programming language, c# by default");
            var sources = app.Argument("<source files>", "Source files to format", multipleValues: true);

            app.OnExecute(() =>
            {
                var rules = new[]
                {
                    new AnalyzerAndFixer(new MultipleBlankLines(), new MultipleBlankLinesFix())
                };

                foreach (var fileName in sources.Values)
                {
                    var result = ApplyFormatter(fileName, rules);
                    if (result != 0)
                    {
                        return result;
                    }
                }
                return 0;
            });

            try
            {
                return app.Execute(args);
            }
            catch (Exception e)
            {
                AnsiConsole.GetError(useConsoleColor: true).WriteLine(e.Message);
                return ExitFailed;
            }
        }

        private struct AnalyzerAndFixer
        {
            public DiagnosticAnalyzer Analyzer { get; }
            public CodeFixProvider CodeFix { get; }

            public AnalyzerAndFixer(DiagnosticAnalyzer analyzer, CodeFixProvider codeFix)
            {
                Analyzer = analyzer;
                CodeFix = codeFix;
            }
        }

        private static int ApplyFormatter(string fileName, AnalyzerAndFixer[] rules)
        {
            string text;
            try
            {
                text = File.ReadAllText(fileName);
            }
            catch
            {
                AnsiConsole.GetError(useConsoleColor: true).WriteLine($"Could not read file {fileName}");
                return ExitFailed;
            }

            var tree = SyntaxFactory.ParseSyntaxTree(text, path: fileName);

            var context = new FormatterAnalysisContext();

            foreach (var rule in rules)
            {
                rule.Analyzer.Initialize(context);
            }

            foreach (var syntaxTreeActions in context.RegisteredSyntaxTreeActions)
            {
                var syntaxTreeContext = new SyntaxTreeAnalysisContext(
                    tree, options: null,
                    reportDiagnostic: delegate { },
                    isSupportedDiagnostic: diag => false,
                    cancellationToken: default(CancellationToken));
            }

            return 0;
        }
    }
}
