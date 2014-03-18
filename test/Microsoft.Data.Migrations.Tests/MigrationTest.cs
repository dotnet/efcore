// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Migrations.Tests
{
    public class MigrationTest
    {
        [Fact]
        public void CreateSequence_adds_migration_operation_on_upgrade_and_downgrade()
        {
            CreateSequence_adds_migration_operation(upgrade: true);
            CreateSequence_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void DropSequence_adds_migration_operation_on_upgrade_and_downgrade()
        {
            DropSequence_adds_migration_operation(upgrade: true);
            DropSequence_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void CreateTable_adds_migration_operation_on_upgrade_and_downgrade()
        {
            CreateTable_adds_migration_operation(upgrade: true);
            CreateTable_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void DropTable_adds_migration_operation_on_upgrade_and_downgrade()
        {
            DropTable_adds_migration_operation(upgrade: true);
            DropTable_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void RenameTable_adds_migration_operation_on_upgrade_and_downgrade()
        {
            RenameTable_adds_migration_operation(upgrade: true);
            RenameTable_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void MoveTable_adds_migration_operation_on_upgrade_and_downgrade()
        {
            MoveTable_adds_migration_operation(upgrade: true);
            MoveTable_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void AddColumn_adds_migration_operation_on_upgrade_and_downgrade()
        {
            AddColumn_adds_migration_operation(upgrade: true);
            AddColumn_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void DropColumn_adds_migration_operation_on_upgrade_and_downgrade()
        {
            DropColumn_adds_migration_operation(upgrade: true);
            DropColumn_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void RenameColumn_adds_migration_operation_on_upgrade_and_downgrade()
        {
            RenameColumn_adds_migration_operation(upgrade: true);
            RenameColumn_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void AddPrimaryKey_adds_migration_operation_on_upgrade_and_downgrade()
        {
            AddPrimaryKey_adds_migration_operation(upgrade: true);
            AddPrimaryKey_adds_migration_operation(upgrade: false);
        }

        [Fact]
        public void DropPrimaryKey_adds_migration_operation_on_upgrade_and_downgrade()
        {
            DropPrimaryKey_adds_migration_operation(upgrade: true);
            DropPrimaryKey_adds_migration_operation(upgrade: false);
        }

        private static void CreateSequence_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var sequence = new Sequence("schema.Sequence");

            var operations = ExecuteMigrationAction(
                (m) => m.CreateSequence(sequence), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<CreateSequenceOperation>(operations[0]);

            var createSequenceOperation = (CreateSequenceOperation)operations[0];

            Assert.Same(sequence, createSequenceOperation.Sequence);
        }

        private static void DropSequence_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var sequence = new Sequence("schema.Sequence");
            database.AddSequence(sequence);

            var operations = ExecuteMigrationAction(
                (m) => m.DropSequence(sequence.Name), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropSequenceOperation>(operations[0]);

            var dropSequenceOperation = (DropSequenceOperation)operations[0];

            Assert.Same(sequence, dropSequenceOperation.Sequence);
        }

        private static void CreateTable_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");

            var operations = ExecuteMigrationAction(
                (m) => m.CreateTable(table), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);

            var createTableOperation = (CreateTableOperation)operations[0];

            Assert.Same(table, createTableOperation.Table);
        }

        private static void DropTable_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");
            database.AddTable(table);

            var operations = ExecuteMigrationAction(
                (m) => m.DropTable(table.Name), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropTableOperation>(operations[0]);

            var dropTableOperation = (DropTableOperation)operations[0];

            Assert.Same(table, dropTableOperation.Table);
        }

        private static void RenameTable_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");
            database.AddTable(table);

            var operations = ExecuteMigrationAction(
                (m) => m.RenameTable(table.Name, "Table2"), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameTableOperation>(operations[0]);

            var renameTableOperation = (RenameTableOperation)operations[0];

            Assert.Same(table, renameTableOperation.Table);
            Assert.Equal("Table2", renameTableOperation.TableName);
        }

        private static void MoveTable_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");
            database.AddTable(table);

            var operations = ExecuteMigrationAction(
                (m) => m.MoveTable(table.Name, "schema2"), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<MoveTableOperation>(operations[0]);

            var moveTableOperation = (MoveTableOperation)operations[0];

            Assert.Same(table, moveTableOperation.Table);
            Assert.Equal("schema2", moveTableOperation.Schema);
        }

        private static void AddColumn_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");
            var column = new Column("C", "int");
            database.AddTable(table);

            var operations = ExecuteMigrationAction(
                (m) => m.AddColumn(column, table.Name), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AddColumnOperation>(operations[0]);

            var addColumnOperation = (AddColumnOperation)operations[0];

            Assert.Same(column, addColumnOperation.Column);
            Assert.Same(table, addColumnOperation.Table);
        }

        private static void DropColumn_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");
            var column = new Column("C", "int");
            table.AddColumn(column);
            database.AddTable(table);

            var operations = ExecuteMigrationAction(
                (m) => m.DropColumn(column.Name, table.Name), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropColumnOperation>(operations[0]);

            var dropColumnOperation = (DropColumnOperation)operations[0];

            Assert.Same(column, dropColumnOperation.Column);
        }

        private static void RenameColumn_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");
            var column = new Column("C", "int");
            table.AddColumn(column);
            database.AddTable(table);

            var operations = ExecuteMigrationAction(
                (m) => m.RenameColumn(column.Name, table.Name, "C2"), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<RenameColumnOperation>(operations[0]);

            var renameColumnOperation = (RenameColumnOperation)operations[0];

            Assert.Same(column, renameColumnOperation.Column);
            Assert.Equal("C2", renameColumnOperation.ColumnName);
        }

        private static void AddPrimaryKey_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");
            var column = new Column("C", "int");
            var primaryKey = new PrimaryKey("schema.PK", new[] { column.Name });
            table.AddColumn(column);
            database.AddTable(table);

            var operations = ExecuteMigrationAction(
                (m) => m.AddPrimaryKey(primaryKey, table.Name), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<AddPrimaryKeyOperation>(operations[0]);

            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[0];

            Assert.Same(primaryKey, addPrimaryKeyOperation.PrimaryKey);
            Assert.Same(table, addPrimaryKeyOperation.Table);
        }

        private static void DropPrimaryKey_adds_migration_operation(bool upgrade)
        {
            var database = new Database("Db");
            var table = new Table("schema.Table");
            var column = new Column("C", "int");
            var primaryKey = new PrimaryKey("schema.PK", new[] { column });
            table.AddColumn(column);
            table.PrimaryKey = primaryKey;
            database.AddTable(table);

            var operations = ExecuteMigrationAction(
                (m) => m.DropPrimaryKey(primaryKey.Name), database, upgrade);

            Assert.Equal(1, operations.Count);
            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];

            Assert.Same(primaryKey, dropPrimaryKeyOperation.PrimaryKey);
        }

        private static IReadOnlyList<MigrationOperation> ExecuteMigrationAction(
            Action<Migration> migrationAction, Database database, bool upgrade)
        {
            var migration = upgrade
                ? new SampleMigration(sourceDatabase: database, targetDatabase: null)
                : new SampleMigration(sourceDatabase: null, targetDatabase: database);

            migration.MigrationAction = migrationAction;

            return upgrade ? migration.Upgrade() : migration.Downgrade();
        }

        private class SampleMigration : Migration
        {
            public SampleMigration(Database sourceDatabase, Database targetDatabase)
                : base(sourceDatabase, targetDatabase)
            {
            }

            public Action<Migration> MigrationAction { get; set; }

            protected override void Up()
            {
                MigrationAction(this);
            }

            protected override void Down()
            {
                MigrationAction(this);
            }
        }
    }
}
