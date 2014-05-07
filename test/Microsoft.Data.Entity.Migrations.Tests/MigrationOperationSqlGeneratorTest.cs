// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.Migrations.Tests
{
    public class MigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_when_create_database_operation()
        {
            Assert.Equal(
                @"CREATE DATABASE ""MyDatabase""",
                Generate(new CreateDatabaseOperation("MyDatabase"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            Assert.Equal(
                @"DROP DATABASE ""MyDatabase""",
                Generate(new DropDatabaseOperation("MyDatabase"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            Assert.Equal(
                @"CREATE SEQUENCE ""dbo"".""MySequence"" AS BIGINT START WITH 0 INCREMENT BY 1",
                Generate(
                    new CreateSequenceOperation(new Sequence("dbo.MySequence")), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_drop_sequence_operation()
        {
            Assert.Equal(
                @"DROP SEQUENCE ""dbo"".""MySequence""",
                Generate(new DropSequenceOperation("dbo.MySequence"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_create_table_operation()
        {
            Column foo, bar;
            var table = new Table(
                "dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", "int") { IsNullable = false, DefaultValue = 5 },
                        bar = new Column("Bar", "int") { IsNullable = true }
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo, bar }, isClustered: false)
                };

            Assert.Equal(
                @"CREATE TABLE ""dbo"".""MyTable"" (
    ""Foo"" int NOT NULL DEFAULT 5,
    ""Bar"" int,
    CONSTRAINT ""MyPK"" PRIMARY KEY NONCLUSTERED (""Foo"", ""Bar"")
)",
                Generate(
                    new CreateTableOperation(table), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_create_table_operation_with_Identity_key()
        {
            Column foo, bar;
            var table = new Table(
                "dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", "int") { IsNullable = false, ValueGenerationStrategy = StoreValueGenerationStrategy.Identity },
                        bar = new Column("Bar", "int") { IsNullable = true }
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo }, isClustered: false)
                };

            Assert.Equal(
                @"CREATE TABLE ""dbo"".""MyTable"" (
    ""Foo"" int NOT NULL IDENTITY,
    ""Bar"" int,
    CONSTRAINT ""MyPK"" PRIMARY KEY NONCLUSTERED (""Foo"")
)",
                Generate(
                    new CreateTableOperation(table), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            Assert.Equal(
                @"DROP TABLE ""dbo"".""MyTable""",
                Generate(new DropTableOperation("dbo.MyTable"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_rename_table_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable', @newname = N'MyTable2', @objtype = N'OBJECT'",
                Generate(
                    new RenameTableOperation("dbo.MyTable", "MyTable2"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_move_table_operation()
        {
            Assert.Equal(
                @"ALTER SCHEMA ""dbo2"" TRANSFER ""dbo"".""MyTable""",
                Generate(
                    new MoveTableOperation("dbo.MyTable", "dbo2"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var column = new Column("Bar", "int") { IsNullable = false, DefaultValue = 5 };

            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD ""Bar"" int NOT NULL DEFAULT 5",
                Generate(
                    new AddColumnOperation("dbo.MyTable", column), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP COLUMN ""Foo""",
                Generate(
                    new DropColumnOperation("dbo.MyTable", "Foo"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_alter_column_operation_with_nullable()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" int NULL",
                Generate(
                    new AlterColumnOperation("dbo.MyTable",
                        new Column("Foo", "int") { IsNullable = true }, isDestructiveChange: false),
                    generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_alter_column_operation_with_not_nullable()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" int NOT NULL",
                Generate(
                    new AlterColumnOperation("dbo.MyTable",
                        new Column("Foo", "int") { IsNullable = false }, isDestructiveChange: false),
                    generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_value()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" SET DEFAULT 'MyDefault'",
                Generate(
                    new AddDefaultConstraintOperation("dbo.MyTable", "Foo", "MyDefault", null), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_sql()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" SET DEFAULT GETDATE()",
                Generate(
                    new AddDefaultConstraintOperation("dbo.MyTable", "Foo", null, "GETDATE()"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_drop_default_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" DROP DEFAULT",
                Generate(
                    new DropDefaultConstraintOperation("dbo.MyTable", "Foo"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_rename_column_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable.Foo', @newname = N'Bar', @objtype = N'COLUMN'",
                Generate(
                    new RenameColumnOperation("dbo.MyTable", "Foo", "Bar"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyPK"" PRIMARY KEY NONCLUSTERED (""Foo"", ""Bar"")",
                Generate(
                    new AddPrimaryKeyOperation("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: false),
                    generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyPK""",
                Generate(new DropPrimaryKeyOperation("dbo.MyTable", "MyPK"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyFK"" FOREIGN KEY (""Foo"", ""Bar"") REFERENCES ""dbo"".""MyTable2"" (""Foo2"", ""Bar2"") ON DELETE CASCADE",
                Generate(
                    new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                        "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: true),
                    generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyFK""",
                Generate(new DropForeignKeyOperation("dbo.MyTable", "MyFK"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_create_index_operation()
        {
            Assert.Equal(
                @"CREATE UNIQUE CLUSTERED INDEX ""MyIndex"" ON ""dbo"".""MyTable"" (""Foo"", ""Bar"")",
                Generate(
                    new CreateIndexOperation("dbo.MyTable", "MyIndex", new[] { "Foo", "Bar" },
                        isUnique: true, isClustered: true),
                    generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_drop_index_operation()
        {
            Assert.Equal(
                @"DROP INDEX ""MyIndex"" ON ""dbo"".""MyTable""",
                Generate(new DropIndexOperation("dbo.MyTable", "MyIndex"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Generate_when_rename_index_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable.MyIndex', @newname = N'MyIndex2', @objtype = N'INDEX'",
                Generate(
                    new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2"), generateIdempotentSql: false).Sql);
        }

        [Fact]
        public void Delimit_identifier()
        {
            var sqlGenerator = new MigrationOperationSqlGenerator(new RelationalTypeMapper());

            Assert.Equal("\"foo\"\"bar\"", sqlGenerator.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_identifier_when_schema_qualified()
        {
            var sqlGenerator = new MigrationOperationSqlGenerator(new RelationalTypeMapper());

            Assert.Equal("\"foo\".\"bar\"", sqlGenerator.DelimitIdentifier(SchemaQualifiedName.Parse("foo.bar")));
        }

        [Fact]
        public void Escape_identifier()
        {
            var sqlGenerator = new MigrationOperationSqlGenerator(new RelationalTypeMapper());

            Assert.Equal("foo\"\"bar", sqlGenerator.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_literal()
        {
            var sqlGenerator = new MigrationOperationSqlGenerator(new RelationalTypeMapper());

            Assert.Equal("'foo''bar'", sqlGenerator.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void Escape_literal()
        {
            var sqlGenerator = new MigrationOperationSqlGenerator(new RelationalTypeMapper());

            Assert.Equal("foo''bar", sqlGenerator.EscapeLiteral("foo'bar"));
        }

        private static SqlStatement Generate(MigrationOperation migrationOperation, bool generateIdempotentSql)
        {
            var sqlGenerator = new MigrationOperationSqlGenerator(new RelationalTypeMapper());

            return sqlGenerator.Generate(new[] { migrationOperation }, generateIdempotentSql).Single();
        }
    }
}
