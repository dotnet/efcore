// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
using Microsoft.Data.Entity.Update;
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
        public void GenerateCommandText_compiles_inserts()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var sqlGeneratorMock = new Mock<SqlGenerator>();
            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command, sqlGeneratorMock.Object);

            batch.GenerateCommandTextBase(sqlGeneratorMock.Object);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendInsertOperation(It.IsAny<StringBuilder>(), "T1", It.IsAny<IReadOnlyList<ColumnModification>>()));
        }

        [Fact]
        public void GenerateCommandText_compiles_updates()
        {
            var stateEntry = CreateStateEntry(EntityState.Modified, ValueGenerationOnSave.WhenInserting);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var sqlGeneratorMock = new Mock<SqlGenerator>();
            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command, sqlGeneratorMock.Object);

            batch.GenerateCommandTextBase(sqlGeneratorMock.Object);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendUpdateOperation(It.IsAny<StringBuilder>(), "T1", It.IsAny<IReadOnlyList<ColumnModification>>()));
        }

        [Fact]
        public void GenerateCommandText_compiles_deletes()
        {
            var stateEntry = CreateStateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var sqlGeneratorMock = new Mock<SqlGenerator>();
            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command, sqlGeneratorMock.Object);

            batch.GenerateCommandTextBase(sqlGeneratorMock.Object);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendDeleteOperation(It.IsAny<StringBuilder>(), "T1", It.IsAny<IReadOnlyList<ColumnModification>>()));
        }

        [Fact]
        public async Task ExecuteAsync_executes_batch_commands_and_consumes_reader()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var mockReader = CreateDataReaderMock();
            var batch = new ModificationCommandBatchFake(mockReader.Object);
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);

            await batch.ExecuteAsync(new Mock<DbTransaction>().Object, new RelationalTypeMapper());

            mockReader.Verify(r => r.ReadAsync(It.IsAny<CancellationToken>()), Times.Exactly(1));
            mockReader.Verify(r => r.NextResultAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationOnSave.WhenInserting);
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatchFake(CreateDataReaderMock(new[] { "Col1" }, new List<object[]> { new object[] { 42 } }).Object);
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);

            await batch.ExecuteAsync(new Mock<DbTransaction>().Object, new RelationalTypeMapper());

            Assert.Equal(42, stateEntry[stateEntry.EntityType.GetProperty("Id")]);
            Assert.Equal("Test", stateEntry[stateEntry.EntityType.GetProperty("Name")]);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values_on_non_key_columns()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Added, ValueGenerationOnSave.WhenInserting, ValueGenerationOnSave.WhenInsertingAndUpdating);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatchFake(CreateDataReaderMock(new[] { "Col1", "Col2" }, new List<object[]> { new object[] { 42, "FortyTwo" } }).Object);
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);

            await batch.ExecuteAsync(new Mock<DbTransaction>().Object, new RelationalTypeMapper());

            Assert.Equal(42, stateEntry[stateEntry.EntityType.GetProperty("Id")]);
            Assert.Equal("FortyTwo", stateEntry[stateEntry.EntityType.GetProperty("Name")]);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values_when_updating()
        {
            var stateEntry = CreateStateEntry(
                EntityState.Modified, ValueGenerationOnSave.WhenInserting, ValueGenerationOnSave.WhenInsertingAndUpdating);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatchFake(CreateDataReaderMock(new[] { "Col2" }, new List<object[]> { new object[] { "FortyTwo" } }).Object);
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);

            await batch.ExecuteAsync(new Mock<DbTransaction>().Object, new RelationalTypeMapper());

            Assert.Equal(1, stateEntry[stateEntry.EntityType.GetProperty("Id")]);
            Assert.Equal("FortyTwo", stateEntry[stateEntry.EntityType.GetProperty("Name")]);
        }
        
        [Fact]
        public async Task Exception_not_thrown_for_more_than_one_row_returned_for_single_command()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationOnSave.WhenInserting);
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var mockReader = CreateDataReaderMock(new[] { "Col1" }, new List<object[]>
                {
                    new object[] { 42 },
                    new object[] { 43 }
                });
            var batch = new ModificationCommandBatchFake(mockReader.Object);
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);

            await batch.ExecuteAsync(new Mock<DbTransaction>().Object, new RelationalTypeMapper());
            
            Assert.Equal(42, stateEntry[stateEntry.EntityType.GetProperty("Id")]);
        }

        [Fact]
        public async Task Exception_thrown_if_rows_returned_for_command_without_store_generated_values_is_not_1()
        {
            var stateEntry = CreateStateEntry(EntityState.Added);
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatchFake(CreateDataReaderMock(new[] { "Col1" }, new List<object[]> { new object[] { 42 } }).Object);
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);

            Assert.Equal(Strings.FormatUpdateConcurrencyException(1, 42),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(new Mock<DbTransaction>().Object, new RelationalTypeMapper()))).Message);
        }

        [Fact]
        public async Task Exception_thrown_if_no_rows_returned_for_command_with_store_generated_values()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationOnSave.WhenInserting);
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator());
            command.AddStateEntry(stateEntry);

            var batch = new ModificationCommandBatchFake(CreateDataReaderMock(new[] { "Col1" }, new List<object[]>()).Object);
            batch.AddCommand(command, new Mock<SqlGenerator> { CallBase = true }.Object);

            Assert.Equal(Strings.FormatUpdateConcurrencyException(1, 0),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(new Mock<DbTransaction>().Object, new RelationalTypeMapper()))).Message);
        }

        [Fact]
        public void CreateStoreCommand_creates_parameters_for_each_ModificationCommand_with_non_null_parameter_name()
        {
            var stateEntry = CreateStateEntry(EntityState.Added, ValueGenerationOnSave.WhenInserting);
            var property = stateEntry.EntityType.GetProperty("Id");
            var batch = new ModificationCommandBatchFake();
            var commandMock = new Mock<ModificationCommand>();
            commandMock.Setup(m => m.ColumnModifications).Returns(
                new List<ColumnModification>
                    {
                        new ColumnModification(
                            stateEntry,
                            property,
                            parameterName: "p",
                            originalParameterName: null,
                            isRead: false,
                            isWrite: false,
                            isKey: false,
                            isCondition: false),
                        new ColumnModification(
                            stateEntry,
                            property,
                            parameterName: null,
                            originalParameterName: "op",
                            isRead: false,
                            isWrite: false,
                            isKey: false,
                            isCondition: false),
                    });

            batch.AddCommand(commandMock.Object, new Mock<SqlGenerator> { CallBase = true }.Object);

            var command = batch.CreateStoreCommandBase(CreateMockDbTransaction(), new RelationalTypeMapper());

            Assert.Equal("p", command.Parameters[0].ParameterName);
            Assert.Equal("op", command.Parameters[1].ParameterName);
        }

        private static Mock<DbConnection> CreateMockDbConnection(DbCommand dbCommand = null)
        {
            var mockConnection = new Mock<DbConnection>();
            mockConnection
                .Protected()
                .Setup<DbCommand>("CreateDbCommand")
                .Returns(dbCommand ?? CreateDbCommandMock().Object);
            return mockConnection;
        }

        private static Mock<DbCommand> CreateDbCommandMock(DbDataReader dataReader = null)
        {
            var dbCommandMock = new Mock<DbCommand>();
            dbCommandMock
                .Protected()
                .Setup<DbParameter>("CreateDbParameter")
                .Returns(() => CreateDbParameterMock().Object);
            dbCommandMock
                .Protected()
                .SetupGet<DbParameterCollection>("DbParameterCollection")
                .Returns(CreateDbParameterCollectionMock().Object);

            var tcs = new TaskCompletionSource<DbDataReader>();
            tcs.SetResult(dataReader ?? CreateDataReaderMock().Object);

            dbCommandMock
                .Protected()
                .Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                .Returns(tcs.Task);

            return dbCommandMock;
        }

        private static Mock<DbParameter> CreateDbParameterMock()
        {
            var dbParameterMock = new Mock<DbParameter>();
            string name = null;
            dbParameterMock
                .Setup(m => m.ParameterName)
                .Returns(() => name);
            dbParameterMock
                .SetupSet(m => m.ParameterName = It.IsAny<string>())
                .Callback<string>(n => name = n);
            return dbParameterMock;
        }

        private static Mock<DbParameterCollection> CreateDbParameterCollectionMock()
        {
            var parameters = new List<object>();
            var dbParameterCollectionMock = new Mock<DbParameterCollection>();
            dbParameterCollectionMock
                .Setup(m => m.Add(It.IsAny<object>()))
                .Returns<object>(p =>
                    {
                        parameters.Add(p);
                        return parameters.Count - 1;
                    });
            dbParameterCollectionMock
                .Protected()
                .Setup<DbParameter>("GetParameter", ItExpr.IsAny<int>())
                .Returns<int>(i => (DbParameter)parameters[i]);
            return dbParameterCollectionMock;
        }

        private static DbTransaction CreateMockDbTransaction(DbConnection dbConnection = null)
        {
            var dbTransactionMock = new Mock<DbTransaction>();
            dbTransactionMock
                .Protected()
                .Setup<DbConnection>("DbConnection")
                .Returns(dbConnection ?? CreateMockDbConnection().Object);

            return dbTransactionMock.Object;
        }

        private static Mock<DbDataReader> CreateDataReaderMock(string[] columnNames = null, IList<object[]> results = null)
        {
            results = results ?? new List<object[]> { new object[] { 1 } };
            columnNames = columnNames ?? new[] { "RowsAffected" };
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
                .Returns(() =>
                    {
                        currentRow = rowIndex < results.Count ? results[rowIndex++] : null;
                        return Task.FromResult(currentRow != null);
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
            key.SetColumnName("Col1");
            entityType.SetKey(key);

            var nonKey = entityType.AddProperty("Name", typeof(string));
            nonKey.SetColumnName("Col2");
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

        public class ModificationCommandBatchFake : ModificationCommandBatch
        {
            private readonly DbDataReader _reader;

            public ModificationCommandBatchFake()
            {
            }

            public ModificationCommandBatchFake(DbDataReader reader)
            {
                _reader = reader;
            }

            protected override string GenerateCommandText(SqlGenerator sqlGenerator)
            {
                return GenerateCommandTextProtected(sqlGenerator);
            }

            public virtual string GenerateCommandTextBase(SqlGenerator sqlGenerator)
            {
                return base.GenerateCommandText(sqlGenerator);
            }

            public virtual string GenerateCommandTextProtected(SqlGenerator sqlGenerator)
            {
                return null;
            }

            protected override DbCommand CreateStoreCommand(DbTransaction transaction, RelationalTypeMapper typeMapper)
            {
                return CreateStoreCommandProtected(transaction, typeMapper);
            }

            public virtual DbCommand CreateStoreCommandBase(DbTransaction transaction, RelationalTypeMapper typeMapper)
            {
                return base.CreateStoreCommand(transaction, typeMapper);
            }

            public virtual DbCommand CreateStoreCommandProtected(DbTransaction transaction, RelationalTypeMapper typeMapper)
            {
                return CreateDbCommandMock(_reader).Object;
            }
        }
    }
}
