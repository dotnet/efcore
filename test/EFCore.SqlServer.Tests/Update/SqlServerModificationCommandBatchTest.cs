// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Update.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update;

public class SqlServerModificationCommandBatchTest
{
    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Deleted)]
    [InlineData(EntityState.Modified)]
    public void AddCommand_returns_false_when_max_batch_size_is_reached(EntityState entityState)
    {
        var batch = CreateBatch(maxBatchSize: 1);

        var firstCommand = CreateModificationCommand("T1", null, false);
        firstCommand.EntityState = entityState;
        var secondCommand = CreateModificationCommand("T1", null, false);
        secondCommand.EntityState = entityState;

        Assert.True(batch.TryAddCommand(firstCommand));
        Assert.False(batch.TryAddCommand(secondCommand));

        Assert.Same(firstCommand, Assert.Single(batch.ModificationCommands));
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added, true)]
    [InlineData(EntityState.Added, false)]
    [InlineData(EntityState.Deleted, true)]
    [InlineData(EntityState.Deleted, false)]
    [InlineData(EntityState.Modified, true)]
    [InlineData(EntityState.Modified, false)]
    public void AddCommand_returns_false_when_max_parameters_are_reached(EntityState entityState, bool withSameTable)
    {
        var typeMapper = CreateTypeMappingSource();
        var intMapping = typeMapper.FindMapping(typeof(int));
        var paramIndex = 0;

        var batch = CreateBatch();

        var command = CreateModificationCommand("T1", null, false);
        command.EntityState = entityState;
        for (var i = 0; i < 2098; i++)
        {
            command.AddColumnModification(CreateModificationParameters("col" + i));
        }

        Assert.True(batch.TryAddCommand(command));

        var secondCommand = CreateModificationCommand("T2", null, false);
        secondCommand.EntityState = entityState;
        secondCommand.AddColumnModification(CreateModificationParameters("col"));
        Assert.False(batch.TryAddCommand(secondCommand));
        Assert.Same(command, Assert.Single(batch.ModificationCommands));
        Assert.Equal(2098, batch.ParameterValues.Count);

        ColumnModificationParameters CreateModificationParameters(string columnName)
            => new()
            {
                ColumnName = columnName,
                ColumnType = "integer",
                TypeMapping = intMapping,
                IsWrite = true,
                OriginalValue = 8,
                GenerateParameterName = () => "p" + paramIndex++
            };
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void AddCommand_when_max_parameters_are_reached_with_pending_commands(bool lastCommandPending)
    {
        var typeMapper = CreateTypeMappingSource();
        var intMapping = typeMapper.FindMapping(typeof(int));
        var paramIndex = 0;

        var batch = CreateBatch();

        for (var i = 0; i < 20; i++)
        {
            var pendingCommand = CreateModificationCommand("T1", null, false);
            pendingCommand.EntityState = EntityState.Added;
            for (var j = 0; j < 100; j++)
            {
                pendingCommand.AddColumnModification(CreateModificationParameters("col" + j));
            }

            Assert.True(batch.TryAddCommand(pendingCommand));
        }

        // We now have 20 pending commands with a total of 2000 parameters.
        // Add another command - either compatible with the pending ones or not - and which also gets us past the 2098 parameter limit.
        var command = CreateModificationCommand(lastCommandPending ? "T1" : "T2", null, false);
        command.EntityState = EntityState.Added;
        for (var i = 0; i < 100; i++)
        {
            command.AddColumnModification(CreateModificationParameters("col" + i));
        }

        Assert.False(batch.TryAddCommand(command));

        batch.Complete(moreBatchesExpected: false);

        Assert.Equal(2000, batch.ParameterValues.Count);
        Assert.Contains("INSERT", batch.StoreCommand.RelationalCommand.CommandText);
        Assert.Equal(20, batch.ResultSetMappings.Count);

        ColumnModificationParameters CreateModificationParameters(string columnName)
            => new()
            {
                ColumnName = columnName,
                ColumnType = "integer",
                TypeMapping = intMapping,
                IsWrite = true,
                OriginalValue = 8,
                GenerateParameterName = () => "p" + paramIndex++
            };
    }

    private class FakeDbContext : DbContext;

    private static TestSqlServerModificationCommandBatch CreateBatch(int maxBatchSize = 42)
    {
        var typeMapper = CreateTypeMappingSource();

        return new TestSqlServerModificationCommandBatch(
            new ModificationCommandBatchFactoryDependencies(
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        typeMapper,
                        new SqlServerExceptionDetector())),
                new SqlServerSqlGenerationHelper(
                    new RelationalSqlGenerationHelperDependencies()),
                new SqlServerUpdateSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        new SqlServerSqlGenerationHelper(
                            new RelationalSqlGenerationHelperDependencies()),
                        typeMapper)),
                new CurrentDbContext(new FakeDbContext()),
                new FakeRelationalCommandDiagnosticsLogger(),
                new FakeDiagnosticsLogger<DbLoggerCategory.Update>()),
            maxBatchSize);
    }

    private static SqlServerTypeMappingSource CreateTypeMappingSource()
        => new(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

    private static INonTrackedModificationCommand CreateModificationCommand(
        string name,
        string schema,
        bool sensitiveLoggingEnabled)
        => new ModificationCommandFactory().CreateNonTrackedModificationCommand(
            new NonTrackedModificationCommandParameters(name, schema, sensitiveLoggingEnabled));

    private class TestSqlServerModificationCommandBatch(ModificationCommandBatchFactoryDependencies dependencies, int maxBatchSize) : SqlServerModificationCommandBatch(dependencies, maxBatchSize)
    {
        public new Dictionary<string, object> ParameterValues
            => base.ParameterValues;

        public new RawSqlCommand StoreCommand
            => base.StoreCommand;

        public new IList<ResultSetMapping> ResultSetMappings
            => base.ResultSetMappings;
    }
}
