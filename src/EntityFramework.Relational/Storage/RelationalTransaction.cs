// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalTransaction : IRelationalTransaction
    {
        private readonly DbTransaction _transaction;
        private readonly ILogger _logger;
        private readonly bool _transactionOwned;

        private bool _disposed;

        public RelationalTransaction(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            [NotNull] ILogger logger,
            bool transactionOwned)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(transaction, nameof(transaction));
            Check.NotNull(logger, nameof(logger));

            if (connection.DbConnection != transaction.Connection)
            {
                throw new InvalidOperationException(RelationalStrings.TransactionAssociatedWithDifferentConnection);
            }

            Connection = connection;

            _transaction = transaction;
            _logger = logger;
            _transactionOwned = transactionOwned;
        }

        public virtual IRelationalConnection Connection { get; }

        public virtual void Commit()
        {
            _logger.LogVerbose(
                RelationalLoggingEventId.CommittingTransaction,
                () => RelationalStrings.RelationalLoggerCommittingTransaction);

            _transaction.Commit();

            ClearTransaction();
        }

        public virtual void Rollback()
        {
            _logger.LogVerbose(
                RelationalLoggingEventId.RollingbackTransaction,
                () => RelationalStrings.RelationalLoggerRollingbackTransaction);

            _transaction.Rollback();

            ClearTransaction();
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_transactionOwned)
                {
                    _transaction.Dispose();
                }

                ClearTransaction();
            }
        }

        private void ClearTransaction()
        {
            Debug.Assert(Connection.Transaction == null || Connection.Transaction == this);

            Connection.UseTransaction(null);
        }

        DbTransaction IAccessor<DbTransaction>.Service => _transaction;
    }
}
