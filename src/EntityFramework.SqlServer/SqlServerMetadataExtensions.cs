// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.SqlServer.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerMetadataExtensions
    {
        public static SqlServerPropertyExtensions SqlServer([NotNull] this Property property)
        {
            Check.NotNull(property, "property");

            return new SqlServerPropertyExtensions(property);
        }

        public static ISqlServerPropertyExtensions SqlServer([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            return new ReadOnlySqlServerPropertyExtensions(property);
        }

        public static SqlServerEntityTypeExtensions SqlServer([NotNull] this EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new SqlServerEntityTypeExtensions(entityType);
        }

        public static ISqlServerEntityTypeExtensions SqlServer([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new ReadOnlySqlServerEntityTypeExtensions(entityType);
        }

        public static SqlServerKeyExtensions SqlServer([NotNull] this Key key)
        {
            Check.NotNull(key, "key");

            return new SqlServerKeyExtensions(key);
        }

        public static ISqlServerKeyExtensions SqlServer([NotNull] this IKey key)
        {
            Check.NotNull(key, "key");

            return new ReadOnlySqlServerKeyExtensions(key);
        }

        public static SqlServerIndexExtensions SqlServer([NotNull] this Index index)
        {
            Check.NotNull(index, "index");

            return new SqlServerIndexExtensions(index);
        }

        public static ISqlServerIndexExtensions SqlServer([NotNull] this IIndex index)
        {
            Check.NotNull(index, "index");

            return new ReadOnlySqlServerIndexExtensions(index);
        }

        public static SqlServerForeignKeyExtensions SqlServer([NotNull] this ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return new SqlServerForeignKeyExtensions(foreignKey);
        }

        public static ISqlServerForeignKeyExtensions SqlServer([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return new ReadOnlySqlServerForeignKeyExtensions(foreignKey);
        }
    }
}
