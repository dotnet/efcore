// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Update.Internal;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Microsoft.EntityFrameworkCore.Update;

public class ReaderModificationCommandBatchTest
{
    [ConditionalFact]
    public void TryAddCommand_adds_command_if_batch_is_valid()
    {
        var parameterNameGenerator = new ParameterNameGenerator();

        var entry1 = CreateEntry(EntityState.Modified);
        var property1 = entry1.EntityType.FindProperty("Name")!;
        var command1 = CreateModificationCommand(
            "T1",
            null,
            true,
            new[]
            {
                new ColumnModificationParameters(
                    entry1,
                    property1,
                    property1.GetTableColumnMappings().Single().Column,
                    parameterNameGenerator.GenerateNext,
                    property1.GetTableColumnMappings().Single().TypeMapping,
                    false, true, false, false, true)
            });

        var entry2 = CreateEntry(EntityState.Modified);
        var property2 = entry2.EntityType.FindProperty("Name")!;
        var command2 = CreateModificationCommand(
            "T2",
            null,
            true,
            new[]
            {
                new ColumnModificationParameters(
                    entry2,
                    property2,
                    property2.GetTableColumnMappings().Single().Column,
                    parameterNameGenerator.GenerateNext,
                    property2.GetTableColumnMappings().Single().TypeMapping,
                    false, true, false, false, true)
            });

        var batch = new ModificationCommandBatchFake { ShouldBeValid = true };
        Assert.True(batch.TryAddCommand(command1));
        Assert.True(batch.TryAddCommand(command2));
        batch.Complete(moreBatchesExpected: false);

        Assert.Collection(
            batch.ModificationCommands,
            m => Assert.Same(command1, m),
            m => Assert.Same(command2, m));

        Assert.Equal(
            @"UPDATE ""T1"" SET ""Col2"" = @p0
RETURNING 1;
UPDATE ""T2"" SET ""Col2"" = @p1
RETURNING 1;
",
            batch.CommandText,
            ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void TryAddCommand_does_not_add_command_batch_is_invalid()
    {
        var parameterNameGenerator = new ParameterNameGenerator();

        var entry1 = CreateEntry(EntityState.Modified);
        var property1 = entry1.EntityType.FindProperty("Name")!;
        var command1 = CreateModificationCommand(
            "T1",
            null,
            true,
            new[]
            {
                new ColumnModificationParameters(
                    entry1,
                    property1,
                    property1.GetTableColumnMappings().Single().Column,
                    parameterNameGenerator.GenerateNext,
                    property1.GetTableColumnMappings().Single().TypeMapping,
                    false, true, false, false, true)
            });

        var entry2 = CreateEntry(EntityState.Modified);
        var property2 = entry2.EntityType.FindProperty("Name")!;
        var command2 = CreateModificationCommand(
            "T2",
            null,
            true,
            new[]
            {
                new ColumnModificationParameters(
                    entry2,
                    property2,
                    property2.GetTableColumnMappings().Single().Column,
                    parameterNameGenerator.GenerateNext,
                    property2.GetTableColumnMappings().Single().TypeMapping,
                    false, true, false, false, true)
            });

        var batch = new ModificationCommandBatchFake();
        Assert.True(batch.TryAddCommand(command1));
        batch.ShouldBeValid = false;

        Assert.False(batch.TryAddCommand(command2));
        batch.Complete(moreBatchesExpected: false);

        Assert.Same(command1, Assert.Single(batch.ModificationCommands));

        Assert.Equal(
            @"UPDATE ""T1"" SET ""Col2"" = @p0
RETURNING 1;
",
            batch.CommandText,
            ignoreLineEndingDifferences: true);

        Assert.Equal(1, batch.StoreCommand.RelationalCommand.Parameters.Count);
        Assert.Equal(1, batch.StoreCommand.ParameterValues.Count);
    }

    [ConditionalFact]
    public void Parameters_are_properly_managed_when_command_adding_fails()
    {
        var entry1 = CreateEntry(EntityState.Added);
        var command1 = CreateModificationCommand(entry1, new ParameterNameGenerator().GenerateNext, true, null);
        command1.AddEntry(entry1, true);

        var entry2 = CreateEntry(EntityState.Added);
        var command2 = CreateModificationCommand(entry2, new ParameterNameGenerator().GenerateNext, true, null);
        command2.AddEntry(entry2, true);

        var batch1 = new ModificationCommandBatchFake(maxBatchSize: 1);
        Assert.True(batch1.TryAddCommand(command1));
        Assert.False(batch1.TryAddCommand(command2));
        batch1.Complete(moreBatchesExpected: false);

        var batch2 = new ModificationCommandBatchFake(maxBatchSize: 1);
        Assert.True(batch2.TryAddCommand(command1));
        batch2.Complete(moreBatchesExpected: false);

        Assert.Equal(2, batch1.StoreCommand.RelationalCommand.Parameters.Count);
        Assert.Equal("p0", batch1.StoreCommand.RelationalCommand.Parameters[0].InvariantName);
        Assert.Equal("p1", batch1.StoreCommand.RelationalCommand.Parameters[1].InvariantName);

        Assert.Equal(2, batch2.StoreCommand.RelationalCommand.Parameters.Count);
        Assert.Equal("p0", batch2.StoreCommand.RelationalCommand.Parameters[0].InvariantName);
        Assert.Equal("p1", batch2.StoreCommand.RelationalCommand.Parameters[1].InvariantName);

        Assert.Equal(1, batch1.FakeSqlGenerator.AppendBatchHeaderCalls);
        Assert.Equal(1, batch1.FakeSqlGenerator.AppendInsertOperationCalls);

        Assert.Equal(1, batch2.FakeSqlGenerator.AppendBatchHeaderCalls);
        Assert.Equal(1, batch2.FakeSqlGenerator.AppendInsertOperationCalls);
    }

    [ConditionalFact]
    public void TryAddCommand_with_insert()
    {
        var entry = CreateEntry(EntityState.Added);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        Assert.Equal(1, batch.FakeSqlGenerator.AppendBatchHeaderCalls);
        Assert.Equal(1, batch.FakeSqlGenerator.AppendInsertOperationCalls);
    }

    [ConditionalFact]
    public void TryAddCommand_with_update()
    {
        var entry = CreateEntry(EntityState.Modified, generateKeyValues: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        Assert.Equal(1, batch.FakeSqlGenerator.AppendBatchHeaderCalls);
        Assert.Equal(1, batch.FakeSqlGenerator.AppendUpdateOperationCalls);
    }

    [ConditionalFact]
    public void TryAddCommand_with_delete()
    {
        var entry = CreateEntry(EntityState.Deleted);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        Assert.Equal(1, batch.FakeSqlGenerator.AppendBatchHeaderCalls);
        Assert.Equal(1, batch.FakeSqlGenerator.AppendDeleteOperationCalls);
    }

    [ConditionalFact]
    public void TryAddCommand_twice()
    {
        var entry = CreateEntry(EntityState.Added);

        var parameterNameGenerator = new ParameterNameGenerator();
        var command1 = CreateModificationCommand(entry, parameterNameGenerator.GenerateNext, true, null);
        command1.AddEntry(entry, true);
        var command2 = CreateModificationCommand(entry, parameterNameGenerator.GenerateNext, true, null);
        command2.AddEntry(entry, true);

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command1);
        batch.TryAddCommand(command2);
        batch.Complete(moreBatchesExpected: false);

        Assert.Equal(1, batch.FakeSqlGenerator.AppendBatchHeaderCalls);
        Assert.Equal(2, batch.FakeSqlGenerator.AppendInsertOperationCalls);
    }

    [ConditionalFact]
    public async Task ExecuteAsync_executes_batch_commands_and_consumes_reader()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var dbDataReader = CreateFakeDataReader();

        var connection = CreateConnection(dbDataReader);

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        await batch.ExecuteAsync(connection);

        Assert.Equal(1, dbDataReader.ReadAsyncCount);
        Assert.Equal(1, dbDataReader.GetInt32Count);
    }

    [ConditionalFact]
    public async Task ExecuteAsync_saves_store_generated_values()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var connection = CreateConnection(
            CreateFakeDataReader(
                ["Col1"], new List<object[]> { new object[] { 42 } }));

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        await batch.ExecuteAsync(connection);

        Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
        Assert.Equal("Test", entry[entry.EntityType.FindProperty("Name")]);
    }

