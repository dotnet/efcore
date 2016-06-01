// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        private static readonly object _compiledQueryLockObject = new object();

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
        {
            Func<QueryContext, TResult> compiledQuery;
            lock (_compiledQueryLockObject)
            {
                if (!_memoryCache.TryGetValue(cacheKey, out compiledQuery))
                {
                    compiledQuery = compiler();
                    _memoryCache.Set(cacheKey, compiledQuery);
                }
            }

            return compiledQuery;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> GetOrAddAsyncQuery<TResult>(
            object cacheKey, Func<Func<QueryContext, IAsyncEnumerable<TResult>>> compiler)
        {
            Func<QueryContext, IAsyncEnumerable<TResult>> compiledQuery;
            lock (_compiledQueryLockObject)
            {
                if (!_memoryCache.TryGetValue(cacheKey, out compiledQuery))
                {
                    compiledQuery = compiler();
                    _memoryCache.Set(cacheKey, compiledQuery);
                }
            }

            return compiledQuery;
        }
    }
}
