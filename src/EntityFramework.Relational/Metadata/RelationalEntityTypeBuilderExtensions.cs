// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
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

            entityTypeBuilder.Metadata.Relational().Table = name;

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
            [CanBeNull] string schemaName)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schemaName, nameof(schemaName));

            entityTypeBuilder.Metadata.Relational().Table = name;
            entityTypeBuilder.Metadata.Relational().Schema = schemaName;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> ToTable<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schemaName)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)ToTable((EntityTypeBuilder)entityTypeBuilder, name, schemaName);
    }
}
