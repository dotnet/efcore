// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalMetadataExtensions
    {
        public static RelationalPropertyExtensions Relational([NotNull] this Property property)
            => new RelationalPropertyExtensions(Check.NotNull(property, nameof(property)));

        public static IRelationalPropertyExtensions Relational([NotNull] this IProperty property)
            => new ReadOnlyRelationalPropertyExtensions(Check.NotNull(property, nameof(property)));

        public static RelationalEntityTypeExtensions Relational([NotNull] this EntityType entityType)
            => new RelationalEntityTypeExtensions(Check.NotNull(entityType, nameof(entityType)));

        public static IRelationalEntityTypeExtensions Relational([NotNull] this IEntityType entityType)
            => new ReadOnlyRelationalEntityTypeExtensions(Check.NotNull(entityType, nameof(entityType)));

        public static RelationalKeyExtensions Relational([NotNull] this Key key)
            => new RelationalKeyExtensions(Check.NotNull(key, nameof(key)));

        public static IRelationalKeyExtensions Relational([NotNull] this IKey key)
            => new ReadOnlyRelationalKeyExtensions(Check.NotNull(key, nameof(key)));

        public static RelationalIndexExtensions Relational([NotNull] this Index index)
            => new RelationalIndexExtensions(Check.NotNull(index, nameof(index)));

        public static IRelationalIndexExtensions Relational([NotNull] this IIndex index)
            => new ReadOnlyRelationalIndexExtensions(Check.NotNull(index, nameof(index)));

        public static RelationalForeignKeyExtensions Relational([NotNull] this ForeignKey foreignKey)
            => new RelationalForeignKeyExtensions(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static IRelationalForeignKeyExtensions Relational([NotNull] this IForeignKey foreignKey)
            => new ReadOnlyRelationalForeignKeyExtensions(Check.NotNull(foreignKey, nameof(foreignKey)));

        public static RelationalModelExtensions Relational([NotNull] this Model model)
            => new RelationalModelExtensions(Check.NotNull(model, nameof(model)));

        public static IRelationalModelExtensions Relational([NotNull] this IModel model)
            => new ReadOnlyRelationalModelExtensions(Check.NotNull(model, nameof(model)));
    }
}
