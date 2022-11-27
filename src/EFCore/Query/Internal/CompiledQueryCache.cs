// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
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
    public virtual bool TryGetQuery<TResult>(object cacheKey, [NotNullWhen(true)] out Func<QueryContext, TResult>? compiledQuery)
        => _memoryCache.TryGetValue(cacheKey, out compiledQuery);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AddQuery<TResult>(object cacheKey, Func<QueryContext, TResult> compiledQuery)
    {
        // TODO: Make sure NeverRemove works the way I think it does.
        // TODO: Also, do these entries still "take up space", meaning that they leave less space for other cache entries (potentially
        // TODO: causing thrashing)? Is Size=0 enough to prevent this? Or do we want a separate dictionary for precompiled queries?
        _memoryCache.Set(cacheKey, compiledQuery, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove, Size = 0 });
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
        // TODO: Reexamine this change, is it necessary?
        // if (_memoryCache.TryGetValue(cacheKey, out Func<QueryContext, TResult>? compiledQuery))
        if (_memoryCache.TryGetValue(cacheKey, out var compiledQuery)
            && compiledQuery is Func<QueryContext, TResult> typedCompiledQuery)
        {
            EntityFrameworkEventSource.Log.CompiledQueryCacheHit();
            return typedCompiledQuery;
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
                if (_memoryCache.TryGetValue(cacheKey, out compiledQuery)
                    && compiledQuery is Func<QueryContext, TResult> typedCompiledQuery2)
                {
                    EntityFrameworkEventSource.Log.CompiledQueryCacheHit();
                }
                else
                {
                    EntityFrameworkEventSource.Log.CompiledQueryCacheMiss();

                    typedCompiledQuery2 = compiler();
                    _memoryCache.Set(cacheKey, compiledQuery, new MemoryCacheEntryOptions { Size = 10 });
                }

                return typedCompiledQuery2!;
            }
        }
        finally
        {
            Locks.TryRemove(cacheKey, out _);
        }
    }
}
