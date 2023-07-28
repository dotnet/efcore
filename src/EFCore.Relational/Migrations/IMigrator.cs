// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     The main service used to generated an EF Core Migrations script or
///     migrate a database directly.
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
public interface IMigrator
{
    /// <summary>
    ///     Migrates the database to either a specified target migration or up to the latest
    ///     migration that exists in the <see cref="IMigrationsAssembly" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="targetMigration">
    ///     The target migration to migrate the database to, or <see langword="null" /> to migrate to the latest.
    /// </param>
    [RequiresUnreferencedCode("Migration generation currently isn't compatible with trimming")]
    [RequiresDynamicCode("Migrations operations are not supported with NativeAOT")]
    void Migrate(string? targetMigration = null);

    /// <summary>
    ///     Migrates the database to either a specified target migration or up to the latest
    ///     migration that exists in the <see cref="IMigrationsAssembly" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="targetMigration">
    ///     The target migration to migrate the database to, or <see langword="null" /> to migrate to the latest.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    [RequiresUnreferencedCode("Migration generation currently isn't compatible with trimming")]
    [RequiresDynamicCode("Migrations operations are not supported with NativeAOT")]
    Task MigrateAsync(
        string? targetMigration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Generates a SQL script to migrate a database either in its entirety, or starting and
    ///     ending at specified migrations.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="fromMigration">
    ///     The migration to start from, or <see langword="null" /> to start from the empty database.
    /// </param>
    /// <param name="toMigration">
    ///     The target migration to migrate the database to, or <see langword="null" /> to migrate to the latest.
    /// </param>
    /// <param name="options">
    ///     The options to use when generating SQL for migrations.
    /// </param>
    /// <returns>The generated script.</returns>
    [RequiresUnreferencedCode("Migration generation currently isn't compatible with trimming")]
    [RequiresDynamicCode("Migrations operations are not supported with NativeAOT")]
    string GenerateScript(
        string? fromMigration = null,
        string? toMigration = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default);
}
