// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerMetadataExtensions
    {
        public static SqlServerPropertyExtensions SqlServer([NotNull] this Property property)
            => new SqlServerPropertyExtensions(Check.NotNull(property, nameof(property)));

        public static ISqlServerPropertyExtensions SqlServer([NotNull] this IProperty property)
            => new ReadOnlySqlServerPropertyExtensions(Check.NotNull(property, nameof(property)));

        public static SqlServerEntityTypeExtensions SqlServer([NotNull] this EntityType entityType)
            => new SqlServerEntityTypeExtensions(Check.NotNull(entityType, nameof(entityType)));

        public static ISqlServerEntityTypeExtensions SqlServer([NotNull] this IEntityType entityType)
            => new ReadOnlySqlServerEntityTypeExtensions(Check.NotNull(entityType, nameof(entityType)));

        public static SqlServerKeyExtensions SqlServer([NotNull] this Key key)
            => new SqlServerKeyExtensions(Check.NotNull(key, nameof(key)));

        public static ISqlServerKeyExtensions SqlServer([NotNull] this IKey key)
            => new ReadOnlySqlServerKeyExtensions(Check.NotNull(key, nameof(key)));

        public static SqlServerIndexExtensions SqlServer([NotNull] this Index index)
            => new SqlServerIndexExtensions(Check.NotNull(index, nameof(index)));

        public static ISqlServerIndexExtensions SqlServer([NotNull] this IIndex index)
            => new ReadOnlySqlServerIndexExtensions(Check.NotNull(index, nameof(index)));

        public static SqlServerForeignKeyExtensions SqlServer([NotNull] this ForeignKey foreignKey)
            => new SqlServerForeignKeyExtensions(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static ISqlServerForeignKeyExtensions SqlServer([NotNull] this IForeignKey foreignKey)
            => new ReadOnlySqlServerForeignKeyExtensions(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static SqlServerModelExtensions SqlServer([NotNull] this Model model)
            => new SqlServerModelExtensions(Check.NotNull(model, nameof(model)));

        public static ISqlServerModelExtensions SqlServer([NotNull] this IModel model)
            => new ReadOnlySqlServerModelExtensions(Check.NotNull(model, nameof(model)));
    }
}
