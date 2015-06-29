// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalMetadataExtensions
    {
        public static RelationalPropertyAnnotations Relational([NotNull] this Property property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)));

        public static IRelationalPropertyAnnotations Relational([NotNull] this IProperty property)
            => new ReadOnlyRelationalPropertyAnnotations(Check.NotNull(property, nameof(property)));

        public static RelationalEntityTypeAnnotations Relational([NotNull] this EntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        public static IRelationalEntityTypeAnnotations Relational([NotNull] this IEntityType entityType)
            => new ReadOnlyRelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        public static RelationalKeyAnnotations Relational([NotNull] this Key key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)));

        public static IRelationalKeyAnnotations Relational([NotNull] this IKey key)
            => new ReadOnlyRelationalKeyAnnotations(Check.NotNull(key, nameof(key)));

        public static RelationalIndexAnnotations Relational([NotNull] this Index index)
            => new RelationalIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static IRelationalIndexAnnotations Relational([NotNull] this IIndex index)
            => new ReadOnlyRelationalIndexAnnotations(Check.NotNull(index, nameof(index)));

        public static RelationalForeignKeyAnnotations Relational([NotNull] this ForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static IRelationalForeignKeyAnnotations Relational([NotNull] this IForeignKey foreignKey)
            => new ReadOnlyRelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static RelationalModelAnnotations Relational([NotNull] this Model model)
            => new RelationalModelAnnotations(Check.NotNull(model, nameof(model)));

        public static IRelationalModelAnnotations Relational([NotNull] this IModel model)
            => new ReadOnlyRelationalModelAnnotations(Check.NotNull(model, nameof(model)));
    }
}
