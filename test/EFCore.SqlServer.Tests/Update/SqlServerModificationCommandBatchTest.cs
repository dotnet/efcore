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
    [ConditionalFact]
    public void AddCommand_returns_false_when_max_batch_size_is_reached()
    {
        var batch = CreateBatch(maxBatchSize: 1);

        var firstCommand = CreateModificationCommand("T1", null, false);
        Assert.True(batch.TryAddCommand(firstCommand));
        Assert.False(batch.TryAddCommand(CreateModificationCommand("T1", null, false)));

        Assert.Same(firstCommand, Assert.Single(batch.ModificationCommands));
    }

    [ConditionalFact]
    public void AddCommand_returns_false_when_max_parameters_are_reached()
    {
        var typeMapper = CreateTypeMappingSource();
        var intMapping = typeMapper.FindMapping(typeof(int));
        var paramIndex = 0;

        var batch = CreateBatch();

        var command = CreateModificationCommand("T1", null, false);
        for (var i = 0; i < 2098; i++)
        {
            command.AddColumnModification(CreateModificationParameters("col" + i));
        }
        Assert.True(batch.TryAddCommand(command));

        var secondCommand = CreateModificationCommand("T2", null, false);
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

    private class FakeDbContext : DbContext
    {
    }

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
                new TypedRelationalValueBufferFactoryFactory(
                    new RelationalValueBufferFactoryDependencies(
                        typeMapper, new CoreSingletonOptions())),
                new CurrentDbContext(new FakeDbContext()),
                new FakeRelationalCommandDiagnosticsLogger()),
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

    private class TestSqlServerModificationCommandBatch : SqlServerModificationCommandBatch
    {
        public TestSqlServerModificationCommandBatch(ModificationCommandBatchFactoryDependencies dependencies, int maxBatchSize)
            : base(dependencies, maxBatchSize)
        {
        }

        public new Dictionary<string, object> ParameterValues
            => base.ParameterValues;
    }
}
