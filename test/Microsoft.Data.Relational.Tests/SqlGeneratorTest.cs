// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Xunit;

namespace Microsoft.Data.Relational.Tests
{
    public class SqlGeneratorTest
    {
        [Fact]
        public void AppendInsertCommandHeader_appends_correct_insert_command_header()
        {
            var stringBuilder = new StringBuilder();

            new SqlGenerator().AppendInsertCommandHeader(stringBuilder, "Table", new[] { "Id, CustomerName" });

            Assert.Equal(
                "INSERT INTO Table (Id, CustomerName)",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertCommand_creates_full_insert_command_text()
        {
            var mockDataStore = new Mock<SqlGenerator> { CallBase = true };

            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var columnNames = new[] { "col" };
            var parameters = new[] { "param" };

            mockDataStore.Object.AppendInsertCommand(stringBuilder, tableName, columnNames, parameters);

            Assert.Equal(
                "INSERT INTO table (col) VALUES (param)",
                stringBuilder.ToString());

            mockDataStore
                .Verify(s => s.AppendInsertCommandHeader(stringBuilder, tableName, columnNames), Times.Once());
            mockDataStore
                .Verify(s => s.AppendValues(stringBuilder, parameters), Times.Once());
        }

        [Fact]
        public void AppendDeleteCommandHeader_appends_correct_command_header()
        {
            var stringBuilder = new StringBuilder();

            new SqlGenerator().AppendDeleteCommandHeader(stringBuilder, "Table");

            Assert.Equal("DELETE FROM Table", stringBuilder.ToString());
        }

        [Fact]
        public void AppendDeleteCommand_creates_full_delete_command_text()
        {
            var mockDataStore = new Mock<SqlGenerator> { CallBase = true };

            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p1") };

            mockDataStore.Object.AppendDeleteCommand(stringBuilder, tableName, whereConditions);

            Assert.Equal(
                "DELETE FROM table WHERE Id = @p1",
                stringBuilder.ToString());

            mockDataStore
                .Verify(s => s.AppendDeleteCommandHeader(stringBuilder, tableName), Times.Once());
            mockDataStore
                .Verify(s => s.AppendWhereClause(stringBuilder, whereConditions), Times.Once());
        }

        [Fact]
        public void AppendUpdateCommandHeader_appends_correct_command_header()
        {
            var stringBuilder = new StringBuilder();

            new SqlGenerator()
                .AppendUpdateCommandHeader(stringBuilder, "Table",
                new[]
                    {
                        new KeyValuePair<string, string>("Col1", "@p1"),
                        new KeyValuePair<string, string>("Name", "@p2"),
                    });

            Assert.Equal("UPDATE Table SET Col1 = @p1, Name = @p2", stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateCommand_creates_full_delete_command_text()
        {
            var mockDataStore = new Mock<SqlGenerator> { CallBase = true };

            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var columnValues = new[] { new KeyValuePair<string, string>("Name", "@p1") };
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p2") };

            mockDataStore.Object.AppendUpdateCommand(stringBuilder, tableName, columnValues, whereConditions);

            Assert.Equal(
                "UPDATE table SET Name = @p1 WHERE Id = @p2",
                stringBuilder.ToString());

            mockDataStore
                .Verify(s => s.AppendUpdateCommandHeader(stringBuilder, tableName, columnValues), Times.Once());
            mockDataStore
                .Verify(s => s.AppendWhereClause(stringBuilder, whereConditions), Times.Once());
        }

        [Fact]
        public void AppendValues_appends_correct_values()
        {
            var stringBuilder = new StringBuilder();

            new SqlGenerator()
                .AppendValues(stringBuilder, new[] { "@p1", "@p2" });

            Assert.Equal(
                "VALUES (@p1, @p2)",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendWhereClause_appends_where_clause()
        {
            var stringBuilder = new StringBuilder();

            new SqlGenerator()
                .AppendWhereClause(
                    stringBuilder,
                    new[]
                    {
                        new KeyValuePair<string, string>("Id", "@p1"),
                        new KeyValuePair<string, string>("Col2", "@p2"),
                        new KeyValuePair<string, string>("Version", "@p3"),
                    });

            Assert.Equal(
                "WHERE Id = @p1 AND Col2 = @p2 AND Version = @p3",
                stringBuilder.ToString());
        }

        public class ParameterValidation
        {
            [Fact]
            public void AppendInsertCommand_checks_parameters_not_null()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendInsertCommand(null, "table", new string[0], new string[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendInsertCommand(new StringBuilder(), null, new string[0], new string[0])).ParamName);

                Assert.Equal(
                    "columnNames",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendInsertCommand(new StringBuilder(), "table", null, new string[0])).ParamName);

                Assert.Equal(
                    "valueParameterNames",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendInsertCommand(new StringBuilder(), "table", new string[0], null)).ParamName);
            }

            [Fact]
            public void AppendInsertCommandHeader_checks_parameters_not_null()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendInsertCommandHeader(null, "table", new string[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendInsertCommandHeader(new StringBuilder(), null, new string[0])).ParamName);

                Assert.Equal(
                    "columnNames",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendInsertCommandHeader(new StringBuilder(), "table", null)).ParamName);
            }

            [Fact]
            public void AppendDeleteCommand_checks_parameters_not_null()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendDeleteCommand(null, "table", new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendDeleteCommand(new StringBuilder(), null, new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendDeleteCommand(new StringBuilder(), "table", null)).ParamName);
            }

            [Fact]
            public void AppendDeleteCommandHeader_checks_parameters_not_null()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendDeleteCommandHeader(null, "table")).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendDeleteCommandHeader(new StringBuilder(), null)).ParamName);
            }

            [Fact]
            public void AppendUpdateCommand_checks_parameters_not_null()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendUpdateCommand(null, "table", new KeyValuePair<string, string>[0], new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendUpdateCommand(new StringBuilder(), null, new KeyValuePair<string, string>[0], new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "columnValues",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendUpdateCommand(new StringBuilder(), "table", null, new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendUpdateCommand(new StringBuilder(), "table", new KeyValuePair<string, string>[0], null)).ParamName);
            }

            [Fact]
            public void AppendUpdateCommandHeader_checks_parameters_not_null()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendUpdateCommandHeader(null, "table", new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendUpdateCommandHeader(new StringBuilder(), null, new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "columnValues",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendUpdateCommandHeader(new StringBuilder(), "table", null)).ParamName);
            }

            [Fact]
            public void AppendValues_checks_parameters_not_null()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendValues(null, new string[0])).ParamName);

                Assert.Equal(
                    "valueParameterNames",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendValues(new StringBuilder(), null)).ParamName);
            }

            [Fact]
            public void AppendWhereClause_checks_parameters_not_null()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendWhereClause(null, new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => new SqlGenerator().AppendWhereClause(new StringBuilder(), null)).ParamName);
            }
        }
    }
}
