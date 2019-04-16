// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    /// <summary>
    ///     Cosmos specific extension methods for <see cref="ReferenceOwnershipBuilder" />.
    /// </summary>
    public static class CosmosReferenceOwnershipBuilderExtensions
    {
        /// <summary>
        ///     Configures the container that the entity maps to when targeting Azure Cosmos.
        /// </summary>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder ToContainer(
            [NotNull] this ReferenceOwnershipBuilder referenceOwnershipBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(referenceOwnershipBuilder, nameof(referenceOwnershipBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            referenceOwnershipBuilder.GetInfrastructure<InternalEntityTypeBuilder>()
                .Cosmos(ConfigurationSource.Explicit)
                .ToContainer(name);

            return referenceOwnershipBuilder;
        }

        /// <summary>
        ///     Configures the container that the entity maps to when targeting Azure Cosmos.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <typeparam name="TRelatedEntity"> The entity type that this relationship targets. </typeparam>
        /// <param name="referenceOwnershipBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ReferenceOwnershipBuilder<TEntity, TRelatedEntity> ToContainer<TEntity, TRelatedEntity>(
            [NotNull] this ReferenceOwnershipBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TRelatedEntity : class
            => (ReferenceOwnershipBuilder<TEntity, TRelatedEntity>)ToContainer((ReferenceOwnershipBuilder)referenceOwnershipBuilder, name);
    }
}
