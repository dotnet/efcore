// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class BuildSource
{
    public ICollection<BuildReference> References { get; } = new List<BuildReference>
    {
        BuildReference.ByName("netstandard"),
        BuildReference.ByName("System.Collections"),
        BuildReference.ByName("System.ComponentModel.Annotations"),
        BuildReference.ByName("System.Data.Common"),
        BuildReference.ByName("System.Linq.Expressions"),
        BuildReference.ByName("System.Runtime"),
        BuildReference.ByName("System.Runtime.Extensions"),
        BuildReference.ByName("System.Text.RegularExpressions")
    };

    public string? TargetDir { get; set; }
    public Dictionary<string, string> Sources { get; set; } = new();
    public bool NullableReferenceTypes { get; set; }
    public bool EmitDocumentationDiagnostics { get; set; }

    public BuildFileResult Build()
    {
        var projectName = Path.GetRandomFileName();
        var references = new List<MetadataReference>();

        foreach (var reference in References)
        {
            if (reference.CopyLocal)
            {
                if (string.IsNullOrEmpty(reference.Path))
                {
                    throw new InvalidOperationException("Could not find path for reference " + reference);
                }

                File.Copy(
                    reference.Path, Path.Combine(TargetDir ?? throw new NullReferenceException(), Path.GetFileName(reference.Path)),
                    overwrite: true);
            }

            references.AddRange(reference.References);
        }

        var compilation = CSharpCompilation.Create(
            projectName,
            Sources.Select(
                s => SyntaxFactory.ParseSyntaxTree(
                    text: s.Value,
                    path: s.Key,
                    options: new CSharpParseOptions(
                        LanguageVersion.Latest,
                        EmitDocumentationDiagnostics ? DocumentationMode.Diagnose : DocumentationMode.Parse))),
            references,
            CreateOptions());

        var targetPath = Path.Combine(TargetDir ?? Path.GetTempPath(), projectName + ".dll");

        using (var stream = File.Create(targetPath))
        {
            var result = compilation.Emit(stream);
            if (!result.Success)
            {
                throw new InvalidOperationException(
                    $@"Build failed:
{string.Join(Environment.NewLine, result.Diagnostics)}");
            }
        }

        return new BuildFileResult(targetPath);
    }

    public Assembly BuildInMemory()
    {
        var projectName = Path.GetRandomFileName();
        var references = new List<MetadataReference>();

        foreach (var reference in References)
        {
            references.AddRange(reference.References);
        }

        var compilation = CSharpCompilation.Create(
            projectName,
            Sources.Select(
                s => SyntaxFactory.ParseSyntaxTree(
                    text: s.Value,
                    path: s.Key,
                    options: new CSharpParseOptions(
                        LanguageVersion.Latest,
                        EmitDocumentationDiagnostics ? DocumentationMode.Diagnose : DocumentationMode.Parse))),
            references,
            CreateOptions());

        var diagnostics = compilation.GetDiagnostics();
        if (!diagnostics.IsEmpty)
        {
            throw new InvalidOperationException(
                $@"Build failed.

First diagnostic:
{diagnostics[0]}

Location:
{diagnostics[0].Location.SourceTree?.GetRoot().FindNode(diagnostics[0].Location.SourceSpan)}

All diagnostics:
{string.Join(Environment.NewLine, diagnostics)}");
        }

        Assembly assembly;
        using (var stream = new MemoryStream())
        {
            var result = compilation.Emit(stream);
            if (!result.Success)
            {
                throw new InvalidOperationException(
                    $@"Failed to emit compilation:
{string.Join(Environment.NewLine, result.Diagnostics)}");
            }

            assembly = Assembly.Load(stream.ToArray());
        }

        return assembly;
    }

    public async Task<Assembly> BuildInMemoryWithWithAnalyzersAsync()
    {
        var compilation = CSharpCompilation
            .Create(
                assemblyName: Path.GetRandomFileName(),
                Sources.Select(
                    s => SyntaxFactory.ParseSyntaxTree(
                        text: s.Value,
                        path: s.Key,
                        options: new CSharpParseOptions(
                            LanguageVersion.Latest,
                            EmitDocumentationDiagnostics ? DocumentationMode.Diagnose : DocumentationMode.Parse))),
                References.SelectMany(r => r.References),
                CreateOptions())
            .WithAnalyzers([new UninitializedDbSetDiagnosticSuppressor()]);

        var diagnostics = await compilation.GetAllDiagnosticsAsync();
        if (!diagnostics.IsEmpty)
        {
            throw new InvalidOperationException(
                $@"Build failed.

First diagnostic:
{diagnostics[0]}

Location:
{diagnostics[0].Location.SourceTree?.GetRoot().FindNode(diagnostics[0].Location.SourceSpan)}

All diagnostics:
{string.Join(Environment.NewLine, diagnostics)}");
        }

        Assembly assembly;
        using (var stream = new MemoryStream())
        {
            var result = compilation.Compilation.Emit(stream);
            if (!result.Success)
            {
                throw new InvalidOperationException(
                    $@"Failed to emit compilation:
{string.Join(Environment.NewLine, result.Diagnostics)}");
            }

            assembly = Assembly.Load(stream.ToArray());
        }

        return assembly;
    }

    private CSharpCompilationOptions CreateOptions()
        => new(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: NullableReferenceTypes ? NullableContextOptions.Enable : NullableContextOptions.Disable,
            reportSuppressedDiagnostics: false,
            specificDiagnosticOptions: new Dictionary<string, ReportDiagnostic>
            {
                // Displays the text of a warning defined with the #warning directive
                { "CS1030", ReportDiagnostic.Suppress },

                // Assuming assembly reference "Assembly Name #1" matches "Assembly Name #2", you may need to supply runtime policy
                { "CS1701", ReportDiagnostic.Suppress },

                // Assuming assembly reference "Assembly Name #1" matches "Assembly Name #2", you may need to supply runtime policy
                { "CS1702", ReportDiagnostic.Suppress },

                // Assembly 'AssemblyName1' uses 'TypeName' which has a higher version than referenced assembly 'AssemblyName2'
                { "CS1705", ReportDiagnostic.Suppress },

                // Unnecessary using directive.
                { "CS8019", ReportDiagnostic.Suppress }
            });
}
