// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.Data.SQLite.Interop;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.SQLite
{
    public class SQLiteDataReader : DbDataReader
    {
        private readonly SQLiteCommand _command;
        private bool _closed;

        // TODO: Step once
        internal SQLiteDataReader(SQLiteCommand command)
        {
            Debug.Assert(command != null, "command is null.");

            _command = command;
        }

        public override int Depth
        {
            get { return 0; }
        }

        public override int FieldCount
        {
            get
            {
                CheckClosed("FieldCount");

                return NativeMethods.sqlite3_column_count(_command.Handle);
            }
        }

        public override bool HasRows
        {
            get
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        public override bool IsClosed
        {
            get { return _closed; }
        }

        public override int RecordsAffected
        {
            get
            {
                Debug.Assert(
                    _command.Connection != null
                        && _command.Connection.Handle != null
                        && !_command.Connection.Handle.IsInvalid,
                    "_command.Connection.Handle is null.");

                return NativeMethods.sqlite3_changes(_command.Connection.Handle);
            }
        }

        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        public override object this[int ordinal]
        {
            get { return GetValue(ordinal); }
        }

        public override IEnumerator GetEnumerator()
        {
            // TODO
            throw new NotImplementedException();
        }

        public override bool Read()
        {
            CheckClosed("Read");

            Debug.Assert(_command.Handle != null && !_command.Handle.IsInvalid, "_command.Handle is null.");
            var rc = NativeMethods.sqlite3_step(_command.Handle);
            if (rc == Constants.SQLITE_DONE)
                return false;
            if (rc != Constants.SQLITE_ROW)
                MarshalEx.ThrowExceptionForRC(rc);

            return true;
        }

        public override bool NextResult()
        {
            return false;
        }

        public override void Close()
        {
            if (_closed)
                return;

            Debug.Assert(_command.OpenReader == this, "_command.ActiveReader is not this.");

            if (_command.Handle != null && !_command.Handle.IsInvalid)
            {
                var rc = NativeMethods.sqlite3_reset(_command.Handle);
                MarshalEx.ThrowExceptionForRC(rc);
            }

            _command.OpenReader = null;
            _closed = true;
        }

        public override string GetName(int ordinal)
        {
            CheckClosed("GetName");

            // TODO: Cache results #Perf
            return NativeMethods.sqlite3_column_name(_command.Handle, ordinal);
        }

        public override int GetOrdinal(string name)
        {
            CheckClosed("GetOrdinal");

            for (var i = 0; i < FieldCount; i++)
                if (GetName(i) == name)
                    return i;

            throw new IndexOutOfRangeException(name);
        }

        public override string GetDataTypeName(int ordinal)
        {
            CheckClosed("GetDataTypeName");

            return NativeMethods.sqlite3_column_decltype(_command.Handle, ordinal);
        }

        public override Type GetFieldType(int ordinal)
        {
            CheckClosed("GetFieldType");

            return GetTypeMap(ordinal).ClrType;
        }

        private SQLiteTypeMap GetTypeMap(int ordinal)
        {
            return SQLiteTypeMap.FromDeclaredType(GetDataTypeName(ordinal), GetSQLiteType(ordinal));
        }

        private SQLiteType GetSQLiteType(int ordinal)
        {
            Debug.Assert(!_closed, "_closed is true.");

            return (SQLiteType)NativeMethods.sqlite3_column_type(_command.Handle, ordinal);
        }

        public override bool IsDBNull(int ordinal)
        {
            CheckClosed("IsDBNull");

            return GetSQLiteType(ordinal) == SQLiteType.Null;
        }

        public override bool GetBoolean(int ordinal)
        {
            CheckClosed("GetBoolean");

            return GetFieldValue<bool>(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            CheckClosed("GetByte");

            return GetFieldValue<byte>(ordinal);
        }

        public override char GetChar(int ordinal)
        {
            CheckClosed("GetChar");

            return GetFieldValue<char>(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            CheckClosed("GetDateTime");

            return GetFieldValue<DateTime>(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            CheckClosed("GetDecimal");

            return GetFieldValue<decimal>(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            CheckClosed("GetDouble");

            return GetFieldValue<double>(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            CheckClosed("GetFloat");

            return GetFieldValue<float>(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            CheckClosed("GetGuid");

            return GetFieldValue<Guid>(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            CheckClosed("GetInt16");

            return GetFieldValue<short>(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            CheckClosed("GetInt32");

            return GetFieldValue<int>(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            CheckClosed("GetInt64");

            return GetFieldValue<long>(ordinal);
        }

        public override string GetString(int ordinal)
        {
            CheckClosed("GetString");

            return GetFieldValue<string>(ordinal);
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
            CheckClosed("GetFieldValue");

            var map = SQLiteTypeMap.FromClrType<T>();
            var value = ColumnReader.Read(map.SQLiteType, _command.Handle, ordinal);

            return (T)map.FromInterop(value);
        }

        public override object GetValue(int ordinal)
        {
            CheckClosed("GetValue");

            var map = GetTypeMap(ordinal);
            var value = ColumnReader.Read(map.SQLiteType, _command.Handle, ordinal);

            return map.FromInterop(value);
        }

        public override int GetValues(object[] values)
        {
            CheckClosed("GetValues");

            for (var i = 0; i < FieldCount; i++)
                values[i] = GetValue(i);

            return FieldCount;
        }

        private void CheckClosed(string operation)
        {
            if (_closed)
                throw new InvalidOperationException(Strings.FormatDataReaderClosed(operation));
        }
    }
}
