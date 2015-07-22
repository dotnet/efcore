// Copyright (c) .NET Foundation. All rights reserved.
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
        public static SqlServerPropertyAnnotations SqlServer([NotNull] this Property property)
            => new SqlServerPropertyAnnotations(Check.NotNull(property, nameof(property)));

        public static ISqlServerPropertyAnnotations SqlServer([NotNull] this IProperty property)
            => new ReadOnlySqlServerPropertyAnnotations(Check.NotNull(property, nameof(property)));

        public static SqlServerEntityTypeAnnotations SqlServer([NotNull] this EntityType entityType)
            => new SqlServerEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        public static ISqlServerEntityTypeAnnotations SqlServer([NotNull] this IEntityType entityType)
            => new ReadOnlySqlServerEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        public static SqlServerKeyAnnotations SqlServer([NotNull] this Key key)
            => new SqlServerKeyAnnotations(Check.NotNull(key, nameof(key)));

        public static ISqlServerKeyAnnotations SqlServer([NotNull] this IKey key)
            => new ReadOnlySqlServerKeyAnnotations(Check.NotNull(key, nameof(key)));

        public static SqlServerIndexAnnotations SqlServer([NotNull] this Index index)
            => new SqlServerIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static ISqlServerIndexAnnotations SqlServer([NotNull] this IIndex index)
            => new ReadOnlySqlServerIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static SqlServerForeignKeyAnnotations SqlServer([NotNull] this ForeignKey foreignKey)
            => new SqlServerForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static ISqlServerForeignKeyAnnotations SqlServer([NotNull] this IForeignKey foreignKey)
            => new ReadOnlySqlServerForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static SqlServerModelAnnotations SqlServer([NotNull] this Model model)
            => new SqlServerModelAnnotations(Check.NotNull(model, nameof(model)));

        public static ISqlServerModelAnnotations SqlServer([NotNull] this IModel model)
            => new ReadOnlySqlServerModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
