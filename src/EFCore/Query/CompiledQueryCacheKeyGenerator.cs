// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
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
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///         and <see href="https://aka.ms/efcore-how-queries-work">How EF Core queries work</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public class CompiledQueryCacheKeyGenerator : ICompiledQueryCacheKeyGenerator
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CompiledQueryCacheKeyGenerator" /> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        public CompiledQueryCacheKeyGenerator(CompiledQueryCacheKeyGeneratorDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Dependencies for this service.
        /// </summary>
        protected virtual CompiledQueryCacheKeyGeneratorDependencies Dependencies { get; }

        /// <summary>
        ///     Generates the cache key for the given query.
        /// </summary>
        /// <param name="query">The query to get the cache key for.</param>
        /// <param name="async">A value indicating whether the query will be executed asynchronously.</param>
        /// <returns>The cache key.</returns>
        public virtual object GenerateCacheKey(Expression query, bool async)
            => GenerateCacheKeyCore(query, async);

        /// <summary>
        ///     Generates the cache key for the given query.
        /// </summary>
        /// <param name="query">The query to get the cache key for.</param>
        /// <param name="async">A value indicating whether the query will be executed asynchronously.</param>
        /// <returns>The cache key.</returns>
        protected CompiledQueryCacheKey GenerateCacheKeyCore(Expression query, bool async) // Intentionally non-virtual
            => new(
                query,
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
        protected readonly struct CompiledQueryCacheKey : IEquatable<CompiledQueryCacheKey>
        {
            private readonly Expression _query;
            private readonly IModel _model;
            private readonly QueryTrackingBehavior _queryTrackingBehavior;
            private readonly bool _async;

            /// <summary>
            ///     Initializes a new instance of the <see cref="CompiledQueryCacheKey" /> class.
            /// </summary>
            /// <param name="query">The query to generate the key for.</param>
            /// <param name="model">The model that queries is written against.</param>
            /// <param name="queryTrackingBehavior">The tracking behavior for results of the query.</param>
            /// <param name="async">A value indicating whether the query will be executed asynchronously.</param>
            public CompiledQueryCacheKey(
                Expression query,
                IModel model,
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
            ///     <see langword="true" /> if the object is a <see cref="CompiledQueryCacheKey" /> and is for the same query, otherwise
            ///     <see langword="false" />.
            /// </returns>
            public override bool Equals(object? obj)
                => obj is CompiledQueryCacheKey other && Equals(other);

            /// <summary>
            ///     Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">
            ///     An object to compare with this object.
            /// </param>
            /// <returns>
            ///     <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.
            /// </returns>
            public bool Equals(CompiledQueryCacheKey other)
                => ReferenceEquals(_model, other._model)
                    && _queryTrackingBehavior == other._queryTrackingBehavior
                    && _async == other._async
                    && ExpressionEqualityComparer.Instance.Equals(_query, other._query);

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
