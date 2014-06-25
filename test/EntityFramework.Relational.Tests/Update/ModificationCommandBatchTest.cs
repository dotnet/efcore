// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
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
    public class ModificationCommandBatchTest
    {
        [Fact]
        public async Task AddCommand_checks_arguments()
        {
            var batch = new ModificationCommandBatch();

            Assert.Equal(
                "modificationCommand",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    batch.AddCommand(null, new ConcreteSqlGenerator())).ParamName);

            Assert.Equal(
                "sqlGenerator",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    batch.AddCommand(new ModificationCommand("T1", new ParameterNameGenerator()), null)).ParamName);
        }

        [Fact]
        public async Task CompileBatch_compiles_inserts()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new ConcreteSqlGenerator());

            await VerifySqlAsync(batch,
                "BatchHeader$" + Environment.NewLine +
                "INSERT INTO [T1] ([Col1], [Col2]) VALUES (@p0, @p1)$" + Environment.NewLine);
        }

        [Fact]
        public async Task CompileBatch_compiles_updates()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified, ValueGenerationOnSave.WhenInserting);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new ConcreteSqlGenerator());

            await VerifySqlAsync(batch,
                "BatchHeader$" + Environment.NewLine +
                "UPDATE [T1] SET [Col2] = @p1 WHERE [Col1] = @p0$" + Environment.NewLine);
        }

        [Fact]
        public async Task CompileBatch_compiles_deletes()
        {
            var stateEntry = CreateStateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new ConcreteSqlGenerator());

            await VerifySqlAsync(batch,
                "BatchHeader$" + Environment.NewLine +
                "DELETE FROM [T1] WHERE [Col1] = @p0$" + Environment.NewLine);
        }

        [Fact]
        public async Task Batch_separator_not_appended_if_batch_header_empty()
        {
            var stateEntry = CreateStateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new ConcreteSqlGenerator(useBatchHeader: false));

            await VerifySqlAsync(batch,
                "DELETE FROM [T1] WHERE [Col1] = @p0$" + Environment.NewLine);
        }
        
        [Fact]
        public async Task ExecuteAsync_checks_arguments()
        {
            var batch = new ModificationCommandBatch();

            Assert.Equal(
                "connection",
                // ReSharper disable once AssignNullToNotNullAttribute
                (await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    batch.ExecuteAsync(null, new RelationalTypeMapper()))).ParamName);

            Assert.Equal(
                "typeMapper",
                // ReSharper disable once AssignNullToNotNullAttribute
                (await Assert.ThrowsAsync<ArgumentNullException>(() =>
                    batch.ExecuteAsync(CreateMockRelationalConnection(), null))).ParamName);
        }

        [Fact]
        public async void ExecuteAsync_executes_batch_commands_and_consumes_reader()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);
            var mockReader = SetupMockDataReader();
            var connection = CreateMockRelationalConnection(mockReader.Object);

            await batch.ExecuteAsync(connection, new RelationalTypeMapper());

            mockReader.Verify(r => r.ReadAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockReader.Verify(r => r.NextResultAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async void ExecuteAsync_saves_store_generated_values()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationOnSave.WhenInserting);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);
            var mockReader = SetupMockDataReader(new[] { "Col1" }, new List<object[]> { new object[] { 42 } });
            var connection = CreateMockRelationalConnection(mockReader.Object);
            
            await batch.ExecuteAsync(connection, new RelationalTypeMapper());

            Assert.Equal(42, stateEntry[stateEntry.EntityType.GetProperty("Id")]);
            Assert.Equal("Test", stateEntry[stateEntry.EntityType.GetProperty("Name")]);
        }

        [Fact]
        public async void ExecuteAsync_saves_store_generated_values_on_non_key_columns()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Added, ValueGenerationOnSave.WhenInserting, ValueGenerationOnSave.WhenInsertingAndUpdating);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);
            var mockReader = SetupMockDataReader(new[] { "Col1", "Col2" }, new List<object[]> { new object[] { 42, "FortyTwo" } });
            var connection = CreateMockRelationalConnection(mockReader.Object);

            await batch.ExecuteAsync(connection, new RelationalTypeMapper());

            Assert.Equal(42, stateEntry[stateEntry.EntityType.GetProperty("Id")]);
            Assert.Equal("FortyTwo", stateEntry[stateEntry.EntityType.GetProperty("Name")]);
        }

        [Fact]
        public async void ExecuteAsync_saves_store_generated_values_when_updating()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Modified, ValueGenerationOnSave.WhenInserting, ValueGenerationOnSave.WhenInsertingAndUpdating);

            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);
            var mockReader = SetupMockDataReader(new[] { "Col2" }, new List<object[]> { new object[] { "FortyTwo" } });
            var connection = CreateMockRelationalConnection(mockReader.Object);

            await batch.ExecuteAsync(connection, new RelationalTypeMapper());

            Assert.Equal(1, stateEntry[stateEntry.EntityType.GetProperty("Id")]);
            Assert.Equal("FortyTwo", stateEntry[stateEntry.EntityType.GetProperty("Name")]);
        }

        [Fact]
        public async void Exception_thrown_for_more_than_one_row_returned_for_single_command()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationOnSave.WhenInserting);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);
            var mockReader = SetupMockDataReader(new[] { "Col1" }, new List<object[]>
                {
                    new object[] { 42 },
                    new object[] { 43 }
                });
            var connection = CreateMockRelationalConnection(mockReader.Object);

            Assert.Equal(Strings.TooManyRowsForModificationCommand,
                (await Assert.ThrowsAsync<DbUpdateException>(
                    async () => await batch.ExecuteAsync(connection, new RelationalTypeMapper()))).Message);
        }

        [Fact]
        public async void Exception_thrown_if_rows_returned_for_command_without_store_generated_values()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);
            var mockReader = SetupMockDataReader(new[] { "Col1" }, new List<object[]> { new object[] { 42 } });
            var connection = CreateMockRelationalConnection(mockReader.Object);

            Assert.Equal(Strings.FormatUpdateConcurrencyException(0, 1),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(connection, new RelationalTypeMapper()))).Message);
        }

        [Fact]
        public async void Exception_thrown_if_no_rows_returned_for_command_with_store_generated_values()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationOnSave.WhenInserting);
            var command = new ModificationCommand("T1", new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatch();
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);
            var mockReader = SetupMockDataReader(new[] { "Col1" });
            var connection = CreateMockRelationalConnection(mockReader.Object);

            Assert.Equal(Strings.FormatUpdateConcurrencyException(1, 0),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(connection, new RelationalTypeMapper()))).Message);
        }

        private async Task VerifySqlAsync(ModificationCommandBatch batch, string expectedSql)
        {
            var sqlString = string.Empty;
            var dbCommandMock = new Mock<DbCommand>();
            dbCommandMock.SetupSet(c => c.CommandText = It.IsAny<string>()).Callback<string>(t => sqlString = t);

            await batch.ExecuteAsync(CreateMockRelationalConnection(dbCommandMock: dbCommandMock), new RelationalTypeMapper());

            Assert.Equal(expectedSql, sqlString);
        }

        private RelationalConnection CreateMockRelationalConnection(DbDataReader dataReader = null, Mock<DbCommand> dbCommandMock = null)
        {
            var mockConnection = new Mock<DbConnection>();
            dbCommandMock = dbCommandMock ?? new Mock<DbCommand>();
            mockConnection
                .Protected()
                .Setup<DbCommand>("CreateDbCommand")
                .Returns(dbCommandMock.Object);
            dbCommandMock
                .Protected()
                .Setup<DbParameter>("CreateDbParameter")
                .Returns(Mock.Of<DbParameter>());
            dbCommandMock
                .Protected()
                .SetupGet<DbParameterCollection>("DbParameterCollection")
                .Returns(Mock.Of<DbParameterCollection>());

            var tcs = new TaskCompletionSource<DbDataReader>();
            tcs.SetResult(dataReader ?? SetupMockDataReader().Object);

            dbCommandMock
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
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static IModel BuildModel(ValueGenerationOnSave keyStrategy, ValueGenerationOnSave nonKeyStrategy)
        {
            var model = new Metadata.Model();

            var entityType = new EntityType(typeof(T1));

            var key = entityType.AddProperty("Id", typeof(int));
            key.ValueGenerationOnSave = keyStrategy;
            key.StorageName = "Col1";
            entityType.SetKey(key);

            var nonKey = entityType.AddProperty("Name", typeof(string));
            nonKey.StorageName = "Col2";
            nonKey.ValueGenerationOnSave = nonKeyStrategy;

            model.AddEntityType(entityType);

            return model;
        }

        private static DbContextConfiguration CreateConfiguration(IModel model)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddEntityFramework().AddInMemoryStore();
            return new DbContext(serviceCollection.BuildServiceProvider(),
                new DbContextOptions()
                    .UseInMemoryStore(persist: false)
                    .UseModel(model))
                .Configuration;
        }

        private static StateEntry CreateStateEntry(
            EntityState entityState,
            ValueGenerationOnSave keyStrategy = ValueGenerationOnSave.None,
            ValueGenerationOnSave nonKeyStrategy = ValueGenerationOnSave.None)
        {
            var model = BuildModel(keyStrategy, nonKeyStrategy);
            var stateEntry = CreateConfiguration(model).Services.StateEntryFactory.Create(
                model.GetEntityType("T1"), new T1 { Id = 1, Name = "Test" });
            stateEntry.EntityState = entityState;
            return stateEntry;
        }

        private class ConcreteSqlGenerator : SqlGenerator
        {
            private readonly string _batchHeader;

            public ConcreteSqlGenerator(bool useBatchHeader = true)
            {
                _batchHeader = useBatchHeader ? "BatchHeader" : null;
            }

            protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
            {
                commandStringBuilder
                    .Append(QuoteIdentifier(columnModification.ColumnName))
                    .Append(" = ")
                    .Append("provider_specific_identity()");
            }

            public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            {
                commandStringBuilder.Append(_batchHeader);
            }

            public override string BatchCommandSeparator
            {
                get { return "$"; }
            }
        }
    }
}
