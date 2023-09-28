// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Creates keys that uniquely identifies the model for a given context. This is used to store and lookup
///     a cached model for a given context.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-model-caching">EF Core model caching</see> for more information and examples.
///     </para>
/// </remarks>
public interface IModelCacheKeyFactory
{
    /// <summary>
    ///     Gets the model cache key for a given context.
    /// </summary>
    /// <param name="context">
    ///     The context to get the model cache key for.
    /// </param>
    /// <param name="designTime">Whether the model should contain design-time configuration.</param>
    /// <returns>The created key.</returns>
    object Create(DbContext context, bool designTime);
}
