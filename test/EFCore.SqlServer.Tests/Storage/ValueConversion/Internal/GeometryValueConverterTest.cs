// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.SqlTypes;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.ValueConversion.Internal;

public class GeometryValueConverterTest
{
    // The IsValid flag is at bit 2 (0x04) of the Properties byte at offset 5 in the SQL Server
    // geography/geometry binary format (MS-SSCLRT).
    private const int PropertiesByteIndex = 5;
    private const byte IsValidFlag = 0x04;

    [ConditionalFact]
    public void IsValid_flag_set_for_valid_geometry()
    {
        var point = new Point(1, 2) { SRID = 4326 };
        Assert.True(point.IsValid);

        var converter = CreateConverter(isGeography: false);
        var sqlBytes = (SqlBytes)converter.ConvertToProvider(point)!;

        Assert.True((sqlBytes.Value[PropertiesByteIndex] & IsValidFlag) != 0, "IsValid flag should be set for valid geometry");
    }

    [ConditionalFact]
    public void IsValid_flag_set_for_valid_geography()
    {
        var point = new Point(1, 2) { SRID = 4326 };
        Assert.True(point.IsValid);

        var converter = CreateConverter(isGeography: true);
        var sqlBytes = (SqlBytes)converter.ConvertToProvider(point)!;

        Assert.True((sqlBytes.Value[PropertiesByteIndex] & IsValidFlag) != 0, "IsValid flag should be set for valid geography");
    }

    [ConditionalFact]
    public void IsValid_flag_set_for_invalid_geometry()
    {
        // Create an invalid geometry (self-intersecting polygon - bowtie shape)
        var polygon = new Polygon(
            new LinearRing(
            [
                new Coordinate(0, 0),
                new Coordinate(2, 2),
                new Coordinate(2, 0),
                new Coordinate(0, 2),
                new Coordinate(0, 0)
            ]));
        Assert.False(polygon.IsValid);

        var converter = CreateConverter(isGeography: false);
        var sqlBytes = (SqlBytes)converter.ConvertToProvider(polygon)!;

        Assert.True(
            (sqlBytes.Value[PropertiesByteIndex] & IsValidFlag) != 0,
            "IsValid flag should be set even for NTS-invalid geometry");
    }

    [ConditionalFact]
    public void IsValid_flag_set_for_invalid_geography()
    {
        // Create an invalid geography (self-intersecting polygon - bowtie shape)
        var polygon = new Polygon(
            new LinearRing(
            [
                new Coordinate(0, 0),
                new Coordinate(2, 2),
                new Coordinate(2, 0),
                new Coordinate(0, 2),
                new Coordinate(0, 0)
            ])) { SRID = 4326 };
        Assert.False(polygon.IsValid);

        var converter = CreateConverter(isGeography: true);
        var sqlBytes = (SqlBytes)converter.ConvertToProvider(polygon)!;

        Assert.True(
            (sqlBytes.Value[PropertiesByteIndex] & IsValidFlag) != 0,
            "IsValid flag should be set even for NTS-invalid geography");
    }

    [ConditionalFact]
    public void Roundtrip_preserves_geometry_data()
    {
        var point = new Point(1, 2) { SRID = 4326 };

        var converter = CreateConverter(isGeography: false);
        var sqlBytes = (SqlBytes)converter.ConvertToProvider(point)!;
        var roundtripped = (Point)converter.ConvertFromProvider(sqlBytes)!;

        Assert.Equal(point.X, roundtripped.X);
        Assert.Equal(point.Y, roundtripped.Y);
        Assert.Equal(point.SRID, roundtripped.SRID);
    }

    private static GeometryValueConverter<Geometry> CreateConverter(bool isGeography)
    {
        var reader = new SqlServerBytesReader(NtsGeometryServices.Instance) { IsGeography = isGeography };
        var writer = new SqlServerBytesWriter { IsGeography = isGeography };
        return new GeometryValueConverter<Geometry>(reader, writer);
    }
}
