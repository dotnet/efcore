// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         The main interaction point between a context and the database provider.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class RelationalDatabase : Database
{
    private IUpdateAdapter? _updateAdapter;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalDatabase" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for the base of this service.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this service.</param>
    public RelationalDatabase(
        DatabaseDependencies dependencies,
        RelationalDatabaseDependencies relationalDependencies)
        : base(dependencies)
    {
        RelationalDependencies = relationalDependencies;
    }

    private IUpdateAdapter UpdateAdapter
        => _updateAdapter ??= Dependencies.UpdateAdapterFactory.Create();

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalDatabaseDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Persists changes from the supplied entries to the database.
    /// </summary>
    /// <param name="entries">Entries representing the changes to be persisted.</param>
    /// <returns>The number of state entries persisted to the database.</returns>
    public override int SaveChanges(IList<IUpdateEntry> entries)
        => RelationalDependencies.BatchExecutor.Execute(
            RelationalDependencies.BatchPreparer.BatchCommands(entries, UpdateAdapter),
            RelationalDependencies.Connection);

    /// <summary>
    ///     Asynchronously persists changes from the supplied entries to the database.
    /// </summary>
    /// <param name="entries">Entries representing the changes to be persisted.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the
    ///     number of entries persisted to the database.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public override Task<int> SaveChangesAsync(
        IList<IUpdateEntry> entries,
        CancellationToken cancellationToken = default)
        => RelationalDependencies.BatchExecutor.ExecuteAsync(
            RelationalDependencies.BatchPreparer.BatchCommands(entries, UpdateAdapter),
            RelationalDependencies.Connection,
            cancellationToken);
}
