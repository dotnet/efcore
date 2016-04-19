// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class SqliteMetadataExtensions
    {
        public static IRelationalEntityTypeAnnotations Sqlite([NotNull] this IEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), SqliteFullAnnotationNames.Instance);

        public static RelationalEntityTypeAnnotations Sqlite([NotNull] this IMutableEntityType entityType)
            => (RelationalEntityTypeAnnotations)Sqlite((IEntityType)entityType);

        public static IRelationalForeignKeyAnnotations Sqlite([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), SqliteFullAnnotationNames.Instance);

        public static RelationalForeignKeyAnnotations Sqlite([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)Sqlite((IForeignKey)foreignKey);

        public static IRelationalIndexAnnotations Sqlite([NotNull] this IIndex index)
            => new RelationalIndexAnnotations(Check.NotNull(index, nameof(index)), SqliteFullAnnotationNames.Instance);

        public static RelationalIndexAnnotations Sqlite([NotNull] this IMutableIndex index)
            => (RelationalIndexAnnotations)Sqlite((IIndex)index);

        public static IRelationalKeyAnnotations Sqlite([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)), SqliteFullAnnotationNames.Instance);

        public static RelationalKeyAnnotations Sqlite([NotNull] this IMutableKey key)
            => (RelationalKeyAnnotations)Sqlite((IKey)key);

        public static IRelationalModelAnnotations Sqlite([NotNull] this IModel model)
            => new RelationalModelAnnotations(Check.NotNull(model, nameof(model)), SqliteFullAnnotationNames.Instance);

        public static RelationalModelAnnotations Sqlite([NotNull] this IMutableModel model)
            => (RelationalModelAnnotations)Sqlite((IModel)model);

        public static IRelationalPropertyAnnotations Sqlite([NotNull] this IProperty property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)), SqliteFullAnnotationNames.Instance);

        public static RelationalPropertyAnnotations Sqlite([NotNull] this IMutableProperty property)
            => (RelationalPropertyAnnotations)Sqlite((IProperty)property);
    }
}
