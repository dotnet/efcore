// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class InMemoryTransactionManager : IDbContextTransactionManager
    {
        private static readonly InMemoryTransaction _stubTransaction = new InMemoryTransaction();

        private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> _logger;

        public InMemoryTransactionManager(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        public virtual IDbContextTransaction BeginTransaction()
        {
            _logger.TransactionIgnoredWarning();

            return _stubTransaction;
        }

        public virtual Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.TransactionIgnoredWarning();

            return Task.FromResult<IDbContextTransaction>(_stubTransaction);
        }

        public virtual void CommitTransaction() => _logger.TransactionIgnoredWarning();

        public virtual void RollbackTransaction() => _logger.TransactionIgnoredWarning();

        public virtual IDbContextTransaction CurrentTransaction => null;

        public virtual void Reset()
        {
        }
    }
}