    [ConditionalFact]
    public async Task ExecuteAsync_saves_store_generated_values_on_non_key_columns()
    {
        var entry = CreateEntry(
            EntityState.Added, generateKeyValues: true, computeNonKeyValue: true);
        entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var connection = CreateConnection(
            CreateFakeDataReader(
                ["Col1", "Col2"], new List<object[]> { new object[] { 42, "FortyTwo" } }));

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        await batch.ExecuteAsync(connection);

        Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
        Assert.Equal("FortyTwo", entry[entry.EntityType.FindProperty("Name")]);
    }

    [ConditionalFact]
    public async Task ExecuteAsync_saves_store_generated_values_when_updating()
    {
        var entry = CreateEntry(
            EntityState.Modified, generateKeyValues: true, overrideKeyValues: true, computeNonKeyValue: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var connection = CreateConnection(
            CreateFakeDataReader(
                ["Col2"], new List<object[]> { new object[] { "FortyTwo" } }));

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        await batch.ExecuteAsync(connection);

        Assert.Equal(1, entry[entry.EntityType.FindProperty("Id")]);
        Assert.Equal("FortyTwo", entry[entry.EntityType.FindProperty("Name")]);
    }

    [ConditionalFact]
    public async Task Exception_not_thrown_for_more_than_one_row_returned_for_single_command()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var connection = CreateConnection(
            CreateFakeDataReader(
                ["Col1"],
                new List<object[]> { new object[] { 42 }, new object[] { 43 } }));

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        await batch.ExecuteAsync(connection);

        Assert.Equal(42, entry[entry.EntityType.FindProperty("Id")]);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Exception_thrown_if_rows_returned_for_command_without_store_generated_values_is_not_1(bool async)
    {
        var entry = CreateEntry(EntityState.Modified);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var connection = CreateConnection(
            CreateFakeDataReader(
                ["Col1"], new List<object[]> { new object[] { 42 } }));

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        var exception = async
            ? await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => batch.ExecuteAsync(connection))
            : Assert.Throws<DbUpdateConcurrencyException>(() => batch.Execute(connection));

        Assert.Equal(RelationalStrings.UpdateConcurrencyException(1, 42), exception.Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Exception_thrown_if_no_rows_returned_for_command_with_store_generated_values(bool async)
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        entry.SetTemporaryValue(entry.EntityType.FindPrimaryKey().Properties[0], -1);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var connection = CreateConnection(
            CreateFakeDataReader(["Col1"], new List<object[]>()));

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        var exception = async
            ? await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => batch.ExecuteAsync(connection))
            : Assert.Throws<DbUpdateConcurrencyException>(() => batch.Execute(connection));

        Assert.Equal(RelationalStrings.UpdateConcurrencyException(1, 0), exception.Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DbException_is_wrapped_with_DbUpdateException(bool async)
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var originalException = new FakeDbException();

        var connection = CreateConnection(
            new FakeCommandExecutor(
                executeReaderAsync: (c, b, ct) => throw originalException,
                executeReader: (c, b) => throw originalException));

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        var actualException = async
            ? await Assert.ThrowsAsync<DbUpdateException>(() => batch.ExecuteAsync(connection))
            : Assert.Throws<DbUpdateException>(() => batch.Execute(connection));

        Assert.Same(originalException, actualException.InnerException);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OperationCanceledException_is_not_wrapped_with_DbUpdateException(bool async)
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);

        var command = CreateModificationCommand(entry, new ParameterNameGenerator().GenerateNext, true, null);
        command.AddEntry(entry, true);

        var originalException = new OperationCanceledException();

        var connection = CreateConnection(
            new FakeCommandExecutor(
                executeReaderAsync: (c, b, ct) => throw originalException,
                executeReader: (c, b) => throw originalException));

        var batch = new ModificationCommandBatchFake();
        batch.TryAddCommand(command);
        batch.Complete(moreBatchesExpected: false);

        var actualException = async
            ? await Assert.ThrowsAsync<OperationCanceledException>(() => batch.ExecuteAsync(connection))
            : Assert.Throws<OperationCanceledException>(() => batch.Execute(connection));

        Assert.Same(originalException, actualException);
    }

