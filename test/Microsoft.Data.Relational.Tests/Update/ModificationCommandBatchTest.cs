// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Xunit;

namespace Microsoft.Data.Relational.Update
{
    public class ModificationCommandBatchTest
    {
        [Fact]
        public void CompileBatch_compiles_inserts()
        {
            var batch = 
                CreateCommandBatch(
                    "Table",
                    new Dictionary<string, object> { { "Id", 42 }, { "Name", "Test" } }.ToArray(),
                    null);

            List<KeyValuePair<string, object>> parameters;
            var sql = batch.CompileBatch(CreateMockSqlGenerator(), out parameters);

            Assert.Equal("BatchHeader$\r\nINSERT;Table;Id,Name;@p0,@p1$", sql.Trim());
            Assert.Equal(new Dictionary<string, object> { { "@p0", 42 }, { "@p1", "Test" } }, parameters);
        }

        [Fact]
        public void CompileBatch_compiles_updates()
        {
            var batch = 
                CreateCommandBatch(
                    "Table",
                    new Dictionary<string, object> { { "Name", "Test" } }.ToArray(),
                    new Dictionary<string, object> { { "Id1", 42 }, { "Id2", 43 } }.ToArray());

            List<KeyValuePair<string, object>> parameters;
            var sql = batch.CompileBatch(CreateMockSqlGenerator(), out parameters);

            Assert.Equal("BatchHeader$\r\nUPDATE;Table;Name=@p0;Id1=@p1,Id2=@p2$", sql.Trim());
            Assert.Equal(new Dictionary<string, object> { { "@p0", "Test" }, { "@p1", 42 }, { "@p2", 43 } }, parameters);
        }

        [Fact]
        public void CompileBatch_compiles_deletes()
        {
            var batch =
                CreateCommandBatch(
                    "Table",
                    null,
                    new Dictionary<string, object> { { "Id1", 42 }, { "Id2", 43 } }.ToArray());

            List<KeyValuePair<string, object>> parameters;
            var sql = batch.CompileBatch(CreateMockSqlGenerator(), out parameters);

            Assert.Equal("BatchHeader$\r\nDELETE;Table;Id1=@p0,Id2=@p1$", sql.Trim());
            Assert.Equal(new Dictionary<string, object> { { "@p0", 42 }, { "@p1", 43 } }, parameters);
        }

        [Fact]
        public void Batch_separator_not_appended_if_batch_header_empty()
        {
            var batch =
                CreateCommandBatch(
                    "Table", null, new Dictionary<string, object> { { "Id1", 42 } }.ToArray());

            List<KeyValuePair<string, object>> parameters;
            var sql = batch.CompileBatch(new SqlGenerator(), out parameters);

            Assert.True(sql.StartsWith("DELETE"));
        }

        private static SqlGenerator CreateMockSqlGenerator()
        {
            var mockSqlGen = new Mock<SqlGenerator>();

            mockSqlGen
                .Setup(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()))
                .Callback((StringBuilder sb) => sb.Append("BatchHeader"));

            mockSqlGen
                .Setup(
                    g => g.AppendInsertCommand(
                        It.IsAny<StringBuilder>(), It.IsAny<string>(),
                        It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
                .Callback(
                    (StringBuilder sb, string t, IEnumerable<string> c, IEnumerable<string> v)
                        =>
                        sb.Append("INSERT").Append(";")
                            .Append(t).Append(";")
                            .Append(string.Join(",", c)).Append(";")
                            .Append(string.Join(",", v)));

            mockSqlGen
                .Setup(
                    g => g.AppendUpdateCommand(
                        It.IsAny<StringBuilder>(), It.IsAny<string>(),
                        It.IsAny<IEnumerable<KeyValuePair<string, string>>>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
                .Callback(
                    (StringBuilder sb, string t, IEnumerable<KeyValuePair<string, string>> cols, IEnumerable<KeyValuePair<string, string>> wheres)
                        =>
                        sb.Append("UPDATE").Append(";")
                            .Append(t).Append(";")
                            .Append(string.Join(",", cols.Select(c => c.Key + "=" + c.Value))).Append(";")
                            .Append(string.Join(",", wheres.Select(c => c.Key + "=" + c.Value))));

            mockSqlGen
                .Setup(
                    g => g.AppendDeleteCommand(
                        It.IsAny<StringBuilder>(), It.IsAny<string>(),
                        It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
                .Callback(
                    (StringBuilder sb, string t, IEnumerable<KeyValuePair<string, string>> wheres)
                        =>
                        sb.Append("DELETE").Append(";")
                            .Append(t).Append(";")
                            .Append(string.Join(",", wheres.Select(c => c.Key + "=" + c.Value))));

            mockSqlGen.Setup(g => g.BatchCommandSeparator).Returns("$");

            return mockSqlGen.Object;
        }

        private static ModificationCommandBatch CreateCommandBatch(string tableName,
            KeyValuePair<string, object>[] columnValues, KeyValuePair<string, object>[] whereClauses)
        {
            var mockModificationCommand = new Mock<ModificationCommand>();
            mockModificationCommand
                .Setup(c => c.TableName)
                .Returns(tableName);

            mockModificationCommand
                .Setup(c => c.ColumnValues)
                .Returns(columnValues);

            mockModificationCommand
                .Setup(c => c.WhereClauses)
                .Returns(whereClauses);

            mockModificationCommand
                .Setup(c => c.Operation)
                .Returns(
                    columnValues == null
                        ? ModificationOperation.Delete
                        : whereClauses == null
                            ? ModificationOperation.Insert
                            : ModificationOperation.Update);

            return
                new ModificationCommandBatch(new[] { mockModificationCommand.Object });
        }
    }
}
