// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class SqlServerTypeMapper : RelationalTypeMapper
    {
        private readonly SqlServerMaxLengthMapping _nvarcharmax = new SqlServerMaxLengthMapping("nvarchar(max)", typeof(string));
        private readonly SqlServerMaxLengthMapping _nvarchar450 = new SqlServerMaxLengthMapping("nvarchar(450)", typeof(string));
        private readonly SqlServerMaxLengthMapping _varcharmax = new SqlServerMaxLengthMapping("varchar(max)", typeof(string), DbType.AnsiString, unicode: false);
        private readonly SqlServerMaxLengthMapping _varchar900 = new SqlServerMaxLengthMapping("varchar(900)", typeof(string), DbType.AnsiString, unicode: false);
        private readonly SqlServerMaxLengthMapping _varbinarymax = new SqlServerMaxLengthMapping("varbinary(max)", typeof(byte[]), DbType.Binary);
        private readonly SqlServerMaxLengthMapping _varbinary900 = new SqlServerMaxLengthMapping("varbinary(900)", typeof(byte[]), DbType.Binary);
        private readonly RelationalSizedTypeMapping _rowversion = new RelationalSizedTypeMapping("rowversion", typeof(byte[]), DbType.Binary, unicode: true, size: 8);
        private readonly RelationalTypeMapping _int = new RelationalTypeMapping("int", typeof(int), DbType.Int32);
        private readonly RelationalTypeMapping _bigint = new RelationalTypeMapping("bigint", typeof(long), DbType.Int64);
        private readonly RelationalTypeMapping _bit = new RelationalTypeMapping("bit", typeof(bool));
        private readonly RelationalTypeMapping _smallint = new RelationalTypeMapping("smallint", typeof(short), DbType.Int16);
        private readonly RelationalTypeMapping _tinyint = new RelationalTypeMapping("tinyint", typeof(byte), DbType.Byte);
        private readonly SqlServerMaxLengthMapping _nchar = new SqlServerMaxLengthMapping("nchar", typeof(string), DbType.StringFixedLength);
        private readonly SqlServerMaxLengthMapping _nvarchar = new SqlServerMaxLengthMapping("nvarchar", typeof(string));
        private readonly SqlServerMaxLengthMapping _char = new SqlServerMaxLengthMapping("char", typeof(string), DbType.AnsiStringFixedLength, unicode: false);
        private readonly SqlServerMaxLengthMapping _varchar = new SqlServerMaxLengthMapping("varchar", typeof(string), DbType.AnsiString, unicode: false);
        private readonly SqlServerMaxLengthMapping _varbinary = new SqlServerMaxLengthMapping("varbinary", typeof(byte[]), DbType.Binary);
        private readonly SqlServerMaxLengthMapping _binary = new SqlServerMaxLengthMapping("binary", typeof(byte[]), DbType.Binary);
        private readonly RelationalTypeMapping _datetime2 = new RelationalTypeMapping("datetime2", typeof(DateTime), DbType.DateTime2);
        private readonly RelationalTypeMapping _double = new RelationalTypeMapping("float", typeof(double));
        private readonly RelationalTypeMapping _datetimeoffset = new RelationalTypeMapping("datetimeoffset", typeof(DateTimeOffset));
        private readonly RelationalTypeMapping _real = new RelationalTypeMapping("real", typeof(float));
        private readonly RelationalTypeMapping _uniqueidentifier = new RelationalTypeMapping("uniqueidentifier", typeof(Guid));
        private readonly RelationalTypeMapping _decimal = new RelationalTypeMapping("decimal(18, 2)", typeof(decimal));
        private readonly RelationalTypeMapping _time = new RelationalTypeMapping("time", typeof(TimeSpan));
        private readonly RelationalTypeMapping _xml = new RelationalTypeMapping("xml", typeof(string));

        private readonly Dictionary<string, RelationalTypeMapping> _simpleNameMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _simpleMappings;
        private readonly HashSet<string> _disallowedMappings;

        public SqlServerTypeMapper()
        {
            _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", _bigint },
                    { "binary varying", _varbinary },
                    { "binary", _binary },
                    { "bit", _bit },
                    { "char varying", _varchar },
                    { "char", _char },
                    { "character varying", _varchar },
                    { "character", _char },
                    { "date", _datetime2 },
                    { "datetime", _datetime2 },
                    { "datetime2", _datetime2 },
                    { "datetimeoffset", _datetimeoffset },
                    { "dec", _decimal },
                    { "decimal", _decimal },
                    { "float", _double },
                    { "image", _varbinary },
                    { "int", _int },
                    { "money", _decimal },
                    { "national char varying", _nvarchar },
                    { "national character varying", _nvarchar },
                    { "national character", _nchar },
                    { "nchar", _nchar },
                    { "ntext", _nvarchar },
                    { "numeric", _decimal },
                    { "nvarchar", _nvarchar },
                    { "real", _real },
                    { "rowversion", _rowversion },
                    { "smalldatetime", _datetime2 },
                    { "smallint", _smallint },
                    { "smallmoney", _decimal },
                    { "text", _varchar },
                    { "time", _time },
                    { "timestamp", _rowversion },
                    { "tinyint", _tinyint },
                    { "uniqueidentifier", _uniqueidentifier },
                    { "varbinary", _varbinary },
                    { "varchar", _varchar },
                    { "xml", _xml }
                };

            _simpleMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _bigint },
                    { typeof(DateTime), _datetime2 },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(bool), _bit },
                    { typeof(byte), _tinyint },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(char), _int },
                    { typeof(short), _smallint },
                    { typeof(float), _real },
                    { typeof(decimal), _decimal },
                    { typeof(TimeSpan), _time }
                };

            // These are disallowed only if specified without any kind of length specified in parenthesis.
            // This is because we don't try to make a new type from this string and any max length value
            // specified in the model, which means use of these strings is almost certainly an error, and
            // if it is not an error, then using, for example, varbinary(1) will work instead.
            _disallowedMappings
                = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "binary varying",
                    "binary",
                    "char varying",
                    "char",
                    "character varying",
                    "character",
                    "national char varying",
                    "national character varying",
                    "national character",
                    "nchar",
                    "nvarchar",
                    "varbinary",
                    "varchar"
                };
        }

        public override void ValidateTypeName(string typeName)
        {
            if (_disallowedMappings.Contains(typeName))
            {
                throw new ArgumentException(SqlServerStrings.UnqualifiedDataType(typeName));
            }
        }

        protected override string GetColumnType(IProperty property) => property.SqlServer().ColumnType;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetSimpleMappings()
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetSimpleNameMappings()
            => _simpleNameMappings;

        public override RelationalTypeMapping FindMapping(Type clrType, bool unicode = true)
        {
            Check.NotNull(clrType, nameof(clrType));

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            return clrType == typeof(string)
                ? (unicode ? _nvarcharmax : _varcharmax)
                : (clrType == typeof(byte[])
                    ? _varbinarymax
                    : base.FindMapping(clrType, unicode));
        }

        protected override RelationalTypeMapping FindCustomMapping(IProperty property, bool unicode = true)
        {
            Check.NotNull(property, nameof(property));

            var clrType = property.ClrType.UnwrapNullableType();

            return clrType == typeof(string)
                ? (unicode
                    ? GetStringMapping(
                        property, 4000,
                        maxLength => new SqlServerMaxLengthMapping("nvarchar(" + maxLength + ")", typeof(string)),
                        _nvarcharmax, _nvarcharmax, _nvarchar450)
                    : GetStringMapping(
                        property, 8000,
                        maxLength => new SqlServerMaxLengthMapping("varchar(" + maxLength + ")", typeof(string), unicode: false),
                        _varcharmax, _varcharmax, _varchar900))
                : clrType == typeof(byte[])
                    ? GetByteArrayMapping(property, 8000,
                        maxLength => new SqlServerMaxLengthMapping("varbinary(" + maxLength + ")", typeof(byte[]), DbType.Binary),
                        _varbinarymax, _varbinarymax, _varbinary900, _rowversion)
                    : null;
        }

        // indexes in SQL Server have a max size of 900 bytes
        protected override bool RequiresKeyMapping(IProperty property)
            => base.RequiresKeyMapping(property) || property.IsIndex();
    }
}
