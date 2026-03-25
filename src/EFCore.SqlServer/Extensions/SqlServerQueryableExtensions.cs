// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.Internal;

#pragma warning disable IDE0130 // Namespace does not match folder structure

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     SQL Server extension methods for LINQ queries.
/// </summary>
public static class SqlServerQueryableExtensions
{
    #region VectorSearch

    /// <summary>
    ///     Search for vectors similar to a given query vector using an approximate nearest neighbors vector search algorithm.
    /// </summary>
    /// <param name="source">The <see cref="DbSet{T}" /> representing the table containing the vector column to query.</param>
    /// <param name="vectorPropertySelector">A selector for the vector property on the entity.</param>
    /// <param name="similarTo">The vector used for search, either a parameter or another vector column.</param>
    /// <param name="metric">
    ///     The distance metric used to calculate the distance between the query vector and the vectors in the specified column.
    ///     An ANN (Approximate Nearest Neighbor) index is used only if a matching ANN index, with the same metric and on the same column,
    ///     is found. If there are no compatible ANN indexes, a warning is raised and the KNN (k-Nearest Neighbor) algorithm is used.
    /// </param>
    /// <param name="topN">The maximum number of similar vectors that must be returned. It must be a positive integer.</param>
    /// <seealso href="https://learn.microsoft.com/sql/t-sql/functions/vector-search-transact-sql">
    ///     SQL Server documentation for <c>VECTOR_SEARCH()</c>.
    /// </seealso>
    /// <seealso href="https://learn.microsoft.com/sql/relational-databases/vectors/vectors-sql-server">Vectors in the SQL Database Engine.</seealso>
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    public static IQueryable<VectorSearchResult<T>> VectorSearch<T, TVector>(
        this DbSet<T> source,
        Expression<Func<T, TVector>> vectorPropertySelector,
        TVector similarTo,
        [NotParameterized] string metric,
        int topN)
        where T : class
        where TVector : unmanaged
    {
        var queryableSource = (IQueryable)source;
        var root = (EntityQueryRootExpression)queryableSource.Expression;

        return queryableSource.Provider is EntityQueryProvider
            ? queryableSource.Provider.CreateQuery<VectorSearchResult<T>>(
                Expression.Call(
                    // Note that the method used is the one below, accepting IQueryable<T>, not DbSet<T>
                    method: new Func<IQueryable<T>, Expression<Func<T, TVector>>, TVector, string, int, IQueryable<VectorSearchResult<T>>>(VectorSearch).Method,
                    root,
                    Expression.Quote(vectorPropertySelector),
                    Expression.Constant(similarTo),
                    Expression.Constant(metric),
                    Expression.Constant(topN)))
            : throw new InvalidOperationException(CoreStrings.FunctionOnNonEfLinqProvider(nameof(VectorSearch)));
    }

    // A separate method stub is required since the public method accepts DbSet (to limit to direct usage on DbSets),
    // but the MethodCallExpression built above needs to accept an EntityQueryRootExpression (which is what we get for
    // a DbSet, but which isn't itself a DbSet).
    [Experimental(EFDiagnostics.SqlServerVectorSearch)]
    private static IQueryable<VectorSearchResult<T>> VectorSearch<T, TVector>(
        this IQueryable<T> source,
        Expression<Func<T, TVector>> vectorPropertySelector,
        TVector similarTo,
        [NotParameterized] string metric,
        int topN)
        where T : class
        where TVector : unmanaged
        => throw new UnreachableException();

    #endregion VectorSearch

    #region Full-text search TVFs

    /// <summary>
    ///     Queries a full-text index using the SQL Server <c>FREETEXTTABLE</c> function with a property selector.
    /// </summary>
    /// <typeparam name="T">The entity type being queried.</typeparam>
    /// <typeparam name="TKey">The type of the full-text key column.</typeparam>
    /// <param name="source">The <see cref="DbSet{T}" /> representing the table with the full-text index.</param>
    /// <param name="columnSelector">
    ///     A selector for the column(s) to search. Can be a single property (e.g., <c>e =&gt; e.Title</c>)
    ///     or multiple properties via an anonymous type (e.g., <c>e =&gt; new { e.Title, e.Body }</c>).
    /// </param>
    /// <param name="freeText">The text to search for.</param>
    /// <param name="languageTerm">Optional language term from <c>sys.syslanguages</c> for word-breaking.</param>
    /// <param name="topN">Optional maximum number of results to return.</param>
    /// <returns>An <see cref="IQueryable{T}" /> of <see cref="FullTextSearchResult{TKey}" /> containing the key and rank of matching rows.</returns>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="FullTextSearchResult{TKey}.Key" /> property to join back to the original table
    ///         to retrieve the full entity data.
    ///     </para>
    ///     <para>
    ///         See <see href="https://learn.microsoft.com/sql/relational-databases/system-functions/freetexttable-transact-sql">
    ///         SQL Server documentation for <c>FREETEXTTABLE</c></see> for more information.
    ///     </para>
    /// </remarks>
    public static IQueryable<FullTextSearchResult<TKey>> FreeTextTable<T, TKey>(
        this DbSet<T> source,
        Expression<Func<T, object>> columnSelector,
        string freeText,
        [NotParameterized] string? languageTerm = null,
        int? topN = null)
        where T : class
    {
        var queryableSource = (IQueryable)source;
        var root = (EntityQueryRootExpression)queryableSource.Expression;

        return queryableSource.Provider is EntityQueryProvider
            ? queryableSource.Provider.CreateQuery<FullTextSearchResult<TKey>>(
                Expression.Call(
                    method: new Func<
                        IQueryable<T>,
                        Expression<Func<T, object>>, string, string?, int?,
                        IQueryable<FullTextSearchResult<TKey>>>(FreeTextTable<T, TKey>).Method,
                    root,
                    Expression.Quote(columnSelector),
                    Expression.Constant(freeText),
                    Expression.Constant(languageTerm, typeof(string)),
                    Expression.Constant(topN, typeof(int?))))
            : throw new InvalidOperationException(CoreStrings.FunctionOnNonEfLinqProvider(nameof(FreeTextTable)));
    }

