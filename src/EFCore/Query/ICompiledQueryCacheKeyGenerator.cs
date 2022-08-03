// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     <para>
///         Creates keys that uniquely identifies a query. This is used to store and lookup compiled versions of a query in a cache.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         and <see href="https://aka.ms/efcore-docs-how-query-works">How EF Core queries work</see> for more information and examples.
///     </para>
/// </remarks>
public interface ICompiledQueryCacheKeyGenerator
{
    /// <summary>
    ///     Generates a cache key.
    /// </summary>
    /// <param name="query">The query to generate a cache key for.</param>
    /// <param name="async"><see langword="true" /> if the query will be executed asynchronously.</param>
    /// <returns>An object representing a query cache key.</returns>
    object GenerateCacheKey(Expression query, bool async);
}
