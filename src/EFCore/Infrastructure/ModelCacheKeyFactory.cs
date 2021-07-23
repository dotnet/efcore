// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Creates keys that uniquely identifies the model for a given context. This is used to store and lookup
    ///         a cached model for a given context. This default implementation uses the context type as they key, thus
    ///         assuming that all contexts of a given type have the same model.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class ModelCacheKeyFactory : IModelCacheKeyFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelCacheKeyFactory" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ModelCacheKeyFactory(ModelCacheKeyFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        /// <summary>
        ///     Gets the model cache key for a given context.
        /// </summary>
        /// <param name="context">
        ///     The context to get the model cache key for.
        /// </param>
        /// <returns> The created key. </returns>
        public virtual object Create(DbContext context)
            => new ModelCacheKey(context);

        /// <summary>
        ///     Gets the model cache key for a given context.
        /// </summary>
        /// <param name="context">
        ///     The context to get the model cache key for.
        /// </param>
        /// <param name="designTime"> Whether the model should contain design-time configuration.</param>
        /// <returns> The created key. </returns>
        public virtual object Create(DbContext context, bool designTime)
            => new ModelCacheKey(context, designTime);
    }
}
