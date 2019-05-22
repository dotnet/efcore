// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Cosmos-specific extension methods for <see cref="EntityTypeBuilder" />.
    /// </summary>
    public static class CosmosEntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Configures the container that the entity type maps to when targeting Azure Cosmos.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder ForCosmosToContainer(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.SetCosmosContainerName(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the container that the entity type maps to when targeting Azure Cosmos.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ForCosmosToContainer<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ForCosmosToContainer((EntityTypeBuilder)entityTypeBuilder, name);

        /// <summary>
        ///     Configures the container that the entity type maps to when targeting Azure Cosmos.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder ForCosmosToContainer(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.ForCosmosCanSetContainer(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetCosmosContainerName(name, fromDataAnnotation);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the container that the entity type maps to can be set
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool ForCosmosCanSetContainer(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(CosmosAnnotationNames.ContainerName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the property name that the entity is mapped to when stored as an embedded document.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the parent property. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder ForCosmosToProperty(
            [NotNull] this OwnedNavigationBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            entityTypeBuilder.OwnedEntityType.SetCosmosContainingPropertyName(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the property name that the entity is mapped to when stored as an embedded document.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the parent property. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TDependentEntity> ForCosmosToProperty<TEntity, TDependentEntity>(
            [NotNull] this OwnedNavigationBuilder<TEntity, TDependentEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TDependentEntity : class
        {
            entityTypeBuilder.OwnedEntityType.SetCosmosContainingPropertyName(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the property name that the entity is mapped to when stored as an embedded document.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the parent property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder ForCosmosToProperty(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.ForCosmosCanSetProperty(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetCosmosContainingPropertyName(name, fromDataAnnotation);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the parent property name to which the entity type is mapped to can be set
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the parent property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool ForCosmosCanSetProperty(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(CosmosAnnotationNames.PropertyName, name, fromDataAnnotation);
        }
    }
}