    [ConditionalFact]
    public void CreateStoreCommand_creates_parameters_for_each_ModificationCommand()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        var property = entry.EntityType.FindProperty("Id");
        entry.SetTemporaryValue(property, 1);

        var batch = new ModificationCommandBatchFake();
        var parameterNameGenerator = new ParameterNameGenerator();

        batch.TryAddCommand(
            CreateModificationCommand(
                "T1",
                null,
                true,
                new[]
                {
                    new ColumnModificationParameters(
                        entry,
                        property,
                        property.GetTableColumnMappings().Single().Column,
                        parameterNameGenerator.GenerateNext,
                        property.GetTableColumnMappings().Single().TypeMapping,
                        false, true, false, false, true)
                }));

        batch.TryAddCommand(
            CreateModificationCommand(
                "T1",
                null,
                true,
                new[]
                {
                    new ColumnModificationParameters(
                        entry,
                        property,
                        property.GetTableColumnMappings().Single().Column,
                        parameterNameGenerator.GenerateNext,
                        property.GetTableColumnMappings().Single().TypeMapping,
                        false, true, false, false, true)
                }));

        batch.Complete(moreBatchesExpected: false);

        var storeCommand = batch.StoreCommand;

        Assert.Equal(2, storeCommand.RelationalCommand.Parameters.Count);
        Assert.Equal("p0", storeCommand.RelationalCommand.Parameters[0].InvariantName);
        Assert.Equal("p1", storeCommand.RelationalCommand.Parameters[1].InvariantName);

