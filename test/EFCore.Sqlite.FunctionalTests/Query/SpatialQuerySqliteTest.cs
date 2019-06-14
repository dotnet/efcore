// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    [SpatialiteRequired]
    public class SpatialQuerySqliteTest : SpatialQueryTestBase<SpatialQuerySqliteFixture>
    {
        public SpatialQuerySqliteTest(SpatialQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task SimpleSelect(bool isAsync)
        {
            await base.SimpleSelect(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", ""p"".""Geometry"", ""p"".""Point""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task Distance_on_converted_geometry_type(bool isAsync)
        {
            await base.Distance_on_converted_geometry_type(isAsync);

            AssertSql(
                @"@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Nullable = false) (Size = 60) (DbType = String)

SELECT ""g"".""Id"", Distance(""g"".""Location"", @__point_0) AS ""Distance""
FROM ""GeoPointEntity"" AS ""g""");
        }

        public override async Task Distance_on_converted_geometry_type_lhs(bool isAsync)
        {
            await base.Distance_on_converted_geometry_type_lhs(isAsync);

            AssertSql(
                @"@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Nullable = false) (Size = 60) (DbType = String)

SELECT ""g"".""Id"", Distance(@__point_0, ""g"".""Location"") AS ""Distance""
FROM ""GeoPointEntity"" AS ""g""");
        }

        public override async Task Distance_on_converted_geometry_type_constant(bool isAsync)
        {
            await base.Distance_on_converted_geometry_type_constant(isAsync);

            AssertSql(
                @"SELECT ""g"".""Id"", Distance(""g"".""Location"", GeomFromText('POINT (0 1)')) AS ""Distance""
FROM ""GeoPointEntity"" AS ""g""");
        }

        public override async Task Distance_on_converted_geometry_type_constant_lhs(bool isAsync)
        {
            await base.Distance_on_converted_geometry_type_constant_lhs(isAsync);

            AssertSql(
                @"SELECT ""g"".""Id"", Distance(GeomFromText('POINT (0 1)'), ""g"".""Location"") AS ""Distance""
FROM ""GeoPointEntity"" AS ""g""");
        }

        public override async Task WithConversion(bool isAsync)
        {
            await base.WithConversion(isAsync);

            AssertSql(
                @"SELECT ""g"".""Id"", ""g"".""Location""
FROM ""GeoPointEntity"" AS ""g""");
        }

        public override async Task Area(bool isAsync)
        {
            await base.Area(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", Area(""p"".""Polygon"") AS ""Area""
FROM ""PolygonEntity"" AS ""p""");
        }

        public override async Task AsBinary(bool isAsync)
        {
            await base.AsBinary(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", AsBinary(""e"".""Point"") AS ""Binary""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task AsText(bool isAsync)
        {
            await base.AsText(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", AsText(""e"".""Point"") AS ""Text""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task Boundary(bool isAsync)
        {
            await base.Boundary(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", Boundary(""p"".""Polygon"") AS ""Boundary""
FROM ""PolygonEntity"" AS ""p""");
        }

        public override async Task Buffer(bool isAsync)
        {
            await base.Buffer(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", Buffer(""e"".""Polygon"", 1.0) AS ""Buffer""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Buffer_quadrantSegments(bool isAsync)
        {
            await base.Buffer_quadrantSegments(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", Buffer(""e"".""Polygon"", 1.0, 8) AS ""Buffer""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Centroid(bool isAsync)
        {
            await base.Centroid(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", Centroid(""p"".""Polygon"") AS ""Centroid""
FROM ""PolygonEntity"" AS ""p""");
        }

        public override async Task Contains(bool isAsync)
        {
            await base.Contains(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0x000100000000000000000000D03F000000000000D03F000000000000D03F0000...' (Size = 60) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Polygon"" IS NOT NULL THEN Contains(""e"".""Polygon"", @__point_0)
//END AS ""Contains""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task ConvexHull(bool isAsync)
        {
            await base.ConvexHull(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", ConvexHull(""e"".""Polygon"") AS ""ConvexHull""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task IGeometryCollection_Count(bool isAsync)
        {
            await base.IGeometryCollection_Count(isAsync);

            AssertSql(
                @"SELECT ""m"".""Id"", NumGeometries(""m"".""MultiLineString"") AS ""Count""
FROM ""MultiLineStringEntity"" AS ""m""");
        }

        public override async Task LineString_Count(bool isAsync)
        {
            await base.LineString_Count(isAsync);

            AssertSql(
                @"SELECT ""l"".""Id"", NumPoints(""l"".""LineString"") AS ""Count""
FROM ""LineStringEntity"" AS ""l""");
        }

        public override async Task CoveredBy(bool isAsync)
        {
            await base.CoveredBy(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x000100000000000000000000F0BF000000000000F0BF00000000000000400000...' (Size = 132) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Point"" IS NOT NULL THEN CoveredBy(""e"".""Point"", @__polygon_0)
//END AS ""CoveredBy""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task Covers(bool isAsync)
        {
            await base.Covers(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0x000100000000000000000000D03F000000000000D03F000000000000D03F0000...' (Size = 60) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Polygon"" IS NOT NULL THEN Covers(""e"".""Polygon"", @__point_0)
//END AS ""Covers""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Crosses(bool isAsync)
        {
            await base.Crosses(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__lineString_0='0x000100000000000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 80) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""LineString"" IS NOT NULL THEN Crosses(""e"".""LineString"", @__lineString_0)
//END AS ""Crosses""
//FROM ""LineStringEntity"" AS ""e""");
        }

        public override async Task Difference(bool isAsync)
        {
            await base.Difference(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

//SELECT ""e"".""Id"", Difference(""e"".""Polygon"", @__polygon_0) AS ""Difference""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Dimension(bool isAsync)
        {
            await base.Dimension(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", Dimension(""p"".""Point"") AS ""Dimension""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task Disjoint(bool isAsync)
        {
            await base.Disjoint(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0x000100000000000000000000F03F000000000000F03F000000000000F03F0000...' (Size = 60) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Polygon"" IS NOT NULL THEN Disjoint(""e"".""Polygon"", @__point_0)
//END AS ""Disjoint""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Distance(bool isAsync)
        {
            await base.Distance(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Size = 60) (DbType = String)

//SELECT ""e"".""Id"", Distance(""e"".""Point"", @__point_0) AS ""Distance""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task Distance_geometry(bool isAsync)
        {
            await base.Distance_geometry(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Size = 60) (DbType = String)

//SELECT ""e"".""Id"", Distance(""e"".""Geometry"", @__point_0) AS ""Distance""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task Distance_constant(bool isAsync)
        {
            await base.Distance_constant(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", Distance(""e"".""Point"", GeomFromText('POINT (0 1)')) AS ""Distance""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task Distance_constant_srid_4326(bool isAsync)
        {
            await base.Distance_constant_srid_4326(isAsync);

            // isse #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", Distance(""e"".""Point"", GeomFromText('POINT (1 1)', 4326)) AS ""Distance""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task Distance_constant_lhs(bool isAsync)
        {
            await base.Distance_constant_lhs(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", Distance(GeomFromText('POINT (0 1)'), ""e"".""Point"") AS ""Distance""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task EndPoint(bool isAsync)
        {
            await base.EndPoint(isAsync);

            AssertSql(
                @"SELECT ""l"".""Id"", EndPoint(""l"".""LineString"") AS ""EndPoint""
FROM ""LineStringEntity"" AS ""l""");
        }

        public override async Task Envelope(bool isAsync)
        {
            await base.Envelope(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", Envelope(""p"".""Polygon"") AS ""Envelope""
FROM ""PolygonEntity"" AS ""p""");
        }

        public override async Task EqualsTopologically(bool isAsync)
        {
            await base.EqualsTopologically(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0x0001000000000000000000000000000000000000000000000000000000000000...' (Size = 60) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Point"" IS NOT NULL THEN Equals(""e"".""Point"", @__point_0)
//END AS ""EqualsTopologically""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task ExteriorRing(bool isAsync)
        {
            await base.ExteriorRing(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", ExteriorRing(""p"".""Polygon"") AS ""ExteriorRing""
FROM ""PolygonEntity"" AS ""p""");
        }

        public override async Task GeometryType(bool isAsync)
        {
            await base.GeometryType(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", CASE rtrim(GeometryType(""p"".""Point""), ' ZM')
    WHEN 'POINT' THEN 'Point'
    WHEN 'LINESTRING' THEN 'LineString'
    WHEN 'POLYGON' THEN 'Polygon'
    WHEN 'MULTIPOINT' THEN 'MultiPoint'
    WHEN 'MULTILINESTRING' THEN 'MultiLineString'
    WHEN 'MULTIPOLYGON' THEN 'MultiPolygon'
    WHEN 'GEOMETRYCOLLECTION' THEN 'GeometryCollection'
END AS ""GeometryType""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task GetGeometryN(bool isAsync)
        {
            await base.GetGeometryN(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", GeometryN(""e"".""MultiLineString"", 0 + 1) AS ""Geometry0""
//FROM ""MultiLineStringEntity"" AS ""e""");
        }

        public override async Task GetInteriorRingN(bool isAsync)
        {
            await base.GetInteriorRingN(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Polygon"" IS NULL OR (NumInteriorRing(""e"".""Polygon"") = 0)
//    THEN NULL ELSE InteriorRingN(""e"".""Polygon"", 0 + 1)
//END AS ""InteriorRing0""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task GetPointN(bool isAsync)
        {
            await base.GetPointN(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", PointN(""e"".""LineString"", 0 + 1) AS ""Point0""
//FROM ""LineStringEntity"" AS ""e""");
        }

        public override async Task InteriorPoint(bool isAsync)
        {
            await base.InteriorPoint(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", PointOnSurface(""p"".""Polygon"") AS ""InteriorPoint"", ""p"".""Polygon""
FROM ""PolygonEntity"" AS ""p""");
        }

        public override async Task Intersection(bool isAsync)
        {
            await base.Intersection(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

//SELECT ""e"".""Id"", Intersection(""e"".""Polygon"", @__polygon_0) AS ""Intersection""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Intersects(bool isAsync)
        {
            await base.Intersects(isAsync);

            // issue 16050
//            AssertSql(
//                @"@__lineString_0='0x000100000000000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 80) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""LineString"" IS NOT NULL THEN Intersects(""e"".""LineString"", @__lineString_0)
//END AS ""Intersects""
//FROM ""LineStringEntity"" AS ""e""");
        }

        public override async Task ICurve_IsClosed(bool isAsync)
        {
            await base.ICurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT ""l"".""Id"", CASE
    WHEN ""l"".""LineString"" IS NOT NULL THEN IsClosed(""l"".""LineString"")
END AS ""IsClosed""
FROM ""LineStringEntity"" AS ""l""");
        }

        public override async Task IMultiCurve_IsClosed(bool isAsync)
        {
            await base.IMultiCurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT ""m"".""Id"", CASE
    WHEN ""m"".""MultiLineString"" IS NOT NULL THEN IsClosed(""m"".""MultiLineString"")
END AS ""IsClosed""
FROM ""MultiLineStringEntity"" AS ""m""");
        }

        public override async Task IsEmpty(bool isAsync)
        {
            await base.IsEmpty(isAsync);

            AssertSql(
                @"SELECT ""m"".""Id"", CASE
    WHEN ""m"".""MultiLineString"" IS NOT NULL THEN IsEmpty(""m"".""MultiLineString"")
END AS ""IsEmpty""
FROM ""MultiLineStringEntity"" AS ""m""");
        }

        public override async Task IsRing(bool isAsync)
        {
            await base.IsRing(isAsync);

            AssertSql(
                @"SELECT ""l"".""Id"", CASE
    WHEN ""l"".""LineString"" IS NOT NULL THEN IsRing(""l"".""LineString"")
END AS ""IsRing""
FROM ""LineStringEntity"" AS ""l""");
        }

        public override async Task IsSimple(bool isAsync)
        {
            await base.IsSimple(isAsync);

            AssertSql(
                @"SELECT ""l"".""Id"", CASE
    WHEN ""l"".""LineString"" IS NOT NULL THEN IsSimple(""l"".""LineString"")
END AS ""IsSimple""
FROM ""LineStringEntity"" AS ""l""");
        }

        public override async Task IsValid(bool isAsync)
        {
            await base.IsValid(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", CASE
    WHEN ""p"".""Point"" IS NOT NULL THEN IsValid(""p"".""Point"")
END AS ""IsValid""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task IsWithinDistance(bool isAsync)
        {
            await base.IsWithinDistance(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0x0001000000000000000000000000000000000000F03F00000000000000000000...' (Size = 60) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN Distance(""e"".""Point"", @__point_0) <= 1.0
//    THEN 1 ELSE 0
//END AS ""IsWithinDistance""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task Item(bool isAsync)
        {
            await base.Item(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", GeometryN(""e"".""MultiLineString"", 0 + 1) AS ""Item0""
//FROM ""MultiLineStringEntity"" AS ""e""");
        }

        public override async Task Length(bool isAsync)
        {
            await base.Length(isAsync);

            AssertSql(
                @"SELECT ""l"".""Id"", GLength(""l"".""LineString"") AS ""Length""
FROM ""LineStringEntity"" AS ""l""");
        }

        public override async Task M(bool isAsync)
        {
            await base.M(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", M(""p"".""Point"") AS ""M""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task NumGeometries(bool isAsync)
        {
            await base.NumGeometries(isAsync);

            AssertSql(
                @"SELECT ""m"".""Id"", NumGeometries(""m"".""MultiLineString"") AS ""NumGeometries""
FROM ""MultiLineStringEntity"" AS ""m""");
        }

        public override async Task NumInteriorRings(bool isAsync)
        {
            await base.NumInteriorRings(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", NumInteriorRing(""p"".""Polygon"") AS ""NumInteriorRings""
FROM ""PolygonEntity"" AS ""p""");
        }

        public override async Task NumPoints(bool isAsync)
        {
            await base.NumPoints(isAsync);

            AssertSql(
                @"SELECT ""l"".""Id"", NumPoints(""l"".""LineString"") AS ""NumPoints""
FROM ""LineStringEntity"" AS ""l""");
        }

        public override async Task OgcGeometryType(bool isAsync)
        {
            await base.OgcGeometryType(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", CASE rtrim(GeometryType(""p"".""Point""), ' ZM')
    WHEN 'POINT' THEN 1
    WHEN 'LINESTRING' THEN 2
    WHEN 'POLYGON' THEN 3
    WHEN 'MULTIPOINT' THEN 4
    WHEN 'MULTILINESTRING' THEN 5
    WHEN 'MULTIPOLYGON' THEN 6
    WHEN 'GEOMETRYCOLLECTION' THEN 7
END AS ""OgcGeometryType""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task Overlaps(bool isAsync)
        {
            await base.Overlaps(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Polygon"" IS NOT NULL THEN Overlaps(""e"".""Polygon"", @__polygon_0)
//END AS ""Overlaps""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task PointOnSurface(bool isAsync)
        {
            await base.PointOnSurface(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", PointOnSurface(""p"".""Polygon"") AS ""PointOnSurface"", ""p"".""Polygon""
FROM ""PolygonEntity"" AS ""p""");
        }

        public override async Task Relate(bool isAsync)
        {
            await base.Relate(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Polygon"" IS NOT NULL THEN Relate(""e"".""Polygon"", @__polygon_0, '212111212')
//END AS ""Relate""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Reverse(bool isAsync)
        {
            await base.Reverse(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", ST_Reverse(""e"".""LineString"") AS ""Reverse""
//FROM ""LineStringEntity"" AS ""e""");
        }

        public override async Task SRID(bool isAsync)
        {
            await base.SRID(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", SRID(""p"".""Point"") AS ""SRID""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task SRID_geometry(bool isAsync)
        {
            await base.SRID_geometry(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", SRID(""p"".""Geometry"") AS ""SRID""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task StartPoint(bool isAsync)
        {
            await base.StartPoint(isAsync);

            AssertSql(
                @"SELECT ""l"".""Id"", StartPoint(""l"".""LineString"") AS ""StartPoint""
FROM ""LineStringEntity"" AS ""l""");
        }

        public override async Task SymmetricDifference(bool isAsync)
        {
            await base.SymmetricDifference(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

//SELECT ""e"".""Id"", SymDifference(""e"".""Polygon"", @__polygon_0) AS ""SymmetricDifference""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task ToBinary(bool isAsync)
        {
            await base.ToBinary(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", AsBinary(""e"".""Point"") AS ""Binary""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task ToText(bool isAsync)
        {
            await base.ToText(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", AsText(""e"".""Point"") AS ""Text""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task Touches(bool isAsync)
        {
            await base.Touches(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Polygon"" IS NOT NULL THEN Touches(""e"".""Polygon"", @__polygon_0)
//END AS ""Touches""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Union(bool isAsync)
        {
            await base.Union(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x00010000000000000000000000000000000000000000000000000000F03F0000...' (Size = 116) (DbType = String)

//SELECT ""e"".""Id"", GUnion(""e"".""Polygon"", @__polygon_0) AS ""Union""
//FROM ""PolygonEntity"" AS ""e""");
        }

        public override async Task Union_void(bool isAsync)
        {
            await base.Union_void(isAsync);

            // issue #16050
//            AssertSql(
//                @"SELECT ""e"".""Id"", UnaryUnion(""e"".""MultiLineString"") AS ""Union""
//FROM ""MultiLineStringEntity"" AS ""e""");
        }

        public override async Task Within(bool isAsync)
        {
            await base.Within(isAsync);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0x000100000000000000000000F0BF000000000000F0BF00000000000000400000...' (Size = 132) (DbType = String)

//SELECT ""e"".""Id"", CASE
//    WHEN ""e"".""Point"" IS NOT NULL THEN Within(""e"".""Point"", @__polygon_0)
//END AS ""Within""
//FROM ""PointEntity"" AS ""e""");
        }

        public override async Task X(bool isAsync)
        {
            await base.X(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", X(""p"".""Point"") AS ""X""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task Y(bool isAsync)
        {
            await base.Y(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", Y(""p"".""Point"") AS ""Y""
FROM ""PointEntity"" AS ""p""");
        }

        public override async Task Z(bool isAsync)
        {
            await base.Z(isAsync);

            AssertSql(
                @"SELECT ""p"".""Id"", Z(""p"".""Point"") AS ""Z""
FROM ""PointEntity"" AS ""p""");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
