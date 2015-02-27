// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalTransaction : DataStoreTransaction
    {
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

        public RelationalTransaction(
            [NotNull] IRelationalConnection connection, 
            [NotNull] DbTransaction dbTransaction, 
            bool transactionOwned, 
            [NotNull] ILogger logger)
            : base(logger)
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
        }

        public virtual DbTransaction DbTransaction { get; }

        public virtual IRelationalConnection Connection { get; }

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
