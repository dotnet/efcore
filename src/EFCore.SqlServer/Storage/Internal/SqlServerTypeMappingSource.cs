// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerTypeMappingSource : RelationalTypeMappingSource
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly RelationalTypeMapping SqlVariant
        = new SqlServerSqlVariantTypeMapping("sql_variant");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly FloatTypeMapping Real
        = new SqlServerFloatTypeMapping("real");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly FloatTypeMapping RealAlias
        = new SqlServerFloatTypeMapping("placeholder", storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ByteTypeMapping Byte
        = new SqlServerByteTypeMapping("tinyint");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly ShortTypeMapping Short
        = new SqlServerShortTypeMapping("smallint");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly LongTypeMapping Long
        = new SqlServerLongTypeMapping("bigint");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerByteArrayTypeMapping Rowversion
        = new(
            "rowversion",
            size: 8,
            comparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerLongTypeMapping LongRowversion
        = new(
            "rowversion",
            converter: new NumberToBytesConverter<long>(),
            providerValueComparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            dbType: DbType.Binary);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerLongTypeMapping UlongRowversion
        = new(
            "rowversion",
            converter: new NumberToBytesConverter<ulong>(),
            providerValueComparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            dbType: DbType.Binary);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly IntTypeMapping Int
        = new("int");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly BoolTypeMapping Bool
        = new SqlServerBoolTypeMapping("bit");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping FixedLengthUnicodeString
        = new(unicode: true, fixedLength: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping TextUnicodeString
        = new("ntext", unicode: true, sqlDbType: SqlDbType.NText, storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping VariableLengthUnicodeString
        = new(unicode: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping VariableLengthMaxUnicodeString
        = new("nvarchar(max)", unicode: true, storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping FixedLengthAnsiString
        = new(fixedLength: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping TextAnsiString
        = new("text", sqlDbType: SqlDbType.Text, storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping VariableLengthAnsiString
        = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping VariableLengthMaxAnsiString
        = new("varchar(max)", storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerByteArrayTypeMapping VariableLengthBinary
        = new();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerByteArrayTypeMapping ImageBinary
        = new("image", sqlDbType: SqlDbType.Image);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerByteArrayTypeMapping VariableLengthMaxBinary
        = new("varbinary(max)", storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerByteArrayTypeMapping FixedLengthBinary
        = new(fixedLength: true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerDateOnlyTypeMapping DateAsDateOnly
        = new("date");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerDateTimeTypeMapping DateAsDateTime
        = new("date", DbType.Date);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerDateTimeTypeMapping SmallDatetime
        = new("smalldatetime", DbType.DateTime, SqlDbType.SmallDateTime);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerDateTimeTypeMapping Datetime
        = new("datetime", DbType.DateTime);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerDateTimeTypeMapping Datetime2
        = new("datetime2", DbType.DateTime2);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerDateTimeTypeMapping Datetime2Alias
        = new("placeholder", DbType.DateTime2, null, StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly DoubleTypeMapping Double
        = new SqlServerDoubleTypeMapping("float");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly DoubleTypeMapping DoubleAlias
        = new SqlServerDoubleTypeMapping("placeholder", storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerDateTimeOffsetTypeMapping Datetimeoffset
        = new("datetimeoffset");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerDateTimeOffsetTypeMapping DatetimeoffsetAlias
        = new("placeholder", DbType.DateTimeOffset, StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly GuidTypeMapping Uniqueidentifier
        = new("uniqueidentifier");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly DecimalTypeMapping Decimal
        = new SqlServerDecimalTypeMapping("decimal", precision: 18, scale: 0);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly DecimalTypeMapping DecimalAlias
        = new SqlServerDecimalTypeMapping("placeholder", precision: 18, scale: 2, storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly DecimalTypeMapping Decimal182
        = new SqlServerDecimalTypeMapping("decimal(18, 2)", precision: 18, scale: 2);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly DecimalTypeMapping Money
        = new SqlServerDecimalTypeMapping("money", DbType.Currency, sqlDbType: SqlDbType.Money, storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly DecimalTypeMapping SmallMoney
        = new SqlServerDecimalTypeMapping(
            "smallmoney", DbType.Currency, sqlDbType: SqlDbType.SmallMoney, storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly TimeSpanTypeMapping TimeAsTimeSpan
        = new SqlServerTimeSpanTypeMapping("time");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly TimeOnlyTypeMapping TimeAsTimeOnly
        = new SqlServerTimeOnlyTypeMapping("time");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly TimeOnlyTypeMapping TimeAlias
        = new SqlServerTimeOnlyTypeMapping("placeholder", StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerStringTypeMapping Xml
        = new("xml", unicode: true, storeTypePostfix: StoreTypePostfix.None);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static readonly SqlServerJsonTypeMapping Json
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
        RelationalTypeMappingSourceDependencies relationalDependencies,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, relationalDependencies)
    {
        _clrTypeMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(int), Int },
                { typeof(long), Long },
                { typeof(DateOnly), DateAsDateOnly },
                { typeof(DateTime), Datetime2 },
                { typeof(Guid), Uniqueidentifier },
                { typeof(bool), Bool },
                { typeof(byte), Byte },
                { typeof(double), Double },
                { typeof(DateTimeOffset), Datetimeoffset },
                { typeof(short), Short },
                { typeof(float), Real },
                { typeof(decimal), Decimal182 },
                { typeof(TimeOnly), TimeAsTimeOnly },
                { typeof(TimeSpan), TimeAsTimeSpan },
                { typeof(JsonElement), Json }
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
                { "bigint", new[] { Long } },
                { "binary varying", new[] { VariableLengthBinary } },
                { "binary", new[] { FixedLengthBinary } },
                { "bit", new[] { Bool } },
                { "char varying", new[] { VariableLengthAnsiString } },
                { "char varying(max)", new[] { VariableLengthMaxAnsiString } },
                { "char", new[] { FixedLengthAnsiString } },
                { "character varying", new[] { VariableLengthAnsiString } },
                { "character varying(max)", new[] { VariableLengthMaxAnsiString } },
                { "character", new[] { FixedLengthAnsiString } },
                { "date", new RelationalTypeMapping[] { DateAsDateOnly, DateAsDateTime } },
                { "datetime", new[] { Datetime } },
                { "datetime2", new[] { Datetime2 } },
                { "datetimeoffset", new[] { Datetimeoffset } },
                { "dec", new[] { Decimal } },
                { "decimal", new[] { Decimal } },
                { "double precision", new[] { Double } },
                { "float", new[] { Double } },
                { "image", new[] { ImageBinary } },
                { "int", new[] { Int } },
                { "money", new[] { Money } },
                { "national char varying", new[] { VariableLengthUnicodeString } },
                { "national char varying(max)", new[] { VariableLengthMaxUnicodeString } },
                { "national character varying", new[] { VariableLengthUnicodeString } },
                { "national character varying(max)", new[] { VariableLengthMaxUnicodeString } },
                { "national character", new[] { FixedLengthUnicodeString } },
                { "nchar", new[] { FixedLengthUnicodeString } },
                { "ntext", new[] { TextUnicodeString } },
                { "numeric", new[] { Decimal } },
                { "nvarchar", new[] { VariableLengthUnicodeString } },
                { "nvarchar(max)", new[] { VariableLengthMaxUnicodeString } },
                { "real", new[] { Real } },
                { "rowversion", new[] { Rowversion } },
                { "smalldatetime", new[] { SmallDatetime } },
                { "smallint", new[] { Short } },
                { "smallmoney", new[] { SmallMoney } },
                { "sql_variant", new[] { SqlVariant } },
                { "text", new[] { TextAnsiString } },
                { "time", new RelationalTypeMapping[] { TimeAsTimeOnly, TimeAsTimeSpan } },
                { "timestamp", new[] { Rowversion } },
                { "tinyint", new[] { Byte } },
                { "uniqueidentifier", new[] { Uniqueidentifier } },
                { "varbinary", new[] { VariableLengthBinary } },
                { "varbinary(max)", new[] { VariableLengthMaxBinary } },
                { "varchar", new[] { VariableLengthAnsiString } },
                { "varchar(max)", new[] { VariableLengthMaxAnsiString } },
                { "xml", new[] { Xml } }
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
        => base.FindMapping(mappingInfo)
            ?? FindRawMapping(mappingInfo)?.Clone(mappingInfo);

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
                return Real;
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
                    useKeyComparison: mappingInfo.IsKeyOrIndex);
            }

            if (clrType == typeof(byte[]))
            {
                if (mappingInfo.IsRowVersion == true)
                {
                    return Rowversion;
                }

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
