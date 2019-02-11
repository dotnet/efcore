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
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string CompiledQueryParameterPrefix = "__";

        private static readonly ConcurrentDictionary<object, object> _querySyncObjects
            = new ConcurrentDictionary<object, object>();

        private readonly IMemoryCache _memoryCache;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public CompiledQueryCache([NotNull] IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

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

                    _memoryCache.Set(cacheKey, compiledQuery);
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
