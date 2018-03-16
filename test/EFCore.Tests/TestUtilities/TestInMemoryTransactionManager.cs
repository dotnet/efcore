// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestInMemoryTransactionManager : InMemoryTransactionManager
    {
        private IDbContextTransaction _currentTransaction;
        private Transaction _enlistedTransaction;

        public TestInMemoryTransactionManager(
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger)
            : base(logger)
        {
        }

        public override IDbContextTransaction CurrentTransaction => _currentTransaction;

        public override Transaction EnlistedTransaction => _enlistedTransaction;

        public override IDbContextTransaction BeginTransaction() => _currentTransaction = new TestInMemoryTransaction(this);

        public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_currentTransaction = new TestInMemoryTransaction(this));

        public override void CommitTransaction() => CurrentTransaction.Commit();

        public override void RollbackTransaction() => CurrentTransaction.Rollback();

        public override void EnlistTransaction(Transaction transaction) => _enlistedTransaction = transaction;

        private class TestInMemoryTransaction : IDbContextTransaction
        {
            public TestInMemoryTransaction(TestInMemoryTransactionManager transactionManager)
            {
                TransactionManager = transactionManager;
            }

            public Guid TransactionId { get; } = Guid.NewGuid();

            private TestInMemoryTransactionManager TransactionManager { get; }

            public void Dispose()
            {
                TransactionManager._currentTransaction = null;
            }

            public void Commit()
            {
                TransactionManager._currentTransaction = null;
            }

            public void Rollback()
            {
                TransactionManager._currentTransaction = null;
            }
        }
    }
}