        Assert.Equal(2, storeCommand.ParameterValues.Count);
        Assert.Equal(1, storeCommand.ParameterValues["p0"]);
        Assert.Equal(1, storeCommand.ParameterValues["p1"]);
    }

    [ConditionalFact]
    public void PopulateParameters_creates_parameter_for_write_ModificationCommand()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        var property = entry.EntityType.FindProperty("Id");
        entry.SetTemporaryValue(property, 1);

        var batch = new ModificationCommandBatchFake();
        var parameterNameGenerator = new ParameterNameGenerator();
        batch.TryAddCommand(
            CreateModificationCommand(
                "T1",
                null,
                true,
                new[]
                {
                    new ColumnModificationParameters(
                        entry,
                        property,
                        property.GetTableColumnMappings().Single().Column,
                        parameterNameGenerator.GenerateNext,
                        property.GetTableColumnMappings().Single().TypeMapping,
                        valueIsRead: false, valueIsWrite: true, columnIsKey: false, columnIsCondition: false,
                        sensitiveLoggingEnabled: true)
                }));

        batch.Complete(moreBatchesExpected: false);

        var storeCommand = batch.StoreCommand;

        Assert.Equal(1, storeCommand.RelationalCommand.Parameters.Count);
        Assert.Equal("p0", storeCommand.RelationalCommand.Parameters[0].InvariantName);

        Assert.Equal(1, storeCommand.ParameterValues.Count);
        Assert.Equal(1, storeCommand.ParameterValues["p0"]);
    }

    [ConditionalFact]
    public void PopulateParameters_creates_parameter_for_condition_ModificationCommand()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        var property = entry.EntityType.FindProperty("Id");
        entry.SetTemporaryValue(property, 1);

        var batch = new ModificationCommandBatchFake();
        var parameterNameGenerator = new ParameterNameGenerator();
        batch.TryAddCommand(
            CreateModificationCommand(
                "T1",
                null,
                true,
                new[]
                {
                    new ColumnModificationParameters(
                        entry,
                        property,
                        property.GetTableColumnMappings().Single().Column,
                        parameterNameGenerator.GenerateNext,
                        property.GetTableColumnMappings().Single().TypeMapping,
                        valueIsRead: false, valueIsWrite: false, columnIsKey: false, columnIsCondition: true,
                        sensitiveLoggingEnabled: true)
                }));

        batch.Complete(moreBatchesExpected: false);

        var storeCommand = batch.StoreCommand;

        Assert.Equal(1, storeCommand.RelationalCommand.Parameters.Count);
        Assert.Equal("p0", storeCommand.RelationalCommand.Parameters[0].InvariantName);

        Assert.Equal(1, storeCommand.ParameterValues.Count);
        Assert.Equal(1, storeCommand.ParameterValues["p0"]);
    }

    [ConditionalFact]
    public void PopulateParameters_creates_parameters_for_write_and_condition_ModificationCommand()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        var property = entry.EntityType.FindProperty("Id");
        entry.SetTemporaryValue(property, 1);

        var batch = new ModificationCommandBatchFake();
        var parameterNameGenerator = new ParameterNameGenerator();
        batch.TryAddCommand(
            CreateModificationCommand(
                "T1",
                null,
                true,
                new[]
                {
                    new ColumnModificationParameters(
                        entry,
                        property,
                        property.GetTableColumnMappings().Single().Column,
                        parameterNameGenerator.GenerateNext,
                        property.GetTableColumnMappings().Single().TypeMapping,
                        valueIsRead: false, valueIsWrite: true, columnIsKey: false, columnIsCondition: true,
                        sensitiveLoggingEnabled: true)
                }));

        batch.Complete(moreBatchesExpected: false);

        var storeCommand = batch.StoreCommand;

        Assert.Equal(2, storeCommand.RelationalCommand.Parameters.Count);
        Assert.Equal("p0", storeCommand.RelationalCommand.Parameters[0].InvariantName);
        Assert.Equal("p1", storeCommand.RelationalCommand.Parameters[1].InvariantName);

        Assert.Equal(2, storeCommand.ParameterValues.Count);
        Assert.Equal(1, storeCommand.ParameterValues["p0"]);
        Assert.Equal(1, storeCommand.ParameterValues["p1"]);
    }

    [ConditionalFact]
    public void PopulateParameters_does_not_create_parameter_for_read_ModificationCommand()
    {
        var entry = CreateEntry(EntityState.Added, generateKeyValues: true);
        var property = entry.EntityType.FindProperty("Id");
        entry.SetTemporaryValue(property, -1);

        var batch = new ModificationCommandBatchFake();
        var parameterNameGenerator = new ParameterNameGenerator();
        batch.TryAddCommand(
            CreateModificationCommand(
                "T1",
                null,
                true,
                new[]
                {
                    new ColumnModificationParameters(
                        entry,
                        property,
                        property.GetTableColumnMappings().Single().Column,
                        parameterNameGenerator.GenerateNext,
                        property.GetTableColumnMappings().Single().TypeMapping,
                        valueIsRead: true, valueIsWrite: false, columnIsKey: false, columnIsCondition: false,
                        sensitiveLoggingEnabled: true)
                }));

        batch.Complete(moreBatchesExpected: false);

        var storeCommand = batch.StoreCommand;

        Assert.Equal(0, storeCommand.RelationalCommand.Parameters.Count);
    }

    private class T1
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    private static IModel BuildModel(bool generateKeyValues, bool computeNonKeyValue)
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        var entityType = modelBuilder.Entity<T1>();

        entityType.Property(t => t.Id).HasColumnName("Col1");
        if (!generateKeyValues)
        {
            entityType.Property(t => t.Id).ValueGeneratedNever();
        }

        entityType.Property(t => t.Name).HasColumnName("Col2");
        if (computeNonKeyValue)
        {
            entityType.Property(t => t.Name).ValueGeneratedOnAddOrUpdate();
        }

        return modelBuilder.FinalizeModel();
    }

    private static InternalEntityEntry CreateEntry(
        EntityState entityState,
        bool generateKeyValues = false,
        bool overrideKeyValues = false,
        bool computeNonKeyValue = false)
    {
        var model = BuildModel(generateKeyValues, computeNonKeyValue);

        return FakeRelationalTestHelpers.Instance.CreateInternalEntry(
            model, entityState, new T1 { Id = overrideKeyValues ? 1 : default, Name = computeNonKeyValue ? null : "Test" });
    }

    private static FakeDbDataReader CreateFakeDataReader(string[] columnNames = null, IList<object[]> results = null)
    {
        results ??= new List<object[]> { new object[] { 1 } };
        columnNames ??= ["RowsAffected"];

        return new FakeDbDataReader(columnNames, results);
    }

    private class ModificationCommandBatchFake : AffectedCountModificationCommandBatch
    {
        private readonly FakeSqlGenerator _fakeSqlGenerator;

        public ModificationCommandBatchFake(IUpdateSqlGenerator sqlGenerator = null, int? maxBatchSize = null)
            : base(CreateDependencies(sqlGenerator), maxBatchSize)
        {
            ShouldBeValid = true;

            _fakeSqlGenerator = Dependencies.UpdateSqlGenerator as FakeSqlGenerator;
        }

        private static ModificationCommandBatchFactoryDependencies CreateDependencies(
            IUpdateSqlGenerator sqlGenerator)
        {
            var typeMappingSource = new TestRelationalTypeMappingSource(
                TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

            var logger = new FakeRelationalCommandDiagnosticsLogger();

            sqlGenerator ??= new FakeSqlGenerator(
                FakeRelationalTestHelpers.Instance.CreateContextServices()
                    .GetRequiredService<UpdateSqlGeneratorDependencies>());

            return new ModificationCommandBatchFactoryDependencies(
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        typeMappingSource,
                        new ExceptionDetector())),
                new RelationalSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                sqlGenerator,
                new CurrentDbContext(new FakeDbContext()),
                logger,
                new FakeDiagnosticsLogger<DbLoggerCategory.Update>());
        }

        public string CommandText
            => SqlBuilder.ToString();

        public bool ShouldBeValid { get; set; }

        protected override bool IsValid()
            => ShouldBeValid;

        public new RawSqlCommand StoreCommand
            => base.StoreCommand;

        public FakeSqlGenerator FakeSqlGenerator
            => _fakeSqlGenerator ?? throw new InvalidOperationException("Not using FakeSqlGenerator");
    }

    private class FakeDbContext : DbContext;

    private const string ConnectionString = "Fake Connection String";

    private static FakeRelationalConnection CreateConnection(FakeCommandExecutor executor)
        => CreateConnection(
            CreateOptions(
                new FakeRelationalOptionsExtension().WithConnection(
                    new FakeDbConnection(ConnectionString, executor))));

    private static FakeRelationalConnection CreateConnection(DbDataReader dbDataReader)
        => CreateConnection(
            new FakeCommandExecutor(
                executeReaderAsync: (c, b, ct) => Task.FromResult(dbDataReader),
                executeReader: (c, b) => dbDataReader));

    private static FakeRelationalConnection CreateConnection(IDbContextOptions options = null)
        => new(options ?? CreateOptions());

    public static IDbContextOptions CreateOptions(RelationalOptionsExtension optionsExtension = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                optionsExtension
                ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

        return optionsBuilder.Options;
    }

    private static IModificationCommand CreateModificationCommand(
        InternalEntityEntry entry,
        Func<string> generateParameterName,
        bool sensitiveLoggingEnabled,
        IComparer<IUpdateEntry> comparer)
    {
        var modificationCommandParameters = new ModificationCommandParameters(
            entry.EntityType.GetTableMappings().Single().Table,
            sensitiveLoggingEnabled,
            detailedErrorsEnabled: false,
            comparer,
            generateParameterName,
            logger: null);
        return CreateModificationCommandSource().CreateModificationCommand(modificationCommandParameters);
    }

    private static INonTrackedModificationCommand CreateModificationCommand(
        string name,
        string schema,
        bool sensitiveLoggingEnabled,
        IReadOnlyList<ColumnModificationParameters> columnModifications)
    {
        var modificationCommand = CreateModificationCommandSource().CreateNonTrackedModificationCommand(
            new NonTrackedModificationCommandParameters(name, schema, sensitiveLoggingEnabled));

        if (columnModifications != null)
        {
            foreach (var columnModification in columnModifications)
            {
                modificationCommand.AddColumnModification(columnModification);
            }
        }

        return modificationCommand;
    }

    private static ModificationCommandFactory CreateModificationCommandSource()
        => new();
}
