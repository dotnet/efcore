// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using CompilationOptions = Microsoft.CodeAnalysis.CompilationOptions;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public abstract class DiagnosticAnalyzerTestBase
{
    private static readonly string[] _usings = { "System", "Microsoft.EntityFrameworkCore" };

    protected abstract DiagnosticAnalyzer CreateDiagnosticAnalyzer();

    protected async Task AssertNoDiagnostics(string source, params string[] extraUsings)
    {
        var (diagnostics, _) = await GetDiagnosticsAsync(source, extraUsings);
        Assert.Empty(diagnostics);
    }

    protected virtual Task<(Diagnostic[], string)> GetDiagnosticsAsync(string source, params string[] extraUsings)
        => GetDiagnosticsAsync(source, analyzerDiagnosticsOnly: true, extraUsings);

    protected virtual async Task<(Diagnostic[], string)> GetDiagnosticsAsync(
            string source, bool analyzerDiagnosticsOnly, params string[] extraUsings)
    {
        var sb = new StringBuilder();
        foreach (var @using in _usings.Concat(extraUsings))
        {
            sb
                .Append("using ")
                .Append(@using)
                .AppendLine(";");
        }

        sb
            .AppendLine()
            .AppendLine("class C {")
            .AppendLine("void M() {")
            .AppendLine(source)
            .AppendLine("}")
            .AppendLine("}");

        var fullSource = sb.ToString();
        return (await GetDiagnosticsFullSourceAsync(fullSource, analyzerDiagnosticsOnly), fullSource);
    }

    protected async Task<Diagnostic[]> GetDiagnosticsFullSourceAsync(string source, bool analyzerDiagnosticsOnly = true)
    {
        var compilation = await CreateProject(source).GetCompilationAsync();
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);

        Assert.Empty(errors);

        var analyzer = CreateDiagnosticAnalyzer();
        var compilationWithAnalyzers
            = compilation
                .WithAnalyzers(ImmutableArray.Create(analyzer),
                    new CompilationWithAnalyzersOptions(
                        new AnalyzerOptions(new()),
                        onAnalyzerException: null,
                        concurrentAnalysis: false,
                        logAnalyzerExecutionTime: false,
                        reportSuppressedDiagnostics: true));

        var diagnostics = analyzerDiagnosticsOnly
            ? await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync()
            : await compilationWithAnalyzers.GetAllDiagnosticsAsync();

        return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
    }

    private Project CreateProject(string source)
    {
        const string fileName = "Test.cs";

        //Debugger.Launch();

        var projectId = ProjectId.CreateNewId(debugName: "TestProject");
        var documentId = DocumentId.CreateNewId(projectId, fileName);

        var metadataReferences
            = DependencyContext.Load(GetType().Assembly)
                .CompileLibraries
                .SelectMany(c => c.ResolveReferencePaths())
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();

        var solution = new AdhocWorkspace()
            .CurrentSolution
            .AddProject(projectId, "TestProject", "TestProject", LanguageNames.CSharp)
            .AddMetadataReferences(projectId, metadataReferences)
            .AddDocument(documentId, fileName, SourceText.From(source));

        return solution.GetProject(projectId)
            .WithCompilationOptions(
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    specificDiagnosticOptions: new Dictionary<string, ReportDiagnostic>
                    {
                        { "CS1701", ReportDiagnostic.Suppress }
                    },
                    nullableContextOptions: NullableContextOptions.Enable));
    }
}
