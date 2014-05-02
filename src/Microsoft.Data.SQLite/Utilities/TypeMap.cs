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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Microsoft.Data.SQLite.Utilities
{
    // TODO: Make this configurable
    // TODO: Avoid boxing #Perf
    // TODO: Compute and cache lookups #Perf
    internal class TypeMap
    {
        private static readonly ICollection<TypeMap> _typeMaps = new List<TypeMap>();
        private static readonly TypeMap _null = Add<DBNull>(Enumerable.Empty<string>());
        private static readonly TypeMap _integer = Add<long>(new[] { "INTEGER" });
        private static readonly TypeMap _real = Add<double>(new[] { "FLOAT", "REAL" });
        private static readonly TypeMap _text = Add<string>(new[] { "CHAR", "NCHAR", "NVARCHAR", "VARCHAR" });
        private static readonly TypeMap _blob = Add<byte[]>(new[] { "BLOB" });

        private readonly Type _clrType;
        private readonly SQLiteType _sqliteType;
        private readonly Func<object, object> _toInterop;
        private readonly Func<object, object> _fromInterop;
        private readonly IEnumerable<string> _declaredTypes;

        static TypeMap()
        {
            Add((bool b) => b ? 1L : 0L, l => l != 0, new[] { "BIT" });
            Add((byte b) => (long)b, l => (byte)l, new[] { "TINYINT" });
            Add((DateTime d) => d.ToString("o"), DateTime.Parse, new[] { "DATETIME" });
            Add((DateTimeOffset d) => d.ToString("o"), DateTimeOffset.Parse, new[] { "DATETIMEOFFSET" });
            Add((decimal d) => d.ToString(CultureInfo.InvariantCulture), decimal.Parse, new[] { "DECIMAL" });
            Add((float f) => (double)f, d => (float)d, new[] { "SINGLE" });
            Add((Guid g) => g.ToByteArray(), b => new Guid(b), new[] { "UNIQUEIDENTIFIER" });
            Add((int i) => (long)i, l => (int)l, new[] { "INT" });
            Add((sbyte b) => (long)b, l => (sbyte)l, new[] { "INT8" });
            Add((short s) => (long)s, l => (short)l, new[] { "SMALLINT" });
            Add((TimeSpan t) => t.ToString("c"), TimeSpan.Parse, new[] { "INTERVAL" });
            Add((uint i) => (long)i, l => (uint)l, new[] { "UINT" });
            Add((ushort s) => (long)s, l => (ushort)l, new[] { "UINT16" });
            Add((ulong l) => unchecked((long)l), l => unchecked((ulong)l), new[] { "ULONG" });
        }

        private TypeMap(Type clrType, SQLiteType sqliteType, IEnumerable<string> declaredTypes)
        {
            Debug.Assert(clrType != null, "clrType is null.");
            Debug.Assert(declaredTypes != null, "declaredTypes is null.");

            _clrType = clrType;
            _sqliteType = sqliteType;
            _declaredTypes = declaredTypes;
        }

        private TypeMap(
                Type clrType,
                SQLiteType sqliteType,
                Func<object, object> toInterop,
                Func<object, object> fromInterop,
                IEnumerable<string> declaredTypes)
            : this(clrType, sqliteType, declaredTypes)
        {
            Debug.Assert(toInterop != null, "toInterop is null.");
            Debug.Assert(fromInterop != null, "fromInterop is null.");

            _toInterop = toInterop;
            _fromInterop = fromInterop;
        }

        public Type ClrType
        {
            get { return _clrType; }
        }

        public SQLiteType SQLiteType
        {
            get { return _sqliteType; }
        }

        public static TypeMap Add<T>(IEnumerable<string> declaredTypes)
        {
            var map = new TypeMap(
                typeof(T),
                GetSQLiteType<T>(),
                declaredTypes);
            _typeMaps.Add(map);

            return map;
        }

        public static TypeMap Add<T, TInterop>(Func<T, TInterop> toInterop, Func<TInterop, T> fromInterop, IEnumerable<string> declaredTypes)
        {
            Debug.Assert(toInterop != null, "toInterop is null.");
            Debug.Assert(fromInterop != null, "fromInterop is null.");
            Debug.Assert(declaredTypes != null, "declaredTypes is null.");

            var map = new TypeMap(
                typeof(T),
                GetSQLiteType<TInterop>(),
                o => toInterop((T)o),
                o => fromInterop((TInterop)o),
                declaredTypes);
            _typeMaps.Add(map);

            return map;
        }

        private static SQLiteType GetSQLiteType<T>()
        {
            if (typeof(T) == typeof(DBNull))
                return SQLiteType.Null;
            if (typeof(T) == typeof(long))
                return SQLiteType.Integer;
            if (typeof(T) == typeof(double))
                return SQLiteType.Float;
            if (typeof(T) == typeof(string))
                return SQLiteType.Text;

            Debug.Assert(typeof(T) == typeof(byte[]), "T is not byte[]");

            return SQLiteType.Blob;
        }

        public static TypeMap FromClrType<T>()
        {
            return FromClrType(typeof(T));
        }

        public static TypeMap FromClrType(Type type)
        {
            // TODO: Consider derived types
            var map = _typeMaps.FirstOrDefault(m => m._clrType == type);
            if (map == null)
                throw new ArgumentException(Strings.FormatUnknownDataType(type));

            return map;
        }

        public static TypeMap FromDeclaredType(string declaredType, SQLiteType sqliteType)
        {
            TypeMap map = null;
            if (declaredType != null)
            {
                // Strip length, precision & scale
                var i = declaredType.IndexOf('(');
                if (i != -1)
                    declaredType = declaredType.Substring(0, i).TrimEnd();

                map = _typeMaps.FirstOrDefault(m => m._declaredTypes.Contains(declaredType));
            }

            if (map == null)
                map = FromSQLiteType(sqliteType);

            return map;
        }

        public static TypeMap FromSQLiteType(SQLiteType type)
        {
            switch (type)
            {
                case SQLiteType.Null:
                    return _null;

                case SQLiteType.Integer:
                    return _integer;

                case SQLiteType.Float:
                    return _real;

                case SQLiteType.Text:
                    return _text;

                default:
                    Debug.Assert(type == SQLiteType.Blob, "type is not Blob.");
                    return _blob;
            }
        }

        public object ToInterop(object value)
        {
            Debug.Assert(value != null, "value is null.");

            if (_toInterop == null)
                return value;

            return _toInterop(value);
        }

        public object FromInterop(object value)
        {
            Debug.Assert(value != null, "value is null.");

            if (_fromInterop == null)
                return value;

            return _fromInterop(value);
        }
    }
}
