// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;

namespace Microsoft.Data.Entity.Migrations
{
    // TODO: Consider throwing exceptions instead of Contract.Assert. 
    // This is related to additional validation in the database model.
    public class DatabaseModelModifier : IMigrationOperationVisitor<DatabaseModel>
    {
        public virtual void Visit(CreateDatabaseOperation createDatabaseOperation, DatabaseModel databaseModel)
        {
        }

        public virtual void Visit(DropDatabaseOperation dropDatabaseOperation, DatabaseModel databaseModel)
        {
        }

        public virtual void Visit(CreateSequenceOperation createSequenceOperation, DatabaseModel databaseModel)
        {
            databaseModel.AddSequence(createSequenceOperation.Sequence);
        }

        public virtual void Visit(DropSequenceOperation dropSequenceOperation, DatabaseModel databaseModel)
        {
            databaseModel.RemoveSequence(dropSequenceOperation.SequenceName);
        }

        public virtual void Visit(CreateTableOperation createTableOperation, DatabaseModel databaseModel)
        {
            databaseModel.AddTable(createTableOperation.Table);
        }

        public virtual void Visit(DropTableOperation dropTableOperation, DatabaseModel databaseModel)
        {
            databaseModel.RemoveTable(dropTableOperation.TableName);
        }

        public virtual void Visit(RenameTableOperation renameTableOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(renameTableOperation.TableName);
            table.Name = new SchemaQualifiedName(renameTableOperation.NewTableName, renameTableOperation.TableName.Schema);
        }

        public virtual void Visit(MoveTableOperation moveTableOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(moveTableOperation.TableName);
            table.Name = new SchemaQualifiedName(moveTableOperation.TableName.Name, moveTableOperation.NewSchema);
        }

        public virtual void Visit(AddColumnOperation addColumnOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(addColumnOperation.TableName);
            table.AddColumn(addColumnOperation.Column);
        }

        public virtual void Visit(DropColumnOperation dropColumnOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(dropColumnOperation.TableName);
            table.RemoveColumn(dropColumnOperation.ColumnName);
        }

        public virtual void Visit(AlterColumnOperation alterColumnOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(alterColumnOperation.TableName);
            var newColumn = alterColumnOperation.NewColumn;
            var column = table.GetColumn(newColumn.Name);
            column.Copy(newColumn);
        }

        public virtual void Visit(AddDefaultConstraintOperation addDefaultConstraintOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(addDefaultConstraintOperation.TableName);
            var column = table.GetColumn(addDefaultConstraintOperation.ColumnName);

            Contract.Assert(!column.HasDefault);

            column.DefaultValue = addDefaultConstraintOperation.DefaultValue;
            column.DefaultSql = addDefaultConstraintOperation.DefaultSql;
        }

        public virtual void Visit(DropDefaultConstraintOperation dropDefaultConstraintOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(dropDefaultConstraintOperation.TableName);
            var column = table.GetColumn(dropDefaultConstraintOperation.ColumnName);

            Contract.Assert(column.HasDefault);

            column.DefaultValue = null;
            column.DefaultSql = null;
        }

        public virtual void Visit(RenameColumnOperation renameColumnOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(renameColumnOperation.TableName);
            var column = table.GetColumn(renameColumnOperation.ColumnName);
            column.Name = renameColumnOperation.NewColumnName;
        }

        public virtual void Visit(AddPrimaryKeyOperation addPrimaryKeyOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(addPrimaryKeyOperation.TableName);

            Contract.Assert(table.PrimaryKey == null);

            table.PrimaryKey = new PrimaryKey(
                addPrimaryKeyOperation.PrimaryKeyName,
                addPrimaryKeyOperation.ColumnNames.Select(table.GetColumn).ToArray(),
                addPrimaryKeyOperation.IsClustered);
        }

        public virtual void Visit(DropPrimaryKeyOperation dropPrimaryKeyOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(dropPrimaryKeyOperation.TableName);

            Contract.Assert(
                table.PrimaryKey != null 
                && table.PrimaryKey.Name == dropPrimaryKeyOperation.PrimaryKeyName);

            table.PrimaryKey = null;
        }

        public virtual void Visit(AddForeignKeyOperation addForeignKeyOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(addForeignKeyOperation.TableName);
            var referencedTable = databaseModel.GetTable(addForeignKeyOperation.ReferencedTableName);
            table.AddForeignKey(
                new ForeignKey(
                    addForeignKeyOperation.ForeignKeyName,
                    addForeignKeyOperation.ColumnNames.Select(table.GetColumn).ToArray(),
                    addForeignKeyOperation.ReferencedColumnNames.Select(referencedTable.GetColumn).ToArray(),
                    addForeignKeyOperation.CascadeDelete));
        }

        public virtual void Visit(DropForeignKeyOperation dropForeignKeyOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(dropForeignKeyOperation.TableName);
            table.RemoveForeignKey(dropForeignKeyOperation.ForeignKeyName);
        }

        public virtual void Visit(CreateIndexOperation createIndexOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(createIndexOperation.TableName);
            table.AddIndex(
                new Index(
                    createIndexOperation.IndexName,
                    createIndexOperation.ColumnNames.Select(table.GetColumn).ToArray(),
                    createIndexOperation.IsUnique,
                    createIndexOperation.IsClustered));
        }

        public virtual void Visit(DropIndexOperation dropIndexOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(dropIndexOperation.TableName);
            table.RemoveIndex(dropIndexOperation.IndexName);
        }

        public virtual void Visit(RenameIndexOperation renameIndexOperation, DatabaseModel databaseModel)
        {
            var table = databaseModel.GetTable(renameIndexOperation.TableName);
            var index = table.GetIndex(renameIndexOperation.IndexName);
            index.Name = renameIndexOperation.NewIndexName;
        }

        public virtual void Visit(SqlOperation sqlOperation, DatabaseModel databaseModel)
        {
        }
    }
}
