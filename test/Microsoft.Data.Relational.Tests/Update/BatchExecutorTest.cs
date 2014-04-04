// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Relational.Update;
using Moq;
using Moq.Protected;
using Xunit;
using Microsoft.Data.Relational.Model;

namespace Microsoft.Data.Relational.Tests.Update
{
    public class BatchExecutorTest
    {
        [Fact]
        public async void ExecuteAsync_executes_batch_commands_and_consumes_reader()
        {
            var mockModificationCommand = new Mock<ModificationCommand>();
            mockModificationCommand.Setup(c => c.Table).Returns(new Table("table"));
            mockModificationCommand.Setup(c => c.ColumnValues)
                .Returns(new[] { new KeyValuePair<Column, object>(new Column("Id", "_"), 1) });
            mockModificationCommand.Setup(c => c.WhereClauses).Returns(new KeyValuePair<Column, object>[0]);

            var batch = new ModificationCommandBatch(new[] { mockModificationCommand.Object });

            var executor = new BatchExecutor(new[] { batch }, new Mock<SqlGenerator> { CallBase = true }.Object);

            var connection = SetupMockConnection(SetupMockDataReader(1).Object);

            await executor.ExecuteAsync(connection, CancellationToken.None);

            var mockReader = Mock.Get(await connection.CreateCommand().ExecuteReaderAsync());

            mockReader
                .Verify(r => r.ReadAsync(It.IsAny<CancellationToken>()), Times.Once);

            mockReader
                .Verify(r => r.NextResultAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void ExecuteAsync_saves_store_generated_values()
        {
            var mockModificationCommand = new Mock<ModificationCommand>();
            mockModificationCommand.Setup(c => c.Table).Returns(new Table("table"));
            mockModificationCommand.Setup(c => c.ColumnValues)
                .Returns(new[] { new KeyValuePair<Column, object>(new Column("Id", "_"), 1) });
            mockModificationCommand.Setup(c => c.WhereClauses).Returns(new KeyValuePair<Column, object>[0]);
            mockModificationCommand.Setup(c => c.RequiresResultPropagation).Returns(true);

            var mockBatch = 
                new Mock<ModificationCommandBatch>(
                    new object[] { new[] { mockModificationCommand.Object } }) 
                    { CallBase = true };

            var executor = new BatchExecutor(new[] { mockBatch.Object }, new Mock<SqlGenerator> { CallBase = true }.Object);

            var connection =
                SetupMockConnection(
                    SetupMockDataReader(1, new[] { "Id" }, new List<object[]> { new object[] { 42 } }).Object);

            await executor.ExecuteAsync(connection, CancellationToken.None);

            mockBatch.Verify(m => m.SaveStoreGeneratedValues(0,
                It.Is<KeyValuePair<string, object>[]>(
                    values => values.Length == 1 && values[0].Key == "Id" && (int)values[0].Value == 42)));
        }

        [Fact]
        public async void Exception_thrown_for_more_than_one_row_returned_for_single_command()
        {
            var mockModificationCommand = new Mock<ModificationCommand>();
            mockModificationCommand.Setup(c => c.Table).Returns(new Table("table"));
            mockModificationCommand.Setup(c => c.ColumnValues)
                .Returns(new[] { new KeyValuePair<Column, object>(new Column("Id", "_"), 1) });
            mockModificationCommand.Setup(c => c.WhereClauses).Returns(new KeyValuePair<Column, object>[0]);
            mockModificationCommand.Setup(c => c.RequiresResultPropagation).Returns(true);

            var batch = new ModificationCommandBatch(new[] { mockModificationCommand.Object });

            var executor = new BatchExecutor(new[] { batch }, new Mock<SqlGenerator> { CallBase = true }.Object);

            var connection =
                SetupMockConnection(
                    SetupMockDataReader(1, new[] { "Id" }, new List<object[]>
                        {
                            new object[] { 42 },
                            new object[] { -42 }
                        }).Object);

            Assert.Equal(Strings.TooManyRowsForModificationCommand,
                (await Assert.ThrowsAsync<DbUpdateException>(
                    async () => await executor.ExecuteAsync(connection, CancellationToken.None))).Message);
        }

        [Fact]
        public async void Exception_thrown_if_rows_returned_for_command_without_store_generated_values()
        {
            var mockModificationCommand = new Mock<ModificationCommand>();
            mockModificationCommand.Setup(c => c.Table).Returns(new Table("table"));
            mockModificationCommand.Setup(c => c.ColumnValues)
                .Returns(new[] { new KeyValuePair<Column, object>(new Column("Id", "_"), 1) });
            mockModificationCommand.Setup(c => c.WhereClauses).Returns(new KeyValuePair<Column, object>[0]);
            mockModificationCommand.Setup(c => c.RequiresResultPropagation).Returns(false);

            var batch = new ModificationCommandBatch(new[] { mockModificationCommand.Object });

            var executor = new BatchExecutor(new[] { batch }, new Mock<SqlGenerator> { CallBase = true }.Object);

            var connection =
                SetupMockConnection(
                    SetupMockDataReader(1, new[] { "Id" }, new List<object[]> { new object[] { 42 } }).Object);

            Assert.Equal(Strings.FormatUpdateConcurrencyException(0, 1),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await executor.ExecuteAsync(connection, CancellationToken.None))).Message);
        }

