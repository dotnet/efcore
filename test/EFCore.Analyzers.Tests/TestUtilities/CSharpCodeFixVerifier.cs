// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.DependencyModel;
using CompilationOptions = Microsoft.CodeAnalysis.CompilationOptions;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test { TestCode = source };
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    public static async Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        var test = new Test { TestCode = source, FixedCode = fixedSource };
        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net100;

            if (NugetConfigFinder.Find() is string nuGetConfigFilePath)
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net100.WithNuGetConfigFilePath(nuGetConfigFilePath);
            }

            TestState.AdditionalReferences.AddRange(
                DependencyContext.Load(GetType().Assembly)!
                    .CompileLibraries
                    .SelectMany(c => c.ResolveReferencePaths())
                    .Select(path => MetadataReference.CreateFromFile(path))
                    .Cast<MetadataReference>());

            DisabledDiagnostics.AddRange(
                "CS1701", // Assuming assembly reference '...' used by '...' matches identity '...' of '...', you may need to supply runtime policy
                "CS1591"  // Missing XML comment for publicly visible type or member '...'
            );
        }

        protected override CompilationOptions CreateCompilationOptions()
            => ((CSharpCompilationOptions)base.CreateCompilationOptions()).WithNullableContextOptions(NullableContextOptions.Enable);
    }
}
