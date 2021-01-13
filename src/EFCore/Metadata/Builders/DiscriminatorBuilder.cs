// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API surface for setting discriminator values.
    /// </summary>
    public class DiscriminatorBuilder : IConventionDiscriminatorBuilder
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public DiscriminatorBuilder([NotNull] IMutableEntityType entityType)
        {
            EntityTypeBuilder = ((EntityType)entityType).Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalEntityTypeBuilder EntityTypeBuilder { get; }

        /// <summary>
        ///     Configures if the discriminator mapping is complete.
        /// </summary>
        /// <param name="complete"> The value indicating if this discriminator mapping is complete. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder IsComplete(bool complete = true)
            => IsComplete(complete, ConfigurationSource.Explicit);

        private DiscriminatorBuilder IsComplete(bool complete, ConfigurationSource configurationSource)
        {
            if (configurationSource == ConfigurationSource.Explicit)
            {
                EntityTypeBuilder.Metadata.SetDiscriminatorMappingComplete(complete);
            }
            else
            {
                if (!EntityTypeBuilder.CanSetAnnotation(
                    CoreAnnotationNames.DiscriminatorMappingComplete, complete, configurationSource))
                {
                    return null;
                }

                EntityTypeBuilder.Metadata.SetDiscriminatorMappingComplete(
                    complete, configurationSource == ConfigurationSource.DataAnnotation);
            }

            return this;
        }

        /// <summary>
        ///     Configures the default discriminator value to use.
        /// </summary>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder HasValue([CanBeNull] object value)
            => HasValue(EntityTypeBuilder, value, ConfigurationSource.Explicit);

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given generic type.
        /// </summary>
        /// <typeparam name="TEntity"> The entity type for which a discriminator value is being set. </typeparam>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder HasValue<TEntity>([CanBeNull] object value)
            => HasValue(typeof(TEntity), value);

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given type.
        /// </summary>
        /// <param name="entityType"> The entity type for which a discriminator value is being set. </param>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder HasValue([NotNull] Type entityType, [CanBeNull] object value)
        {
            var entityTypeBuilder = EntityTypeBuilder.ModelBuilder.Entity(
                entityType, ConfigurationSource.Explicit, shouldBeOwned: null);

            return HasValue(entityTypeBuilder, value, ConfigurationSource.Explicit);
        }

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given type.
        /// </summary>
        /// <param name="entityTypeName"> The name of the entity type for which a discriminator value is being set. </param>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder HasValue([NotNull] string entityTypeName, [CanBeNull] object value)
        {
            var entityTypeBuilder = EntityTypeBuilder.ModelBuilder.Entity(
                entityTypeName, ConfigurationSource.Explicit, shouldBeOwned: null);

            return HasValue(entityTypeBuilder, value, ConfigurationSource.Explicit);
        }

        private DiscriminatorBuilder HasValue(
            InternalEntityTypeBuilder entityTypeBuilder,
            object value,
            ConfigurationSource configurationSource)
        {
            if (entityTypeBuilder == null)
            {
                return null;
            }

            var baseEntityTypeBuilder = EntityTypeBuilder;
            if (!baseEntityTypeBuilder.Metadata.IsAssignableFrom(entityTypeBuilder.Metadata)
                && ((baseEntityTypeBuilder.Metadata.ClrType != null
                        && entityTypeBuilder.Metadata.ClrType != null
                        && !baseEntityTypeBuilder.Metadata.ClrType.IsAssignableFrom(entityTypeBuilder.Metadata.ClrType))
                    || entityTypeBuilder.HasBaseType(baseEntityTypeBuilder.Metadata, configurationSource) == null))
            {
                throw new InvalidOperationException(
                    CoreStrings.DiscriminatorEntityTypeNotDerived(
                        entityTypeBuilder.Metadata.DisplayName(),
                        baseEntityTypeBuilder.Metadata.DisplayName()));
            }

            if (configurationSource == ConfigurationSource.Explicit)
            {
                entityTypeBuilder.Metadata.SetDiscriminatorValue(value);
            }
            else
            {
                if (!entityTypeBuilder.CanSetAnnotation(CoreAnnotationNames.DiscriminatorValue, value, configurationSource))
                {
                    return null;
                }

                entityTypeBuilder.Metadata.SetDiscriminatorValue(value, configurationSource == ConfigurationSource.DataAnnotation);
            }

            return this;
        }

        /// <inheritdoc />
        IConventionDiscriminatorBuilder IConventionDiscriminatorBuilder.IsComplete(bool complete, bool fromDataAnnotation)
            => IsComplete(complete, fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        bool IConventionDiscriminatorBuilder.CanSetIsComplete(bool complete, bool fromDataAnnotation)
            => ((IConventionEntityTypeBuilder)EntityTypeBuilder).CanSetAnnotation(
                CoreAnnotationNames.DiscriminatorMappingComplete, fromDataAnnotation);

        /// <inheritdoc />
        IConventionDiscriminatorBuilder IConventionDiscriminatorBuilder.HasValue(object value, bool fromDataAnnotation)
            => HasValue(
                EntityTypeBuilder, value,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        IConventionDiscriminatorBuilder IConventionDiscriminatorBuilder.HasValue(
            IConventionEntityType entityType,
            object value,
            bool fromDataAnnotation)
            => HasValue(
                (InternalEntityTypeBuilder)entityType.Builder, value,
                fromDataAnnotation ? ConfigurationSource.DataAnnotation : ConfigurationSource.Convention);

        /// <inheritdoc />
        bool IConventionDiscriminatorBuilder.CanSetValue(object value, bool fromDataAnnotation)
            => ((IConventionDiscriminatorBuilder)this).CanSetValue(EntityTypeBuilder.Metadata, value, fromDataAnnotation);

        /// <inheritdoc />
        bool IConventionDiscriminatorBuilder.CanSetValue(IConventionEntityType entityType, object value, bool fromDataAnnotation)
        {
            var baseEntityTypeBuilder = EntityTypeBuilder;
            if (!baseEntityTypeBuilder.Metadata.IsAssignableFrom(entityType)
                && !entityType.Builder.CanSetBaseType(baseEntityTypeBuilder.Metadata, fromDataAnnotation))
            {
                return false;
            }

            return entityType.Builder.CanSetAnnotation(CoreAnnotationNames.DiscriminatorValue, value, fromDataAnnotation);
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
