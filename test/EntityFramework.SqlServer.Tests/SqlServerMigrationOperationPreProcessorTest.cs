// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;
using Index = Microsoft.Data.Entity.Relational.Model.Index;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerMigrationOperationPreProcessorTest
    {
        [Fact]
        public void Visit_with_alter_column_operation_and_timestamp_column()
        {
            var database = new DatabaseModel();
            var column0
                = new Column("Id", typeof(byte[]))
                    {
                        IsTimestamp = true
                    };
            var column1 = new Column("P", typeof(string));
            var table = new Table("A", new[] { column0, column1 });
            database.AddTable(table);

            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(byte[])), true);

            var operations = PreProcess(database, alterColumnOperation);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropColumnOperation>(operations[0]);
            Assert.IsType<AddColumnOperation>(operations[1]);

            var dropColumnOperation = (DropColumnOperation)operations[0];
            var addColumnOperation = (AddColumnOperation)operations[1];

            Assert.Equal("Id", dropColumnOperation.ColumnName);
            Assert.Equal("Id", addColumnOperation.Column.Name);
            Assert.False(addColumnOperation.Column.IsTimestamp);
        }

        [Fact]
        public void Visit_with_alter_column_operation_resets_primary_key()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Key("Id");
                    });

            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(int)) { IsNullable = false },
                    true);

            var operations = PreProcess(modelBuilder, alterColumnOperation);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[2]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[2];

            Assert.Equal("PK_A", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("PK_A", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "Id" }, addPrimaryKeyOperation.ColumnNames);
        }

        [Fact]
        public void Visit_with_alter_column_operation_resets_unique_constraints_on_column()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        var p = b.Property<string>("P").Metadata;
                        b.Key("Id");
                        b.Metadata.AddKey(p);
                    });

            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("P", typeof(int)), true);

            var operations = PreProcess(modelBuilder, alterColumnOperation);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<AddUniqueConstraintOperation>(operations[2]);

            var dropUniqueConstraintOperation = (DropUniqueConstraintOperation)operations[0];
            var addUniqueConstraintOperation = (AddUniqueConstraintOperation)operations[2];

            Assert.Equal("UC_A_P", dropUniqueConstraintOperation.UniqueConstraintName);
            Assert.Equal("UC_A_P", addUniqueConstraintOperation.UniqueConstraintName);
            Assert.Equal(new[] { "P" }, addUniqueConstraintOperation.ColumnNames);
        }

        [Fact]
        public void Visit_with_alter_column_operation_resets_foreign_keys_on_the_column()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Key("Id");
                    });
            modelBuilder.Entity("B",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "P");
                    });

            var alterColumnOperation
                = new AlterColumnOperation(
                    "B",
                    new Column("P", typeof(int)), true);

            var operations = PreProcess(modelBuilder, alterColumnOperation);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<AddForeignKeyOperation>(operations[2]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[2];

            Assert.Equal("FK_B_A_P", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("FK_B_A_P", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal(new[] { "P" }, addForeignKeyOperation.ColumnNames);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ReferencedColumnNames);
        }

        [Fact]
        public void Visit_with_alter_column_operation_resets_foreign_keys_referencing_the_column()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Key("Id");
                    });
            modelBuilder.Entity("B",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "P");
                    });

            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(int)) { IsNullable = false }, true);

            var operations = PreProcess(modelBuilder, alterColumnOperation);

            Assert.Equal(5, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<DropPrimaryKeyOperation>(operations[1]);
            Assert.Same(alterColumnOperation, operations[2]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[3]);
            Assert.IsType<AddForeignKeyOperation>(operations[4]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[4];

            Assert.Equal("FK_B_A_P", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("FK_B_A_P", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal(new[] { "P" }, addForeignKeyOperation.ColumnNames);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ReferencedColumnNames);
        }

        [Fact]
        public void Visit_with_alter_column_operation_resets_indexes()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.Index("P");
                    });

            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("P", typeof(int)), true);

            var operations = PreProcess(modelBuilder, alterColumnOperation);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<CreateIndexOperation>(operations[2]);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            var createIndexOperation = (CreateIndexOperation)operations[2];

            Assert.Equal("IX_A_P", dropIndexOperation.IndexName);
            Assert.Equal("IX_A_P", createIndexOperation.IndexName);
            Assert.Equal(new[] { "P" }, createIndexOperation.ColumnNames);
        }

        [Fact]
        public void Visit_with_alter_column_does_not_reset_indexes_if_same_type_but_smaller_max_length()
        {
            var database = new DatabaseModel();
            var column
                = new Column("Id", typeof(string))
                    {
                        MaxLength = 10
                    };
            var table = new Table("A", new[] { column });
            var index = new Index("IX", new[] { column });
            database.AddTable(table);
            table.AddIndex(index);

            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(string))
                        {
                            MaxLength = 9
                        }, true);

            var operations = PreProcess(database, alterColumnOperation);

            Assert.Equal(1, operations.Count);

            Assert.Same(alterColumnOperation, operations[0]);
        }

        [Fact]
        public void Visit_with_alter_column_operation_drops_default_constraint()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ForSqlServer().DefaultExpression("abc");
                        b.Key("Id");
                    });

            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("P", typeof(int)), true);

            var operations = PreProcess(modelBuilder, alterColumnOperation);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropDefaultConstraintOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);

            var dropDefaultConstraintOperation = (DropDefaultConstraintOperation)operations[0];

            Assert.Same("P", dropDefaultConstraintOperation.ColumnName);
        }

        [Fact]
        public void Visit_with_consecutive_alter_column_operations()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A", b =>
                {
                    b.Property<string>("Id");
                    b.Key("Id");
                    b.Index("Id").ForSqlServer().Clustered(false);
                });
            modelBuilder.Entity("B", b =>
                {
                    b.Property<string>("Id");
                    b.Key("Id");
                    b.ForeignKey("A", "Id");
                    b.Index("Id").ForSqlServer().Clustered(false);
                });
            modelBuilder.Entity("A", b => b.ForeignKey("B", "Id"));

            var alterColumnOperation0
                = new AlterColumnOperation(
                    "B",
                    new Column("Id", typeof(int)) { IsNullable = false }, true);
            var alterColumnOperation1
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(int)) { IsNullable = false }, true);

            var operations = PreProcess(modelBuilder, alterColumnOperation0, alterColumnOperation1);

            Assert.Equal(14, operations.Count);

            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.IsType<DropIndexOperation>(operations[1]);
            Assert.IsType<DropForeignKeyOperation>(operations[2]);
            Assert.IsType<DropForeignKeyOperation>(operations[3]);
            Assert.IsType<DropPrimaryKeyOperation>(operations[4]);
            Assert.IsType<DropPrimaryKeyOperation>(operations[5]);
            Assert.Same(alterColumnOperation0, operations[6]);
            Assert.Same(alterColumnOperation1, operations[7]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[8]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[9]);
            Assert.IsType<AddForeignKeyOperation>(operations[10]);
            Assert.IsType<AddForeignKeyOperation>(operations[11]);
            Assert.IsType<CreateIndexOperation>(operations[12]);
            Assert.IsType<CreateIndexOperation>(operations[13]);

            var dropIndexOperation0 = (DropIndexOperation)operations[0];
            var dropIndexOperation1 = (DropIndexOperation)operations[1];
            var dropForeignKeyOperation0 = (DropForeignKeyOperation)operations[2];
            var dropForeignKeyOperation1 = (DropForeignKeyOperation)operations[3];
            var dropPrimaryKeyOperation0 = (DropPrimaryKeyOperation)operations[4];
            var dropPrimaryKeyOperation1 = (DropPrimaryKeyOperation)operations[5];
            var addPrimaryKeyOperation0 = (AddPrimaryKeyOperation)operations[8];
            var addPrimaryKeyOperation1 = (AddPrimaryKeyOperation)operations[9];
            var addForeignKeyOperation0 = (AddForeignKeyOperation)operations[10];
            var addForeignKeyOperation1 = (AddForeignKeyOperation)operations[11];
            var createIndexOperation0 = (CreateIndexOperation)operations[12];
            var createIndexOperation1 = (CreateIndexOperation)operations[13];

            Assert.Equal("IX_B_Id", dropIndexOperation0.IndexName);
            Assert.Equal("IX_A_Id", dropIndexOperation1.IndexName);
            Assert.Equal("FK_B_A_Id", dropForeignKeyOperation0.ForeignKeyName);
            Assert.Equal("FK_A_B_Id", dropForeignKeyOperation1.ForeignKeyName);
            Assert.Equal("PK_B", dropPrimaryKeyOperation0.PrimaryKeyName);
            Assert.Equal("PK_A", dropPrimaryKeyOperation1.PrimaryKeyName);
            Assert.Equal("PK_B", addPrimaryKeyOperation0.PrimaryKeyName);
            Assert.Equal(new[] { "Id" }, addPrimaryKeyOperation0.ColumnNames);
            Assert.Equal("PK_A", addPrimaryKeyOperation1.PrimaryKeyName);
            Assert.Equal(new[] { "Id" }, addPrimaryKeyOperation1.ColumnNames);
            Assert.Equal("FK_B_A_Id", addForeignKeyOperation0.ForeignKeyName);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation0.ColumnNames);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation0.ReferencedColumnNames);
            Assert.Equal("FK_A_B_Id", addForeignKeyOperation1.ForeignKeyName);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation1.ColumnNames);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation1.ReferencedColumnNames);
            Assert.Equal("IX_B_Id", createIndexOperation0.IndexName);
            Assert.Equal(new[] { "Id" }, createIndexOperation0.ColumnNames);
            Assert.Equal("IX_A_Id", createIndexOperation1.IndexName);
            Assert.Equal(new[] { "Id" }, createIndexOperation1.ColumnNames);
        }

        [Fact]
        public void Visit_with_drop_column_operation_drops_default_constraint()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ForSqlServer().DefaultExpression("abc");
                        b.Key("Id");
                    });

            var dropColumnOperation = new DropColumnOperation("A", "P");

            var operations = PreProcess(modelBuilder, dropColumnOperation);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropDefaultConstraintOperation>(operations[0]);
            Assert.IsType<DropColumnOperation>(operations[1]);
        }

        [Fact]
        public void Visit_with_drop_table_operation_drops_foreign_keys_referencing_the_table()
        {
            var modelBuilder = new BasicModelBuilder();
            modelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });
            modelBuilder.Entity("B",
                b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });
            modelBuilder.Entity("C",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<int>("P1");
                    b.Property<int>("P2");
                    b.Key("Id");
                    b.ForeignKey("A", "P1").ForRelational().Name("FKA");
                    b.ForeignKey("B", "P2").ForRelational().Name("FKB");
                });

            var dropTableOperation = new DropTableOperation("A");

            var operations = PreProcess(modelBuilder, dropTableOperation);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.IsType<DropTableOperation>(operations[1]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];

            Assert.Equal("C", dropForeignKeyOperation.TableName);
            Assert.Equal("FKA", dropForeignKeyOperation.ForeignKeyName);
            Assert.Same(dropTableOperation, operations[1]);
        }

        private static IReadOnlyList<MigrationOperation> PreProcess(BasicModelBuilder modelBuilder, params MigrationOperation[] operations)
        {
            return PreProcess(new SqlServerDatabaseBuilder(new SqlServerTypeMapper()).GetDatabase(modelBuilder.Model), operations);
        }

        private static IReadOnlyList<MigrationOperation> PreProcess(DatabaseModel database, params MigrationOperation[] operations)
        {
            var context = new SqlServerMigrationOperationPreProcessor.Context(
                new SqlServerMigrationOperationSqlGeneratorFactory().Create(database));

            foreach (var operation in operations)
            {
                operation.Accept(new SqlServerMigrationOperationPreProcessor(), context);
            }

            return context.CompositeOperation.Operations.ToArray();
        }
    }
}
