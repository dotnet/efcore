// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
    /// </summary>
    public sealed class ModelSourceDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="ModelSource" />.
        ///     </para>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
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
        public ModelSourceDependencies(
            [NotNull] ICoreConventionSetBuilder coreConventionSetBuilder,
            [NotNull] IModelCustomizer modelCustomizer,
            [NotNull] IModelCacheKeyFactory modelCacheKeyFactory)
        {
            Check.NotNull(coreConventionSetBuilder, nameof(coreConventionSetBuilder));
            Check.NotNull(modelCustomizer, nameof(modelCustomizer));
            Check.NotNull(modelCacheKeyFactory, nameof(modelCacheKeyFactory));

            CoreConventionSetBuilder = coreConventionSetBuilder;
            ModelCustomizer = modelCustomizer;
            ModelCacheKeyFactory = modelCacheKeyFactory;
        }

        /// <summary>
        ///     Gets the <see cref="ICoreConventionSetBuilder" /> that will build the conventions to be used
        ///     to build the model.
        /// </summary>
        public ICoreConventionSetBuilder CoreConventionSetBuilder { get; }

        /// <summary>
        ///     Gets the <see cref="IModelCustomizer" /> that will perform additional configuration of the model
        ///     in addition to what is discovered by convention.
        /// </summary>
        public IModelCustomizer ModelCustomizer { get; }

        /// <summary>
        ///     Gets the <see cref="IModelCacheKeyFactory" /> that will create keys used to store and lookup models
        ///     the model cache.
        /// </summary>
        public IModelCacheKeyFactory ModelCacheKeyFactory { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="coreConventionSetBuilder"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModelSourceDependencies With([NotNull] ICoreConventionSetBuilder coreConventionSetBuilder)
            => new ModelSourceDependencies(coreConventionSetBuilder, ModelCustomizer, ModelCacheKeyFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="modelCustomizer"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModelSourceDependencies With([NotNull] IModelCustomizer modelCustomizer)
            => new ModelSourceDependencies(CoreConventionSetBuilder, modelCustomizer, ModelCacheKeyFactory);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="modelCacheKeyFactory"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModelSourceDependencies With([NotNull] IModelCacheKeyFactory modelCacheKeyFactory)
            => new ModelSourceDependencies(CoreConventionSetBuilder, ModelCustomizer, modelCacheKeyFactory);
    }
}
