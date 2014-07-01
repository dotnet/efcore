// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.SQLite.Utilities
{
    // TODO: Make this more configurable
    // TODO: Avoid boxing #Perf
    // TODO: Compute and cache lookups #Perf
    public class SQLiteTypeMap
    {
        private static readonly ICollection<SQLiteTypeMap> _typeMaps = new List<SQLiteTypeMap>();
        private static readonly SQLiteTypeMap _null = Add<DBNull>(Enumerable.Empty<string>(), 0);
        private static readonly SQLiteTypeMap _integer = Add<long>(new[] { "INTEGER" }, DbType.Int64);
        private static readonly SQLiteTypeMap _real = Add<double>(new[] { "FLOAT", "REAL" }, DbType.Double);
        private static readonly SQLiteTypeMap _text = Add<string>(new[] { "CHAR", "NCHAR", "NVARCHAR", "VARCHAR" }, DbType.String);
        private static readonly SQLiteTypeMap _blob = Add<byte[]>(new[] { "BLOB" }, DbType.Binary);

        private readonly Type _clrType;
        private readonly SQLiteType _sqliteType;
        private readonly Func<object, object> _toInterop;
        private readonly Func<object, object> _fromInterop;
        private readonly IEnumerable<string> _declaredTypes;
        private readonly DbType _dbType;

        static SQLiteTypeMap()
        {
            Add((bool b) => b ? 1L : 0L, l => l != 0, new[] { "BIT" }, DbType.Boolean);
            Add((byte b) => (long)b, l => (byte)l, new[] { "TINYINT" }, DbType.Byte);
            Add((DateTime d) => d.ToString("o"), DateTime.Parse, new[] { "DATETIME" }, DbType.DateTime);
            Add((DateTimeOffset d) => d.ToString("o"), DateTimeOffset.Parse, new[] { "DATETIMEOFFSET" }, DbType.DateTimeOffset);
            Add((decimal d) => d.ToString(CultureInfo.InvariantCulture), decimal.Parse, new[] { "DECIMAL" }, DbType.Decimal);
            Add((float f) => (double)f, d => (float)d, new[] { "SINGLE" }, DbType.Single);
            Add((Guid g) => g.ToByteArray(), b => new Guid(b), new[] { "UNIQUEIDENTIFIER" }, DbType.Guid);
            Add((int i) => (long)i, l => (int)l, new[] { "INT" }, DbType.Int32);
            Add((sbyte b) => (long)b, l => (sbyte)l, new[] { "INT8" }, DbType.SByte);
            Add((short s) => (long)s, l => (short)l, new[] { "SMALLINT" }, DbType.Int16);
            Add((TimeSpan t) => t.ToString("c"), TimeSpan.Parse, new[] { "INTERVAL" }, DbType.Time);
            Add((uint i) => (long)i, l => (uint)l, new[] { "UINT" }, DbType.UInt32);
            Add((ushort s) => (long)s, l => (ushort)l, new[] { "UINT16" }, DbType.UInt16);
            Add((ulong l) => unchecked((long)l), l => unchecked((ulong)l), new[] { "ULONG" }, DbType.UInt64);
        }

        private SQLiteTypeMap(Type clrType, SQLiteType sqliteType, IEnumerable<string> declaredTypes, DbType dbType)
        {
            Debug.Assert(clrType != null, "clrType is null.");
            Debug.Assert(declaredTypes != null, "declaredTypes is null.");

            _clrType = clrType;
            _sqliteType = sqliteType;
            _declaredTypes = declaredTypes;
            _dbType = dbType;
        }

        private SQLiteTypeMap(
            Type clrType,
            SQLiteType sqliteType,
            Func<object, object> toInterop,
            Func<object, object> fromInterop,
            IEnumerable<string> declaredTypes,
            DbType dbType)
            : this(clrType, sqliteType, declaredTypes, dbType)
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

        public IEnumerable<string> DeclaredTypes
        {
            get { return _declaredTypes; }
        }

        public DbType DbType
        {
            get { return _dbType; }
        }

        public static SQLiteTypeMap Add<T>(IEnumerable<string> declaredTypes, DbType dbType)
        {
            var map = new SQLiteTypeMap(
                typeof(T),
                GetSQLiteType<T>(),
                declaredTypes,
                dbType);
            _typeMaps.Add(map);

            return map;
        }

        public static SQLiteTypeMap Add<T, TInterop>(Func<T, TInterop> toInterop, Func<TInterop, T> fromInterop, IEnumerable<string> declaredTypes, DbType dbType)
        {
            Debug.Assert(toInterop != null, "toInterop is null.");
            Debug.Assert(fromInterop != null, "fromInterop is null.");
            Debug.Assert(declaredTypes != null, "declaredTypes is null.");

            var map = new SQLiteTypeMap(
                typeof(T),
                GetSQLiteType<TInterop>(),
                o => toInterop((T)o),
                o => fromInterop((TInterop)o),
                declaredTypes,
                dbType);
            _typeMaps.Add(map);

            return map;
        }

        private static SQLiteType GetSQLiteType<T>()
        {
            if (typeof(T) == typeof(DBNull))
            {
                return SQLiteType.Null;
            }
            if (typeof(T) == typeof(long))
            {
                return SQLiteType.Integer;
            }
            if (typeof(T) == typeof(double))
            {
                return SQLiteType.Float;
            }
            if (typeof(T) == typeof(string))
            {
                return SQLiteType.Text;
            }

            Debug.Assert(typeof(T) == typeof(byte[]), "T is not byte[]");

            return SQLiteType.Blob;
        }

        public static SQLiteTypeMap FromClrType<T>()
        {
            return FromClrType(typeof(T));
        }

        public static SQLiteTypeMap FromClrType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.GetTypeInfo().IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            // TODO: Consider derived types
            var map = _typeMaps.FirstOrDefault(m => m._clrType == type);
            if (map == null)
            {
                throw new ArgumentException(Strings.FormatUnknownDataType(type));
            }

            return map;
        }

        public static SQLiteTypeMap FromDeclaredType(string declaredType, SQLiteType sqliteType)
        {
            SQLiteTypeMap map = null;
            if (declaredType != null)
            {
                // Strip length, precision & scale
                var i = declaredType.IndexOf('(');
                if (i != -1)
                {
                    declaredType = declaredType.Substring(0, i).TrimEnd();
                }

                map = _typeMaps.FirstOrDefault(m => m._declaredTypes.Contains(declaredType));
            }

            if (map == null)
            {
                map = FromSQLiteType(sqliteType);
            }

            return map;
        }

        public static SQLiteTypeMap FromSQLiteType(SQLiteType type)
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
            {
                return value;
            }

            return _toInterop(value);
        }

        public object FromInterop(object value)
        {
            Debug.Assert(value != null, "value is null.");

            if (_fromInterop == null)
            {
                return value;
            }

            return _fromInterop(value);
        }
    }
}
