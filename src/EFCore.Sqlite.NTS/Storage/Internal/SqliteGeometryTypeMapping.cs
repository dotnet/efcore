// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Json;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.ValueConversion.Internal;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteGeometryTypeMapping<TGeometry> : RelationalGeometryTypeMapping<TGeometry, byte[]>
    where TGeometry : Geometry
{
    private static readonly MethodInfo _getBytes
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFieldValue), [typeof(int)])!
            .MakeGenericMethod(typeof(byte[]));

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [UsedImplicitly]
    public SqliteGeometryTypeMapping(NtsGeometryServices geometryServices, string storeType)
        : base(
            new GeometryValueConverter<TGeometry>(CreateReader(geometryServices), CreateWriter(storeType)),
            storeType,
            SqliteJsonGeometryWktReaderWriter.Instance)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected SqliteGeometryTypeMapping(
        RelationalTypeMappingParameters parameters,
        ValueConverter<TGeometry, byte[]>? converter)
        : base(parameters, converter)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SqliteGeometryTypeMapping<TGeometry>(parameters, SpatialConverter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var builder = new StringBuilder();
        var geometry = (Geometry)value;

        builder
            .Append("GeomFromText('")
            .Append(geometry.AsText())
            .Append('\'');

        if (geometry.SRID != 0)
        {
            builder
                .Append(", ")
                .Append(geometry.SRID);
        }

        builder.Append(')');

        return builder.ToString();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override MethodInfo GetDataReaderMethod()
        => _getBytes;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override string AsText(object value)
        => ((Geometry)value).AsText();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override int GetSrid(object value)
        => ((Geometry)value).SRID;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Type WktReaderType
        => typeof(WKTReader);

    private static GaiaGeoReader CreateReader(NtsGeometryServices geometryServices)
        => new(geometryServices.DefaultCoordinateSequenceFactory, geometryServices.DefaultPrecisionModel);

    private static GaiaGeoWriter CreateWriter(string storeType)
    {
        var handleOrdinates = storeType.ToUpperInvariant() switch
        {
            "POINT" or "LINESTRING" or "POLYGON" or "MULTIPOINT" or "MULTILINESTRING" or "MULTIPOLYGON" or "GEOMETRYCOLLECTION"
                or "GEOMETRY" => Ordinates.XY,
            "POINTZ" or "LINESTRINGZ" or "POLYGONZ" or "MULTIPOINTZ" or "MULTILINESTRINGZ" or "MULTIPOLYGONZ" or "GEOMETRYCOLLECTIONZ"
                or "GEOMETRYZ" => Ordinates.XYZ,
            "POINTM" or "LINESTRINGM" or "POLYGONM" or "MULTIPOINTM" or "MULTILINESTRINGM" or "MULTIPOLYGONM" or "GEOMETRYCOLLECTIONM"
                or "GEOMETRYM" => Ordinates.XYM,
            "POINTZM" or "LINESTRINGZM" or "POLYGONZM" or "MULTIPOINTZM" or "MULTILINESTRINGZM" or "MULTIPOLYGONZM"
                or "GEOMETRYCOLLECTIONZM" or "GEOMETRYZM" => Ordinates.XYZM,
            _ => throw new ArgumentException(SqliteNTSStrings.InvalidGeometryType(storeType), nameof(storeType)),
        };
        return new GaiaGeoWriter { HandleOrdinates = handleOrdinates };
    }
}
