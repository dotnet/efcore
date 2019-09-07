// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class CompiledQueryCache : ICompiledQueryCache
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public const string CompiledQueryParameterPrefix = "__";

        private static readonly ConcurrentDictionary<object, object> _querySyncObjects
            = new ConcurrentDictionary<object, object>();

        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CompiledQueryCache([NotNull] IMemoryCache memoryCache)
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
            object cacheKey, Func<Func<QueryContext, TResult>> compiler)
            => GetOrAddQueryCore(cacheKey, compiler);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<QueryContext, TResult> GetOrAddAsyncQuery<TResult>(
            object cacheKey, Func<Func<QueryContext, TResult>> compiler)
            => GetOrAddQueryCore(cacheKey, compiler);

        private Func<QueryContext, TFunc> GetOrAddQueryCore<TFunc>(
            object cacheKey, Func<Func<QueryContext, TFunc>> compiler)
        {
            retry:
            if (!_memoryCache.TryGetValue(cacheKey, out Func<QueryContext, TFunc> compiledQuery))
            {
                if (!_querySyncObjects.TryAdd(cacheKey, value: null))
                {
                    goto retry;
                }

                try
                {
                    compiledQuery = compiler();

                    _memoryCache.Set(cacheKey, compiledQuery, new MemoryCacheEntryOptions { Size = 10 });
                }
                finally
                {
                    _querySyncObjects.TryRemove(cacheKey, out _);
                }
            }

            return compiledQuery;
        }
    }
}
