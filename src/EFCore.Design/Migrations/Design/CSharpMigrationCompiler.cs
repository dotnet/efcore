// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Design.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public class CSharpMigrationCompiler : IMigrationCompiler
{
    private static readonly CSharpCompilationOptions CompilationOptions = new(
        OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: OptimizationLevel.Debug,
        nullableContextOptions: NullableContextOptions.Enable,
        specificDiagnosticOptions: new Dictionary<string, ReportDiagnostic>
        {
            // Suppress common warnings that don't affect functionality
            { "CS1701", ReportDiagnostic.Suppress }, // Assembly reference mismatch
            { "CS1702", ReportDiagnostic.Suppress }, // Assembly reference mismatch
            { "CS8019", ReportDiagnostic.Suppress }, // Unnecessary using directive
        });

    private static readonly CSharpParseOptions ParseOptions = new(
        LanguageVersion.Latest,
        DocumentationMode.None);

    // Cache of assembly references to avoid repeated resolution
    private IReadOnlyList<MetadataReference>? _cachedReferences;

    /// <inheritdoc />
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    public virtual Assembly CompileMigration(
        ScaffoldedMigration scaffoldedMigration,
        Type contextType)
    {
        var assemblyName = $"DynamicMigration_{scaffoldedMigration.MigrationId}_{Guid.NewGuid():N}";
        _ = contextType.Assembly;

        // Parse the source code into syntax trees
        var syntaxTrees = new List<SyntaxTree>
        {
            SyntaxFactory.ParseSyntaxTree(
                scaffoldedMigration.MigrationCode,
                ParseOptions,
                $"{scaffoldedMigration.MigrationId}.cs"),
            SyntaxFactory.ParseSyntaxTree(
                scaffoldedMigration.MetadataCode,
                ParseOptions,
                $"{scaffoldedMigration.MigrationId}.Designer.cs"),
            SyntaxFactory.ParseSyntaxTree(
                scaffoldedMigration.SnapshotCode,
                ParseOptions,
                $"{scaffoldedMigration.SnapshotName}.cs")
        };

        // Gather assembly references
        var references = GetOrCreateCachedReferences();

        // Create the compilation
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            references,
            CompilationOptions);

        // Emit to memory and load
        using var assemblyStream = new MemoryStream();
        var emitResult = compilation.Emit(assemblyStream);

        if (!emitResult.Success)
        {
            var errors = emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString());

            throw new InvalidOperationException(
                DesignStrings.MigrationCompilationFailed(
                    scaffoldedMigration.MigrationId,
                    string.Join(Environment.NewLine, errors)));
        }

        assemblyStream.Seek(0, SeekOrigin.Begin);
        return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
    }

    private IReadOnlyList<MetadataReference> GetOrCreateCachedReferences()
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _cachedReferences,
            this,
            static self =>
            {
                var references = new List<MetadataReference>();

                // Add references from all loaded assemblies (except dynamic/in-memory ones)
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    AddAssemblyReference(references, assembly);
                }

                return references;
            });

    private static void AddAssemblyReference(List<MetadataReference> references, Assembly assembly)
    {
        if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
        {
            return;
        }

        try
        {
            var reference = MetadataReference.CreateFromFile(assembly.Location);
            if (!references.Any(r => r.Display == reference.Display))
            {
                references.Add(reference);
            }
        }
        catch
        {
            // Ignore assemblies that can't be referenced
        }
    }
}
