// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.SqlServer.Tests
{
    public class SqlServerMigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            var table = new Table("dbo.T");
            var column0 = new Column("Foo", "int");
            var column1 = new Column("Bar", "int");
            table.AddColumn(column0);
            table.AddColumn(column1);
            var primaryKey = new PrimaryKey("pk", new[] { column0, column1 });

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"T\" ADD CONSTRAINT \"pk\" PRIMARY KEY NONCLUSTERED (\"Foo\", \"Bar\")",
                SqlServerMigrationOperationSqlGenerator.Generate(new AddPrimaryKeyOperation(primaryKey, table)));
        }

        [Fact]
        public void Generate_when_add_primary_key_operation_when_is_clustered()
        {
            var table = new Table("dbo.T");
            var column = new Column("Foo", "int");
            table.AddColumn(column);
            var primaryKey = new PrimaryKey("pk", new[] { column });

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"T\" ADD CONSTRAINT \"pk\" PRIMARY KEY NONCLUSTERED (\"Foo\")",
                SqlServerMigrationOperationSqlGenerator.Generate(new AddPrimaryKeyOperation(primaryKey, table)));
        }

        [Fact]
        public void Generate_when_create_sequence_operation_and_idempotent()
        {
            Assert.Equal(
                @"IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'MySequence')
    CREATE SEQUENCE ""MySequence"" AS BIGINT START WITH 0 INCREMENT BY 1",
                SqlServerMigrationOperationSqlGenerator.Generate(new CreateSequenceOperation(new Sequence("MySequence"))));
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
