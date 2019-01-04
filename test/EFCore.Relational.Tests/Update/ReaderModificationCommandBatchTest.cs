// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Update
{
    public class ReaderModificationCommandBatchTest
    {
        [Fact]
        public void AddCommand_adds_command_if_possible()
        {
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);

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
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);

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
            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);

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

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var fakeSqlGenerator = new FakeSqlGenerator(
                RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<UpdateSqlGeneratorDependencies>());
            var batch = new ModificationCommandBatchFake(fakeSqlGenerator);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            Assert.Equal(1, fakeSqlGenerator.AppendBatchHeaderCalls);
            Assert.Equal(1, fakeSqlGenerator.AppendInsertOperationCalls);
        }

        [Fact]
        public void UpdateCommandText_compiles_updates()
        {
            var entry = CreateEntry(EntityState.Modified, generateKeyValues: true);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var fakeSqlGenerator = new FakeSqlGenerator(
                RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<UpdateSqlGeneratorDependencies>());
            var batch = new ModificationCommandBatchFake(fakeSqlGenerator);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            Assert.Equal(1, fakeSqlGenerator.AppendBatchHeaderCalls);
            Assert.Equal(1, fakeSqlGenerator.AppendUpdateOperationCalls);
        }

        [Fact]
        public void UpdateCommandText_compiles_deletes()
        {
            var entry = CreateEntry(EntityState.Deleted);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var fakeSqlGenerator = new FakeSqlGenerator(
                RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<UpdateSqlGeneratorDependencies>());
            var batch = new ModificationCommandBatchFake(fakeSqlGenerator);
            batch.AddCommand(command);

            batch.UpdateCachedCommandTextBase(0);

            Assert.Equal(1, fakeSqlGenerator.AppendBatchHeaderCalls);
            Assert.Equal(1, fakeSqlGenerator.AppendDeleteOperationCalls);
        }

        [Fact]
        public void UpdateCommandText_compiles_multiple_commands()
        {
            var entry = CreateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var fakeSqlGenerator = new FakeSqlGenerator(
                RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<UpdateSqlGeneratorDependencies>());
            var batch = new ModificationCommandBatchFake(fakeSqlGenerator);
            batch.AddCommand(command);
            batch.AddCommand(command);

            Assert.Equal("..", batch.CommandText);

            Assert.Equal(1, fakeSqlGenerator.AppendBatchHeaderCalls);
        }

        [Fact]
        public async Task ExecuteAsync_executes_batch_commands_and_consumes_reader()
        {
            var entry = CreateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var dbDataReader = CreateFakeDataReader();

            var connection = CreateConnection(dbDataReader);

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            await batch.ExecuteAsync(connection);

            Assert.Equal(1, dbDataReader.ReadAsyncCount);
            Assert.Equal(1, dbDataReader.GetInt32Count);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var connection = CreateConnection(
                CreateFakeDataReader(
                    new[] { "Col1" }, new List<object[]>
                    {
                        new object[] { 42 }
                    }));

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
            Assert.Equal("Test", entry[entry.EntityType.FindProperty("Name")]);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values_on_non_key_columns()
        {
            var entry = CreateEntry(
                EntityState.Added, generateKeyValues: true, computeNonKeyValue: true);
            entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var connection = CreateConnection(
                CreateFakeDataReader(
                    new[] { "Col1", "Col2" }, new List<object[]>
                    {
                        new object[] { 42, "FortyTwo" }
                    }));

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
            Assert.Equal("FortyTwo", entry[entry.EntityType.FindProperty("Name")]);
        }

        [Fact]
        public async Task ExecuteAsync_saves_store_generated_values_when_updating()
        {
            var entry = CreateEntry(
                EntityState.Modified, generateKeyValues: true, computeNonKeyValue: true);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var connection = CreateConnection(
                CreateFakeDataReader(
                    new[] { "Col2" }, new List<object[]>
                    {
                        new object[] { "FortyTwo" }
                    }));

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            await batch.ExecuteAsync(connection);

            Assert.Equal(1, entry[entry.EntityType.FindProperty("Id")]);
            Assert.Equal("FortyTwo", entry[entry.EntityType.FindProperty("Name")]);
        }

        [Fact]
        public async Task Exception_not_thrown_for_more_than_one_row_returned_for_single_command()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var connection = CreateConnection(
                CreateFakeDataReader(
                    new[] { "Col1" },
                    new List<object[]>
                    {
                        new object[] { 42 },
                        new object[] { 43 }
                    }));

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            await batch.ExecuteAsync(connection);

            Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
        }

        [Fact]
        public async Task Exception_thrown_if_rows_returned_for_command_without_store_generated_values_is_not_1()
        {
            var entry = CreateEntry(EntityState.Added);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var connection = CreateConnection(
                CreateFakeDataReader(
                    new[] { "Col1" }, new List<object[]>
                    {
                        new object[] { 42 }
                    }));

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            Assert.Equal(
                RelationalStrings.UpdateConcurrencyException(1, 42),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(connection))).Message);
        }

        [Fact]
        public async Task Exception_thrown_if_no_rows_returned_for_command_with_store_generated_values()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

            var command = new ModificationCommand("T1", null, new ParameterNameGenerator().GenerateNext, true, null);
            command.AddEntry(entry);

            var connection = CreateConnection(
                CreateFakeDataReader(new[] { "Col1" }, new List<object[]>()));

            var batch = new ModificationCommandBatchFake();
            batch.AddCommand(command);

            Assert.Equal(
                RelationalStrings.UpdateConcurrencyException(1, 0),
                (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await batch.ExecuteAsync(connection))).Message);
        }

        [Fact]
        public void CreateStoreCommand_creates_parameters_for_each_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.SetTemporaryValue(property, 1);

            var batch = new ModificationCommandBatchFake();
            var parameterNameGenerator = new ParameterNameGenerator();

            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    parameterNameGenerator.GenerateNext,
                    true,
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            parameterNameGenerator.GenerateNext,
                            false, true, false, false, false)
                    }));

            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    parameterNameGenerator.GenerateNext,
                    true,
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            parameterNameGenerator.GenerateNext,
                            false, true, false, false, false)
                    }));

            var storeCommand = batch.CreateStoreCommandBase();

            Assert.Equal(2, storeCommand.RelationalCommand.Parameters.Count);
            Assert.Equal("p0", storeCommand.RelationalCommand.Parameters[0].InvariantName);
            Assert.Equal("p1", storeCommand.RelationalCommand.Parameters[1].InvariantName);

            Assert.Equal(2, storeCommand.ParameterValues.Count);
            Assert.Equal(1, storeCommand.ParameterValues["p0"]);
            Assert.Equal(1, storeCommand.ParameterValues["p1"]);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_write_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.SetTemporaryValue(property, 1);

            var batch = new ModificationCommandBatchFake();
            var parameterNameGenerator = new ParameterNameGenerator();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    parameterNameGenerator.GenerateNext,
                    true,
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            parameterNameGenerator.GenerateNext,
                            false, true, false, false, false)
                    }));

            var storeCommand = batch.CreateStoreCommandBase();

            Assert.Equal(1, storeCommand.RelationalCommand.Parameters.Count);
            Assert.Equal("p0", storeCommand.RelationalCommand.Parameters[0].InvariantName);

            Assert.Equal(1, storeCommand.ParameterValues.Count);
            Assert.Equal(1, storeCommand.ParameterValues["p0"]);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_condition_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.SetTemporaryValue(property, 1);

            var batch = new ModificationCommandBatchFake();
            var parameterNameGenerator = new ParameterNameGenerator();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    parameterNameGenerator.GenerateNext,
                    true,
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            parameterNameGenerator.GenerateNext,
                            false, false, false, true, false)
                    }));

            var storeCommand = batch.CreateStoreCommandBase();

            Assert.Equal(1, storeCommand.RelationalCommand.Parameters.Count);
            Assert.Equal("p0", storeCommand.RelationalCommand.Parameters[0].InvariantName);

            Assert.Equal(1, storeCommand.ParameterValues.Count);
            Assert.Equal(1, storeCommand.ParameterValues["p0"]);
        }

        [Fact]
        public void PopulateParameters_creates_parameter_for_write_and_condition_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.SetTemporaryValue(property, 1);

            var batch = new ModificationCommandBatchFake();
            var parameterNameGenerator = new ParameterNameGenerator();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    parameterNameGenerator.GenerateNext,
                    true,
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            parameterNameGenerator.GenerateNext,
                            false, true, false, true, false)
                    }));

            var storeCommand = batch.CreateStoreCommandBase();

            Assert.Equal(1, storeCommand.RelationalCommand.Parameters.Count);
            Assert.Equal("p0", storeCommand.RelationalCommand.Parameters[0].InvariantName);

            Assert.Equal(1, storeCommand.ParameterValues.Count);
            Assert.Equal(1, storeCommand.ParameterValues["p0"]);
        }

        [Fact]
        public void PopulateParameters_does_not_create_parameter_for_read_ModificationCommand()
        {
            var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
            var property = entry.EntityType.FindProperty("Id");
            entry.SetTemporaryValue(property, -1);

            var batch = new ModificationCommandBatchFake();
            var parameterNameGenerator = new ParameterNameGenerator();
            batch.AddCommand(
                new FakeModificationCommand(
                    "T",
                    "S",
                    parameterNameGenerator.GenerateNext,
                    true,
                    new List<ColumnModification>
                    {
                        new ColumnModification(
                            entry,
                            property,
                            property.TestProvider(),
                            parameterNameGenerator.GenerateNext,
                            true, false, false, false, false)
                    }));

            var storeCommand = batch.CreateStoreCommandBase();

            Assert.Equal(0, storeCommand.RelationalCommand.Parameters.Count);
        }

        private class T1
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private static IModel BuildModel(bool generateKeyValues, bool computeNonKeyValue)
        {
            var model = new Model();

            var entityType = model.AddEntityType(typeof(T1));

            var key = entityType.AddProperty("Id", typeof(int));
            key.ValueGenerated = generateKeyValues ? ValueGenerated.OnAdd : ValueGenerated.Never;
            key.Relational().ColumnName = "Col1";
            entityType.GetOrSetPrimaryKey(key);

            var nonKey = entityType.AddProperty("Name", typeof(string));
            nonKey.Relational().ColumnName = "Col2";
            nonKey.ValueGenerated = computeNonKeyValue ? ValueGenerated.OnAddOrUpdate : ValueGenerated.Never;

            GenerateMapping(key);
            GenerateMapping(nonKey);

            return model;
        }

        private static void GenerateMapping(IMutableProperty property)
            => property[CoreAnnotationNames.TypeMapping] =
                new TestRelationalTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())
                    .FindMapping(property);

        private static InternalEntityEntry CreateEntry(
            EntityState entityState,
            bool generateKeyValues = false,
            bool computeNonKeyValue = false)
        {
            var model = BuildModel(generateKeyValues, computeNonKeyValue);

            return RelationalTestHelpers.Instance.CreateInternalEntry(
                model, entityState, new T1
                {
                    Id = 1,
                    Name = computeNonKeyValue ? null : "Test"
                });
        }

        private static FakeDbDataReader CreateFakeDataReader(string[] columnNames = null, IList<object[]> results = null)
        {
            results = results ?? new List<object[]>
            {
                new object[] { 1 }
            };
            columnNames = columnNames ?? new[] { "RowsAffected" };

            return new FakeDbDataReader(columnNames, results);
        }

        private class ModificationCommandBatchFake : AffectedCountModificationCommandBatch
        {
            public ModificationCommandBatchFake(
                IUpdateSqlGenerator sqlGenerator = null)
                : base(
                    new RelationalCommandBuilderFactory(
                        new FakeDiagnosticsLogger<DbLoggerCategory.Database.Command>(),
                        new TestRelationalTypeMappingSource(
                            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())),
                    new RelationalSqlGenerationHelper(
                        new RelationalSqlGenerationHelperDependencies()),
                    sqlGenerator ?? new FakeSqlGenerator(
                        RelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<UpdateSqlGeneratorDependencies>()),
                    new TypedRelationalValueBufferFactoryFactory(
                        new RelationalValueBufferFactoryDependencies(
                            new TestRelationalTypeMappingSource(
                                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                            new CoreSingletonOptions())))
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

            public RawSqlCommand CreateStoreCommandBase()
                => CreateStoreCommand();
        }

        private const string ConnectionString = "Fake Connection String";

        private static FakeRelationalConnection CreateConnection(DbDataReader dbDataReader)
        {
            var fakeDbConnection = new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeReaderAsync: (c, b, ct) => Task.FromResult(dbDataReader),
                    executeReader: (c, b) => dbDataReader));

            var optionsExtension = new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection);

            var options = CreateOptions(optionsExtension);

            return CreateConnection(options);
        }

        private static FakeRelationalConnection CreateConnection(IDbContextOptions options = null)
            => new FakeRelationalConnection(options ?? CreateOptions());

        public static IDbContextOptions CreateOptions(RelationalOptionsExtension optionsExtension = null)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(
                    optionsExtension
                    ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

            return optionsBuilder.Options;
        }
    }
}
