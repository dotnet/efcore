// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.Relational.Utilities;
using ForeignKey = Microsoft.Data.Relational.Model.ForeignKey;

namespace Microsoft.Data.Relational
{
    public class DatabaseBuilder
    {
        // TODO: IModel may not be an appropriate cache key if we want to be
        // able to unload IModel instances and create new ones.
        private readonly ThreadSafeDictionaryCache<IModel, ModelDatabaseMapping> _mappingCache
            = new ThreadSafeDictionaryCache<IModel, ModelDatabaseMapping>();

        public virtual Database GetDatabase([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return GetMapping(model).Database;
        }

        public virtual ModelDatabaseMapping GetMapping([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return _mappingCache.GetOrAdd(model, m =>
                {
                    // TODO: Consider making this lazy since we don't want to load the whole model just to
                    // save changes to a single entity.
                    var database = new Database();
                    var mapping = new ModelDatabaseMapping(m, database);

                    foreach (var entityType in m.EntityTypes)
                    {
                        var table = BuildTable(database, entityType);
                        mapping.Map(entityType, table);

                        foreach (var property in entityType.Properties)
                        {
                            mapping.Map(property, BuildColumn(table, property));
                        }

                        var primaryKey = entityType.GetKey();
                        mapping.Map(primaryKey, BuildPrimaryKey(database, primaryKey));
                    }

                    foreach (var entityType in m.EntityTypes)
                    {
                        foreach (var foreignKey in entityType.ForeignKeys)
                        {
                            mapping.Map(foreignKey, BuildForeignKey(database, foreignKey));
                        }
                    }

                    return mapping;
                });
        }

        private string PrimaryKeyName([NotNull] IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            return primaryKey.StorageName ?? string.Format("PK_{0}", primaryKey.EntityType.StorageName);
        }

        private string ForeignKeyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.StorageName ?? string.Format(
                "FK_{0}_{1}_{2}",
                foreignKey.EntityType.StorageName,
                foreignKey.ReferencedEntityType.StorageName,
                string.Join("_", foreignKey.Properties.OrderBy(p => p.Name).Select(p => p.StorageName)));
        }

        // TODO: Use this or remove it
        public static string IndexName([NotNull] Table table, [NotNull] IEnumerable<Column> columns)
        {
            Check.NotNull(table, "table");
            Check.NotNull(columns, "columns");

            return string.Format(
                "IX_{0}_{1}",
                table.Name,
                string.Join("_", columns.OrderBy(c => c.Name).Select(c => c.Name)));
        }

        private static Table BuildTable(Database database, IEntityType entityType)
        {
            var table = new Table(entityType.StorageName);

            database.AddTable(table);

            return table;
        }

        private static Column BuildColumn(Table table, IProperty property)
        {
            var column =
                new Column(property.StorageName, property.PropertyType, property.ColumnType())
                    {
                        IsNullable = property.IsNullable,
                        DefaultValue = property.ColumnDefaultValue(),
                        DefaultSql = property.ColumnDefaultSql(),
                        ValueGenerationStrategy = TranslateValueGenerationStrategy(property.ValueGenerationStrategy),
                        IsTimestamp = property.PropertyType == typeof(byte[]) && property.IsConcurrencyToken
                    };

            table.AddColumn(column);

            return column;
        }

        private static StoreValueGenerationStrategy TranslateValueGenerationStrategy(ValueGenerationStrategy generationStrategy)
        {
            switch (generationStrategy)
            {
                case ValueGenerationStrategy.StoreComputed:
                    return StoreValueGenerationStrategy.Computed;
                case ValueGenerationStrategy.StoreIdentity:
                    return StoreValueGenerationStrategy.Identity;
                default:
                    return StoreValueGenerationStrategy.None;
            }
        }

        private PrimaryKey BuildPrimaryKey(Database database, IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            var table = database.GetTable(primaryKey.EntityType.StorageName);
            var columns = primaryKey.Properties.Select(
                p => table.GetColumn(p.StorageName)).ToArray();
            var isClustered = primaryKey.IsClustered();

            table.PrimaryKey = new PrimaryKey(PrimaryKeyName(primaryKey), columns, isClustered);

            return table.PrimaryKey;
        }

        private ForeignKey BuildForeignKey(Database database, IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            var table = database.GetTable(foreignKey.EntityType.StorageName);
            var referencedTable = database.GetTable(foreignKey.ReferencedEntityType.StorageName);
            var columns = foreignKey.Properties.Select(
                p => table.GetColumn(p.StorageName)).ToArray();
            var referenceColumns = foreignKey.ReferencedProperties.Select(
                p => referencedTable.GetColumn(p.StorageName)).ToArray();
            var cascadeDelete = foreignKey.CascadeDelete();

            var storeForeignKey = new ForeignKey(
                ForeignKeyName(foreignKey), columns, referenceColumns, cascadeDelete);

            table.AddForeignKey(storeForeignKey);

            return storeForeignKey;
        }
    }
}
