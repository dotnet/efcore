// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

namespace Microsoft.EntityFrameworkCore.Migrations;

public class MigrationCommandExecutorTest
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Executes_migration_commands_in_same_transaction(bool async)
    {
        var fakeConnection = CreateConnection();
        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var commandList = new List<MigrationCommand>
        {
            new(CreateRelationalCommand(), null, logger), new(CreateRelationalCommand(), null, logger)
        };

        var migrationCommandExecutor = new MigrationCommandExecutor();

        if (async)
        {
            await migrationCommandExecutor.ExecuteNonQueryAsync(commandList, fakeConnection);
        }
        else
        {
            migrationCommandExecutor.ExecuteNonQuery(commandList, fakeConnection);
        }

        Assert.Equal(1, fakeConnection.DbConnections.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].OpenCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].CloseCount);

        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions[0].CommitCount);
        Assert.Equal(0, fakeConnection.DbConnections[0].DbTransactions[0].RollbackCount);

        Assert.Equal(2, fakeConnection.DbConnections[0].DbCommands.Count);
        Assert.Same(
            fakeConnection.DbConnections[0].DbTransactions[0],
            fakeConnection.DbConnections[0].DbCommands[0].Transaction);
        Assert.Same(
            fakeConnection.DbConnections[0].DbTransactions[0],
            fakeConnection.DbConnections[0].DbCommands[1].Transaction);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Executes_migration_commands_in_user_transaction(bool async)
    {
        var fakeConnection = CreateConnection();
        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var commandList = new List<MigrationCommand>
        {
            new(CreateRelationalCommand(), null, logger), new(CreateRelationalCommand(), null, logger)
        };

        var migrationCommandExecutor = new MigrationCommandExecutor();

        IDbContextTransaction tx;
        using (tx = fakeConnection.BeginTransaction())
        {
            if (async)
            {
                await migrationCommandExecutor.ExecuteNonQueryAsync(commandList, fakeConnection);
            }
            else
            {
                migrationCommandExecutor.ExecuteNonQuery(commandList, fakeConnection);
            }

            tx.Commit();
        }

        Assert.Equal(1, fakeConnection.DbConnections.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].OpenCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].CloseCount);

        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions[0].CommitCount);
        Assert.Equal(0, fakeConnection.DbConnections[0].DbTransactions[0].RollbackCount);

        Assert.Equal(2, fakeConnection.DbConnections[0].DbCommands.Count);
        Assert.Same(
            fakeConnection.DbConnections[0].DbTransactions[0],
            fakeConnection.DbConnections[0].DbCommands[0].Transaction);
        Assert.Same(
            fakeConnection.DbConnections[0].DbTransactions[0],
            fakeConnection.DbConnections[0].DbCommands[1].Transaction);
        Assert.Same(
            tx.GetDbTransaction(),
            fakeConnection.DbConnections[0].DbTransactions[0]);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Executes_transaction_suppressed_migration_commands_in_user_transaction(bool async)
    {
        var fakeConnection = CreateConnection();
        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var commandList = new List<MigrationCommand>
        {
            new(CreateRelationalCommand(), null, logger), new(CreateRelationalCommand(), null, logger, transactionSuppressed: true)
        };

        var migrationCommandExecutor = new MigrationCommandExecutor();

        IDbContextTransaction tx;
        using (tx = fakeConnection.BeginTransaction())
        {
            if (async)
            {
                Assert.Equal(
                    RelationalStrings.TransactionSuppressedMigrationInUserTransaction,
                    (await Assert.ThrowsAsync<NotSupportedException>(
                        async ()
                            => await migrationCommandExecutor.ExecuteNonQueryAsync(commandList, fakeConnection))).Message);
            }
            else
            {
                Assert.Equal(
                    RelationalStrings.TransactionSuppressedMigrationInUserTransaction,
                    Assert.Throws<NotSupportedException>(
                        ()
                            => migrationCommandExecutor.ExecuteNonQuery(commandList, fakeConnection)).Message);
            }

            tx.Rollback();
        }

        Assert.Equal(1, fakeConnection.DbConnections.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].OpenCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].CloseCount);

        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions.Count);
        Assert.Equal(0, fakeConnection.DbConnections[0].DbTransactions[0].CommitCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions[0].RollbackCount);

        Assert.Equal(0, fakeConnection.DbConnections[0].DbCommands.Count);
        Assert.Same(
            tx.GetDbTransaction(),
            fakeConnection.DbConnections[0].DbTransactions[0]);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Executes_migration_commands_with_transaction_suppressed_outside_of_transaction(bool async)
    {
        var fakeConnection = CreateConnection();
        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var commandList = new List<MigrationCommand>
        {
            new(CreateRelationalCommand(), null, logger, transactionSuppressed: true),
            new(CreateRelationalCommand(), null, logger, transactionSuppressed: true)
        };

        var migrationCommandExecutor = new MigrationCommandExecutor();

        if (async)
        {
            await migrationCommandExecutor.ExecuteNonQueryAsync(commandList, fakeConnection);
        }
        else
        {
            migrationCommandExecutor.ExecuteNonQuery(commandList, fakeConnection);
        }

        Assert.Equal(1, fakeConnection.DbConnections.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].OpenCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].CloseCount);

        Assert.Equal(0, fakeConnection.DbConnections[0].DbTransactions.Count);

        Assert.Equal(2, fakeConnection.DbConnections[0].DbCommands.Count);
        Assert.Null(fakeConnection.DbConnections[0].DbCommands[0].Transaction);
        Assert.Null(fakeConnection.DbConnections[0].DbCommands[1].Transaction);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Ends_transaction_when_transaction_is_suppressed(bool async)
    {
        var fakeConnection = CreateConnection();
        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var commandList = new List<MigrationCommand>
        {
            new(CreateRelationalCommand(), null, logger), new(CreateRelationalCommand(), null, logger, transactionSuppressed: true)
        };

        var migrationCommandExecutor = new MigrationCommandExecutor();

        if (async)
        {
            await migrationCommandExecutor.ExecuteNonQueryAsync(commandList, fakeConnection);
        }
        else
        {
            migrationCommandExecutor.ExecuteNonQuery(commandList, fakeConnection);
        }

        Assert.Equal(1, fakeConnection.DbConnections.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].OpenCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].CloseCount);

        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions[0].CommitCount);
        Assert.Equal(0, fakeConnection.DbConnections[0].DbTransactions[0].RollbackCount);

        Assert.Equal(2, fakeConnection.DbConnections[0].DbCommands.Count);
        Assert.Same(
            fakeConnection.DbConnections[0].DbTransactions[0],
            fakeConnection.DbConnections[0].DbCommands[0].Transaction);
        Assert.Null(
            fakeConnection.DbConnections[0].DbCommands[1].Transaction);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Begins_new_transaction_when_transaction_nolonger_suppressed(bool async)
    {
        var fakeConnection = CreateConnection();
        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var commandList = new List<MigrationCommand>
        {
            new(CreateRelationalCommand(), null, logger, transactionSuppressed: true), new(CreateRelationalCommand(), null, logger)
        };

        var migrationCommandExecutor = new MigrationCommandExecutor();

        if (async)
        {
            await migrationCommandExecutor.ExecuteNonQueryAsync(commandList, fakeConnection);
        }
        else
        {
            migrationCommandExecutor.ExecuteNonQuery(commandList, fakeConnection);
        }

        Assert.Equal(1, fakeConnection.DbConnections.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].OpenCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].CloseCount);

        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions[0].CommitCount);
        Assert.Equal(0, fakeConnection.DbConnections[0].DbTransactions[0].RollbackCount);

        Assert.Equal(2, fakeConnection.DbConnections[0].DbCommands.Count);
        Assert.Null(
            fakeConnection.DbConnections[0].DbCommands[0].Transaction);
        Assert.Same(
            fakeConnection.DbConnections[0].DbTransactions[0],
            fakeConnection.DbConnections[0].DbCommands[1].Transaction);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Executes_commands_in_order_regardless_of_transaction_suppression(bool async)
    {
        var fakeConnection = CreateConnection();
        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var commandList = new List<MigrationCommand>
        {
            new(CreateRelationalCommand(commandText: "First"), null, logger),
            new(CreateRelationalCommand(commandText: "Second"), null, logger, transactionSuppressed: true),
            new(CreateRelationalCommand(commandText: "Third"), null, logger)
        };

        var migrationCommandExecutor = new MigrationCommandExecutor();

        if (async)
        {
            await migrationCommandExecutor.ExecuteNonQueryAsync(commandList, fakeConnection);
        }
        else
        {
            migrationCommandExecutor.ExecuteNonQuery(commandList, fakeConnection);
        }

        Assert.Equal(1, fakeConnection.DbConnections.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].OpenCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].CloseCount);

        Assert.Equal(2, fakeConnection.DbConnections[0].DbTransactions.Count);
        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions[0].CommitCount);
        Assert.Equal(0, fakeConnection.DbConnections[0].DbTransactions[0].RollbackCount);
        Assert.Equal(1, fakeConnection.DbConnections[0].DbTransactions[1].CommitCount);
        Assert.Equal(0, fakeConnection.DbConnections[0].DbTransactions[1].RollbackCount);

        Assert.Equal(3, fakeConnection.DbConnections[0].DbCommands.Count);

        var command = fakeConnection.DbConnections[0].DbCommands[0];

        Assert.Same(
            fakeConnection.DbConnections[0].DbTransactions[0],
            command.Transaction);
        Assert.Equal(
            "First",
            command.CommandText);

        command = fakeConnection.DbConnections[0].DbCommands[1];

        Assert.Null(command.Transaction);
        Assert.Equal(
            "Second",
            command.CommandText);

        command = fakeConnection.DbConnections[0].DbCommands[2];

        Assert.Same(
            fakeConnection.DbConnections[0].DbTransactions[1],
            command.Transaction);
        Assert.Equal(
            "Third",
            command.CommandText);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Disposes_transaction_on_exception(bool async)
    {
        var fakeDbConnection =
            new FakeDbConnection(
                ConnectionString,
                new FakeCommandExecutor(
                    executeNonQuery: c => throw new InvalidOperationException(),
                    executeNonQueryAsync: (c, ct) => throw new InvalidOperationException()));

        var fakeConnection =
            CreateConnection(
                CreateOptions(
                    new FakeRelationalOptionsExtension().WithConnection(fakeDbConnection)));

        var logger = new FakeRelationalCommandDiagnosticsLogger();

        var commandList = new List<MigrationCommand> { new(CreateRelationalCommand(), null, logger) };

        var migrationCommandExecutor = new MigrationCommandExecutor();

        if (async)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                async ()
                    => await migrationCommandExecutor.ExecuteNonQueryAsync(commandList, fakeConnection));
        }
        else
        {
            Assert.Throws<InvalidOperationException>(
                ()
                    => migrationCommandExecutor.ExecuteNonQuery(commandList, fakeConnection));
        }

        Assert.Equal(1, fakeDbConnection.OpenCount);
        Assert.Equal(1, fakeDbConnection.CloseCount);

        Assert.Equal(1, fakeDbConnection.DbTransactions.Count);
        Assert.Equal(1, fakeDbConnection.DbTransactions[0].DisposeCount);
        Assert.Equal(0, fakeDbConnection.DbTransactions[0].CommitCount);
        Assert.Equal(0, fakeDbConnection.DbTransactions[0].RollbackCount);
    }

    private const string ConnectionString = "Fake Connection String";

    private static FakeRelationalConnection CreateConnection(IDbContextOptions options = null)
        => new(options ?? CreateOptions());

    private static IDbContextOptions CreateOptions(RelationalOptionsExtension optionsExtension = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                optionsExtension
                ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

        return optionsBuilder.Options;
    }

    private IRelationalCommand CreateRelationalCommand(
        string commandText = "Command Text",
        IReadOnlyList<IRelationalParameter> parameters = null)
        => new RelationalCommand(
            new RelationalCommandBuilderDependencies(
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>()),
                new ExceptionDetector()),
            commandText,
            parameters ?? []);
}
