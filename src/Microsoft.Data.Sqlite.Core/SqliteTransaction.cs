// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite.Properties;

using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents a transaction made against a SQLite database.
    /// </summary>
    public class SqliteTransaction : DbTransaction
    {
        private SqliteConnection _connection;
        private readonly IsolationLevel _isolationLevel;
        private bool _completed;
        private bool _externalRollback;

        internal SqliteTransaction(SqliteConnection connection, IsolationLevel isolationLevel)
        {
            if ((isolationLevel == IsolationLevel.ReadUncommitted
                 && connection.ConnectionOptions.Cache != SqliteCacheMode.Shared)
                || isolationLevel == IsolationLevel.ReadCommitted
                || isolationLevel == IsolationLevel.RepeatableRead)
            {
                isolationLevel = IsolationLevel.Serializable;
            }

            _connection = connection;
            _isolationLevel = isolationLevel;

            if (isolationLevel == IsolationLevel.ReadUncommitted)
            {
                connection.ExecuteNonQuery("PRAGMA read_uncommitted = 1;");
            }
            else if (isolationLevel == IsolationLevel.Serializable)
            {
                connection.ExecuteNonQuery("PRAGMA read_uncommitted = 0;");
            }
            else if (isolationLevel != IsolationLevel.Unspecified)
            {
                throw new ArgumentException(Resources.InvalidIsolationLevel(isolationLevel));
            }

            connection.ExecuteNonQuery(
                IsolationLevel == IsolationLevel.Serializable
                    ? "BEGIN IMMEDIATE;"
                    : "BEGIN;");
            sqlite3_rollback_hook(connection.Handle, RollbackExternal, null);
        }

        /// <summary>
        ///     Gets the connection associated with the transaction.
        /// </summary>
        /// <value>The connection associated with the transaction.</value>
        public new virtual SqliteConnection Connection
            => _connection;

        /// <summary>
        ///     Gets the connection associated with the transaction.
        /// </summary>
        /// <value>The connection associated with the transaction.</value>
        protected override DbConnection DbConnection
            => Connection;

        internal bool ExternalRollback
            => _externalRollback;

        /// <summary>
        ///     Gets the isolation level for the transaction. This cannot be changed if the transaction is completed or
        ///     closed.
        /// </summary>
        /// <value>The isolation level for the transaction.</value>
        public override IsolationLevel IsolationLevel
            => _completed || _connection.State != ConnectionState.Open
                ? throw new InvalidOperationException(Resources.TransactionCompleted)
                : _isolationLevel != IsolationLevel.Unspecified
                    ? _isolationLevel
                    : (_connection.ConnectionOptions.Cache == SqliteCacheMode.Shared
                       && _connection.ExecuteScalar<long>("PRAGMA read_uncommitted;") != 0)
                        ? IsolationLevel.ReadUncommitted
                        : IsolationLevel.Serializable;

        /// <summary>
        ///     Applies the changes made in the transaction.
        /// </summary>
        public override void Commit()
        {
            if (ExternalRollback
                || _completed
                || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.TransactionCompleted);
            }

            sqlite3_rollback_hook(_connection.Handle, null, null);
            _connection.ExecuteNonQuery("COMMIT;");
            Complete();
        }

        /// <summary>
        ///     Reverts the changes made in the transaction.
        /// </summary>
        public override void Rollback()
        {
            if (_completed || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Resources.TransactionCompleted);
            }

            RollbackInternal();
        }

        /// <summary>
        ///     Releases any resources used by the transaction and rolls it back.
        /// </summary>
        /// <param name="disposing">
        ///     true to release managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing
                && !_completed
                && _connection.State == ConnectionState.Open)
            {
                RollbackInternal();
            }
        }

        private void Complete()
        {
            _connection.Transaction = null;
            _connection = null;
            _completed = true;
        }

        private void RollbackInternal()
        {
            if (!ExternalRollback)
            {
                sqlite3_rollback_hook(_connection.Handle, null, null);
                _connection.ExecuteNonQuery("ROLLBACK;");
            }

            Complete();
        }

        private void RollbackExternal(object userData)
        {
            sqlite3_rollback_hook(_connection.Handle, null, null);
            _externalRollback = true;
        }
    }
}
