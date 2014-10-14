// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.SQLite.Tests
{
    public class SQLiteMigrationOperationPreProcessorTest
    {
        [Fact]
        public void Visit_with_create_table_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            var operation
                = new CreateTableOperation(
                    new Table("T", new[]
                        {
                            new Column("Id", typeof(int))
                        }));

            var operations = PreProcess(modelBuilder, operation);

            Assert.Equal(1, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);

            var createTableOperation = (CreateTableOperation)operations[0];
            Assert.NotSame(operation, createTableOperation);
            Assert.NotSame(operation.Table, createTableOperation.Table);
            Assert.Equal("T", createTableOperation.Table.Name);
            Assert.Equal(new[] { "Id" }, createTableOperation.Table.Columns.Select(c => c.Name));
            Assert.Equal(new[] { typeof(int) }, createTableOperation.Table.Columns.Select(c => c.ClrType));
        }

        [Fact]
        public void Visit_with_create_table_operation_followed_by_supported_table_subordinate_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            var operation
                = new CreateTableOperation(
                    new Table("T", new[]
                        {
                            new Column("Id", typeof(int))
                        }));
            var addColumnOperation = new AddColumnOperation("T", new Column("C", typeof(string)));

            var operations = PreProcess(modelBuilder, operation, addColumnOperation);

            Assert.Equal(1, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);

            var createTableOperation = (CreateTableOperation)operations[0];
            Assert.NotSame(operation, createTableOperation);
            Assert.NotSame(operation.Table, createTableOperation.Table);
            Assert.Equal("T", createTableOperation.Table.Name);
            Assert.Equal(new[] { "Id", "C" }, createTableOperation.Table.Columns.Select(c => c.Name));
            Assert.Equal(new[] { typeof(int), typeof(string) }, createTableOperation.Table.Columns.Select(c => c.ClrType));
        }

        [Fact]
        public void Visit_with_create_table_operation_followed_by_unsupported_table_subordinate_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("T1",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var operation
                = new CreateTableOperation(
                    new Table("T2", new[]
                        {
                            new Column("C", typeof(int))
                        }));
            var addForeignKeyOperation
                = new AddForeignKeyOperation("T2", "FK", new[] { "C" }, "T1", new[] { "Id" }, cascadeDelete: true);

            var operations = PreProcess(modelBuilder, operation, addForeignKeyOperation);

            Assert.Equal(1, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);

            var createTableOperation = (CreateTableOperation)operations[0];

            Assert.NotSame(operation, createTableOperation);
            Assert.NotSame(operation.Table, createTableOperation.Table);
            Assert.Equal("T2", createTableOperation.Table.Name);
            Assert.Equal(new[] { "C" }, createTableOperation.Table.Columns.Select(c => c.Name));
            Assert.Equal(new[] { typeof(int) }, createTableOperation.Table.Columns.Select(c => c.ClrType));
            Assert.Equal(1, createTableOperation.Table.ForeignKeys.Count);
            Assert.Equal("FK", createTableOperation.Table.ForeignKeys[0].Name);
            Assert.Equal("T1", createTableOperation.Table.ForeignKeys[0].ReferencedTable.Name);
            Assert.Equal(new[] { "C" }, createTableOperation.Table.ForeignKeys[0].Columns.Select(c => c.Name));
            Assert.Equal(new[] { "Id" }, createTableOperation.Table.ForeignKeys[0].ReferencedColumns.Select(c => c.Name));
        }

        [Fact]
        public void Visit_with_create_table_operation_followed_by_rename_operation_and_table_subordinate_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            var operation
                = new CreateTableOperation(
                    new Table("dbo.T", new[]
                        {
                            new Column("Id", typeof(int))
                        }));
            var moveTableOperation = new MoveTableOperation("dbo.T", "dbo2");
            var renameTableOperation = new RenameTableOperation("dbo2.T", "T2");
            var addColumnOperation = new AddColumnOperation("dbo2.T2", new Column("C", typeof(string)));

            var operations = PreProcess(modelBuilder, operation, moveTableOperation, renameTableOperation, addColumnOperation);

            Assert.Equal(1, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);

            var createTableOperation = (CreateTableOperation)operations[0];
            Assert.NotSame(operation, createTableOperation);
            Assert.NotSame(operation.Table, createTableOperation.Table);
            Assert.Equal("dbo2.T2", createTableOperation.Table.Name);
            Assert.Equal(new[] { "Id", "C" }, createTableOperation.Table.Columns.Select(c => c.Name));
            Assert.Equal(new[] { typeof(int), typeof(string) }, createTableOperation.Table.Columns.Select(c => c.ClrType));
        }

        [Fact]
        public void Visit_with_supported_table_subordinate_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("T",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var operation = new AddColumnOperation("T", new Column("C", typeof(string)));

            var operations = PreProcess(modelBuilder, operation);

            Assert.Equal(1, operations.Count);
            Assert.Same(operation, operations[0]);
        }

        [Fact]
        public void Visit_with_rename_operation_followed_by_supported_table_subordinate_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("T",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                        // TODO: SQLite-specific. Issue #875
                        b.ForRelational().Table("T", "dbo");
                    });
            var moveTableOperation = new MoveTableOperation("dbo.T", "dbo2");
            var renameTableOperation = new RenameTableOperation("dbo2.T", "T2");
            var addColumnOperation = new AddColumnOperation("dbo2.T2", new Column("C", typeof(string)));

            var operations = PreProcess(modelBuilder, moveTableOperation, renameTableOperation, addColumnOperation);

            Assert.Equal(3, operations.Count);
            Assert.Same(moveTableOperation, operations[0]);
            Assert.Same(renameTableOperation, operations[1]);
            Assert.Same(addColumnOperation, operations[2]);
        }

        [Fact]
        public void Visit_with_unsupported_table_subordinate_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("T1",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            modelBuilder.Entity("T2",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("C");
                        b.Key("Id");
                    });

            var addForeignKeyOperation
                = new AddForeignKeyOperation("T2", "FK", new[] { "C" }, "T1", new[] { "Id" }, cascadeDelete: true);

            var operations = PreProcess(modelBuilder, addForeignKeyOperation);

            Assert.Equal(4, operations.Count);
            Assert.IsType<RenameTableOperation>(operations[0]);
            Assert.IsType<CreateTableOperation>(operations[1]);
            Assert.IsType<CopyDataOperation>(operations[2]);
            Assert.IsType<DropTableOperation>(operations[3]);

            var renameTableOperation = (RenameTableOperation)operations[0];

            Assert.Equal("T2", renameTableOperation.TableName);
            Assert.Equal("__mig_tmp__T2", renameTableOperation.NewTableName);

            var createTableOperation = (CreateTableOperation)operations[1];

            Assert.Equal("T2", createTableOperation.Table.Name);
            Assert.Equal(new[] { "Id", "C" }, createTableOperation.Table.Columns.Select(c => c.Name));
            Assert.Equal(new[] { typeof(int), typeof(int) }, createTableOperation.Table.Columns.Select(c => c.ClrType));
            Assert.Equal(1, createTableOperation.Table.ForeignKeys.Count);
            Assert.Equal("FK", createTableOperation.Table.ForeignKeys[0].Name);
            Assert.Equal("T1", createTableOperation.Table.ForeignKeys[0].ReferencedTable.Name);
            Assert.Equal(new[] { "C" }, createTableOperation.Table.ForeignKeys[0].Columns.Select(c => c.Name));
            Assert.Equal(new[] { "Id" }, createTableOperation.Table.ForeignKeys[0].ReferencedColumns.Select(c => c.Name));

            var copyDataOperation = (CopyDataOperation)operations[2];

            Assert.Equal("__mig_tmp__T2", copyDataOperation.SourceTableName);
            Assert.Equal(new[] { "Id", "C" }, copyDataOperation.SourceColumnNames);
            Assert.Equal("T2", copyDataOperation.TargetTableName);
            Assert.Equal(new[] { "Id", "C" }, copyDataOperation.TargetColumnNames);

            var dropTableOperation = (DropTableOperation)operations[3];

            Assert.Equal("__mig_tmp__T2", dropTableOperation.TableName);
        }

        [Fact]
        public void Visit_with_rename_operation_followed_by_unsupported_subordinate_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("T1",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });
            modelBuilder.Entity("T2",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<int>("C");
                        b.Key("Id");
                        // TODO: SQLite-specific. Issue #875
                        b.ForRelational().Table("T", "dbo");
                    });

            var moveTableOperation = new MoveTableOperation("dbo.T", "dbo2");
            var renameTableOperation = new RenameTableOperation("dbo2.T", "T2");
            var addForeignKeyOperation
                = new AddForeignKeyOperation("dbo2.T2", "FK", new[] { "C" }, "T1", new[] { "Id" }, cascadeDelete: true);

            var operations = PreProcess(modelBuilder, moveTableOperation, renameTableOperation, addForeignKeyOperation);

            Assert.Equal(3, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);
            Assert.IsType<CopyDataOperation>(operations[1]);
            Assert.IsType<DropTableOperation>(operations[2]);

            var createTableOperation = (CreateTableOperation)operations[0];

            Assert.Equal("dbo2.T2", createTableOperation.Table.Name);
            Assert.Equal(new[] { "Id", "C" }, createTableOperation.Table.Columns.Select(c => c.Name));
            Assert.Equal(new[] { typeof(int), typeof(int) }, createTableOperation.Table.Columns.Select(c => c.ClrType));
            Assert.Equal(1, createTableOperation.Table.ForeignKeys.Count);
            Assert.Equal("FK", createTableOperation.Table.ForeignKeys[0].Name);
            Assert.Equal("T1", createTableOperation.Table.ForeignKeys[0].ReferencedTable.Name);
            Assert.Equal(new[] { "C" }, createTableOperation.Table.ForeignKeys[0].Columns.Select(c => c.Name));
            Assert.Equal(new[] { "Id" }, createTableOperation.Table.ForeignKeys[0].ReferencedColumns.Select(c => c.Name));

            var copyDataOperation = (CopyDataOperation)operations[1];

            Assert.Equal("dbo.T", copyDataOperation.SourceTableName);
            Assert.Equal(new[] { "Id", "C" }, copyDataOperation.SourceColumnNames);
            Assert.Equal("dbo2.T2", copyDataOperation.TargetTableName);
            Assert.Equal(new[] { "Id", "C" }, copyDataOperation.TargetColumnNames);

            var dropTableOperation = (DropTableOperation)operations[2];

            Assert.Equal("dbo.T", dropTableOperation.TableName);
        }

        [Fact]
        public void Visit_with_non_table_operation_handles_pending_operations()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("T1",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var createTableOperation
                = new CreateTableOperation(
                    new Table("T2", new[]
                        {
                            new Column("C", typeof(int))
                        }));
            var addForeignKeyOperation
                = new AddForeignKeyOperation("T2", "FK", new[] { "C" }, "T1", new[] { "Id" }, cascadeDelete: true);
            var sqlOperation = new SqlOperation("Sql");

            var preProcessor = new SQLiteMigrationOperationPreProcessor();
            var context
                = new MySQLiteMigrationOperationPreProcessorContext(
                    new SQLiteMigrationOperationSqlGeneratorFactory().Create(
                        new DatabaseBuilder().GetDatabase(modelBuilder.Model)));

            preProcessor.Visit(createTableOperation, context);
            preProcessor.Visit(addForeignKeyOperation, context);

            context.HandlePendingOperationsFlag = false;

            Assert.Equal(0, context.Statements.Count);

            context.HandlePendingOperationsFlag = true;

            preProcessor.Visit(sqlOperation, context);

            context.HandlePendingOperationsFlag = false;

            Assert.Equal(2, context.Statements.Count);
            Assert.Equal(
                @"CREATE TABLE ""T2"" (
    ""C"" INT,
    CONSTRAINT ""FK"" FOREIGN KEY (""C"") REFERENCES ""T1"" (""Id"") ON DELETE CASCADE
)",
                context.Statements[0].Sql);
            Assert.Equal("Sql", context.Statements[1].Sql);
        }

        [Fact]
        public void Visit_with_rename_index_operation()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("T",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                        // TODO: SQLite-specific. Issue #875
                        b.Index("Id").IsUnique().ForRelational().Name("IX");
                    });

            var renameIndexOperation = new RenameIndexOperation("T", "IX", "IX2");

            var preProcessor = new SQLiteMigrationOperationPreProcessor();
            var context
                = new SQLiteMigrationOperationPreProcessor.Context(
                    new SQLiteMigrationOperationSqlGeneratorFactory().Create(
                        new DatabaseBuilder().GetDatabase(modelBuilder.Model)));

            preProcessor.Visit(renameIndexOperation, context);

            Assert.Equal(2, context.Statements.Count);

            Assert.Equal(@"DROP INDEX ""IX""", context.Statements[0].Sql);
            Assert.Equal(@"CREATE UNIQUE INDEX ""IX"" ON ""T"" (""Id"")", context.Statements[1].Sql);
        }

        [Fact]
        public void Table_operation_order_is_retained()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("T1",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                    });
            modelBuilder.Entity("T2",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Key("Id");
                    });

            var createTableOperation
                = new CreateTableOperation(
                    new Table("T3", new[]
                        {
                            new Column("C", typeof(int))
                        }));
            var addForeignKeyOperation
                = new AddForeignKeyOperation("T3", "FK", new[] { "C" }, "T1", new[] { "Id" }, cascadeDelete: true);
            var addColumnOperation = new AddColumnOperation("T2", new Column("C", typeof(string)));
            var dropColumOperation = new DropColumnOperation("T1", "P");

            var operations = PreProcess(modelBuilder, createTableOperation, addForeignKeyOperation, addColumnOperation, dropColumOperation);

            Assert.Equal(6, operations.Count);
            Assert.IsType<CreateTableOperation>(operations[0]);
            Assert.IsType<AddColumnOperation>(operations[1]);
            Assert.IsType<RenameTableOperation>(operations[2]);
            Assert.IsType<CreateTableOperation>(operations[3]);
            Assert.IsType<CopyDataOperation>(operations[4]);
            Assert.IsType<DropTableOperation>(operations[5]);

            var createTableOperation1 = (CreateTableOperation)operations[0];

            Assert.Equal("T3", createTableOperation1.Table.Name);
            Assert.Equal(1, createTableOperation1.Table.ForeignKeys.Count);
            Assert.Equal("FK", createTableOperation1.Table.ForeignKeys[0].Name);

            Assert.Same(addColumnOperation, operations[1]);

            var renameTableOperation = (RenameTableOperation)operations[2];

            Assert.Equal("T1", renameTableOperation.TableName);
            Assert.Equal("__mig_tmp__T1", renameTableOperation.NewTableName);

            var createTableOperation2 = (CreateTableOperation)operations[3];

            Assert.Equal("T1", createTableOperation2.Table.Name);
            Assert.Equal(new[] { "Id" }, createTableOperation2.Table.Columns.Select(c => c.Name));

            var copyDataOperation = (CopyDataOperation)operations[4];

            Assert.Equal("__mig_tmp__T1", copyDataOperation.SourceTableName);
            Assert.Equal(new[] { "Id" }, copyDataOperation.SourceColumnNames);
            Assert.Equal("T1", copyDataOperation.TargetTableName);
            Assert.Equal(new[] { "Id" }, copyDataOperation.TargetColumnNames);

            var dropTableOperation = (DropTableOperation)operations[5];

            Assert.Equal("__mig_tmp__T1", dropTableOperation.TableName);
        }

        private static IReadOnlyList<MigrationOperation> PreProcess(
            BasicModelBuilder modelBuilder, params MigrationOperation[] operations)
        {
            return PreProcess(new DatabaseBuilder().GetDatabase(modelBuilder.Model), operations);
        }

        private static IReadOnlyList<MigrationOperation> PreProcess(
            DatabaseModel database, params MigrationOperation[] operations)
        {
            var context = new SQLiteMigrationOperationPreProcessor.Context(
                new SQLiteMigrationOperationSqlGeneratorFactory().Create(database));

            foreach (var operation in operations)
            {
                operation.Accept(new SQLiteMigrationOperationPreProcessor(), context);
            }

            context.Database = context.Generator.Database.Clone();

            return context.Handlers.SelectMany(
                h => h.HandleOperations(context).Concat(context.DeferredOperations)).ToArray();
        }

        private class MySQLiteMigrationOperationPreProcessorContext : SQLiteMigrationOperationPreProcessor.Context
        {
            public MySQLiteMigrationOperationPreProcessorContext(SQLiteMigrationOperationSqlGenerator generator)
                : base(generator)
            {
            }

            public bool HandlePendingOperationsFlag { get; set; }

            public override void HandlePendingOperations()
            {
                if (HandlePendingOperationsFlag)
                {
                    base.HandlePendingOperations();
                }
            }
        }
    }
}
