// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CompiledQueryCache : ICompiledQueryCache
    {
        private const int EvictionThreshold = 500;
        private const int EvictionCount = 25;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string CompiledQueryParameterPrefix = "__";

        private readonly ConcurrentDictionary<object, CacheEntry> _cache
            = new ConcurrentDictionary<object, CacheEntry>();

        private long _recencyCounter;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, TResult> GetOrAddQuery<TResult>(
            object cacheKey, Func<Func<QueryContext, TResult>> compiler)
            => GetOrAddQueryCore(cacheKey, compiler);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> GetOrAddAsyncQuery<TResult>(
            object cacheKey, Func<Func<QueryContext, IAsyncEnumerable<TResult>>> compiler)
            => GetOrAddQueryCore(cacheKey, compiler);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Clear() => _cache.Clear();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual int Count => _cache.Count;

        private Func<QueryContext, TFunc> GetOrAddQueryCore<TFunc>(
            object cacheKey, Func<Func<QueryContext, TFunc>> compiler)
        {
            Interlocked.Increment(ref _recencyCounter);

            var entry = _cache.GetOrAdd(
                cacheKey,
                k => new CacheEntry(compiler()));

            entry._recency = _recencyCounter;

            if (_cache.Count > EvictionThreshold)
            {
                var entries = _cache.ToArray();

                foreach (var toRemove in entries.OrderBy(e => e.Value._recency).Take(EvictionCount))
                {
                    _cache.TryRemove(toRemove.Key, out _);
                }
            }

            return (Func<QueryContext, TFunc>)entry._compiledQuery;
        }

        private class CacheEntry
        {
            public CacheEntry(object compiledQuery)
            {
                _compiledQuery = compiledQuery;
            }

            public long _recency;
            public readonly object _compiledQuery;
        }
    }
}
