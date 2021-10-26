// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlServerTypeMappingSource : RelationalTypeMappingSource
    {
        private readonly RelationalTypeMapping _sqlVariant
            = new SqlServerSqlVariantTypeMapping("sql_variant");

        private readonly FloatTypeMapping _real
            = new SqlServerFloatTypeMapping("real");

        private readonly FloatTypeMapping _realAlias
            = new SqlServerFloatTypeMapping("placeholder", storeTypePostfix: StoreTypePostfix.None);

        private readonly ByteTypeMapping _byte
            = new SqlServerByteTypeMapping("tinyint");

        private readonly ShortTypeMapping _short
            = new SqlServerShortTypeMapping("smallint");

        private readonly LongTypeMapping _long
            = new SqlServerLongTypeMapping("bigint");

        private readonly SqlServerByteArrayTypeMapping _rowversion
            = new(
                "rowversion",
                size: 8,
                comparer: new ValueComparer<byte[]>(
                    (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                    v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                    v => v.ToArray()),
                storeTypePostfix: StoreTypePostfix.None);

        private readonly IntTypeMapping _int
            = new("int");

        private readonly BoolTypeMapping _bool
            = new SqlServerBoolTypeMapping("bit");

        private readonly SqlServerStringTypeMapping _fixedLengthUnicodeString
            = new(unicode: true, fixedLength: true);

        private readonly SqlServerStringTypeMapping _textUnicodeString
            = new("ntext", unicode: true, sqlDbType: SqlDbType.NText, storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerStringTypeMapping _variableLengthUnicodeString
            = new(unicode: true);

        private readonly SqlServerStringTypeMapping _variableLengthMaxUnicodeString
            = new("nvarchar(max)", unicode: true, storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerStringTypeMapping _fixedLengthAnsiString
            = new(fixedLength: true);

        private readonly SqlServerStringTypeMapping _textAnsiString
            = new("text", sqlDbType: SqlDbType.Text, storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerStringTypeMapping _variableLengthAnsiString
            = new();

        private readonly SqlServerStringTypeMapping _variableLengthMaxAnsiString
            = new("varchar(max)", storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerByteArrayTypeMapping _variableLengthBinary
            = new();

        private readonly SqlServerByteArrayTypeMapping _imageBinary
            = new("image", sqlDbType: SqlDbType.Image);

        private readonly SqlServerByteArrayTypeMapping _variableLengthMaxBinary
            = new("varbinary(max)", storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerByteArrayTypeMapping _fixedLengthBinary
            = new(fixedLength: true);

        private readonly SqlServerDateTimeTypeMapping _date
            = new("date", DbType.Date);

        private readonly SqlServerDateTimeTypeMapping _datetime
            = new("datetime", DbType.DateTime);

        private readonly SqlServerDateTimeTypeMapping _datetime2
            = new("datetime2", DbType.DateTime2);

        private readonly SqlServerDateTimeTypeMapping _datetime2Alias
            = new("placeholder", DbType.DateTime2, StoreTypePostfix.None);

        private readonly DoubleTypeMapping _double
            = new SqlServerDoubleTypeMapping("float");

        private readonly DoubleTypeMapping _doubleAlias
            = new SqlServerDoubleTypeMapping("placeholder", storeTypePostfix: StoreTypePostfix.None);

        private readonly SqlServerDateTimeOffsetTypeMapping _datetimeoffset
            = new("datetimeoffset");

        private readonly SqlServerDateTimeOffsetTypeMapping _datetimeoffsetAlias
            = new("placeholder", DbType.DateTimeOffset, StoreTypePostfix.None);

        private readonly GuidTypeMapping _uniqueidentifier
            = new("uniqueidentifier");

        private readonly DecimalTypeMapping _decimal
            = new SqlServerDecimalTypeMapping("decimal");

        private readonly DecimalTypeMapping _decimalAlias
            = new SqlServerDecimalTypeMapping("placeholder", precision: 18, scale: 2, storeTypePostfix: StoreTypePostfix.None);

        private readonly DecimalTypeMapping _decimal182
            = new SqlServerDecimalTypeMapping("decimal(18, 2)", precision: 18, scale: 2);

        private readonly DecimalTypeMapping _money
            = new SqlServerDecimalTypeMapping("money", storeTypePostfix: StoreTypePostfix.None);

        private readonly TimeSpanTypeMapping _time
            = new SqlServerTimeSpanTypeMapping("time");

        private readonly SqlServerStringTypeMapping _xml
            = new("xml", unicode: true, storeTypePostfix: StoreTypePostfix.None);

        private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

        private readonly Dictionary<Type, RelationalTypeMapping> _clrNoFacetTypeMappings;

        private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlServerTypeMappingSource(
            TypeMappingSourceDependencies dependencies,
            RelationalTypeMappingSourceDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(int), _int },
                    { typeof(long), _long },
                    { typeof(DateTime), _datetime2 },
                    { typeof(Guid), _uniqueidentifier },
                    { typeof(bool), _bool },
                    { typeof(byte), _byte },
                    { typeof(double), _double },
                    { typeof(DateTimeOffset), _datetimeoffset },
                    { typeof(short), _short },
                    { typeof(float), _real },
                    { typeof(decimal), _decimal182 },
                    { typeof(TimeSpan), _time }
                };

            _clrNoFacetTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
                    { typeof(DateTime), _datetime2Alias },
                    { typeof(double), _doubleAlias },
                    { typeof(DateTimeOffset), _datetimeoffsetAlias },
                    { typeof(float), _realAlias },
                    { typeof(decimal), _decimalAlias }
                };

            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                {
                    { "bigint", _long },
                    { "binary varying", _variableLengthBinary },
                    { "binary", _fixedLengthBinary },
                    { "bit", _bool },
                    { "char varying", _variableLengthAnsiString },
                    { "char varying(max)", _variableLengthMaxAnsiString },
                    { "char", _fixedLengthAnsiString },
                    { "character varying", _variableLengthAnsiString },
                    { "character varying(max)", _variableLengthMaxAnsiString },
                    { "character", _fixedLengthAnsiString },
                    { "date", _date },
                    { "datetime", _datetime },
                    { "datetime2", _datetime2 },
                    { "datetimeoffset", _datetimeoffset },
                    { "dec", _decimal },
                    { "decimal", _decimal },
                    { "double precision", _double },
                    { "float", _double },
                    { "image", _imageBinary },
                    { "int", _int },
                    { "money", _money },
                    { "national char varying", _variableLengthUnicodeString },
                    { "national char varying(max)", _variableLengthMaxUnicodeString },
                    { "national character varying", _variableLengthUnicodeString },
                    { "national character varying(max)", _variableLengthMaxUnicodeString },
                    { "national character", _fixedLengthUnicodeString },
                    { "nchar", _fixedLengthUnicodeString },
                    { "ntext", _textUnicodeString },
                    { "numeric", _decimal },
                    { "nvarchar", _variableLengthUnicodeString },
                    { "nvarchar(max)", _variableLengthMaxUnicodeString },
                    { "real", _real },
                    { "rowversion", _rowversion },
                    { "smalldatetime", _datetime },
                    { "smallint", _short },
                    { "smallmoney", _money },
                    { "sql_variant", _sqlVariant },
                    { "text", _textAnsiString },
                    { "time", _time },
                    { "timestamp", _rowversion },
                    { "tinyint", _byte },
                    { "uniqueidentifier", _uniqueidentifier },
                    { "varbinary", _variableLengthBinary },
                    { "varbinary(max)", _variableLengthMaxBinary },
                    { "varchar", _variableLengthAnsiString },
                    { "varchar(max)", _variableLengthMaxAnsiString },
                    { "xml", _xml }
                };
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => base.FindMapping(mappingInfo) ?? FindRawMapping(mappingInfo)?.Clone(mappingInfo);

        private RelationalTypeMapping? FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;

            if (storeTypeName != null)
            {
                var storeTypeNameBase = mappingInfo.StoreTypeNameBase;
                if (storeTypeNameBase!.StartsWith("[", StringComparison.Ordinal)
                    && storeTypeNameBase.EndsWith("]", StringComparison.Ordinal))
                {
                    storeTypeNameBase = storeTypeNameBase.Substring(1, storeTypeNameBase.Length - 2);
                }

                if (clrType == typeof(float)
                    && mappingInfo.Precision != null
                    && mappingInfo.Precision <= 24
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

                if (clrType != null
                    && _clrNoFacetTypeMappings.TryGetValue(clrType, out mapping))
                {
                    return mapping;
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
                    var maxSize = isAnsi ? 8000 : 4000;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? isAnsi ? 900 : 450 : null);
                    if (size > maxSize)
                    {
                        size = isFixedLength ? maxSize : null;
                    }

                    if (size == null
                        && storeTypeName == null)
                    {
                        return isAnsi
                            ? isFixedLength
                                ? _fixedLengthAnsiString
                                : _variableLengthMaxAnsiString
                            : isFixedLength
                                ? _fixedLengthUnicodeString
                                : _variableLengthMaxUnicodeString;
                    }

                    return new SqlServerStringTypeMapping(
                        unicode: !isAnsi,
                        size: size,
                        fixedLength: isFixedLength,
                        storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None);
                }

                if (clrType == typeof(byte[]))
                {
                    if (mappingInfo.IsRowVersion == true)
                    {
                        return _rowversion;
                    }

                    var isFixedLength = mappingInfo.IsFixedLength == true;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 900 : null);
                    if (size > 8000)
                    {
                        size = isFixedLength ? 8000 : null;
                    }

                    return size == null
                        ? _variableLengthMaxBinary
                        : new SqlServerByteArrayTypeMapping(
                            size: size,
                            fixedLength: isFixedLength,
                            storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None);
                }
            }

            return null;
        }

        private static readonly List<string> _nameBasesUsingPrecision = new()
        {
            "decimal",
            "dec",
            "numeric",
            "datetime2",
            "datetimeoffset",
            "double precision",
            "float"
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override string? ParseStoreTypeName(
            string? storeTypeName,
            out bool? unicode,
            out int? size,
            out int? precision,
            out int? scale)
        {
            var parsedName = base.ParseStoreTypeName(storeTypeName, out unicode, out size, out precision, out scale);

            if (size.HasValue
                && storeTypeName != null
                && _nameBasesUsingPrecision.Any(n => storeTypeName.StartsWith(n, StringComparison.OrdinalIgnoreCase)))
            {
                precision = size;
                size = null;
            }

            return parsedName;
        }
    }
}
