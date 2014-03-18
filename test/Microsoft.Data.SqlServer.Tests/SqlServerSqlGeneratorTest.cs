// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.SqlServer
{
    public class SqlServerSqlGeneratorTest
    {
        [Fact]
        public void AppendBatchHeader_should_append_SET_NOCOUNT_OFF()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator().AppendBatchHeader(sb);

            Assert.Equal("SET NOCOUNT OFF", sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_Select_for_insert_operation_with_identity()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, "table", new [] { "Id1", "Id2" },
                    new Dictionary<string, string> { { "Id2", "@p0" }, { "Name", "@p1" } }.ToArray(), 
                    new Dictionary<string, ValueGenerationStrategy> { { "Id1", ValueGenerationStrategy.StoreIdentity } }.ToArray());

            Assert.Equal(
                "INSERT INTO table (Id2, Name) VALUES (@p0, @p1);\r\nSELECT Id1 FROM table WHERE Id2 = @p0 AND Id1 = scope_identity()", 
                sb.ToString());
        }

        public class ParameterValidation
        {
            [Fact]
            public void AppendBatchHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlServerSqlGenerator().AppendBatchHeader(null)).ParamName);
            }

            [Fact]
            public void AppendModificationOperationSelectWhereClause_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlServerSqlGenerator()
                            .AppendModificationOperationSelectWhereClause(null, new Dictionary<string, string>(),
                                new Dictionary<string, ValueGenerationStrategy>())).ParamName);

                Assert.Equal(
                    "knownKeyValues",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlServerSqlGenerator()
                            .AppendModificationOperationSelectWhereClause(new StringBuilder(), null,
                                new Dictionary<string, ValueGenerationStrategy>())).ParamName);

                Assert.Equal(
                    "generatedKeys",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlServerSqlGenerator()
                            .AppendModificationOperationSelectWhereClause(new StringBuilder(), 
                                new Dictionary<string, string>(), null)).ParamName);
            }
        }
    }
}
