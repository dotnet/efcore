// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;
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

            CreateSqlGenerator()
                .AppendInsertCommandHeader(stringBuilder, "Table", new[] { "Id, CustomerName" });

            Assert.Equal(
                "INSERT INTO Table (Id, CustomerName)",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertCommand_creates_full_insert_command_text()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var columnNames = new[] { "col" };
            var parameters = new[] { "param" };

            sqlGenerator.AppendInsertCommand(
                stringBuilder, tableName, new Dictionary<string, string> { { columnNames[0], parameters[0] } });

            Assert.Equal(
                "INSERT INTO table (col) VALUES (param)",
                stringBuilder.ToString());

            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendInsertCommandHeader(stringBuilder, tableName, columnNames), Times.Once());
            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendValues(stringBuilder, parameters), Times.Once());
        }

        [Fact]
        public void AppendDeleteCommandHeader_appends_correct_command_header()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
                .AppendDeleteCommandHeader(stringBuilder, "Table");

            Assert.Equal("DELETE FROM Table", stringBuilder.ToString());
        }

        [Fact]
        public void AppendDeleteCommand_creates_full_delete_command_text()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p1") };

            sqlGenerator.AppendDeleteCommand(stringBuilder, tableName, whereConditions);

            Assert.Equal(
                "DELETE FROM table WHERE Id = @p1",
                stringBuilder.ToString());

            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendDeleteCommandHeader(stringBuilder, tableName), Times.Once());
            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendWhereClause(stringBuilder, whereConditions), Times.Once());
        }

        [Fact]
        public void AppendUpdateCommandHeader_appends_correct_command_header()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
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
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var columnValues = new[] { new KeyValuePair<string, string>("Name", "@p1") };
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p2") };

            sqlGenerator.AppendUpdateCommand(stringBuilder, tableName, columnValues, whereConditions);

            Assert.Equal(
                "UPDATE table SET Name = @p1 WHERE Id = @p2",
                stringBuilder.ToString());

            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendUpdateCommandHeader(stringBuilder, tableName, columnValues), Times.Once());
            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendWhereClause(stringBuilder, whereConditions), Times.Once());
        }

        [Fact]
        public void AppendSelectCommandHeader_appends_correct_select_header()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
                .AppendSelectCommandHeader(stringBuilder, new[] { "Id", "Name", "ZipCode" });

            Assert.Equal("SELECT Id, Name, ZipCode", stringBuilder.ToString());
        }

        [Fact]
        public void AppednSelectCommand_creates_full_select_command_text()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var columnNames = new[] { "Id", "Name" };
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p2") };

            sqlGenerator.AppendSelectCommand(stringBuilder, tableName, columnNames, whereConditions);

            Assert.Equal(
                "SELECT Id, Name FROM table WHERE Id = @p2",
                stringBuilder.ToString());

            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendSelectCommandHeader(stringBuilder, columnNames), Times.Once());
            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendWhereClause(stringBuilder, whereConditions), Times.Once());
        }

        [Fact]
        public void AppendFromClause_appends_correct_from_clause()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
                .AppendFromClause(stringBuilder, "table");

            Assert.Equal("FROM table", stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_if_store_generated_columns_exist()
        {
            var sqlGenerator = CreateSqlGenerator();

            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var keyColumns = new[] { new KeyValuePair<string, string>("Id", "storetype") };
            var columnsToParameters =
                new Dictionary<string, string> { { "Id", "@p0" }, { "Name", "@p1" } }.ToArray();
            var storeGenerateColumns =
                new Dictionary<string, ValueGenerationStrategy> { { "TimeStamp", ValueGenerationStrategy.StoreComputed } }.ToArray();

            sqlGenerator
                .AppendInsertOperation(stringBuilder, tableName, keyColumns, columnsToParameters, storeGenerateColumns);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendInsertCommand(stringBuilder, tableName, columnsToParameters), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.CreateWhereConditionsForStoreGeneratedKeys(It.IsAny<IEnumerable<KeyValuePair<string, ValueGenerationStrategy>>>()),
                    Times.Never);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(stringBuilder, tableName,
                    It.Is<IEnumerable<string>>(cols => cols.SequenceEqual(storeGenerateColumns.Select(c => c.Key))),
                    It.Is<IEnumerable<KeyValuePair<string, string>>>(wheres => wheres.SequenceEqual(columnsToParameters.Where(k => k.Key == "Id")))),
                    Times.Once);
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_calls_into_CreateWhereConditionsForStoreGeneratedKeys_if_store_generated_keys_exist()
        {
            var storeGenKeyWheres = new KeyValuePair<string, string>[0];

            var sqlGenerator = CreateSqlGenerator();
            Mock.Get(sqlGenerator)
                .Setup(g => g.CreateWhereConditionsForStoreGeneratedKeys(It.IsAny<IEnumerable<KeyValuePair<string, ValueGenerationStrategy>>>()))
                .Returns(storeGenKeyWheres);

            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var keyColumns = new[] { new KeyValuePair<string, string>("Id", "storetype") };
            var columnsToParameters =
                new Dictionary<string, string> { { "Name", "@p1" } }.ToArray();
            var storeGenerateColumns =
                new Dictionary<string, ValueGenerationStrategy> { { "Id", ValueGenerationStrategy.StoreComputed } }.ToArray();

            sqlGenerator
                .AppendInsertOperation(stringBuilder, tableName, keyColumns, columnsToParameters, storeGenerateColumns);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendInsertCommand(stringBuilder, tableName, columnsToParameters), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(stringBuilder, tableName,
                    It.Is<IEnumerable<string>>(cols => cols.SequenceEqual(keyColumns.Select(c => c.Key))),
                    It.Is<IEnumerable<KeyValuePair<string, string>>>(wheres => wheres.SequenceEqual(storeGenKeyWheres))),
                    Times.Once);
        }

        [Fact]
        public void AppendInsertOperation_appends_only_insert_if_no_store_generated_columns_exist()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var keyColumns = new[] { new KeyValuePair<string, string>("Id", "storetype") };
            var columnsToParameters =
                new Dictionary<string, string> { { "Id", "@p0" }, { "Name", "@p1" } }.ToArray();
            var storeGenerateColumns = new KeyValuePair<string, ValueGenerationStrategy>[0];

            sqlGenerator
                .AppendInsertOperation(stringBuilder, tableName, keyColumns, columnsToParameters, storeGenerateColumns);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendInsertCommand(stringBuilder, tableName, columnsToParameters), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(
                    It.IsAny<StringBuilder>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(),
                    It.IsAny<IEnumerable<KeyValuePair<string, string>>>()),
                    Times.Never);
        }

        [Fact]
        public void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            const string tableName = "table";

            var columnValues = new[] { new KeyValuePair<string, string>("Name", "@p1") };
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p2") };
            var storeGeneratedNonKeyColumns = new[] { "LastUpdate" };

            sqlGenerator
                .AppendUpdateOperation(stringBuilder, tableName, columnValues, whereConditions, storeGeneratedNonKeyColumns);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendUpdateCommand(stringBuilder, tableName, columnValues, whereConditions), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(
                    stringBuilder, tableName,
                    It.Is<IEnumerable<string>>(columnNames => columnNames.SequenceEqual(storeGeneratedNonKeyColumns)),
                    whereConditions), Times.Once);
        }

        [Fact]
        public void AppendUpdateOperation_does_not_append_select_if_store_generated_columns_dont_exist()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            const string tableName = "table";

            var columnValues = new[] { new KeyValuePair<string, string>("Name", "@p1") };
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p2") };
            var storeGeneratedNonKeyColumns = new string[0];

            sqlGenerator
                .AppendUpdateOperation(stringBuilder, tableName, columnValues, whereConditions, storeGeneratedNonKeyColumns);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendUpdateCommand(stringBuilder, tableName, columnValues, whereConditions), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(It.IsAny<StringBuilder>(), It.IsAny<string>(),
                    It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()),
                    Times.Never);
        }

        [Fact]
        public void AppendValues_appends_correct_values()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
                .AppendValues(stringBuilder, new[] { "@p1", "@p2" });

            Assert.Equal(
                "VALUES (@p1, @p2)",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendWhereClause_appends_where_clause()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
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

        [Fact]
        public void Default_BatchCommandSeparator_is_semicolon()
        {
            Assert.Equal(";", CreateSqlGenerator().BatchCommandSeparator);
        }

        public class ParameterValidation
        {
            [Fact]
            public void AppendInsertOperation_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertOperation(null, "table", new[] { new KeyValuePair<string, string>("Id", "smallint") },
                                new KeyValuePair<string, string>[0], new KeyValuePair<string, ValueGenerationStrategy>[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertOperation(new StringBuilder(), null, new[] { new KeyValuePair<string, string>("Id", "smallint") },
                                new KeyValuePair<string, string>[0], new KeyValuePair<string, ValueGenerationStrategy>[0])).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendInsertOperation(new StringBuilder(), string.Empty, new[] { new KeyValuePair<string, string>("Id", "smallint") },
                                new KeyValuePair<string, string>[0], new KeyValuePair<string, ValueGenerationStrategy>[0])).Message);

                Assert.Equal(
                    "keyColumns",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertOperation(new StringBuilder(), "table", null, new KeyValuePair<string, string>[0],
                                new KeyValuePair<string, ValueGenerationStrategy>[0])).ParamName);

                Assert.Equal(
                    "columnsToParameters",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertOperation(new StringBuilder(), "table", new[] { new KeyValuePair<string, string>("Id", "smallint") },
                                null, new KeyValuePair<string, ValueGenerationStrategy>[0])).ParamName);

                Assert.Equal(
                    "storeGeneratedColumns",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertOperation(new StringBuilder(), "table", new[] { new KeyValuePair<string, string>("Id", "smallint") },
                                new KeyValuePair<string, string>[0], null)).ParamName);
            }

            [Fact]
            public void AppendInsertCommand_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommand(null, "table", new Dictionary<string, string>())).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommand(new StringBuilder(), null, new Dictionary<string, string>())).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommand(new StringBuilder(), string.Empty, new Dictionary<string, string>())).Message);

                Assert.Equal(
                    "columnsToParameters",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommand(new StringBuilder(), "table", null)).ParamName);
            }

            [Fact]
            public void AppendInsertCommandHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommandHeader(null, "table", new string[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommandHeader(new StringBuilder(), null, new string[0])).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommandHeader(new StringBuilder(), string.Empty, new string[0])).Message);

                Assert.Equal(
                    "columnNames",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommandHeader(new StringBuilder(), "table", null)).ParamName);
            }

            [Fact]
            public void AppendDeleteCommand_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommand(null, "table", new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommand(new StringBuilder(), null, new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommand(new StringBuilder(), string.Empty, new KeyValuePair<string, string>[0])).Message);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommand(new StringBuilder(), "table", null)).ParamName);
            }

            [Fact]
            public void AppendDeleteCommandHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommandHeader(null, "table")).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommandHeader(new StringBuilder(), null)).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommand(new StringBuilder(), string.Empty, new KeyValuePair<string, string>[0])).Message);
            }

            [Fact]
            public void AppendUpdateOperstion_validates_paramters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(null, "table", new KeyValuePair<string, string>[0],
                                new KeyValuePair<string, string>[0], new string[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(new StringBuilder(), null, new KeyValuePair<string, string>[0],
                                new KeyValuePair<string, string>[0], new string[0])).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(new StringBuilder(), string.Empty, new KeyValuePair<string, string>[0],
                                new KeyValuePair<string, string>[0], new string[0])).Message);

                Assert.Equal(
                    "columnValues",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(new StringBuilder(), "table", null,
                                new KeyValuePair<string, string>[0], new string[0])).ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(new StringBuilder(), "table", new KeyValuePair<string, string>[0],
                                null, new string[0])).ParamName);

                Assert.Equal(
                    "storeGeneratedNonKeyColumns",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(new StringBuilder(), "table", new KeyValuePair<string, string>[0],
                                new KeyValuePair<string, string>[0], null)).ParamName);
            }

            [Fact]
            public void AppendUpdateCommand_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(null, "table", new KeyValuePair<string, string>[0], new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(new StringBuilder(), null, new KeyValuePair<string, string>[0], new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(new StringBuilder(), string.Empty, new KeyValuePair<string, string>[0], new KeyValuePair<string, string>[0])).Message);

                Assert.Equal(
                    "columnValues",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(new StringBuilder(), "table", null, new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(new StringBuilder(), "table", new KeyValuePair<string, string>[0], null)).ParamName);
            }

            [Fact]
            public void AppendUpdateCommandHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommandHeader(null, "table", new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommandHeader(new StringBuilder(), null, new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommandHeader(new StringBuilder(), string.Empty, new KeyValuePair<string, string>[0])).Message);

                Assert.Equal(
                    "columnValues",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommandHeader(new StringBuilder(), "table", null)).ParamName);
            }

            [Fact]
            public void AppendSelectCommand_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(null, "table", new string[0], new KeyValuePair<string, string>[0]))
                        .ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(new StringBuilder(), null, new string[0], new KeyValuePair<string, string>[0]))
                        .ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(new StringBuilder(), string.Empty, new string[0], new KeyValuePair<string, string>[0]))
                        .Message);

                Assert.Equal(
                    "columnNames",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(new StringBuilder(), "table", null, new KeyValuePair<string, string>[0]))
                        .ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(new StringBuilder(), "table", new string[0], null))
                        .ParamName);
            }

            [Fact]
            public void AppendSelectCommandHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommandHeader(null, new string[0])).ParamName);

                Assert.Equal(
                    "columnNames",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommandHeader(new StringBuilder(), null)).ParamName);
            }

            [Fact]
            public void AppendFromClause_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendFromClause(null, "table")).ParamName);

                Assert.Equal(
                    "tableName",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendFromClause(new StringBuilder(), null)).ParamName);

                Assert.Equal(
                    Strings.FormatArgumentIsEmpty("tableName"),
                    Assert.Throws<ArgumentException>(
                        () => CreateSqlGenerator()
                            .AppendFromClause(new StringBuilder(), string.Empty)).Message);
            }

            [Fact]
            public void AppendValues_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendValues(null, new string[0])).ParamName);

                Assert.Equal(
                    "valueParameterNames",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendValues(new StringBuilder(), null)).ParamName);
            }

            [Fact]
            public void AppendWhereClause_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendWhereClause(null, new KeyValuePair<string, string>[0])).ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendWhereClause(new StringBuilder(), null)).ParamName);
            }
        }

        private static SqlGenerator CreateSqlGenerator()
        {
            return new Mock<SqlGenerator> { CallBase = true }.Object;
        }
    }
}
