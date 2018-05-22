// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Oracle.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Oracle.Storage.Internal
{
    public class OracleTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly OracleStringTypeMapping _unboundedUnicodeString
            = new OracleStringTypeMapping(
                "NCLOB",
                dbType: null,
                unicode: true);

        private readonly OracleByteArrayTypeMapping _rowversion
            = new OracleByteArrayTypeMapping(
                "RAW(8)",
                dbType: DbType.Binary,
                size: 8,
                comparer: new ValueComparer<byte[]>(
                    (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                    v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                    v => v == null ? null : v.ToArray()));

        private readonly IntTypeMapping _int
            = new IntTypeMapping("NUMBER(10)", DbType.Int32);

        private readonly LongTypeMapping _long
            = new LongTypeMapping("NUMBER(19)", DbType.Int64);

        private readonly ShortTypeMapping _short
            = new ShortTypeMapping("NUMBER(6)", DbType.Int16);

        private readonly ByteTypeMapping _byte
            = new ByteTypeMapping("NUMBER(3)", DbType.Byte);

        private readonly OracleStringTypeMapping _fixedLengthUnicodeString
            = new OracleStringTypeMapping("NCHAR", dbType: DbType.String, unicode: true, fixedLength: true);

        private readonly OracleStringTypeMapping _variableLengthUnicodeString
            = new OracleStringTypeMapping("NVARCHAR2", dbType: null, unicode: true);

        private readonly OracleStringTypeMapping _fixedLengthAnsiString
            = new OracleStringTypeMapping("CHAR", dbType: DbType.AnsiString, fixedLength: true);

        private readonly OracleStringTypeMapping _variableLengthAnsiString
            = new OracleStringTypeMapping("VARCHAR2", dbType: DbType.AnsiString);

        private readonly OracleByteArrayTypeMapping _variableLengthBinary
            = new OracleByteArrayTypeMapping("BLOB");

        private readonly OracleByteArrayTypeMapping _fixedLengthBinary
            = new OracleByteArrayTypeMapping("RAW");

        private readonly OracleDateTimeTypeMapping _date
            = new OracleDateTimeTypeMapping("DATE", dbType: DbType.Date);

        private readonly OracleDateTimeTypeMapping _datetime
            = new OracleDateTimeTypeMapping("TIMESTAMP", dbType: DbType.DateTime);

        private readonly DoubleTypeMapping _double
            = new OracleDoubleTypeMapping("FLOAT(49)");

        private readonly OracleDateTimeOffsetTypeMapping _datetimeoffset
            = new OracleDateTimeOffsetTypeMapping("TIMESTAMP WITH TIME ZONE");

        private readonly OracleDateTimeOffsetTypeMapping _datetimeoffset3
            = new OracleDateTimeOffsetTypeMapping("TIMESTAMP(3) WITH TIME ZONE");

        private readonly FloatTypeMapping _real
            = new OracleFloatTypeMapping("REAL");

        private readonly DecimalTypeMapping _decimal
            = new OracleDecimalTypeMapping("DECIMAL(29,4)", precision: 29, scale: 4, storeTypePostfix: StoreTypePostfix.PrecisionAndScale);

        private readonly TimeSpanTypeMapping _time
            = new OracleTimeSpanTypeMapping("INTERVAL DAY TO SECOND");

        private readonly OracleStringTypeMapping _xml
            = new OracleStringTypeMapping("XML", dbType: null, unicode: true);

        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        // These are disallowed only if specified without any kind of length specified in parenthesis.
        private readonly HashSet<string> _disallowedMappings
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public OracleTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
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

            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "number(19)", _long },
                    { "blob", _variableLengthBinary },
                    { "raw", _fixedLengthBinary },
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
                    { "varchar2", _variableLengthAnsiString },
                    { "clob", _unboundedUnicodeString },
                    { "xml", _xml },
                    { "number", _int }
                };
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var mapping = FindRawMapping(mappingInfo)?.Clone(mappingInfo);

            if (_disallowedMappings.Contains(mapping?.StoreType))
            {
                throw new ArgumentException(OracleStrings.UnqualifiedDataType(mapping.StoreType));
            }

            return mapping;
        }

        private RelationalTypeMapping FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                if (clrType == typeof(float)
                    && mappingInfo.Size != null
                    && mappingInfo.Size <= 24
                    && (storeTypeNameBase.Equals("float", StringComparison.OrdinalIgnoreCase)
                        || storeTypeNameBase.Equals("double precision", StringComparison.OrdinalIgnoreCase)))
                {
                    return _real;
                }

                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mapping)
                    || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mapping))
                {
                    return clrType == null
                           || mapping.ClrType == clrType
                        ? mapping
                        : null;
                }
            }

            if (clrType != null)
            {
                if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
                {
                    return mapping;
                }

                if (clrType == typeof(string))
                {
                    var isAnsi = mappingInfo.IsUnicode == false;
                    var isFixedLength = mappingInfo.IsFixedLength == true;
                    var baseName = (isAnsi ? "" : "N") + (isFixedLength ? "CHAR" : "VARCHAR2");
                    var unboundedName = isAnsi ? "CLOB" : "NCLOB";
                    var maxSize = isAnsi ? 4000 : 2000;
                    var storeTypePostfix = (StoreTypePostfix?)null;

                    var size = (int?)(mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (isAnsi ? 900 : 450) : maxSize));
                    if (size > maxSize)
                    {
                        size = null;
                        storeTypePostfix = StoreTypePostfix.None;
                    }

                    return new OracleStringTypeMapping(
                        storeTypePostfix == StoreTypePostfix.None ? unboundedName : baseName + "(" + size + ")",
                        isAnsi ? DbType.AnsiString : (DbType?)null,
                        !isAnsi,
                        size,
                        isFixedLength,
                        storeTypePostfix);
                }

                if (clrType == typeof(byte[]))
                {
                    if (mappingInfo.IsRowVersion == true)
                    {
                        return _rowversion;
                    }

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? (int?)900 : null);
                    var storeTypePostfix = (StoreTypePostfix?)null;
                    if (size > 2000)
                    {
                        size = null;
                        storeTypePostfix = StoreTypePostfix.None;
                    }

                    return new OracleByteArrayTypeMapping(
                        (size == -1 || size == null) ? "BLOB" : "RAW(" + size + ")",
                        DbType.Binary,
                        size,
                        storeTypePostfix: storeTypePostfix);
                }
            }

            return null;
        }
    }
}
