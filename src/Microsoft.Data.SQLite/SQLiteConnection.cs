// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.SQLite.Interop;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.SQLite
{
    public class SQLiteConnection : DbConnection
    {
        private string _connectionString;
        private SQLiteConnectionStringBuilder _connectionOptions;
        private ConnectionState _state;
        private DatabaseHandle _handle;

        public SQLiteConnection()
        {
        }

        public SQLiteConnection([NotNull] string connectionString)
            : this()
        {
            Check.NotEmpty(connectionString, "connectionString");

            ConnectionString = connectionString;
        }

        internal DatabaseHandle Handle
        {
            get { return _handle; }
        }

        public override string ConnectionString
        {
            get { return _connectionString; }
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");
                if (_state != ConnectionState.Closed)
                    throw new InvalidOperationException(Strings.ConnectionStringRequiresClosedConnection);

                _connectionString = value;
                _connectionOptions = new SQLiteConnectionStringBuilder(value);
            }
        }

        public override string Database
        {
            get { return "main"; }
        }

        public override string DataSource
        {
            get { return _connectionOptions.Filename; }
        }

        public override string ServerVersion
        {
            get { return NativeMethods.sqlite3_libversion(); }
        }

        public override ConnectionState State
        {
            get { return _state; }
        }

        internal SQLiteTransaction Transaction { get; set; }

        private void SetState(ConnectionState value)
        {
            if (_state == value)
                return;

            var originalState = _state;
            _state = value;
            OnStateChange(new StateChangeEventArgs(originalState, value));
        }

        public override void Open()
        {
            if (_state == ConnectionState.Open)
                return;
            if (_connectionString == null)
                throw new InvalidOperationException(Strings.OpenRequiresSetConnectionString);

            Debug.Assert(_handle == null, "_handle is not null.");
            Debug.Assert(_connectionOptions != null, "_connectionOptions is null.");

            // TODO: Register transaction hooks
            var rc = NativeMethods.sqlite3_open_v2(
                _connectionOptions.Filename,
                out _handle,
                _connectionOptions.GetFlags(),
                _connectionOptions.VirtualFileSystem);
            MarshalEx.ThrowExceptionForRC(rc);

            SetState(ConnectionState.Open);
        }

        public override void Close()
        {
            ReleaseNativeObjects();
            SetState(ConnectionState.Closed);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SetState(ConnectionState.Closed);
            }

            ReleaseNativeObjects();

            base.Dispose(disposing);
        }

        private void ReleaseNativeObjects()
        {
            if (_handle == null || _handle.IsInvalid)
                return;

            _handle.Dispose();
            _handle = null;
        }

        public new SQLiteCommand CreateCommand()
        {
            return new SQLiteCommand { Connection = this, Transaction = Transaction };
        }

        protected override DbCommand CreateDbCommand()
        {
            return CreateCommand();
        }

        public new SQLiteTransaction BeginTransaction()
        {
            return BeginTransaction(IsolationLevel.Unspecified);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel);
        }

        public new SQLiteTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_state != ConnectionState.Open)
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("BeginTransaction"));
            if (Transaction != null)
                throw new InvalidOperationException(Strings.ParallelTransactionsNotSupported);

            return Transaction = new SQLiteTransaction(this, isolationLevel);
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotSupportedException();
        }
    }
}
