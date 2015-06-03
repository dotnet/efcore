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

#if NET45 || DNX451
using System.Data;
#endif

namespace Microsoft.Data.Sqlite
{
    public class SqliteDataReader : DbDataReader
    {
        private readonly Sqlite3Handle _db;
        private readonly Queue<Tuple<Sqlite3StmtHandle, bool>> _stmtQueue;
        private Sqlite3StmtHandle _stmt;
        private bool _hasRows;
        private bool _stepped;
        private bool _done;
        private bool _closed;

        internal SqliteDataReader(
            Sqlite3Handle db,
            Queue<Tuple<Sqlite3StmtHandle, bool>> stmtQueue,
            int recordsAffected)
        {
            if (stmtQueue.Count != 0)
            {
                var tuple = stmtQueue.Dequeue();
                _stmt = tuple.Item1;
                _hasRows = tuple.Item2;
            }

            _db = db;
            _stmtQueue = stmtQueue;
            RecordsAffected = recordsAffected;
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

        public virtual IntPtr Handle => _stmt?.DangerousGetHandle() ?? IntPtr.Zero;

        public override bool HasRows => _hasRows;
        public override bool IsClosed => _closed;
        public override int RecordsAffected { get; }

        /// <remarks>The <paramref name="name"/> parameter is case sensitive.</remarks>
        public override object this[string name] => GetValue(GetOrdinal(name));

        public override object this[int ordinal] => GetValue(ordinal);

        public override IEnumerator GetEnumerator()
        {
#if NET45 || DNX451
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
            MarshalEx.ThrowExceptionForRC(rc, _db);

            _done = rc == Constants.SQLITE_DONE;

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


#if NET45 || DNX451
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
        }

        public override string GetName(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Strings.FormatDataReaderClosed("GetName"));
            }

            var name = NativeMethods.sqlite3_column_name16(_stmt, ordinal);
            if (name == null && (ordinal < 0 || ordinal >= FieldCount))
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            return name;
        }

        /// <remarks>The <paramref name="name"/> parameter is case sensitive.</remarks>
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

            var typeName = NativeMethods.sqlite3_column_decltype16(_stmt, ordinal);
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
                case Constants.SQLITE_INTEGER:
                    return "INTEGER";

                case Constants.SQLITE_FLOAT:
                    return "REAL";

                case Constants.SQLITE_TEXT:
                    return "TEXT";

                case Constants.SQLITE_BLOB:
                    return "BLOB";

                case Constants.SQLITE_NULL:
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
                case Constants.SQLITE_INTEGER:
                    return typeof(long);

                case Constants.SQLITE_FLOAT:
                    return typeof(double);

                case Constants.SQLITE_TEXT:
                    return typeof(string);

                case Constants.SQLITE_BLOB:
                    return typeof(byte[]);

                case Constants.SQLITE_NULL:
                    return typeof(int);

                default:
                    Debug.Fail("Unexpected column type: " + sqliteType);
                    return typeof(int);
            }
        }

        private int GetSqliteType(int ordinal)
        {
            var type = NativeMethods.sqlite3_column_type(_stmt, ordinal);
            if (type == Constants.SQLITE_NULL && (ordinal < 0 || ordinal >= FieldCount))
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

            return GetSqliteType(ordinal) == Constants.SQLITE_NULL;
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

            return NativeMethods.sqlite3_column_text16(_stmt, ordinal);
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
            else if (type == typeof(byte))
            {
                return (T)(object)GetByte(ordinal);
            }
            else if (type == typeof(byte[]))
            {
                return (T)(object)GetBlob(ordinal);
            }
            else if (type == typeof(char))
            {
                return (T)(object)GetChar(ordinal);
            }
            else if (type == typeof(DateTime))
            {
                return (T)(object)GetDateTime(ordinal);
            }
            else if (type == typeof(DateTimeOffset))
            {
                return (T)(object)DateTimeOffset.Parse(GetString(ordinal));
            }
            else if (type == typeof(DBNull))
            {
                if (!_stepped || _done)
                {
                    throw new InvalidOperationException(Strings.NoData);
                }

                return (T)(object)DBNull.Value;
            }
            else if (type == typeof(decimal))
            {
                return (T)(object)GetDecimal(ordinal);
            }
            else if (type == typeof(double))
            {
                return (T)(object)GetDouble(ordinal);
            }
            else if (type == typeof(float))
            {
                return (T)(object)GetFloat(ordinal);
            }
            else if (type == typeof(Guid))
            {
                return (T)(object)GetGuid(ordinal);
            }
            else if (type == typeof(int))
            {
                return (T)(object)GetInt32(ordinal);
            }
            else if (type == typeof(long))
            {
                return (T)(object)GetInt64(ordinal);
            }
            else if (type == typeof(sbyte))
            {
                return (T)(object)((sbyte)GetInt64(ordinal));
            }
            else if (type == typeof(short))
            {
                return (T)(object)GetInt16(ordinal);
            }
            else if (type == typeof(string))
            {
                return (T)(object)GetString(ordinal);
            }
            else if (type == typeof(TimeSpan))
            {
                return (T)(object)TimeSpan.Parse(GetString(ordinal));
            }
            else if (type == typeof(uint))
            {
                return (T)(object)((uint)GetInt64(ordinal));
            }
            else if (type == typeof(ulong))
            {
                return (T)(object)((ulong)GetInt64(ordinal));
            }
            else if (type == typeof(ushort))
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
                case Constants.SQLITE_INTEGER:
                    return GetInt64(ordinal);

                case Constants.SQLITE_FLOAT:
                    return GetDouble(ordinal);

                case Constants.SQLITE_TEXT:
                    return GetString(ordinal);

                case Constants.SQLITE_BLOB:
                    return GetBlob(ordinal);

                case Constants.SQLITE_NULL:
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
