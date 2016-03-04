// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Relational.Tests.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Tests;
using Microsoft.EntityFrameworkCore.Update;
using Moq;
using Xunit;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Update
{
    public class ReaderModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_adds_command_if_possible()
        {
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());

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
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());

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
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());

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

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var sqlGeneratorMock = new Mock<IUpdateSqlGenerator>();
            var batch = new ModificationCommandBatchFake(sqlGeneratorMock.Object);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendInsertOperation(It.IsAny<StringBuilder>(), command, 0));
        }

        [Fact]
        public void UpdateCommandText_compiles_updates()
        {
            var entry = CreateEntry(EntityState.Modified, generateKeyValues: true);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var sqlGeneratorMock = new Mock<IUpdateSqlGenerator>();
            var batch = new ModificationCommandBatchFake(sqlGeneratorMock.Object);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendUpdateOperation(It.IsAny<StringBuilder>(), command, 0));
        }

        [Fact]
        public void UpdateCommandText_compiles_deletes()
        {
            var entry = CreateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var sqlGeneratorMock = new Mock<IUpdateSqlGenerator>();
            var batch = new ModificationCommandBatchFake(sqlGeneratorMock.Object);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            sqlGeneratorMock.Verify(g => g.AppendBatchHeader(It.IsAny<StringBuilder>()));
            sqlGeneratorMock.Verify(g => g.AppendDeleteOperation(It.IsAny<StringBuilder>(), command, 0));
        }

        [Fact]
        public void UpdateCommandText_compiles_multiple_commands()
        {
            var entry = CreateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
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
            public FakeSqlGenerator()
                : base(new RelationalSqlGenerationHelper())
            {
            }

            public override ResultSetMapping AppendInsertOperation(StringBuilder commandStringBuilder, ModificationCommand command, int commandPosition)
            {
                if (!string.IsNullOrEmpty(command.Schema))
                {
                    commandStringBuilder.Append(command.Schema + ".");
                }
                commandStringBuilder.Append(command.TableName);

                return ResultSetMapping.NotLastInResultSet;
            }

            public int AppendBatchHeaderCalls { get; set; }

            public override void AppendBatchHeader(StringBuilder commandStringBuilder)
            {
                AppendBatchHeaderCalls++;
                base.AppendBatchHeader(commandStringBuilder);
            }

            protected override ResultSetMapping AppendSelectAffectedCountCommand(
                StringBuilder commandStringBuilder, string name, string schema, int commandPosition)
            {
                return ResultSetMapping.NoResultSet;
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

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var dbDataReader = CreateFakeDataReader();

            var commandBuilderFactory = new FakeCommandBuilderFactory(dbDataReader);

            var batch = new ModificationCommandBatchFake(factory: commandBuilderFactory);
            batch.AddCommand(command);

            var connection = CreateConnection();

            await batch.ExecuteAsync(connection);

            Assert.Equal(1, dbDataReader.ReadAsyncCount);
            Assert.Equal(1, dbDataReader.GetInt32Count);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.MarkAsTemporary(entry.EntityType.FindPrimaryKey().Properties[0]);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var commandBuilderFactory = new FakeCommandBuilderFactory(
                CreateFakeDataReader(new[] { "Col1" }, new List<object[]> { new object[] { 42 } }));

            var batch = new ModificationCommandBatchFake(factory: commandBuilderFactory);
            batch.AddCommand(command);

            var connection = CreateConnection();

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
            Assert.Equal("Test", entry[entry.EntityType.FindProperty("Name")]);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values_on_non_key_columns()
        {
            var entry = CreateEntry(
                EntityState.Added, generateKeyValues: true, computeNonKeyValue: true);
            entry.MarkAsTemporary(entry.EntityType.FindPrimaryKey().Properties[0]);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var commandBuilderFactory = new FakeCommandBuilderFactory(
                CreateFakeDataReader(new[] { "Col1", "Col2" }, new List<object[]> { new object[] { 42, "FortyTwo" } }));

            var batch = new ModificationCommandBatchFake(factory: commandBuilderFactory);
            batch.AddCommand(command);

            var connection = CreateConnection();

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
            Assert.Equal("FortyTwo", entry[entry.EntityType.FindProperty("Name")]);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values_when_updating()
        {
            var entry = CreateEntry(
                EntityState.Modified, generateKeyValues: true, computeNonKeyValue: true);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var commandBuilderFactory = new FakeCommandBuilderFactory(
                CreateFakeDataReader(new[] { "Col2" }, new List<object[]> { new object[] { "FortyTwo" } }));

            var batch = new ModificationCommandBatchFake(factory: commandBuilderFactory);
            batch.AddCommand(command);

            var connection = CreateConnection();

            await batch.ExecuteAsync(connection);

            Assert.Equal(1, entry[entry.EntityType.FindProperty("Id")]);
            Assert.Equal("FortyTwo", entry[entry.EntityType.FindProperty("Name")]);
        }

        [Fact]
        public async Task Exception_not_thrown_for_more_than_one_row_returned_for_single_command()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.MarkAsTemporary(entry.EntityType.FindPrimaryKey().Properties[0]);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var commandBuilderFactory = new FakeCommandBuilderFactory(
                CreateFakeDataReader(
                    new[] { "Col1" },
                    new List<object[]>
                    {
                        new object[] { 42 },
                        new object[] { 43 }
                    }));

            var batch = new ModificationCommandBatchFake(factory: commandBuilderFactory);
            batch.AddCommand(command);

            var connection = CreateConnection();

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
        }

        [Fact]
        public async Task Exception_thrown_if_rows_returned_for_command_without_store_generated_values_is_not_1()
        {
            var entry = CreateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var commandBuilderFactory = new FakeCommandBuilderFactory(
                CreateFakeDataReader(new[] { "Col1" }, new List<object[]> { new object[] { 42 } }));

            var batch = new ModificationCommandBatchFake(factory: commandBuilderFactory);
            batch.AddCommand(command);

            var connection = CreateConnection();

            Assert.Equal(RelationalStrings.UpdateConcurrencyException(1, 42),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(connection))).Message);
        }

        [Fact]
        public async Task Exception_thrown_if_no_rows_returned_for_command_with_store_generated_values()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.MarkAsTemporary(entry.EntityType.FindPrimaryKey().Properties[0]);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator(), p => p.TestProvider());
            command.AddEntry(entry);

            var commandBuilderFactory = new FakeCommandBuilderFactory(
                CreateFakeDataReader(new[] { "Col1" }, new List<object[]>()));

            var batch = new ModificationCommandBatchFake(factory: commandBuilderFactory);
            batch.AddCommand(command);

            var connection = CreateConnection();

            Assert.Equal(RelationalStrings.UpdateConcurrencyException(1, 0),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(connection))).Message);
        }

        [Fact]
        public void CreateStoreCommand_creates_parameters_for_each_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.MarkAsTemporary(property);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    new ParameterNameGenerator(),
                    p => p.TestProvider(),
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            new ParameterNameGenerator(),
                            false, true, false, false)
                    }));

            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    new ParameterNameGenerator(),
                    p => p.TestProvider(),
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            new ParameterNameGenerator(),
                            false, true, false, false)
                    }));

            var command = batch.CreateStoreCommandBase();

            Assert.Equal(2, command.Parameters.Count);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_write_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.MarkAsTemporary(property);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    new ParameterNameGenerator(),
                    p => p.TestProvider(),
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            new ParameterNameGenerator(),
                            false, true, false, false)
                    }));

            var command = batch.CreateStoreCommandBase();

            Assert.Equal(1, command.Parameters.Count);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_condition_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.MarkAsTemporary(property);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    new ParameterNameGenerator(),
                    p => p.TestProvider(),
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            new ParameterNameGenerator(),
                            false, false, false, true)
                    }));

            var command = batch.CreateStoreCommandBase();

            Assert.Equal(1, command.Parameters.Count);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_write_and_condition_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.MarkAsTemporary(property);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    new ParameterNameGenerator(),
                    p => p.TestProvider(),
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            new ParameterNameGenerator(),
                            false, true, false, true)
                    }));

            var command = batch.CreateStoreCommandBase();

            Assert.Equal(2, command.Parameters.Count);
        }

        [Fact]
        public void PopulateParameters_does_not_create_parameter_for_read_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.MarkAsTemporary(property);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    new ParameterNameGenerator(),
                    p => p.TestProvider(),
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            new ParameterNameGenerator(),
                            true, false, false, false)
                    }));

            var command = batch.CreateStoreCommandBase();

            Assert.Equal(0, command.Parameters.Count);
        }

        private class T1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static IModel BuildModel(bool generateKeyValues, bool computeNonKeyValue)
        {
            var model = new EntityFrameworkCore.Metadata.Internal.Model();

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

        private static FakeDbDataReader CreateFakeDataReader(string[] columnNames = null, IList<object[]> results = null)
        {
            results = results ?? new List<object[]> { new object[] { 1 } };
            columnNames = columnNames ?? new[] { "RowsAffected" };

            return new FakeDbDataReader(columnNames, results);
        }

        private class ModificationCommandBatchFake : AffectedCountModificationCommandBatch
        {
            public ModificationCommandBatchFake(
                IUpdateSqlGenerator sqlGenerator = null,
                IRelationalCommandBuilderFactory factory = null)
                : base(
                    factory ?? new FakeCommandBuilderFactory(),
                    new RelationalSqlGenerationHelper(),
                    sqlGenerator ?? new FakeSqlGenerator(),
                    new TypedRelationalValueBufferFactoryFactory())
            {
                ShouldAddCommand = true;
                ShouldValidateSql = true;
            }

            public string CommandText => GetCommandText();

            public bool ShouldAddCommand { get; set; }

            protected override bool CanAddCommand(ModificationCommand modificationCommand) => ShouldAddCommand;

            public bool ShouldValidateSql { get; set; }

            protected override bool IsCommandTextValid() => ShouldValidateSql;

            protected override void UpdateCachedCommandText(int commandIndex)
            {
                CachedCommandText = CachedCommandText ?? new StringBuilder();
                CachedCommandText.Append(".");
            }

            public void UpdateCachedCommandTextBase(int commandIndex) => base.UpdateCachedCommandText(commandIndex);

            public IRelationalCommand CreateStoreCommandBase() => CreateStoreCommand();
        }

        private class FakeModificationCommand : ModificationCommand
        {
            public FakeModificationCommand(
                string name,
                string schema,
                ParameterNameGenerator parameterNameGenerator,
                Func<IProperty, IRelationalPropertyAnnotations> getPropertyExtensions,
                IReadOnlyList<ColumnModification> columnModifications)
                : base(name, schema, parameterNameGenerator, getPropertyExtensions)
            {
                ColumnModifications = columnModifications;
            }

            public override IReadOnlyList<ColumnModification> ColumnModifications { get; }
        }

        private class FakeCommandBuilderFactory : IRelationalCommandBuilderFactory
        {
            private readonly DbDataReader _reader;

            public FakeCommandBuilderFactory(DbDataReader reader = null)
            {
                _reader = reader;
            }

            public IRelationalCommandBuilder Create() => new FakeCommandBuilder(_reader);
        }

        private class FakeCommandBuilder : IRelationalCommandBuilder
        {
            private readonly DbDataReader _reader;
            private readonly List<IRelationalParameter> _parameters = new List<IRelationalParameter>();

            public FakeCommandBuilder(DbDataReader reader = null)
            {
                _reader = reader;
            }

            public IndentedStringBuilder Instance { get; } = new IndentedStringBuilder();

            public void AddParameter(IRelationalParameter relationalParameter) => _parameters.Add(relationalParameter);

            public IRelationalParameter CreateParameter(string name, object value, Func<IRelationalTypeMapper, RelationalTypeMapping> mapType, bool? nullable, string invariantName) 
                => new RelationalParameter(name, value, new RelationalTypeMapping("name", typeof(Type)), null, invariantName);

            public IRelationalCommand Build()
                => new FakeRelationalCommand(
                    Instance.ToString(),
                    _parameters,
                    _reader);
        }

        private class FakeRelationalCommand : IRelationalCommand
        {
            private readonly DbDataReader _reader;

            public FakeRelationalCommand(
                string commandText,
                IReadOnlyList<IRelationalParameter> parameters,
                DbDataReader reader)
            {
                CommandText = commandText;
                Parameters = parameters;

                _reader = reader;
            }

            public string CommandText { get; }

            public IReadOnlyList<IRelationalParameter> Parameters { get; }

            public int ExecuteNonQuery(IRelationalConnection connection, bool manageConnection = true)
            {
                throw new NotImplementedException();
            }

            public Task<int> ExecuteNonQueryAsync(IRelationalConnection connection, bool manageConnection = true, CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }

            public object ExecuteScalar(IRelationalConnection connection, bool manageConnection = true)
            {
                throw new NotImplementedException();
            }

            public Task<object> ExecuteScalarAsync(IRelationalConnection connection, bool manageConnection = true, CancellationToken cancellationToken = default(CancellationToken))
            {
                throw new NotImplementedException();
            }

            public RelationalDataReader ExecuteReader(IRelationalConnection connection, bool manageConnection = true, IReadOnlyDictionary<string, object> parameters = null)
                => new RelationalDataReader(null, new FakeDbCommand(), _reader);

            public Task<RelationalDataReader> ExecuteReaderAsync(IRelationalConnection connection, bool manageConnection = true, IReadOnlyDictionary<string, object> parameters = null, CancellationToken cancellationToken = default(CancellationToken))
                => Task.FromResult(new RelationalDataReader(null, new FakeDbCommand(), _reader));
        }

        private const string ConnectionString = "Fake Connection String";

        private static FakeRelationalConnection CreateConnection(IDbContextOptions options = null)
            => new FakeRelationalConnection(options ?? CreateOptions());

        public static IDbContextOptions CreateOptions(FakeRelationalOptionsExtension optionsExtension = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(optionsExtension ?? new FakeRelationalOptionsExtension { ConnectionString = ConnectionString });

            return optionsBuilder.Options;
        }
    }
}
