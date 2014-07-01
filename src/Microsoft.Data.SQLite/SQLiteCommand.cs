// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.SQLite.Interop;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.SQLite
{
    public class SQLiteCommand : DbCommand
    {
        private SQLiteConnection _connection;
        private CommandType _commandType = CommandType.Text;
        private string _commandText;
        private bool _prepared;
        private IEnumerable<StatementHandle> _handles;
        private SQLiteParameterCollection _parameters = null;

        public SQLiteCommand()
        {
        }

        public SQLiteCommand([NotNull] string commandText)
            : this()
        {
            Check.NotEmpty(commandText, "commandText");

            _commandText = commandText;
        }

        public SQLiteCommand([NotNull] string commandText, [NotNull] SQLiteConnection connection)
            : this(commandText)
        {
            Check.NotEmpty(commandText, "commandText");
            Check.NotNull(connection, "connection");

            _connection = connection;
        }

        public SQLiteCommand(
            [NotNull] string commandText,
            [NotNull] SQLiteConnection connection,
            [NotNull] SQLiteTransaction transaction)
            : this(commandText, connection)
        {
            Check.NotEmpty(commandText, "commandText");
            Check.NotNull(connection, "connection");
            Check.NotNull(transaction, "transaction");

            Transaction = transaction;
        }

        public override CommandType CommandType
        {
            get { return _commandType; }
            set
            {
                if (value != CommandType.Text)
                {
                    throw new ArgumentException(Strings.FormatInvalidCommandType(value));
                }

                _commandType = value;
            }
        }

        public override string CommandText
        {
            get { return _commandText; }
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");

                if (_prepared && _commandText != value)
                {
                    ReleaseNativeObjects();
                    _prepared = false;
                }

                _commandText = value;
            }
        }

        public new SQLiteConnection Connection
        {
            get { return _connection; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                if (_prepared && _connection != value)
                {
                    ReleaseNativeObjects();
                    _prepared = false;
                }

                _connection = value;
            }
        }

        protected override DbConnection DbConnection
        {
            get { return Connection; }
            set { Connection = (SQLiteConnection)value; }
        }

        public new SQLiteTransaction Transaction { get; set; }

        protected override DbTransaction DbTransaction
        {
            get { return Transaction; }
            set { Transaction = (SQLiteTransaction)value; }
        }

        public new SQLiteParameterCollection Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    _parameters = new SQLiteParameterCollection();
                }

                return _parameters;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return Parameters; }
        }

        public override int CommandTimeout { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }

        internal SQLiteDataReader OpenReader { get; set; }

        public new SQLiteParameter CreateParameter()
        {
            return new SQLiteParameter();
        }

        protected override DbParameter CreateDbParameter()
        {
            return CreateParameter();
        }

        public override void Prepare()
        {
            if (OpenReader != null)
            {
                throw new InvalidOperationException(Strings.OpenReaderExists);
            }
            if (_connection == null
                || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("Prepare"));
            }
            if (string.IsNullOrWhiteSpace(_commandText))
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresSetCommandText("Prepare"));
            }
            if (_prepared)
            {
                return;
            }

            Debug.Assert(_connection.Handle != null && !_connection.Handle.IsInvalid, "_connection.Handle is null.");
            Debug.Assert(_handles == null, "_handles is not null.");

            var handles = new List<StatementHandle>();
            var remainingSql = _commandText;
            do
            {
                StatementHandle handle;
                var rc = NativeMethods.sqlite3_prepare_v2(
                    _connection.Handle,
                    remainingSql,
                    out handle,
                    out remainingSql);
                MarshalEx.ThrowExceptionForRC(rc);

                handles.Add(handle);
            }
            while (!string.IsNullOrWhiteSpace(remainingSql));

            _handles = handles;
            _prepared = true;
        }

        public new SQLiteDataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }

        // TODO: Honor behavior
        public new SQLiteDataReader ExecuteReader(CommandBehavior behavior)
        {
            if (OpenReader != null)
            {
                throw new InvalidOperationException(Strings.OpenReaderExists);
            }
            if (_connection == null
                || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("ExecuteReader"));
            }
            if (string.IsNullOrWhiteSpace(_commandText))
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresSetCommandText("ExecuteReader"));
            }

            ValidateTransaction();
            Prepare();
            Bind();

            var changes = 0;
            var resultHandles = new List<StatementHandle>();
            foreach (var handle in _handles)
            {
                var hasResults = NativeMethods.sqlite3_stmt_readonly(handle) != 0;

                var rc = NativeMethods.sqlite3_step(handle);
                if (rc == Constants.SQLITE_ROW
                    || (rc == Constants.SQLITE_DONE && hasResults))
                {
                    resultHandles.Add(handle);

                    continue;
                }

                rc = NativeMethods.sqlite3_reset(handle);
                MarshalEx.ThrowExceptionForRC(rc);

                changes += NativeMethods.sqlite3_changes(_connection.Handle);
            }

            return OpenReader = new SQLiteDataReader(this, resultHandles, changes);
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        public override int ExecuteNonQuery()
        {
            if (OpenReader != null)
            {
                throw new InvalidOperationException(Strings.OpenReaderExists);
            }
            if (_connection == null
                || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("ExecuteNonQuery"));
            }
            if (string.IsNullOrWhiteSpace(_commandText))
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresSetCommandText("ExecuteNonQuery"));
            }

            ValidateTransaction();
            Prepare();
            Bind();

            Debug.Assert(_connection.Handle != null && !_connection.Handle.IsInvalid, "_connection.Handle is null.");

            var changes = 0;
            foreach (var handle in _handles)
            {
                var hasChanges = NativeMethods.sqlite3_stmt_readonly(handle) == 0;

                NativeMethods.sqlite3_step(handle);
                var rc = NativeMethods.sqlite3_reset(handle);
                MarshalEx.ThrowExceptionForRC(rc);

                if (hasChanges)
                {
                    changes += NativeMethods.sqlite3_changes(_connection.Handle);
                }
            }

            return changes;
        }

        public override object ExecuteScalar()
        {
            if (OpenReader != null)
            {
                throw new InvalidOperationException(Strings.OpenReaderExists);
            }
            if (_connection == null
                || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("ExecuteScalar"));
            }
            if (string.IsNullOrWhiteSpace(_commandText))
            {
                throw new InvalidOperationException(Strings.FormatCallRequiresSetCommandText("ExecuteScalar"));
            }

            ValidateTransaction();
            Prepare();
            Bind();

            object result = null;
            var gotResult = false;
            foreach (var handle in _handles)
            {
                try
                {
                    var rc = NativeMethods.sqlite3_step(handle);
                    if (rc != Constants.SQLITE_DONE
                        && rc != Constants.SQLITE_ROW)
                    {
                        MarshalEx.ThrowExceptionForRC(rc);
                    }

                    var hasResults = NativeMethods.sqlite3_stmt_readonly(handle) != 0;

                    if (!gotResult && hasResults)
                    {
                        if (rc == Constants.SQLITE_ROW)
                        {
                            var declaredType = NativeMethods.sqlite3_column_decltype(handle, 0);
                            var sqliteType = (SQLiteType)NativeMethods.sqlite3_column_type(handle, 0);
                            var map = SQLiteTypeMap.FromDeclaredType(declaredType, sqliteType);
                            var value = ColumnReader.Read(map.SQLiteType, handle, 0);

                            result = map.FromInterop(value);
                        }

                        gotResult = true;
                    }
                }
                finally
                {
                    var rc = NativeMethods.sqlite3_reset(handle);
                    MarshalEx.ThrowExceptionForRC(rc);
                }
            }

            return result;
        }

        public override void Cancel()
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection = null;
                _prepared = false;

                if (OpenReader != null)
                {
                    OpenReader.Close();
                }
            }

            ReleaseNativeObjects();

            base.Dispose(disposing);
        }

        private void Bind()
        {
            Debug.Assert(_prepared, "_prepared is false.");
            Debug.Assert(_handles != null, "_handles is null.");
            Debug.Assert(OpenReader == null, "ActiveReader is not null.");
            if (_parameters == null
                || _parameters.Bound)
            {
                return;
            }

            foreach (var handle in _handles)
            {
                var rc = NativeMethods.sqlite3_clear_bindings(handle);
                MarshalEx.ThrowExceptionForRC(rc);
            }

            _parameters.Bind(_handles);
        }

        private void ReleaseNativeObjects()
        {
            if (_handles == null)
            {
                return;
            }

            foreach (var handle in _handles)
            {
                handle.Dispose();
            }

            _handles = null;
        }

        private void ValidateTransaction()
        {
            if (Transaction != _connection.Transaction)
            {
                if (Transaction == null)
                {
                    throw new InvalidOperationException(Strings.TransactionRequired);
                }

                throw new InvalidOperationException(Strings.TransactionConnectionMismatch);
            }
        }
    }
}
