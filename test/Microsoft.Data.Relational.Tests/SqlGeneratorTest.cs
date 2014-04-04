// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Model;
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
            var table = new Table("Table", new[] { new Column("Id", "_"), new Column("CustomerName", "_") });

            CreateSqlGenerator()
                .AppendInsertCommandHeader(stringBuilder, table, table.Columns);

            Assert.Equal(
                "INSERT INTO Table (Id, CustomerName)",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertCommand_creates_full_insert_command_text()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            var table = new Table("table", new[] {new Column("col", "_")});
            var parameters = new[] { "param" };

            sqlGenerator.AppendInsertCommand(
                stringBuilder, table, new Dictionary<Column, string> { { table.Columns.First(), "param" } });

            Assert.Equal(
                "INSERT INTO table (col) VALUES (param)",
                stringBuilder.ToString());

            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendInsertCommandHeader(stringBuilder, table, table.Columns), Times.Once());
            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendValues(stringBuilder, parameters), Times.Once());
        }

        [Fact]
        public void AppendDeleteCommandHeader_appends_correct_command_header()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
                .AppendDeleteCommandHeader(stringBuilder, new Table("Table"));

            Assert.Equal("DELETE FROM Table", stringBuilder.ToString());
        }

        [Fact]
        public void AppendDeleteCommand_creates_full_delete_command_text()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            var table = new Table("table", new[] { new Column("Id", "_") });
            var whereConditions = new[] { new KeyValuePair<Column, string>(table.Columns.Single(), "@p1") };

            sqlGenerator.AppendDeleteCommand(stringBuilder, table, whereConditions);

            Assert.Equal(
                "DELETE FROM table WHERE Id = @p1",
                stringBuilder.ToString());

            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendDeleteCommandHeader(stringBuilder, table), Times.Once());
            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendWhereClause(stringBuilder, whereConditions), Times.Once());
        }

        [Fact]
        public void AppendUpdateCommandHeader_appends_correct_command_header()
        {
            var stringBuilder = new StringBuilder();
            
            CreateSqlGenerator()
                .AppendUpdateCommandHeader(stringBuilder, new Table("Table"),
                    new[]
                        {
                            new KeyValuePair<Column, string>(new Column("Col1", "_"), "@p1"),
                            new KeyValuePair<Column, string>(new Column("Name", "_"), "@p2"),
                        });

            Assert.Equal("UPDATE Table SET Col1 = @p1, Name = @p2", stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateCommand_creates_full_delete_command_text()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            var table = new Table("table");
            var columnValues = new[] { new KeyValuePair<Column, string>(new Column("Name", "_"), "@p1") };
            var whereConditions = new[] { new KeyValuePair<Column, string>(new Column("Id", "_"), "@p2") };

            sqlGenerator.AppendUpdateCommand(stringBuilder, table, columnValues, whereConditions);

            Assert.Equal(
                "UPDATE table SET Name = @p1 WHERE Id = @p2",
                stringBuilder.ToString());

            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendUpdateCommandHeader(stringBuilder, table, columnValues), Times.Once());
            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendWhereClause(stringBuilder, whereConditions), Times.Once());
        }

        [Fact]
        public void AppendSelectCommandHeader_appends_correct_select_header()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
                .AppendSelectCommandHeader(stringBuilder, 
                    new[] { new Column("Id", "_"), new Column("Name", "_"), new Column("ZipCode", "_") });

            Assert.Equal("SELECT Id, Name, ZipCode", stringBuilder.ToString());
        }

        [Fact]
        public void AppendSelectCommand_creates_full_select_command_text()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();
            var table = new Table("table", new[] { new Column("Id", "_"), new Column("Name", "_")});
            var whereConditions = new[] { new KeyValuePair<Column, string>(table.Columns.First(), "@p2") };

            sqlGenerator.AppendSelectCommand(stringBuilder, table, table.Columns, whereConditions);

            Assert.Equal(
                "SELECT Id, Name FROM table WHERE Id = @p2",
                stringBuilder.ToString());

            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendSelectCommandHeader(stringBuilder, table.Columns), Times.Once());
            Mock.Get(sqlGenerator)
                .Verify(s => s.AppendWhereClause(stringBuilder, whereConditions), Times.Once());
        }

        [Fact]
        public void AppendFromClause_appends_correct_from_clause()
        {
            var stringBuilder = new StringBuilder();

            CreateSqlGenerator()
                .AppendFromClause(stringBuilder, new Table("table"));

            Assert.Equal("FROM table", stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_select_if_store_generated_columns_exist()
        {
            var sqlGenerator = CreateSqlGenerator();

            var stringBuilder = new StringBuilder();
            var table = new Table("table",
                new[]
                    {
                        new Column("Id", "storetype"),
                        new Column("Name", "_"),
                        new Column("Timestamp", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Computed }
                    });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Id").ToArray());

            var columnsToParameters =
                new Dictionary<Column, string>
                    {
                        { table.Columns.Single(c => c.Name == "Id"), "@p0" }, 
                        { table.Columns.Single(c => c.Name == "Name"), "@p1" } 
                    }.ToArray();

            sqlGenerator
                .AppendInsertOperation(stringBuilder, table, columnsToParameters);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendInsertCommand(stringBuilder, table, columnsToParameters), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.CreateWhereConditionsForStoreGeneratedKeys(It.IsAny<Column[]>()),
                Times.Never);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(stringBuilder, table,
                    It.Is<IEnumerable<Column>>(cols => cols.Single().Name == "Timestamp"),
                    It.Is<IEnumerable<KeyValuePair<Column, string>>>(w => w.Single().Key.Name == "Id" && w.Single().Value == "@p0")),
                    Times.Once);
        }

        [Fact]
        public void AppendInsertOperation_appends_insert_and_calls_into_CreateWhereConditionsForStoreGeneratedKeys_if_store_generated_keys_exist()
        {
            var table = new Table("table",
                new[]
                    {
                        new Column("Id", "storetype") { ValueGenerationStrategy = StoreValueGenerationStrategy.Computed}, 
                        new Column("Name", "_")
                    });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Id").ToArray());

            var storeGenKeyWheres = 
                new [] { new KeyValuePair<Column, string>(table.PrimaryKey.Columns.Single(), "abc")};

            var sqlGenerator = CreateSqlGenerator();
            Mock.Get(sqlGenerator)
                .Setup(g => g.CreateWhereConditionsForStoreGeneratedKeys(It.IsAny<Column[]>()))
                .Returns(storeGenKeyWheres);

            var stringBuilder = new StringBuilder();

            var columnsToParameters =
                new Dictionary<Column, string> { { table.Columns.Single(c => c.Name == "Name"), "@p0" } }.ToArray();

            sqlGenerator
                .AppendInsertOperation(stringBuilder, table, columnsToParameters);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendInsertCommand(stringBuilder, table, columnsToParameters), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.CreateWhereConditionsForStoreGeneratedKeys(
                     It.Is<Column[]>(cols => cols.Single().Name == "Id")),
                Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(stringBuilder, table,
                    It.Is<IEnumerable<Column>>(cols => cols.Single().Name == "Id"),
                    It.Is<IEnumerable<KeyValuePair<Column, string>>>(wheres => wheres.SequenceEqual(storeGenKeyWheres))),
                    Times.Once);
        }

        [Fact]
        public void AppendInsertOperation_appends_only_insert_if_no_store_generated_columns_exist()
        {
            var table = new Table("table", new[] { new Column("Id", "storetype"), new Column("Name", "_") });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Id").ToArray());

            var columnsToParameters =
                new Dictionary<Column, string>
                    {
                        { table.Columns.Single(c => c.Name == "Id"), "@p0" }, 
                        { table.Columns.Single(c => c.Name == "Name"), "@p1" }
                    }.ToArray();

            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();

            sqlGenerator
                .AppendInsertOperation(stringBuilder, table, columnsToParameters);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendInsertCommand(stringBuilder, table, columnsToParameters), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(
                    It.IsAny<StringBuilder>(), It.IsAny<Table>(), It.IsAny<IEnumerable<Column>>(),
                    It.IsAny<IEnumerable<KeyValuePair<Column, string>>>()),
                    Times.Never);
        }

        [Fact]
        public void AppendUpdateOperation_appends_update_and_select_if_store_generated_columns_exist()
        {
            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();

            var table = new Table("table", 
                new[]
                    {
                        new Column("Id", "storetype"), 
                        new Column("Name", "_"), 
                        new Column("LastUpdate", "_") { ValueGenerationStrategy = StoreValueGenerationStrategy.Computed }
                    });

            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Id").ToArray());

            var columnValues = 
                new[] { new KeyValuePair<Column, string>(table.Columns.Single(c => c.Name == "Name"), "@p1") };
            var whereConditions = 
                new[] { new KeyValuePair<Column, string>(table.Columns.Single(c => c.Name == "Id"), "@p2") };
            

            sqlGenerator
                .AppendUpdateOperation(stringBuilder, table, columnValues, whereConditions);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendUpdateCommand(stringBuilder, table, columnValues, whereConditions), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(
                        stringBuilder, table,
                         It.Is<IEnumerable<Column>>(cols => cols.Single().Name == "LastUpdate"),
                        whereConditions), Times.Once);
        }

        [Fact]
        public void AppendUpdateOperation_does_not_append_select_if_store_generated_columns_dont_exist()
        {
            var table = new Table("table", new[] { new Column("Id", "storetype"), new Column("Name", "_") });
            table.PrimaryKey = new PrimaryKey("PK", table.Columns.Where(c => c.Name == "Id").ToArray());

            var sqlGenerator = CreateSqlGenerator();
            var stringBuilder = new StringBuilder();

            var columnValues = new[] { new KeyValuePair<Column, string>(table.Columns.Single(c => c.Name == "Name"), "@p1") };
            var whereConditions = new[] { new KeyValuePair<Column, string>(table.PrimaryKey.Columns.Single(), "@p2") };

            sqlGenerator
                .AppendUpdateOperation(stringBuilder, table, columnValues, whereConditions);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendUpdateCommand(stringBuilder, table, columnValues, whereConditions), Times.Once);

            Mock.Get(sqlGenerator)
                .Verify(g => g.AppendSelectCommand(It.IsAny<StringBuilder>(), It.IsAny<Table>(),
                    It.IsAny<IEnumerable<Column>>(), It.IsAny<IEnumerable<KeyValuePair<Column, string>>>()),
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
                    new Dictionary<Column, string>
                        {
                            { new Column("Id", "_"), "@p1" },
                            { new Column("Col2", "_"), "@p2" },
                            { new Column("Version", "_"), "@p3" }
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
                            .AppendInsertOperation(null, new Table("table"), new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertOperation(new StringBuilder(), null, new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "columnsToParameters",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertOperation(new StringBuilder(), new Table("table"), null)).ParamName);
            }

            [Fact]
            public void AppendInsertCommand_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommand(null, new Table("table"), new Dictionary<Column, string>())).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommand(new StringBuilder(), null, new Dictionary<Column, string>())).ParamName);

                Assert.Equal(
                    "columnsToParameters",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommand(new StringBuilder(), new Table("table"), null)).ParamName);
            }

            [Fact]
            public void AppendInsertCommandHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommandHeader(null, new Table("table"), new Column[0])).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommandHeader(new StringBuilder(), null, new Column[0])).ParamName);

                Assert.Equal(
                    "columns",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendInsertCommandHeader(new StringBuilder(), new Table("table"), null)).ParamName);
            }

            [Fact]
            public void AppendDeleteCommand_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommand(null, new Table("table"), new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommand(new StringBuilder(), null, new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommand(new StringBuilder(), new Table("table"), null)).ParamName);
            }

            [Fact]
            public void AppendDeleteCommandHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommandHeader(null, new Table("table"))).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendDeleteCommandHeader(new StringBuilder(), null)).ParamName);
            }

            [Fact]
            public void AppendUpdateOperstion_validates_paramters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(null, new Table("table"), new KeyValuePair<Column, string>[0],
                                new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(new StringBuilder(), null, new KeyValuePair<Column, string>[0],
                                new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "columnValues",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(new StringBuilder(), new Table("table"), null,
                                new KeyValuePair<Column, string>[0])).ParamName);


                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateOperation(new StringBuilder(), new Table("table"),
                            new KeyValuePair<Column, string>[0], null)).ParamName);
            }

            [Fact]
            public void AppendUpdateCommand_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(null, new Table("table"), new KeyValuePair<Column, string>[0], 
                                new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(new StringBuilder(), null, new KeyValuePair<Column, string>[0],
                                new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "columnValues",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(new StringBuilder(), new Table("table"), null,
                                new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommand(new StringBuilder(), new Table("table"), new KeyValuePair<Column, string>[0],
                                null)).ParamName);
            }

            [Fact]
            public void AppendUpdateCommandHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommandHeader(null, new Table("table"), new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommandHeader(new StringBuilder(), null, new KeyValuePair<Column, string>[0])).ParamName);

                Assert.Equal(
                    "columnValues",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendUpdateCommandHeader(new StringBuilder(), new Table("table"), null)).ParamName);
            }

            [Fact]
            public void AppendSelectCommand_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(null, new Table("table"), new Column[0], new KeyValuePair<Column, string>[0]))
                                .ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(new StringBuilder(), null, new Column[0], new KeyValuePair<Column, string>[0]))
                                .ParamName);

                Assert.Equal(
                    "columns",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(new StringBuilder(), new Table("table"), null, new KeyValuePair<Column, string>[0]))
                                .ParamName);

                Assert.Equal(
                    "whereConditions",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommand(new StringBuilder(), new Table("table"), new Column[0], null))
                                .ParamName);
            }

            [Fact]
            public void AppendSelectCommandHeader_validates_parameters()
            {
                Assert.Equal(
                    "commandStringBuilder",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendSelectCommandHeader(null, new Column[0])).ParamName);

                Assert.Equal(
                    "columns",
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
                            .AppendFromClause(null, new Table("table"))).ParamName);

                Assert.Equal(
                    "table",
                    Assert.Throws<ArgumentNullException>(
                        () => CreateSqlGenerator()
                            .AppendFromClause(new StringBuilder(), null)).ParamName);
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
                            .AppendWhereClause(null, new KeyValuePair<Column, string>[0])).ParamName);

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
