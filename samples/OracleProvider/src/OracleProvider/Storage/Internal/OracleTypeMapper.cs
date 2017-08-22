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
    public class OracleTypeMapper : RelationalTypeMapper
    {
        private readonly OracleStringTypeMapping _defaultUnicodeString
            = new OracleStringTypeMapping("NVARCHAR2(2000)", dbType: null, unicode: true);

        private readonly OracleStringTypeMapping _unboundedUnicodeString
            = new OracleStringTypeMapping("NCLOB", dbType: null, unicode: true);

        private readonly OracleStringTypeMapping _keyUnicodeString
            = new OracleStringTypeMapping("NVARCHAR2(450)", dbType: null, unicode: true, size: 450);

        private readonly OracleStringTypeMapping _defaultAnsiString
            = new OracleStringTypeMapping("VARCHAR2(4000)", dbType: DbType.AnsiString);

        private readonly OracleStringTypeMapping _unboundedAnsiString
            = new OracleStringTypeMapping("CLOB", dbType: DbType.AnsiString);

        private readonly OracleStringTypeMapping _keyAnsiString
            = new OracleStringTypeMapping("VARCHAR2(900)", dbType: DbType.AnsiString, unicode: false, size: 900);

        private readonly OracleByteArrayTypeMapping _unboundedBinary
            = new OracleByteArrayTypeMapping("BLOB");

        private readonly OracleByteArrayTypeMapping _keyBinary
            = new OracleByteArrayTypeMapping("RAW(900)", dbType: DbType.Binary, size: 900);

        private readonly OracleByteArrayTypeMapping _rowversion
            = new OracleByteArrayTypeMapping("RAW(8)", dbType: DbType.Binary, size: 8);

        private readonly IntTypeMapping _int = new IntTypeMapping("NUMBER(10)", DbType.Int32);

        private readonly LongTypeMapping _long = new LongTypeMapping("NUMBER(19)", DbType.Int64);

        private readonly ShortTypeMapping _short = new ShortTypeMapping("NUMBER(6)", DbType.Int16);

        private readonly ByteTypeMapping _byte = new ByteTypeMapping("NUMBER(3)", DbType.Byte);

        private readonly BoolTypeMapping _bool = new BoolTypeMapping("NUMBER(1)");

        private readonly OracleStringTypeMapping _fixedLengthUnicodeString
            = new OracleStringTypeMapping("NCHAR", dbType: DbType.String, unicode: true);

        private readonly OracleStringTypeMapping _variableLengthUnicodeString
            = new OracleStringTypeMapping("NVARCHAR2", dbType: null, unicode: true);

        private readonly OracleStringTypeMapping _fixedLengthAnsiString
            = new OracleStringTypeMapping("CHAR", dbType: DbType.AnsiString);

        private readonly OracleStringTypeMapping _variableLengthAnsiString
            = new OracleStringTypeMapping("VARCHAR2", dbType: DbType.AnsiString);

        private readonly OracleByteArrayTypeMapping _variableLengthBinary = new OracleByteArrayTypeMapping("BLOB");

        private readonly OracleByteArrayTypeMapping _fixedLengthBinary = new OracleByteArrayTypeMapping("RAW");

        private readonly OracleDateTimeTypeMapping _date = new OracleDateTimeTypeMapping("DATE", dbType: DbType.Date);

        private readonly OracleDateTimeTypeMapping _datetime = new OracleDateTimeTypeMapping("TIMESTAMP", dbType: DbType.DateTime);

        private readonly DoubleTypeMapping _double = new OracleDoubleTypeMapping("FLOAT(49)");

        private readonly OracleDateTimeOffsetTypeMapping _datetimeoffset = new OracleDateTimeOffsetTypeMapping("TIMESTAMP WITH TIME ZONE");

        // TODO: Remove this hard-coded mapping
        private readonly OracleDateTimeOffsetTypeMapping _datetimeoffset3 = new OracleDateTimeOffsetTypeMapping("TIMESTAMP(3) WITH TIME ZONE");

        private readonly FloatTypeMapping _real = new OracleFloatTypeMapping("REAL");

        private readonly GuidTypeMapping _uniqueidentifier = new OracleGuidTypeMapping("RAW(16)", DbType.Binary);

        private readonly DecimalTypeMapping _decimal = new DecimalTypeMapping("DECIMAL(29,4)");

        private readonly TimeSpanTypeMapping _time = new OracleTimeSpanTypeMapping("INTERVAL DAY TO SECOND");

        private readonly OracleStringTypeMapping _xml = new OracleStringTypeMapping("XML", dbType: null, unicode: true);

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly HashSet<string> _disallowedMappings;

        public OracleTypeMapper([NotNull] RelationalTypeMapperDependencies dependencies)
            : base(dependencies)
        {
            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "number(19)", _long },
                    { "blob", _variableLengthBinary },
                    { "raw", _fixedLengthBinary },
                    { "number(1)", _bool },
                    { "char", _fixedLengthAnsiString },
                    { "date", _date },
                    { "timestamp", _datetime },
                    { "timestamp(3) with time zone", _datetimeoffset3 },
                    { "timestamp with time zone", _datetimeoffset },
                    { "decimal(29,4)", _decimal },
                    { "float(49)", _double },
                    { "number(10)", _int },
                    { "nchar", _fixedLengthUnicodeString },
                    { "nvarchar2", _variableLengthUnicodeString },
                    { "number(6)", _short },
                    { "interval", _time },
                    { "number(3)", _byte },
                    { "raw(16)", _uniqueidentifier },
                    { "varchar2", _variableLengthAnsiString },
                    { "xml", _xml }
                };

            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(DateTime), _datetime },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(bool), _bool },
                    { typeof(byte), _byte },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(short), _short },
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
                    "nvarchar2",
                    "varchar2"
                };

            ByteArrayMapper
                = new ByteArrayRelationalTypeMapper(
                    maxBoundedLength: 2000,
                    defaultMapping: _unboundedBinary,
                    unboundedMapping: _unboundedBinary,
                    keyMapping: _keyBinary,
                    rowVersionMapping: _rowversion,
                    createBoundedMapping: size => new OracleByteArrayTypeMapping(
                        "RAW(" + size + ")",
                        DbType.Binary,
                        size));

            StringMapper
                = new StringRelationalTypeMapper(
                    maxBoundedAnsiLength: 4000,
                    defaultAnsiMapping: _defaultAnsiString,
                    unboundedAnsiMapping: _unboundedAnsiString,
                    keyAnsiMapping: _keyAnsiString,
                    createBoundedAnsiMapping: size => new OracleStringTypeMapping(
                        "VARCHAR2(" + size + ")",
                        DbType.AnsiString,
                        unicode: false,
                        size: size),
                    maxBoundedUnicodeLength: 2000,
                    defaultUnicodeMapping: _defaultUnicodeString,
                    unboundedUnicodeMapping: _unboundedUnicodeString,
                    keyUnicodeMapping: _keyUnicodeString,
                    createBoundedUnicodeMapping: size => new OracleStringTypeMapping(
                        "NVARCHAR2(" + size + ")",
                        dbType: null,
                        unicode: true,
                        size: size));
        }

        public override IByteArrayRelationalTypeMapper ByteArrayMapper { get; }

        public override IStringRelationalTypeMapper StringMapper { get; }

        public override void ValidateTypeName(string storeType)
        {
            if (_disallowedMappings.Contains(storeType))
            {
                throw new ArgumentException(OracleStrings.UnqualifiedDataType(storeType));
            }
        }

        protected override string GetColumnType(IProperty property) => property.Oracle().ColumnType;

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings()
            => _storeTypeMappings;

        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            clrType = clrType.UnwrapNullableType().UnwrapEnumType();

            return clrType == typeof(string)
                ? _defaultUnicodeString
                : (clrType == typeof(byte[])
                    ? _unboundedBinary
                    : base.FindMapping(clrType));
        }

        // Indexes in Oracle have a max size of 900 bytes

        protected override bool RequiresKeyMapping(IProperty property)
            => base.RequiresKeyMapping(property) || property.IsIndex();
    }
}
