// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.Converters;
using Microsoft.EntityFrameworkCore.Utilities;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleTypeMapper : RelationalTypeMapper
    {
        private static readonly IDictionary<Type, MethodInfo> _getXMethods
            = new Dictionary<Type, MethodInfo>
            {
                { typeof(OracleTimeStampTZ), typeof(OracleDataReader).GetTypeInfo().GetDeclaredMethod(nameof(OracleDataReader.GetOracleTimeStampTZ)) }
            };

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

        private readonly OracleDateTimeOffsetTypeMapping _datetimeoffset
            = new OracleDateTimeOffsetTypeMapping(
                "TIMESTAMP WITH TIME ZONE",
                new ValueConverter<DateTimeOffset, OracleTimeStampTZ>(
                    v => new OracleTimeStampTZ(v.DateTime, v.Offset.ToString()),
                    v => new DateTimeOffset(v.Value, v.GetTimeZoneOffset())));

        private readonly FloatTypeMapping _real = new OracleFloatTypeMapping("REAL");

        private readonly DecimalTypeMapping _decimal = new DecimalTypeMapping("NUMBER(29,4)");

        private readonly TimeSpanTypeMapping _time = new OracleTimeSpanTypeMapping("INTERVAL DAY TO SECOND");

        private readonly OracleStringTypeMapping _xml = new OracleStringTypeMapping("XML", dbType: null, unicode: true);

        private readonly Dictionary<string, IList<RelationalTypeMapping>> _storeTypeMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private readonly HashSet<string> _disallowedMappings;

        public OracleTypeMapper(
            [NotNull] CoreTypeMapperDependencies coreDependencies,
            [NotNull] RelationalTypeMapperDependencies dependencies)
            : base(coreDependencies, dependencies)
        {
            _storeTypeMappings
                = new Dictionary<string, IList<RelationalTypeMapping>>(StringComparer.OrdinalIgnoreCase)
                {
                    { "number(19)", new List<RelationalTypeMapping> { _long } },
                    { "blob", new List<RelationalTypeMapping> { _variableLengthBinary } },
                    { "raw", new List<RelationalTypeMapping> { _fixedLengthBinary } },
                    { "char", new List<RelationalTypeMapping> { _fixedLengthAnsiString } },
                    { "date", new List<RelationalTypeMapping> { _date } },
                    { "timestamp", new List<RelationalTypeMapping> { _datetime } },
                    { "timestamp(3) with time zone", new List<RelationalTypeMapping> { _datetimeoffset } },
                    { "timestamp with time zone", new List<RelationalTypeMapping> { _datetimeoffset } },
                    { "number(29,4)", new List<RelationalTypeMapping> { _decimal } },
                    { "float(49)", new List<RelationalTypeMapping> { _double } },
                    { "number(10)", new List<RelationalTypeMapping> { _int } },
                    { "nchar", new List<RelationalTypeMapping> { _fixedLengthUnicodeString } },
                    { "nvarchar2", new List<RelationalTypeMapping> { _variableLengthUnicodeString } },
                    { "number(6)", new List<RelationalTypeMapping> { _short } },
                    { "interval", new List<RelationalTypeMapping> { _time } },
                    { "number(3)", new List<RelationalTypeMapping> { _byte } },
                    { "varchar2", new List<RelationalTypeMapping> { _variableLengthAnsiString } },
                    { "clob", new List<RelationalTypeMapping> { _unboundedUnicodeString, _unboundedAnsiString } },
                    { "xml", new List<RelationalTypeMapping> { _xml } }
                };

            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(DateTime), _datetime },
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

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings()
            => _clrTypeMappings;

        protected override IReadOnlyDictionary<string, IList<RelationalTypeMapping>> GetMultipleStoreTypeMappings()
            => _storeTypeMappings;

        public override RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            var underlyingType = clrType.UnwrapNullableType().UnwrapEnumType();

            return underlyingType == typeof(string)
                ? _defaultUnicodeString
                : underlyingType == typeof(byte[])
                    ? _unboundedBinary
                    : base.FindMapping(clrType);
        }

        // Indexes in Oracle have a max size of 900 bytes

        protected override bool RequiresKeyMapping(IProperty property)
            => base.RequiresKeyMapping(property) || property.IsIndex();

        public override MethodInfo GetDataReaderMethod(Type type)
            => _getXMethods.TryGetValue(type, out var method)
                ? method
                : base.GetDataReaderMethod(type);
    }
}
