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

using System.Linq;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests.Builders
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

            builder.CreateSequence("dbo.MySequence", "bigint", 10, 5);

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<CreateSequenceOperation>(builder.Operations[0]);

            var operation = (CreateSequenceOperation)builder.Operations[0];

            Assert.Equal("dbo.MySequence", operation.Sequence.Name);
            Assert.Equal("bigint", operation.Sequence.DataType);
            Assert.Equal(10, operation.Sequence.StartWith);
            Assert.Equal(5, operation.Sequence.IncrementBy);
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

            Assert.Equal("dbo.MyTable", operation.Table.Name);
            Assert.Equal(2, operation.Table.Columns.Count);
            Assert.Equal(new[] { "Foo", "Bar" }, operation.Table.Columns.Select(c => c.Name));
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
        public void AddDefaultConstraint_adds_operation()
        {
            var builder = new MigrationBuilder();

            builder.AddDefaultConstraint("dbo.MyTable", "Foo", DefaultConstraint.Value(5));

            Assert.Equal(1, builder.Operations.Count);
            Assert.IsType<AddDefaultConstraintOperation>(builder.Operations[0]);

            var operation = (AddDefaultConstraintOperation)builder.Operations[0];

            Assert.Equal("dbo.MyTable", operation.TableName);
            Assert.Equal("Foo", operation.ColumnName);
            Assert.Equal(5, operation.DefaultValue);
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

        private class MyMigrationOperation : MigrationOperation
        {
            public override void GenerateSql(
                MigrationOperationSqlGenerator generator,
                IndentedStringBuilder stringBuilder,
                bool generateIdempotentSql)
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
