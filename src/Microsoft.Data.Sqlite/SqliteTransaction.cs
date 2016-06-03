// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite.Utilities;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Represents a transaction made against a SQLite database.
    /// </summary>
    public class SqliteTransaction : DbTransaction
    {
        private SqliteConnection _connection;
        private readonly IsolationLevel _isolationLevel;
        private bool _completed;

        internal SqliteTransaction(SqliteConnection connection, IsolationLevel isolationLevel, int commandTimeout)
        {
            _connection = connection;
            _isolationLevel = isolationLevel;

            if (isolationLevel == IsolationLevel.ReadUncommitted)
            {
                if (connection.ConnectionStringBuilder.Cache != SqliteCacheMode.Shared)
                {
                    throw new ArgumentException(Strings.InvalidIsolationLevelForUnsharedCache(isolationLevel));
                }
                connection.ExecuteNonQuery("PRAGMA read_uncommitted = 1;");
            }
            else if (isolationLevel == IsolationLevel.Serializable)
            {
                connection.ExecuteNonQuery("PRAGMA read_uncommitted = 0;");
            }
            else if (isolationLevel != IsolationLevel.Unspecified)
            {
                throw new ArgumentException(Strings.InvalidIsolationLevel(isolationLevel));
            }

            // TODO: Register transaction hooks to detect when a user manually completes a transaction created using
            //       this API
            var beginCommand = (isolationLevel == IsolationLevel.Serializable)
                ? "BEGIN IMMEDIATE"
                : "BEGIN";
            connection.ExecuteNonQuery(beginCommand, commandTimeout);
        }

        /// <summary>
        /// Gets the connection associated with the transaction.
        /// </summary>
        public new virtual SqliteConnection Connection
            => _connection;

        /// <summary>
        /// Gets the connection associated with the transaction.
        /// </summary>
        protected override DbConnection DbConnection
            => Connection;

        /// <summary>
        /// Gets the isolation level for the transaction. This cannot be changed if the transaction is completed or
        /// closed.
        /// </summary>
        public override IsolationLevel IsolationLevel
        {
            get
            {
                if (_completed || _connection.State != ConnectionState.Open)
                {
                    throw new InvalidOperationException(Strings.TransactionCompleted);
                }

                return _isolationLevel != IsolationLevel.Unspecified
                    ? _isolationLevel
                    : _connection.ExecuteScalar<long>("PRAGMA read_uncommitted;") != 0
                        ? IsolationLevel.ReadUncommitted
                        : IsolationLevel.Serializable;
            }
        }

        /// <summary>
        /// Applies the changes made in the transaction.
        /// </summary>
        public override void Commit()
        {
            if (_completed || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.TransactionCompleted);
            }

            _connection.ExecuteNonQuery("COMMIT;");
            Complete();
        }

        /// <summary>
        /// Reverts the changes made in the transaction.
        /// </summary>
        public override void Rollback()
        {
            if (_completed || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.TransactionCompleted);
            }

            RollbackInternal();
        }

        /// <summary>
        /// Releases any resources used by the transaction and rolls it back.
        /// </summary>
        /// <param name="disposing">
        /// true to release managed and unmanaged resources; false to release only unmanaged resources.
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
            _connection.ExecuteNonQuery("ROLLBACK;");
            Complete();
        }
    }
}
