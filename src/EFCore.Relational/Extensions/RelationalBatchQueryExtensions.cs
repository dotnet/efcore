// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Relational database specific extension methods for executing multiple LINQ queries in a single database round-trip.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see> for more information and examples.
/// </remarks>
public static class RelationalBatchQueryExtensions
{
    /// <summary>
    ///     Executes the given queries as a single batch in one database round-trip, returning a
    ///     <see cref="BatchResultsReader" /> from which each query's results can be consumed in order.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All queries are sent to the database in one command; the result sets must be consumed in the order
    ///         in which the queries were supplied. Results can be streamed - rows are only read from the database
    ///         as they are consumed.
    ///     </para>
    ///     <para>
    ///         Queries that do not translate to a single self-contained SQL statement cannot be batched: split
    ///         queries, streaming GroupBy queries and non-composed FromSql queries are not supported. When a
    ///         retrying execution strategy is configured, all result sets are buffered eagerly during execution
    ///         so that the execution can be retried; otherwise results are streamed.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <param name="context">The context to execute the batch against.</param>
    /// <param name="queries">The queries to execute, each created from the supplied context.</param>
    /// <returns>A reader providing sequential access to each query's results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context" /> or <paramref name="queries" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">A query cannot be executed as part of a batch.</exception>
    public static BatchResultsReader Batch<TContext>(
        this TContext context,
        params Func<TContext, IQueryable>[] queries)
        where TContext : DbContext
    {
        Check.NotNull(context);
        Check.NotNull(queries);

        return RelationalBatchQueryExecutor.Execute(context, GetBatchableQueries(context, queries));
    }

    /// <summary>
    ///     Asynchronously executes the given queries as a single batch in one database round-trip, returning a
    ///     <see cref="BatchResultsReader" /> from which each query's results can be consumed in order.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All queries are sent to the database in one command; the result sets must be consumed in the order
    ///         in which the queries were supplied. Results can be streamed - rows are only read from the database
    ///         as they are consumed.
    ///     </para>
    ///     <para>
    ///         Queries that do not translate to a single self-contained SQL statement cannot be batched: split
    ///         queries, streaming GroupBy queries and non-composed FromSql queries are not supported. When a
    ///         retrying execution strategy is configured, all result sets are buffered eagerly during execution
    ///         so that the execution can be retried; otherwise results are streamed.
    ///     </para>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use 'await' to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///         See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <param name="context">The context to execute the batch against.</param>
    /// <param name="queries">The queries to execute, each created from the supplied context.</param>
    /// <returns>A task containing a reader providing sequential access to each query's results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context" /> or <paramref name="queries" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">A query cannot be executed as part of a batch.</exception>
    public static Task<BatchResultsReader> BatchAsync<TContext>(
        this TContext context,
        params Func<TContext, IQueryable>[] queries)
        where TContext : DbContext
        => context.BatchAsync(queries, default);

    /// <summary>
    ///     Asynchronously executes the given queries as a single batch in one database round-trip, returning a
    ///     <see cref="BatchResultsReader" /> from which each query's results can be consumed in order.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         All queries are sent to the database in one command; the result sets must be consumed in the order
    ///         in which the queries were supplied. Results can be streamed - rows are only read from the database
    ///         as they are consumed.
    ///     </para>
    ///     <para>
    ///         Multiple active operations on the same context instance are not supported. Use 'await' to ensure
    ///         that any asynchronous operations have completed before calling another method on this context.
    ///         See <see href="https://aka.ms/efcore-docs-threading">Avoiding DbContext threading issues</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <param name="context">The context to execute the batch against.</param>
    /// <param name="queries">The queries to execute, each created from the supplied context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task containing a reader providing sequential access to each query's results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context" /> or <paramref name="queries" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">A query cannot be executed as part of a batch.</exception>
    /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
    public static Task<BatchResultsReader> BatchAsync<TContext>(
        this TContext context,
        IEnumerable<Func<TContext, IQueryable>> queries,
        CancellationToken cancellationToken)
        where TContext : DbContext
    {
        Check.NotNull(context);
        Check.NotNull(queries);

        return RelationalBatchQueryExecutor.ExecuteAsync(context, GetBatchableQueries(context, queries), cancellationToken);
    }

    private static IReadOnlyList<IRelationalBatchQueryingEnumerable> GetBatchableQueries<TContext>(
        TContext context,
        IEnumerable<Func<TContext, IQueryable>> queries)
        where TContext : DbContext
    {
        var batchableQueries = new List<IRelationalBatchQueryingEnumerable>();

        foreach (var queryFunc in queries)
        {
            Check.NotNull(queryFunc, nameof(queries));

            var queryable = queryFunc(context);
            var enumerable = queryable.Provider.Execute<IEnumerable>(queryable.Expression);

            batchableQueries.Add(
                enumerable switch
                {
                    IRelationalBatchQueryingEnumerable batchable => batchable,
                    IRelationalQueryingEnumerable => throw new InvalidOperationException(
                        RelationalStrings.BatchQueryUnsupportedQueryType),
                    _ => throw new InvalidOperationException(RelationalStrings.BatchQueryRequiresRelationalProvider)
                });
        }

        if (batchableQueries.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.BatchQueryEmpty);
        }

        return batchableQueries;
    }
}
