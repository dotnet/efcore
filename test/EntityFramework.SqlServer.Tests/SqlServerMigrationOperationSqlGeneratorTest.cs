// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class SqlServerMigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_when_create_database_operation()
        {
            Assert.Equal(
                @"CREATE DATABASE [MyDatabase]",
                Generate(new CreateDatabaseOperation("MyDatabase")).Sql);
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            Assert.Equal(
                @"DROP DATABASE [MyDatabase]",
                Generate(new DropDatabaseOperation("MyDatabase")).Sql);
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            Assert.Equal(
                @"CREATE SEQUENCE [dbo].[MySequence] AS bigint START WITH 0 INCREMENT BY 1",
                Generate(new CreateSequenceOperation(new Sequence("dbo.MySequence", typeof(long), 0, 1))).Sql);
        }

        [Fact]
        public void Generate_when_move_sequence_operation()
        {
            Assert.Equal(
                @"ALTER SCHEMA [dbo2] TRANSFER [dbo].[MySequence]",
                Generate(new MoveSequenceOperation("dbo.MySequence", "dbo2")).Sql);
        }

        [Fact]
        public void Generate_when_rename_sequence_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MySequence', @newname = N'MySequence2', @objtype = N'OBJECT'",
                Generate(new RenameSequenceOperation("dbo.MySequence", "MySequence2")).Sql);
        }

        [Fact]
        public void Generate_when_drop_sequence_operation()
        {
            Assert.Equal(
                @"DROP SEQUENCE [dbo].[MySequence]",
                Generate(new DropSequenceOperation("dbo.MySequence")).Sql);
        }

        [Fact]
        public void Generate_when_alter_sequence_operation()
        {
            Assert.Equal(
                @"ALTER SEQUENCE [dbo].[MySequence] INCREMENT BY 7",
                Generate(new AlterSequenceOperation("dbo.MySequence", 7)).Sql);
        }

        [Fact]
        public void Generate_when_create_table_operation()
        {
            Column foo, bar;
            var table = new Table("dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", "int") { IsNullable = false, DefaultValue = 5 },
                        bar = new Column("Bar", "int") { IsNullable = true }
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo, bar }, isClustered: false)
                };
            var database = new DatabaseModel();
            database.AddTable(table);

            Assert.Equal(
                @"CREATE TABLE [dbo].[MyTable] (
    [Foo] int NOT NULL DEFAULT 5,
    [Bar] int,
    CONSTRAINT [MyPK] PRIMARY KEY NONCLUSTERED ([Foo], [Bar])
)",
                Generate(new CreateTableOperation(table), database).Sql);
        }

        [Fact]
        public void Generate_when_create_table_operation_with_Identity_key()
        {
            Column foo, bar;
            var table = new Table(
                "dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", "int") { IsNullable = false, ClrType = typeof(int), GenerateValueOnAdd = true },
                        bar = new Column("Bar", "int") { IsNullable = true }
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo }, isClustered: false)
                };
            var database = new DatabaseModel();
            database.AddTable(table);

            Assert.Equal(
                @"CREATE TABLE [dbo].[MyTable] (
    [Foo] int NOT NULL IDENTITY,
    [Bar] int,
    CONSTRAINT [MyPK] PRIMARY KEY NONCLUSTERED ([Foo])
)",
                Generate(new CreateTableOperation(table), database).Sql);
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            Assert.Equal(
                @"DROP TABLE [dbo].[MyTable]",
                Generate(new DropTableOperation("dbo.MyTable")).Sql);
        }

        [Fact]
        public void Generate_when_rename_table_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable', @newname = N'MyTable2', @objtype = N'OBJECT'",
                Generate(new RenameTableOperation("dbo.MyTable", "MyTable2")).Sql);
        }

        [Fact]
        public void Generate_when_move_table_operation()
        {
            Assert.Equal(
                @"ALTER SCHEMA [dbo2] TRANSFER [dbo].[MyTable]",
                Generate(new MoveTableOperation("dbo.MyTable", "dbo2")).Sql);
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var database = new DatabaseModel();
            database.AddTable(new Table("dbo.MyTable"));

            var column = new Column("Bar", "int") { IsNullable = false, DefaultValue = 5 };

            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD [Bar] int NOT NULL DEFAULT 5",
                Generate(new AddColumnOperation("dbo.MyTable", column), database).Sql);
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            var database = new DatabaseModel();
            database.AddTable(new Table("dbo.MyTable", new[] { new Column("Foo", typeof(int)) }));

            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] DROP COLUMN [Foo]",
                Generate(new DropColumnOperation("dbo.MyTable", "Foo"), database).Sql);
        }

        [Fact]
        public void Generate_when_alter_column_operation()
        {
            var database = new DatabaseModel();
            var table
                = new Table(
                    "dbo.MyTable",
                    new[]
                        {
                            new Column("Foo", typeof(int)) { IsNullable = true }
                        });
            database.AddTable(table);

            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ALTER COLUMN [Foo] int NOT NULL",
                Generate(
                    new AlterColumnOperation("dbo.MyTable", new Column("Foo", typeof(int)) { IsNullable = false },
                        isDestructiveChange: false),
                    database).Sql);
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD CONSTRAINT [DF_dbo.MyTable_Foo] DEFAULT 5 FOR [Foo]",
                Generate(
                    new AddDefaultConstraintOperation("dbo.MyTable", "Foo", 5, null)).Sql);
        }

        [Fact]
        public void Generate_when_drop_default_constraint_operation()
        {
            Assert.Equal(
                @"DECLARE @var0 nvarchar(128)
SELECT @var0 = name FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.MyTable') AND COL_NAME(parent_object_id, parent_column_id) = N'Foo'
EXECUTE('ALTER TABLE [dbo].[MyTable] DROP CONSTRAINT ""' + @var0 + '""')",
                Generate(
                    new DropDefaultConstraintOperation("dbo.MyTable", "Foo")).Sql);
        }

        [Fact]
        public void Generate_when_rename_column_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable.Foo', @newname = N'Foo2', @objtype = N'COLUMN'",
                Generate(
                    new RenameColumnOperation("dbo.MyTable", "Foo", "Foo2")).Sql);
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD CONSTRAINT [MyPK] PRIMARY KEY NONCLUSTERED ([Foo], [Bar])",
                Generate(
                    new AddPrimaryKeyOperation("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: false)).Sql);
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] DROP CONSTRAINT [MyPK]",
                Generate(new DropPrimaryKeyOperation("dbo.MyTable", "MyPK")).Sql);
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable] ADD CONSTRAINT [MyFK] FOREIGN KEY ([Foo], [Bar]) REFERENCES [dbo].[MyTable2] ([Foo2], [Bar2]) ON DELETE CASCADE",
                Generate(
                    new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                        "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: true)).Sql);
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE [dbo].[MyTable2] DROP CONSTRAINT [MyFK]",
                Generate(new DropForeignKeyOperation("dbo.MyTable2", "MyFK")).Sql);
        }

        [Fact]
        public void Generate_when_create_index_operation()
        {
            Assert.Equal(
                @"CREATE UNIQUE CLUSTERED INDEX [MyIndex] ON [dbo].[MyTable] ([Foo], [Bar])",
                Generate(
                    new CreateIndexOperation("dbo.MyTable", "MyIndex", new[] { "Foo", "Bar" },
                        isUnique: true, isClustered: true)).Sql);
        }

        [Fact]
        public void Generate_when_drop_index_operation()
        {
            Assert.Equal(
                @"DROP INDEX [MyIndex] ON [dbo].[MyTable]",
                Generate(new DropIndexOperation("dbo.MyTable", "MyIndex")).Sql);
        }

        [Fact]
        public void Generate_when_rename_index_operation()
        {
            Assert.Equal(
                @"EXECUTE sp_rename @objname = N'dbo.MyTable.MyIndex', @newname = N'MyIndex2', @objtype = N'INDEX'",
                Generate(
                    new RenameIndexOperation("dbo.MyTable", "MyIndex", "MyIndex2")).Sql);
        }

        [Fact]
        public void GenerateDataType_for_string_thats_not_a_key()
        {
            Assert.Equal(
                "nvarchar(max)",
                GenerateDataType(CreateColumn(typeof(string))));
        }

        [Fact]
        public void GenerateDataType_for_string_key()
        {
            var column = new Column("Username", typeof(string));
            var table = new Table("dbo.Users");
            table.PrimaryKey = new PrimaryKey("PK_Users", new List<Column> { column }.AsReadOnly());
            table.AddColumn(column);

            Assert.Equal("nvarchar(128)", GenerateDataType(column));
        }

        [Fact]
        public void GenerateDataType_for_DateTime()
        {
            Assert.Equal(
                "datetime2",
                GenerateDataType(CreateColumn(typeof(DateTime))));
        }

        [Fact]
        public void GenerateDataType_for_decimal()
        {
            Assert.Equal(
                "decimal(18, 2)",
                GenerateDataType(CreateColumn(typeof(decimal))));
        }

        [Fact]
        public void GenerateDataType_for_Guid()
        {
            Assert.Equal(
                "uniqueidentifier",
                GenerateDataType(CreateColumn(typeof(Guid))));
        }

        [Fact]
        public void GenerateDataType_for_bool()
        {
            Assert.Equal(
                "bit",
                GenerateDataType(CreateColumn(typeof(bool))));
        }

        [Fact]
        public void GenerateDataType_for_byte()
        {
            Assert.Equal(
                "tinyint",
                GenerateDataType(CreateColumn(typeof(byte))));
        }

        [Fact]
        public void GenerateDataType_for_char()
        {
            Assert.Equal(
                "int",
                GenerateDataType(CreateColumn(typeof(char))));
        }

        [Fact]
        public void GenerateDataType_for_double()
        {
            Assert.Equal(
                "float",
                GenerateDataType(CreateColumn(typeof(double))));
        }

        [Fact]
        public void GenerateDataType_for_short()
        {
            Assert.Equal(
                "smallint",
                GenerateDataType(CreateColumn(typeof(short))));
        }

        [Fact]
        public void GenerateDataType_for_long()
        {
            Assert.Equal(
                "bigint",
                GenerateDataType(CreateColumn(typeof(long))));
        }

        [Fact]
        public void GenerateDataType_for_sbyte()
        {
            Assert.Equal(
                "smallint",
                GenerateDataType(CreateColumn(typeof(sbyte))));
        }

        [Fact]
        public void GenerateDataType_for_float()
        {
            Assert.Equal(
                "real",
                GenerateDataType(CreateColumn(typeof(float))));
        }

        [Fact]
        public void GenerateDataType_for_ushort()
        {
            Assert.Equal(
                "int",
                GenerateDataType(CreateColumn(typeof(ushort))));
        }

        [Fact]
        public void GenerateDataType_for_uint()
        {
            Assert.Equal(
                "bigint",
                GenerateDataType(CreateColumn(typeof(uint))));
        }

        [Fact]
        public void GenerateDataType_for_ulong()
        {
            Assert.Equal(
                "numeric(20, 0)",
                GenerateDataType(CreateColumn(typeof(ulong))));
        }

        [Fact]
        public void GenerateDataType_for_DateTimeOffset()
        {
            Assert.Equal(
                "datetimeoffset",
                GenerateDataType(CreateColumn(typeof(DateTimeOffset))));
        }

        [Fact]
        public void GenerateDataType_for_byte_array_that_is_not_a_concurrency_token_or_a_primary_key()
        {
            Assert.Equal(
                "varbinary(max)",
                GenerateDataType(CreateColumn(typeof(byte[]))));
        }

        [Fact]
        public void GenerateDataType_for_byte_array_key()
        {
            var column = new Column("Username", typeof(byte[]));
            var table = new Table("dbo.Users") { PrimaryKey = new PrimaryKey("PK_Users", new[] { column }) };
            table.AddColumn(column);

            Assert.Equal("varbinary(128)", GenerateDataType(column));
        }

        [Fact]
        public void GenerateDataType_for_byte_array_concurrency_token()
        {
            var column = new Column("Username", typeof(byte[])) { IsTimestamp = true };
            var table = new Table("dbo.Users");
            table.AddColumn(column);

            Assert.Equal("rowversion", GenerateDataType(column));
        }

        private static Column CreateColumn(Type clrType)
        {
            var column = new Column("Username", clrType);
            var table = new Table("dbo.Users");
            table.AddColumn(column);
            return column;
        }

        private static string GenerateDataType(Column column)
        {
            var sqlGenerator = CreateSqlGenerator();
            sqlGenerator.Database = new DatabaseModel();
            sqlGenerator.Database.AddTable(column.Table);
            return sqlGenerator.GenerateDataType(column.Table, column);
        }

        [Fact]
        public void Delimit_identifier()
        {
            var sqlGenerator = CreateSqlGenerator();

            Assert.Equal("[foo[]]bar]", sqlGenerator.DelimitIdentifier("foo[]bar"));
        }

        [Fact]
        public void Escape_identifier()
        {
            var sqlGenerator = CreateSqlGenerator();

            Assert.Equal("foo[]]]]bar", sqlGenerator.EscapeIdentifier("foo[]]bar"));
        }

        [Fact]
        public void Delimit_literal()
        {
            var sqlGenerator = CreateSqlGenerator();

            Assert.Equal("'foo''bar'", sqlGenerator.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void Escape_literal()
        {
            var sqlGenerator = CreateSqlGenerator();

            Assert.Equal("foo''bar", sqlGenerator.EscapeLiteral("foo'bar"));
        }

        private static SqlStatement Generate(MigrationOperation migrationOperation, DatabaseModel database = null)
        {
            return CreateSqlGenerator(database).Generate(migrationOperation);
        }

        private static SqlServerMigrationOperationSqlGenerator CreateSqlGenerator(DatabaseModel database = null)
        {
            return
                new SqlServerMigrationOperationSqlGenerator(new SqlServerTypeMapper())
                    {
                        Database = database ?? new DatabaseModel()
                    };
        }
    }
}
