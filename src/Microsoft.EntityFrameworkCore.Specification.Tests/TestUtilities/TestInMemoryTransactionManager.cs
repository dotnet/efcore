// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public class TestInMemoryTransactionManager : InMemoryTransactionManager
    {
        private IDbContextTransaction _currentTransaction;

        public TestInMemoryTransactionManager(ILogger<InMemoryTransactionManager> logger)
            : base(logger)
        {
        }

        public override IDbContextTransaction CurrentTransaction => _currentTransaction;

        public override IDbContextTransaction BeginTransaction()
        {
            _currentTransaction = new TestInMemoryTransaction(this);
            return _currentTransaction;
        }

        public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _currentTransaction = new TestInMemoryTransaction(this);
            return Task.FromResult(_currentTransaction);
        }

        public override void CommitTransaction() => CurrentTransaction.Commit();

        public override void RollbackTransaction() => CurrentTransaction.Rollback();

        private class TestInMemoryTransaction : IDbContextTransaction
        {
            public TestInMemoryTransaction(TestInMemoryTransactionManager transactionManager)
            {
                TransactionManager = transactionManager;
            }

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
