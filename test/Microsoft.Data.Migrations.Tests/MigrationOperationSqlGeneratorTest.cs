// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Migrations.Tests
{
    public class MigrationOperationSqlGeneratorTest
    {
        #region Fixture

        private class UnknownOperation : MigrationOperation
        {
        }

        #endregion

        [Fact]
        public void Generate_when_unsupported_operation()
        {
            Assert.Equal(
                Strings.FormatUnknownOperation(typeof(MigrationOperationSqlGenerator), typeof(UnknownOperation)),
                Assert.Throws<NotSupportedException>(
                    () => MigrationOperationSqlGenerator.Generate(new UnknownOperation())).Message);
        }

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
                "ALTER TABLE \"dbo\".\"T\" ADD CONSTRAINT \"pk\" PRIMARY KEY (\"Foo\", \"Bar\")",
                MigrationOperationSqlGenerator.Generate(new AddPrimaryKeyOperation(primaryKey, table)));
        }

        [Fact]
        public void Generate_when_create_sequence_operation()
        {
            Assert.Equal(
                "CREATE SEQUENCE \"dbo\".\"MySequence\" AS BIGINT START WITH 0 INCREMENT BY 1",
                MigrationOperationSqlGenerator.Generate(new CreateSequenceOperation(new Sequence("dbo.MySequence"))));
        }

        [Fact]
        public void Delimit_identifier()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("\"foo\"\"bar\"", ddlSqlGenerator.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_identifier_when_schema_qualified()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("\"foo\".\"bar\"", ddlSqlGenerator.DelimitIdentifier(SchemaQualifiedName.Parse("foo.bar")));
        }

        [Fact]
        public void Escape_identifier()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("foo\"\"bar", ddlSqlGenerator.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void Delimit_literal()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("'foo''bar'", ddlSqlGenerator.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void Escape_literal()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("foo''bar", ddlSqlGenerator.EscapeLiteral("foo'bar"));
        }
    }
}
