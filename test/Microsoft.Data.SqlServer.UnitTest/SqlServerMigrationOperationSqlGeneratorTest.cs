// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerMigrationOperationSqlGeneratorTest
    {
        [Fact]
        public void GenerateWhenAddPrimaryKeyOperation()
        {
            var table = new Table("dbo.T");
            var primaryKey = new PrimaryKey("pk");
            primaryKey.Columns.AddRange(new[] { new Column("Foo", "int"), new Column("Bar", "int") });

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"T\" ADD CONSTRAINT \"pk\" PRIMARY KEY NONCLUSTERED (\"Foo\", \"Bar\")",
                SqlServerMigrationOperationSqlGenerator.Generate(new AddPrimaryKeyOperation(primaryKey, table)));
        }

        [Fact]
        public void GenerateWhenAddPrimaryKeyOperationWhenIsClustered()
        {
            var table = new Table("dbo.T");
            var primaryKey = new PrimaryKey("pk") { IsClustered = true };
            primaryKey.Columns.Add(new Column("Foo", "int"));

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"T\" ADD CONSTRAINT \"pk\" PRIMARY KEY (\"Foo\")",
                SqlServerMigrationOperationSqlGenerator.Generate(new AddPrimaryKeyOperation(primaryKey, table)));
        }

        [Fact]
        public void GenerateWhenCreateSequenceOperationAndIdempotent()
        {
            Assert.Equal(
                @"IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'MySequence')
    CREATE SEQUENCE ""MySequence"" AS BIGINT START WITH 0 INCREMENT BY 1",
                SqlServerMigrationOperationSqlGenerator.Generate(new CreateSequenceOperation(new Sequence("MySequence"))));
        }

        [Fact]
        public void DelimitIdentifier()
        {
            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator();

            Assert.Equal("\"foo\"\"bar\"", sqlGenerator.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void EscapeIdentifier()
        {
            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator();

            Assert.Equal("foo\"\"bar", sqlGenerator.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void DelimitLiteral()
        {
            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator();

            Assert.Equal("'foo''bar'", sqlGenerator.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void EscapeLiteral()
        {
            var sqlGenerator = new SqlServerMigrationOperationSqlGenerator();

            Assert.Equal("foo''bar", sqlGenerator.EscapeLiteral("foo'bar"));
        }
    }
}
