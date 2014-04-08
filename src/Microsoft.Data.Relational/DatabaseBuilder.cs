// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Model;
using Microsoft.Data.Relational.Utilities;
using ForeignKey = Microsoft.Data.Relational.Model.ForeignKey;

namespace Microsoft.Data.Relational
{
    public class DatabaseBuilder
    {
        private ModelDatabaseMapping _mapping;

        public virtual Database Build([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            return BuildMapping(model).Database;
        }

        public virtual ModelDatabaseMapping BuildMapping([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var database = new Database();
            _mapping = new ModelDatabaseMapping(model, database);

            foreach (var entityType in model.EntityTypes)
            {
                BuildTable(database, entityType);
                BuildPrimaryKey(database, entityType.GetKey());
            }

            foreach (var entityType in model.EntityTypes)
            {
                foreach (var foreignKey in entityType.ForeignKeys)
                {
                    BuildForeignKey(database, foreignKey);
                }
            }

            return _mapping;
        }

        public virtual string TableName([NotNull] IEntityType type)
        {
            Check.NotNull(type, "type");

            return type.StorageName ?? type.Name;
        }

        public virtual string ColumnName([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return property.StorageName ?? property.Name;
        }

        public virtual string PrimaryKeyName([NotNull] IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            return primaryKey.StorageName ?? string.Format("PK_{0}", TableName(primaryKey.EntityType));
        }

        public virtual string ForeignKeyName([NotNull] IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            return foreignKey.StorageName ?? string.Format(
                "FK_{0}_{1}_{2}", 
                TableName(foreignKey.EntityType), 
                TableName(foreignKey.ReferencedEntityType), 
                string.Join("_", foreignKey.Properties.OrderBy(p => p.Name).Select(p => ColumnName(p))));
        }

        public virtual string IndexName([NotNull] Table table, [NotNull] IEnumerable<Column> columns)
        {
            Check.NotNull(table, "table");
            Check.NotNull(columns, "columns");

            return string.Format(
                "IX_{0}_{1}",
                table.Name,
                string.Join("_", columns.OrderBy(c => c.Name).Select(c => c.Name)));
        }

        private void BuildTable(Database database, IEntityType entityType)
        {
            var table = new Table(TableName(entityType));

            foreach (var property in entityType.Properties)
            {
                BuildColumn(table, property);
            }

            database.AddTable(table);
            _mapping.Map(entityType, table);
        }

        private void BuildColumn(Table table, IProperty property)
        {
            var column =
                new Column(ColumnName(property), property.PropertyType, property.ColumnType())
                    {
                        IsNullable = property.IsNullable,
                        DefaultValue = property.ColumnDefaultValue(),
                        DefaultSql = property.ColumnDefaultSql(),
                        ValueGenerationStrategy = TranslateValueGenerationStrategy(property.ValueGenerationStrategy)
                    };

            table.AddColumn(column);
            _mapping.Map(property, column);
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

        private void BuildPrimaryKey(Database database, IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            var table = database.GetTable(TableName(primaryKey.EntityType));
            var columns = primaryKey.Properties.Select(
                p => table.GetColumn(ColumnName(p))).ToArray();
            var isClustered = primaryKey.IsClustered();

            table.PrimaryKey = new PrimaryKey(PrimaryKeyName(primaryKey), columns, isClustered);
            _mapping.Map(primaryKey, table.PrimaryKey);
        }

        private void BuildForeignKey(Database database, IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            var table = database.GetTable(TableName(foreignKey.EntityType));
            var referencedTable = database.GetTable(TableName(foreignKey.ReferencedEntityType));
            var columns = foreignKey.Properties.Select(
                p => table.GetColumn(ColumnName(p))).ToArray();
            var referenceColumns = foreignKey.ReferencedProperties.Select(
                p => referencedTable.GetColumn(ColumnName(p))).ToArray();
            var cascadeDelete = foreignKey.CascadeDelete();

            var storeForeignKey = new ForeignKey(
                ForeignKeyName(foreignKey), columns, referenceColumns, cascadeDelete);
            
            table.AddForeignKey(storeForeignKey);
            _mapping.Map(foreignKey, table.ForeignKeys.Last());
        }

        private static void BuildIndex(Database database, IEntityType entityType)
        {
            // TODO: Not implemented.

            throw new NotImplementedException();
        }
    }
}
