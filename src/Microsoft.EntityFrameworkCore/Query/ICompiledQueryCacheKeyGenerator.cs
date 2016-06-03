// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A cache key generator for the compiled query cache.
    /// </summary>
    public interface ICompiledQueryCacheKeyGenerator
    {
        /// <summary>
        ///     Generates a cache key.
        /// </summary>
        /// <param name="query"> The query to generate a cache key for. </param>
        /// <param name="async"> True if the query will be executed asynchronously. </param>
        /// <returns> An object representing a query cache key. </returns>
        object GenerateCacheKey([NotNull] Expression query, bool async);
    }
}
