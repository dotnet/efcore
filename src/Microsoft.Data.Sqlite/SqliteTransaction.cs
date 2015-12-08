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

        internal SqliteTransaction(SqliteConnection connection, IsolationLevel isolationLevel)
        {
            _connection = connection;
            _isolationLevel = isolationLevel;

            if (isolationLevel == IsolationLevel.ReadUncommitted)
            {
                if (connection.ConnectionStringBuilder.Cache != SqliteCacheMode.Shared)
                {
                    throw new ArgumentException(Strings.FormatInvalidIsolationLevelForUnsharedCache(isolationLevel));
                }
                connection.ExecuteNonQuery("PRAGMA read_uncommitted = 1;");
            }
            else if (isolationLevel == IsolationLevel.Serializable)
            {
                connection.ExecuteNonQuery("PRAGMA read_uncommitted = 0;");
            }
            else if (isolationLevel != IsolationLevel.Unspecified)
            {
                throw new ArgumentException(Strings.FormatInvalidIsolationLevel(isolationLevel));
            }

            // TODO: Register transaction hooks to detect when a user manually completes a transaction created using this API
            connection.ExecuteNonQuery("BEGIN;");
        }

        public new virtual SqliteConnection Connection => _connection;
        protected override DbConnection DbConnection => Connection;

        /// <summary>
        /// Specifies the IsolationLevel for this transaction. This cannot be changed if the transaction is complete or closed.
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
        /// Persists the results of all statements executed during this transaction.
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
        /// Reverses all changes made in this transaction. 
        /// </summary>
        public override void Rollback()
        {
            if (_completed || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.TransactionCompleted);
            }

            RollbackInternal();
        }

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
