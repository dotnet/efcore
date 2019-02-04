// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     A fluent API builder for setting discriminator values.
    /// </summary>
    public class DiscriminatorBuilder
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DiscriminatorBuilder(
            [NotNull] RelationalAnnotationsBuilder annotationsBuilder,
            [NotNull] Func<InternalEntityTypeBuilder, RelationalEntityTypeBuilderAnnotations> getRelationalEntityTypeBuilderAnnotations)
        {
            AnnotationsBuilder = annotationsBuilder;
            GetRelationalEntityTypeBuilderAnnotations = getRelationalEntityTypeBuilderAnnotations;
        }

        private Func<InternalEntityTypeBuilder, RelationalEntityTypeBuilderAnnotations> GetRelationalEntityTypeBuilderAnnotations { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual RelationalAnnotationsBuilder AnnotationsBuilder { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual InternalEntityTypeBuilder EntityTypeBuilder => (InternalEntityTypeBuilder)AnnotationsBuilder.MetadataBuilder;

        /// <summary>
        ///     Configures the default discriminator value to use.
        /// </summary>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder HasValue([CanBeNull] object value)
            => HasValue(EntityTypeBuilder, value);

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
            var entityTypeBuilder =
                EntityTypeBuilder.Metadata.IsQueryType
                    ? EntityTypeBuilder.ModelBuilder.Query(
                        entityType, AnnotationsBuilder.ConfigurationSource)
                    : EntityTypeBuilder.ModelBuilder.Entity(
                        entityType, AnnotationsBuilder.ConfigurationSource, owned: null);

            return HasValue(entityTypeBuilder, value);
        }

        /// <summary>
        ///     Configures the discriminator value to use for entities of the given type.
        /// </summary>
        /// <param name="entityTypeName"> The name of the entity type for which a discriminator value is being set. </param>
        /// <param name="value"> The discriminator value. </param>
        /// <returns> The same builder so that multiple calls can be chained. </returns>
        public virtual DiscriminatorBuilder HasValue([NotNull] string entityTypeName, [CanBeNull] object value)
        {
            var entityTypeBuilder =
                EntityTypeBuilder.Metadata.IsQueryType
                    ? EntityTypeBuilder.ModelBuilder.Query(
                        entityTypeName, AnnotationsBuilder.ConfigurationSource)
                    : EntityTypeBuilder.ModelBuilder.Entity(
                        entityTypeName, AnnotationsBuilder.ConfigurationSource, owned: null);

            return HasValue(entityTypeBuilder, value);
        }

        private DiscriminatorBuilder HasValue([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [CanBeNull] object value)
        {
            var baseEntityTypeBuilder = EntityTypeBuilder;
            if (!baseEntityTypeBuilder.Metadata.IsAssignableFrom(entityTypeBuilder.Metadata)
                && entityTypeBuilder.HasBaseType(baseEntityTypeBuilder.Metadata, AnnotationsBuilder.ConfigurationSource) == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.DiscriminatorEntityTypeNotDerived(
                        entityTypeBuilder.Metadata.DisplayName(),
                        baseEntityTypeBuilder.Metadata.DisplayName()));
            }

            return GetRelationalEntityTypeBuilderAnnotations(entityTypeBuilder).HasDiscriminatorValue(value) ? this : null;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
