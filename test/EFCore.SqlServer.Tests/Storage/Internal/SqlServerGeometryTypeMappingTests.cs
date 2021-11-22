// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

public class SqlServerGeometryTypeMappingTests
{
    [ConditionalFact]
    public void GenerateSqlLiteral_works()
    {
        Assert.Equal("geometry::Parse('POINT (1 2)')", Literal(new Point(1, 2), "geometry"));
        Assert.Equal("geometry::STGeomFromText('POINT (1 2)', 4326)", Literal(new Point(1, 2) { SRID = 4326 }, "geometry"));
        Assert.Equal("geography::Parse('POINT (1 2)')", Literal(new Point(1, 2) { SRID = 4326 }, "geography"));
        Assert.Equal("geography::STGeomFromText('POINT (1 2)', 0)", Literal(new Point(1, 2), "geography"));
        Assert.Equal("geometry::Parse('POINT (1 2 3)')", Literal(new Point(1, 2, 3), "geometry"));
        Assert.Equal("geometry::Parse('POINT (1 2 3 4)')", Literal(new Point(new CoordinateZM(1, 2, 3, 4)), "geometry"));
        Assert.Equal("geometry::Parse('POINT (1 2 NULL 3)')", Literal(new Point(new CoordinateM(1, 2, 3)), "geometry"));
    }

    private static string Literal(Geometry value, string storeType)
        => new SqlServerGeometryTypeMapping<Geometry>(NtsGeometryServices.Instance, storeType)
            .GenerateSqlLiteral(value);
}
