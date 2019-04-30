// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API surface for setting discriminator values.
    /// </summary>
    /// <typeparam name="TDiscriminator"> The type of the discriminator property. </typeparam>
    public class DiscriminatorBuilder<TDiscriminator>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public DiscriminatorBuilder([NotNull] DiscriminatorBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

        private DiscriminatorBuilder Builder { get; }

        /// <summary>
        ///     Configures the default discriminator value to use.
        /// </summary>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder<TDiscriminator> HasValue([CanBeNull] TDiscriminator value)
        {
            var builder = Builder.HasValue(value);
            return builder == null ? null : new DiscriminatorBuilder<TDiscriminator>(builder);
        }

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given generic type.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type for which a discriminator value is being set. </typeparam>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder<TDiscriminator> HasValue<TEntity>([CanBeNull] TDiscriminator value)
            => HasValue(typeof(TEntity), value);

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given type.
        /// </summary>
        /// <param name="entityType"> The entity type for which a discriminator value is being set. </param>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder<TDiscriminator> HasValue([NotNull] Type entityType, [CanBeNull] TDiscriminator value)
        {
            var builder = Builder.HasValue(entityType, value);
            return builder == null ? null : new DiscriminatorBuilder<TDiscriminator>(builder);
        }

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given type.
        /// </summary>
        /// <param name="entityTypeName"> The name of the entity type for which a discriminator value is being set. </param>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder<TDiscriminator> HasValue([NotNull] string entityTypeName, [CanBeNull] TDiscriminator value)
        {
            var builder = Builder.HasValue(entityTypeName, value);
            return builder == null ? null : new DiscriminatorBuilder<TDiscriminator>(builder);
        }
    }
}
