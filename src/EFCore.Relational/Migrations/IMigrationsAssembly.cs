// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A service representing an assembly containing EF Core Migrations.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
///     </para>
/// </remarks>
public interface IMigrationsAssembly
{
    /// <summary>
    ///     A dictionary mapping migration identifiers to the <see cref="TypeInfo" /> of the class
    ///     that represents the migration.
    /// </summary>
    IReadOnlyDictionary<string, TypeInfo> Migrations { get; }

    /// <summary>
    ///     The snapshot of the <see cref="IModel" /> contained in the assembly.
    /// </summary>
    ModelSnapshot? ModelSnapshot { get; }

    /// <summary>
    ///     The assembly that contains the migrations, snapshot, etc.
    /// </summary>
    Assembly Assembly { get; }

    /// <summary>
    ///     Finds a migration identifier in the assembly with the given a full migration name or
    ///     just its identifier.
    /// </summary>
    /// <param name="nameOrId">The name or identifier to lookup.</param>
    /// <returns>The identifier of the migration, or <see langword="null" /> if none was found.</returns>
    string? FindMigrationId(string nameOrId);

    /// <summary>
    ///     Creates an instance of the migration class.
    /// </summary>
    /// <param name="migrationClass">
    ///     The <see cref="TypeInfo" /> for the migration class, as obtained from the <see cref="Migrations" /> dictionary.
    /// </param>
    /// <param name="activeProvider">The name of the current database provider.</param>
    /// <returns>The migration instance.</returns>
    Migration CreateMigration(TypeInfo migrationClass, string activeProvider);
}
