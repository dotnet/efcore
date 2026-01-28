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
}

/// <summary>
///     Represents the results from a call to
///     <see cref="SqlServerQueryableExtensions.VectorSearch{T, TVector}(DbSet{T}, Expression{Func{T, TVector}}, TVector, string, int)" />.
/// </summary>
[Experimental(EFDiagnostics.SqlServerVectorSearch)]
public readonly struct VectorSearchResult<T>(T value, double distance)
{
    /// <summary>
    ///     The entity instance representing the row with a similar vector.
    /// </summary>
    public T Value { get; } = value;

    /// <summary>
    ///    The distance between the query vector and the similar vector.
    /// </summary>
    public double Distance { get; } = distance;
}
