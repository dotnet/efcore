// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Migrations.Design;

/// <summary>
///     A service for compiling scaffolded migration source code into an in-memory assembly.
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
public interface IMigrationCompiler
{
    /// <summary>
    ///     Compiles scaffolded migration source code into an in-memory assembly.
    /// </summary>
    /// <param name="scaffoldedMigration">The scaffolded migration containing C# source code.</param>
    /// <param name="contextType">The type of the <see cref="DbContext" /> for which the migration was created.</param>
    /// <param name="references">Additional assembly references to include in compilation, if any.</param>
    /// <returns>An <see cref="Assembly" /> containing the compiled migration and model snapshot.</returns>
    /// <exception cref="InvalidOperationException">Thrown when compilation fails.</exception>
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    Assembly CompileMigration(
        ScaffoldedMigration scaffoldedMigration,
        Type contextType,
        IEnumerable<Assembly>? references = null);
}
