// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     Cosmos-specific extension methods for LINQ queries.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>, and
///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
/// </remarks>
public static class CosmosQueryableExtensions
{
    internal static readonly MethodInfo WithPartitionKeyMethodInfo1
        = typeof(CosmosQueryableExtensions).GetTypeInfo()
            .GetDeclaredMethods(nameof(WithPartitionKey))
            .Single(mi => mi.GetParameters().Length == 2);

    internal static readonly MethodInfo WithPartitionKeyMethodInfo2
        = typeof(CosmosQueryableExtensions).GetTypeInfo()
            .GetDeclaredMethods(nameof(WithPartitionKey))
            .Single(mi => mi.GetParameters().Length == 3);

    internal static readonly MethodInfo WithPartitionKeyMethodInfo3
        = typeof(CosmosQueryableExtensions).GetTypeInfo()
            .GetDeclaredMethods(nameof(WithPartitionKey))
            .Single(mi => mi.GetParameters().Length == 4);

    /// <summary>
    ///     Specify the partition key for partition used for the query.
    ///     Required when using a resource token that provides permission based on a partition key for authentication,
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="partitionKeyValue">The partition key value.</param>
    /// <returns>A new query with the set partition key.</returns>
    public static IQueryable<TEntity> WithPartitionKey<TEntity>(this IQueryable<TEntity> source, object partitionKeyValue)
        where TEntity : class
    {
        Check.NotNull(partitionKeyValue, nameof(partitionKeyValue));

        return
            source.Provider is EntityQueryProvider
                ? source.Provider.CreateQuery<TEntity>(
                    Expression.Call(
                        instance: null,
                        method: WithPartitionKeyMethodInfo1.MakeGenericMethod(typeof(TEntity)),
                        source.Expression,
                        Expression.Constant(partitionKeyValue, typeof(object))))
                : source;
    }

    /// <summary>
    ///     Specify the partition key for partition used for the query.
    ///     Required when using a resource token that provides permission based on a partition key for authentication,
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="partitionKeyValue1">The first value in a hierarchical partition key.</param>
    /// <param name="partitionKeyValue2">The second value in a hierarchical partition key.</param>
    /// <returns>A new query with the set partition key.</returns>
    public static IQueryable<TEntity> WithPartitionKey<TEntity>(
        this IQueryable<TEntity> source,
        object partitionKeyValue1,
        object partitionKeyValue2)
        where TEntity : class
    {
        Check.NotNull(partitionKeyValue1, nameof(partitionKeyValue1));
        Check.NotNull(partitionKeyValue2, nameof(partitionKeyValue2));

        return
            source.Provider is EntityQueryProvider
                ? source.Provider.CreateQuery<TEntity>(
                    Expression.Call(
                        instance: null,
                        method: WithPartitionKeyMethodInfo2.MakeGenericMethod(typeof(TEntity)),
                        source.Expression,
                        Expression.Constant(partitionKeyValue1, typeof(object)),
                        Expression.Constant(partitionKeyValue2, typeof(object))))
                : source;
    }

    /// <summary>
    ///     Specify the partition key for partition used for the query.
    ///     Required when using a resource token that provides permission based on a partition key for authentication,
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
    /// <param name="source">The source query.</param>
    /// <param name="partitionKeyValue1">The first value in a hierarchical partition key.</param>
    /// <param name="partitionKeyValue2">The second value in a hierarchical partition key.</param>
    /// <param name="partitionKeyValue3">The third value in a hierarchical partition key.</param>
    /// <returns>A new query with the set partition key.</returns>
    public static IQueryable<TEntity> WithPartitionKey<TEntity>(
        this IQueryable<TEntity> source,
        object partitionKeyValue1,
        object partitionKeyValue2,
        object partitionKeyValue3)
        where TEntity : class
    {
        Check.NotNull(partitionKeyValue1, nameof(partitionKeyValue1));
        Check.NotNull(partitionKeyValue2, nameof(partitionKeyValue2));
        Check.NotNull(partitionKeyValue3, nameof(partitionKeyValue3));

        return
            source.Provider is EntityQueryProvider
                ? source.Provider.CreateQuery<TEntity>(
                    Expression.Call(
                        instance: null,
                        method: WithPartitionKeyMethodInfo3.MakeGenericMethod(typeof(TEntity)),
                        source.Expression,
                        Expression.Constant(partitionKeyValue1, typeof(object)),
                        Expression.Constant(partitionKeyValue2, typeof(object)),
                        Expression.Constant(partitionKeyValue3, typeof(object))))
                : source;
    }

