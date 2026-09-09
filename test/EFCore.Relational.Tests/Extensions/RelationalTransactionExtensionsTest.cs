// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class RelationalTransactionExtensionsTest
{
    [ConditionalFact]
    public void GetDbTransaction_returns_the_DbTransaction()
    {
        var dbConnection = new FakeDbConnection(ConnectionString);
        var dbTransaction = new FakeDbTransaction(dbConnection);

        var connection = new FakeRelationalConnection(
            CreateOptions((FakeRelationalOptionsExtension)new FakeRelationalOptionsExtension().WithConnection(dbConnection)));

        var loggerFactory = new ListLoggerFactory();

        var transaction = new RelationalTransaction(
            connection,
            dbTransaction,
            new Guid(),
            new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
                loggerFactory,
                new LoggingOptions(),
                new DiagnosticListener("Fake"),
                new TestRelationalLoggingDefinitions(),
                new NullDbContextLogger()),
            false,
            new RelationalSqlGenerationHelper(
                new RelationalSqlGenerationHelperDependencies()));

        Assert.Equal(dbTransaction, transaction.GetDbTransaction());
    }

    [ConditionalFact]
    public void GetDbTransaction_throws_on_non_relational_provider()
    {
        var transaction = new NonRelationalTransaction();

        Assert.Equal(
            RelationalStrings.RelationalNotInUse,
            Assert.Throws<InvalidOperationException>(
                () => transaction.GetDbTransaction()).Message);
    }

    private class NonRelationalTransaction : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();

        public void Commit()
            => throw new NotImplementedException();

        public void Dispose()
            => throw new NotImplementedException();

        public void Rollback()
            => throw new NotImplementedException();

        public Task CommitAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task RollbackAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public ValueTask DisposeAsync()
            => throw new NotImplementedException();
    }

    private const string ConnectionString = "Fake Connection String";

    public static IDbContextOptions CreateOptions(
        FakeRelationalOptionsExtension optionsExtension = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder();

        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(optionsExtension ?? new FakeRelationalOptionsExtension().WithConnectionString(ConnectionString));

        return optionsBuilder.Options;
    }
}
