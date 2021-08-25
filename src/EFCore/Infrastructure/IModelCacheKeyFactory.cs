// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Creates keys that uniquely identifies the model for a given context. This is used to store and lookup
    ///         a cached model for a given context.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     For more information, <see href="https://aka.ms/efcore-model-caching">EF Core model caching</see>.
    /// </remarks>
    public interface IModelCacheKeyFactory
    {
        /// <summary>
        ///     Gets the model cache key for a given context.
        /// </summary>
        /// <param name="context">
        ///     The context to get the model cache key for.
        /// </param>
        /// <returns> The created key. </returns>
        [Obsolete("Use the overload with most parameters")]
        object Create(DbContext context)
            => Create(context, true);

        /// <summary>
        ///     Gets the model cache key for a given context.
        /// </summary>
        /// <param name="context">
        ///     The context to get the model cache key for.
        /// </param>
        /// <param name="designTime"> Whether the model should contain design-time configuration.</param>
        /// <returns> The created key. </returns>
        object Create(DbContext context, bool designTime)
#pragma warning disable CS0618 // Type or member is obsolete
            => Create(context);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