    /// <summary>
    ///     Creates a LINQ query based on an interpolated string representing a SQL query.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If the database provider supports composing on the supplied SQL, you can compose on top of the raw SQL query using
    ///         LINQ operators.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include interpolated parameter place holders in the SQL query string. Any interpolated parameter values
    ///         you supply will automatically be converted to a Cosmos parameter.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-raw-sql">Executing raw SQL commands with EF Core</see>
    ///         for more information and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TEntity">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">
    ///     An <see cref="IQueryable{T}" /> to use as the base of the interpolated string SQL query (typically a <see cref="DbSet{TEntity}" />).
    /// </param>
    /// <param name="sql">The interpolated string representing a SQL query with parameters.</param>
    /// <returns>An <see cref="IQueryable{T}" /> representing the interpolated string SQL query.</returns>
    public static IQueryable<TEntity> FromSql<TEntity>(
        this DbSet<TEntity> source,
        [NotParameterized] FormattableString sql)
        where TEntity : class
    {
        Check.NotNull(sql, nameof(sql));
        Check.NotEmpty(sql.Format, nameof(source));

        var queryableSource = (IQueryable)source;
        return queryableSource.Provider.CreateQuery<TEntity>(
            GenerateFromSqlQueryRoot(
                queryableSource,
                sql.Format,
                sql.GetArguments()));
    }

    /// <summary>
    ///     Creates a LINQ query based on a raw SQL query.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The returned <see cref="IQueryable{T}" /> can be composed over using LINQ to build more complex queries.
    ///     </para>
    ///     <para>
    ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
    ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
    ///         arguments. Any parameter values you supply will automatically be converted to a Cosmos parameter.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>, and
    ///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information
    ///         and examples.
    ///     </para>
    /// </remarks>
    /// <typeparam name="TEntity">The type of the elements of <paramref name="source" />.</typeparam>
    /// <param name="source">
    ///     An <see cref="IQueryable{T}" /> to use as the base of the raw SQL query (typically a <see cref="DbSet{TEntity}" />).
    /// </param>
    /// <param name="sql">The raw SQL query.</param>
    /// <param name="parameters">The values to be assigned to parameters.</param>
    /// <returns>An <see cref="IQueryable{T}" /> representing the raw SQL query.</returns>
    [StringFormatMethod("sql")]
    public static IQueryable<TEntity> FromSqlRaw<TEntity>(
        this DbSet<TEntity> source,
        [NotParameterized] string sql,
        params object?[] parameters)
        where TEntity : class
    {
        Check.NotEmpty(sql, nameof(sql));
        Check.NotNull(parameters, nameof(parameters));

        var queryableSource = (IQueryable)source;
        return queryableSource.Provider.CreateQuery<TEntity>(
            GenerateFromSqlQueryRoot(
                queryableSource,
                sql,
                parameters));
    }

    private static FromSqlQueryRootExpression GenerateFromSqlQueryRoot(
        IQueryable source,
        string sql,
        object?[] arguments)
    {
        var entityQueryRootExpression = (EntityQueryRootExpression)source.Expression;

        var entityType = entityQueryRootExpression.EntityType;

        Check.DebugAssert(
            (entityType.BaseType is null && !entityType.GetDirectlyDerivedTypes().Any())
            || entityType.FindDiscriminatorProperty() is not null,
            "Found FromSql on a TPT entity type, but TPT isn't supported on Cosmos");

        return new FromSqlQueryRootExpression(
            entityQueryRootExpression.QueryProvider!,
            entityType,
            sql,
            Expression.Constant(arguments));
    }

    internal static readonly MethodInfo ToPageAsyncMethodInfo
        = typeof(CosmosQueryableExtensions).GetMethod(nameof(ToPageAsync))!;

    /// <summary>
    ///     Allows paginating through query results by repeatedly executing the same query, passing continuation tokens to retrieve
    ///     successive pages of the result set, and specifying the maximum number of results per page.
    /// </summary>
    /// <param name="source">The source query.</param>
    /// <param name="continuationToken">
    ///     An optional continuation token returned from a previous execution of this query via
    ///     <see cref="CosmosPage{T}.ContinuationToken" />. If <see langword="null" />, retrieves query results from the start.
    /// </param>
    /// <param name="pageSize">
    ///     The maximum number of results in the returned <see cref="CosmosPage{T}" />. The page may contain fewer results if the database
    ///     did not contain enough matching results.
    /// </param>
    /// <param name="responseContinuationTokenLimitInKb">Limits the length of continuation token in the query response.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="CosmosPage{T}" /> containing at most <paramref name="pageSize" /> results.</returns>
    [Experimental(EFDiagnostics.PagingExperimental)]
    public static Task<CosmosPage<TSource>> ToPageAsync<TSource>(
        this IQueryable<TSource> source,
        int pageSize,
        string? continuationToken,
        int? responseContinuationTokenLimitInKb = null,
        CancellationToken cancellationToken = default)
    {
        if (source.Provider is not IAsyncQueryProvider provider)
        {
            throw new InvalidOperationException(CoreStrings.IQueryableProviderNotAsync);
        }

        return provider.ExecuteAsync<Task<CosmosPage<TSource>>>(
            Expression.Call(
                instance: null,
                method: ToPageAsyncMethodInfo.MakeGenericMethod(typeof(TSource)),
                arguments:
                [
                    source.Expression,
                    Expression.Constant(pageSize, typeof(int)),
                    Expression.Constant(continuationToken, typeof(string)),
                    Expression.Constant(responseContinuationTokenLimitInKb, typeof(int?)),
                    Expression.Constant(default(CancellationToken), typeof(CancellationToken))
                ]),
            cancellationToken);
    }
}
