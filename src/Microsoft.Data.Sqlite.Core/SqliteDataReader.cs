// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;
using System.Text;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Provides methods for reading the result of a command executed against a SQLite database.
    /// </summary>
    public class SqliteDataReader : DbDataReader
    {
        private static readonly byte[] _emptyByteArray = new byte[0];

        private readonly SqliteCommand _command;
        private readonly bool _closeConnection;
        private readonly Queue<(sqlite3_stmt, bool)> _stmtQueue;
        private sqlite3_stmt _stmt;
        private bool _hasRows;
        private bool _stepped;
        private bool _done;
        private bool _closed;

        internal SqliteDataReader(
            SqliteCommand command,
            Queue<(sqlite3_stmt, bool)> stmtQueue,
            int recordsAffected,
            bool closeConnection)
        {
            if (stmtQueue.Count != 0)
            {
                (_stmt, _hasRows) = stmtQueue.Dequeue();
            }

            _command = command;
            _stmtQueue = stmtQueue;
            RecordsAffected = recordsAffected;
            _closeConnection = closeConnection;
        }

        /// <summary>
        /// Gets the depth of nesting for the current row. Always zero.
        /// </summary>
        /// <value>The depth of nesting for the current row.</value>
        public override int Depth
            => 0;

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        /// <value>The number of columns in the current row.</value>
        public override int FieldCount
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(FieldCount)))
                : raw.sqlite3_column_count(_stmt);

        /// <summary>
        /// Gets a handle to underlying prepared statement.
        /// </summary>
        /// <value>A handle to underlying prepared statement.</value>
        /// <seealso href="http://sqlite.org/c3ref/stmt.html">Prepared Statement Object</seealso>
        public virtual sqlite3_stmt Handle
            => _stmt;

        /// <summary>
        /// Gets a value indicating whether the data reader contains any rows.
        /// </summary>
        /// <value>A value indicating whether the data reader contains any rows.</value>
        public override bool HasRows
            => _hasRows;

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        /// <value>A value indicating whether the data reader is closed.</value>
        public override bool IsClosed
            => _closed;

        /// <summary>
        /// Gets the number of rows inserted, updated, or deleted. -1 for SELECT statements.
        /// </summary>
        /// <value>The number of rows inserted, updated, or deleted.</value>
        public override int RecordsAffected { get; }

        /// <summary>
        /// Gets the value of the specified column.
        /// </summary>
        /// <param name="name">The name of the column. The value is case-sensitive.</param>
        /// <returns>The value.</returns>
        public override object this[string name]
            => GetValue(GetOrdinal(name));

        /// <summary>
        /// Gets the value of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value.</returns>
        public override object this[int ordinal]
            => GetValue(ordinal);

        /// <summary>
        /// Gets an enumerator that can be used to iterate through the rows in the data reader.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public override IEnumerator GetEnumerator()
            => new DbEnumerator(this, closeReader: false);

        /// <summary>
        /// Advances to the next row in the result set.
        /// </summary>
        /// <returns>true if there are more rows; otherwise, false.</returns>
        public override bool Read()
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(Read)));
            }

            if (!_stepped)
            {
                _stepped = true;

                return _hasRows;
            }

            var rc = raw.sqlite3_step(_stmt);
            SqliteException.ThrowExceptionForRC(rc, _command.Connection.Handle);

            _done = rc == raw.SQLITE_DONE;

            return !_done;
        }

        /// <summary>
        /// Advances to the next result set for batched statements.
        /// </summary>
        /// <returns>true if there are more result sets; otherwise, false.</returns>
        public override bool NextResult()
        {
            if (_stmtQueue.Count == 0)
            {
                return false;
            }

            (_stmt, _hasRows) = _stmtQueue.Dequeue();
            _stepped = false;
            _done = false;

            return true;
        }

        /// <summary>
        /// Closes the data reader.
        /// </summary>
        public override void Close()
            => Dispose(true);

        /// <summary>
        /// Releases any resources used by the data reader and closes it.
        /// </summary>
        /// <param name="disposing">
        /// true to release managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            _closed = true;

            if (_closeConnection)
            {
                _command.Connection.Close();
            }
        }

        /// <summary>
        /// Gets the name of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The name of the column.</returns>
        public override string GetName(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetName)));
            }

            var name = raw.sqlite3_column_name(_stmt, ordinal);
            if (name == null
                && (ordinal < 0 || ordinal >= FieldCount))
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            return name;
        }

        /// <summary>
        /// Gets the ordinal of the specified column.
        /// </summary>
        /// <param name="name">The name of the column.</param>
        /// <returns>The zero-based column ordinal.</returns>
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

        /// <summary>
        /// Gets the declared data type name of the specified column. The storage class is returned for computed
        /// columns.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type name of the column.</returns>
        /// <remarks>Due to SQLite's dynamic type system, this may not reflect the actual type of the value.</remarks>
        /// <seealso href="http://sqlite.org/datatype3.html">Datatypes In SQLite Version 3</seealso>
        public override string GetDataTypeName(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetDataTypeName)));
            }

            var typeName = raw.sqlite3_column_decltype(_stmt, ordinal);
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
                case raw.SQLITE_INTEGER:
                    return "INTEGER";

                case raw.SQLITE_FLOAT:
                    return "REAL";

                case raw.SQLITE_TEXT:
                    return "TEXT";

                case raw.SQLITE_BLOB:
                    return "BLOB";

                case raw.SQLITE_NULL:
                    return "INTEGER";

                default:
                    Debug.Assert(false, "Unexpected column type: " + sqliteType);
                    return "INTEGER";
            }
        }

        /// <summary>
        /// Gets the data type of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The data type of the column.</returns>
        public override Type GetFieldType(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetFieldType)));
            }

            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case raw.SQLITE_INTEGER:
                    return typeof(long);

                case raw.SQLITE_FLOAT:
                    return typeof(double);

                case raw.SQLITE_TEXT:
                    return typeof(string);

                case raw.SQLITE_BLOB:
                    return typeof(byte[]);

                case raw.SQLITE_NULL:
                    return typeof(int);

                default:
                    Debug.Assert(false, "Unexpected column type: " + sqliteType);
                    return typeof(int);
            }
        }

        private int GetSqliteType(int ordinal)
        {
            var type = raw.sqlite3_column_type(_stmt, ordinal);
            if (type == raw.SQLITE_NULL
                && (ordinal < 0 || ordinal >= FieldCount))
            {
                // NB: Message is provided by the framework
                throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, message: null);
            }

            return type;
        }

        /// <summary>
        /// Gets a value indicating whether the specified column is <see cref="DBNull" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>true if the specified column is <see cref="DBNull" />; otherwise, false.</returns>
        public override bool IsDBNull(int ordinal)
            => _closed
                ? throw new InvalidOperationException(Resources.DataReaderClosed(nameof(IsDBNull)))
                : !_stepped || _done
                    ? throw new InvalidOperationException(Resources.NoData)
                    : GetSqliteType(ordinal) == raw.SQLITE_NULL;

        /// <summary>
        /// Gets the value of the specified column as a <see cref="bool" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override bool GetBoolean(int ordinal)
            => GetInt64(ordinal) != 0;

        /// <summary>
        /// Gets the value of the specified column as a <see cref="byte" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override byte GetByte(int ordinal)
            => (byte)GetInt64(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="char" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override char GetChar(int ordinal)
            => (char)GetInt64(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="DateTime" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override DateTime GetDateTime(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case raw.SQLITE_FLOAT:
                case raw.SQLITE_INTEGER:
                    return FromJulianDate(GetDouble(ordinal));
                default:
                    return DateTime.Parse(GetString(ordinal), CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="DateTimeOffset" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public DateTimeOffset GetDateTimeOffset(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case raw.SQLITE_FLOAT:
                case raw.SQLITE_INTEGER:
                    return new DateTimeOffset(FromJulianDate(GetDouble(ordinal)));
                default:
                    return DateTimeOffset.Parse(GetString(ordinal), CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="decimal" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override decimal GetDecimal(int ordinal)
            => decimal.Parse(GetString(ordinal), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="double" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override double GetDouble(int ordinal)
            => IsDBNull(ordinal)
                ? throw new InvalidCastException()
                : raw.sqlite3_column_double(_stmt, ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="float" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override float GetFloat(int ordinal)
            => (float)GetDouble(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Guid" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override Guid GetGuid(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case raw.SQLITE_BLOB:
                    var bytes = GetBlob(ordinal);
                    if (bytes.Length == 16)
                    {
                        return new Guid(bytes);
                    }
                    else
                    {
                        return new Guid(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
                    }
                default:
                    return new Guid(GetString(ordinal));
            }
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="short" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override short GetInt16(int ordinal)
            => (short)GetInt64(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="int" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override int GetInt32(int ordinal)
            => (int)GetInt64(ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="long" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override long GetInt64(int ordinal)
            => IsDBNull(ordinal)
                ? throw new InvalidCastException()
                : raw.sqlite3_column_int64(_stmt, ordinal);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="string" />.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override string GetString(int ordinal)
            => IsDBNull(ordinal)
                ? throw new InvalidCastException()
                : raw.sqlite3_column_text(_stmt, ordinal);

        /// <summary>
        /// Reads a stream of bytes from the specified column. Not supported.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which the data is copied.</param>
        /// <param name="bufferOffset">The index to which the data will be copied.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
            => throw new NotSupportedException();

        /// <summary>
        /// Reads a stream of characters from the specified column. Not supported.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <param name="dataOffset">The index from which to begin the read operation.</param>
        /// <param name="buffer">The buffer into which the data is copied.</param>
        /// <param name="bufferOffset">The index to which the data will be copied.</param>
        /// <param name="length">The maximum number of characters to read.</param>
        /// <returns>The actual number of characters read.</returns>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
            => throw new NotSupportedException();

        /// <summary>
        /// Gets the value of the specified column.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
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
                return (T)(object)GetDateTimeOffset(ordinal);
            }
            if (type == typeof(DBNull))
            {
                if (!_stepped || _done)
                {
                    throw new InvalidOperationException(Resources.NoData);
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

        /// <summary>
        /// Gets the value of the specified column.
        /// </summary>
        /// <param name="ordinal">The zero-based column ordinal.</param>
        /// <returns>The value of the column.</returns>
        public override object GetValue(int ordinal)
        {
            if (_closed)
            {
                throw new InvalidOperationException(Resources.DataReaderClosed(nameof(GetValue)));
            }

            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case raw.SQLITE_INTEGER:
                    return GetInt64(ordinal);

                case raw.SQLITE_FLOAT:
                    return GetDouble(ordinal);

                case raw.SQLITE_TEXT:
                    return GetString(ordinal);

                case raw.SQLITE_BLOB:
                    return GetBlob(ordinal);

                case raw.SQLITE_NULL:
                    if (!_stepped || _done)
                    {
                        throw new InvalidOperationException(Resources.NoData);
                    }

                    return DBNull.Value;

                default:
                    Debug.Assert(false, "Unexpected column type: " + sqliteType);
                    return GetInt32(ordinal);
            }
        }

        /// <summary>
        /// Gets the column values of the current row.
        /// </summary>
        /// <param name="values">An array into which the values are copied.</param>
        /// <returns>The number of values copied into the array.</returns>
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
            => IsDBNull(ordinal)
                ? throw new InvalidCastException()
                : raw.sqlite3_column_blob(_stmt, ordinal) ?? _emptyByteArray;

        /// <summary>
        /// Computes DateTime from julian date. This function is a port of the
        /// computeYMD and computeHMS functions from the original Sqlite core
        /// source code in 'date.c'.
        /// </summary>
        /// <param name="julianDate">Real value containing the julian date</param>
        /// <returns>The converted DateTime.</returns>
        private static DateTime FromJulianDate(double julianDate)
        {
            // computeYMD
            var iJD = (long)(julianDate * 86400000.0 + 0.5);
            var Z = (int)((iJD + 43200000) / 86400000);
            var A = (int)((Z - 1867216.25) / 36524.25);
            A = Z + 1 + A - (A / 4);
            var B = A + 1524;
            var C = (int)((B - 122.1) / 365.25);
            var D = (36525 * (C & 32767)) / 100;
            var E = (int)((B - D) / 30.6001);
            var X1 = (int)(30.6001 * E);
            var day = B - D - X1;
            var month = E < 14 ? E - 1 : E - 13;
            var year = month > 2 ? C - 4716 : C - 4715;

            // computeHMS
            var s = (int)((iJD + 43200000) % 86400000);
            var fracSecond = s / 1000.0;
            s = (int)fracSecond;
            fracSecond -= s;
            var hour = s / 3600;
            s -= hour * 3600;
            var minute = s / 60;
            fracSecond += s - minute * 60;

            var second = (int)fracSecond;
            var millisecond = (int)Math.Round((fracSecond - second) * 1000.0);

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }
    }
}
