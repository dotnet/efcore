// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
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
        public CompiledQueryCache([NotNull] IDbContextServices contextServices)
        {
            _memoryCache = contextServices.MemoryCache;
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
        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> GetOrAddAsyncQuery<TResult>(
            object cacheKey, Func<Func<QueryContext, IAsyncEnumerable<TResult>>> compiler)
            => GetOrAddQueryCore(cacheKey, compiler);

        private Func<QueryContext, TFunc> GetOrAddQueryCore<TFunc>(
            object cacheKey, Func<Func<QueryContext, TFunc>> compiler)
        {
            Func<QueryContext, TFunc> compiledQuery;

            retry:
            if (!_memoryCache.TryGetValue(cacheKey, out compiledQuery))
            {
                if (!_querySyncObjects.TryAdd(cacheKey, null))
                {
                    goto retry;
                }

                compiledQuery = compiler();

                _memoryCache.Set(cacheKey, compiledQuery);

                object _;
                _querySyncObjects.TryRemove(cacheKey, out _);
            }

            return compiledQuery;
        }
    }
}
