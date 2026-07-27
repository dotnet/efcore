// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.Extensions.DependencyModel;
using CompilationOptions = Microsoft.CodeAnalysis.CompilationOptions;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
        => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    public static Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test { TestCode = source };
        test.ExpectedDiagnostics.AddRange(expected);
        return test.RunAsync();
    }

    public class Test : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
    {
        public Test()
        {
#if NET11_0
            // Microsoft.CodeAnalysis.Testing currently does not have built-in support for .NET 11.0.
            // ReferenceAssemblies = ReferenceAssemblies.Net.Net110;

            ReferenceAssemblies = ReferenceAssembliesNet110;
#else
#error Update the above to match the targeted TFM
#endif

            if (NugetConfigFinder.Find() is string nuGetConfigFilePath)
            {
                ReferenceAssemblies = ReferenceAssemblies.WithNuGetConfigFilePath(nuGetConfigFilePath);
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

        private static readonly ReferenceAssemblies ReferenceAssembliesNet110 = new("net11.0");
    }
}
