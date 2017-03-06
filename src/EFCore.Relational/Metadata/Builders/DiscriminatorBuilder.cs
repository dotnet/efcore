// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class DiscriminatorBuilder
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DiscriminatorBuilder([NotNull] RelationalAnnotationsBuilder annotationsBuilder,
            [NotNull] Func<InternalEntityTypeBuilder, RelationalEntityTypeBuilderAnnotations> getRelationalEntityTypeBuilderAnnotations)
        {
            AnnotationsBuilder = annotationsBuilder;
            GetRelationalEntityTypeBuilderAnnotations = getRelationalEntityTypeBuilderAnnotations;
        }

        private Func<InternalEntityTypeBuilder, RelationalEntityTypeBuilderAnnotations> GetRelationalEntityTypeBuilderAnnotations { get; }
        protected virtual RelationalAnnotationsBuilder AnnotationsBuilder { get; }
        protected virtual InternalEntityTypeBuilder EntityTypeBuilder => (InternalEntityTypeBuilder)AnnotationsBuilder.MetadataBuilder;

        public virtual DiscriminatorBuilder HasValue([CanBeNull] object value)
            => HasValue(EntityTypeBuilder, value);

        public virtual DiscriminatorBuilder HasValue<TEntity>([CanBeNull] object value)
            => HasValue(typeof(TEntity), value);

        public virtual DiscriminatorBuilder HasValue([NotNull] Type entityType, [CanBeNull] object value)
        {
            var entityTypeBuilder = EntityTypeBuilder.ModelBuilder.Entity(entityType, AnnotationsBuilder.ConfigurationSource);
            return HasValue(entityTypeBuilder, value);
        }

        public virtual DiscriminatorBuilder HasValue([NotNull] string entityTypeName, [CanBeNull] object value)
        {
            var entityTypeBuilder = EntityTypeBuilder.ModelBuilder.Entity(entityTypeName, AnnotationsBuilder.ConfigurationSource);
            return HasValue(entityTypeBuilder, value);
        }

        private DiscriminatorBuilder HasValue([NotNull] InternalEntityTypeBuilder entityTypeBuilder, [CanBeNull] object value)
        {
            var baseEntityTypeBuilder = EntityTypeBuilder;
            if (!baseEntityTypeBuilder.Metadata.IsAssignableFrom(entityTypeBuilder.Metadata)
                && (entityTypeBuilder.HasBaseType(baseEntityTypeBuilder.Metadata, AnnotationsBuilder.ConfigurationSource) == null))
            {
                throw new InvalidOperationException(RelationalStrings.DiscriminatorEntityTypeNotDerived(
                    entityTypeBuilder.Metadata.DisplayName(),
                    baseEntityTypeBuilder.Metadata.DisplayName()));
            }

            return GetRelationalEntityTypeBuilderAnnotations(entityTypeBuilder).HasDiscriminatorValue(value) ? this : null;
        }
    }
}
