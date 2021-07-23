// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A cache key generator for the compiled query cache.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
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
        /// <param name="async"> <see langword="true" /> if the query will be executed asynchronously. </param>
        /// <returns> An object representing a query cache key. </returns>
        object GenerateCacheKey(Expression query, bool async);
    }
}
