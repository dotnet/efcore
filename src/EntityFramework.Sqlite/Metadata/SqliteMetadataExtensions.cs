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
        public static ISqliteEntityTypeExtensions Sqlite([NotNull] this IEntityType entityType) => new ReadOnlySqliteEntityTypeExtensions(entityType);
        public static SqliteEntityTypeExtensions Sqlite([NotNull] this EntityType entityType) => new SqliteEntityTypeExtensions(entityType);
        public static ISqliteForeignKeyExtensions Sqlite([NotNull] this IForeignKey foreignKey) => new ReadOnlySqliteForeignKeyExtensions(foreignKey);
        public static SqliteForeignKeyExtensions Sqlite([NotNull] this ForeignKey foreignKey) => new SqliteForeignKeyExtensions(foreignKey);
        public static ISqliteIndexExtensions Sqlite([NotNull] this IIndex index) => new ReadOnlySqliteIndexExtensions(index);
        public static SqliteIndexExtensions Sqlite([NotNull] this Index index) => new SqliteIndexExtensions(index);
        public static ISqliteKeyExtensions Sqlite([NotNull] this IKey key) => new ReadOnlySqliteKeyExtensions(key);
        public static SqliteKeyExtensions Sqlite([NotNull] this Key key) => new SqliteKeyExtensions(key);
        public static ISqliteModelExtensions Sqlite([NotNull] this IModel model) => new ReadOnlySqliteModelExtensions(model);
        public static SqliteModelExtensions Sqlite([NotNull] this Model model) => new SqliteModelExtensions(model);
        public static ISqlitePropertyExtensions Sqlite([NotNull] this IProperty property) => new ReadOnlySqlitePropertyExtensions(property);
        public static SqlitePropertyExtensions Sqlite([NotNull] this Property property) => new SqlitePropertyExtensions(property);
    }
}
