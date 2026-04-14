// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Storage;

public class InMemoryTransactionManagerTest
{
    private class FakeTransactionManagerContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .ReplaceService<IDbContextTransactionManager, FakeTransactionManager>()
                .EnableServiceProviderCaching(false)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());
    }

    private class FakeTransactionManager : IDbContextTransactionManager
    {
        public void ResetState()
            => throw new NotImplementedException();

        public Task ResetStateAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IDbContextTransaction BeginTransaction()
            => throw new NotImplementedException();

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public void CommitTransaction()
            => throw new NotImplementedException();

        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public void RollbackTransaction()
            => throw new NotImplementedException();

        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IDbContextTransaction CurrentTransaction
            => throw new NotImplementedException();
    }

    [ConditionalFact]
    public void Enlist_operations_fails_if_provider_does_not_support_enlistment()
    {
        using var context = new FakeTransactionManagerContext();

        Assert.Equal(
            CoreStrings.TransactionsNotSupported,
            Assert.Throws<NotSupportedException>(() => context.Database.EnlistTransaction(Transaction.Current)).Message);

        Assert.Equal(
            CoreStrings.TransactionsNotSupported,
            Assert.Throws<NotSupportedException>(() => context.Database.GetEnlistedTransaction()).Message);
    }

    [ConditionalFact]
    public void CurrentTransaction_returns_null()
    {
        var transactionManager = new InMemoryTransactionManager(CreateLogger());

        Assert.Null(transactionManager.CurrentTransaction);
    }

    [ConditionalFact]
    public void Throws_on_BeginTransaction()
        => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).BeginTransaction());

    [ConditionalFact]
    public void Throws_on_BeginTransactionAsync()
        => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).BeginTransactionAsync().GetAwaiter().GetResult());

    [ConditionalFact]
    public void Throws_on_CommitTransaction()
        => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).CommitTransaction());

    [ConditionalFact]
    public void Throws_on_CommitTransactionAsync()
        => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).CommitTransactionAsync().GetAwaiter().GetResult());

    [ConditionalFact]
    public void Throws_on_RollbackTransaction()
        => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).RollbackTransaction());

    [ConditionalFact]
    public void Throws_on_RollbackTransactionAsync()
        => AssertThrows(() => new InMemoryTransactionManager(CreateLogger()).RollbackTransactionAsync().GetAwaiter().GetResult());

    private static void AssertThrows(Action action)
        => Assert.Equal(
            CoreStrings.WarningAsErrorTemplate(
                InMemoryEventId.TransactionIgnoredWarning,
                InMemoryResources.LogTransactionsNotSupported(new TestLogger<InMemoryLoggingDefinitions>()).GenerateMessage(),
                "InMemoryEventId.TransactionIgnoredWarning"),
            Assert.Throws<InvalidOperationException>(action).Message);

    private DiagnosticsLogger<DbLoggerCategory.Database.Transaction> CreateLogger()
    {
        var options = new LoggingOptions();
        options.Initialize(new DbContextOptionsBuilder().ConfigureWarnings(w => w.Default(WarningBehavior.Throw)).Options);
        var logger = new DiagnosticsLogger<DbLoggerCategory.Database.Transaction>(
            new ListLoggerFactory(l => false),
            options,
            new DiagnosticListener("Fake"),
            new InMemoryLoggingDefinitions(),
            new NullDbContextLogger());
        return logger;
    }
}
