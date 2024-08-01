// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     <para>
///         A service on the EF internal service provider that allows providers or extensions to execute logic
///         after <see cref="IMigrator.Migrate(Action{DbContext, IMigratorData}?, string?, TimeSpan?)"/> is called.
///     </para>
///     <para>
///         This type is typically used by providers or extensions. It is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
/// </remarks>
public interface IMigratorPlugin
{
    /// <summary>
    ///     Called by <see cref="IMigrator.Migrate(Action{DbContext, IMigratorData}?, string?, TimeSpan?)"/> before applying the migrations.
    /// </summary>
    /// <param name="context">The <see cref="DbContext" /> that is being migrated.</param>
    /// <param name="data">The <see cref="IMigratorData" /> that contains the result of the migrations application.</param>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    void Migrating(DbContext context, IMigratorData data);

    /// <summary>
    ///     Called by <see cref="IMigrator.MigrateAsync(Func{DbContext, IMigratorData, CancellationToken, Task}?, string?, TimeSpan?, CancellationToken)"/> before applying the migrations.
    /// </summary>
    /// <param name="context">The <see cref="DbContext" /> that is being migrated.</param>
    /// <param name="data">The <see cref="IMigratorData" /> that contains the result of the migrations application.</param>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task MigratingAsync(
        DbContext context,
        IMigratorData data,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Called by <see cref="IMigrator.Migrate(Action{DbContext, IMigratorData}?, string?, TimeSpan?)"/> after applying the migrations, but before the seeding action.
    /// </summary>
    /// <param name="context">The <see cref="DbContext" /> that is being migrated.</param>
    /// <param name="data">The <see cref="IMigratorData" /> that contains the result of the migrations application.</param>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    void Migrated(DbContext context, IMigratorData data);

    /// <summary>
    ///     Called by <see cref="IMigrator.MigrateAsync(Func{DbContext, IMigratorData, CancellationToken, Task}?, string?, TimeSpan?, CancellationToken)"/> after applying the migrations, but before the seeding action.
    /// </summary>
    /// <param name="context">The <see cref="DbContext" /> that is being migrated.</param>
    /// <param name="data">The <see cref="IMigratorData" /> that contains the result of the migrations application.</param>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see> for more information and examples.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    Task MigratedAsync(
        DbContext context,
        IMigratorData data,
        CancellationToken cancellationToken = default);
}
