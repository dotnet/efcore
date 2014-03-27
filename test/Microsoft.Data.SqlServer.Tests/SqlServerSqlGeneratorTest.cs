// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Model;
using Xunit;

namespace Microsoft.Data.SqlServer.Tests
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
            var id1Column = new Column("Id1", "int") { GenerationStrategy = StoreValueGenerationStrategy.Identity };
            var id2Column = new Column("Id2", "nvarchar(max)");
            var nameColumn = new Column("Name", "nvarchar(30)");
            var table = new Table("table", new[] { id1Column, id2Column, nameColumn });
            table.PrimaryKey = new PrimaryKey("PK", new[] { id1Column, id2Column });

            var sb = new StringBuilder();
            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, table, 
                    new Dictionary<Column, string>{{id2Column, "@p0"}, {nameColumn, "@p1"}}.ToArray());

            Assert.Equal(
                "INSERT INTO table (Id2, Name) VALUES (@p0, @p1);\r\nSELECT Id1 FROM table WHERE Id2 = @p0 AND Id1 = scope_identity()",
                sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_valid_Select_for_insert_operation_with_identity_key_and_computed_non_key_column()
        {
            var id1Column = new Column("Id1", "int") { GenerationStrategy = StoreValueGenerationStrategy.Identity };
            var insertedColumn = new Column("Inserted", "int") { GenerationStrategy = StoreValueGenerationStrategy.Computed };
            var nameColumn = new Column("Name", "nvarchar(30)");
            var table = new Table("table", new[] { id1Column, nameColumn, insertedColumn });
            table.PrimaryKey = new PrimaryKey("PK", new[] { id1Column });

            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, table, new[] { new KeyValuePair<Column, string>(nameColumn, "@p0") });

            Assert.Equal(
                "INSERT INTO table (Name) VALUES (@p0);\r\nSELECT Id1, Inserted FROM table WHERE Id1 = scope_identity()",
                sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_valid_statement_for_non_identity_auto_generated_keys()
        {
            var id1Column = new Column("Id1", "int") { GenerationStrategy = StoreValueGenerationStrategy.Computed };
            var id2Column = new Column("Id2", "nvarchar(max)");
            var nameColumn = new Column("Name", "nvarchar(30)");
            var table = new Table("Customers", new[] { id1Column, id2Column, nameColumn });
            table.PrimaryKey = new PrimaryKey("PK", new[] { id1Column, id2Column });

            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, table, 
                    new Dictionary<Column, string> { { id2Column, "@p0" }, { nameColumn, "@p1" } }.ToArray());

            const string expected =
                "DECLARE @generated_keys_Customers TABLE(Id1 int, Id2 nvarchar(max));\r\n" +
                "INSERT INTO Customers (Id2, Name)\r\n" +
                "OUTPUT inserted.Id1, inserted.Id2 INTO @generated_keys_Customers\r\n" +
                "VALUES (@p0, @p1);\r\n" +
                "SELECT t.Id1 FROM @generated_keys_Customers AS g JOIN Customers AS t ON g.Id1 = t.Id1 AND g.Id2 = t.Id2";

            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_valid_statement_for_computed_and_identity_composite_key()
        {
            var id1Column = new Column("Id1", "int") { GenerationStrategy = StoreValueGenerationStrategy.Computed };
            var id2Column = new Column("Id2", "nvarchar(max)") { GenerationStrategy = StoreValueGenerationStrategy.Identity };
            var nameColumn = new Column("Name", "nvarchar(30)");
            var table = new Table("Customers", new[] { id1Column, id2Column, nameColumn });
            table.PrimaryKey = new PrimaryKey("PK", new[] { id1Column, id2Column });

            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, table, new[] { new KeyValuePair<Column, string>(nameColumn, "@p0") });

            const string expected =
                "DECLARE @generated_keys_Customers TABLE(Id1 int, Id2 nvarchar(max));\r\n" +
                "INSERT INTO Customers (Name)\r\n" +
                "OUTPUT inserted.Id1, inserted.Id2 INTO @generated_keys_Customers\r\n" +
                "VALUES (@p0);\r\n" +
                "SELECT t.Id1, t.Id2 FROM @generated_keys_Customers AS g JOIN Customers AS t ON g.Id1 = t.Id1 AND g.Id2 = t.Id2";

            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void AppendInsertOperation_test_appends_valid_statement_for_computed_and_identity_composite_key_and_computed_non_key_column()
        {
            var id1Column = new Column("Id1", "int") { GenerationStrategy = StoreValueGenerationStrategy.Computed };
            var id2Column = new Column("Id2", "nvarchar(max)") { GenerationStrategy = StoreValueGenerationStrategy.Identity };
            var insertedColumn = new Column("Inserted", "int") { GenerationStrategy = StoreValueGenerationStrategy.Computed };
            var nameColumn = new Column("Name", "nvarchar(30)");
            var table = new Table("Customers", new[] { id1Column, id2Column, insertedColumn, nameColumn });
            table.PrimaryKey = new PrimaryKey("PK", new[] { id1Column, id2Column });
            var sb = new StringBuilder();

            new SqlServerSqlGenerator()
                .AppendInsertOperation(sb, table, new[] { new KeyValuePair<Column, string>(nameColumn, "@p0") });

            const string expected =
                "DECLARE @generated_keys_Customers TABLE(Id1 int, Id2 nvarchar(max));\r\n" +
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
            public void CreateWhereConditionsForStoreGeneratedKeys_validates_parameters()
            {
                Assert.Equal(
                    "storeGeneratedKeyColumns",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlServerSqlGenerator()
                            .CreateWhereConditionsForStoreGeneratedKeys(null)).ParamName);
            }
        }
    }
}
