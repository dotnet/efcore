// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.Sqlite.Utilities;

namespace Microsoft.Data.Sqlite
{
    public class SqliteTransaction : DbTransaction
    {
        private SqliteConnection _connection;
        private IsolationLevel _isolationLevel;

        internal SqliteTransaction(SqliteConnection connection, IsolationLevel isolationLevel)
        {
            Debug.Assert(connection != null, "connection is null.");
            Debug.Assert(connection.State == ConnectionState.Open, "connection.State is not Open.");

            _connection = connection;
            _isolationLevel = isolationLevel;

            if (_isolationLevel == IsolationLevel.ReadUncommitted)
            {
                _connection.ExecuteNonQuery("PRAGMA read_uncommitted = 1");
            }
            else if (_isolationLevel == IsolationLevel.Serializable)
            {
                _connection.ExecuteNonQuery("PRAGMA read_uncommitted = 0");
            }
            else if (_isolationLevel != IsolationLevel.Unspecified)
            {
                throw new ArgumentException(Strings.FormatInvalidIsolationLevel(isolationLevel));
            }

            _connection.ExecuteNonQuery("BEGIN");
        }

        public new SqliteConnection Connection
        {
            get { return _connection; }
        }

        protected override DbConnection DbConnection
        {
            get { return Connection; }
        }

        public override IsolationLevel IsolationLevel
        {
            get
            {
                CheckCompleted();

                if (_isolationLevel == IsolationLevel.Unspecified)
                {
                    _isolationLevel = _connection.ExecuteScalar<long>("PRAGMA read_uncommitted") != 0
                        ? IsolationLevel.ReadUncommitted
                        : IsolationLevel.Serializable;
                }

                return _isolationLevel;
            }
        }

        public override void Commit()
        {
            CheckCompleted();

            _connection.ExecuteNonQuery("COMMIT");
            _connection.Transaction = null;
            _connection = null;
        }

        public override void Rollback()
        {
            CheckCompleted();

            Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing
                || _connection == null)
            {
                return;
            }

            if (_connection.State == ConnectionState.Open)
            {
                _connection.ExecuteNonQuery("ROLLBACK");
            }

            _connection.Transaction = null;
            _connection = null;
        }

        private void CheckCompleted()
        {
            if (_connection == null
                || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.TransactionCompleted);
            }
        }
    }
}
