// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class DocumentDbTransactionManager : IDbContextTransactionManager
    {
        private static readonly DocumentDbTransaction _stubTransaction = new DocumentDbTransaction();
        public IDbContextTransaction CurrentTransaction => null;

        public Transaction EnlistedTransaction => null;

        public IDbContextTransaction BeginTransaction()
        {
            return _stubTransaction;
        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IDbContextTransaction>(_stubTransaction);
        }

        public void CommitTransaction()
        {
            throw new NotImplementedException();
        }

        public void EnlistTransaction(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public void ResetState()
        {
            throw new NotImplementedException();
        }

        public void RollbackTransaction()
        {
            throw new NotImplementedException();
        }
    }



}
