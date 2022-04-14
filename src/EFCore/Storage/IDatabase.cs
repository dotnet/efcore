// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         The main interaction point between a context and the database provider.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
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
public interface IDatabase
{
    /// <summary>
    ///     Persists changes from the supplied entries to the database.
    /// </summary>
    /// <param name="entries">Entries representing the changes to be persisted.</param>
    /// <returns>The number of state entries persisted to the database.</returns>
    int SaveChanges(IList<IUpdateEntry> entries);

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
    Task<int> SaveChangesAsync(
        IList<IUpdateEntry> entries,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Compiles the given query to generate a <see cref="Func{QueryContext, TResult}" />.
    /// </summary>
    /// <typeparam name="TResult">The type of query result.</typeparam>
    /// <param name="query">The query to compile.</param>
    /// <param name="async">A value indicating whether this is an async query.</param>
    /// <returns>A <see cref="Func{QueryContext, TResult}" /> which can be invoked to get results of the query.</returns>
    Func<QueryContext, TResult> CompileQuery<TResult>(Expression query, bool async);
}
