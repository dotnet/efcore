// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     A service that compiles scaffolded migration C# code into an in-memory assembly using Roslyn.
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
    private readonly object _referenceLock = new();

    /// <inheritdoc />
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    public virtual CompiledMigration CompileMigration(
        ScaffoldedMigration scaffoldedMigration,
        Type contextType,
        IEnumerable<Assembly>? additionalReferences = null)
    {
        var assemblyName = $"DynamicMigration_{scaffoldedMigration.MigrationId}_{Guid.NewGuid():N}";

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
        var references = GetMetadataReferences(contextType, additionalReferences);

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
        var assembly = AssemblyLoadContext.Default.LoadFromStream(assemblyStream);

        // Find the migration and snapshot types
        var migrationTypeInfo = FindMigrationTypeInfo(assembly, scaffoldedMigration.MigrationId);
        var snapshotTypeInfo = FindSnapshotTypeInfo(assembly, scaffoldedMigration.SnapshotName);

        return new CompiledMigration(
            assembly,
            migrationTypeInfo,
            snapshotTypeInfo,
            scaffoldedMigration.MigrationId,
            scaffoldedMigration);
    }

    /// <summary>
    ///     Gets the metadata references required for compilation.
    /// </summary>
    /// <param name="contextType">The DbContext type.</param>
    /// <param name="additionalReferences">Additional assembly references.</param>
    /// <returns>The list of metadata references.</returns>
    protected virtual IReadOnlyList<MetadataReference> GetMetadataReferences(
        Type contextType,
        IEnumerable<Assembly>? additionalReferences)
    {
        // Get or create cached base references
        var baseReferences = GetOrCreateCachedReferences();

        // Add context-specific references
        var contextAssembly = contextType.Assembly;
        var allReferences = new List<MetadataReference>(baseReferences);

        // Add the context's assembly and its references
        AddAssemblyReference(allReferences, contextAssembly);
        foreach (var referencedAssembly in contextAssembly.GetReferencedAssemblies())
        {
            try
            {
                var assembly = Assembly.Load(referencedAssembly);
                AddAssemblyReference(allReferences, assembly);
            }
            catch
            {
                // Ignore assemblies that can't be loaded
            }
        }

        // Add any additional references
        if (additionalReferences != null)
        {
            foreach (var assembly in additionalReferences)
            {
                AddAssemblyReference(allReferences, assembly);
            }
        }

        return allReferences;
    }

    private IReadOnlyList<MetadataReference> GetOrCreateCachedReferences()
    {
        if (_cachedReferences != null)
        {
            return _cachedReferences;
        }

        lock (_referenceLock)
        {
            if (_cachedReferences != null)
            {
                return _cachedReferences;
            }

            var references = new List<MetadataReference>();

            // Add references from all loaded assemblies that we need
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                {
                    continue;
                }

                var name = assembly.GetName().Name;
                if (name == null)
                {
                    continue;
                }

                // Include core runtime, EF Core, and common dependencies
                if (IsRequiredAssembly(name))
                {
                    AddAssemblyReference(references, assembly);
                }
            }

            _cachedReferences = references;
            return _cachedReferences;
        }
    }

    private static bool IsRequiredAssembly(string assemblyName)
    {
        return assemblyName.StartsWith("System", StringComparison.Ordinal)
            || assemblyName.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
            || assemblyName.StartsWith("Microsoft.Extensions", StringComparison.Ordinal)
            || assemblyName == "netstandard"
            || assemblyName == "mscorlib"
            || assemblyName.StartsWith("Npgsql", StringComparison.Ordinal)
            || assemblyName.StartsWith("MySql", StringComparison.Ordinal)
            || assemblyName.StartsWith("Oracle", StringComparison.Ordinal);
    }

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

    private static System.Reflection.TypeInfo FindMigrationTypeInfo(Assembly assembly, string migrationId)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(Migration).IsAssignableFrom(type))
            {
                var migrationAttribute = type.GetCustomAttribute<MigrationAttribute>();
                if (migrationAttribute != null
                    && string.Equals(migrationAttribute.Id, migrationId, StringComparison.Ordinal))
                {
                    return type.GetTypeInfo();
                }
            }
        }

        throw new InvalidOperationException(
            DesignStrings.MigrationTypeNotFound(migrationId));
    }

    private static System.Reflection.TypeInfo? FindSnapshotTypeInfo(Assembly assembly, string snapshotName)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (typeof(ModelSnapshot).IsAssignableFrom(type)
                && type.Name == snapshotName)
            {
                return type.GetTypeInfo();
            }
        }

        return null;
    }
}
