// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
        public static EntityTypeBuilder ToContainer(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            entityTypeBuilder.Metadata.SetContainer(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the container that the entity type maps to when targeting Azure Cosmos.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type being configured. </typeparam>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the container. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> ToContainer<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToContainer((EntityTypeBuilder)entityTypeBuilder, name);

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
        public static IConventionEntityTypeBuilder ToContainer(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetContainer(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetContainer(name, fromDataAnnotation);

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
        public static bool CanSetContainer(
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
        public static OwnedNavigationBuilder ToJsonProperty(
            [NotNull] this OwnedNavigationBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            entityTypeBuilder.OwnedEntityType.SetContainingPropertyName(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the property name that the entity is mapped to when stored as an embedded document.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the parent property. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static OwnedNavigationBuilder<TEntity, TDependentEntity> ToJsonProperty<TEntity, TDependentEntity>(
            [NotNull] this OwnedNavigationBuilder<TEntity, TDependentEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            where TDependentEntity : class
        {
            entityTypeBuilder.OwnedEntityType.SetContainingPropertyName(name);

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
        public static IConventionEntityTypeBuilder ToJsonProperty(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetJsonProperty(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetContainingPropertyName(name, fromDataAnnotation);

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
        public static bool CanSetJsonProperty(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(CosmosAnnotationNames.PropertyName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the property that is used to store the partition key.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the partition key property. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder HasPartitionKey(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            entityTypeBuilder.Metadata.SetPartitionKeyPropertyName(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the property that is used to store the partition key.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the partition key property. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> HasPartitionKey<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
        {
            entityTypeBuilder.Metadata.SetPartitionKeyPropertyName(name);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the property that is used to store the partition key.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="propertyExpression"> The  partition key property. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> HasPartitionKey<TEntity, TProperty>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : class
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            entityTypeBuilder.Metadata.SetPartitionKeyPropertyName(propertyExpression.GetPropertyAccess().GetSimpleMemberName());

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the property that is used to store the partition key.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the partition key property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder HasPartitionKey(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetPartitionKey(name, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetPartitionKeyPropertyName(name, fromDataAnnotation);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the property that is used to store the partition key can be set
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="name"> The name of the partition key property. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the configuration can be applied. </returns>
        public static bool CanSetPartitionKey(
            [NotNull] this IConventionEntityTypeBuilder entityTypeBuilder, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(CosmosAnnotationNames.PartitionKeyName, name, fromDataAnnotation);
        }
    }
}
