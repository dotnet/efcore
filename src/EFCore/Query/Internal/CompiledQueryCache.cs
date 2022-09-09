// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class CompiledQueryCache : ICompiledQueryCache
{
    private static readonly ConcurrentDictionary<object, object> Locks = new();

    private readonly IMemoryCache _memoryCache;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompiledQueryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

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
        if (_memoryCache.TryGetValue(cacheKey, out Func<QueryContext, TResult>? compiledQuery))
        {
            EntityFrameworkEventSource.Log.CompiledQueryCacheHit();
            return compiledQuery!;
        }

        // When multiple threads attempt to start processing the same query (program startup / thundering
        // herd), have only one actually process and block the others.
        // Note that the following synchronization isn't perfect - some race conditions may cause concurrent
        // processing. This is benign (and rare).
        var compilationLock = Locks.GetOrAdd(cacheKey, _ => new object());
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

                return compiledQuery!;
            }
        }
        finally
        {
            Locks.TryRemove(cacheKey, out _);
        }
    }
}
