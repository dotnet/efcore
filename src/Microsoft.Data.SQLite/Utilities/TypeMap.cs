// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Data.SQLite.Utilities
{
    internal class TypeMap
    {
        private static readonly TypeMap _bool = Create(o => (bool)o ? 1L : 0L);
        private static readonly TypeMap _byte = Create(o => (long)(byte)o);
        private static readonly TypeMap _byteArray = Create(o => (byte[])o);
        private static readonly TypeMap _dbNull = Create(o => DBNull.Value);
        private static readonly TypeMap _sbyte = Create(o => (long)(sbyte)o);
        private static readonly TypeMap _char = Create(o => ((char)o).ToString());
        private static readonly TypeMap _dateTime = Create(o => ((DateTime)o).ToString("o"));
        private static readonly TypeMap _dateTimeOffset = Create(o => ((DateTimeOffset)o).ToString("o"));
        private static readonly TypeMap _decimal = Create(o => ((decimal)o).ToString(CultureInfo.InvariantCulture));
        private static readonly TypeMap _double = Create(o => (double)o);
        private static readonly TypeMap _float = Create(o => (double)(float)o);
        private static readonly TypeMap _guid = Create(o => ((Guid)o).ToByteArray());
        private static readonly TypeMap _int = Create(o => (long)(int)o);
        private static readonly TypeMap _uint = Create(o => (long)(uint)o);
        private static readonly TypeMap _long = Create(o => (long)o);
        private static readonly TypeMap _ulong = Create(o => unchecked((long)(ulong)o));
        private static readonly TypeMap _short = Create(o => (long)(short)o);
        private static readonly TypeMap _timeSpan = Create(o => ((TimeSpan)o).ToString("c"));
        private static readonly TypeMap _ushort = Create(o => (long)(ushort)o);
        private static readonly TypeMap _string = Create(o => (string)o);

        private readonly SQLiteType _sqliteType;
        private readonly Func<object, object> _toInterop;

        private TypeMap(SQLiteType sqliteType, Func<object, object> toInterop)
        {
            Debug.Assert(toInterop != null, "toInterop is null.");

            _sqliteType = sqliteType;
            _toInterop = toInterop;
        }

        public SQLiteType SQLiteType
        {
            get { return _sqliteType; }
        }

        private static TypeMap Create<TInterop>(Func<object, TInterop> toInterop)
        {
            Debug.Assert(toInterop != null, "toInterop is null.");

            SQLiteType sqliteType = 0;
            if (typeof(TInterop) == typeof(long))
                sqliteType = SQLiteType.Integer;
            else if (typeof(TInterop) == typeof(double))
                sqliteType = SQLiteType.Float;
            else if (typeof(TInterop) == typeof(string))
                sqliteType = SQLiteType.Text;
            else if (typeof(TInterop) == typeof(byte[]))
                sqliteType = SQLiteType.Blob;
            else if (typeof(TInterop) == typeof(DBNull))
                sqliteType = SQLiteType.Null;
            else
                Debug.Fail("Unexpected type.");

            return new TypeMap(sqliteType, o => toInterop(o));
        }

        public static TypeMap FromClrType(object value)
        {
            Debug.Assert(value != null, "value is null.");

            if (value is bool)
                return _bool;
            if (value is byte)
                return _byte;
            if (value is byte[])
                return _byteArray;
            if (value is DBNull)
                return _dbNull;
            if (value is sbyte)
                return _sbyte;
            if (value is char)
                return _char;
            if (value is DateTime)
                return _dateTime;
            if (value is DateTimeOffset)
                return _dateTimeOffset;
            if (value is decimal)
                return _decimal;
            if (value is double)
                return _double;
            if (value is float)
                return _float;
            if (value is Guid)
                return _guid;
            if (value is int)
                return _int;
            if (value is uint)
                return _uint;
            if (value is long)
                return _long;
            if (value is ulong)
                return _ulong;
            if (value is short)
                return _short;
            if (value is TimeSpan)
                return _timeSpan;
            if (value is ushort)
                return _ushort;
            if (value is string)
                return _string;

            throw new ArgumentException(Strings.FormatUnknownDataType(value.GetType()));
        }

        public object ToInterop(object value)
        {
            Debug.Assert(value != null, "value is null.");

            // TODO: Avoid boxing the result #Perf
            return _toInterop(value);
        }
    }
}
