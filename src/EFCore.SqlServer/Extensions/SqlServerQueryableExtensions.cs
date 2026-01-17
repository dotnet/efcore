// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server specific extension methods for LINQ queries.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SqlServerQueryableExtensions
{
    private static readonly MethodInfo _containsTableMethod
        = typeof(SqlServerQueryableExtensions).GetMethod(nameof(ContainsTable))!;

    private static readonly MethodInfo _freeTextTableMethod
        = typeof(SqlServerQueryableExtensions).GetMethod(nameof(FreeTextTable))!;

    /// <summary>
    ///     Provides a mapping to the SQL Server CONTAINSTABLE full-text search function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="propertySelector">A lambda expression selecting the property to search.</param>
    /// <param name="searchCondition">The search condition.</param>
    /// <returns>The table-valued function result containing Key and Rank columns.</returns>
    public static IQueryable<FullTextTableResult<TKey>> ContainsTable<TEntity, TKey>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, object>> propertySelector,
        string searchCondition)
        where TEntity : class
        => source.Provider.CreateQuery<FullTextTableResult<TKey>>(
            Expression.Call(
                null,
                _containsTableMethod.MakeGenericMethod(typeof(TEntity), typeof(TKey)),
                source.Expression,
                propertySelector,
                Expression.Constant(searchCondition)));

    /// <summary>
    ///     Provides a mapping to the SQL Server FREETEXTTABLE full-text search function.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-database-functions">Database functions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-sqlserver">Accessing SQL Server and Azure SQL databases with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="propertySelector">A lambda expression selecting the property to search.</param>
    /// <param name="freeText">The text that will be searched for in the property.</param>
    /// <returns>The table-valued function result containing Key and Rank columns.</returns>
    public static IQueryable<FullTextTableResult<TKey>> FreeTextTable<TEntity, TKey>(
        this IQueryable<TEntity> source,
        Expression<Func<TEntity, object>> propertySelector,
        string freeText)
        where TEntity : class
        => source.Provider.CreateQuery<FullTextTableResult<TKey>>(
            Expression.Call(
                null,
                _freeTextTableMethod.MakeGenericMethod(typeof(TEntity), typeof(TKey)),
                source.Expression,
                propertySelector,
                Expression.Constant(freeText)));

    /// <summary>
    ///     The result type for SQL Server full-text table-valued functions (CONTAINSTABLE / FREETEXTTABLE).
    /// </summary>
    /// <typeparam name="TKey">The type of the key column.</typeparam>
    public class FullTextTableResult<TKey>(TKey key, int rank)
    {
        /// <summary>
        ///     The key of the row matched.
        /// </summary>
        public TKey Key { get; } = key;

        /// <summary>
        ///     The ranking value assigned to the row.
        /// </summary>
        public int Rank { get; } = rank;
    }
}
