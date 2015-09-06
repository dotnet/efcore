// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalEntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder ToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            ((IAccessor<InternalEntityTypeBuilder>)entityTypeBuilder).GetService()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name);

        public static EntityTypeBuilder ToTable(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            ((IAccessor<InternalEntityTypeBuilder>)entityTypeBuilder).GetService()
                .Relational(ConfigurationSource.Explicit)
                .ToTable(name, schema);

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name, schema);
        
        public static DiscriminatorBuilder Discriminator([NotNull] this EntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            return ((IAccessor<InternalEntityTypeBuilder>)entityTypeBuilder).GetService()
                .Relational(ConfigurationSource.Explicit).Discriminator();
        }

        public static DiscriminatorBuilder Discriminator(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string name,
            [NotNull] Type discriminatorType)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(discriminatorType, nameof(discriminatorType));

            return ((IAccessor<InternalEntityTypeBuilder>)entityTypeBuilder).GetService()
                .Relational(ConfigurationSource.Explicit).Discriminator(name, discriminatorType);
        }

        public static DiscriminatorBuilder<TDiscriminator> Discriminator<TEntity, TDiscriminator>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [NotNull] Expression<Func<TEntity, TDiscriminator>> propertyExpression)
            where TEntity : class
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(propertyExpression, nameof(propertyExpression));

            return new DiscriminatorBuilder<TDiscriminator>(((IAccessor<InternalEntityTypeBuilder>)entityTypeBuilder).GetService()
                .Relational(ConfigurationSource.Explicit).Discriminator(propertyExpression.GetPropertyAccess()));
        }
    }
}
