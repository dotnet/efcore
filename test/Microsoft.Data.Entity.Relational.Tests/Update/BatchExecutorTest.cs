// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class BatchExecutorTest
    {
        [Fact]
        public async void ExecuteAsync_executes_batch_commands_and_consumes_reader()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });
            var mockReader = SetupMockDataReader();
            var connection = SetupMockConnection(mockReader.Object);

            var executor = new BatchExecutor(new Mock<SqlGenerator> { CallBase = true }.Object, connection, new RelationalTypeMapper());

            await executor.ExecuteAsync(new[] { batch }, CancellationToken.None);

            mockReader.Verify(r => r.ReadAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockReader.Verify(r => r.NextResultAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void ExecuteAsync_saves_store_generated_values()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationStrategy.StoreIdentity);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });
            var mockReader = SetupMockDataReader(new[] { "Col1" }, new List<object[]> { new object[] { 42 } });
            var connection = SetupMockConnection(mockReader.Object);

            var executor = new BatchExecutor(new Mock<SqlGenerator> { CallBase = true }.Object, connection, new RelationalTypeMapper());

            await executor.ExecuteAsync(new[] { batch }, CancellationToken.None);

            Assert.Equal(42, stateEntry[stateEntry.EntityType.GetProperty("Col1")]);
            Assert.Equal("Test", stateEntry[stateEntry.EntityType.GetProperty("Col2")]);
        }

        [Fact]
        public async void ExecuteAsync_saves_store_generated_values_on_non_key_columns()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Added, ValueGenerationStrategy.StoreIdentity, ValueGenerationStrategy.StoreComputed);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });
            var mockReader = SetupMockDataReader(new[] { "Col1", "Col2" }, new List<object[]> { new object[] { 42, "FortyTwo" } });
            var connection = SetupMockConnection(mockReader.Object);

            var executor = new BatchExecutor(new Mock<SqlGenerator> { CallBase = true }.Object, connection, new RelationalTypeMapper());

            await executor.ExecuteAsync(new[] { batch }, CancellationToken.None);

            Assert.Equal(42, stateEntry[stateEntry.EntityType.GetProperty("Col1")]);
            Assert.Equal("FortyTwo", stateEntry[stateEntry.EntityType.GetProperty("Col2")]);
        }

        [Fact]
        public async void ExecuteAsync_saves_store_generated_values_when_updating()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Modified, ValueGenerationStrategy.StoreIdentity, ValueGenerationStrategy.StoreComputed);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });
            var mockReader = SetupMockDataReader(new[] { "Col2" }, new List<object[]> { new object[] { "FortyTwo" } });
            var connection = SetupMockConnection(mockReader.Object);

            var executor = new BatchExecutor(new Mock<SqlGenerator> { CallBase = true }.Object, connection, new RelationalTypeMapper());

            await executor.ExecuteAsync(new[] { batch }, CancellationToken.None);

            Assert.Equal(1, stateEntry[stateEntry.EntityType.GetProperty("Col1")]);
            Assert.Equal("FortyTwo", stateEntry[stateEntry.EntityType.GetProperty("Col2")]);
        }

        [Fact]
        public async void Exception_thrown_for_more_than_one_row_returned_for_single_command()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationStrategy.StoreIdentity);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });
            var mockReader = SetupMockDataReader(new[] { "Col1" }, new List<object[]>
                {
                    new object[] { 42 },
                    new object[] { 43 }
                });
            var connection = SetupMockConnection(mockReader.Object);

            var executor = new BatchExecutor(new Mock<SqlGenerator> { CallBase = true }.Object, connection, new RelationalTypeMapper());

            Assert.Equal(Strings.TooManyRowsForModificationCommand,
                (await Assert.ThrowsAsync<DbUpdateException>(
                    async () => await executor.ExecuteAsync(new[] { batch }, CancellationToken.None))).Message);
        }

        [Fact]
        public async void Exception_thrown_if_rows_returned_for_command_without_store_generated_values()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });
            var mockReader = SetupMockDataReader(new[] { "Col1" }, new List<object[]> { new object[] { 42 } });
            var connection = SetupMockConnection(mockReader.Object);

            var executor = new BatchExecutor(new Mock<SqlGenerator> { CallBase = true }.Object, connection, new RelationalTypeMapper());

            Assert.Equal(Strings.FormatUpdateConcurrencyException(0, 1),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await executor.ExecuteAsync(new[] { batch }, CancellationToken.None))).Message);
        }

        [Fact]
        public async void Exception_thrown_if_no_rows_returned_for_command_with_store_generated_values()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationStrategy.StoreIdentity);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch(new[] { command });
            var mockReader = SetupMockDataReader(new[] { "Col1" });
            var connection = SetupMockConnection(mockReader.Object);

            var executor = new BatchExecutor(new Mock<SqlGenerator> { CallBase = true }.Object, connection, new RelationalTypeMapper());

            Assert.Equal(Strings.FormatUpdateConcurrencyException(1, 0),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await executor.ExecuteAsync(new[] { batch }, CancellationToken.None))).Message);
        }

        private RelationalConnection SetupMockConnection(DbDataReader dataReader)
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

            var mockRelationalConnection = new Mock<RelationalConnection>();
            mockRelationalConnection.Setup(m => m.DbConnection).Returns(mockConnection.Object);

            return mockRelationalConnection.Object;
        }

        private static Mock<DbDataReader> SetupMockDataReader(string[] columnNames = null, IList<object[]> results = null)
        {
            results = results ?? new List<object[]>();
            columnNames = columnNames ?? new string[0];
            var rowIndex = 0;
            object[] currentRow = null;

            var mockDataReader = new Mock<DbDataReader>();

            mockDataReader.Setup(r => r.FieldCount).Returns(columnNames.Length);
            mockDataReader.Setup(r => r.GetName(It.IsAny<int>())).Returns((int columnIdx) => columnNames[columnIdx]);
            mockDataReader.Setup(r => r.GetValue(It.IsAny<int>())).Returns((int columnIdx) => currentRow[columnIdx]);
            mockDataReader.Setup(r => r.GetFieldValue<int>(It.IsAny<int>())).Returns((int columnIdx) => (int)currentRow[columnIdx]);
            mockDataReader.Setup(r => r.GetFieldValue<string>(It.IsAny<int>())).Returns((int columnIdx) => (string)currentRow[columnIdx]);
            mockDataReader.Setup(r => r.GetFieldValue<object>(It.IsAny<int>())).Returns((int columnIdx) => currentRow[columnIdx]);

            mockDataReader
                .Setup(r => r.ReadAsync(It.IsAny<CancellationToken>()))
                .Callback(() => currentRow = rowIndex < results.Count ? results[rowIndex++] : null)
                .Returns(() =>
                    {
                        var tcs = new TaskCompletionSource<bool>();
                        tcs.SetResult(currentRow != null);
                        return tcs.Task;
                    });

            return mockDataReader;
        }

        private class T1
        {
            public int Col1 { get; set; }
            public string Col2 { get; set; }
        }

        private static IModel BuildModel(ValueGenerationStrategy keyStrategy, ValueGenerationStrategy nonKeyStrategy)
        {
            var model = new Metadata.Model();

            var entityType = new EntityType(typeof(T1));

            var key = entityType.AddProperty("Col1", typeof(int));
            key.ValueGenerationStrategy = keyStrategy;
            entityType.SetKey(key);

            var nonKey = entityType.AddProperty("Col2", typeof(string));
            nonKey.ValueGenerationStrategy = nonKeyStrategy;

            model.AddEntityType(entityType);

            return model;
        }

        private static DbContextConfiguration CreateConfiguration(IModel model)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework();
            return new DbContext(serviceCollection.BuildServiceProvider(),
                new DbContextOptions()
                    .UseModel(model)
                    .BuildConfiguration())
                .Configuration;
        }

        private static StateEntry CreateStateEntry(
            EntityState entityState,
            ValueGenerationStrategy keyStrategy = ValueGenerationStrategy.None,
            ValueGenerationStrategy nonKeyStrategy = ValueGenerationStrategy.None)
        {
            var model = BuildModel(keyStrategy, nonKeyStrategy);

            var stateEntry = CreateConfiguration(model).Services.StateEntryFactory.Create(
                model.GetEntityType("T1"), new T1 { Col1 = 1, Col2 = "Test" });

            stateEntry.EntityState = entityState;

            return stateEntry;
        }

        //private static Table CreateTable(
        //    StoreValueGenerationStrategy keyStrategy = StoreValueGenerationStrategy.None,
        //    StoreValueGenerationStrategy nonKeyStrategy = StoreValueGenerationStrategy.None)
        //{
        //    var key = new Column("Col1", "_") { ValueGenerationStrategy = keyStrategy };
        //    return new Table("T1", new[] { key, new Column("Col2", "_") { ValueGenerationStrategy = nonKeyStrategy } })
        //        {
        //            PrimaryKey = new PrimaryKey("PK", new[] { key })
        //        };
        //}
    }
}
