// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Query;

/// <inheritdoc />
public class RelationalCompiledQueryCacheKeyGenerator : CompiledQueryCacheKeyGenerator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalCompiledQueryCacheKeyGenerator" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this service.</param>
    public RelationalCompiledQueryCacheKeyGenerator(
        CompiledQueryCacheKeyGeneratorDependencies dependencies,
        RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies)
        : base(dependencies)
        => RelationalDependencies = relationalDependencies;

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalCompiledQueryCacheKeyGeneratorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public override object GenerateCacheKey(Expression query, bool async)
        => GenerateCacheKeyCore(query, async);

    /// <summary>
    ///     Generates the cache key for the given query.
    /// </summary>
    /// <param name="query">The query to get the cache key for.</param>
    /// <param name="async">A value indicating whether the query will be executed asynchronously.</param>
    /// <returns>The cache key.</returns>
    protected new RelationalCompiledQueryCacheKey
        GenerateCacheKeyCore(Expression query, bool async) // Intentionally non-virtual
    {
        var relationalOptions = RelationalOptionsExtension.Extract(RelationalDependencies.ContextOptions);

#pragma warning disable EF9001 // Window function support is experimental
        return new RelationalCompiledQueryCacheKey(
            base.GenerateCacheKeyCore(query, async),
            relationalOptions.UseRelationalNulls,
            relationalOptions.QuerySplittingBehavior,
            shouldBuffer: ExecutionStrategy.Current?.RetriesOnFailure ?? Dependencies.IsRetryingExecutionStrategy,
            relationalOptions.UseWindowFunctionAggregates);
#pragma warning restore EF9001
    }

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
    protected readonly struct RelationalCompiledQueryCacheKey : IEquatable<RelationalCompiledQueryCacheKey>
    {
        private readonly CompiledQueryCacheKey _compiledQueryCacheKey;
        private readonly bool _useRelationalNulls;
        private readonly QuerySplittingBehavior? _querySplittingBehavior;
        private readonly bool _shouldBuffer;
        private readonly bool _useWindowFunctionAggregates;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalCompiledQueryCacheKey" /> class.
        /// </summary>
        /// <param name="compiledQueryCacheKey">The non-relational cache key.</param>
        /// <param name="useRelationalNulls">True to use relational null logic.</param>
        /// <param name="querySplittingBehavior"><see cref="QuerySplittingBehavior" /> to use when loading related collections.</param>
        /// <param name="shouldBuffer"><see langword="true" /> if the query should be buffered.</param>
        public RelationalCompiledQueryCacheKey(
            CompiledQueryCacheKey compiledQueryCacheKey,
            bool useRelationalNulls,
            QuerySplittingBehavior? querySplittingBehavior,
            bool shouldBuffer)
#pragma warning disable EF9001 // Window function support is experimental
            : this(compiledQueryCacheKey, useRelationalNulls, querySplittingBehavior, shouldBuffer, useWindowFunctionAggregates: false)
#pragma warning restore EF9001
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalCompiledQueryCacheKey" /> class.
        /// </summary>
        /// <param name="compiledQueryCacheKey">The non-relational cache key.</param>
        /// <param name="useRelationalNulls">True to use relational null logic.</param>
        /// <param name="querySplittingBehavior"><see cref="QuerySplittingBehavior" /> to use when loading related collections.</param>
        /// <param name="shouldBuffer"><see langword="true" /> if the query should be buffered.</param>
        /// <param name="useWindowFunctionAggregates">
        ///     <see langword="true" /> if aggregates projected alongside the elements of a grouping are translated as window functions.
        /// </param>
        [Experimental(EFDiagnostics.ExperimentalApi)]
        public RelationalCompiledQueryCacheKey(
            CompiledQueryCacheKey compiledQueryCacheKey,
            bool useRelationalNulls,
            QuerySplittingBehavior? querySplittingBehavior,
            bool shouldBuffer,
            bool useWindowFunctionAggregates)
        {
            _compiledQueryCacheKey = compiledQueryCacheKey;
            _useRelationalNulls = useRelationalNulls;
            _querySplittingBehavior = querySplittingBehavior;
            _shouldBuffer = shouldBuffer;
            _useWindowFunctionAggregates = useWindowFunctionAggregates;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is RelationalCompiledQueryCacheKey key
                && Equals(key);

        /// <inheritdoc />
        public bool Equals(RelationalCompiledQueryCacheKey other)
            => _compiledQueryCacheKey.Equals(other._compiledQueryCacheKey)
                && _useRelationalNulls == other._useRelationalNulls
                && _querySplittingBehavior == other._querySplittingBehavior
                && _shouldBuffer == other._shouldBuffer
                && _useWindowFunctionAggregates == other._useWindowFunctionAggregates;

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(
                _compiledQueryCacheKey, _useRelationalNulls, _querySplittingBehavior, _shouldBuffer, _useWindowFunctionAggregates);
    }
}
