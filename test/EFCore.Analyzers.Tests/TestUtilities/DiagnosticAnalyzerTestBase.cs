// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyModel;
using Xunit;

namespace EFCore.Analyzers.Test.TestUtilities
{
    public abstract class DiagnosticAnalyzerTestBase
    {
        protected abstract DiagnosticAnalyzer CreateDiagnosticAnalyzer();

        protected async Task<Diagnostic[]> GetDiagnosticsAsync(string source)
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

            source = $"using Microsoft.EntityFrameworkCore;using System.Data.SqlClient;class C {{ void M() {{ {source} }} }}";

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
}
