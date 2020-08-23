// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class CompiledQueryCache : ICompiledQueryCache
    {
        private static readonly ConcurrentDictionary<object, object> _locks
            = new ConcurrentDictionary<object, object>();

        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CompiledQueryCache([NotNull] IMemoryCache memoryCache)
            => _memoryCache = memoryCache;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<QueryContext, TResult> GetOrAddQuery<TResult>(
            object cacheKey,
            Func<Func<QueryContext, TResult>> compiler)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (_memoryCache.TryGetValue(cacheKey, out Func<QueryContext, TResult> compiledQuery))
            {
                EntityFrameworkEventSource.Log.CompiledQueryCacheHit();
                return compiledQuery;
            }

            // When multiple threads attempt to start processing the same query (program startup / thundering
            // herd), have only one actually process and block the others.
            // Note that the following synchronization isn't perfect - some race conditions may cause concurrent
            // processing. This is benign (and rare).
            var compilationLock = _locks.GetOrAdd(cacheKey, _ => new object());
            try
            {
                lock (compilationLock)
                {
                    if (_memoryCache.TryGetValue(cacheKey, out compiledQuery))
                    {
                        EntityFrameworkEventSource.Log.CompiledQueryCacheHit();
                    }
                    else
                    {
                        EntityFrameworkEventSource.Log.CompiledQueryCacheMiss();

                        compiledQuery = compiler();
                        _memoryCache.Set(cacheKey, compiledQuery, new MemoryCacheEntryOptions { Size = 10 });
                    }

                    return compiledQuery;
                }
            }
            finally
            {
                _locks.TryRemove(cacheKey, out _);
            }
        }
    }
}
