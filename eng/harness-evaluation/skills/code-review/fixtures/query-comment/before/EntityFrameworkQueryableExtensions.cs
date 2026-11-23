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
}