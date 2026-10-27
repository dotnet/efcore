using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore;

public static class EntityFrameworkQueryableExtensions
{
    /// <summary>
    ///     Adds a tag to the collection of tags associated with an EF LINQ query.
    /// </summary>
    public static IQueryable<TEntity> TagWith<TEntity>(
        this IQueryable<TEntity> source,
        [NotParameterized] string tag)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        return source.Provider is EntityQueryProvider
            ? source.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    null,
                    new Func<IQueryable<TEntity>, string, IQueryable<TEntity>>(TagWith).Method,
                    source.Expression,
                    Expression.Constant(tag)))
            : source;
    }

    /// <summary>
    ///     Adds a formatted comment before the SQL generated for a LINQ query.
    /// </summary>
    /// <remarks>
    ///     Comments are informational annotations. They do not change the query's result or filtering.
    ///     Relational providers emit short single-line comments compactly and other comments as lines.
    ///     Multiple calls retain their order. Non-EF query providers return the source unchanged.
    /// </remarks>
    /// <typeparam name="TEntity">The type of the query elements.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="comment">The text to include before the query.</param>
    /// <returns>A query carrying the comment.</returns>
    /// <exception cref="ArgumentNullException">The source or comment is null.</exception>
    /// <exception cref="ArgumentException">The comment is empty or whitespace.</exception>
    public static IQueryable<TEntity> Comment<TEntity>(
        this IQueryable<TEntity> source,
        [NotParameterized] string comment)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);

        return source.Provider is EntityQueryProvider
            ? source.Provider.CreateQuery<TEntity>(
                Expression.Call(
                    null,
                    new Func<IQueryable<TEntity>, string, IQueryable<TEntity>>(Comment).Method,
                    source.Expression,
                    Expression.Constant(comment)))
            : source;
    }
}