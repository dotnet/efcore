// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Relational.Model;
using Moq;
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
                Generate(new CreateDatabaseOperation("MyDatabase")).Sql);
        }

        [Fact]
        public void Generate_when_drop_database_operation()
        {
            Assert.Equal(
                @"DROP DATABASE ""MyDatabase""",
                Generate(new DropDatabaseOperation("MyDatabase")).Sql);
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            Assert.Equal(
                @"CREATE SEQUENCE ""dbo"".""MySequence"" AS bigint START WITH 0 INCREMENT BY 1",
                Generate(
                    new CreateSequenceOperation(new Sequence("dbo.MySequence", "bigint", 0, 1))).Sql);
        }

        [Fact]
        public void Generate_when_drop_sequence_operation()
        {
            Assert.Equal(
                @"DROP SEQUENCE ""dbo"".""MySequence""",
                Generate(new DropSequenceOperation("dbo.MySequence")).Sql);
        }

        [Fact]
        public void Generate_when_alter_sequence_operation()
        {
            Assert.Equal(
                @"ALTER SEQUENCE ""dbo"".""MySequence"" INCREMENT BY 7",
                Generate(new AlterSequenceOperation("dbo.MySequence", 7)).Sql);
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
    CONSTRAINT ""MyPK"" PRIMARY KEY (""Foo"", ""Bar"")
)",
                Generate(
                    new CreateTableOperation(table)).Sql);
        }

        [Fact]
        public void Generate_when_create_table_with_unique_constraints()
        {
            Column foo, bar, c1, c2;
            var table = new Table(
                "dbo.MyTable",
                new[]
                    {
                        foo = new Column("Foo", "int") { IsNullable = false, DefaultValue = 5 },
                        bar = new Column("Bar", "int") { IsNullable = true },
                        c1 = new Column("C1", "varchar"),
                        c2 = new Column("C2", "varchar")
                    })
                {
                    PrimaryKey = new PrimaryKey("MyPK", new[] { foo }, isClustered: false)
                };
            table.AddUniqueConstraint(new UniqueConstraint("MyUC0", new[] { c1 }));
            table.AddUniqueConstraint(new UniqueConstraint("MyUC1", new[] { bar, c2 }));

            Assert.Equal(
                @"CREATE TABLE ""dbo"".""MyTable"" (
    ""Foo"" int NOT NULL DEFAULT 5,
    ""Bar"" int,
    ""C1"" varchar,
    ""C2"" varchar,
    CONSTRAINT ""MyPK"" PRIMARY KEY (""Foo""),
    CONSTRAINT ""MyUC0"" UNIQUE (""C1""),
    CONSTRAINT ""MyUC1"" UNIQUE (""Bar"", ""C2"")
)",
                Generate(new CreateTableOperation(table)).Sql);
        }

        [Fact]
        public void Generate_when_drop_table_operation()
        {
            Assert.Equal(
                @"DROP TABLE ""dbo"".""MyTable""",
                Generate(new DropTableOperation("dbo.MyTable")).Sql);
        }

        [Fact]
        public void Generate_when_add_column_operation()
        {
            var database = new DatabaseModel();
            database.AddTable(new Table("dbo.MyTable"));

            var column = new Column("Bar", "int") { IsNullable = false, DefaultValue = 5 };

            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD ""Bar"" int NOT NULL DEFAULT 5",
                Generate(new AddColumnOperation("dbo.MyTable", column), database).Sql);
        }

        [Fact]
        public void Generate_when_drop_column_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP COLUMN ""Foo""",
                Generate(
                    new DropColumnOperation("dbo.MyTable", "Foo")).Sql);
        }

        [Fact]
        public void Generate_when_alter_column_operation_with_nullable()
        {
            var database = new DatabaseModel();
            var table
                = new Table(
                    "dbo.MyTable",
                    new[]
                        {
                            new Column("Foo", typeof(int)) { IsNullable = false }
                        });
            database.AddTable(table);

            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" int NULL",
                Generate(
                    new AlterColumnOperation("dbo.MyTable",
                        new Column("Foo", "int") { IsNullable = true }, isDestructiveChange: false),
                    database).Sql);
        }

        [Fact]
        public void Generate_when_alter_column_operation_with_not_nullable()
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
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" int NOT NULL",
                Generate(
                    new AlterColumnOperation("dbo.MyTable",
                        new Column("Foo", "int") { IsNullable = false }, isDestructiveChange: false),
                    database).Sql);
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_value()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" SET DEFAULT 'MyDefault'",
                Generate(
                    new AddDefaultConstraintOperation("dbo.MyTable", "Foo", "MyDefault", null)).Sql);
        }

        [Fact]
        public void Generate_when_add_default_constraint_operation_with_default_sql()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" SET DEFAULT GETDATE()",
                Generate(
                    new AddDefaultConstraintOperation("dbo.MyTable", "Foo", null, "GETDATE()")).Sql);
        }

        [Fact]
        public void Generate_when_drop_default_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ALTER COLUMN ""Foo"" DROP DEFAULT",
                Generate(
                    new DropDefaultConstraintOperation("dbo.MyTable", "Foo")).Sql);
        }

        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyPK"" PRIMARY KEY (""Foo"", ""Bar"")",
                Generate(
                    new AddPrimaryKeyOperation("dbo.MyTable", "MyPK", new[] { "Foo", "Bar" }, isClustered: false)).Sql);
        }

        [Fact]
        public void Generate_when_drop_primary_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyPK""",
                Generate(new DropPrimaryKeyOperation("dbo.MyTable", "MyPK")).Sql);
        }

        [Fact]
        public void Generate_when_add_unique_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyUC"" UNIQUE (""Foo"", ""Bar"")",
                Generate(
                    new AddUniqueConstraintOperation("dbo.MyTable", "MyUC", new[] { "Foo", "Bar" })).Sql);
        }

        [Fact]
        public void Generate_when_drop_unique_constraint_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyUC""",
                Generate(new DropUniqueConstraintOperation("dbo.MyTable", "MyUC")).Sql);
        }

        [Fact]
        public void Generate_when_add_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" ADD CONSTRAINT ""MyFK"" FOREIGN KEY (""Foo"", ""Bar"") REFERENCES ""dbo"".""MyTable2"" (""Foo2"", ""Bar2"") ON DELETE CASCADE",
                Generate(
                    new AddForeignKeyOperation("dbo.MyTable", "MyFK", new[] { "Foo", "Bar" },
                        "dbo.MyTable2", new[] { "Foo2", "Bar2" }, cascadeDelete: true)).Sql);
        }

        [Fact]
        public void Generate_when_drop_foreign_key_operation()
        {
            Assert.Equal(
                @"ALTER TABLE ""dbo"".""MyTable"" DROP CONSTRAINT ""MyFK""",
                Generate(new DropForeignKeyOperation("dbo.MyTable", "MyFK")).Sql);
        }

        [Fact]
        public void Generate_when_create_index_operation()
        {
            Assert.Equal(
                @"CREATE UNIQUE CLUSTERED INDEX ""MyIndex"" ON ""dbo"".""MyTable"" (""Foo"", ""Bar"")",
                Generate(
                    new CreateIndexOperation("dbo.MyTable", "MyIndex", new[] { "Foo", "Bar" },
                        isUnique: true, isClustered: true)).Sql);
        }

        [Fact]
        public void Generate_when_drop_index_operation()
        {
            Assert.Equal(
                @"DROP INDEX ""MyIndex""",
                Generate(new DropIndexOperation("dbo.MyTable", "MyIndex")).Sql);
        }

        [Fact]
        public void Generate_when_copy_data_operation()
        {
            Assert.Equal(
                @"INSERT INTO ""dbo"".""T2"" ( ""C"", ""D"" )
    SELECT ""A"", ""B"" FROM ""dbo"".""T1""",
                Generate(new CopyDataOperation("dbo.T1", new[] { "A", "B" }, "dbo.T2", new[] { "C", "D" })).Sql);
        }

        [Fact]
        public void Generate_when_sql_operation()
        {
            const string sql =
                @"UPDATE T
    SET C1='V1'
    WHERE C2='V2'";

            var statement = Generate(new SqlOperation(sql));

            Assert.Equal(sql, statement.Sql);
            Assert.False(statement.SuppressTransaction);
        }

        [Fact]
        public void Generate_when_sql_operation_with_suppress_transaction_true()
        {
            const string sql =
                @"UPDATE T
    SET C1='V1'
    WHERE C2='V2'";

            var statement = Generate(new SqlOperation(sql) { SuppressTransaction = true });

            Assert.Equal(sql, statement.Sql);
            Assert.True(statement.SuppressTransaction);
        }

        [Fact]
        public void Delimit_identifier()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("\"foo\"\"bar\"", sqlGenerator.Object.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_identifier_when_schema_qualified()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("\"foo\".\"bar\"", sqlGenerator.Object.DelimitIdentifier(SchemaQualifiedName.Parse("foo.bar")));
        }

        [Fact]
        public void Escape_identifier()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("foo\"\"bar", sqlGenerator.Object.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_literal()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("'foo''bar'", sqlGenerator.Object.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void Escape_literal()
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true };

            Assert.Equal("foo''bar", sqlGenerator.Object.EscapeLiteral("foo'bar"));
        }

        [Fact]
        public void Database_setter_clones_value()
        {
            var sqlGenerator = (new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true }).Object;
            var database = new DatabaseModel();
            var table = new Table("dbo.MyTable");

            database.AddTable(table);
            sqlGenerator.Database = database;

            Assert.NotSame(database, sqlGenerator.Database);
            Assert.Equal(1, sqlGenerator.Database.Tables.Count);
            Assert.NotSame(table, sqlGenerator.Database.Tables[0]);
            Assert.Equal("dbo.MyTable", sqlGenerator.Database.Tables[0].Name);
        }

        [Fact]
        public void Generate_updates_database_model()
        {
            var sqlGenerator = (new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true }).Object;
            var database = new DatabaseModel();
            var table = new Table("dbo.MyTable");
            var column = new Column("Foo", typeof(int));

            sqlGenerator.Database = database;
            sqlGenerator.DatabaseModelModifier = new DatabaseModelModifier();

            var statementCount = sqlGenerator.Generate(
                new MigrationOperation[]
                    {
                        new CreateTableOperation(table),
                        new AddColumnOperation(table.Name, column)
                    })
                .Count();

            Assert.Equal(2, statementCount);
            Assert.Equal(1, sqlGenerator.Database.Tables.Count);
            Assert.NotSame(table, sqlGenerator.Database.Tables[0]);
            Assert.Equal(1, sqlGenerator.Database.Tables[0].Columns.Count);
            Assert.NotSame(column, sqlGenerator.Database.Tables[0].Columns[0]);
        }

        private static MigrationOperationSqlGenerator CreateSqlGenerator(DatabaseModel database = null)
        {
            var sqlGenerator = new Mock<MigrationOperationSqlGenerator>(new RelationalTypeMapper()) { CallBase = true }.Object;
            sqlGenerator.Database = database ?? new DatabaseModel();
            sqlGenerator.DatabaseModelModifier = new DatabaseModelModifier();
            return sqlGenerator;
        }

        private static SqlStatement Generate(MigrationOperation migrationOperation, DatabaseModel database = null)
        {
            return CreateSqlGenerator(database).Generate(migrationOperation);
        }
    }
}
