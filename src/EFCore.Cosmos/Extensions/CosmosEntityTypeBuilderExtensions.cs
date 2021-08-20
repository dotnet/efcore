// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
            this EntityTypeBuilder entityTypeBuilder,
            string? name)
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
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name)
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
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToContainer(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
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
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetContainer(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
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
            this OwnedNavigationBuilder entityTypeBuilder,
            string? name)
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
        public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToJsonProperty<TOwnerEntity, TDependentEntity>(
            this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> entityTypeBuilder,
            string? name)
            where TOwnerEntity : class
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
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? ToJsonProperty(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
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
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetJsonProperty(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
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
            this EntityTypeBuilder entityTypeBuilder,
            string? name)
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
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? name)
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
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TEntity : class
        {
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            entityTypeBuilder.Metadata.SetPartitionKeyPropertyName(propertyExpression.GetMemberAccess().GetSimpleMemberName());

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
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? HasPartitionKey(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
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
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetPartitionKey(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            string? name,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            return entityTypeBuilder.CanSetAnnotation(CosmosAnnotationNames.PartitionKeyName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures this entity to use CosmosDb etag concurrency checks.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder UseETagConcurrency(this EntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Property<string>("_etag")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures this entity to use CosmosDb etag concurrency checks.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> UseETagConcurrency<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder)
            where TEntity : class
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            UseETagConcurrency((EntityTypeBuilder)entityTypeBuilder);
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the time to live for analytical store in seconds at container scope.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="seconds"> The time to live. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder HasAnalyticalStoreTimeToLive(
            this EntityTypeBuilder entityTypeBuilder,
            int? seconds)
        {
            entityTypeBuilder.Metadata.SetAnalyticalStoreTimeToLive(seconds);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the time to live for analytical store in seconds at container scope.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="seconds"> The time to live. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> HasAnalyticalStoreTimeToLive<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            int? seconds)
            where TEntity : class
        {
            entityTypeBuilder.Metadata.SetAnalyticalStoreTimeToLive(seconds);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the time to live for analytical store in seconds at container scope.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="seconds"> The time to live. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? HasAnalyticalStoreTimeToLive(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            int? seconds,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetAnalyticalStoreTimeToLive(seconds, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetAnalyticalStoreTimeToLive(seconds, fromDataAnnotation);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the time to live for analytical store can be set
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="seconds"> The time to live. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetAnalyticalStoreTimeToLive(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            int? seconds,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(CosmosAnnotationNames.AnalyticalStoreTimeToLive, seconds, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the default time to live in seconds at container scope.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="seconds"> The time to live. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder HasDefaultTimeToLive(
            this EntityTypeBuilder entityTypeBuilder,
            int? seconds)
        {
            entityTypeBuilder.Metadata.SetDefaultTimeToLive(seconds);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the default time to live in seconds at container scope.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="seconds"> The time to live. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static EntityTypeBuilder<TEntity> HasDefaultTimeToLive<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            int? seconds)
            where TEntity : class
        {
            entityTypeBuilder.Metadata.SetDefaultTimeToLive(seconds);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the default time to live in seconds at container scope.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="seconds"> The time to live. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        public static IConventionEntityTypeBuilder? HasDefaultTimeToLive(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            int? seconds,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetDefaultTimeToLive(seconds, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetDefaultTimeToLive(seconds, fromDataAnnotation);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the default time to live can be set
        ///     from the current configuration source
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="seconds"> The time to live. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the configuration can be applied. </returns>
        public static bool CanSetDefaultTimeToLive(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            int? seconds,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return entityTypeBuilder.CanSetAnnotation(CosmosAnnotationNames.DefaultTimeToLive, seconds, fromDataAnnotation);
        }

        /// <summary>
        ///     Configures the manual provisioned throughput offering.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="throughput"> The throughput to set. </param>
        public static EntityTypeBuilder HasManualThroughput(this EntityTypeBuilder entityTypeBuilder, int? throughput)
        {
            entityTypeBuilder.Metadata.SetThroughput(throughput, autoscale: false);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the manual provisioned throughput offering.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="throughput"> The throughput to set. </param>
        public static EntityTypeBuilder<TEntity> HasManualThroughput<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, int? throughput)
            where TEntity : class
        {
            entityTypeBuilder.Metadata.SetThroughput(throughput, autoscale: false);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the autoscale provisioned throughput offering.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="throughput"> The throughput to set. </param>
        public static EntityTypeBuilder HasAutoscaleThroughput(this EntityTypeBuilder entityTypeBuilder, int? throughput)
        {
            entityTypeBuilder.Metadata.SetThroughput(throughput, autoscale: true);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the autoscale provisioned throughput offering.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="throughput"> The throughput to set. </param>
        public static EntityTypeBuilder<TEntity> HasAutoscaleThroughput<TEntity>(this EntityTypeBuilder<TEntity> entityTypeBuilder, int? throughput)
            where TEntity : class
        {
            entityTypeBuilder.Metadata.SetThroughput(throughput, autoscale: true);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Configures the provisioned throughput.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="throughput"> The throughput to set. </param>
        /// <param name="autoscale"> Whether autoscale is enabled. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static IConventionEntityTypeBuilder? HasThroughput(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            int? throughput,
            bool autoscale,
            bool fromDataAnnotation = false)
        {
            if (!entityTypeBuilder.CanSetThroughput(throughput, autoscale, fromDataAnnotation))
            {
                return null;
            }

            entityTypeBuilder.Metadata.SetThroughput(throughput, autoscale, fromDataAnnotation);

            return entityTypeBuilder;
        }

        /// <summary>
        ///     Returns a value indicating whether the given throughput can be set.
        /// </summary>
        /// <param name="entityTypeBuilder"> The builder for the entity type being configured. </param>
        /// <param name="throughput"> The throughput to set. </param>
        /// <param name="autoscale"> Whether autoscale is enabled. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <see langword="true" /> if the given container name can be set as default. </returns>
        public static bool CanSetThroughput(
            this IConventionEntityTypeBuilder entityTypeBuilder,
            int? throughput,
            bool autoscale,
            bool fromDataAnnotation = false)
        {
            var existingAnnotation = entityTypeBuilder.Metadata.FindAnnotation(CosmosAnnotationNames.Throughput);
            if (existingAnnotation == null)
            {
                return true;
            }

            var configurationSource = fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention;
            if (configurationSource.Overrides(existingAnnotation.GetConfigurationSource()))
            {
                return true;
            }

            var existingThroughput = (ThroughputProperties?)existingAnnotation.Value;
            return autoscale
                ? existingThroughput?.Throughput == throughput
                : existingThroughput?.AutoscaleMaxThroughput == throughput;
        }
    }
}
