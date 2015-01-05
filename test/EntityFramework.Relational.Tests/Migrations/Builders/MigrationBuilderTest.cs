// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Migrations.Builders
{
    public class MigrationBuilderTest
    {
        [Fact]
        public void Migration_operation_can_be_added_and_retrieved()
        {
            var builder = new MigrationBuilder();
            var operation = new MyMigrationOperation();

            Assert.Equal(0, builder.Operations.Count);

            builder.AddOperation(operation);

            Assert.Equal(1, builder.Operations.Count);
            Assert.Same(operation, builder.Operations[0]);
        }

        [Fact]
        public void CreateDatabase_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.CreateDatabase("MyDb");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<CreateDatabaseOperation>(builder.Operations[0]);

            var operation = (CreateDatabaseOperation)builder.Operations[0];

            Assert.Equal("MyDb", operation.DatabaseName);
        }

        [Fact]
        public void DropDatabase_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropDatabase("MyDb");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropDatabaseOperation>(builder.Operations[0]);

            var operation = (DropDatabaseOperation)builder.Operations[0];

            Assert.Equal("MyDb", operation.DatabaseName);
        }

        [Fact]
        public void CreateSequence_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.CreateSequence("dbo.MySequence", 13, 7, 3, 103, typeof(int));

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<CreateSequenceOperation>(builder.Operations[0]);

            var operation = (CreateSequenceOperation)builder.Operations[0];

            Assert.Equal("dbo.MySequence", operation.SequenceName);
            Assert.Equal(13, operation.StartValue);
            Assert.Equal(7, operation.IncrementBy);
            Assert.Equal(3, operation.MinValue);
            Assert.Equal(103, operation.MaxValue);
            Assert.Equal(typeof(int), operation.Type);
        }

        [Fact]
        public void DropSequence_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropSequence("dbo.MySequence");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropSequenceOperation>(builder.Operations[0]);

            var operation = (DropSequenceOperation)builder.Operations[0];

            Assert.Equal("dbo.MySequence", operation.SequenceName);
        }

        [Fact]
        public void MoveSequence_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.MoveSequence("dbo.MySequence", "RenamedSchema");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<MoveSequenceOperation>(builder.Operations[0]);

            var operation = (MoveSequenceOperation)builder.Operations[0];

            Assert.Equal("dbo.MySequence", operation.SequenceName);
            Assert.Equal("RenamedSchema", operation.NewSchema);
        }

        [Fact]
        public void RenameSequence_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.RenameSequence("dbo.MySequence", "RenamedSequence");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<RenameSequenceOperation>(builder.Operations[0]);

            var operation = (RenameSequenceOperation)builder.Operations[0];

            Assert.Equal("dbo.MySequence", operation.SequenceName);
            Assert.Equal("RenamedSequence", operation.NewSequenceName);
        }

        [Fact]
        public void AlterSequence_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AlterSequence("dbo.MySequence", 7);

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AlterSequenceOperation>(builder.Operations[0]);

            var operation = (AlterSequenceOperation)builder.Operations[0];

            Assert.Equal("dbo.MySequence", operation.SequenceName);
            Assert.Equal(7, operation.NewIncrementBy);
        }

        [Fact]
        public void CreateTable_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.CreateTable(
                "dbo.MyTable",
                c => new
                    {
                        Foo = c.Int(),
                        Bar = c.Int()
                    });

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<CreateTableOperation>(builder.Operations[0]);

            var operation = (CreateTableOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal(2, operation.Columns.Count);
            Assert.Equal(new[] { "Foo", "Bar" }, operation.Columns.Select(c => c.Name));
        }

        [Fact]
        public void DropTable_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropTable("dbo.MyTable");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropTableOperation>(builder.Operations[0]);

            var operation = (DropTableOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
        }

        [Fact]
        public void RenameTable_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.RenameTable("dbo.MyTable", "MyTable2");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<RenameTableOperation>(builder.Operations[0]);

            var operation = (RenameTableOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyTable2", operation.NewTableName);
        }

        [Fact]
        public void MoveTable_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.MoveTable("dbo.MyTable", "dbo2");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<MoveTableOperation>(builder.Operations[0]);

            var operation = (MoveTableOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("dbo2", operation.NewSchema);
        }

        [Fact]
        public void AddColumn_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AddColumn("dbo.MyTable", "Foo", c => c.Int());

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AddColumnOperation>(builder.Operations[0]);

            var operation = (AddColumnOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("Foo", operation.Column.Name);
            Assert.Equal(typeof(int), operation.Column.ClrType);
        }

        [Fact]
        public void DropColumn_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropColumn("dbo.MyTable", "Foo");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropColumnOperation>(builder.Operations[0]);

            var operation = (DropColumnOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("Foo", operation.ColumnName);
        }

        [Fact]
        public void RenameColumn_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.RenameColumn("dbo.MyTable", "Foo", "Bar");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<RenameColumnOperation>(builder.Operations[0]);

            var operation = (RenameColumnOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("Foo", operation.ColumnName);
            Assert.Equal("Bar", operation.NewColumnName);
        }

        [Fact]
        public void AlterColumn_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AlterColumn("dbo.MyTable", "Foo", c => c.Int());

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AlterColumnOperation>(builder.Operations[0]);

            var operation = (AlterColumnOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("Foo", operation.NewColumn.Name);
            Assert.Equal(typeof(int), operation.NewColumn.ClrType);
        }

        [Fact]
        public void AddDefaultValue_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AddDefaultValue("dbo.MyTable", "Foo", 5);

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AddDefaultConstraintOperation>(builder.Operations[0]);

            var operation = (AddDefaultConstraintOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("Foo", operation.ColumnName);
            Assert.Equal(5, operation.DefaultValue);
            Assert.Null(operation.DefaultSql);
        }

        [Fact]
        public void AddDefaultExpression_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AddDefaultExpression("dbo.MyTable", "Foo", "SqlExpression");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AddDefaultConstraintOperation>(builder.Operations[0]);

            var operation = (AddDefaultConstraintOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("Foo", operation.ColumnName);
            Assert.Null(operation.DefaultValue);
            Assert.Equal("SqlExpression", operation.DefaultSql);
        }

        [Fact]
        public void DropDefaultConstraint_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropDefaultConstraint("dbo.MyTable", "Foo");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropDefaultConstraintOperation>(builder.Operations[0]);

            var operation = (DropDefaultConstraintOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("Foo", operation.ColumnName);
        }

        [Fact]
        public void AddPrimaryKey_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AddPrimaryKey("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: true);

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AddPrimaryKeyOperation>(builder.Operations[0]);

            var operation = (AddPrimaryKeyOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyPK", operation.PrimaryKeyName);
            Assert.Equal(new[] { "Foo", "Bar" }, operation.ColumnNames);
            Assert.True(operation.IsClustered);
        }

        [Fact]
        public void DropPrimaryKey_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropPrimaryKey("dbo.MyTable", "MyPK");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropPrimaryKeyOperation>(builder.Operations[0]);

            var operation = (DropPrimaryKeyOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyPK", operation.PrimaryKeyName);
        }

        [Fact]
        public void AddUniqueConstraint_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AddUniqueConstraint("dbo.MyTable", "MyUC", new[] { "Foo", "Bar" });

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AddUniqueConstraintOperation>(builder.Operations[0]);

            var operation = (AddUniqueConstraintOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyUC", operation.UniqueConstraintName);
            Assert.Equal(new[] { "Foo", "Bar" }, operation.ColumnNames);
        }

        [Fact]
        public void DropUniqueConstraint_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropUniqueConstraint("dbo.MyTable", "MyUC");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropUniqueConstraintOperation>(builder.Operations[0]);

            var operation = (DropUniqueConstraintOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyUC", operation.UniqueConstraintName);
        }

        [Fact]
        public void AddForeignKey_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AddForeignKey("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: true);

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AddForeignKeyOperation>(builder.Operations[0]);

            var operation = (AddForeignKeyOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyFK", operation.ForeignKeyName);
            Assert.Equal(new[] { "Foo", "Bar" }, operation.ColumnNames);
            Assert.Equal("dbo.MyTable2", operation.ReferencedTableName);
            Assert.Equal(new[] { "Foo2", "Bar2" }, operation.ReferencedColumnNames);
            Assert.True(operation.CascadeDelete);
        }

        [Fact]
        public void DropForeignKey_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropForeignKey("dbo.MyTable", "MyFK");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropForeignKeyOperation>(builder.Operations[0]);

            var operation = (DropForeignKeyOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyFK", operation.ForeignKeyName);
        }

        [Fact]
        public void CreateIndex_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.CreateIndex("dbo.MyTable", "MyIdx", new[] { "Foo", "Bar" },
                isUnique: true, isClustered: true);

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<CreateIndexOperation>(builder.Operations[0]);

            var operation = (CreateIndexOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyIdx", operation.IndexName);
            Assert.Equal(new[] { "Foo", "Bar" }, operation.ColumnNames);
            Assert.True(operation.IsUnique);
            Assert.True(operation.IsClustered);
        }

        [Fact]
        public void DropIndex_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.DropIndex("dbo.MyTable", "MyIdx");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<DropIndexOperation>(builder.Operations[0]);

            var operation = (DropIndexOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyIdx", operation.IndexName);
        }

        [Fact]
        public void RenameIndex_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.RenameIndex("dbo.MyTable", "MyIdx", "MyIdx2");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<RenameIndexOperation>(builder.Operations[0]);

            var operation = (RenameIndexOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("MyIdx", operation.IndexName);
            Assert.Equal("MyIdx2", operation.NewIndexName);
        }

        [Fact]
        public void CopyData_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.CopyData("dbo.T1", new[] { "C1, C2" }, "dbo.T2", new[] { "C1, C3" });

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<CopyDataOperation>(builder.Operations[0]);

            var operation = (CopyDataOperation)builder.Operations[0];

            Assert.Equal("dbo.T1", operation.SourceTableName);
            Assert.Equal(new[] { "C1, C2" }, operation.SourceColumnNames);
            Assert.Equal("dbo.T2", operation.TargetTableName);
            Assert.Equal(new[] { "C1, C3" }, operation.TargetColumnNames);
        }

        [Fact]
        public void Sql_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.Sql("MySql");

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<SqlOperation>(builder.Operations[0]);

            var operation = (SqlOperation)builder.Operations[0];

            Assert.Equal("MySql", operation.Sql);
            Assert.False(operation.SuppressTransaction);
        }

        [Fact]
        public void Sql_adds_operation_with_suppress_transaction_true()
        {
            var builder = new MigrationBuilder();

            builder.Sql("MySql", suppressTransaction: true);

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<SqlOperation>(builder.Operations[0]);

            var operation = (SqlOperation)builder.Operations[0];

            Assert.Equal("MySql", operation.Sql);
            Assert.True(operation.SuppressTransaction);
        }

        private class MyMigrationOperation : MigrationOperation
        {
            public override void Accept<TVisitor, TContext>(
                TVisitor visitor,
                TContext context)
            {
            }

            public override void GenerateSql(
                MigrationOperationSqlGenerator generator,
                SqlBatchBuilder batchBuilder)
            {
            }

            public override void GenerateCode(
                MigrationCodeGenerator generator,
                IndentedStringBuilder stringBuilder)
            {
            }
        }
    }
}
