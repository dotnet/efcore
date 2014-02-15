// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Migrations.Model;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerDdlSqlGeneratorTest
    {
        [Fact]
        public void GenerateWhenCreateSequenceOperationAndIdempotent()
        {
            Assert.Equal(
                @"IF NOT EXISTS (SELECT * FROM sys.sequences WHERE name = 'MySequence')
    CREATE SEQUENCE ""MySequence"" AS BIGINT START WITH 0 INCREMENT BY 1",
                SqlServerDdlSqlGenerator.Generate(new CreateSequenceOperation("MySequence")));
        }

        [Fact]
        public void DelimitIdentifier()
        {
            var ddlSqlGenerator = new SqlServerDdlSqlGenerator();

            Assert.Equal("\"foo\"\"bar\"", ddlSqlGenerator.DelimitIdentifier("foo\"bar"));
        }

        [Fact]
        public void EscapeIdentifier()
        {
            var ddlSqlGenerator = new SqlServerDdlSqlGenerator();

            Assert.Equal("foo\"\"bar", ddlSqlGenerator.EscapeIdentifier("foo\"bar"));
        }

        [Fact]
        public void DelimitLiteral()
        {
            var ddlSqlGenerator = new SqlServerDdlSqlGenerator();

            Assert.Equal("'foo''bar'", ddlSqlGenerator.DelimitLiteral("foo'bar"));
        }

        [Fact]
        public void EscapeLiteral()
        {
            var ddlSqlGenerator = new SqlServerDdlSqlGenerator();

            Assert.Equal("foo''bar", ddlSqlGenerator.EscapeLiteral("foo'bar"));
        }
    }
}
