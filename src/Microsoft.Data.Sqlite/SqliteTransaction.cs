// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite.Utilities;

namespace Microsoft.Data.Sqlite
{
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

        public virtual new SqliteConnection Connection => _connection;
        protected override DbConnection DbConnection => Connection;

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

        public override void Commit()
        {
            if (_completed || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.TransactionCompleted);
            }

            _connection.ExecuteNonQuery("COMMIT;");
            Complete();
        }

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
            if (disposing && !_completed && _connection.State == ConnectionState.Open)
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