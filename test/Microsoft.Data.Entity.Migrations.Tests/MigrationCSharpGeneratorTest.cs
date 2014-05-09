// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class MigrationCSharpGeneratorTest
    {
        [Fact]
        public void Generate_when_create_database_operation()
        {
            Assert.Equal(
                @"CreateDatabase(""MyDatabase"")",
                CSharpMigrationCodeGenerator.Generate(new CreateDatabaseOperation("MyDatabase")));
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            Assert.Equal(
                @"DropDatabase(""MyDatabase"")",
                CSharpMigrationCodeGenerator.Generate(new DropDatabaseOperation("MyDatabase")));
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            Assert.Equal(
                @"CreateSequence(""dbo.MySequence"", ""BIGINT"", 10, 5)",
                CSharpMigrationCodeGenerator.Generate(new CreateSequenceOperation(
                    new Sequence("dbo.MySequence", "BIGINT", 10, 5))));
        }

        [Fact]
        public void Generate_when_drop_sequence_operation()
        {
            Assert.Equal(
                @"DropSequence(""dbo.MySequence"")",
                CSharpMigrationCodeGenerator.Generate(new DropSequenceOperation("dbo.MySequence")));
        }

        [Fact]
        public void Generate_when_create_table_operation_without_primary_key()
        {
            var table = new Table("dbo.MyTable",
                new[]
                    {
                        new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 },
                        new Column("Bar", typeof(int))
                    });

            Assert.Equal(
                @"CreateTable(""dbo.MyTable"",
    c => new
        {
            Foo = c.Int(nullable: false, defaultValue: 5),
            Bar = c.Int()
        })",
                CSharpMigrationCodeGenerator.Generate(new CreateTableOperation(table)));
        }

        [Fact]
        public void Generate_when_create_table_operation_with_one_primary_key_columns()
        {
            Column foo;
            var table = new Table("dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 },
                        new Column("Bar", typeof(int))
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo })
                };

            Assert.Equal(
                @"CreateTable(""dbo.MyTable"",
    c => new
        {
            Foo = c.Int(nullable: false, defaultValue: 5),
            Bar = c.Int()
        })
    .PrimaryKey(""MyPK"", t => t.Foo)",
                CSharpMigrationCodeGenerator.Generate(new CreateTableOperation(table)));
        }

        [Fact]
        public void Generate_when_create_table_operation_with_multiple_primary_key_columns()
        {
            Column foo, bar;
            var table = new Table("dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 },
                        bar = new Column("Bar", typeof(int))
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo, bar })
                };

            Assert.Equal(
                @"CreateTable(""dbo.MyTable"",
    c => new
        {
            Foo = c.Int(nullable: false, defaultValue: 5),
            Bar = c.Int()
        })
    .PrimaryKey(""MyPK"",
        t => new
            {
                Foo => t.Foo,
                Bar => t.Bar
            })",
                CSharpMigrationCodeGenerator.Generate(new CreateTableOperation(table)));
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            Assert.Equal(
                @"DropTable(""dbo.MyTable"")",
                CSharpMigrationCodeGenerator.Generate(new DropTableOperation("dbo.MyTable")));
        }

        [Fact]
        public void Generate_when_rename_table_operation()
        {
            Assert.Equal(
                @"RenameTable(""dbo.MyTable"", ""MyTable2"")",
                CSharpMigrationCodeGenerator.Generate(new RenameTableOperation("dbo.MyTable", "MyTable2")));
        }

        [Fact]
        public void Generate_when_move_table_operation()
        {
            Assert.Equal(
                @"MoveTable(""dbo.MyTable"", ""dbo2"")",
                CSharpMigrationCodeGenerator.Generate(new MoveTableOperation("dbo.MyTable", "dbo2")));
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var column = new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = "5" };

            Assert.Equal(
                @"AddColumn(""dbo.MyTable"", ""Foo"", c => c.Int(nullable: false, defaultValue: ""5""))",
                CSharpMigrationCodeGenerator.Generate(new AddColumnOperation("dbo.MyTable", column)));
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            Assert.Equal(
                @"DropColumn(""dbo.MyTable"", ""Foo"")",
                CSharpMigrationCodeGenerator.Generate(new DropColumnOperation("dbo.MyTable", "Foo")));
        }

        [Fact]
        public void Generate_when_rename_column_operation()
        {
            Assert.Equal(
                @"RenameColumn(""dbo.MyTable"", ""Foo"", ""Foo2"")",
                CSharpMigrationCodeGenerator.Generate(new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2")));
        }

        [Fact]
        public void Generate_when_alter_column_operation()
        {
            var newColumn = new Column("Foo", typeof(int)) { IsNullable = false, DefaultValue = 5 };

            Assert.Equal(
                @"AlterColumn(""dbo.MyTable"", ""Foo"", c => c.Int(nullable: false, defaultValue: 5))",
                CSharpMigrationCodeGenerator.Generate(new AlterColumnOperation("dbo.MyTable", newColumn, isDestructiveChange: true)));
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_value()
        {
            Assert.Equal(
                @"AddDefaultConstraint(""dbo.MyTable"", ""Foo"", DefaultConstraint.Value(5))",
                CSharpMigrationCodeGenerator.Generate(new AddDefaultConstraintOperation(
                    "dbo.MyTable", "Foo", 5, null)));
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_sql()
        {
            Assert.Equal(
                @"AddDefaultConstraint(""dbo.MyTable"", ""Foo"", DefaultConstraint.Sql(""GETDATE()""))",
                CSharpMigrationCodeGenerator.Generate(new AddDefaultConstraintOperation(
                    "dbo.MyTable", "Foo", null, "GETDATE()")));
        }

        [Fact]
        public void Generate_when_drop_default_constraint_operation()
        {
            Assert.Equal(
                @"DropDefaultConstraint(""dbo.MyTable"", ""Foo"")",
                CSharpMigrationCodeGenerator.Generate(new DropDefaultConstraintOperation("dbo.MyTable", "Foo")));
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            Assert.Equal(
                @"AddPrimaryKey(""dbo.MyTable"", ""MyPK"", new[] { ""Foo"", ""Bar"" }, isClustered: false)",
                CSharpMigrationCodeGenerator.Generate(new AddPrimaryKeyOperation("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: false)));
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            Assert.Equal(
                @"DropPrimaryKey(""dbo.MyTable"", ""MyPK"")",
                CSharpMigrationCodeGenerator.Generate(new DropPrimaryKeyOperation("dbo.MyTable", "MyPK")));
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            Assert.Equal(
                @"AddForeignKey(""dbo.MyTable"", ""MyFK"", new[] { ""Foo"", ""Bar"" }, ""dbo.MyTable2"", new[] { ""Foo2"", ""Bar2"" }, cascadeDelete: false)",
                CSharpMigrationCodeGenerator.Generate(
                    new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                        "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: false)));
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            Assert.Equal(
                @"DropForeignKey(""dbo.MyTable"", ""MyFK"")",
                CSharpMigrationCodeGenerator.Generate(new DropForeignKeyOperation("dbo.MyTable", "MyFK")));
        }

        [Fact]
        public void Generate_when_create_index_operation()
        {
            Assert.Equal(
                @"CreateIndex(""dbo.MyTable"", ""MyIdx"", new[] { ""Foo"", ""Bar"" }, isUnique: false, isClustered: false)",
                CSharpMigrationCodeGenerator.Generate(new CreateIndexOperation(
                    "dbo.MyTable", "MyIdx", new[] { "Foo", "Bar" }, isUnique: false, isClustered: false)));
        }

        [Fact]
        public void Generate_when_drop_index_key_operation()
        {
            Assert.Equal(
                @"DropIndex(""dbo.MyTable"", ""MyIdx"")",
                CSharpMigrationCodeGenerator.Generate(new DropIndexOperation("dbo.MyTable", "MyIdx")));
        }

        [Fact]
        public void Generate_when_rename_index_key_operation()
        {
            Assert.Equal(
                @"RenameIndex(""dbo.MyTable"", ""MyIdx"", ""MyNewIdx"")",
                CSharpMigrationCodeGenerator.Generate(new RenameIndexOperation("dbo.MyTable", "MyIdx", "MyNewIdx")));
        }

        // TODO: Add missing GenerateLiteral unit tests.

        [Fact]
        public void Generate_string_literal()
        {
            var csharpGenerator = new CSharpMigrationCodeGenerator();

            Assert.Equal("\"foo\\\"bar\"", csharpGenerator.GenerateLiteral("foo\"bar"));
        }

        [Fact]
        public void Escape_string()
        {
            var csharpGenerator = new CSharpMigrationCodeGenerator();

            Assert.Equal("foo\\\"bar", csharpGenerator.EscapeString("foo\"bar"));
        }

        [Fact]
        public void Generate_migration_class()
        {
            var upgradeOperations
                = new[]
                    {
                        new AddColumnOperation("dbo.MyTable", new Column("Foo", typeof(int))),
                        new AddColumnOperation("dbo.MyTable", new Column("Bar", typeof(int)))
                    };

            var downgradeOperations
                = new[]
                    {
                        new DropColumnOperation("dbo.MyTable", "Foo"),
                        new DropColumnOperation("dbo.MyTable", "Bar")
                    };

            var codeGenerator = new CSharpMigrationCodeGenerator();
            var stringBuilder = new IndentedStringBuilder();

            codeGenerator.GenerateClass("MyNamespace", "MyClass", upgradeOperations, downgradeOperations, stringBuilder);

            Assert.Equal(
                @"using System;
using Microsoft.Data.Migrations;
using Microsoft.Data.Migrations.Builders;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;

namespace MyNamespace
{
    public class MyClass : Migration
    {
        public override Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn(""dbo.MyTable"", ""Foo"", c => c.Int());
            migrationBuilder.AddColumn(""dbo.MyTable"", ""Bar"", c => c.Int());
        }
        public override Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(""dbo.MyTable"", ""Foo"");
            migrationBuilder.DropColumn(""dbo.MyTable"", ""Bar"");
        }
    }
}",
                stringBuilder.ToString());
        }
    }
}
