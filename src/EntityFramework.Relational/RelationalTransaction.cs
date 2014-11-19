// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalTransaction : DataStoreTransaction
    {
        private readonly DbTransaction _dbTransaction;
        private readonly RelationalConnection _connection;
        private readonly bool _transactionOwned;
        private bool _disposed;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RelationalTransaction()
        {
        }

        public RelationalTransaction([NotNull] RelationalConnection connection, [NotNull] DbTransaction dbTransaction, bool transactionOwned, [NotNull] ILogger logger)
            : base(logger)
        {
            Check.NotNull(connection, "connection");
            Check.NotNull(dbTransaction, "dbTransaction");
            Check.NotNull(logger, "logger");

            if (connection.DbConnection != dbTransaction.Connection)
            {
                throw new InvalidOperationException(Strings.TransactionAssociatedWithDifferentConnection);
            }

            _connection = connection;
            _dbTransaction = dbTransaction;
            _transactionOwned = transactionOwned;
        }

        public virtual DbTransaction DbTransaction
        {
            get { return _dbTransaction; }
        }

        public virtual RelationalConnection Connection
        {
            get { return _connection; }
        }

        public override void Commit()
        {
            Logger.CommittingTransaction();

            DbTransaction.Commit();
            ClearTransaction();
        }

        public override void Rollback()
        {
            Logger.RollingbackTransaction();

            DbTransaction.Rollback();
            ClearTransaction();
        }

        public override void Dispose()
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
