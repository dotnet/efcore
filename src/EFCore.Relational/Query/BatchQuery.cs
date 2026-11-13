// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Creates <see cref="BatchQuery{TContext, TResult}" /> instances, which carry a batchable query together
///     with its result type.
/// </summary>
/// <remarks>
///     Wrapping a query with <see cref="Create{TContext, TResult}" /> allows its results to be consumed from a
///     <see cref="BatchResultsReader" /> without spelling out the element type, which makes anonymous-type
///     projections consumable: pass the same instance to <see cref="O:RelationalBatchQueryExtensions.Batch" />
///     and to <see cref="BatchResultsReader.NextAsList{TContext, TResult}(BatchQuery{TContext, TResult})" />
///     (or one of its counterparts), and the element type is inferred.
/// </remarks>
public static class BatchQuery
{
    /// <summary>
    ///     Creates a <see cref="BatchQuery{TContext, TResult}" /> carrying the given query together with its
    ///     result type.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <typeparam name="TResult">The element type of the query.</typeparam>
    /// <param name="query">The query, created from the context supplied to the batch.</param>
    /// <returns>The wrapped query.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query" /> is <see langword="null" />.</exception>
    public static BatchQuery<TContext, TResult> Create<TContext, TResult>(
        Expression<Func<TContext, IQueryable<TResult>>> query)
        where TContext : DbContext
    {
        Check.NotNull(query);

        return new BatchQuery<TContext, TResult>(query);
    }
}

/// <summary>
///     A batchable query carrying its result type, created with
///     <see cref="BatchQuery.Create{TContext, TResult}" />.
/// </summary>
/// <remarks>
///     Pass the same instance to <see cref="O:RelationalBatchQueryExtensions.Batch" /> and to the
///     type-inferring result methods of <see cref="BatchResultsReader" /> so the element type - including
///     an anonymous type - never needs to be written out.
/// </remarks>
/// <typeparam name="TContext">The type of the context.</typeparam>
/// <typeparam name="TResult">The element type of the query.</typeparam>
public sealed class BatchQuery<TContext, TResult>
    where TContext : DbContext
{
    internal BatchQuery(Expression<Func<TContext, IQueryable<TResult>>> query)
        => Expression = query;

    /// <summary>
    ///     Gets the query expression.
    /// </summary>
    public Expression<Func<TContext, IQueryable<TResult>>> Expression { get; }

    /// <summary>
    ///     Converts the wrapped query to the expression shape accepted by
    ///     <see cref="O:RelationalBatchQueryExtensions.Batch" /> and
    ///     <see cref="O:RelationalBatchQueryExtensions.BatchAsync" />.
    /// </summary>
    /// <param name="batchQuery">The wrapped query.</param>
    public static implicit operator Expression<Func<TContext, object>>(BatchQuery<TContext, TResult> batchQuery)
    {
        Check.NotNull(batchQuery);

        var expression = batchQuery.Expression;

        return System.Linq.Expressions.Expression.Lambda<Func<TContext, object>>(
            System.Linq.Expressions.Expression.Convert(expression.Body, typeof(object)),
            expression.Parameters);
    }
}
