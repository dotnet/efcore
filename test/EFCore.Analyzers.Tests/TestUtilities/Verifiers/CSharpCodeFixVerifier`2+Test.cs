using System;
using System.Collections.Immutable;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.EntityFrameworkCore.TestUtilities.Verifiers
{
    public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
        {
            static Test()
            {
                // If we have outdated defaults from the host unit test application targeting an older .NET Framework, use more
                // reasonable TLS protocol version for outgoing connections.
#pragma warning disable CA5364 // Do Not Use Deprecated Security Protocols
#pragma warning disable CS0618 // Type or member is obsolete
                if (ServicePointManager.SecurityProtocol == (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls))
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CA5364 // Do Not Use Deprecated Security Protocols
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                }
            }

            public Test()
            {
                ReferenceAssemblies = AdditionalMetadataReferences.Default;

                SolutionTransforms.Add((solution, projectId) =>
                {
                    var project = solution.GetProject(projectId)!;
                    var parseOptions = (CSharpParseOptions)project.ParseOptions!;
                    solution = solution.WithProjectParseOptions(projectId, parseOptions.WithLanguageVersion(LanguageVersion));

                    var compilationOptions = project.CompilationOptions!;
                    
                    solution = solution.WithProjectCompilationOptions(projectId, compilationOptions);

                    if (AnalyzerConfigDocument is not null)
                    {
                        solution = solution.AddAnalyzerConfigDocument(
                            DocumentId.CreateNewId(projectId, debugName: ".editorconfig"),
                            ".editorconfig",
                            SourceText.From($"is_global = true" + Environment.NewLine + AnalyzerConfigDocument),
                            filePath: @"z:\.editorconfig");
                    }

                    return solution;
                });
            }

            protected override bool IsCompilerDiagnosticIncluded(Diagnostic diagnostic, CompilerDiagnostics compilerDiagnostics)
            {
                return !diagnostic.IsSuppressed;
            }

            public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp9;

            public string? AnalyzerConfigDocument { get; set; }
        }
    }

}
