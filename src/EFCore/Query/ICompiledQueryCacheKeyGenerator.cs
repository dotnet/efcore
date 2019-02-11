// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A cache key generator for the compiled query cache.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
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
