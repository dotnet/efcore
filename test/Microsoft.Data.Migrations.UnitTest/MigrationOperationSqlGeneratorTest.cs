// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.Migrations
{
    public class MigrationOperationSqlGeneratorTest
    {
        #region Fixture

        private class UnknownOperation : MigrationOperation
        {
        }

        #endregion

        [Fact]
        public void GenerateWhenUnsupportedOperation()
        {
            Assert.Equal(
                Strings.UnknownOperation(typeof(MigrationOperationSqlGenerator), typeof(UnknownOperation)),
                Assert.Throws<NotSupportedException>(
                    () => MigrationOperationSqlGenerator.Generate(new UnknownOperation())).Message);
        }

        [Fact]
        public void GenerateWhenAddPrimaryKeyOperation()
        {
            var table = new Table("dbo.T");
            var primaryKey = new PrimaryKey("pk");
            primaryKey.Columns.AddRange(new[] { new Column("Foo", "int"), new Column("Bar", "int") });

            Assert.Equal(
                "ALTER TABLE \"dbo\".\"T\" ADD CONSTRAINT \"pk\" PRIMARY KEY (\"Foo\", \"Bar\")",
                MigrationOperationSqlGenerator.Generate(new AddPrimaryKeyOperation(primaryKey, table)));
        }

        [Fact]
        public void GenerateWhenCreateSequenceOperation()
        {
            Assert.Equal(
                "CREATE SEQUENCE \"dbo\".\"MySequence\" AS BIGINT START WITH 0 INCREMENT BY 1",
                MigrationOperationSqlGenerator.Generate(new CreateSequenceOperation(new Sequence("dbo.MySequence"))));
        }

        [Fact]
        public void DelimitIdentifier()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("\"foo\"\"bar\"", ddlSqlGenerator.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void DelimitIdentifierWhenSchemaQualified()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("\"foo\".\"bar\"", ddlSqlGenerator.DelimitIdentifier(SchemaQualifiedName.Parse("foo.bar")));
        }

        [Fact]
        public void EscapeIdentifier()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("foo\"\"bar", ddlSqlGenerator.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void DelimitLiteral()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("'foo''bar'", ddlSqlGenerator.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void EscapeLiteral()
        {
            var ddlSqlGenerator = new MigrationOperationSqlGenerator();

            Assert.Equal("foo''bar", ddlSqlGenerator.EscapeLiteral("foo'bar"));
        }
    }
}
