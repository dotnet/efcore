// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

    public string TargetDir { get; set; }
    public Dictionary<string, string> Sources { get; set; } = new();
    public bool NullableReferenceTypes { get; set; }

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

                File.Copy(reference.Path, Path.Combine(TargetDir, Path.GetFileName(reference.Path)), overwrite: true);
            }

            references.AddRange(reference.References);
        }

        var compilation = CSharpCompilation.Create(
            projectName,
            Sources.Select(s => SyntaxFactory.ParseSyntaxTree(s.Value).WithFilePath(s.Key)),
            references,
            CreateOptions());

        var targetPath = Path.Combine(TargetDir ?? Path.GetTempPath(), projectName + ".dll");

        using (var stream = File.OpenWrite(targetPath))
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
            Sources.Select(s => SyntaxFactory.ParseSyntaxTree(s.Value).WithFilePath(s.Key)),
            references,
            CreateOptions());

        Assembly assembly;
        using (var stream = new MemoryStream())
        {
            var result = compilation.Emit(stream);
            if (!result.Success)
            {
                throw new InvalidOperationException(
                    $@"Build failed:
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
            generalDiagnosticOption: ReportDiagnostic.Error,
            specificDiagnosticOptions: new Dictionary<string, ReportDiagnostic>
            {
                { "CS1030", ReportDiagnostic.Suppress },
                { "CS1701", ReportDiagnostic.Suppress },
                { "CS1702", ReportDiagnostic.Suppress }, // Always thrown for .NET Core
                {
                    "CS1705", ReportDiagnostic.Suppress
                }, // Assembly 'AssemblyName1' uses 'TypeName' which has a higher version than referenced assembly 'AssemblyName2'
                { "CS8019", ReportDiagnostic.Suppress } // Unnecessary using directive.
            });
}
