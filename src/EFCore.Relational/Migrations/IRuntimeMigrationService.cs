// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     A service for creating and applying migrations at runtime without requiring recompilation.
/// </summary>
/// <remarks>
///     <para>
///         This service enables creating migrations dynamically based on the current model state,
///         compiling them in-memory using Roslyn, and applying them to the database in a single operation.
///     </para>
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
public interface IRuntimeMigrationService
{
    /// <summary>
    ///     Creates a new migration based on pending model changes, compiles it dynamically,
    ///     and applies it to the database.
    /// </summary>
    /// <param name="migrationName">The name for the new migration.</param>
    /// <param name="options">Options controlling migration creation and application.</param>
    /// <returns>A result containing information about the created and applied migration.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when there are no pending model changes or when compilation fails.
    /// </exception>
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    RuntimeMigrationResult CreateAndApplyMigration(
        string migrationName,
        RuntimeMigrationOptions? options = null);

    /// <summary>
    ///     Creates a new migration based on pending model changes, compiles it dynamically,
    ///     and applies it to the database.
    /// </summary>
    /// <param name="migrationName">The name for the new migration.</param>
    /// <param name="options">Options controlling migration creation and application.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains information about the created and applied migration.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when there are no pending model changes or when compilation fails.
    /// </exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    [RequiresDynamicCode("Runtime migration compilation requires dynamic code generation.")]
    Task<RuntimeMigrationResult> CreateAndApplyMigrationAsync(
        string migrationName,
        RuntimeMigrationOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether there are pending model changes that would result in a new migration.
    /// </summary>
    /// <returns>
    ///     <see langword="true" /> if there are pending model changes; otherwise, <see langword="false" />.
    /// </returns>
    bool HasPendingModelChanges();
}
