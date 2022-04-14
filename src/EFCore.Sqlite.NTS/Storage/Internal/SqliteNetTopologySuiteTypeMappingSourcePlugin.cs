// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteNetTopologySuiteTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
{
    private static readonly Dictionary<string, Type> StoreTypeMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "GEOMETRY", typeof(Geometry) },
        { "GEOMETRYZ", typeof(Geometry) },
        { "GEOMETRYM", typeof(Geometry) },
        { "GEOMETRYZM", typeof(Geometry) },
        { "GEOMETRYCOLLECTION", typeof(GeometryCollection) },
        { "GEOMETRYCOLLECTIONZ", typeof(GeometryCollection) },
        { "GEOMETRYCOLLECTIONM", typeof(GeometryCollection) },
        { "GEOMETRYCOLLECTIONZM", typeof(GeometryCollection) },
        { "LINESTRING", typeof(LineString) },
        { "LINESTRINGZ", typeof(LineString) },
        { "LINESTRINGM", typeof(LineString) },
        { "LINESTRINGZM", typeof(LineString) },
        { "MULTILINESTRING", typeof(MultiLineString) },
        { "MULTILINESTRINGZ", typeof(MultiLineString) },
        { "MULTILINESTRINGM", typeof(MultiLineString) },
        { "MULTILINESTRINGZM", typeof(MultiLineString) },
        { "MULTIPOINT", typeof(MultiPoint) },
        { "MULTIPOINTZ", typeof(MultiPoint) },
        { "MULTIPOINTM", typeof(MultiPoint) },
        { "MULTIPOINTZM", typeof(MultiPoint) },
        { "MULTIPOLYGON", typeof(MultiPolygon) },
        { "MULTIPOLYGONZ", typeof(MultiPolygon) },
        { "MULTIPOLYGONM", typeof(MultiPolygon) },
        { "MULTIPOLYGONZM", typeof(MultiPolygon) },
        { "POINT", typeof(Point) },
        { "POINTZ", typeof(Point) },
        { "POINTM", typeof(Point) },
        { "POINTZM", typeof(Point) },
        { "POLYGON", typeof(Polygon) },
        { "POLYGONZ", typeof(Polygon) },
        { "POLYGONM", typeof(Polygon) },
        { "POLYGONZM", typeof(Polygon) }
    };

    private readonly NtsGeometryServices _geometryServices;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteNetTopologySuiteTypeMappingSourcePlugin(NtsGeometryServices geometryServices)
    {
        _geometryServices = geometryServices;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;
        string? defaultStoreType = null;
        Type? defaultClrType = null;

        return (clrType != null
                && TryGetDefaultStoreType(clrType, out defaultStoreType))
            || (storeTypeName != null
                && StoreTypeMappings.TryGetValue(storeTypeName, out defaultClrType))
                ? (RelationalTypeMapping)Activator.CreateInstance(
                    typeof(SqliteGeometryTypeMapping<>).MakeGenericType(clrType ?? defaultClrType ?? typeof(Geometry)),
                    _geometryServices,
                    storeTypeName ?? defaultStoreType ?? "GEOMETRY")!
                : null;
    }

    private static bool TryGetDefaultStoreType(Type type, out string? defaultStoreType)
    {
        if (typeof(LineString).IsAssignableFrom(type))
        {
            defaultStoreType = "LINESTRING";
        }
        else if (typeof(MultiLineString).IsAssignableFrom(type))
        {
            defaultStoreType = "MULTILINESTRING";
        }
        else if (typeof(MultiPoint).IsAssignableFrom(type))
        {
            defaultStoreType = "MULTIPOINT";
        }
        else if (typeof(MultiPolygon).IsAssignableFrom(type))
        {
            defaultStoreType = "MULTIPOLYGON";
        }
        else if (typeof(Point).IsAssignableFrom(type))
        {
            defaultStoreType = "POINT";
        }
        else if (typeof(Polygon).IsAssignableFrom(type))
        {
            defaultStoreType = "POLYGON";
        }
        else if (typeof(GeometryCollection).IsAssignableFrom(type))
        {
            defaultStoreType = "GEOMETRYCOLLECTION";
        }
        else if (typeof(Geometry).IsAssignableFrom(type))
        {
            defaultStoreType = "GEOMETRY";
        }
        else
        {
            defaultStoreType = null;
        }

        return defaultStoreType != null;
    }
}
