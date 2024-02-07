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
    private static readonly SqlServerFloatTypeMapping RealAlias
        = new("placeholder", storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerByteArrayTypeMapping Rowversion
        = new(
            "rowversion",
            size: 8,
            comparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerLongTypeMapping LongRowversion
        = new(
            "rowversion",
            converter: new NumberToBytesConverter<long>(),
            providerValueComparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            dbType: DbType.Binary);

    private static readonly SqlServerLongTypeMapping UlongRowversion
        = new(
            "rowversion",
            converter: new NumberToBytesConverter<ulong>(),
            providerValueComparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            dbType: DbType.Binary);

    private static readonly SqlServerStringTypeMapping FixedLengthUnicodeString
        = new(unicode: true, fixedLength: true);

    private static readonly SqlServerStringTypeMapping TextUnicodeString
        = new("ntext", unicode: true, sqlDbType: SqlDbType.NText, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerStringTypeMapping VariableLengthUnicodeString
        = new(unicode: true);

    private static readonly SqlServerStringTypeMapping VariableLengthMaxUnicodeString
        = new("nvarchar(max)", unicode: true, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerStringTypeMapping FixedLengthAnsiString
        = new(fixedLength: true);

    private static readonly SqlServerStringTypeMapping TextAnsiString
        = new("text", sqlDbType: SqlDbType.Text, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerStringTypeMapping VariableLengthMaxAnsiString
        = new("varchar(max)", storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerByteArrayTypeMapping ImageBinary
        = new("image", sqlDbType: SqlDbType.Image);

    private static readonly SqlServerByteArrayTypeMapping VariableLengthMaxBinary
        = new("varbinary(max)", storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerByteArrayTypeMapping FixedLengthBinary
        = new(fixedLength: true);

    private static readonly SqlServerDateTimeTypeMapping DateAsDateTime
        = new("date", DbType.Date);

    private static readonly SqlServerDateTimeTypeMapping SmallDatetime
        = new("smalldatetime", DbType.DateTime, SqlDbType.SmallDateTime);

    private static readonly SqlServerDateTimeTypeMapping Datetime
        = new("datetime", DbType.DateTime);

    private static readonly SqlServerDateTimeTypeMapping Datetime2Alias
        = new("placeholder", DbType.DateTime2, null, StoreTypePostfix.None);

    private static readonly DoubleTypeMapping DoubleAlias
        = new SqlServerDoubleTypeMapping("placeholder", storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerDateTimeOffsetTypeMapping DatetimeoffsetAlias
        = new("placeholder", DbType.DateTimeOffset, StoreTypePostfix.None);

    private static readonly SqlServerDecimalTypeMapping Decimal
        = new("decimal", precision: 18, scale: 0);

    private static readonly SqlServerDecimalTypeMapping DecimalAlias
        = new("placeholder", precision: 18, scale: 2, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerDecimalTypeMapping Money
        = new("money", DbType.Currency, sqlDbType: SqlDbType.Money, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerDecimalTypeMapping SmallMoney
        = new("smallmoney", DbType.Currency, sqlDbType: SqlDbType.SmallMoney, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SqlServerTimeOnlyTypeMapping TimeAlias
        = new("placeholder", StoreTypePostfix.None);

    private static readonly GuidTypeMapping Uniqueidentifier
        = new("uniqueidentifier");

    private static readonly SqlServerStringTypeMapping Xml
        = new("xml", unicode: true, storeTypePostfix: StoreTypePostfix.None);

    private static readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

    private static readonly Dictionary<Type, RelationalTypeMapping> _clrNoFacetTypeMappings;

    private static readonly Dictionary<string, RelationalTypeMapping[]> _storeTypeMappings;

    static SqlServerTypeMappingSource()
    {
        _clrTypeMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(int), IntTypeMapping.Default },
                { typeof(long), SqlServerLongTypeMapping.Default },
                { typeof(DateOnly), SqlServerDateOnlyTypeMapping.Default },
                { typeof(DateTime), SqlServerDateTimeTypeMapping.Default },
                { typeof(Guid), Uniqueidentifier },
                { typeof(bool), SqlServerBoolTypeMapping.Default },
                { typeof(byte), SqlServerByteTypeMapping.Default },
                { typeof(double), SqlServerDoubleTypeMapping.Default },
                { typeof(DateTimeOffset), SqlServerDateTimeOffsetTypeMapping.Default },
                { typeof(short), SqlServerShortTypeMapping.Default },
                { typeof(float), SqlServerFloatTypeMapping.Default },
                { typeof(decimal), SqlServerDecimalTypeMapping.Default },
                { typeof(TimeOnly), SqlServerTimeOnlyTypeMapping.Default },
                { typeof(TimeSpan), SqlServerTimeSpanTypeMapping.Default },
                { typeof(JsonElement), SqlServerJsonTypeMapping.Default }
            };

        _clrNoFacetTypeMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(DateTime), Datetime2Alias },
                { typeof(DateTimeOffset), DatetimeoffsetAlias },
                { typeof(TimeOnly), TimeAlias },
                { typeof(double), DoubleAlias },
                { typeof(float), RealAlias },
                { typeof(decimal), DecimalAlias }
            };

        // ReSharper disable CoVariantArrayConversion
        _storeTypeMappings
            = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "bigint", [SqlServerLongTypeMapping.Default] },
                { "binary varying", [SqlServerByteArrayTypeMapping.Default] },
                { "binary", [FixedLengthBinary] },
                { "bit", [SqlServerBoolTypeMapping.Default] },
                { "char varying", [SqlServerStringTypeMapping.Default] },
                { "char varying(max)", [VariableLengthMaxAnsiString] },
                { "char", [FixedLengthAnsiString] },
                { "character varying", [SqlServerStringTypeMapping.Default] },
                { "character varying(max)", [VariableLengthMaxAnsiString] },
                { "character", [FixedLengthAnsiString] },
                { "date", [SqlServerDateOnlyTypeMapping.Default, DateAsDateTime] },
                { "datetime", [Datetime] },
                { "datetime2", [SqlServerDateTimeTypeMapping.Default] },
                { "datetimeoffset", [SqlServerDateTimeOffsetTypeMapping.Default] },
                { "dec", [Decimal] },
                { "decimal", [Decimal] },
                { "double precision", [SqlServerDoubleTypeMapping.Default] },
                { "float", [SqlServerDoubleTypeMapping.Default] },
                { "image", [ImageBinary] },
                { "int", [IntTypeMapping.Default] },
                { "money", [Money] },
                { "national char varying", [VariableLengthUnicodeString] },
                { "national char varying(max)", [VariableLengthMaxUnicodeString] },
                { "national character varying", [VariableLengthUnicodeString] },
                { "national character varying(max)", [VariableLengthMaxUnicodeString] },
                { "national character", [FixedLengthUnicodeString] },
                { "nchar", [FixedLengthUnicodeString] },
                { "ntext", [TextUnicodeString] },
                { "numeric", [Decimal] },
                { "nvarchar", [VariableLengthUnicodeString] },
                { "nvarchar(max)", [VariableLengthMaxUnicodeString] },
                { "real", [SqlServerFloatTypeMapping.Default] },
                { "rowversion", [Rowversion] },
                { "smalldatetime", [SmallDatetime] },
                { "smallint", [SqlServerShortTypeMapping.Default] },
                { "smallmoney", [SmallMoney] },
                { "sql_variant", [SqlServerSqlVariantTypeMapping.Default] },
                { "text", [TextAnsiString] },
                { "time", [SqlServerTimeOnlyTypeMapping.Default, SqlServerTimeSpanTypeMapping.Default] },
                { "timestamp", [Rowversion] },
                { "tinyint", [SqlServerByteTypeMapping.Default] },
                { "uniqueidentifier", [Uniqueidentifier] },
                { "varbinary", [SqlServerByteArrayTypeMapping.Default] },
                { "varbinary(max)", [VariableLengthMaxBinary] },
                { "varchar", [SqlServerStringTypeMapping.Default] },
                { "varchar(max)", [VariableLengthMaxAnsiString] },
                { "xml", [Xml] }
            };
        // ReSharper restore CoVariantArrayConversion
    }

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
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => base.FindMapping(mappingInfo)
            ?? FindRawMapping(mappingInfo)?.WithTypeMappingInfo(mappingInfo);

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
                storeTypeNameBase = storeTypeNameBase[1..^1];
            }

            if (clrType == typeof(float)
                && mappingInfo.Precision is <= 24
                && (storeTypeNameBase.Equals("float", StringComparison.OrdinalIgnoreCase)
                    || storeTypeNameBase.Equals("double precision", StringComparison.OrdinalIgnoreCase)))
            {
                return SqlServerFloatTypeMapping.Default;
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
                return UlongRowversion;
            }

            if (clrType == typeof(long) && mappingInfo.IsRowVersion == true)
            {
                return LongRowversion;
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
                    && !mappingInfo.IsKeyOrIndex)
                {
                    return isAnsi
                        ? isFixedLength
                            ? FixedLengthAnsiString
                            : VariableLengthMaxAnsiString
                        : isFixedLength
                            ? FixedLengthUnicodeString
                            : VariableLengthMaxUnicodeString;
                }

                return new SqlServerStringTypeMapping(
                    unicode: !isAnsi,
                    size: size,
                    fixedLength: isFixedLength,
                    storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None,
                    useKeyComparison: mappingInfo.IsKey);
            }

            if (clrType == typeof(byte[]))
            {
                if (mappingInfo.IsRowVersion == true)
                {
                    return Rowversion;
                }

                if (mappingInfo.ElementTypeMapping == null)
                {
                    var isFixedLength = mappingInfo.IsFixedLength == true;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 900 : null);
                    if (size is < 0 or > 8000)
                    {
                        size = isFixedLength ? 8000 : null;
                    }

                    return size == null
                        ? VariableLengthMaxBinary
                        : new SqlServerByteArrayTypeMapping(
                            size: size,
                            fixedLength: isFixedLength,
                            storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None);
                }
            }
        }

        return null;
    }

    private static readonly List<string> NameBasesUsingPrecision =
    [
        "decimal",
        "dec",
        "numeric",
        "datetime2",
        "datetimeoffset",
        "double precision",
        "float",
        "time"
    ];

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
