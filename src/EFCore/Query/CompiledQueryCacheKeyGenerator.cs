// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Creates keys that uniquely identifies a query. This is used to store and lookup
    ///         compiled versions of a query in a cache.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class CompiledQueryCacheKeyGenerator : ICompiledQueryCacheKeyGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CompiledQueryCacheKeyGenerator" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public CompiledQueryCacheKeyGenerator([NotNull] CompiledQueryCacheKeyGeneratorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies used to create a <see cref="CompiledQueryCacheKeyGenerator" />
        /// </summary>
        protected virtual CompiledQueryCacheKeyGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Generates the cache key for the given query.
        /// </summary>
        /// <param name="query"> The query to get the cache key for. </param>
        /// <param name="async"> A value indicating whether the query will be executed asynchronously. </param>
        /// <returns> The cache key. </returns>
        public virtual object GenerateCacheKey(Expression query, bool async)
            => GenerateCacheKeyCore(query, async);

        /// <summary>
        ///     Generates the cache key for the given query.
        /// </summary>
        /// <param name="query"> The query to get the cache key for. </param>
        /// <param name="async"> A value indicating whether the query will be executed asynchronously. </param>
        /// <returns> The cache key. </returns>
        protected CompiledQueryCacheKey GenerateCacheKeyCore([NotNull] Expression query, bool async)
            => new CompiledQueryCacheKey(
                Check.NotNull(query, nameof(query)),
                Dependencies.Model,
                Dependencies.CurrentContext.Context.ChangeTracker.QueryTrackingBehavior,
                async);

        /// <summary>
        ///     <para>
        ///         A key that uniquely identifies a query. This is used to store and lookup
        ///         compiled versions of a query in a cache.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        protected readonly struct CompiledQueryCacheKey
        {
            private readonly Expression _query;
            private readonly IModel _model;
            private readonly QueryTrackingBehavior _queryTrackingBehavior;
            private readonly bool _async;

            /// <summary>
            ///     Initializes a new instance of the <see cref="CompiledQueryCacheKey" /> class.
            /// </summary>
            /// <param name="query"> The query to generate the key for. </param>
            /// <param name="model"> The model that queries is written against. </param>
            /// <param name="queryTrackingBehavior"> The tracking behavior for results of the query. </param>
            /// <param name="async"> A value indicating whether the query will be executed asynchronously. </param>
            public CompiledQueryCacheKey(
                [NotNull] Expression query,
                [NotNull] IModel model,
                QueryTrackingBehavior queryTrackingBehavior,
                bool async)
            {
                _query = query;
                _model = model;
                _queryTrackingBehavior = queryTrackingBehavior;
                _async = async;
            }

            /// <summary>
            ///     Determines if this key is equivalent to a given object (i.e. if they are keys for the same query).
            /// </summary>
            /// <param name="obj">
            ///     The object to compare this key to.
            /// </param>
            /// <returns>
            ///     True if the object is a <see cref="CompiledQueryCacheKey" /> and is for the same query, otherwise false.
            /// </returns>
            public override bool Equals(object obj)
            {
                if (obj is null
                    || !(obj is CompiledQueryCacheKey))
                {
                    return false;
                }

                var other = (CompiledQueryCacheKey)obj;

                return ReferenceEquals(_model, other._model)
                       && _queryTrackingBehavior == other._queryTrackingBehavior
                       && _async == other._async
                       && ExpressionEqualityComparer.Instance.Equals(_query, other._query);
            }

            /// <summary>
            ///     Gets the hash code for the key.
            /// </summary>
            /// <returns>
            ///     The hash code for the key.
            /// </returns>
            public override int GetHashCode()
            {
                var hash = new HashCode();
                hash.Add(_query, ExpressionEqualityComparer.Instance);
                hash.Add(_model);
                hash.Add(_queryTrackingBehavior);
                hash.Add(_async);
                return hash.ToHashCode();
            }
        }
    }
}
