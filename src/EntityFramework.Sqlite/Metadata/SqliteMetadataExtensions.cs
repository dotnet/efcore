// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Sqlite.Metadata;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqliteMetadataExtensions
    {
        public static ISqliteEntityTypeAnnotations Sqlite([NotNull] this IEntityType entityType) => new ReadOnlySqliteEntityTypeAnnotations(entityType);
        public static SqliteEntityTypeAnnotations Sqlite([NotNull] this EntityType entityType) => new SqliteEntityTypeAnnotations(entityType);
        public static ISqliteForeignKeyAnnotations Sqlite([NotNull] this IForeignKey foreignKey) => new ReadOnlySqliteForeignKeyAnnotations(foreignKey);
        public static SqliteForeignKeyAnnotations Sqlite([NotNull] this ForeignKey foreignKey) => new SqliteForeignKeyAnnotations(foreignKey);
        public static ISqliteIndexAnnotations Sqlite([NotNull] this IIndex index) => new ReadOnlySqliteIndexAnnotations(index);
        public static SqliteIndexAnnotations Sqlite([NotNull] this Index index) => new SqliteIndexAnnotations(index);
        public static ISqliteKeyAnnotations Sqlite([NotNull] this IKey key) => new ReadOnlySqliteKeyAnnotations(key);
        public static SqliteKeyAnnotations Sqlite([NotNull] this Key key) => new SqliteKeyAnnotations(key);
        public static ISqliteModelAnnotations Sqlite([NotNull] this IModel model) => new ReadOnlySqliteModelAnnotations(model);
        public static SqliteModelAnnotations Sqlite([NotNull] this Model model) => new SqliteModelAnnotations(model);
        public static ISqlitePropertyAnnotations Sqlite([NotNull] this IProperty property) => new ReadOnlySqlitePropertyAnnotations(property);
        public static SqlitePropertyAnnotations Sqlite([NotNull] this Property property) => new SqlitePropertyAnnotations(property);
    }
}
