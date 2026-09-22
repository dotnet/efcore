// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.Migrations;

public class MigrationCommandListBuilderTest
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void MigrationCommandListBuilder_groups_multiple_statements_into_one_batch(bool suppressTransaction)
    {
        var commandListBuilder = CreateBuilder();
        commandListBuilder.AppendLine("Statement1");
        commandListBuilder.AppendLine("Statement2");
        commandListBuilder.AppendLine("Statement3");
        commandListBuilder.EndCommand(suppressTransaction);

        var commandList = commandListBuilder.GetCommandList();

        Assert.Equal(1, commandList.Count);

        Assert.Equal(suppressTransaction, commandList[0].TransactionSuppressed);
        Assert.NotNull(commandList[0].CommandLogger);
        Assert.Equal(
            @"Statement1
Statement2
Statement3
",
            commandList[0].CommandText,
            ignoreLineEndingDifferences: true);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void MigrationCommandListBuilder_correctly_produces_multiple_batches(bool suppressTransaction)
    {
        var commandListBuilder = CreateBuilder();
        commandListBuilder.AppendLine("Statement1");
        commandListBuilder.EndCommand(suppressTransaction);
        commandListBuilder.AppendLine("Statement2");
        commandListBuilder.AppendLine("Statement3");
        commandListBuilder.EndCommand(suppressTransaction);
        commandListBuilder.AppendLine("Statement4");
        commandListBuilder.AppendLine("Statement5");
        commandListBuilder.AppendLine("Statement6");
        commandListBuilder.EndCommand(suppressTransaction);

        var commandList = commandListBuilder.GetCommandList();

        Assert.Equal(3, commandList.Count);

        Assert.Equal(suppressTransaction, commandList[0].TransactionSuppressed);
        Assert.NotNull(commandList[0].CommandLogger);
        Assert.Equal(
            @"Statement1
",
            commandList[0].CommandText,
            ignoreLineEndingDifferences: true);

        Assert.Equal(suppressTransaction, commandList[1].TransactionSuppressed);
        Assert.NotNull(commandList[1].CommandLogger);
        Assert.Equal(
            @"Statement2
Statement3
",
            commandList[1].CommandText,
            ignoreLineEndingDifferences: true);

        Assert.Equal(suppressTransaction, commandList[2].TransactionSuppressed);
        Assert.NotNull(commandList[2].CommandLogger);
        Assert.Equal(
            @"Statement4
Statement5
Statement6
",
            commandList[2].CommandText,
            ignoreLineEndingDifferences: true);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void MigrationCommandListBuilder_ignores_empty_batches(bool suppressTransaction)
    {
        var commandListBuilder = CreateBuilder();
        commandListBuilder.AppendLine("Statement1");
        commandListBuilder.EndCommand(suppressTransaction);
        commandListBuilder.EndCommand(suppressTransaction: true);
        commandListBuilder.EndCommand(suppressTransaction: true);
        commandListBuilder.AppendLine("Statement2");
        commandListBuilder.AppendLine("Statement3");
        commandListBuilder.EndCommand(suppressTransaction);
        commandListBuilder.EndCommand();

        var commandList = commandListBuilder.GetCommandList();

        Assert.Equal(2, commandList.Count);

        Assert.Equal(suppressTransaction, commandList[0].TransactionSuppressed);
        Assert.NotNull(commandList[0].CommandLogger);
        Assert.Equal(
            @"Statement1
",
            commandList[0].CommandText,
            ignoreLineEndingDifferences: true);

        Assert.Equal(suppressTransaction, commandList[1].TransactionSuppressed);
        Assert.NotNull(commandList[1].CommandLogger);
        Assert.Equal(
            @"Statement2
Statement3
",
            commandList[1].CommandText,
            ignoreLineEndingDifferences: true);
    }

    private MigrationCommandListBuilder CreateBuilder()
    {
        var typeMappingSource = new TestRelationalTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var logger = new FakeRelationalCommandDiagnosticsLogger();
        var migrationsLogger = new FakeDiagnosticsLogger<DbLoggerCategory.Migrations>();
        var generationHelper = new RelationalSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies());

        return new MigrationCommandListBuilder(
            new MigrationsSqlGeneratorDependencies(
                new RelationalCommandBuilderFactory(
                    new RelationalCommandBuilderDependencies(
                        typeMappingSource,
                        new ExceptionDetector())),
                new FakeSqlGenerator(
                    new UpdateSqlGeneratorDependencies(
                        generationHelper,
                        typeMappingSource)),
                generationHelper,
                typeMappingSource,
                new CurrentDbContext(new FakeDbContext()),
                new ModificationCommandFactory(),
                new LoggingOptions(),
                logger,
                migrationsLogger));
    }

    private class FakeDbContext : DbContext;
}
