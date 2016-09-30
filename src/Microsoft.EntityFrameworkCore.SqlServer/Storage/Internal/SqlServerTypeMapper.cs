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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlServerTypeMapper : RelationalTypeMapper
    {
        private readonly SqlServerMaxLengthMapping _nvarcharmax
            = new SqlServerMaxLengthMapping("nvarchar(max)", typeof(string), dbType: null, unicode: true, size: null);

        private readonly SqlServerMaxLengthMapping _nvarchar450
            = new SqlServerMaxLengthMapping("nvarchar(450)", typeof(string), dbType: null, unicode: true, size: 450);

        private readonly SqlServerMaxLengthMapping _varcharmax
            = new SqlServerMaxLengthMapping("varchar(max)", typeof(string), dbType: DbType.AnsiString, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly SqlServerMaxLengthMapping _varchar900
            = new SqlServerMaxLengthMapping("varchar(900)", typeof(string), dbType: DbType.AnsiString, unicode: false, size: 900, hasNonDefaultUnicode: true);

        private readonly SqlServerMaxLengthMapping _varbinarymax
            = new SqlServerMaxLengthMapping("varbinary(max)", typeof(byte[]), dbType: DbType.Binary, unicode: false, size: null);

        private readonly SqlServerMaxLengthMapping _varbinary900
            = new SqlServerMaxLengthMapping("varbinary(900)", typeof(byte[]), dbType: DbType.Binary, unicode: false, size: 900);

        private readonly RelationalTypeMapping _rowversion
            = new RelationalTypeMapping("rowversion", typeof(byte[]), dbType: DbType.Binary, unicode: false, size: 8);

        private readonly RelationalTypeMapping _int
            = new RelationalTypeMapping("int", typeof(int), dbType: DbType.Int32);

        private readonly RelationalTypeMapping _bigint
            = new RelationalTypeMapping("bigint", typeof(long), dbType: DbType.Int64);

        private readonly RelationalTypeMapping _smallint
            = new RelationalTypeMapping("smallint", typeof(short), dbType: DbType.Int16);

        private readonly RelationalTypeMapping _tinyint
            = new RelationalTypeMapping("tinyint", typeof(byte), dbType: DbType.Byte);

        private readonly RelationalTypeMapping _bit
            = new RelationalTypeMapping("bit", typeof(bool));

        private readonly SqlServerMaxLengthMapping _nchar
            = new SqlServerMaxLengthMapping("nchar", typeof(string), dbType: DbType.StringFixedLength, unicode: true, size: null);

        private readonly SqlServerMaxLengthMapping _nvarchar
            = new SqlServerMaxLengthMapping("nvarchar", typeof(string), dbType: null, unicode: true, size: null);

        private readonly SqlServerMaxLengthMapping _char
            = new SqlServerMaxLengthMapping("char", typeof(string), dbType: DbType.AnsiStringFixedLength, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly SqlServerMaxLengthMapping _varchar
            = new SqlServerMaxLengthMapping("varchar", typeof(string), dbType: DbType.AnsiString, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly SqlServerMaxLengthMapping _varbinary
            = new SqlServerMaxLengthMapping("varbinary", typeof(byte[]), dbType: DbType.Binary);

        private readonly SqlServerMaxLengthMapping _binary
            = new SqlServerMaxLengthMapping("binary", typeof(byte[]), dbType: DbType.Binary);

        private readonly RelationalTypeMapping _date
            = new RelationalTypeMapping("date", typeof(DateTime), dbType: DbType.Date);

        private readonly RelationalTypeMapping _datetime
            = new RelationalTypeMapping("datetime", typeof(DateTime), dbType: DbType.DateTime);

        private readonly RelationalTypeMapping _datetime2
            = new RelationalTypeMapping("datetime2", typeof(DateTime), dbType: DbType.DateTime2);

        private readonly RelationalTypeMapping _double
            = new RelationalTypeMapping("float", typeof(double));

        private readonly RelationalTypeMapping _datetimeoffset
            = new RelationalTypeMapping("datetimeoffset", typeof(DateTimeOffset));

        private readonly RelationalTypeMapping _real
            = new RelationalTypeMapping("real", typeof(float));

        private readonly RelationalTypeMapping _uniqueidentifier
            = new RelationalTypeMapping("uniqueidentifier", typeof(Guid));

        private readonly RelationalTypeMapping _decimal
            = new RelationalTypeMapping("decimal(18, 2)", typeof(decimal));

        private readonly RelationalTypeMapping _time
            = new RelationalTypeMapping("time", typeof(TimeSpan));

        private readonly RelationalTypeMapping _xml
            = new SqlServerMaxLengthMapping("xml", typeof(string), dbType: null, unicode: true, size: null);

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly HashSet<string> _disallowedMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerTypeMapper()
        {
            _storeTypeMappings
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
                    { "date", _date },
                    { "datetime", _datetime },
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
                    { "smalldatetime", _datetime },
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

            _clrTypeMappings
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

            ByteArrayMapper
                = new ByteArrayRelationalTypeMapper(
                    8000,
                    _varbinarymax,
                    _varbinarymax,
                    _varbinary900,
                    _rowversion, size => new SqlServerMaxLengthMapping(
                        "varbinary(" + size + ")",
                        typeof(byte[]),
                        DbType.Binary,
                        unicode: false,
                        size: size,
                        hasNonDefaultUnicode: false,
                        hasNonDefaultSize: true));

            StringMapper
                = new StringRelationalTypeMapper(
                    8000,
                    _varcharmax,
                    _varcharmax,
                    _varchar900,
                    size => new SqlServerMaxLengthMapping(
                        "varchar(" + size + ")",
                        typeof(string),
                        dbType: DbType.AnsiString,
                        unicode: false,
                        size: size,
                        hasNonDefaultUnicode: true,
                        hasNonDefaultSize: true),
                    4000,
                    _nvarcharmax,
                    _nvarcharmax,
                    _nvarchar450,
                    size => new SqlServerMaxLengthMapping(
                        "nvarchar(" + size + ")",
                        typeof(string),
                        dbType: null,
                        unicode: true,
                        size: size,
                        hasNonDefaultUnicode: false,
                        hasNonDefaultSize: true));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IStringRelationalTypeMapper StringMapper { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override void ValidateTypeName(string storeType)
        {
            if (_disallowedMappings.Contains(storeType))
            {
                throw new ArgumentException(SqlServerStrings.UnqualifiedDataType(storeType));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GetColumnType(IProperty property) => property.SqlServer().ColumnType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _storeTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            return clrType == typeof(string)
                ? _nvarcharmax
                : (clrType == typeof(byte[])
                    ? _varbinarymax
                    : base.FindMapping(clrType));
        }

        // Indexes in SQL Server have a max size of 900 bytes
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool RequiresKeyMapping(IProperty property)
            => base.RequiresKeyMapping(property) || property.IsIndex();
    }
}
