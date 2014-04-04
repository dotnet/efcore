// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Data.Relational.Update;
using Microsoft.Data.Relational.Model;
using Moq;
using Xunit;

namespace Microsoft.Data.Relational.Tests.Update
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

        [Fact]
        public void Cannot_save_store_generated_results_multiple_times()
        {
            var mockCommand = new Mock<ModificationCommand>();
            mockCommand.Setup(c => c.RequiresResultPropagation).Returns(true);

            var batch = new ModificationCommandBatch(new[] { mockCommand.Object });
            batch.SaveStoreGeneratedValues(0, new KeyValuePair<string, object>[0]);

            Assert.Equal(Strings.StoreGenValuesSavedMultipleTimesForCommand,
                Assert.Throws<InvalidOperationException>(
                    () => batch.SaveStoreGeneratedValues(0, new KeyValuePair<string, object>[0])).Message);
        }

        [Fact]
        public void SaveStoreGeneratedValues_validates_parameter()
        {
            Assert.Equal("storeGeneratedValues",
                Assert.Throws<ArgumentNullException>(
                    () => new ModificationCommandBatch(new ModificationCommand[1])
                        .SaveStoreGeneratedValues(0, null)).ParamName);
        }

        [Fact]
        public void Propagate_results_propagates_results_for_commands()
        {
            var mockCommand1 = new Mock<ModificationCommand>();
            mockCommand1.Setup(c => c.RequiresResultPropagation).Returns(true);
            var values1 = new[] { new KeyValuePair<string, object>("Col1", 42) };

            var mockCommand2 = new Mock<ModificationCommand>();
            mockCommand2.Setup(c => c.RequiresResultPropagation).Returns(true);
            var values2 = new[] { new KeyValuePair<string, object>("1loC", -42) };

            var batch = new ModificationCommandBatch(new[] { mockCommand1.Object, mockCommand2.Object });
            batch.SaveStoreGeneratedValues(0, values1);
            batch.SaveStoreGeneratedValues(1, values2);
            batch.PropagateResults();

            mockCommand1.Verify(c => c.PropagateResults(values1), Times.Once);
            mockCommand2.Verify(c => c.PropagateResults(values2), Times.Once);
        }

        [Fact]
        public void Propagate_results_throws_when_propagating_results_for_commands_without_store_generated_values()
        {
            var mockCommand = new Mock<ModificationCommand>();
            mockCommand.Setup(c => c.RequiresResultPropagation).Returns(false);
            mockCommand.Setup(c => c.Table).Returns(new Table("table"));

            var batch = new ModificationCommandBatch(new[] { mockCommand.Object });
            batch.SaveStoreGeneratedValues(0, new[] { new KeyValuePair<string, object>("Col1", 42) });
            Assert.Equal(Strings.FormatNoStoreGenColumnsToPropagateResults("table"),
                Assert.Throws<InvalidOperationException>(() => batch.PropagateResults()).Message);
        }

        [Fact]
        public void Propagate_results_throws_when_results_not_propagated_for_commands_with_store_generated_values()
        {
            var mockCommand = new Mock<ModificationCommand>();
            mockCommand.Setup(c => c.RequiresResultPropagation).Returns(true);
            mockCommand.Setup(c => c.Table).Returns(new Table("table"));

            Assert.Equal(Strings.FormatResultsNotPropagatedForStoreGenColumns("table"),
                Assert.Throws<InvalidOperationException>(
                    () => new ModificationCommandBatch(new[] { mockCommand.Object }).PropagateResults()).Message);
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

            return
                new ModificationCommandBatch(new[] { mockModificationCommand.Object });
        }
    }
}
