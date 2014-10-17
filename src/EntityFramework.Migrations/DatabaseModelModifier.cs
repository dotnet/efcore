// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Migrations.Utilities;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Migrations
{
    // TODO: Consider throwing exceptions instead of Contract.Assert. 
    // This is related to additional validation in the database model.
    public class DatabaseModelModifier : MigrationOperationVisitor<DatabaseModel>
    {
        public virtual void Modify([NotNull] DatabaseModel databaseModel, [NotNull] IEnumerable<MigrationOperation> migrationOperations)
        {
            Check.NotNull(databaseModel, "databaseModel");
            Check.NotNull(migrationOperations, "migrationOperations");

            foreach (var operation in migrationOperations)
            {
                Modify(databaseModel, operation);
            }
        }

        public virtual void Modify([NotNull] DatabaseModel databaseModel, [NotNull] MigrationOperation operation)
        {
            Check.NotNull(databaseModel, "databaseModel");
            Check.NotNull(operation, "operation");

            operation.Accept(this, databaseModel);
        }

        public override void Visit(CreateSequenceOperation createSequenceOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(createSequenceOperation, "createSequenceOperation");
            Check.NotNull(databaseModel, "databaseModel");

            databaseModel.AddSequence(createSequenceOperation.Sequence.Clone(new CloneContext()));
        }

        public override void Visit(DropSequenceOperation dropSequenceOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(dropSequenceOperation, "dropSequenceOperation");
            Check.NotNull(databaseModel, "databaseModel");

            databaseModel.RemoveSequence(dropSequenceOperation.SequenceName);
        }

        public override void Visit(RenameSequenceOperation renameSequenceOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(renameSequenceOperation, "renameSequenceOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var sequence = databaseModel.GetSequence(renameSequenceOperation.SequenceName);
            sequence.Name = new SchemaQualifiedName(renameSequenceOperation.NewSequenceName, renameSequenceOperation.SequenceName.Schema);
        }

        public override void Visit(MoveSequenceOperation moveSequenceOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(moveSequenceOperation, "moveSequenceOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var sequence = databaseModel.GetSequence(moveSequenceOperation.SequenceName);
            sequence.Name = new SchemaQualifiedName(moveSequenceOperation.SequenceName.Name, moveSequenceOperation.NewSchema);
        }

        public override void Visit(AlterSequenceOperation alterSequenceOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(alterSequenceOperation, "alterSequenceOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var sequence = databaseModel.GetSequence(alterSequenceOperation.SequenceName);
            sequence.IncrementBy = alterSequenceOperation.NewIncrementBy;
        }

        public override void Visit(CreateTableOperation createTableOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(createTableOperation, "createTableOperation");
            Check.NotNull(databaseModel, "databaseModel");

            // TODO: Revisit, figure out a better place to put the cloning code below, 
            // or find a solution that doesn't require it.
            // Currently the table passed to CreateTable operation contains foreign keys
            // and indexes. The foreign keys are need to be able to determine the correct
            // data type of a column. However the differ creates separate operations for
            // creating these foreign keys and indexes.

            var cloneContext = new CloneContext();
            var table
                = new Table(
                    createTableOperation.Table.Name,
                    createTableOperation.Table.Columns.Select(c => c.Clone(cloneContext)));

            if (createTableOperation.Table.PrimaryKey != null)
            {
                table.PrimaryKey = createTableOperation.Table.PrimaryKey.Clone(cloneContext);
            }

            foreach (var uniqueConstraint in createTableOperation.Table.UniqueConstraints)
            {
                table.AddUniqueConstraint(uniqueConstraint.Clone(cloneContext));
            }

            databaseModel.AddTable(table);
        }

        public override void Visit(DropTableOperation dropTableOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(dropTableOperation, "dropTableOperation");
            Check.NotNull(databaseModel, "databaseModel");

            databaseModel.RemoveTable(dropTableOperation.TableName);
        }

        public override void Visit(RenameTableOperation renameTableOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(renameTableOperation, "renameTableOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(renameTableOperation.TableName);
            table.Name = new SchemaQualifiedName(renameTableOperation.NewTableName, renameTableOperation.TableName.Schema);
        }

        public override void Visit(MoveTableOperation moveTableOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(moveTableOperation, "moveTableOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(moveTableOperation.TableName);
            table.Name = new SchemaQualifiedName(moveTableOperation.TableName.Name, moveTableOperation.NewSchema);
        }

        public override void Visit(AddColumnOperation addColumnOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(addColumnOperation, "addColumnOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(addColumnOperation.TableName);
            table.AddColumn(addColumnOperation.Column.Clone(new CloneContext()));
        }

        public override void Visit(DropColumnOperation dropColumnOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(dropColumnOperation, "dropColumnOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(dropColumnOperation.TableName);
            table.RemoveColumn(dropColumnOperation.ColumnName);
        }

        public override void Visit(AlterColumnOperation alterColumnOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(alterColumnOperation, "alterColumnOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(alterColumnOperation.TableName);
            var newColumn = alterColumnOperation.NewColumn;
            var column = table.GetColumn(newColumn.Name);
            column.Copy(newColumn);
        }

        public override void Visit(AddDefaultConstraintOperation addDefaultConstraintOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(addDefaultConstraintOperation, "addDefaultConstraintOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(addDefaultConstraintOperation.TableName);
            var column = table.GetColumn(addDefaultConstraintOperation.ColumnName);

            Contract.Assert(!column.HasDefault);

            column.DefaultValue = addDefaultConstraintOperation.DefaultValue;
            column.DefaultSql = addDefaultConstraintOperation.DefaultSql;
        }

        public override void Visit(DropDefaultConstraintOperation dropDefaultConstraintOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(dropDefaultConstraintOperation, "dropDefaultConstraintOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(dropDefaultConstraintOperation.TableName);
            var column = table.GetColumn(dropDefaultConstraintOperation.ColumnName);

            Contract.Assert(column.HasDefault);

            column.DefaultValue = null;
            column.DefaultSql = null;
        }

        public override void Visit(RenameColumnOperation renameColumnOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(renameColumnOperation, "renameColumnOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(renameColumnOperation.TableName);
            var column = table.GetColumn(renameColumnOperation.ColumnName);
            column.Name = renameColumnOperation.NewColumnName;
        }

        public override void Visit(AddPrimaryKeyOperation addPrimaryKeyOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(addPrimaryKeyOperation, "addPrimaryKeyOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(addPrimaryKeyOperation.TableName);

            Contract.Assert(table.PrimaryKey == null);

            table.PrimaryKey = new PrimaryKey(
                addPrimaryKeyOperation.PrimaryKeyName,
                addPrimaryKeyOperation.ColumnNames.Select(table.GetColumn).ToArray(),
                addPrimaryKeyOperation.IsClustered);
        }

        public override void Visit(DropPrimaryKeyOperation dropPrimaryKeyOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(dropPrimaryKeyOperation, "dropPrimaryKeyOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(dropPrimaryKeyOperation.TableName);

            Contract.Assert(
                table.PrimaryKey != null
                && table.PrimaryKey.Name == dropPrimaryKeyOperation.PrimaryKeyName);

            table.PrimaryKey = null;
        }

        public override void Visit(AddUniqueConstraintOperation addUniqueConstraintOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(addUniqueConstraintOperation, "addUniqueConstraintOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(addUniqueConstraintOperation.TableName);
            table.AddUniqueConstraint(
                new UniqueConstraint(
                    addUniqueConstraintOperation.UniqueConstraintName,
                    addUniqueConstraintOperation.ColumnNames.Select(table.GetColumn).ToArray()));
        }

        public override void Visit(DropUniqueConstraintOperation dropUniqueConstraintOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(dropUniqueConstraintOperation, "dropUniqueConstraintOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(dropUniqueConstraintOperation.TableName);
            table.RemoveUniqueConstraint(dropUniqueConstraintOperation.UniqueConstraintName);
        }

        public override void Visit(AddForeignKeyOperation addForeignKeyOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(addForeignKeyOperation, "addForeignKeyOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(addForeignKeyOperation.TableName);
            var referencedTable = databaseModel.GetTable(addForeignKeyOperation.ReferencedTableName);
            table.AddForeignKey(
                new ForeignKey(
                    addForeignKeyOperation.ForeignKeyName,
                    addForeignKeyOperation.ColumnNames.Select(table.GetColumn).ToArray(),
                    addForeignKeyOperation.ReferencedColumnNames.Select(referencedTable.GetColumn).ToArray(),
                    addForeignKeyOperation.CascadeDelete));
        }

        public override void Visit(DropForeignKeyOperation dropForeignKeyOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(dropForeignKeyOperation, "dropForeignKeyOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(dropForeignKeyOperation.TableName);
            table.RemoveForeignKey(dropForeignKeyOperation.ForeignKeyName);
        }

        public override void Visit(CreateIndexOperation createIndexOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(createIndexOperation, "createIndexOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(createIndexOperation.TableName);
            table.AddIndex(
                new Index(
                    createIndexOperation.IndexName,
                    createIndexOperation.ColumnNames.Select(table.GetColumn).ToArray(),
                    createIndexOperation.IsUnique,
                    createIndexOperation.IsClustered));
        }

        public override void Visit(DropIndexOperation dropIndexOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(dropIndexOperation, "dropIndexOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(dropIndexOperation.TableName);
            table.RemoveIndex(dropIndexOperation.IndexName);
        }

        public override void Visit(RenameIndexOperation renameIndexOperation, DatabaseModel databaseModel)
        {
            Check.NotNull(renameIndexOperation, "renameIndexOperation");
            Check.NotNull(databaseModel, "databaseModel");

            var table = databaseModel.GetTable(renameIndexOperation.TableName);
            var index = table.GetIndex(renameIndexOperation.IndexName);
            index.Name = renameIndexOperation.NewIndexName;
        }

        protected override void VisitDefault(MigrationOperation sqlOperation, DatabaseModel databaseModel)
        {
        }
    }
}
