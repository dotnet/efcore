// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NetTopologySuite;
using NetTopologySuite.Geometries;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal
{
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
}
