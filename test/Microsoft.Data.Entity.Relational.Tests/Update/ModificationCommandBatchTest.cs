// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Update;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class ModificationCommandBatchTest
    {
        [Fact]
        public void CompileBatch_compiles_inserts()
        {
            var batch =
                CreateCommandBatch(
                    new Table("Table"),
                    new Dictionary<Column, object>
                        {
                            { new Column("Id", "_"), 42 }, { new Column("Name", "_"), "Test" }
                        }.ToArray(),
                    null);

            List<KeyValuePair<string, object>> parameters;
            var sql = batch.CompileBatch(CreateMockSqlGenerator(), out parameters);

            Assert.Equal("BatchHeader$\r\nINSERT;Table;Id,Name;@p0,@p1$", sql.Trim());
            Assert.Equal(new Dictionary<string, object> { { "@p0", 42 }, { "@p1", "Test" } }, parameters);
        }

        [Fact]
        public void CompileBatch_compiles_updates()
        {
            var id1Column = new Column("Id1", "int") { ValueGenerationStrategy = StoreValueGenerationStrategy.Identity };
            var table = new Table("Table", new[] { id1Column }) { PrimaryKey = new PrimaryKey("PK", new[] { id1Column }) };

            var batch =
                CreateCommandBatch(
                    table,
                    new Dictionary<Column, object> { { new Column("Name", "_"), "Test" } }.ToArray(),
                    new Dictionary<Column, object>
                        {
                            { new Column("Id1", "_"), 42 }, { new Column("Id2", "_"), 43 }
                        }.ToArray());

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
                    new Table("Table"),
                    null,
                    new Dictionary<Column, object>
                        {
                            { new Column("Id1", "_"), 42 }, { new Column("Id2", "_"), 43 }
                        }.ToArray());

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
                    new Table("Table"), null, new Dictionary<Column, object> { { new Column("Id1", "_"), 42 } }.ToArray());

            List<KeyValuePair<string, object>> parameters;
            var sql = batch.CompileBatch(new Mock<SqlGenerator> { CallBase = true }.Object, out parameters);

            Assert.True(sql.StartsWith("DELETE"));
        }

        private static SqlGenerator CreateMockSqlGenerator()
        {
            var mockSqlGen = new Mock<SqlGenerator>() { CallBase = true };

            mockSqlGen
                .Setup(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()))
                .Callback((StringBuilder sb) => sb.Append("BatchHeader"));

            mockSqlGen
                .Setup(
                    g => g.AppendInsertCommand(
                        It.IsAny<StringBuilder>(), It.IsAny<Table>(),
                        It.IsAny<IEnumerable<KeyValuePair<Column, string>>>()))
                .Callback(
                    (StringBuilder sb, Table t, IEnumerable<KeyValuePair<Column, string>> colParams)
                        =>
                        sb.Append("INSERT").Append(";")
                            .Append(t.Name).Append(";")
                            .Append(string.Join(",", colParams.Select(c => c.Key.Name))).Append(";")
                            .Append(string.Join(",", colParams.Select(c => c.Value))));

            mockSqlGen
                .Setup(
                    g => g.AppendUpdateCommand(
                        It.IsAny<StringBuilder>(), It.IsAny<Table>(),
                        It.IsAny<IEnumerable<KeyValuePair<Column, string>>>(), It.IsAny<IEnumerable<KeyValuePair<Column, string>>>()))
                .Callback(
                    (StringBuilder sb, Table t, IEnumerable<KeyValuePair<Column, string>> cols, IEnumerable<KeyValuePair<Column, string>> wheres)
                        =>
                        sb.Append("UPDATE").Append(";")
                            .Append(t.Name).Append(";")
                            .Append(string.Join(",", cols.Select(c => c.Key.Name + "=" + c.Value))).Append(";")
                            .Append(string.Join(",", wheres.Select(c => c.Key.Name + "=" + c.Value))));

            mockSqlGen
                .Setup(
                    g => g.AppendDeleteCommand(
                        It.IsAny<StringBuilder>(), It.IsAny<Table>(),
                        It.IsAny<IEnumerable<KeyValuePair<Column, string>>>()))
                .Callback(
                    (StringBuilder sb, Table t, IEnumerable<KeyValuePair<Column, string>> wheres)
                        =>
                        sb.Append("DELETE").Append(";")
                            .Append(t.Name).Append(";")
                            .Append(string.Join(",", wheres.Select(c => c.Key.Name + "=" + c.Value))));

            mockSqlGen.Setup(g => g.BatchCommandSeparator).Returns("$");

            return mockSqlGen.Object;
        }

        private static ModificationCommandBatch CreateCommandBatch(Table table,
            KeyValuePair<Column, object>[] columnValues, KeyValuePair<Column, object>[] whereClauses)
        {
            var mockModificationCommand = new Mock<ModificationCommand>();
            mockModificationCommand
                .Setup(c => c.Table)
                .Returns(table);

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

            return new ModificationCommandBatch(new[] { mockModificationCommand.Object });
        }
    }
}
