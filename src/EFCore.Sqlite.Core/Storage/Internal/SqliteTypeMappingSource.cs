// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteTypeMappingSource : RelationalTypeMappingSource
{
    private static readonly HashSet<string> SpatialiteTypes
        = new(StringComparer.OrdinalIgnoreCase)
        {
            "GEOMETRY",
            "GEOMETRYZ",
            "GEOMETRYM",
            "GEOMETRYZM",
            "GEOMETRYCOLLECTION",
            "GEOMETRYCOLLECTIONZ",
            "GEOMETRYCOLLECTIONM",
            "GEOMETRYCOLLECTIONZM",
            "LINESTRING",
            "LINESTRINGZ",
            "LINESTRINGM",
            "LINESTRINGZM",
            "MULTILINESTRING",
            "MULTILINESTRINGZ",
            "MULTILINESTRINGM",
            "MULTILINESTRINGZM",
            "MULTIPOINT",
            "MULTIPOINTZ",
            "MULTIPOINTM",
            "MULTIPOINTZM",
            "MULTIPOLYGON",
            "MULTIPOLYGONZ",
            "MULTIPOLYGONM",
            "MULTIPOLYGONZM",
            "POINT",
            "POINTZ",
            "POINTM",
            "POINTZM",
            "POLYGON",
            "POLYGONZ",
            "POLYGONM",
            "POLYGONZM"
        };

    private const string IntegerTypeName = "INTEGER";
    private const string RealTypeName = "REAL";
    private const string BlobTypeName = "BLOB";
    private const string TextTypeName = "TEXT";

    private static readonly LongTypeMapping Integer = new(IntegerTypeName);
    private static readonly DoubleTypeMapping Real = new(RealTypeName);
    private static readonly ByteArrayTypeMapping Blob = new(BlobTypeName);
    private static readonly SqliteStringTypeMapping Text = new(TextTypeName);

    private readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings = new()
    {
        { typeof(string), Text },
        { typeof(byte[]), Blob },
        { typeof(bool), new BoolTypeMapping(IntegerTypeName) },
        { typeof(byte), new ByteTypeMapping(IntegerTypeName) },
        { typeof(char), new CharTypeMapping(TextTypeName) },
        { typeof(int), new IntTypeMapping(IntegerTypeName) },
        { typeof(long), Integer },
        { typeof(sbyte), new SByteTypeMapping(IntegerTypeName) },
        { typeof(short), new ShortTypeMapping(IntegerTypeName) },
        { typeof(uint), new UIntTypeMapping(IntegerTypeName) },
        { typeof(ulong), new SqliteULongTypeMapping(IntegerTypeName) },
        { typeof(ushort), new UShortTypeMapping(IntegerTypeName) },
        { typeof(DateTime), new SqliteDateTimeTypeMapping(TextTypeName) },
        { typeof(DateTimeOffset), new SqliteDateTimeOffsetTypeMapping(TextTypeName) },
        { typeof(TimeSpan), new TimeSpanTypeMapping(TextTypeName) },
        { typeof(DateOnly), new SqliteDateOnlyTypeMapping(TextTypeName) },
        { typeof(TimeOnly), new SqliteTimeOnlyTypeMapping(TextTypeName) },
        { typeof(decimal), new SqliteDecimalTypeMapping(TextTypeName) },
        { typeof(double), Real },
        { typeof(float), new FloatTypeMapping(RealTypeName) },
        { typeof(Guid), new SqliteGuidTypeMapping(TextTypeName) },
        { typeof(JsonElement), new SqliteJsonTypeMapping(TextTypeName) }
    };

    private readonly Dictionary<string, RelationalTypeMapping> _storeTypeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { IntegerTypeName, Integer },
        { RealTypeName, Real },
        { BlobTypeName, Blob },
        { TextTypeName, Text }
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteTypeMappingSource(
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
    public static bool IsSpatialiteType(string columnType)
        => SpatialiteTypes.Contains(columnType);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var mapping = base.FindMapping(mappingInfo) ?? FindRawMapping(mappingInfo);

        return mapping != null
            && mappingInfo.StoreTypeName != null
                ? mapping.Clone(mappingInfo.StoreTypeName, null)
                : mapping;
    }

    private RelationalTypeMapping? FindRawMapping(RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        if (clrType != null
            && _clrTypeMappings.TryGetValue(clrType, out var mapping))
        {
            return mapping;
        }

        var storeTypeName = mappingInfo.StoreTypeName;
        if (storeTypeName != null
            && _storeTypeMappings.TryGetValue(storeTypeName, out mapping)
            && (clrType == null || mapping.ClrType.UnwrapNullableType() == clrType))
        {
            return mapping;
        }

        if (storeTypeName != null)
        {
            var affinityTypeMapping = _typeRules.Select(r => r(storeTypeName)).FirstOrDefault(r => r != null);

            if (affinityTypeMapping != null)
            {
                return clrType == null || affinityTypeMapping.ClrType.UnwrapNullableType() == clrType
                    ? affinityTypeMapping
                    : null;
            }

            if (clrType == null || clrType == typeof(byte[]))
            {
                return Blob;
            }
        }

        return null;
    }

    private readonly Func<string, RelationalTypeMapping?>[] _typeRules =
    {
        name => Contains(name, "INT")
            ? Integer
            : null,
        name => Contains(name, "CHAR")
            || Contains(name, "CLOB")
            || Contains(name, "TEXT")
                ? Text
                : null,
        name => Contains(name, "BLOB")
            || Contains(name, "BIN")
                ? Blob
                : null,
        name => Contains(name, "REAL")
            || Contains(name, "FLOA")
            || Contains(name, "DOUB")
                ? Real
                : null
    };

    private static bool Contains(string haystack, string needle)
        => haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0;
}
