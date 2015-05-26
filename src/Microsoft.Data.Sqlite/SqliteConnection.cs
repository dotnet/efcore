// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite
{
    public class SqliteConnection : DbConnection
    {
        private const string MainDatabaseName = "main";

        private string _connectionString;
        private SqliteConnectionStringBuilder _connectionStringBuilder;
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
                _connectionStringBuilder = new SqliteConnectionStringBuilder(value);
            }
        }

        public override string Database => MainDatabaseName;
        public override string DataSource =>
            _state == ConnectionState.Open
                ? NativeMethods.sqlite3_db_filename(_db, MainDatabaseName)
                : _connectionStringBuilder.DataSource;
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

            var rc = NativeMethods.sqlite3_open16(_connectionStringBuilder.DataSource, out _db);
            MarshalEx.ThrowExceptionForRC(rc, _db);
            SetState(ConnectionState.Open);
        }

        public override void Close()
        {
            if (_db == null || _db.IsInvalid)
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
        public virtual new SqliteCommand CreateCommand() => new SqliteCommand { Connection = this, Transaction = Transaction };
        protected override DbCommand CreateDbCommand() => CreateCommand();
        public virtual new SqliteTransaction BeginTransaction() => BeginTransaction(IsolationLevel.Unspecified);
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => BeginTransaction(isolationLevel);

        public virtual new SqliteTransaction BeginTransaction(IsolationLevel isolationLevel)
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