    /// <summary>
    ///     Queries a full-text index using the SQL Server <c>FREETEXTTABLE</c> function, searching all full-text indexed columns.
    /// </summary>
    /// <typeparam name="T">The entity type being queried.</typeparam>
    /// <typeparam name="TKey">The type of the full-text key column.</typeparam>
    /// <param name="source">The <see cref="DbSet{T}" /> representing the table with the full-text index.</param>
    /// <param name="freeText">The text to search for.</param>
    /// <param name="languageTerm">Optional language term from <c>sys.syslanguages</c> for word-breaking.</param>
    /// <param name="topN">Optional maximum number of results to return.</param>
    /// <returns>An <see cref="IQueryable{T}" /> of <see cref="FullTextSearchResult{TKey}" /> containing the key and rank of matching rows.</returns>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="FullTextSearchResult{TKey}.Key" /> property to join back to the original table
    ///         to retrieve the full entity data.
    ///     </para>
    ///     <para>
    ///         See <see href="https://learn.microsoft.com/sql/relational-databases/system-functions/freetexttable-transact-sql">
    ///         SQL Server documentation for <c>FREETEXTTABLE</c></see> for more information.
    ///     </para>
    /// </remarks>
    public static IQueryable<FullTextSearchResult<TKey>> FreeTextTable<T, TKey>(
        this DbSet<T> source,
        string freeText,
        [NotParameterized] string? languageTerm = null,
        int? topN = null)
        where T : class
    {
        var queryableSource = (IQueryable)source;
        var root = (EntityQueryRootExpression)queryableSource.Expression;

        return queryableSource.Provider is EntityQueryProvider
            ? queryableSource.Provider.CreateQuery<FullTextSearchResult<TKey>>(
                Expression.Call(
                    method: new Func<
                        IQueryable<T>,
                        string,
                        string?,
                        int?,
                        IQueryable<FullTextSearchResult<TKey>>>(FreeTextTable<T, TKey>).Method,
                    root,
                    Expression.Constant(freeText),
                    Expression.Constant(languageTerm, typeof(string)),
                    Expression.Constant(topN, typeof(int?))))
            : throw new InvalidOperationException(CoreStrings.FunctionOnNonEfLinqProvider(nameof(FreeTextTable)));
    }

    // A separate method stub is required since the public method accepts DbSet (to limit to direct usage on DbSets),
    // but the MethodCallExpression built above needs to accept an EntityQueryRootExpression (which is what we get for
    // a DbSet, but which isn't itself a DbSet).
    private static IQueryable<FullTextSearchResult<TKey>> FreeTextTable<T, TKey>(
        this IQueryable<T> source,
        Expression<Func<T, object>> columnSelector,
        string freeText,
        [NotParameterized] string? languageTerm,
        int? topN)
        where T : class
        => throw new UnreachableException();

    private static IQueryable<FullTextSearchResult<TKey>> FreeTextTable<T, TKey>(
        this IQueryable<T> source,
        string freeText,
        [NotParameterized] string? languageTerm,
        int? topN)
        where T : class
        => throw new UnreachableException();

