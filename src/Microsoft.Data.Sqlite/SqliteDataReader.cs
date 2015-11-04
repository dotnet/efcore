// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Data.Sqlite.Interop;
using Microsoft.Data.Sqlite.Utilities;
using static Microsoft.Data.Sqlite.Interop.Constants;
#if NET451
using System.Data;
#endif

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Provides methods for reading the result of a statement executed against a SQLite database.
    /// </summary>
    public class SqliteDataReader : DbDataReader
    {
        private readonly SqliteConnection _connection;
        private readonly bool _closeConnection;
        private readonly Queue<Tuple<Sqlite3StmtHandle, bool>> _stmtQueue;
        private Sqlite3StmtHandle _stmt;
        private bool _hasRows;
        private bool _stepped;
        private bool _done;
        private bool _closed;

        internal SqliteDataReader(
            SqliteConnection connection,
            Queue<Tuple<Sqlite3StmtHandle, bool>> stmtQueue,
            int recordsAffected,
            bool closeConnection)
        {
            if (stmtQueue.Count != 0)
            {
                var tuple = stmtQueue.Dequeue();
                _stmt = tuple.Item1;
                _hasRows = tuple.Item2;
            }

            _connection = connection;
            _stmtQueue = stmtQueue;
            RecordsAffected = recordsAffected;
            _closeConnection = closeConnection;
        }

        public override int Depth => 0;

        public override int FieldCount
        {
            get
            {
                if (_closed)
                {
                    throw new InvalidOperationException(Strings.FormatDataReaderClosed("FieldCount"));
                }

                return NativeMethods.sqlite3_column_count(_stmt);
            }
        }

        /// <summary>
        /// Represents an unmanaged pointer to a sqlite3_stmt object. <see href="https://www.sqlite.org/c3ref/stmt.html">See SQLite.org for more documentation on proper usage of this object.</see>
        /// </summary>
        public virtual IntPtr Handle => _stmt?.DangerousGetHandle() ?? IntPtr.Zero;

        public override bool HasRows => _hasRows;
        public override bool IsClosed => _closed;
        public override int RecordsAffected { get; }

        /// <remarks>The <paramref name="name" /> parameter is case sensitive.</remarks>
        public override object this[string name] => GetValue(GetOrdinal(name));

        public override object this[int ordinal] => GetValue(ordinal);

        public override IEnumerator GetEnumerator()
        {
#if NET451
            return new DbEnumerator(this);
#else
    // TODO: Remove when the System.Data.Common includes DbEnumerator
            throw new NotImplementedException();
#endif
        }

        public override bool Read()
        {
            if (_closed)
            {
                throw new InvalidOperationException(Strings.FormatDataReaderClosed("Read"));
            }

            if (!_stepped)
            {
                _stepped = true;

                return _hasRows;
            }

            var rc = NativeMethods.sqlite3_step(_stmt);
            MarshalEx.ThrowExceptionForRC(rc, _connection.DbHandle);

            _done = rc == SQLITE_DONE;

            return !_done;
        }

        public override bool NextResult()
        {
            if (_stmtQueue.Count == 0)
            {
                return false;
            }

            _stmt.Dispose();

            var tuple = _stmtQueue.Dequeue();
            _stmt = tuple.Item1;
            _hasRows = tuple.Item2;
            _stepped = false;
            _done = false;

            return true;
        }

#if NET451
        // TODO: Remove when fixed in System.Data.Common
        public override void Close() => Dispose(true);

        public override DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }
