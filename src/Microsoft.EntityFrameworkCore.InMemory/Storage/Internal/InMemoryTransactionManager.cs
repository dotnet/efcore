// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class InMemoryTransactionManager : IDbContextTransactionManager
    {
        private static readonly InMemoryTransaction _stubTransaction = new InMemoryTransaction();

        private readonly ILogger<InMemoryTransactionManager> _logger;

        public InMemoryTransactionManager([NotNull] ILogger<InMemoryTransactionManager> logger)
        {
            Check.NotNull(logger, nameof(logger));

            _logger = logger;
        }

        public virtual IDbContextTransaction BeginTransaction()
        {
            LogWarning();

            return _stubTransaction;
        }

        public virtual Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            LogWarning();

            return Task.FromResult<IDbContextTransaction>(_stubTransaction);
        }

        public virtual void CommitTransaction() => LogWarning();

        public virtual void RollbackTransaction() => LogWarning();

        public virtual IDbContextTransaction CurrentTransaction => null;

        protected virtual void LogWarning()
        {
            _logger.LogWarning(
                InMemoryEventId.TransactionIgnoredWarning,
                () => InMemoryStrings.TransactionsNotSupported);
        }
    }
}
