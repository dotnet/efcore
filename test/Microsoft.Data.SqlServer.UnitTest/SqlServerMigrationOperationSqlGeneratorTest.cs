// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerMigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void Generate_when_add_primary_key_operation()
        {
            var table = new Table("dbo.T");
            var primaryKey = new PrimaryKey("pk");
            primaryKey.Columns.AddRange(new[] { new Column("Foo", "int"), new Column("Bar", "int") });

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"T\" ADD CONSTRAINT \"pk\" PRIMARY KEY NONCLUSTERED (\"Foo\", \"Bar\")",
                SqlServerMigrationOperationSqlGenerator.Generate(new AddPrimaryKeyOperation(primaryKey, table)));
        }

        [Fact]
        public void Generate_when_add_primary_key_operation_when_is_clustered()
        {
            var table = new Table("dbo.T");
            var primaryKey = new PrimaryKey("pk") { IsClustered = true };
            primaryKey.Columns.Add(new Column("Foo", "int"));

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"T\" ADD CONSTRAINT \"pk\" PRIMARY KEY (\"Foo\")",
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
