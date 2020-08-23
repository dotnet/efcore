// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Cosmos DB specific extension methods for LINQ queries.
    /// </summary>
    public static class CosmosQueryableExtensions
    {
        internal static readonly MethodInfo WithPartitionKeyMethodInfo
            = typeof(CosmosQueryableExtensions)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(WithPartitionKey));

        /// <summary>
        ///     Specify the partition key for partition used for the query. Required when using
        ///     a resource token that provides permission based on a partition key for authentication,
        /// </summary>
        /// <typeparam name="TEntity"> The type of entity being queried. </typeparam>
        /// <param name="source"> The source query. </param>
        /// <param name="partitionKey"> The partition key. </param>
        /// <returns> A new query with the set partition key. </returns>
        public static IQueryable<TEntity> WithPartitionKey<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotNull] [NotParameterized] string partitionKey)
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
    }
}
