// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Microsoft.Data.Relational;
using Xunit;

namespace Microsoft.Data.Migrations
{
    public class DdlSqlGeneratorTest
    {
        [Fact]
        public void GenerateWhenCreateSequenceOperation()
        {
            Assert.Equal(
                "CREATE SEQUENCE \"MySequence\" AS BIGINT START WITH 0 INCREMENT BY 1",
                DdlSqlGenerator.Generate(new CreateSequenceOperation("MySequence")));
        }

        [Fact]
        public void GenerateWhenCreateSequenceOperationWithSchema()
        {
            Assert.Equal(
                "CREATE SEQUENCE \"dbo\".\"MySequence\" AS BIGINT START WITH 0 INCREMENT BY 1",
                DdlSqlGenerator.Generate(new CreateSequenceOperation("dbo.MySequence")));
        }

        [Fact]
        public void DelimitIdentifier()
        {
            var ddlSqlGenerator = new DdlSqlGenerator();

            Assert.Equal("\"foo\"\"bar\"", ddlSqlGenerator.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void DelimitIdentifierWhenSchemaQualified()
        {
            var ddlSqlGenerator = new DdlSqlGenerator();

            Assert.Equal("\"foo\".\"bar\"", ddlSqlGenerator.DelimitIdentifier(SchemaQualifiedName.Parse("foo.bar")));
        }

        [Fact]
        public void EscapeIdentifier()
        {
            var ddlSqlGenerator = new DdlSqlGenerator();

            Assert.Equal("foo\"\"bar", ddlSqlGenerator.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void DelimitLiteral()
        {
            var ddlSqlGenerator = new DdlSqlGenerator();

            Assert.Equal("'foo''bar'", ddlSqlGenerator.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void EscapeLiteral()
        {
            var ddlSqlGenerator = new DdlSqlGenerator();

            Assert.Equal("foo''bar", ddlSqlGenerator.EscapeLiteral("foo'bar"));
        }
    }
}
