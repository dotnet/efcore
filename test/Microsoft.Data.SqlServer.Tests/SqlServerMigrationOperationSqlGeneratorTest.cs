// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.SqlServer.Tests
{
    public class SqlServerMigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_when_create_database_operation()
        {
            Assert.Equal(
@"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MyDatabase')
    CREATE DATABASE ""MyDatabase""",
                SqlServerMigrationOperationSqlGenerator.Generate(new CreateDatabaseOperation("MyDatabase")));
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.databases WHERE name = 'MyDatabase')
    DROP DATABASE ""MyDatabase""",
                SqlServerMigrationOperationSqlGenerator.Generate(new DropDatabaseOperation("MyDatabase")));
        }

        [Fact]
        public void Generate_when_create_sequence_operation_and_idempotent()
        {
            Assert.Equal(
@"IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'MySequence' AND schema_id = SCHEMA_ID(N'dbo'))
    CREATE SEQUENCE ""dbo"".""MySequence"" AS BIGINT START WITH 0 INCREMENT BY 1",
                SqlServerMigrationOperationSqlGenerator.Generate(new CreateSequenceOperation(new Sequence("dbo.MySequence"))));
        }

        [Fact]
        public void Generate_when_drop_sequence_operation_and_idempotent()
        {
            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.sequences WHERE name = 'MySequence' AND schema_id = SCHEMA_ID(N'dbo'))
    DROP SEQUENCE ""dbo"".""MySequence""",
                SqlServerMigrationOperationSqlGenerator.Generate(new DropSequenceOperation("dbo.MySequence")));
        }

        [Fact]
        public void Generate_when_create_table_operation()
        {
            Column foo, bar;
            var table = new Table("dbo.MyTable",
                new[]
                {
                    foo = new Column("Foo", "int") { IsNullable = false, DefaultValue = "5" },
                    bar = new Column("Bar", "int") { IsNullable = true }
                })
            {
                PrimaryKey = new PrimaryKey("MyPK", new[] { foo, bar })
            };

            Assert.Equal(
@"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MyTable' AND schema_id = SCHEMA_ID(N'dbo'))
    CREATE TABLE ""dbo"".""MyTable"" (
        ""Foo"" int NOT NULL DEFAULT 5,
        ""Bar"" int
        CONSTRAINT ""MyPK"" PRIMARY KEY NONCLUSTERED (""Foo"", ""Bar"")
    )",
                SqlServerMigrationOperationSqlGenerator.Generate(new CreateTableOperation(table)));
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MyTable' AND schema_id = SCHEMA_ID(N'dbo'))
    DROP TABLE ""dbo"".""MyTable""",
                SqlServerMigrationOperationSqlGenerator.Generate(new DropTableOperation("dbo.MyTable")));
        }

        [Fact]
        public void Generate_when_rename_table_operation()
        {

            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MyTable' AND schema_id = SCHEMA_ID(N'dbo'))
    EXECUTE sp_rename @objname = N'dbo.MyTable', @newname = N'MyTable2', @objtype = N'OBJECT'",
                SqlServerMigrationOperationSqlGenerator.Generate(new RenameTableOperation("dbo.MyTable", "MyTable2")));
        }

        [Fact]
        public void Generate_when_move_table_operation()
        {
            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MyTable' AND schema_id = SCHEMA_ID(N'dbo'))
    ALTER SCHEMA ""dbo2"" TRANSFER ""dbo"".""MyTable""",
                SqlServerMigrationOperationSqlGenerator.Generate(new MoveTableOperation("dbo.MyTable", "dbo2")));
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var column = new Column("Bar", "int") { IsNullable = false, DefaultValue = "5" };

            Assert.Equal(
@"IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'Bar' AND object_id = OBJECT_ID(N'dbo.MyTable'))
    ALTER TABLE ""dbo"".""MyTable"" ADD ""Bar"" int NOT NULL DEFAULT 5",
                SqlServerMigrationOperationSqlGenerator.Generate(new AddColumnOperation("dbo.MyTable", column)));
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.columns WHERE name = 'Foo' AND object_id = OBJECT_ID(N'dbo.MyTable'))
    ALTER TABLE ""dbo"".""MyTable"" DROP COLUMN ""Foo""",
                SqlServerMigrationOperationSqlGenerator.Generate(new DropColumnOperation("dbo.MyTable", "Foo")));
        }

        [Fact]
        public void Generate_when_rename_column_operation()
        {
            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.columns WHERE name = 'Foo' AND object_id = OBJECT_ID(N'dbo.MyTable'))
    EXECUTE sp_rename @objname = N'dbo.MyTable.Foo', @newname = N'Foo2', @objtype = N'COLUMN'",
                SqlServerMigrationOperationSqlGenerator.Generate(
                    new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2")));
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            Column foo, bar;
            var table = new Table("dbo.MyTable",
                new[]
                {
                    foo = new Column("Foo", "int"),
                    bar = new Column("Bar", "int")
                });
            var primaryKey = new PrimaryKey("MyPK", new[] { foo, bar });

            Assert.Equal(
@"IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = OBJECT_ID(N'dbo.MyTable'))
    ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyPK"" PRIMARY KEY NONCLUSTERED (""Foo"", ""Bar"")",
                SqlServerMigrationOperationSqlGenerator.Generate(new AddPrimaryKeyOperation("dbo.MyTable", primaryKey)));
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.key_constraints WHERE type = 'PK' AND parent_object_id = OBJECT_ID(N'dbo.MyTable') AND name = 'MyPK')
    ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyPK""",
                SqlServerMigrationOperationSqlGenerator.Generate(new DropPrimaryKeyOperation("dbo.MyTable", "MyPK")));
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            Assert.Equal(
@"IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.MyTable2') AND name = 'MyFK')
    ALTER TABLE ""dbo"".""MyTable2"" ADD CONSTRAINT ""MyFK"" FOREIGN KEY (""Foo2"", ""Bar2"") REFERENCES ""dbo"".""MyTable"" (""Foo"", ""Bar"") ON DELETE CASCADE",
                SqlServerMigrationOperationSqlGenerator.Generate(
                    new AddForeignKeyOperation("MyFK", "dbo.MyTable", "dbo.MyTable2",
                        new[] { "Foo", "Bar" }, new[] { "Foo2", "Bar2" }, cascadeDelete: true)));
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            Assert.Equal(
@"IF EXISTS (SELECT * FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID(N'dbo.MyTable2') AND name = 'MyFK')
    ALTER TABLE ""dbo"".""MyTable2"" DROP CONSTRAINT ""MyFK""",
                SqlServerMigrationOperationSqlGenerator.Generate(new DropForeignKeyOperation("dbo.MyTable2", "MyFK")));
        }

        [Fact]
        public void Delimit_identifier()
        {
            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator();

            Assert.Equal("\"foo\"\"bar\"", sqlGenerator.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void Escape_identifier()
        {
            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator();

            Assert.Equal("foo\"\"bar", sqlGenerator.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_literal()
        {
            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator();

            Assert.Equal("'foo''bar'", sqlGenerator.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void Escape_literal()
        {
            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator();

            Assert.Equal("foo''bar", sqlGenerator.EscapeLiteral("foo'bar"));
        }
    }
}
