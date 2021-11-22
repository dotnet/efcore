// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;

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

    protected virtual async Task<(Diagnostic[], string)> GetDiagnosticsAsync(string source, params string[] extraUsings)
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
        return (await GetDiagnosticsFullSourceAsync(fullSource), fullSource);
    }

    protected async Task<Diagnostic[]> GetDiagnosticsFullSourceAsync(string source)
    {
        var compilation = await CreateProject(source).GetCompilationAsync();
        var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);

        Assert.Empty(errors);

        var analyzer = CreateDiagnosticAnalyzer();
        var compilationWithAnalyzers
            = compilation
                .WithOptions(
                    compilation.Options.WithSpecificDiagnosticOptions(
                        analyzer.SupportedDiagnostics.ToDictionary(d => d.Id, d => ReportDiagnostic.Default)))
                .WithAnalyzers(ImmutableArray.Create(analyzer));

        var diagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();

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
            .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }
}
