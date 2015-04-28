// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalEntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder Table(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string tableName)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(tableName, nameof(tableName));

            entityTypeBuilder.Metadata.Relational().Table = tableName;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> Table<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string tableName)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)Table((EntityTypeBuilder)entityTypeBuilder, tableName);

        public static EntityTypeBuilder Table(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [CanBeNull] string tableName,
            [CanBeNull] string schemaName)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NullButNotEmpty(tableName, nameof(tableName));
            Check.NullButNotEmpty(schemaName, nameof(schemaName));

            entityTypeBuilder.Metadata.Relational().Table = tableName;
            entityTypeBuilder.Metadata.Relational().Schema = schemaName;

            return entityTypeBuilder;
        }

        public static EntityTypeBuilder<TEntity> Table<TEntity>(
            [NotNull] this EntityTypeBuilder<TEntity> entityTypeBuilder,
            [CanBeNull] string tableName,
            [CanBeNull] string schemaName)
            where TEntity : class
            => (EntityTypeBuilder<TEntity>)Table((EntityTypeBuilder)entityTypeBuilder, tableName, schemaName);
    }
}
