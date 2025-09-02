// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestInMemoryTransactionManager(
    IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger) : InMemoryTransactionManager(logger)
{
    private IDbContextTransaction _currentTransaction;
    private Transaction _enlistedTransaction;

    public override IDbContextTransaction CurrentTransaction
        => _currentTransaction;

    public override Transaction EnlistedTransaction
        => _enlistedTransaction;

    public override IDbContextTransaction BeginTransaction()
        => _currentTransaction = new TestInMemoryTransaction(this);

    public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_currentTransaction = new TestInMemoryTransaction(this));

    public override void CommitTransaction()
        => CurrentTransaction.Commit();

    public override Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => CurrentTransaction.CommitAsync(cancellationToken);

    public override void RollbackTransaction()
        => CurrentTransaction.Rollback();

    public override Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        => CurrentTransaction.RollbackAsync(cancellationToken);

    public override void EnlistTransaction(Transaction transaction)
        => _enlistedTransaction = transaction;

    private class TestInMemoryTransaction(TestInMemoryTransactionManager transactionManager) : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();

        private TestInMemoryTransactionManager TransactionManager { get; } = transactionManager;

        public void Dispose()
            => TransactionManager._currentTransaction = null;

        public void Commit()
            => TransactionManager._currentTransaction = null;

        public void Rollback()
            => TransactionManager._currentTransaction = null;

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            TransactionManager._currentTransaction = null;

            return Task.CompletedTask;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            TransactionManager._currentTransaction = null;

            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            Dispose();

            return default;
        }
    }
}