        [Fact]
        public async void Exception_thrown_if_no_rows_returned_for_command_with_store_generated_values()
        {
            var mockModificationCommand = new Mock<ModificationCommand>();
            mockModificationCommand.Setup(c => c.Table).Returns(new Table("table"));
            mockModificationCommand.Setup(c => c.ColumnValues)
                .Returns(new[] { new KeyValuePair<Column, object>(new Column("Id", "_"), 1) });
            mockModificationCommand.Setup(c => c.WhereClauses).Returns(new KeyValuePair<Column, object>[0]);
            mockModificationCommand.Setup(c => c.RequiresResultPropagation).Returns(true);

            var batch = new ModificationCommandBatch(new[] { mockModificationCommand.Object });

            var executor = new BatchExecutor(new[] { batch }, new Mock<SqlGenerator> { CallBase = true }.Object);

            var connection = SetupMockConnection(SetupMockDataReader(0).Object);

            Assert.Equal(Strings.FormatUpdateConcurrencyException(1, 0),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await executor.ExecuteAsync(connection, CancellationToken.None))).Message);
        }

        private DbConnection SetupMockConnection(DbDataReader dataReader)
        {
            var mockConnection = new Mock<DbConnection>();
            var mockCommand = new Mock<DbCommand>();
            mockConnection
                .Protected()
                .Setup<DbCommand>("CreateDbCommand")
                .Returns(mockCommand.Object);
            mockCommand
                .Protected()
                .Setup<DbParameter>("CreateDbParameter")
                .Returns(Mock.Of<DbParameter>());
            mockCommand
                .Protected()
                .SetupGet<DbParameterCollection>("DbParameterCollection")
                .Returns(Mock.Of<DbParameterCollection>());

            var tcs = new TaskCompletionSource<DbDataReader>();
            tcs.SetResult(dataReader);

            mockCommand
                .Protected()
                .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                .Returns(tcs.Task);

            return mockConnection.Object;
        }

        private static Mock<DbDataReader> SetupMockDataReader(int recordsAffected, string[] columnNames = null, IList<object[]> results = null)
        {
            var mockDataReader = new Mock<DbDataReader>();
            mockDataReader
                .Setup(r => r.RecordsAffected)
                .Returns(recordsAffected);

            if (results != null && columnNames != null)
            {
                var rowIndex = 0;
                object[] currentRow = null;

                mockDataReader.Setup(r => r.FieldCount).Returns(columnNames.Length);
                mockDataReader.Setup(r => r.GetName(It.IsAny<int>())).Returns((int columnIdx) => columnNames[columnIdx]);
                mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns((int columnIdx) => currentRow[columnIdx]);

                mockDataReader
                    .Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                    .Callback(() => currentRow = rowIndex < results.Count ? results[rowIndex++] : null)
                    .Returns(() =>
                        {
                            var tcs = new TaskCompletionSource<bool>();
                            tcs.SetResult(currentRow != null);
                            return tcs.Task;
                        });
            }

            return mockDataReader;
        }
    }
}
