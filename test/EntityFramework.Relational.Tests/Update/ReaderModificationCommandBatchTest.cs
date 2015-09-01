// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.TestUtilities.FakeProvider;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Update
{
    public class ReaderModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_adds_command_if_possible()
        {
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);
            batch.ShouldAddCommand = true;
            batch.ShouldValidateSql = true;

            batch.AddCommand(command);

            Assert.Equal(2, batch.ModificationCommands.Count);
            Assert.Same(command, batch.ModificationCommands[0]);
            Assert.Equal("..", batch.CommandText);
        }

        [Fact]
        public void AddCommand_does_not_add_command_if_not_possible()
        {
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);
            batch.ShouldAddCommand = false;
            batch.ShouldValidateSql = true;

            batch.AddCommand(command);

            Assert.Equal(1, batch.ModificationCommands.Count);
            Assert.Equal(".", batch.CommandText);
        }

        [Fact]
        public void AddCommand_does_not_add_command_if_resulting_sql_is_invalid()
        {
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);
            batch.ShouldAddCommand = true;
            batch.ShouldValidateSql = false;

            batch.AddCommand(command);

            Assert.Equal(1, batch.ModificationCommands.Count);
            Assert.Equal(".", batch.CommandText);
        }

        [Fact]
        public void UpdateCommandText_compiles_inserts()
        {
            var entry = CreateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var sqlGeneratorMock = new Mock<IUpdateSqlGenerator>();
            var batch = new ModificationCommandBatchFake(sqlGeneratorMock.Object);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendInsertOperation(It.IsAny<StringBuilder>(), command));
        }

        [Fact]
        public void UpdateCommandText_compiles_updates()
        {
            var entry = CreateEntry(EntityState.Modified, generateKeyValues: true);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var sqlGeneratorMock = new Mock<IUpdateSqlGenerator>();
            var batch = new ModificationCommandBatchFake(sqlGeneratorMock.Object);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendUpdateOperation(It.IsAny<StringBuilder>(), command));
        }

        [Fact]
        public void UpdateCommandText_compiles_deletes()
        {
            var entry = CreateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var sqlGeneratorMock = new Mock<IUpdateSqlGenerator>();
            var batch = new ModificationCommandBatchFake(sqlGeneratorMock.Object);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendDeleteOperation(It.IsAny<StringBuilder>(), command));
        }

        [Fact]
        public void UpdateCommandText_compiles_multiple_commands()
        {
            var entry = CreateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var fakeSqlGenerator = new FakeSqlGenerator();
            var batch = new ModificationCommandBatchFake(fakeSqlGenerator);
            batch.AddCommand(command);
            batch.AddCommand(command);

            Assert.Equal("..", batch.CommandText);

            Assert.Equal(1, fakeSqlGenerator.AppendBatchHeaderCalls);
        }

        private class FakeSqlGenerator : UpdateSqlGenerator
        {
            public override void AppendInsertOperation(StringBuilder commandStringBuilder, ModificationCommand command)
            {
                if (!string.IsNullOrEmpty(command.Schema))
                {
                    commandStringBuilder.Append(command.Schema + ".");
                }
                commandStringBuilder.Append(command.TableName);
            }

            public int AppendBatchHeaderCalls { get; set; }

            public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            {
                AppendBatchHeaderCalls++;
                base.AppendBatchHeader(commandStringBuilder);
            }

            protected override void AppendSelectAffectedCountCommand(StringBuilder commandStringBuilder, string name, string schema)
            {
            }

            protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder, int expectedRowsAffected)
            {
            }

            protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder, ColumnModification columnModification)
            {
            }
        }

        [Fact]
        public async Task ExecuteAsync_executes_batch_commands_and_consumes_reader()
        {
            var entry = CreateEntry(EntityState.Added);
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            var dbDataReader = CreateDbDataReader();

            var connection = CreateConnection(
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                        Task.FromResult<DbDataReader>(dbDataReader)));

            await batch.ExecuteAsync(connection);

            Assert.Equal(1, dbDataReader.ReadAsyncCount);
            Assert.Equal(1, dbDataReader.GetInt32Count);
            Assert.Equal(1, dbDataReader.CloseCount);
            Assert.Equal(1, dbDataReader.DisposeCount);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.MarkAsTemporary(entry.EntityType.GetPrimaryKey().Properties[0]);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            var connection = CreateConnection(
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                        Task.FromResult<DbDataReader>(
                            CreateDbDataReader(new[] { "Col1" }, new List<object[]> { new object[] { 42 } }))));

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.GetProperty("Id")]);
            Assert.Equal("Test", entry[entry.EntityType.GetProperty("Name")]);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values_on_non_key_columns()
        {
            var entry = CreateEntry(
                EntityState.Added, generateKeyValues: true, computeNonKeyValue: true);
            entry.MarkAsTemporary(entry.EntityType.GetPrimaryKey().Properties[0]);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);


            var connection = CreateConnection(
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                    Task.FromResult<DbDataReader>(
                        CreateDbDataReader(new[] { "Col1", "Col2" }, new List<object[]> { new object[] { 42, "FortyTwo" } }))));

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.GetProperty("Id")]);
            Assert.Equal("FortyTwo", entry[entry.EntityType.GetProperty("Name")]);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values_when_updating()
        {
            var entry = CreateEntry(
                EntityState.Modified, generateKeyValues: true, computeNonKeyValue: true);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            var connection = CreateConnection(
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                        Task.FromResult<DbDataReader>(
                            CreateDbDataReader(new[] { "Col2" }, new List<object[]> { new object[] { "FortyTwo" } }))));

            await batch.ExecuteAsync(connection);

            Assert.Equal(1, entry[entry.EntityType.GetProperty("Id")]);
            Assert.Equal("FortyTwo", entry[entry.EntityType.GetProperty("Name")]);
        }

        [Fact]
        public async Task Exception_not_thrown_for_more_than_one_row_returned_for_single_command()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.MarkAsTemporary(entry.EntityType.GetPrimaryKey().Properties[0]);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            var connection = CreateConnection(
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                        Task.FromResult<DbDataReader>(
                            CreateDbDataReader(new[] { "Col1" }, new List<object[]>
                            {
                                new object[] { 42 },
                                new object[] { 43 }
                            }))));

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.GetProperty("Id")]);
        }

        [Fact]
        public async Task Exception_thrown_if_rows_returned_for_command_without_store_generated_values_is_not_1()
        {
            var entry = CreateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            var connection = CreateConnection(
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                        Task.FromResult<DbDataReader>(
                            CreateDbDataReader(new[] { "Col1" }, new List<object[]> { new object[] { 42 } }))));

            Assert.Equal(Strings.UpdateConcurrencyException(1, 42),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(
                        connection))).Message);
        }

        [Fact]
        public async Task Exception_thrown_if_no_rows_returned_for_command_with_store_generated_values()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.MarkAsTemporary(entry.EntityType.GetPrimaryKey().Properties[0]);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider(), new TypedRelationalValueBufferFactoryFactory());
            command.AddEntry(entry);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            var connection = CreateConnection(
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) =>
                        Task.FromResult<DbDataReader>(
                            CreateDbDataReader(new[] { "Col1" }, new List<object[]>()))));

            Assert.Equal(Strings.UpdateConcurrencyException(1, 0),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(
                        connection))).Message);
        }

        [Fact]
        public void CreateStoreCommand_creates_parameters_for_each_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.GetProperty("Id");
            entry.MarkAsTemporary(property);
            var batch = new ModificationCommandBatchFake();

            var commandMock1 = new Mock<ModificationCommand>(
                "T",
                "S",
                new ParameterNameGenerator(),
                (Func<IProperty, IRelationalPropertyAnnotations>)(p => p.TestProvider()),
                Mock.Of<IRelationalValueBufferFactoryFactory>());

            commandMock1.Setup(m => m.ColumnModifications).Returns(
                new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            new ParameterNameGenerator(),
                            false, true, false, false)
                    });
            batch.AddCommand(commandMock1.Object);

            var commandMock2 = new Mock<ModificationCommand>(
                "T",
                "S",
                new ParameterNameGenerator(),
                (Func<IProperty, IRelationalPropertyAnnotations>)(p => p.TestProvider()),
                Mock.Of<IRelationalValueBufferFactoryFactory>());
            commandMock2.Setup(m => m.ColumnModifications).Returns(
                new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            new ParameterNameGenerator(),
                            false, true, false, false)
                    });
            batch.AddCommand(commandMock2.Object);

            var command = batch.CreateStoreCommandBase("foo");

            Assert.Equal("foo", command.CommandText);
            Assert.Equal(2, batch.PopulateParameterCalls);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_write_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.GetProperty("Id");
            entry.MarkAsTemporary(property);
            var batch = new ModificationCommandBatchFake();
            var parameterList = new RelationalParameterList();

            batch.PopulateParametersBase(
                parameterList,
                new ColumnModification(
                    entry,
                    property,
                    property.TestProvider(),
                    new ParameterNameGenerator(),
                    false, true, false, false));

            Assert.Equal(1, parameterList.RelationalParameters.Count);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_condition_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.GetProperty("Id");
            entry.MarkAsTemporary(property);
            var batch = new ModificationCommandBatchFake();
            var parameterList = new RelationalParameterList();

            batch.PopulateParametersBase(
                parameterList,
                new ColumnModification(
                    entry,
                    property,
                    property.TestProvider(),
                    new ParameterNameGenerator(),
                    false, false, false, true));

            Assert.Equal(1, parameterList.RelationalParameters.Count);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_write_and_condition_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.GetProperty("Id");
            entry.MarkAsTemporary(property);
            var batch = new ModificationCommandBatchFake();
            var parameterList = new RelationalParameterList();

            batch.PopulateParametersBase(
                parameterList,
                new ColumnModification(
                    entry,
                    property,
                    property.TestProvider(),
                    new ParameterNameGenerator(),
                    false, true, false, true));

            Assert.Equal(2, parameterList.RelationalParameters.Count);
        }

        [Fact]
        public void PopulateParameters_does_not_create_parameter_for_read_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.GetProperty("Id");
            entry.MarkAsTemporary(property);
            var batch = new ModificationCommandBatchFake();
            var parameterList = new RelationalParameterList();

            batch.PopulateParametersBase(
                parameterList,
                new ColumnModification(
                    entry,
                    property,
                    property.TestProvider(),
                    new ParameterNameGenerator(),
                    true, false, false, false));

            Assert.Equal(0, parameterList.RelationalParameters.Count);
        }

        private class T1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static IModel BuildModel(bool generateKeyValues, bool computeNonKeyValue)
        {
            var model = new Entity.Metadata.Model();

            var entityType = model.AddEntityType(typeof(T1));

            var key = entityType.AddProperty("Id", typeof(int));
            key.IsShadowProperty = false;
            key.ValueGenerated = generateKeyValues ? ValueGenerated.OnAdd : ValueGenerated.Never;
            key.Relational().ColumnName = "Col1";
            entityType.GetOrSetPrimaryKey(key);

            var nonKey = entityType.AddProperty("Name", typeof(string));
            nonKey.IsShadowProperty = false;
            nonKey.Relational().ColumnName = "Col2";
            nonKey.ValueGenerated = computeNonKeyValue ? ValueGenerated.OnAddOrUpdate : ValueGenerated.Never;

            return model;
        }

        private static InternalEntityEntry CreateEntry(
            EntityState entityState,
            bool generateKeyValues = false,
            bool computeNonKeyValue = false)
        {
            var model = BuildModel(generateKeyValues, computeNonKeyValue);

            return RelationalTestHelpers.Instance.CreateInternalEntry(model, entityState, new T1 { Id = 1, Name = computeNonKeyValue ? null : "Test" });
        }

        private class ModificationCommandBatchFake : AffectedCountModificationCommandBatch
        {
            public ModificationCommandBatchFake(IUpdateSqlGenerator sqlGenerator = null)
                : base(
                      sqlGenerator ?? new FakeSqlGenerator(),
                      new RelationalCommandBuilderFactory(
                          new LoggerFactory(),
                          new ConcreteTypeMapper()))
            {
                ShouldAddCommand = true;
                ShouldValidateSql = true;
            }

            public string CommandText
            {
                get { return GetCommandText(); }
            }

            public bool ShouldAddCommand { get; set; }

            protected override bool CanAddCommand(ModificationCommand modificationCommand)
            {
                return ShouldAddCommand;
            }

            public bool ShouldValidateSql { get; set; }

            protected override bool IsCommandTextValid()
            {
                return ShouldValidateSql;
            }

            protected override void UpdateCachedCommandText(int commandIndex)
            {
                CachedCommandText = CachedCommandText ?? new StringBuilder();
                CachedCommandText.Append(".");
            }

            public void UpdateCachedCommandTextBase(int commandIndex)
            {
                base.UpdateCachedCommandText(commandIndex);
            }

            protected override IRelationalCommand CreateStoreCommand([NotNull] string commandText)
            {
                return base.CreateStoreCommand(commandText);
            }

            public IRelationalCommand CreateStoreCommandBase(string commandText)
            {
                return base.CreateStoreCommand(commandText);
            }

            public int PopulateParameterCalls { get; set; }

            protected override void PopulateParameters(RelationalParameterList parameterList, ColumnModification columnModification)
            {
                PopulateParameterCalls++;
            }

            public void PopulateParametersBase(RelationalParameterList parameterList, ColumnModification columnModification)
            {
                base.PopulateParameters(parameterList, columnModification);
            }
        }

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
            protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }
                = new Dictionary<Type, RelationalTypeMapping>
                    {
                        { typeof(int), new RelationalTypeMapping("int", DbType.String) }
                    };

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }
                = new Dictionary<string, RelationalTypeMapping>();
        }

        private static FakeRelationalConnection CreateConnection(FakeCommandExecutor commandExecutor)
            => FakeProviderTestHelpers.CreateConnection(
                FakeProviderTestHelpers.CreateOptions(
                    FakeProviderTestHelpers.CreateOptionsExtension(
                        FakeProviderTestHelpers.CreateDbConnection(commandExecutor))));

        private static FakeDbDataReader CreateDbDataReader(string[] columnNames = null, IList<object[]> results = null)
            => new FakeDbDataReader(
                columnNames ?? new[] { "RowsAffected" },
                results ?? new List<object[]> { new object[] { 1 } });
    }
}
