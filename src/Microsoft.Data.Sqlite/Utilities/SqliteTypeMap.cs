// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Data.Sqlite.Utilities
{
    // TODO: Make this more configurable
    // TODO: Avoid boxing #Perf
    // TODO: Compute and cache lookups #Perf
    public class SqliteTypeMap
    {
        private static readonly ICollection<SqliteTypeMap> _typeMaps = new List<SqliteTypeMap>();
        private static readonly SqliteTypeMap _null = Add<DBNull>(Enumerable.Empty<string>(), 0);
        private static readonly SqliteTypeMap _integer = Add<long>(new[] { "INTEGER" }, DbType.Int64);
        private static readonly SqliteTypeMap _real = Add<double>(new[] { "FLOAT", "REAL" }, DbType.Double);
        private static readonly SqliteTypeMap _text = Add<string>(new[] { "CHAR", "NCHAR", "NVARCHAR", "VARCHAR" }, DbType.String);
        private static readonly SqliteTypeMap _blob = Add<byte[]>(new[] { "BLOB" }, DbType.Binary);

        private readonly Type _clrType;
        private readonly SqliteType _sqliteType;
        private readonly Func<object, object> _toInterop;
        private readonly Func<object, object> _fromInterop;
        private readonly IEnumerable<string> _declaredTypes;
        private readonly DbType _dbType;

        static SqliteTypeMap()
        {
            Add((bool b) => b ? 1L : 0L, l => l != 0, new[] { "BIT" }, DbType.Boolean);
            Add((byte b) => (long)b, l => (byte)l, new[] { "TINYINT" }, DbType.Byte);
            Add((DateTime d) => d.ToString("o"), DateTime.Parse, new[] { "DATETIME" }, DbType.DateTime);
            Add((DateTimeOffset d) => d.ToString("o"), DateTimeOffset.Parse, new[] { "DATETIMEOFFSET" }, DbType.DateTimeOffset);
            Add((decimal d) => d.ToString(CultureInfo.InvariantCulture), d => decimal.Parse(d, CultureInfo.InvariantCulture), new[] { "DECIMAL" }, DbType.Decimal);
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

        private SqliteTypeMap(Type clrType, SqliteType sqliteType, IEnumerable<string> declaredTypes, DbType dbType)
        {
            Debug.Assert(clrType != null, "clrType is null.");
            Debug.Assert(declaredTypes != null, "declaredTypes is null.");

            _clrType = clrType;
            _sqliteType = sqliteType;
            _declaredTypes = declaredTypes;
            _dbType = dbType;
        }

        private SqliteTypeMap(
            Type clrType,
            SqliteType sqliteType,
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

        public SqliteType SqliteType
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

        public static SqliteTypeMap Add<T>(IEnumerable<string> declaredTypes, DbType dbType)
        {
            var map = new SqliteTypeMap(
                typeof(T),
                GetSqliteType<T>(),
                declaredTypes,
                dbType);
            _typeMaps.Add(map);

            return map;
        }

        public static SqliteTypeMap Add<T, TInterop>(Func<T, TInterop> toInterop, Func<TInterop, T> fromInterop, IEnumerable<string> declaredTypes, DbType dbType)
        {
            Debug.Assert(toInterop != null, "toInterop is null.");
            Debug.Assert(fromInterop != null, "fromInterop is null.");
            Debug.Assert(declaredTypes != null, "declaredTypes is null.");

            var map = new SqliteTypeMap(
                typeof(T),
                GetSqliteType<TInterop>(),
                o => toInterop((T)o),
                o => fromInterop((TInterop)o),
                declaredTypes,
                dbType);
            _typeMaps.Add(map);

            return map;
        }

        private static SqliteType GetSqliteType<T>()
        {
            if (typeof(T) == typeof(DBNull))
            {
                return SqliteType.Null;
            }
            if (typeof(T) == typeof(long))
            {
                return SqliteType.Integer;
            }
            if (typeof(T) == typeof(double))
            {
                return SqliteType.Float;
            }
            if (typeof(T) == typeof(string))
            {
                return SqliteType.Text;
            }

            Debug.Assert(typeof(T) == typeof(byte[]), "T is not byte[]");

            return SqliteType.Blob;
        }

        public static SqliteTypeMap FromClrType<T>()
        {
            return FromClrType(typeof(T));
        }

        public static SqliteTypeMap FromClrType(Type type)
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

        public static SqliteTypeMap FromDeclaredType(string declaredType, SqliteType sqliteType)
        {
            SqliteTypeMap map = null;
            if (declaredType != null)
            {
                // Strip length, precision & scale
                var i = declaredType.IndexOf('(');
                if (i != -1)
                {
                    declaredType = declaredType.Substring(0, i).TrimEnd();
                }

                map = _typeMaps.FirstOrDefault(
                    m => m._declaredTypes.Contains(declaredType, StringComparer.OrdinalIgnoreCase));
            }

            if (map == null)
            {
                map = FromSqliteType(sqliteType);
            }

            return map;
        }

        public static SqliteTypeMap FromSqliteType(SqliteType type)
        {
            switch (type)
            {
                case SqliteType.Null:
                    return _null;

                case SqliteType.Integer:
                    return _integer;

                case SqliteType.Float:
                    return _real;

                case SqliteType.Text:
                    return _text;

                default:
                    Debug.Assert(type == SqliteType.Blob, "type is not Blob.");
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