#endif

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (_stmt != null)
            {
                _stmt.Dispose();
                _stmt = null;
            }

            while (_stmtQueue.Count != 0)
            {
                _stmtQueue.Dequeue().Item1.Dispose();
            }

            _closed = true;

            if (_closeConnection)
            {
                _connection.Close();
            }
        }

        public override string GetName(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Strings.FormatDataReaderClosed("GetName"));
            }

            var name = NativeMethods.sqlite3_column_name(_stmt, ordinal);
            if (name == null
                && (ordinal < 0 || ordinal >= FieldCount))
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            return name;
        }

        /// <remarks>The <paramref name="name" /> parameter is case sensitive.</remarks>
        public override int GetOrdinal(string name)
        {
            for (var i = 0; i < FieldCount; i++)
            {
                if (GetName(i) == name)
                {
                    return i;
                }
            }

            // NB: Message is provided by framework
            throw new ArgumentOutOfRangeException(nameof(name), name, message: null);
        }

        public override string GetDataTypeName(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Strings.FormatDataReaderClosed("GetDataTypeName"));
            }

            var typeName = NativeMethods.sqlite3_column_decltype(_stmt, ordinal);
            if (typeName != null)
            {
                var i = typeName.IndexOf('(');

                return i == -1
                    ? typeName
                    : typeName.Substring(0, i);
            }

            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_INTEGER:
                    return "INTEGER";

                case SQLITE_FLOAT:
                    return "REAL";

                case SQLITE_TEXT:
                    return "TEXT";

                case SQLITE_BLOB:
                    return "BLOB";

                case SQLITE_NULL:
                    return "INTEGER";

                default:
                    Debug.Fail("Unexpected column type: " + sqliteType);
                    return "INTEGER";
            }
        }

        public override Type GetFieldType(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Strings.FormatDataReaderClosed("GetFieldType"));
            }

            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_INTEGER:
                    return typeof(long);

                case SQLITE_FLOAT:
                    return typeof(double);

                case SQLITE_TEXT:
                    return typeof(string);

                case SQLITE_BLOB:
                    return typeof(byte[]);

                case SQLITE_NULL:
                    return typeof(int);

                default:
                    Debug.Fail("Unexpected column type: " + sqliteType);
                    return typeof(int);
            }
        }

        private int GetSqliteType(int ordinal)
        {
            var type = NativeMethods.sqlite3_column_type(_stmt, ordinal);
            if (type == SQLITE_NULL
                && (ordinal < 0 || ordinal >= FieldCount))
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            return type;
        }

        public override bool IsDBNull(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Strings.FormatDataReaderClosed("IsDBNull"));
            }
            if (!_stepped || _done)
            {
                throw new InvalidOperationException(Strings.NoData);
            }

            return GetSqliteType(ordinal) == SQLITE_NULL;
        }

        public override bool GetBoolean(int ordinal) => GetInt64(ordinal) != 0;
        public override byte GetByte(int ordinal) => (byte)GetInt64(ordinal);
        public override char GetChar(int ordinal) => (char)GetInt64(ordinal);
        public override DateTime GetDateTime(int ordinal) => DateTime.Parse(GetString(ordinal), CultureInfo.InvariantCulture);
        public override decimal GetDecimal(int ordinal) => decimal.Parse(GetString(ordinal), CultureInfo.InvariantCulture);

        public override double GetDouble(int ordinal)
        {
            if (IsDBNull(ordinal))
            {
                throw new InvalidCastException();
            }

            return NativeMethods.sqlite3_column_double(_stmt, ordinal);
        }

        public override float GetFloat(int ordinal) => (float)GetDouble(ordinal);
        public override Guid GetGuid(int ordinal) => new Guid(GetBlob(ordinal));
        public override short GetInt16(int ordinal) => (short)GetInt64(ordinal);
        public override int GetInt32(int ordinal) => (int)GetInt64(ordinal);

        public override long GetInt64(int ordinal)
        {
            if (IsDBNull(ordinal))
            {
                throw new InvalidCastException();
            }

            return NativeMethods.sqlite3_column_int64(_stmt, ordinal);
        }

        public override string GetString(int ordinal)
        {
            if (IsDBNull(ordinal))
            {
                throw new InvalidCastException();
            }

            return NativeMethods.sqlite3_column_text(_stmt, ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotSupportedException();
        }

        public override T GetFieldValue<T>(int ordinal)
        {
            var type = typeof(T).UnwrapNullableType().UnwrapEnumType();
            if (type == typeof(bool))
            {
                return (T)(object)GetBoolean(ordinal);
            }
            if (type == typeof(byte))
            {
                return (T)(object)GetByte(ordinal);
            }
            if (type == typeof(byte[]))
            {
                return (T)(object)GetBlob(ordinal);
            }
            if (type == typeof(char))
            {
                return (T)(object)GetChar(ordinal);
            }
            if (type == typeof(DateTime))
            {
                return (T)(object)GetDateTime(ordinal);
            }
            if (type == typeof(DateTimeOffset))
            {
                return (T)(object)DateTimeOffset.Parse(GetString(ordinal));
            }
            if (type == typeof(DBNull))
            {
                if (!_stepped || _done)
                {
                    throw new InvalidOperationException(Strings.NoData);
                }

                return (T)(object)DBNull.Value;
            }
            if (type == typeof(decimal))
            {
                return (T)(object)GetDecimal(ordinal);
            }
            if (type == typeof(double))
            {
                return (T)(object)GetDouble(ordinal);
            }
            if (type == typeof(float))
            {
                return (T)(object)GetFloat(ordinal);
            }
            if (type == typeof(Guid))
            {
                return (T)(object)GetGuid(ordinal);
            }
            if (type == typeof(int))
            {
                return (T)(object)GetInt32(ordinal);
            }
            if (type == typeof(long))
            {
                return (T)(object)GetInt64(ordinal);
            }
            if (type == typeof(sbyte))
            {
                return (T)(object)((sbyte)GetInt64(ordinal));
            }
            if (type == typeof(short))
            {
                return (T)(object)GetInt16(ordinal);
            }
            if (type == typeof(string))
            {
                return (T)(object)GetString(ordinal);
            }
            if (type == typeof(TimeSpan))
            {
                return (T)(object)TimeSpan.Parse(GetString(ordinal));
            }
            if (type == typeof(uint))
            {
                return (T)(object)((uint)GetInt64(ordinal));
            }
            if (type == typeof(ulong))
            {
                return (T)(object)((ulong)GetInt64(ordinal));
            }
            if (type == typeof(ushort))
            {
                return (T)(object)((ushort)GetInt64(ordinal));
            }

            return base.GetFieldValue<T>(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Strings.FormatDataReaderClosed("GetValue"));
            }

            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_INTEGER:
                    return GetInt64(ordinal);

                case SQLITE_FLOAT:
                    return GetDouble(ordinal);

                case SQLITE_TEXT:
                    return GetString(ordinal);

                case SQLITE_BLOB:
                    return GetBlob(ordinal);

                case SQLITE_NULL:
                    if (!_stepped || _done)
                    {
                        throw new InvalidOperationException(Strings.NoData);
                    }

                    return DBNull.Value;

                default:
                    Debug.Fail("Unexpected column type: " + sqliteType);
                    return GetInt32(ordinal);
            }
        }

        public override int GetValues(object[] values)
        {
            int i;
            for (i = 0; i < FieldCount; i++)
            {
                values[i] = GetValue(i);
            }

            return i;
        }

        private byte[] GetBlob(int ordinal)
        {
            if (IsDBNull(ordinal))
            {
                throw new InvalidCastException();
            }

            return NativeMethods.sqlite3_column_blob(_stmt, ordinal);
        }
    }
}
