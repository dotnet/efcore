// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

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

    private readonly SqlServerLongTypeMapping _longRowversion
        = new(
            "rowversion",
            converter: new NumberToBytesConverter<long>(),
            providerValueComparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            dbType: DbType.Binary);

    private readonly SqlServerLongTypeMapping _ulongRowversion
        = new(
            "rowversion",
            converter: new NumberToBytesConverter<ulong>(),
            providerValueComparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            dbType: DbType.Binary);

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

    private readonly SqlServerDateOnlyTypeMapping _dateAsDateOnly
        = new("date");

    private readonly SqlServerDateTimeTypeMapping _dateAsDateTime
        = new("date", DbType.Date);

    private readonly SqlServerDateTimeTypeMapping _smallDatetime
        = new("smalldatetime", DbType.DateTime, SqlDbType.SmallDateTime);

    private readonly SqlServerDateTimeTypeMapping _datetime
        = new("datetime", DbType.DateTime);

    private readonly SqlServerDateTimeTypeMapping _datetime2
        = new("datetime2", DbType.DateTime2);

    private readonly SqlServerDateTimeTypeMapping _datetime2Alias
        = new("placeholder", DbType.DateTime2, null, StoreTypePostfix.None);

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
        = new SqlServerDecimalTypeMapping("decimal", precision: 18, scale: 0);

    private readonly DecimalTypeMapping _decimalAlias
        = new SqlServerDecimalTypeMapping("placeholder", precision: 18, scale: 2, storeTypePostfix: StoreTypePostfix.None);

    private readonly DecimalTypeMapping _decimal182
        = new SqlServerDecimalTypeMapping("decimal(18, 2)", precision: 18, scale: 2);

    private readonly DecimalTypeMapping _money
        = new SqlServerDecimalTypeMapping("money", DbType.Currency, sqlDbType: SqlDbType.Money, storeTypePostfix: StoreTypePostfix.None);

    private readonly DecimalTypeMapping _smallMoney
        = new SqlServerDecimalTypeMapping(
            "smallmoney", DbType.Currency, sqlDbType: SqlDbType.SmallMoney, storeTypePostfix: StoreTypePostfix.None);

    private readonly TimeSpanTypeMapping _timeAsTimeSpan
        = new SqlServerTimeSpanTypeMapping("time");

    private readonly TimeOnlyTypeMapping _timeAsTimeOnly
        = new SqlServerTimeOnlyTypeMapping("time");

    private readonly TimeOnlyTypeMapping _timeAlias
        = new SqlServerTimeOnlyTypeMapping("placeholder", StoreTypePostfix.None);

    private readonly SqlServerStringTypeMapping _xml
        = new("xml", unicode: true, storeTypePostfix: StoreTypePostfix.None);

    private readonly SqlServerJsonTypeMapping _json
        = new("nvarchar(max)");

    private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

    private readonly Dictionary<Type, RelationalTypeMapping> _clrNoFacetTypeMappings;

    private readonly Dictionary<string, RelationalTypeMapping[]> _storeTypeMappings;

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
                { typeof(DateOnly), _dateAsDateOnly },
                { typeof(DateTime), _datetime2 },
                { typeof(Guid), _uniqueidentifier },
                { typeof(bool), _bool },
                { typeof(byte), _byte },
                { typeof(double), _double },
                { typeof(DateTimeOffset), _datetimeoffset },
                { typeof(short), _short },
                { typeof(float), _real },
                { typeof(decimal), _decimal182 },
                { typeof(TimeOnly), _timeAsTimeOnly },
                { typeof(TimeSpan), _timeAsTimeSpan },
                { typeof(JsonElement), _json }
            };

        _clrNoFacetTypeMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(DateTime), _datetime2Alias },
                { typeof(DateTimeOffset), _datetimeoffsetAlias },
                { typeof(TimeOnly), _timeAlias },
                { typeof(double), _doubleAlias },
                { typeof(float), _realAlias },
                { typeof(decimal), _decimalAlias }
            };

        // ReSharper disable CoVariantArrayConversion
        _storeTypeMappings
            = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "bigint", new[] { _long } },
                { "binary varying", new[] { _variableLengthBinary } },
                { "binary", new[] { _fixedLengthBinary } },
                { "bit", new[] { _bool } },
                { "char varying", new[] { _variableLengthAnsiString } },
                { "char varying(max)", new[] { _variableLengthMaxAnsiString } },
                { "char", new[] { _fixedLengthAnsiString } },
                { "character varying", new[] { _variableLengthAnsiString } },
                { "character varying(max)", new[] { _variableLengthMaxAnsiString } },
                { "character", new[] { _fixedLengthAnsiString } },
                { "date", new RelationalTypeMapping[] { _dateAsDateOnly, _dateAsDateTime } },
                { "datetime", new[] { _datetime } },
                { "datetime2", new[] { _datetime2 } },
                { "datetimeoffset", new[] { _datetimeoffset } },
                { "dec", new[] { _decimal } },
                { "decimal", new[] { _decimal } },
                { "double precision", new[] { _double } },
                { "float", new[] { _double } },
                { "image", new[] { _imageBinary } },
                { "int", new[] { _int } },
                { "money", new[] { _money } },
                { "national char varying", new[] { _variableLengthUnicodeString } },
                { "national char varying(max)", new[] { _variableLengthMaxUnicodeString } },
                { "national character varying", new[] { _variableLengthUnicodeString } },
                { "national character varying(max)", new[] { _variableLengthMaxUnicodeString } },
                { "national character", new[] { _fixedLengthUnicodeString } },
                { "nchar", new[] { _fixedLengthUnicodeString } },
                { "ntext", new[] { _textUnicodeString } },
                { "numeric", new[] { _decimal } },
                { "nvarchar", new[] { _variableLengthUnicodeString } },
                { "nvarchar(max)", new[] { _variableLengthMaxUnicodeString } },
                { "real", new[] { _real } },
                { "rowversion", new[] { _rowversion } },
                { "smalldatetime", new[] { _smallDatetime } },
                { "smallint", new[] { _short } },
                { "smallmoney", new[] {_smallMoney } },
                { "sql_variant", new[] { _sqlVariant } },
                { "text", new[] { _textAnsiString } },
                { "time", new RelationalTypeMapping[] { _timeAsTimeOnly, _timeAsTimeSpan } },
                { "timestamp", new[] { _rowversion } },
                { "tinyint", new[] { _byte } },
                { "uniqueidentifier", new[] { _uniqueidentifier } },
                { "varbinary", new[] { _variableLengthBinary } },
                { "varbinary(max)", new[] { _variableLengthMaxBinary } },
                { "varchar", new[] { _variableLengthAnsiString } },
                { "varchar(max)", new[] { _variableLengthMaxAnsiString } },
                { "xml", new[] { _xml } }
            };
        // ReSharper restore CoVariantArrayConversion
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
                && mappingInfo.Precision is <= 24
                && (storeTypeNameBase.Equals("float", StringComparison.OrdinalIgnoreCase)
                    || storeTypeNameBase.Equals("double precision", StringComparison.OrdinalIgnoreCase)))
            {
                return _real;
            }

            if (_storeTypeMappings.TryGetValue(storeTypeName, out var mappings)
                || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mappings))
            {
                // We found the user-specified store type. No CLR type was provided - we're probably scaffolding from an existing database,
                // take the first mapping as the default.
                if (clrType is null)
                {
                    return mappings[0];
                }

                // A CLR type was provided - look for a mapping between the store and CLR types. If not found, fail
                // immediately.
                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                return null;
            }

            // SQL Server supports aliases (e.g. CREATE TYPE datetimeAlias FROM datetime2(6))
            // Since we don't know the store name above, usually we end up in the clrType-only lookup below and everything goes well.
            // However, when a facet is specified (length/precision/scale), that facet would get appended to the store type; we don't want
            // this in the case of aliased types, since the facet is already part of the type. So we check whether the CLR type supports
            // facets, and return a special type mapping that doesn't support facets.
            if (clrType != null
                && _clrNoFacetTypeMappings.TryGetValue(clrType, out var mapping))
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

            if (clrType == typeof(ulong) && mappingInfo.IsRowVersion == true)
            {
                return _ulongRowversion;
            }

            if (clrType == typeof(long) && mappingInfo.IsRowVersion == true)
            {
                return _longRowversion;
            }

            if (clrType == typeof(string))
            {
                var isAnsi = mappingInfo.IsUnicode == false;
                var isFixedLength = mappingInfo.IsFixedLength == true;
                var maxSize = isAnsi ? 8000 : 4000;

                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? isAnsi ? 900 : 450 : null);
                if (size < 0 || size > maxSize)
                {
                    size = isFixedLength ? maxSize : null;
                }

                if (size == null
                    && storeTypeName == null
                    && !mappingInfo.HasKeySemantics)
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
                    storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None,
                    useKeyComparison: mappingInfo.HasKeySemantics);
            }

            if (clrType == typeof(byte[]))
            {
                if (mappingInfo.IsRowVersion == true)
                {
                    return _rowversion;
                }

                var isFixedLength = mappingInfo.IsFixedLength == true;

                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 900 : null);
                if (size < 0 || size > 8000)
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

    private static readonly List<string> NameBasesUsingPrecision = new()
    {
        "decimal",
        "dec",
        "numeric",
        "datetime2",
        "datetimeoffset",
        "double precision",
        "float",
        "time"
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string? ParseStoreTypeName(
        string? storeTypeName,
        ref bool? unicode,
        ref int? size,
        ref int? precision,
        ref int? scale)
    {
        if (storeTypeName == null)
        {
            return null;
        }

        var originalSize = size;
        var parsedName = base.ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);

        if (size.HasValue
            && NameBasesUsingPrecision.Any(n => storeTypeName.StartsWith(n, StringComparison.OrdinalIgnoreCase)))
        {
            precision = size;
            size = originalSize;
        }
        else if (storeTypeName.Trim().EndsWith("(max)", StringComparison.OrdinalIgnoreCase))
        {
            size = -1;
        }

        return parsedName;
    }
}
