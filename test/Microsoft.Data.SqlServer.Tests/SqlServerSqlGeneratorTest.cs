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
        public void AppendInsertOperation_test_appends_Select_for_insert_operation_with_identity_key()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, "table",
                    new Dictionary<string, string> { { "Id1", "int" }, { "Id2", "nvarchar(max)" } }.ToArray(),
                    new Dictionary<string, string> { { "Id2", "@p0" }, { "Name", "@p1" } }.ToArray(),
                    new Dictionary<string, ValueGenerationStrategy> { { "Id1", ValueGenerationStrategy.StoreIdentity } }.ToArray());

            Assert.Equal(
                "INSERT INTO table (Id2, Name) VALUES (@p0, @p1);\r\nSELECT Id1 FROM table WHERE Id2 = @p0 AND Id1 = scope_identity()",
                sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_valid_Select_for_insert_operation_with_identity_key_and_computed_non_key_column()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, "table",
                    new [] { new KeyValuePair<string, string>( "Id1", "int" ) },
                    new Dictionary<string, string> { { "Name", "@p0" } }.ToArray(),
                    new Dictionary<string, ValueGenerationStrategy>
                        {
                            { "Id1", ValueGenerationStrategy.StoreIdentity }, 
                            { "Inserted", ValueGenerationStrategy.StoreComputed }
                        }.ToArray());

            Assert.Equal(
                "INSERT INTO table (Name) VALUES (@p0);\r\nSELECT Id1, Inserted FROM table WHERE Id1 = scope_identity()",
                sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_valid_statement_for_non_identity_auto_generated_keys()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, "Customers",
                    new Dictionary<string, string> { { "Id1", "int" }, { "Id2", "nvarchar(max)" } }.ToArray(),
                    new Dictionary<string, string> { { "Id2", "@p0" }, { "Name", "@p1" } }.ToArray(),
                    new Dictionary<string, ValueGenerationStrategy> { { "Id1", ValueGenerationStrategy.StoreComputed } }.ToArray());

            const string expected =
                "DECLARE @generated_keys_Customers table(Id1 int, Id2 nvarchar(max));\r\n" +
                "INSERT INTO Customers (Id2, Name)\r\n" +
                "OUTPUT inserted.Id1, inserted.Id2 INTO @generated_keys_Customers\r\n" +
                "VALUES (@p0, @p1);\r\n" +
                "SELECT t.Id1 FROM @generated_keys_Customers AS g JOIN Customers AS t ON g.Id1 = t.Id1 AND g.Id2 = t.Id2";

            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_valid_statement_for_computed_and_identity_composite_key()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, "Customers",
                    new Dictionary<string, string> { { "Id1", "int" }, { "Id2", "nvarchar(max)" } }.ToArray(),
                    new Dictionary<string, string> { { "Name", "@p0" } }.ToArray(),
                    new Dictionary<string, ValueGenerationStrategy>
                        {
                            { "Id1", ValueGenerationStrategy.StoreComputed },
                            { "Id2", ValueGenerationStrategy.StoreIdentity }
                        }.ToArray());

            const string expected =
                "DECLARE @generated_keys_Customers table(Id1 int, Id2 nvarchar(max));\r\n" +
                "INSERT INTO Customers (Name)\r\n" +
                "OUTPUT inserted.Id1, inserted.Id2 INTO @generated_keys_Customers\r\n" +
                "VALUES (@p0);\r\n" +
                "SELECT t.Id1, t.Id2 FROM @generated_keys_Customers AS g JOIN Customers AS t ON g.Id1 = t.Id1 AND g.Id2 = t.Id2";

            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_valid_statement_for_computed_and_identity_composite_key_and_computed_non_key_column()
        {
            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, "Customers",
                    new Dictionary<string, string> { { "Id1", "int" }, { "Id2", "nvarchar(max)" } }.ToArray(),
                    new Dictionary<string, string> { { "Name", "@p0" } }.ToArray(),
                    new Dictionary<string, ValueGenerationStrategy>
                        {
                            { "Id1", ValueGenerationStrategy.StoreComputed },
                            { "Id2", ValueGenerationStrategy.StoreIdentity },
                            { "Inserted", ValueGenerationStrategy.StoreComputed }
                        }.ToArray());

            const string expected =
                "DECLARE @generated_keys_Customers table(Id1 int, Id2 nvarchar(max));\r\n" +
                "INSERT INTO Customers (Name)\r\n" +
                "OUTPUT inserted.Id1, inserted.Id2 INTO @generated_keys_Customers\r\n" +
                "VALUES (@p0);\r\n" +
                "SELECT t.Id1, t.Id2, t.Inserted FROM @generated_keys_Customers AS g JOIN Customers AS t ON g.Id1 = t.Id1 AND g.Id2 = t.Id2";

            Assert.Equal(expected, sb.ToString());
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
