// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class SqlServerMetadataExtensions
    {
        public static SqlServerPropertyAnnotations SqlServer([NotNull] this IMutableProperty property)
            => (SqlServerPropertyAnnotations)SqlServer((IProperty)property);

        public static ISqlServerPropertyAnnotations SqlServer([NotNull] this IProperty property)
            => new SqlServerPropertyAnnotations(Check.NotNull(property, nameof(property)));

        public static RelationalEntityTypeAnnotations SqlServer([NotNull] this IMutableEntityType entityType)
            => (RelationalEntityTypeAnnotations)SqlServer((IEntityType)entityType);

        public static IRelationalEntityTypeAnnotations SqlServer([NotNull] this IEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), SqlServerAnnotationNames.Prefix);

        public static SqlServerKeyAnnotations SqlServer([NotNull] this IMutableKey key)
            => (SqlServerKeyAnnotations)SqlServer((IKey)key);

        public static ISqlServerKeyAnnotations SqlServer([NotNull] this IKey key)
            => new SqlServerKeyAnnotations(Check.NotNull(key, nameof(key)));

        public static SqlServerIndexAnnotations SqlServer([NotNull] this IMutableIndex index)
            => (SqlServerIndexAnnotations)SqlServer((IIndex)index);

        public static ISqlServerIndexAnnotations SqlServer([NotNull] this IIndex index)
            => new SqlServerIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static IRelationalForeignKeyAnnotations SqlServer([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), SqlServerAnnotationNames.Prefix);

        public static RelationalForeignKeyAnnotations SqlServer([NotNull] this IMutableForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)SqlServer((IForeignKey)foreignKey);

        public static SqlServerModelAnnotations SqlServer([NotNull] this IMutableModel model)
            => (SqlServerModelAnnotations)SqlServer((IModel)model);

        public static ISqlServerModelAnnotations SqlServer([NotNull] this IModel model)
            => new SqlServerModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
