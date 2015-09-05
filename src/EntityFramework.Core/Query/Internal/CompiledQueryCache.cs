// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Framework.Caching.Memory;

namespace Microsoft.Data.Entity.Query.Internal
{
    public class CompiledQueryCache : ICompiledQueryCache
    {
        public const string CompiledQueryParameterPrefix = "__";

        private static readonly object _compiledQueryLockObject = new object();

        private readonly IMemoryCache _memoryCache;

        public CompiledQueryCache([NotNull] IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

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
