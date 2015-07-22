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
            => (RelationalPropertyAnnotations)Relational((IProperty)property);

        public static IRelationalPropertyAnnotations Relational([NotNull] this IProperty property)
            => new RelationalPropertyAnnotations(Check.NotNull(property, nameof(property)), null);

        public static RelationalEntityTypeAnnotations Relational([NotNull] this EntityType entityType)
            => (RelationalEntityTypeAnnotations)Relational((IEntityType)entityType);

        public static IRelationalEntityTypeAnnotations Relational([NotNull] this IEntityType entityType)
            => new RelationalEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)), null);

        public static RelationalKeyAnnotations Relational([NotNull] this Key key)
            => (RelationalKeyAnnotations)Relational((IKey)key);

        public static IRelationalKeyAnnotations Relational([NotNull] this IKey key)
            => new RelationalKeyAnnotations(Check.NotNull(key, nameof(key)), null);

        public static RelationalIndexAnnotations Relational([NotNull] this Index index)
            => (RelationalIndexAnnotations)Relational((IIndex)index);

        public static IRelationalIndexAnnotations Relational([NotNull] this IIndex index)
            => new RelationalIndexAnnotations(Check.NotNull(index, nameof(index)), null);

        public static RelationalForeignKeyAnnotations Relational([NotNull] this ForeignKey foreignKey)
            => (RelationalForeignKeyAnnotations)Relational((IForeignKey)foreignKey);

        public static IRelationalForeignKeyAnnotations Relational([NotNull] this IForeignKey foreignKey)
            => new RelationalForeignKeyAnnotations(Check.NotNull(foreignKey, nameof(foreignKey)), null);

        public static RelationalModelAnnotations Relational([NotNull] this Model model)
            => (RelationalModelAnnotations)Relational((IModel)model);

        public static IRelationalModelAnnotations Relational([NotNull] this IModel model)
            => new RelationalModelAnnotations(Check.NotNull(model, nameof(model)), null);
    }
}
