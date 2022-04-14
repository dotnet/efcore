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
public abstract class Database : IDatabase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Database" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    protected Database(DatabaseDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual DatabaseDependencies Dependencies { get; }

    /// <summary>
    ///     Persists changes from the supplied entries to the database.
    /// </summary>
    /// <param name="entries">Entries representing the changes to be persisted.</param>
    /// <returns>The number of state entries persisted to the database.</returns>
    public abstract int SaveChanges(IList<IUpdateEntry> entries);

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
    public abstract Task<int> SaveChangesAsync(
        IList<IUpdateEntry> entries,
        CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public virtual Func<QueryContext, TResult> CompileQuery<TResult>(Expression query, bool async)
        => Dependencies.QueryCompilationContextFactory
            .Create(async)
            .CreateQueryExecutor<TResult>(query);
}