    /// <summary>
    ///     Queries a full-text index using the SQL Server <c>CONTAINSTABLE</c> function with a property selector.
    /// </summary>
    /// <typeparam name="T">The entity type being queried.</typeparam>
    /// <typeparam name="TKey">The type of the full-text key column.</typeparam>
    /// <param name="source">The <see cref="DbSet{T}" /> representing the table with the full-text index.</param>
    /// <param name="columnSelector">
    ///     A selector for the column(s) to search. Can be a single property (e.g., <c>e =&gt; e.Title</c>)
    ///     or multiple properties via an anonymous type (e.g., <c>e =&gt; new { e.Title, e.Body }</c>).
    /// </param>
    /// <param name="searchCondition">The search condition, supporting full-text predicates like <c>AND</c>, <c>OR</c>, <c>NEAR</c>, etc.</param>
    /// <param name="languageTerm">Optional language term from <c>sys.syslanguages</c> for word-breaking.</param>
    /// <param name="topN">Optional maximum number of results to return.</param>
    /// <returns>An <see cref="IQueryable{T}" /> of <see cref="FullTextSearchResult{TKey}" /> containing the key and rank of matching rows.</returns>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="FullTextSearchResult{TKey}.Key" /> property to join back to the original table
    ///         to retrieve the full entity data.
    ///     </para>
    ///     <para>
    ///         See <see href="https://learn.microsoft.com/sql/relational-databases/system-functions/containstable-transact-sql">
    ///         SQL Server documentation for <c>CONTAINSTABLE</c></see> for more information.
    ///     </para>
    /// </remarks>
    public static IQueryable<FullTextSearchResult<TKey>> ContainsTable<T, TKey>(
        this DbSet<T> source,
        Expression<Func<T, object>> columnSelector,
        string searchCondition,
        [NotParameterized] string? languageTerm = null,
        int? topN = null)
        where T : class
    {
        var queryableSource = (IQueryable)source;
        var root = (EntityQueryRootExpression)queryableSource.Expression;

        return queryableSource.Provider is EntityQueryProvider
            ? queryableSource.Provider.CreateQuery<FullTextSearchResult<TKey>>(
                Expression.Call(
                    method: new Func<IQueryable<T>, Expression<Func<T, object>>, string, string?, int?, IQueryable<FullTextSearchResult<TKey>>>(ContainsTable<T, TKey>).Method,
                    root,
                    Expression.Quote(columnSelector),
                    Expression.Constant(searchCondition),
                    Expression.Constant(languageTerm, typeof(string)),
                    Expression.Constant(topN, typeof(int?))))
            : throw new InvalidOperationException(CoreStrings.FunctionOnNonEfLinqProvider(nameof(ContainsTable)));
    }

    /// <summary>
    ///     Queries a full-text index using the SQL Server <c>CONTAINSTABLE</c> function, searching all full-text indexed columns.
    /// </summary>
    /// <typeparam name="T">The entity type being queried.</typeparam>
    /// <typeparam name="TKey">The type of the full-text key column.</typeparam>
    /// <param name="source">The <see cref="DbSet{T}" /> representing the table with the full-text index.</param>
    /// <param name="searchCondition">The search condition, supporting full-text predicates like <c>AND</c>, <c>OR</c>, <c>NEAR</c>, etc.</param>
    /// <param name="languageTerm">Optional language term from <c>sys.syslanguages</c> for word-breaking.</param>
    /// <param name="topN">Optional maximum number of results to return.</param>
    /// <returns>An <see cref="IQueryable{T}" /> of <see cref="FullTextSearchResult{TKey}" /> containing the key and rank of matching rows.</returns>
    /// <remarks>
    ///     <para>
    ///         Use the <see cref="FullTextSearchResult{TKey}.Key" /> property to join back to the original table
    ///         to retrieve the full entity data.
    ///     </para>
    ///     <para>
    ///         See <see href="https://learn.microsoft.com/sql/relational-databases/system-functions/containstable-transact-sql">
    ///         SQL Server documentation for <c>CONTAINSTABLE</c></see> for more information.
    ///     </para>
    /// </remarks>
    public static IQueryable<FullTextSearchResult<TKey>> ContainsTable<T, TKey>(
        this DbSet<T> source,
        string searchCondition,
        [NotParameterized] string? languageTerm = null,
        int? topN = null)
        where T : class
    {
        var queryableSource = (IQueryable)source;
        var root = (EntityQueryRootExpression)queryableSource.Expression;

        return queryableSource.Provider is EntityQueryProvider
            ? queryableSource.Provider.CreateQuery<FullTextSearchResult<TKey>>(
                Expression.Call(
                    method: new Func<IQueryable<T>, string, string?, int?, IQueryable<FullTextSearchResult<TKey>>>(ContainsTable<T, TKey>).Method,
                    root,
                    Expression.Constant(searchCondition),
                    Expression.Constant(languageTerm, typeof(string)),
                    Expression.Constant(topN, typeof(int?))))
            : throw new InvalidOperationException(CoreStrings.FunctionOnNonEfLinqProvider(nameof(ContainsTable)));
    }

    private static IQueryable<FullTextSearchResult<TKey>> ContainsTable<T, TKey>(
        this IQueryable<T> source,
        Expression<Func<T, object>> columnSelector,
        string searchCondition,
        [NotParameterized] string? languageTerm,
        int? topN)
        where T : class
        => throw new UnreachableException();

    private static IQueryable<FullTextSearchResult<TKey>> ContainsTable<T, TKey>(
        this IQueryable<T> source,
        string searchCondition,
        [NotParameterized] string? languageTerm,
        int? topN)
        where T : class
        => throw new UnreachableException();

    #endregion Full-text search TVFs
}
