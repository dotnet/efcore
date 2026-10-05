// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

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
public interface IMigrationCompiler
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     Compiles scaffolded migration source code into an in-memory assembly.
    /// </remarks>
    /// <param name="scaffoldedMigration">The scaffolded migration containing C# source code.</param>
    /// <param name="contextType">The type of the <see cref="DbContext" /> for which the migration was created.</param>
    /// <returns>An <see cref="Assembly" /> containing the compiled migration and model snapshot.</returns>
    /// <exception cref="InvalidOperationException">Thrown when compilation fails.</exception>
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    Assembly CompileMigration(
        ScaffoldedMigration scaffoldedMigration,
        Type contextType);
}
