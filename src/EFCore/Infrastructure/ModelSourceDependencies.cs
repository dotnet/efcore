// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="ModelSource" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />.
    ///         This means a single instance of each service is used by many <see cref="DbContext" /> instances.
    ///         The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public sealed record ModelSourceDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="ModelSource" />.
        ///     </para>
        ///     <para>
        ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///         any release. You should only use it directly in your code with extreme caution and knowing that
        ///         doing so can result in application failures when updating to a new Entity Framework Core release.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        [EntityFrameworkInternal]
        public ModelSourceDependencies(
            IModelCustomizer modelCustomizer,
            IModelCacheKeyFactory modelCacheKeyFactory,
            IMemoryCache memoryCache)
        {
            Check.NotNull(modelCustomizer, nameof(modelCustomizer));
            Check.NotNull(modelCacheKeyFactory, nameof(modelCacheKeyFactory));
            Check.NotNull(memoryCache, nameof(memoryCache));

            ModelCustomizer = modelCustomizer;
            ModelCacheKeyFactory = modelCacheKeyFactory;
            MemoryCache = memoryCache;
        }

        /// <summary>
        ///     Gets the <see cref="IModelCustomizer" /> that will perform additional configuration of the model
        ///     in addition to what is discovered by convention.
        /// </summary>
        public IModelCustomizer ModelCustomizer { get; init; }

        /// <summary>
        ///     Gets the <see cref="IModelCacheKeyFactory" /> that will create keys used to store and lookup models
        ///     the model cache.
        /// </summary>
        public IModelCacheKeyFactory ModelCacheKeyFactory { get; init; }

        /// <summary>
        ///     Gets the <see cref="IMemoryCache" /> that will be used to cache models.
        /// </summary>
        public IMemoryCache MemoryCache { get; init; }
    }
}
