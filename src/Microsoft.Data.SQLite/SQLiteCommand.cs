// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
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
        private StatementHandle _handle;

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
                    throw new ArgumentException(Strings.FormatInvalidCommandType(value));

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

        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        public override int CommandTimeout { get; set; }
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }

        protected override DbParameter CreateDbParameter()
        {
            // TODO
            throw new NotImplementedException();
        }

        public override void Prepare()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("Prepare"));
            if (string.IsNullOrWhiteSpace(_commandText))
                throw new InvalidOperationException(Strings.FormatCallRequiresSetCommandText("Prepare"));
            if (_prepared)
                return;

            Debug.Assert(_connection.Handle != null && !_connection.Handle.IsInvalid, "_connection.Handle is null.");
            Debug.Assert(_handle == null, "_handle is not null.");

            string tail;
            var rc = NativeMethods.sqlite3_prepare_v2(
                _connection.Handle,
                _commandText,
                Encoding.UTF8.GetByteCount(_commandText) + 1,
                out _handle,
                out tail);
            MarshalEx.ThrowExceptionForRC(rc);

            // TODO: Handle this. Only the only first statement is compiled. This is what
            //       remains uncompiled.
            Debug.Assert(string.IsNullOrEmpty(tail), "CommandText contains more than one statement.");

            _prepared = true;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("ExecuteDbDataReader"));
            if (string.IsNullOrWhiteSpace(_commandText))
                throw new InvalidOperationException(Strings.FormatCallRequiresSetCommandText("ExecuteDbDataReader"));

            Prepare();

            // TODO
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("ExecuteNonQuery"));
            if (string.IsNullOrWhiteSpace(_commandText))
                throw new InvalidOperationException(Strings.FormatCallRequiresSetCommandText("ExecuteNonQuery"));

            Prepare();

            NativeMethods.sqlite3_step(_handle);
            var rc = NativeMethods.sqlite3_reset(_handle);
            MarshalEx.ThrowExceptionForRC(rc);

            Debug.Assert(_connection.Handle != null && !_connection.Handle.IsInvalid, "_connection.Handle is null.");

            return NativeMethods.sqlite3_changes(_connection.Handle);
        }

        public override object ExecuteScalar()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
                throw new InvalidOperationException(Strings.FormatCallRequiresOpenConnection("ExecuteScalar"));
            if (string.IsNullOrWhiteSpace(_commandText))
                throw new InvalidOperationException(Strings.FormatCallRequiresSetCommandText("ExecuteScalar"));

            Prepare();

            var rc = NativeMethods.sqlite3_step(_handle);
            try
            {
                if (rc == Constants.SQLITE_DONE)
                    return null;
                if (rc != Constants.SQLITE_ROW)
                    MarshalEx.ThrowExceptionForRC(rc);

                switch (NativeMethods.sqlite3_column_type(_handle, 0))
                {
                    case Constants.SQLITE_INTEGER:
                        return NativeMethods.sqlite3_column_int64(_handle, 0);

                    case Constants.SQLITE_FLOAT:
                        return NativeMethods.sqlite3_column_double(_handle, 0);

                    case Constants.SQLITE_TEXT:
                        return NativeMethods.sqlite3_column_text(_handle, 0);

                    case Constants.SQLITE_BLOB:
                        return NativeMethods.sqlite3_column_blob(_handle, 0);

                    case Constants.SQLITE_NULL:
                        return DBNull.Value;

                    default:
                        Debug.Assert(false, "Unexpected value.");
                        return DBNull.Value;
                }
            }
            finally
            {
                rc = NativeMethods.sqlite3_reset(_handle);
                MarshalEx.ThrowExceptionForRC(rc);
            }
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
    }
}
