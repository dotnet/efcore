// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Storage
{
    public class RelationalTransaction : IRelationalTransaction
    {
        private readonly bool _transactionOwned;
        private bool _disposed;

        public RelationalTransaction(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction dbTransaction,
            bool transactionOwned,
            [NotNull] ILogger logger)
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(dbTransaction, nameof(dbTransaction));
            Check.NotNull(logger, nameof(logger));

            if (connection.DbConnection != dbTransaction.Connection)
            {
                throw new InvalidOperationException(Strings.TransactionAssociatedWithDifferentConnection);
            }

            Connection = connection;
            DbTransaction = dbTransaction;
            _transactionOwned = transactionOwned;
            Logger = logger;
        }

        protected virtual ILogger Logger { get; }

        public virtual DbTransaction DbTransaction { get; }

        public virtual IRelationalConnection Connection { get; }

        public virtual void Commit()
        {
            Logger.CommittingTransaction();

            DbTransaction.Commit();
            ClearTransaction();
        }

        public virtual void Rollback()
        {
            Logger.RollingbackTransaction();

            DbTransaction.Rollback();
            ClearTransaction();
        }

        public virtual void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                if (_transactionOwned)
                {
                    DbTransaction.Dispose();
                }
                ClearTransaction();
            }
        }

        private void ClearTransaction()
        {
            Debug.Assert(Connection.Transaction == null || Connection.Transaction == this);

            Connection.UseTransaction(null);
        }
    }
}
