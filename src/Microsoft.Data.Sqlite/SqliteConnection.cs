// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Represents a connection with a SQLite database.
    /// </summary>
    public class SqliteConnection : DbConnection
    {
        private const string MainDatabaseName = "main";

        private string _connectionString;
        private ConnectionState _state;
        private Sqlite3Handle _db;

        public SqliteConnection()
        {
        }

        public SqliteConnection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        internal virtual Sqlite3Handle DbHandle => _db;

        /// <summary>
        /// Represents an unmanaged pointer to a sqlite3 database object. <see href="https://www.sqlite.org/c3ref/sqlite3.html">See SQLite.org for more documentation on proper usage of this object.</see>
        /// </summary>
        public virtual IntPtr Handle => _db?.DangerousGetHandle() ?? IntPtr.Zero;

        public override string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (_state != ConnectionState.Closed)
                {
                    throw new InvalidOperationException(Strings.ConnectionStringRequiresClosedConnection);
                }

                _connectionString = value;
                ConnectionStringBuilder = new SqliteConnectionStringBuilder(value);
            }
        }

        internal SqliteConnectionStringBuilder ConnectionStringBuilder { get; set; }

        public override string Database => MainDatabaseName;

        public override string DataSource =>
            _state == ConnectionState.Open
                ? NativeMethods.sqlite3_db_filename(_db, MainDatabaseName)
                : ConnectionStringBuilder.DataSource;

        /// <summary>
        /// Corresponds to the version of the SQLite library used by the connection.
        /// </summary>
        public override string ServerVersion => NativeMethods.sqlite3_libversion();
        public override ConnectionState State => _state;
        protected internal SqliteTransaction Transaction { get; set; }

        protected virtual void SetState(ConnectionState value)
        {
            var originalState = _state;
            if (originalState != value)
            {
                _state = value;
                OnStateChange(new StateChangeEventArgs(originalState, value));
            }
        }

        public override void Open()
        {
            if (_state == ConnectionState.Open)
            {
                return;
            }
            if (ConnectionString == null)
            {
                throw new InvalidOperationException(Strings.OpenRequiresSetConnectionString);
            }

            var flags = Constants.SQLITE_OPEN_READWRITE | Constants.SQLITE_OPEN_CREATE;
            flags |= (ConnectionStringBuilder.CacheMode == CacheMode.Shared) ? Constants.SQLITE_OPEN_SHAREDCACHE : Constants.SQLITE_OPEN_PRIVATECACHE;

            var rc = NativeMethods.sqlite3_open_v2(ConnectionStringBuilder.DataSource, out _db, flags, vfs: null);
            MarshalEx.ThrowExceptionForRC(rc, _db);
            SetState(ConnectionState.Open);
        }

        public override void Close()
        {
            if (_db == null
                || _db.IsInvalid)
            {
                return;
            }

            Transaction?.Dispose();
            _db.Dispose();
            _db = null;
            SetState(ConnectionState.Closed);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }

            base.Dispose(disposing);
        }

        // NB: Other providers don't set Transaction
        public new virtual SqliteCommand CreateCommand() => new SqliteCommand { Connection = this, Transaction = Transaction };
        protected override DbCommand CreateDbCommand() => CreateCommand();
        public new virtual SqliteTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => BeginTransaction(isolationLevel);

        public new virtual SqliteTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_state != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("BeginTransaction"));
            }
            if (Transaction != null)
            {
                throw new InvalidOperationException(Strings.ParallelTransactionsNotSupported);
            }

            return Transaction = new SqliteTransaction(this, isolationLevel);
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }
    }
}
