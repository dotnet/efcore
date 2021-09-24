// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API surface for setting discriminator values.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
    /// <typeparam name="TDiscriminator">The type of the discriminator property.</typeparam>
    public class DiscriminatorBuilder<TDiscriminator>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public DiscriminatorBuilder(DiscriminatorBuilder builder)
        {
            Check.NotNull(builder, nameof(builder));

            Builder = builder;
        }

        private DiscriminatorBuilder Builder { get; }

        /// <summary>
        ///     Configures if the discriminator mapping is complete.
        /// </summary>
        /// <param name="complete">The value indicating if this discriminator mapping is complete.</param>
        /// <returns>The same builder so that multiple calls can be chained.</returns>
        public virtual DiscriminatorBuilder<TDiscriminator> IsComplete(bool complete = true)
            => new DiscriminatorBuilder<TDiscriminator>(Builder.IsComplete(complete));

        /// <summary>
        ///     Configures the default discriminator value to use.
        /// </summary>
        /// <param name="value">The discriminator value.</param>
        /// <returns>The same builder so that multiple calls can be chained.</returns>
        public virtual DiscriminatorBuilder<TDiscriminator> HasValue(TDiscriminator value)
            => new DiscriminatorBuilder<TDiscriminator>(Builder.HasValue(value));

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given generic type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type for which a discriminator value is being set.</typeparam>
        /// <param name="value">The discriminator value.</param>
        /// <returns>The same builder so that multiple calls can be chained.</returns>
        public virtual DiscriminatorBuilder<TDiscriminator> HasValue<TEntity>(TDiscriminator value)
            => HasValue(typeof(TEntity), value);

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given type.
        /// </summary>
        /// <param name="entityType">The entity type for which a discriminator value is being set.</param>
        /// <param name="value">The discriminator value.</param>
        /// <returns>The same builder so that multiple calls can be chained.</returns>
        public virtual DiscriminatorBuilder<TDiscriminator> HasValue(Type entityType, TDiscriminator value)
            => new DiscriminatorBuilder<TDiscriminator>(Builder.HasValue(entityType, value));

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given type.
        /// </summary>
        /// <param name="entityTypeName">The name of the entity type for which a discriminator value is being set.</param>
        /// <param name="value">The discriminator value.</param>
        /// <returns>The same builder so that multiple calls can be chained.</returns>
        public virtual DiscriminatorBuilder<TDiscriminator> HasValue(string entityTypeName, TDiscriminator value)
            => new DiscriminatorBuilder<TDiscriminator>(Builder.HasValue(entityTypeName, value));
    }
}
