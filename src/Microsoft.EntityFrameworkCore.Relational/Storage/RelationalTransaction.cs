// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class RelationalTransaction : IDbContextTransaction, IInfrastructure<DbTransaction>
    {
        private readonly IRelationalConnection _relationalConnection;
        private readonly DbTransaction _dbTransaction;
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

            _relationalConnection = connection;

            _dbTransaction = transaction;
            _logger = logger;
            _transactionOwned = transactionOwned;
        }

        public virtual void Commit()
        {
            _logger.LogDebug(
                RelationalEventId.CommittingTransaction,
                () => RelationalStrings.RelationalLoggerCommittingTransaction);

            _dbTransaction.Commit();

            ClearTransaction();
        }

        public virtual void Rollback()
        {
            _logger.LogDebug(
                RelationalEventId.RollingbackTransaction,
                () => RelationalStrings.RelationalLoggerRollingbackTransaction);

            _dbTransaction.Rollback();

            ClearTransaction();
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                if (_transactionOwned)
                {
                    _dbTransaction.Dispose();
                }

                ClearTransaction();
            }
        }

        private void ClearTransaction()
        {
            Debug.Assert((_relationalConnection.CurrentTransaction == null) || (_relationalConnection.CurrentTransaction == this));

            _relationalConnection.UseTransaction(null);
        }

        DbTransaction IInfrastructure<DbTransaction>.Instance => _dbTransaction;
    }
}
