// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
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
        private readonly SqlServerStringTypeMapping _nvarcharmax
            = new SqlServerStringTypeMapping("nvarchar(max)", dbType: null, unicode: true, size: null);

        private readonly SqlServerStringTypeMapping _nvarchar450
            = new SqlServerStringTypeMapping("nvarchar(450)", dbType: null, unicode: true, size: 450);

        private readonly SqlServerStringTypeMapping _varcharmax
            = new SqlServerStringTypeMapping("varchar(max)", dbType: DbType.AnsiString, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly SqlServerStringTypeMapping _varchar900
            = new SqlServerStringTypeMapping("varchar(900)", dbType: DbType.AnsiString, unicode: false, size: 900, hasNonDefaultUnicode: true);

        private readonly SqlServerByteArrayTypeMapping _varbinarymax
            = new SqlServerByteArrayTypeMapping("varbinary(max)", dbType: DbType.Binary, unicode: false, size: null);

        private readonly SqlServerByteArrayTypeMapping _varbinary900
            = new SqlServerByteArrayTypeMapping("varbinary(900)", dbType: DbType.Binary, unicode: false, size: 900);

        private readonly SqlServerByteArrayTypeMapping _rowversion
            = new SqlServerByteArrayTypeMapping("rowversion", dbType: DbType.Binary, unicode: false, size: 8);

        private readonly IntTypeMapping _int = new IntTypeMapping("int");

        private readonly UIntTypeMapping _uint = new UIntTypeMapping("bigint"); // need bigint to contain C# uint

        private readonly LongTypeMapping _long = new LongTypeMapping("bigint");

        private readonly ULongTypeMapping _ulong = new ULongTypeMapping("decimal(20)");  // need decimal(20) to contain a C# ulong

        private readonly ShortTypeMapping _short = new ShortTypeMapping("smallint");

        private readonly UShortTypeMapping _ushort = new UShortTypeMapping("int"); // need int to contain C# ushort

        private readonly ByteTypeMapping _byte = new ByteTypeMapping("tinyint");

        private readonly SByteTypeMapping _sbyte = new SByteTypeMapping("smallint"); // need smallint to contain C# sbyte

        private readonly BoolTypeMapping _bool = new BoolTypeMapping("bit");

        private readonly SqlServerStringTypeMapping _nchar
            = new SqlServerStringTypeMapping("nchar", dbType: DbType.StringFixedLength, unicode: true, size: null);

        private readonly SqlServerStringTypeMapping _nvarchar
            = new SqlServerStringTypeMapping("nvarchar", dbType: null, unicode: true, size: null);

        private readonly SqlServerStringTypeMapping _char
            = new SqlServerStringTypeMapping("char", dbType: DbType.AnsiStringFixedLength, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly SqlServerStringTypeMapping _varchar
            = new SqlServerStringTypeMapping("varchar", dbType: DbType.AnsiString, unicode: false, size: null, hasNonDefaultUnicode: true);

        private readonly SqlServerByteArrayTypeMapping _varbinary = new SqlServerByteArrayTypeMapping("varbinary");

        private readonly SqlServerByteArrayTypeMapping _binary = new SqlServerByteArrayTypeMapping("binary");

        private readonly SqlServerDateTimeTypeMapping _date = new SqlServerDateTimeTypeMapping("date", dbType: DbType.Date);

        private readonly SqlServerDateTimeTypeMapping _datetime = new SqlServerDateTimeTypeMapping("datetime", dbType: DbType.DateTime);

        private readonly SqlServerDateTimeTypeMapping _datetime2 = new SqlServerDateTimeTypeMapping("datetime2", dbType: DbType.DateTime2);

        private readonly DoubleTypeMapping _double = new DoubleTypeMapping("float"); // Note: "float" is correct SQL Server type to map to CLR-type double

        private readonly SqlServerDateTimeOffsetTypeMapping _datetimeoffset = new SqlServerDateTimeOffsetTypeMapping("datetimeoffset");

        private readonly FloatTypeMapping _real = new FloatTypeMapping("real"); // Note: "real" is correct SQL Server type to map to CLR-type float

        private readonly GuidTypeMapping _uniqueidentifier = new GuidTypeMapping("uniqueidentifier");

        private readonly DecimalTypeMapping _decimal = new DecimalTypeMapping("decimal(18, 2)");

        private readonly TimeSpanTypeMapping _time = new TimeSpanTypeMapping("time");

        private readonly SqlServerStringTypeMapping _xml = new SqlServerStringTypeMapping("xml", dbType: null, unicode: true);

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly HashSet<string> _disallowedMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlServerTypeMapper([NotNull] RelationalTypeMapperDependencies dependencies)
            : base(dependencies)
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", _long },
                    { "binary varying", _varbinary },
                    { "binary", _binary },
                    { "bit", _bool },
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
                    { "smallint", _short },
                    { "smallmoney", _decimal },
                    { "text", _varchar },
                    { "time", _time },
                    { "timestamp", _rowversion },
                    { "tinyint", _byte },
                    { "uniqueidentifier", _uniqueidentifier },
                    { "varbinary", _varbinary },
                    { "varchar", _varchar },
                    { "xml", _xml }
                };

            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(uint), _uint },
                    { typeof(long), _long },
                    { typeof(ulong), _ulong },
                    { typeof(DateTime), _datetime2 },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(bool), _bool },
                    { typeof(byte), _byte },
                    { typeof(sbyte), _sbyte },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(char), new CharTypeMapping("char(1)") },
                    { typeof(short), _short },
                    { typeof(ushort), _ushort },
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
                    _rowversion,
                    size => new SqlServerByteArrayTypeMapping(
                        "varbinary(" + size + ")",
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
                    size => new SqlServerStringTypeMapping(
                        "varchar(" + size + ")",
                        dbType: DbType.AnsiString,
                        unicode: false,
                        size: size,
                        hasNonDefaultUnicode: true,
                        hasNonDefaultSize: true),
                    4000,
                    _nvarcharmax,
                    _nvarcharmax,
                    _nvarchar450,
                    size => new SqlServerStringTypeMapping(
                        "nvarchar(" + size + ")",
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
