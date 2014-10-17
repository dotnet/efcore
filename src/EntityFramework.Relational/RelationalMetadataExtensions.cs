// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Relational.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RelationalMetadataExtensions
    {
        public static RelationalPropertyExtensions Relational([NotNull] this Property property)
        {
            Check.NotNull(property, "property");

            return new RelationalPropertyExtensions(property);
        }

        public static IRelationalPropertyExtensions Relational([NotNull] this IProperty property)
        {
            Check.NotNull(property, "property");

            return new ReadOnlyRelationalPropertyExtensions(property);
        }

        public static RelationalEntityTypeExtensions Relational([NotNull] this EntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new RelationalEntityTypeExtensions(entityType);
        }

        public static IRelationalEntityTypeExtensions Relational([NotNull] this IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return new ReadOnlyRelationalEntityTypeExtensions(entityType);
        }

        public static RelationalKeyExtensions Relational([NotNull] this Key key)
        {
            Check.NotNull(key, "key");

            return new RelationalKeyExtensions(key);
        }

        public static IRelationalKeyExtensions Relational([NotNull] this IKey key)
        {
            Check.NotNull(key, "key");

            return new ReadOnlyRelationalKeyExtensions(key);
        }

        public static RelationalIndexExtensions Relational([NotNull] this Index index)
        {
            Check.NotNull(index, "index");

            return new RelationalIndexExtensions(index);
        }

        public static IRelationalIndexExtensions Relational([NotNull] this IIndex index)
        {
            Check.NotNull(index, "index");

            return new ReadOnlyRelationalIndexExtensions(index);
        }

        public static RelationalForeignKeyExtensions Relational([NotNull] this ForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return new RelationalForeignKeyExtensions(foreignKey);
        }

        public static IRelationalForeignKeyExtensions Relational([NotNull] this IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return new ReadOnlyRelationalForeignKeyExtensions(foreignKey);
        }

        public static RelationalModelExtensions Relational([NotNull] this Model model)
        {
            Check.NotNull(model, "model");

            return new RelationalModelExtensions(model);
        }

        public static IRelationalModelExtensions Relational([NotNull] this IModel model)
        {
            Check.NotNull(model, "model");

            return new ReadOnlyRelationalModelExtensions(model);
        }
    }
}
