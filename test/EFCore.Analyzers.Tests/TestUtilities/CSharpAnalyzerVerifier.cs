// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Model;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.Extensions.DependencyModel;
using CompilationOptions = Microsoft.CodeAnalysis.CompilationOptions;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, XUnitVerifier>.Diagnostic(diagnosticId);

    public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test { TestCode = source };
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    public class Test : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
    {
        protected override CompilationOptions CreateCompilationOptions()
        {
            var cSharpOptions = (CSharpCompilationOptions)base.CreateCompilationOptions();
            return cSharpOptions
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithSpecificDiagnosticOptions(
                    new Dictionary<string, ReportDiagnostic>
                    {
                        { "CS1701", ReportDiagnostic.Suppress }, { "CS1591", ReportDiagnostic.Suppress }
                    });
        }

        protected override async Task<Project> CreateProjectImplAsync(
            EvaluatedProjectState primaryProject,
            ImmutableArray<EvaluatedProjectState> additionalProjects,
            CancellationToken cancellationToken)
        {
            var metadataReferences
                = DependencyContext.Load(GetType().Assembly)!
                    .CompileLibraries
                    .SelectMany(c => c.ResolveReferencePaths())
                    .Select(path => MetadataReference.CreateFromFile(path))
                    .Cast<MetadataReference>()
                    .ToList();

            var project = await base.CreateProjectImplAsync(primaryProject, additionalProjects, cancellationToken).ConfigureAwait(false);
            return project.WithMetadataReferences(metadataReferences);
        }
    }
}
