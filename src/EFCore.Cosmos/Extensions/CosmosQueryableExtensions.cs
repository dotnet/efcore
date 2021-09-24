// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Cosmos-specific extension methods for LINQ queries.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information.
    /// </remarks>
    public static class CosmosQueryableExtensions
    {
        internal static readonly MethodInfo WithPartitionKeyMethodInfo
            = typeof(CosmosQueryableExtensions).GetRequiredDeclaredMethod(nameof(WithPartitionKey));

        /// <summary>
        ///     Specify the partition key for partition used for the query. Required when using
        ///     a resource token that provides permission based on a partition key for authentication,
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>, and
        ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information.
        /// </remarks>
        /// <typeparam name="TEntity">The type of entity being queried.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="partitionKey">The partition key.</param>
        /// <returns>A new query with the set partition key.</returns>
        public static IQueryable<TEntity> WithPartitionKey<TEntity>(
            this IQueryable<TEntity> source,
            [NotParameterized] string partitionKey)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(partitionKey, nameof(partitionKey));

            return
                source.Provider is EntityQueryProvider
                    ? source.Provider.CreateQuery<TEntity>(
                        Expression.Call(
                            instance: null,
                            method: WithPartitionKeyMethodInfo.MakeGenericMethod(typeof(TEntity)),
                            source.Expression,
                            Expression.Constant(partitionKey)))
                    : source;
        }

        /// <summary>
        ///     <para>
        ///         Creates a LINQ query based on a raw SQL query.
        ///     </para>
        ///     <para>
        ///         You can compose on top of the raw SQL query using LINQ operators:
        ///     </para>
        ///     <code>context.Blogs.FromSqlRaw("SELECT * FROM root c).OrderBy(b => b.Name)</code>
        ///     <para>
        ///         As with any API that accepts SQL it is important to parameterize any user input to protect against a SQL injection
        ///         attack. You can include parameter place holders in the SQL query string and then supply parameter values as additional
        ///         arguments. Any parameter values you supply will automatically be converted to a Cosmos parameter:
        ///     </para>
        ///     <code>context.Blogs.FromSqlRaw(""SELECT * FROM root c WHERE c["Name"] = {0})", userSuppliedSearchTerm)</code>
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-query">Querying data with EF Core</see>, and
        ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information.
        /// </remarks>
        /// <typeparam name="TEntity">The type of the elements of <paramref name="source" />.</typeparam>
        /// <param name="source">
        ///     An <see cref="IQueryable{T}" /> to use as the base of the raw SQL query (typically a <see cref="DbSet{TEntity}" />).
        ///</param>
        /// <param name="sql">The raw SQL query.</param>
        /// <param name="parameters">The values to be assigned to parameters.</param>
        /// <returns>An <see cref="IQueryable{T}" /> representing the raw SQL query.</returns>
        [StringFormatMethod("sql")]
        public static IQueryable<TEntity> FromSqlRaw<TEntity>(
            this DbSet<TEntity> source,
            [NotParameterized] string sql,
            params object[] parameters)
            where TEntity : class
        {
            Check.NotNull(source, nameof(source));
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(parameters, nameof(parameters));

            var queryableSource = (IQueryable)source;
            var queryRootExpression = (QueryRootExpression)queryableSource.Expression;

            var entityType = queryRootExpression.EntityType;

            Check.DebugAssert(
                (entityType.BaseType is null && !entityType.GetDirectlyDerivedTypes().Any())
                || entityType.FindDiscriminatorProperty() is not null,
                "Found FromSql on a TPT entity type, but TPT isn't supported on Cosmos");

            var fromSqlQueryRootExpression = new FromSqlQueryRootExpression(
                queryRootExpression.QueryProvider!,
                entityType,
                sql,
                Expression.Constant(parameters));

            return queryableSource.Provider.CreateQuery<TEntity>(fromSqlQueryRootExpression);
        }
    }
}
