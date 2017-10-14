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
        private readonly SqlServerStringTypeMapping _unboundedUnicodeString
            = new SqlServerStringTypeMapping("nvarchar(max)", dbType: null, unicode: true);

        private readonly SqlServerStringTypeMapping _keyUnicodeString
            = new SqlServerStringTypeMapping("nvarchar(450)", dbType: null, unicode: true, size: 450);

        private readonly SqlServerStringTypeMapping _unboundedAnsiString
            = new SqlServerStringTypeMapping("varchar(max)", dbType: DbType.AnsiString);

        private readonly SqlServerStringTypeMapping _keyAnsiString
            = new SqlServerStringTypeMapping("varchar(900)", dbType: DbType.AnsiString, unicode: false, size: 900);

        private readonly SqlServerByteArrayTypeMapping _unboundedBinary
            = new SqlServerByteArrayTypeMapping("varbinary(max)");

        private readonly SqlServerByteArrayTypeMapping _keyBinary
            = new SqlServerByteArrayTypeMapping("varbinary(900)", dbType: DbType.Binary, size: 900);

        private readonly SqlServerByteArrayTypeMapping _rowversion
            = new SqlServerByteArrayTypeMapping("rowversion", dbType: DbType.Binary, size: 8);

        private readonly IntTypeMapping _int = new IntTypeMapping("int", DbType.Int32);

        private readonly LongTypeMapping _long = new LongTypeMapping("bigint", DbType.Int64);

        private readonly ShortTypeMapping _short = new ShortTypeMapping("smallint", DbType.Int16);

        private readonly ByteTypeMapping _byte = new ByteTypeMapping("tinyint", DbType.Byte);

        private readonly StringTypeMapping _unicodeFixedChar
            = new StringTypeMapping(
                "nchar(1)", 
                new ValueConverter<char, string>(
                    v => v.ToString(), 
                    v => v != null && v.Length >= 1 ? v[0] : (char)0),
                unicode: true);

        private readonly StringTypeMapping _unicodeVarChar
            = new StringTypeMapping(
                "nvarchar(1)",
                new ValueConverter<char, string>(
                    v => v.ToString(),
                    v => v != null && v.Length >= 1 ? v[0] : (char)0),
                unicode: true);

        private readonly StringTypeMapping _ansiFixedChar
            = new StringTypeMapping(
                "char(1)",
                new ValueConverter<char, string>(
                    v => v.ToString(),
                    v => v != null && v.Length >= 1 ? v[0] : (char)0),
                DbType.AnsiString);

        private readonly StringTypeMapping _ansiVarChar
            = new StringTypeMapping(
                "varchar(1)",
                new ValueConverter<char, string>(
                    v => v.ToString(),
                    v => v != null && v.Length >= 1 ? v[0] : (char)0),
                DbType.AnsiString);

        private readonly IntTypeMapping _intChar
            = new IntTypeMapping(
                "int",
                new ValueConverter<char, int>(v => v, v => (char)v),
                DbType.Int32);

        private readonly UIntTypeMapping _uint = new UIntTypeMapping(
            "int",
            new ValueConverter<uint, int>(v => (int)v, v => (uint)v),
            DbType.Int32);

        private readonly ULongTypeMapping _ulong = new ULongTypeMapping(
            "bigint",
            new ValueConverter<ulong, long>(v => (long)v, v => (ulong)v),
            DbType.Int64);

        private readonly UShortTypeMapping _ushort = new UShortTypeMapping(
            "smallint",
            new ValueConverter<ushort, short>(v => (short)v, v => (ushort)v),
            DbType.Int16);

        private readonly SByteTypeMapping _sbyte = new SByteTypeMapping(
            "tinyint",
            new ValueConverter<sbyte, byte>(v => (byte)v, v => (sbyte)v),
            DbType.Byte);

        private readonly BoolTypeMapping _bool = new BoolTypeMapping("bit");

        private readonly SqlServerStringTypeMapping _fixedLengthUnicodeString
            = new SqlServerStringTypeMapping("nchar", dbType: DbType.String, unicode: true);

        private readonly SqlServerStringTypeMapping _variableLengthUnicodeString
            = new SqlServerStringTypeMapping("nvarchar", dbType: null, unicode: true);

        private readonly SqlServerStringTypeMapping _fixedLengthAnsiString
            = new SqlServerStringTypeMapping("char", dbType: DbType.AnsiString);

        private readonly SqlServerStringTypeMapping _variableLengthAnsiString
            = new SqlServerStringTypeMapping("varchar", dbType: DbType.AnsiString);

        private readonly SqlServerByteArrayTypeMapping _variableLengthBinary = new SqlServerByteArrayTypeMapping("varbinary");

        private readonly SqlServerByteArrayTypeMapping _fixedLengthBinary = new SqlServerByteArrayTypeMapping("binary");

        private readonly SqlServerDateTimeTypeMapping _date = new SqlServerDateTimeTypeMapping("date", dbType: DbType.Date);

        private readonly SqlServerDateTimeTypeMapping _datetime = new SqlServerDateTimeTypeMapping("datetime", dbType: DbType.DateTime);

        private readonly SqlServerDateTimeTypeMapping _datetime2 = new SqlServerDateTimeTypeMapping("datetime2", dbType: DbType.DateTime2);

        private readonly DoubleTypeMapping _double = new SqlServerDoubleTypeMapping("float"); // Note: "float" is correct SQL Server type to map to CLR-type double

        private readonly SqlServerDateTimeOffsetTypeMapping _datetimeoffset = new SqlServerDateTimeOffsetTypeMapping("datetimeoffset");

        private readonly FloatTypeMapping _real = new SqlServerFloatTypeMapping("real"); // Note: "real" is correct SQL Server type to map to CLR-type float

        private readonly GuidTypeMapping _uniqueidentifier = new GuidTypeMapping("uniqueidentifier", DbType.Guid);

        private readonly DecimalTypeMapping _decimal = new DecimalTypeMapping("decimal(18, 2)");

        private readonly TimeSpanTypeMapping _time = new SqlServerTimeSpanTypeMapping("time");

        private readonly SqlServerStringTypeMapping _xml = new SqlServerStringTypeMapping("xml", dbType: null, unicode: true);

        private readonly UIntTypeMapping _uintToBigint = new UIntTypeMapping(
            "bigint",
            new ValueConverter<uint, long>(v => (long)v, v => (uint)v),
            DbType.Int64);

        private readonly UShortTypeMapping _ushortToInt = new UShortTypeMapping(
            "int",
            new ValueConverter<ushort, int>(v => v, v => (ushort)v),
            DbType.Int32);

        private readonly SByteTypeMapping _sbyteToSmallint = new SByteTypeMapping(
            "smallint",
            new ValueConverter<sbyte, short>(v => v, v => (sbyte)v),
            DbType.Int16);

        private readonly ULongTypeMapping _ulongToDecimal = new ULongTypeMapping(
            "decimal(20, 0)",
            new ValueConverter<ulong, decimal>(v => v, v => (ulong)v),
            DbType.Decimal);

        private readonly Dictionary<string, IList<RelationalTypeMapping>> _storeTypeMappings;
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
                = new Dictionary<string, IList<RelationalTypeMapping>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", new List<RelationalTypeMapping> { _long, _uintToBigint, _ulong } },
                    { "binary varying", new List<RelationalTypeMapping> { _variableLengthBinary } },
                    { "binary", new List<RelationalTypeMapping> { _fixedLengthBinary } },
                    { "bit", new List<RelationalTypeMapping> { _bool } },
                    { "char varying", new List<RelationalTypeMapping> { _variableLengthAnsiString, _ansiVarChar } },
                    { "char", new List<RelationalTypeMapping> { _fixedLengthAnsiString, _ansiFixedChar } },
                    { "character varying", new List<RelationalTypeMapping> { _variableLengthAnsiString, _ansiVarChar } },
                    { "character", new List<RelationalTypeMapping> { _fixedLengthAnsiString, _ansiFixedChar } },
                    { "date", new List<RelationalTypeMapping> { _date } },
                    { "datetime", new List<RelationalTypeMapping> { _datetime } },
                    { "datetime2", new List<RelationalTypeMapping> { _datetime2 } },
                    { "datetimeoffset", new List<RelationalTypeMapping> { _datetimeoffset } },
                    { "dec", new List<RelationalTypeMapping> { _decimal, _ulongToDecimal } },
                    { "decimal", new List<RelationalTypeMapping> { _decimal, _ulongToDecimal } },
                    { "float", new List<RelationalTypeMapping> { _double } },
                    { "image", new List<RelationalTypeMapping> { _variableLengthBinary } },
                    { "int", new List<RelationalTypeMapping> { _int, _ushortToInt, _uint, _intChar } },
                    { "money", new List<RelationalTypeMapping> { _decimal } },
                    { "national char varying", new List<RelationalTypeMapping> { _variableLengthUnicodeString, _unicodeVarChar } },
                    { "national character varying", new List<RelationalTypeMapping> { _variableLengthUnicodeString, _unicodeVarChar } },
                    { "national character", new List<RelationalTypeMapping> { _fixedLengthUnicodeString, _unicodeFixedChar } },
                    { "nchar", new List<RelationalTypeMapping> { _fixedLengthUnicodeString, _unicodeFixedChar } },
                    { "ntext", new List<RelationalTypeMapping> { _variableLengthUnicodeString, _unicodeVarChar } },
                    { "numeric", new List<RelationalTypeMapping> { _decimal, _ulongToDecimal } },
                    { "nvarchar", new List<RelationalTypeMapping> { _variableLengthUnicodeString, _unicodeVarChar } },
                    { "real", new List<RelationalTypeMapping> { _real } },
                    { "rowversion", new List<RelationalTypeMapping> { _rowversion } },
                    { "smalldatetime", new List<RelationalTypeMapping> { _datetime } },
                    { "smallint", new List<RelationalTypeMapping> { _short, _sbyteToSmallint, _ushort } },
                    { "smallmoney", new List<RelationalTypeMapping> { _decimal } },
                    { "text", new List<RelationalTypeMapping> { _variableLengthAnsiString, _ansiVarChar } },
                    { "time", new List<RelationalTypeMapping> { _time } },
                    { "timestamp", new List<RelationalTypeMapping> { _rowversion } },
                    { "tinyint", new List<RelationalTypeMapping> { _byte, _sbyte } },
                    { "uniqueidentifier", new List<RelationalTypeMapping> { _uniqueidentifier } },
                    { "varbinary", new List<RelationalTypeMapping> { _variableLengthBinary } },
                    { "varchar", new List<RelationalTypeMapping> { _variableLengthAnsiString, _ansiVarChar } },
                    { "xml", new List<RelationalTypeMapping> { _xml } }
                };

            // Note: sbyte, ushort, uint, char and ulong type mappings are not supported by SQL Server.
            // We would need the type conversions feature to allow this to work - see https://github.com/aspnet/EntityFramework/issues/242.
            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(uint), _uintToBigint },
                    { typeof(ulong), _ulongToDecimal },
                    { typeof(DateTime), _datetime2 },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(bool), _bool },
                    { typeof(byte), _byte },
                    { typeof(sbyte), _sbyteToSmallint },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(short), _short },
                    { typeof(ushort), _ushortToInt },
                    { typeof(float), _real },
                    { typeof(decimal), _decimal },
                    { typeof(TimeSpan), _time },
                    { typeof(char), _unicodeFixedChar },
                };

            // These are disallowed only if specified without any kind of length specified in parenthesis.
            // This is because we don't try to make a new type from this string and any max length value
            // specified in the model, which means use of these strings is almost certainly an error, and
            // if it is not an error, then using, for example, varbinary(1) will work instead.
            _disallowedMappings
                = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    // binary
                    "binary",

                    // varbinary
                    "binary varying",
                    "varbinary",

                    // char
                    "char",
                    "character",

                    // varchar
                    "char varying",
                    "character varying",
                    "varchar",

                    // nchar
                    "national char",
                    "national character",
                    "nchar",

                    // nvarchar
                    "national char varying",
                    "national character varying",
                    "nvarchar"
                };

            ByteArrayMapper
                = new ByteArrayRelationalTypeMapper(
                    maxBoundedLength: 8000,
                    defaultMapping: _unboundedBinary,
                    unboundedMapping: _unboundedBinary,
                    keyMapping: _keyBinary,
                    rowVersionMapping: _rowversion,
                    createBoundedMapping: size => new SqlServerByteArrayTypeMapping(
                        "varbinary(" + size + ")",
                        DbType.Binary,
                        size));

            StringMapper
                = new StringRelationalTypeMapper(
                    maxBoundedAnsiLength: 8000,
                    defaultAnsiMapping: _unboundedAnsiString,
                    unboundedAnsiMapping: _unboundedAnsiString,
                    keyAnsiMapping: _keyAnsiString,
                    createBoundedAnsiMapping: size => new SqlServerStringTypeMapping(
                        "varchar(" + size + ")",
                        DbType.AnsiString,
                        unicode: false,
                        size: size),
                    maxBoundedUnicodeLength: 4000,
                    defaultUnicodeMapping: _unboundedUnicodeString,
                    unboundedUnicodeMapping: _unboundedUnicodeString,
                    keyUnicodeMapping: _keyUnicodeString,
                    createBoundedUnicodeMapping: size => new SqlServerStringTypeMapping(
                        "nvarchar(" + size + ")",
                        dbType: null,
                        unicode: true,
                        size: size));
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
        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IReadOnlyDictionary<string, IList<RelationalTypeMapping>> GetMultipleStoreTypeMappings()
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
                ? _unboundedUnicodeString
                : clrType == typeof(byte[])
                    ? _unboundedBinary
                    : base.FindMapping(clrType);
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
