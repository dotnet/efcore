// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Utilities;
using ForeignKey = Microsoft.Data.Entity.Relational.Model.ForeignKey;
using Index = Microsoft.Data.Entity.Relational.Model.Index;

namespace Microsoft.Data.Entity.Relational
{
    public class DatabaseBuilder
    {
        // TODO: IModel may not be an appropriate cache key if we want to be
        // able to unload IModel instances and create new ones.
        // Issue #765
        private readonly ThreadSafeDictionaryCache<IModel, ModelDatabaseMapping> _mappingCache
            = new ThreadSafeDictionaryCache<IModel, ModelDatabaseMapping>();

        public virtual DatabaseModel GetDatabase([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return GetMapping(model).Database;
        }

        public virtual ModelDatabaseMapping GetMapping([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return _mappingCache.GetOrAdd(model, BuildMapping);
        }

        protected virtual ModelDatabaseMapping BuildMapping([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            // TODO: Consider making this lazy since we don't want to load the whole model just to
            // save changes to a single entity.
            var database = new DatabaseModel();
            var mapping = new ModelDatabaseMapping(model, database);

            foreach (var entityType in model.EntityTypes)
            {
                var table = BuildTable(database, entityType);
                mapping.Map(entityType, table);

                foreach (var property in OrderProperties(entityType))
                {
                    mapping.Map(property, BuildColumn(table, property));

                    BuildSequence(property, database);
                }

                var primaryKey = entityType.GetPrimaryKey();
                if (primaryKey != null)
                {
                    mapping.Map(primaryKey, BuildPrimaryKey(database, primaryKey));
                }

                foreach (var key in entityType.Keys.Except(new[] { primaryKey }))
                {
                    mapping.Map(key, BuildUniqueConstraint(database, key));
                }

                foreach (var index in entityType.Indexes)
                {
                    mapping.Map(index, BuildIndex(database, index));
                }
            }

            foreach (var entityType in model.EntityTypes)
            {
                foreach (var foreignKey in entityType.ForeignKeys)
                {
                    mapping.Map(foreignKey, BuildForeignKey(database, foreignKey));
                }
            }

            return mapping;            
        }

        private static IEnumerable<IProperty> OrderProperties(IEntityType entityType)
        {
            var primaryKey = entityType.GetPrimaryKey();

            var primaryKeyProperties
                = primaryKey != null
                    ? primaryKey.Properties.ToArray()
                    : new IProperty[0];

            var foreignKeyProperties
                = entityType.ForeignKeys
                    .SelectMany(fk => fk.Properties)
                    .Except(primaryKeyProperties)
                    .Distinct()
                    .ToArray();

            var otherProperties
                = entityType.Properties
                    .Except(primaryKeyProperties.Concat(foreignKeyProperties))
                    .OrderBy(p => p.ColumnName())
                    .ToArray();

            return primaryKeyProperties
                .Concat(otherProperties)
                .Concat(foreignKeyProperties);
        }

        private static string PrimaryKeyName([NotNull] IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            return primaryKey.KeyName() ?? string.Format("PK_{0}", GetFullTableName(primaryKey.EntityType));
        }

        private static string UniqueConstraintName([NotNull] IKey key)
        {
            Check.NotNull(key, "key");

            return key.KeyName() ?? string.Format(
                "UC_{0}_{1}",
                GetFullTableName(key.EntityType),
                string.Join("_", key.Properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Select(p => p.ColumnName())));
        }

        private static string ForeignKeyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.KeyName() ?? string.Format(
                "FK_{0}_{1}_{2}",
                GetFullTableName(foreignKey.EntityType),
                GetFullTableName(foreignKey.ReferencedEntityType),
                string.Join("_", foreignKey.Properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Select(p => p.ColumnName())));
        }

        private static string IndexName([NotNull] IIndex index)
        {
            Check.NotNull(index, "index");

            return index.IndexName() ?? string.Format(
                "IX_{0}_{1}",
                GetFullTableName(index.EntityType),
                string.Join("_", index.Properties.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase).Select(p => p.ColumnName())));
        }

        private static string GetFullTableName(IEntityType entityType)
        {
            var schema = entityType.Schema();
            var tableName = entityType.TableName();
            return !string.IsNullOrEmpty(schema) ? schema + "." + tableName : tableName;
        }

        private static Table BuildTable(DatabaseModel database, IEntityType entityType)
        {
            var table = new Table(entityType.SchemaQualifiedName());

            database.AddTable(table);

            return table;
        }

        private static Column BuildColumn(Table table, IProperty property)
        {
            var column =
                new Column(property.ColumnName(), property.PropertyType, property.ColumnType())
                    {
                        IsNullable = property.IsNullable,
                        DefaultValue = property.ColumnDefaultValue(),
                        DefaultSql = property.ColumnDefaultSql(),
                        ValueGenerationStrategy = property.ValueGeneration,
                        IsTimestamp = property.PropertyType == typeof(byte[]) && property.IsConcurrencyToken,
                        MaxLength = property.MaxLength > 0 ? property.MaxLength : (int?)null
                    };

            // TODO: This is a workaround to get the value-generation annotations into the relational model
            // so they can be used for appropriate DDL gen. Hopefully changes can be made to avoid copying all
            // this stuff, or to do it in a cleaner manner.
            // Issue #767
            foreach (var annotation in property.EntityType.Model.Annotations
                .Concat(property.EntityType.Annotations)
                .Concat(property.Annotations))
            {
                column[annotation.Name] = annotation.Value;
            }

            table.AddColumn(column);

            return column;
        }

        private PrimaryKey BuildPrimaryKey(DatabaseModel database, IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            var table = database.GetTable(primaryKey.EntityType.SchemaQualifiedName());
            var columns = primaryKey.Properties.Select(
                p => table.GetColumn(p.ColumnName())).ToArray();
            var isClustered = primaryKey.IsClustered();

            table.PrimaryKey = new PrimaryKey(PrimaryKeyName(primaryKey), columns, isClustered);

            return table.PrimaryKey;
        }

        private static UniqueConstraint BuildUniqueConstraint(DatabaseModel database, IKey key)
        {
            Check.NotNull(key, "key");

            var table = database.GetTable(key.EntityType.SchemaQualifiedName());
            var columns = key.Properties.Select(
                p => table.GetColumn(p.ColumnName())).ToArray();

            var uniqueConstraint = new UniqueConstraint(UniqueConstraintName(key), columns);

            table.AddUniqueConstraint(uniqueConstraint);

            return uniqueConstraint;
        }

        private ForeignKey BuildForeignKey(DatabaseModel database, IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            var table = database.GetTable(foreignKey.EntityType.SchemaQualifiedName());
            var referencedTable = database.GetTable(foreignKey.ReferencedEntityType.SchemaQualifiedName());
            var columns = foreignKey.Properties.Select(
                p => table.GetColumn(p.ColumnName())).ToArray();
            var referenceColumns = foreignKey.ReferencedProperties.Select(
                p => referencedTable.GetColumn(p.ColumnName())).ToArray();
            var cascadeDelete = foreignKey.CascadeDelete();

            var storeForeignKey = new ForeignKey(
                ForeignKeyName(foreignKey), columns, referenceColumns, cascadeDelete);

            table.AddForeignKey(storeForeignKey);

            return storeForeignKey;
        }

        private static Index BuildIndex(DatabaseModel database, IIndex index)
        {
            Check.NotNull(index, "index");

            var table = database.GetTable(index.EntityType.SchemaQualifiedName());
            var columns = index.Properties.Select(
                p => table.GetColumn(p.ColumnName())).ToArray();

            var storeIndex = new Index(
                IndexName(index), columns, index.IsUnique, index.IsClustered());

            table.AddIndex(storeIndex);

            return storeIndex;
        }

        private void BuildSequence([NotNull] IProperty property, [NotNull] DatabaseModel database)
        {
            Check.NotNull(property, "property");
            Check.NotNull(database, "database");

            var sequence = BuildSequence(property);
            if (sequence == null)
            {
                return;
            }

            var existingSequence = database.TryGetSequence(sequence.Name);
            if (existingSequence == null)
            {
                database.AddSequence(sequence);
                return;
            }

            if (!string.Equals(sequence.DataType, existingSequence.DataType, StringComparison.OrdinalIgnoreCase)
                || sequence.StartWith != existingSequence.StartWith
                || sequence.IncrementBy != existingSequence.IncrementBy)
            {
                throw new InvalidOperationException(Strings.FormatSequenceDefinitionMismatch(sequence.Name));
            }
        }

        protected virtual Sequence BuildSequence([NotNull] IProperty property)
        {
            return null;
        }
    }
}
