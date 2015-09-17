// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqliteMetadataExtensions
    {
        public static IRelationalEntityTypeAnnotations Sqlite([NotNull] this IEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), SqliteAnnotationNames.Prefix);

        public static RelationalEntityTypeAnnotations Sqlite([NotNull] this EntityType entityType)
            => (RelationalEntityTypeAnnotations)Sqlite((IEntityType)entityType);

        public static IRelationalForeignKeyAnnotations Sqlite([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), SqliteAnnotationNames.Prefix);

        public static RelationalForeignKeyAnnotations Sqlite([NotNull] this ForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)Sqlite((IForeignKey)foreignKey);

        public static IRelationalIndexAnnotations Sqlite([NotNull] this IIndex index)
            => new RelationalIndexAnnotations(Check.NotNull(index, nameof(index)), SqliteAnnotationNames.Prefix);

        public static RelationalIndexAnnotations Sqlite([NotNull] this Index index)
            => (RelationalIndexAnnotations)Sqlite((IIndex)index);

        public static IRelationalKeyAnnotations Sqlite([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)), SqliteAnnotationNames.Prefix);

        public static RelationalKeyAnnotations Sqlite([NotNull] this Key key)
            => (RelationalKeyAnnotations)Sqlite((IKey)key);

        public static RelationalModelAnnotations Sqlite([NotNull] this Model model)
            => (RelationalModelAnnotations)Sqlite((IModel)model);

        public static IRelationalModelAnnotations Sqlite([NotNull] this IModel model)
            => new RelationalModelAnnotations(Check.NotNull(model, nameof(model)), SqliteAnnotationNames.Prefix);

        public static IRelationalPropertyAnnotations Sqlite([NotNull] this IProperty property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)), SqliteAnnotationNames.Prefix);

        public static RelationalPropertyAnnotations Sqlite([NotNull] this Property property)
            => (RelationalPropertyAnnotations)Sqlite((IProperty)property);
    }
}
