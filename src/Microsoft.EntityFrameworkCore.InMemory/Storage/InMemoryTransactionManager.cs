// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class InMemoryTransactionManager : IDbContextTransactionManager
    {
        private readonly bool _ignoreTransactions;

        public InMemoryTransactionManager([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, nameof(options));

            var optionsExtension = options.Extensions.OfType<InMemoryOptionsExtension>().FirstOrDefault();
            if (optionsExtension != null)
            {
                _ignoreTransactions = optionsExtension.IgnoreTransactions;
            }
        }

        public virtual IDbContextTransaction BeginTransaction()
        {
            if (!_ignoreTransactions)
            {
                throw new InvalidOperationException(InMemoryStrings.TransactionsNotSupported);
            }
            return new InMemoryTransaction();
        }

        public virtual Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!_ignoreTransactions)
            {
                throw new InvalidOperationException(InMemoryStrings.TransactionsNotSupported);
            }

            return Task.FromResult<IDbContextTransaction>(new InMemoryTransaction());
        }

        public virtual void CommitTransaction()
        {
            if (!_ignoreTransactions)
            {
                throw new InvalidOperationException(InMemoryStrings.TransactionsNotSupported);
            }
        }

        public virtual void RollbackTransaction()
        {
            if (!_ignoreTransactions)
            {
                throw new InvalidOperationException(InMemoryStrings.TransactionsNotSupported);
            }
        }
    }
}
