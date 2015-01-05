// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerMigrationOperationPreProcessorTest
    {
        [Fact]
        public void Process_with_alter_column_operation_and_timestamp_column()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<byte[]>("Id").ConcurrencyToken();
                        b.Property<string>("P");
                        b.Key("Id");
                    });

            var inOperations = new MigrationOperationCollection();
            inOperations.Add(
                new AlterColumnOperation(
                    "A", 
                    new Column("Id", typeof(byte[])), 
                    isDestructiveChange: true));

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(4, operations.Count);
            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.IsType<DropColumnOperation>(operations[1]);
            Assert.IsType<AddColumnOperation>(operations[2]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[3]);
            
            var dropColumnOperation = (DropColumnOperation)operations[1];
            var addColumnOperation = (AddColumnOperation)operations[2];

            Assert.Equal("Id", dropColumnOperation.ColumnName);
            Assert.Equal("Id", addColumnOperation.Column.Name);
            Assert.False(addColumnOperation.Column.IsTimestamp);
        }

        [Fact]
        public void Process_with_alter_column_operation_and_computed_source_column()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A").Property<int>("P").StoreComputed().ForSqlServer().DefaultExpression("1 + 2");

            var inOperations = new MigrationOperationCollection();
            inOperations.Add(
                new AlterColumnOperation(
                    "A",
                    new Column("P", typeof(int)) { IsComputed = false },
                    isDestructiveChange: true));

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(2, operations.Count);
            Assert.IsType<DropColumnOperation>(operations[0]);
            Assert.IsType<AddColumnOperation>(operations[1]);

            var dropColumnOperation = (DropColumnOperation)operations[0];
            var addColumnOperation = (AddColumnOperation)operations[1];

            Assert.Equal("P", dropColumnOperation.ColumnName);
            Assert.Equal("P", addColumnOperation.Column.Name);
            Assert.False(addColumnOperation.Column.IsComputed);
        }

        [Fact]
        public void Process_with_alter_column_operation_and_computed_target_column()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A").Property<int>("P").ForSqlServer().DefaultExpression("1 + 2");

            var inOperations = new MigrationOperationCollection();
            inOperations.Add(
                new AlterColumnOperation(
                    "A",
                    new Column("P", typeof(int)) { IsComputed = true },
                    isDestructiveChange: true));

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(3, operations.Count);
            Assert.IsType<DropDefaultConstraintOperation>(operations[0]);
            Assert.IsType<DropColumnOperation>(operations[1]);
            Assert.IsType<AddColumnOperation>(operations[2]);

            var dropColumnOperation = (DropColumnOperation)operations[1];
            var addColumnOperation = (AddColumnOperation)operations[2];

            Assert.Equal("P", dropColumnOperation.ColumnName);
            Assert.Equal("P", addColumnOperation.Column.Name);
            Assert.True(addColumnOperation.Column.IsComputed);
        }

        [Fact]
        public void Process_with_alter_column_operation_resets_primary_key()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Key("Id");
                    });


            var inOperations = new MigrationOperationCollection();
            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(int)) { IsNullable = false },
                    isDestructiveChange: false);

            inOperations.Add(alterColumnOperation);

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[2]);

            var dropPrimaryKeyOperation = (DropPrimaryKeyOperation)operations[0];
            var addPrimaryKeyOperation = (AddPrimaryKeyOperation)operations[2];

            Assert.Equal("PK_A", dropPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal("PK_A", addPrimaryKeyOperation.PrimaryKeyName);
            Assert.Equal(new[] { "Id" }, addPrimaryKeyOperation.ColumnNames.AsEnumerable());
        }

        [Fact]
        public void Process_with_alter_column_operation_resets_unique_constraints_on_column()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        var p = b.Property<string>("P").Metadata;
                        b.Key("Id");
                        b.Metadata.AddKey(p);
                    });

            var inOperations = new MigrationOperationCollection();
            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("P", typeof(int)), 
                    isDestructiveChange: false);

            inOperations.Add(alterColumnOperation);

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropUniqueConstraintOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<AddUniqueConstraintOperation>(operations[2]);

            var dropUniqueConstraintOperation = (DropUniqueConstraintOperation)operations[0];
            var addUniqueConstraintOperation = (AddUniqueConstraintOperation)operations[2];

            Assert.Equal("UC_A_P", dropUniqueConstraintOperation.UniqueConstraintName);
            Assert.Equal("UC_A_P", addUniqueConstraintOperation.UniqueConstraintName);
            Assert.Equal(new[] { "P" }, addUniqueConstraintOperation.ColumnNames.AsEnumerable());
        }

        [Fact]
        public void Process_with_alter_column_operation_resets_foreign_keys_on_the_column()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Key("Id");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "P");
                    });

            var inOperations = new MigrationOperationCollection();
            var alterColumnOperation
                = new AlterColumnOperation(
                    "B",
                    new Column("P", typeof(int)), 
                    isDestructiveChange: false);
            inOperations.Add(alterColumnOperation);

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<AddForeignKeyOperation>(operations[2]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];
            var addForeignKeyOperation = (AddForeignKeyOperation)operations[2];

            Assert.Equal("FK_B_A_P", dropForeignKeyOperation.ForeignKeyName);
            Assert.Equal("FK_B_A_P", addForeignKeyOperation.ForeignKeyName);
            Assert.Equal(new[] { "P" }, addForeignKeyOperation.ColumnNames.AsEnumerable());
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ReferencedColumnNames.AsEnumerable());
        }

        [Fact]
        public void Process_with_alter_column_operation_resets_foreign_keys_referencing_the_column()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Key("Id");
                    });
            sourceModelBuilder.Entity("B",
                b =>
                    {
                        b.Property<string>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.ForeignKey("A", "P");
                    });

            var inOperations = new MigrationOperationCollection();
            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(int)) { IsNullable = false }, 
                    isDestructiveChange: false);

            inOperations.Add(alterColumnOperation);

            var operations = Process(inOperations, sourceModel);

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
            Assert.Equal(new[] { "P" }, addForeignKeyOperation.ColumnNames.AsEnumerable());
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation.ReferencedColumnNames.AsEnumerable());
        }

        [Fact]
        public void Process_with_alter_column_operation_resets_indexes()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P");
                        b.Key("Id");
                        b.Index("P");
                    });

            var inOperations = new MigrationOperationCollection();
            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("P", typeof(int)), 
                    isDestructiveChange: false);

            inOperations.Add(alterColumnOperation);

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropIndexOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<CreateIndexOperation>(operations[2]);

            var dropIndexOperation = (DropIndexOperation)operations[0];
            var createIndexOperation = (CreateIndexOperation)operations[2];

            Assert.Equal("IX_A_P", dropIndexOperation.IndexName);
            Assert.Equal("IX_A_P", createIndexOperation.IndexName);
            Assert.Equal(new[] { "P" }, createIndexOperation.ColumnNames.AsEnumerable());
        }

        [Fact]
        public void Process_with_alter_column_does_not_reset_indexes_if_same_type_but_smaller_max_length()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                {
                    b.Property<string>("Id").MaxLength(10);
                    b.Key("Id");
                    b.Index("Id").ForSqlServer().Name("IX");
                });

            var inOperations = new MigrationOperationCollection();
            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(string)) { MaxLength = 9 }, 
                    isDestructiveChange: false);

            inOperations.Add(alterColumnOperation);

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(3, operations.Count);

            Assert.IsType<DropPrimaryKeyOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);
            Assert.IsType<AddPrimaryKeyOperation>(operations[2]);
        }

        [Fact]
        public void Process_with_alter_column_operation_drops_default_constraint()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ForSqlServer().DefaultExpression("abc");
                        b.Key("Id");
                    });

            var inOperations = new MigrationOperationCollection();
            var alterColumnOperation
                = new AlterColumnOperation(
                    "A",
                    new Column("P", typeof(int)),
                    isDestructiveChange: false);

            inOperations.Add(alterColumnOperation);

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropDefaultConstraintOperation>(operations[0]);
            Assert.Same(alterColumnOperation, operations[1]);

            var dropDefaultConstraintOperation = (DropDefaultConstraintOperation)operations[0];

            Assert.Same("P", dropDefaultConstraintOperation.ColumnName);
        }

        [Fact]
        public void Process_with_consecutive_alter_column_operations()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A", b =>
                {
                    b.Property<string>("Id");
                    b.Key("Id");
                    b.Index("Id").ForSqlServer().Clustered(false);
                });
            sourceModelBuilder.Entity("B", b =>
                {
                    b.Property<string>("Id");
                    b.Key("Id");
                    b.ForeignKey("A", "Id");
                    b.Index("Id").ForSqlServer().Clustered(false);
                });
            sourceModelBuilder.Entity("A", b => b.ForeignKey("B", "Id"));

            var inOperations = new MigrationOperationCollection();
            var alterColumnOperation0
                = new AlterColumnOperation(
                    "B",
                    new Column("Id", typeof(int)) { IsNullable = false }, 
                    isDestructiveChange: false);
            var alterColumnOperation1
                = new AlterColumnOperation(
                    "A",
                    new Column("Id", typeof(int)) { IsNullable = false }, 
                    isDestructiveChange: false);

            inOperations.Add(alterColumnOperation0);
            inOperations.Add(alterColumnOperation1);

            var operations = Process(inOperations, sourceModel);

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
            Assert.Equal(new[] { "Id" }, addPrimaryKeyOperation0.ColumnNames.AsEnumerable());
            Assert.Equal("PK_A", addPrimaryKeyOperation1.PrimaryKeyName);
            Assert.Equal(new[] { "Id" }, addPrimaryKeyOperation1.ColumnNames.AsEnumerable());
            Assert.Equal("FK_B_A_Id", addForeignKeyOperation0.ForeignKeyName);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation0.ColumnNames.AsEnumerable());
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation0.ReferencedColumnNames.AsEnumerable());
            Assert.Equal("FK_A_B_Id", addForeignKeyOperation1.ForeignKeyName);
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation1.ColumnNames.AsEnumerable());
            Assert.Equal(new[] { "Id" }, addForeignKeyOperation1.ReferencedColumnNames.AsEnumerable());
            Assert.Equal("IX_B_Id", createIndexOperation0.IndexName);
            Assert.Equal(new[] { "Id" }, createIndexOperation0.ColumnNames.AsEnumerable());
            Assert.Equal("IX_A_Id", createIndexOperation1.IndexName);
            Assert.Equal(new[] { "Id" }, createIndexOperation1.ColumnNames.AsEnumerable());
        }

        [Fact]
        public void Process_with_drop_column_operation_drops_default_constraint()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                    {
                        b.Property<int>("Id");
                        b.Property<string>("P").ForSqlServer().DefaultExpression("abc");
                        b.Key("Id");
                    });

            var inOperations = new MigrationOperationCollection();
            var dropColumnOperation = new DropColumnOperation("A", "P");

            inOperations.Add(dropColumnOperation);

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(2, operations.Count);

            Assert.IsType<DropDefaultConstraintOperation>(operations[0]);
            Assert.Same(dropColumnOperation, operations[1]);

            var dropDefaultConstraintOperation = (DropDefaultConstraintOperation)operations[0];

            Assert.Equal("P", dropDefaultConstraintOperation.ColumnName);
        }

        [Fact]
        public void Process_with_drop_table_operation_drops_foreign_keys_referencing_the_table()
        {
            var sourceModel = new Model();
            var sourceModelBuilder = new BasicModelBuilder(sourceModel);

            sourceModelBuilder.Entity("A",
                b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });
            sourceModelBuilder.Entity("B",
                b =>
                {
                    b.Property<int>("Id");
                    b.Key("Id");
                });
            sourceModelBuilder.Entity("C",
                b =>
                {
                    b.Property<int>("Id");
                    b.Property<int>("P1");
                    b.Property<int>("P2");
                    b.Key("Id");
                    b.ForeignKey("A", "P1").ForRelational().Name("FKA");
                    b.ForeignKey("B", "P2").ForRelational().Name("FKB");
                });

            var inOperations = new MigrationOperationCollection();
            var dropTableOperation = new DropTableOperation("A");

            inOperations.Add(dropTableOperation);

            var operations = Process(inOperations, sourceModel);

            Assert.Equal(2, operations.Count);
            Assert.IsType<DropForeignKeyOperation>(operations[0]);
            Assert.Same(dropTableOperation, operations[1]);

            var dropForeignKeyOperation = (DropForeignKeyOperation)operations[0];

            Assert.Equal("C", dropForeignKeyOperation.TableName);
            Assert.Equal("FKA", dropForeignKeyOperation.ForeignKeyName);
            Assert.Same(dropTableOperation, operations[1]);
        }

        private static IReadOnlyList<MigrationOperation> Process(
            MigrationOperationCollection operations, IModel sourceModel, IModel targetModel = null)
        {
            var extensionProvider = new SqlServerMetadataExtensionProvider();
            var typeMapper = new SqlServerTypeMapper();
            var operationFactory = new SqlServerMigrationOperationFactory(extensionProvider);
            var operationProcessor = new SqlServerMigrationOperationProcessor(
                extensionProvider, typeMapper, operationFactory);

            return operationProcessor.Process(operations, sourceModel, targetModel ?? new Model());
        }
    }
}
