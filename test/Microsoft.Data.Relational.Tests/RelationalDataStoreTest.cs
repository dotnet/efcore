// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.Data.Relational.Model;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Microsoft.Data.Relational
{
    public class RelationalDataStoreTest
    {
        [Fact]
        public void AppendInsertCommandHeader_appends_correct_insert_command_header()
        {
            var stringBuilder = new StringBuilder();

            new RelationalDataStoreInvoker()
                .InvokeAppendInsertCommandHeader(stringBuilder, "Table", new[] { "Id, CustomerName" });

            Assert.Equal(
                "INSERT INTO Table (Id, CustomerName)",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendInsertCommand_creates_full_insert_command_text()
        {
            var mockDataStore = new Mock<RelationalDataStoreInvoker> { CallBase = true };

            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var columnNames = new[] { "col" };
            var parameters = new[] { "param" };

            mockDataStore.Object.InvokeAppendInsertCommand(stringBuilder, tableName, columnNames, parameters);

            Assert.Equal(
                "INSERT INTO table (col) VALUES (param)",
                stringBuilder.ToString());

            mockDataStore.Protected()
                .Verify("AppendInsertCommandHeader", Times.Once(), stringBuilder, tableName, columnNames);
            mockDataStore.Protected()
                .Verify("AppendValues", Times.Once(), stringBuilder, parameters);
        }

        [Fact]
        public void AppendDeleteCommandHeader_appends_correct_command_header()
        {
            var stringBuilder = new StringBuilder();

            new RelationalDataStoreInvoker()
                .InvokeAppendDeleteCommandHeader(stringBuilder, "Table");

            Assert.Equal("DELETE FROM Table", stringBuilder.ToString());
        }

        [Fact]
        public void AppendDeleteCommand_creates_full_delete_command_text()
        {
            var mockDataStore = new Mock<RelationalDataStoreInvoker> { CallBase = true };

            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p1") };

            mockDataStore.Object.InvokeAppendDeleteCommand(stringBuilder, tableName, whereConditions);

            Assert.Equal(
                "DELETE FROM table WHERE Id = @p1",
                stringBuilder.ToString());

            mockDataStore.Protected()
                .Verify("AppendDeleteCommandHeader", Times.Once(), stringBuilder, tableName);
            mockDataStore.Protected()
                .Verify("AppendWhereClause", Times.Once(), stringBuilder, whereConditions);
        }

        [Fact]
        public void AppendUpdateCommandHeader_appends_correct_command_header()
        {
            var stringBuilder = new StringBuilder();

            new RelationalDataStoreInvoker()
                .InvokeAppendUpdateCommandHeader(stringBuilder, "Table",
                new []
                    {
                        new KeyValuePair<string, string>("Col1", "@p1"),
                        new KeyValuePair<string, string>("Name", "@p2"),
                    });

            Assert.Equal("UPDATE Table SET Col1 = @p1, Name = @p2", stringBuilder.ToString());
        }

        [Fact]
        public void AppendUpdateCommand_creates_full_delete_command_text()
        {
            var mockDataStore = new Mock<RelationalDataStoreInvoker> { CallBase = true };

            var stringBuilder = new StringBuilder();
            const string tableName = "table";
            var columnValues = new[] { new KeyValuePair<string, string>("Name", "@p1") };
            var whereConditions = new[] { new KeyValuePair<string, string>("Id", "@p2") };

            mockDataStore.Object.InvokeAppendUpdateCommand(stringBuilder, tableName, columnValues, whereConditions);

            Assert.Equal(
                "UPDATE table SET Name = @p1 WHERE Id = @p2",
                stringBuilder.ToString());

            mockDataStore.Protected()
                .Verify("AppendUpdateCommandHeader", Times.Once(), stringBuilder, tableName, columnValues);
            mockDataStore.Protected()
                .Verify("AppendWhereClause", Times.Once(), stringBuilder, whereConditions);
        }

        [Fact]
        public void AppendValues_appends_correct_values()
        {
            var stringBuilder = new StringBuilder();

            new RelationalDataStoreInvoker()
                .InvokeAppendValues(stringBuilder, new[] { "@p1", "@p2" });

            Assert.Equal(
                "VALUES (@p1, @p2)",
                stringBuilder.ToString());
        }

        [Fact]
        public void AppendWhereClause_appends_where_clause()
        {
            var stringBuilder = new StringBuilder();

            new RelationalDataStoreInvoker()
                .InvokeAppendWhereClause(
                    stringBuilder, 
                    new []
                    {
                        new KeyValuePair<string, string>("Id", "@p1"),
                        new KeyValuePair<string, string>("Col2", "@p2"),
                        new KeyValuePair<string, string>("Version", "@p3"),
                    });

            Assert.Equal(
                "WHERE Id = @p1 AND Col2 = @p2 AND Version = @p3",
                stringBuilder.ToString());
        }

        public class RelationalDataStoreInvoker : RelationalDataStore
        {
            public RelationalDataStoreInvoker()
                : base("fakeConnString")
            {
            }

            public void InvokeAppendInsertCommand(
                StringBuilder stringBuilder, string tableName, IEnumerable<string> columnNames, IEnumerable<string> valueParameterNames)
            {
                AppendInsertCommand(stringBuilder, tableName, columnNames, valueParameterNames);
            }

            public void InvokeAppendInsertCommandHeader(StringBuilder commandStringBuilder, string tableName, IEnumerable<string> columnNames)
            {
                AppendInsertCommandHeader(commandStringBuilder, tableName, columnNames);
            }

            public void InvokeAppendDeleteCommand(StringBuilder stringBuilder, string tableName, IEnumerable<KeyValuePair<string, string>> valueParameterNames)
            {
                AppendDeleteCommand(stringBuilder, tableName, valueParameterNames);
            }

            public void InvokeAppendDeleteCommandHeader(StringBuilder commandStringBuilder, string tableName)
            {
                AppendDeleteCommandHeader(commandStringBuilder, tableName);
            }

            public void InvokeAppendUpdateCommand(StringBuilder commandStringBuilder, string tableName,
                IEnumerable<KeyValuePair<string, string>> columnValues,
                IEnumerable<KeyValuePair<string, string>> whereConditions)
            {
                AppendUpdateCommand(commandStringBuilder, tableName, columnValues, whereConditions);
            }

            public void InvokeAppendUpdateCommandHeader(StringBuilder commandStringBuilder, string tableName, IEnumerable<KeyValuePair<string, string>> columnValues)
            {
                AppendUpdateCommandHeader(commandStringBuilder, tableName, columnValues);
            }

            public void InvokeAppendValues(StringBuilder commandStringBuilder, IEnumerable<string> valueParameterNames)
            {
                AppendValues(commandStringBuilder, valueParameterNames);
            }

            public void InvokeAppendWhereClause(StringBuilder commandStringBuilder, IEnumerable<KeyValuePair<string, string>> whereConditions)
            {
                AppendWhereClause(commandStringBuilder, whereConditions);
            }
        }

    }
}
