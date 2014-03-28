// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
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
        public virtual Database Build([NotNull] IModel model)
        {
            Check.NotNull(model, "model");

            var database = new Database(model.StorageName);

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

            return database;
        }

        private static void BuildTable(Database database, IEntityType entityType)
        {
            var table = new Table(entityType.StorageName);

            foreach (var property in entityType.Properties)
            {
                BuildColumn(table, property);
            }

            database.AddTable(table);
        }

        private static void BuildColumn(Table table, IProperty property)
        {
            table.AddColumn(
                new Column(property.StorageName, property.PropertyType, property.ColumnType())
                    {
                        IsNullable = property.IsNullable,
                        DefaultValue = property.ColumnDefaultValue(),
                        DefaultSql = property.ColumnDefaultSql()
                    });
        }

        private static void BuildPrimaryKey(Database database, IKey primaryKey)
        {
            Check.NotNull(primaryKey, "primaryKey");

            var table = database.GetTable(primaryKey.EntityType.StorageName);
            var columns = primaryKey.Properties.Select(
                p => table.GetColumn(p.StorageName)).ToArray();
            var isClustered = primaryKey.IsClustered();

            table.PrimaryKey = new PrimaryKey(
                primaryKey.StorageName, columns, isClustered);
        }

        private static void BuildForeignKey(Database database, IForeignKey foreignKey)
        {
            Check.NotNull(foreignKey, "foreignKey");

            var table = database.GetTable(foreignKey.EntityType.StorageName);
            var referencedTable = database.GetTable(foreignKey.ReferencedEntityType.StorageName);
            var columns = foreignKey.Properties.Select(
                p => table.GetColumn(p.StorageName)).ToArray();
            var referenceColumns = foreignKey.ReferencedProperties.Select(
                p => referencedTable.GetColumn(p.StorageName)).ToArray();
            var cascadeDelete = foreignKey.CascadeDelete();

            table.AddForeignKey(new ForeignKey(
                foreignKey.StorageName, columns, referenceColumns, cascadeDelete));
        }

        private static void BuildIndex(Database database, IEntityType entityType)
        {
            // TODO: Not implemented.

            throw new NotImplementedException();
        }
    }
}
