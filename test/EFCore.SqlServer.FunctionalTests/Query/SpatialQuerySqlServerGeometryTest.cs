// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using NetTopologySuite.Geometries;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SpatialQuerySqlServerGeometryTest : SpatialQueryTestBase<SpatialQuerySqlServerGeometryFixture>
    {
        public SpatialQuerySqlServerGeometryTest(SpatialQuerySqlServerGeometryFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task SimpleSelect(bool isAsync)
        {
            await base.SimpleSelect(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Geometry], [p].[Point]
FROM [PointEntity] AS [p]");
        }

        public override async Task WithConversion(bool isAsync)
        {
            await base.WithConversion(isAsync);

            AssertSql(
                @"SELECT [g].[Id], [g].[Location]
FROM [GeoPointEntity] AS [g]");
        }

        public override async Task Area(bool isAsync)
        {
            await base.Area(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STArea() AS [Area]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task AsBinary(bool isAsync)
        {
            await base.AsBinary(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].STAsBinary() AS [Binary]
//FROM [PointEntity] AS [e]");
        }

        public override async Task AsText(bool isAsync)
        {
            await base.AsText(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].AsTextZM() AS [Text]
//FROM [PointEntity] AS [e]");
        }

        public override async Task Boundary(bool isAsync)
        {
            await base.Boundary(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STBoundary() AS [Boundary]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task Buffer(bool isAsync)
        {
            await base.Buffer(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Polygon].STBuffer(1.0E0) AS [Buffer]
//FROM [PolygonEntity] AS [e]");
        }

        [ConditionalTheory(Skip = "No Server Translation.")]
        public override Task Buffer_quadrantSegments(bool isAsync)
        {
            return base.Buffer_quadrantSegments(isAsync);
        }

        public override async Task Centroid(bool isAsync)
        {
            await base.Centroid(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STCentroid() AS [Centroid]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task Contains(bool isAsync)
        {
            await base.Contains(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__point_0='0x00000000010C000000000000D03F000000000000D03F' (Size = 22) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STContains(@__point_0) AS [Contains]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task ConvexHull(bool isAsync)
        {
            await base.ConvexHull(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Polygon].STConvexHull() AS [ConvexHull]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task IGeometryCollection_Count(bool isAsync)
        {
            await base.IGeometryCollection_Count(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[MultiLineString].STNumGeometries() AS [Count]
FROM [MultiLineStringEntity] AS [m]");
        }

        public override async Task LineString_Count(bool isAsync)
        {
            await base.LineString_Count(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STNumPoints() AS [Count]
FROM [LineStringEntity] AS [l]");
        }

        [ConditionalTheory(Skip = "No Server Translation.")]
        public override Task CoveredBy(bool isAsync)
        {
            return base.CoveredBy(isAsync);
        }

        [ConditionalTheory(Skip = "No Server Translation.")]
        public override Task Covers(bool isAsync)
        {
            return base.Covers(isAsync);
        }

        public override async Task Crosses(bool isAsync)
        {
            await base.Crosses(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__lineString_0='0x000000000114000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 38) (DbType = Object)

//SELECT [e].[Id], [e].[LineString].STCrosses(@__lineString_0) AS [Crosses]
//FROM [LineStringEntity] AS [e]");
        }

        public override async Task Difference(bool isAsync)
        {
            await base.Difference(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__polygon_0='0x0000000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STDifference(@__polygon_0) AS [Difference]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task Distance_on_converted_geometry_type(bool isAsync)
        {
            await base.Distance_on_converted_geometry_type(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__point_0='0x00000000010C000000000000F03F0000000000000000' (Nullable = false) (Size = 22) (DbType = Object)

//SELECT [e].[Id], [e].[Location].STDistance(@__point_0) AS [Distance]
//FROM [GeoPointEntity] AS [e]");
        }

        public override async Task Distance_on_converted_geometry_type_lhs(bool isAsync)
        {
            await base.Distance_on_converted_geometry_type_lhs(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__point_0='0x00000000010C000000000000F03F0000000000000000' (Nullable = false) (Size = 22) (DbType = Object)

//SELECT [e].[Id], @__point_0.STDistance([e].[Location]) AS [Distance]
//FROM [GeoPointEntity] AS [e]");
        }

        public override async Task Distance_on_converted_geometry_type_constant(bool isAsync)
        {
            await base.Distance_on_converted_geometry_type_constant(isAsync);

            AssertSql(
                @"SELECT [g].[Id], [g].[Location].STDistance(geometry::Parse('POINT (0 1)')) AS [Distance]
FROM [GeoPointEntity] AS [g]");
        }

        public override async Task Distance_on_converted_geometry_type_constant_lhs(bool isAsync)
        {
            await base.Distance_on_converted_geometry_type_constant_lhs(isAsync);

            AssertSql(
                @"SELECT [g].[Id], geometry::Parse('POINT (0 1)').STDistance([g].[Location]) AS [Distance]
FROM [GeoPointEntity] AS [g]");
        }

        public override async Task Distance_constant(bool isAsync)
        {
            await base.Distance_constant(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].STDistance('POINT (0 1)') AS [Distance]
//FROM [PointEntity] AS [e]");
        }

        public override async Task Distance_constant_srid_4326(bool isAsync)
        {
            await AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Point == null ? (double?)null : e.Point.Distance(new Point(1, 1) { SRID = 4326 })
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Null(a.Distance);
                });

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].STDistance(geometry::STGeomFromText('POINT (1 1)', 4326)) AS [Distance]
//FROM [PointEntity] AS [e]");
        }

        public override async Task Distance_constant_lhs(bool isAsync)
        {
            await base.Distance_constant_lhs(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], geometry::Parse('POINT (0 1)').STDistance([e].[Point]) AS [Distance]
//FROM [PointEntity] AS [e]");
        }

        public override async Task Dimension(bool isAsync)
        {
            await base.Dimension(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STDimension() AS [Dimension]
FROM [PointEntity] AS [p]");
        }

        public override async Task Disjoint(bool isAsync)
        {
            await base.Disjoint(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__point_0='0x00000000010C000000000000F03F000000000000F03F' (Size = 22) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STDisjoint(@__point_0) AS [Disjoint]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task Distance(bool isAsync)
        {
            await base.Distance(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__point_0='0x00000000010C0000000000000000000000000000F03F' (Size = 22) (DbType = Object)

//SELECT [e].[Id], [e].[Point].STDistance(@__point_0) AS [Distance]
//FROM [PointEntity] AS [e]");
        }

        public override async Task Distance_geometry(bool isAsync)
        {
            await base.Distance_geometry(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__point_0='0x00000000010C0000000000000000000000000000F03F' (Size = 22) (DbType = Object)

//SELECT [e].[Id], [e].[Geometry].STDistance(@__point_0) AS [Distance]
//FROM [PointEntity] AS [e]");
        }

        public override async Task EndPoint(bool isAsync)
        {
            await base.EndPoint(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STEndPoint() AS [EndPoint]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task Envelope(bool isAsync)
        {
            await base.Envelope(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STEnvelope() AS [Envelope]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task EqualsTopologically(bool isAsync)
        {
            await base.EqualsTopologically(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__point_0='0x00000000010C00000000000000000000000000000000' (Size = 22) (DbType = Object)

//SELECT [e].[Id], [e].[Point].STEquals(@__point_0) AS [EqualsTopologically]
//FROM [PointEntity] AS [e]");
        }

        public override async Task ExteriorRing(bool isAsync)
        {
            await base.ExteriorRing(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STExteriorRing() AS [ExteriorRing]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task GeometryType(bool isAsync)
        {
            await base.GeometryType(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STGeometryType() AS [GeometryType]
FROM [PointEntity] AS [p]");
        }

        public override async Task GetGeometryN(bool isAsync)
        {
            await base.GetGeometryN(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[MultiLineString].STGeometryN(0 + 1) AS [Geometry0]
//FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task GetInteriorRingN(bool isAsync)
        {
            await base.GetInteriorRingN(isAsync);

            AssertSql(
                @"SELECT [p].[Id], CASE
    WHEN [p].[Polygon] IS NULL OR (([p].[Polygon].STNumInteriorRing() = 0) AND [p].[Polygon].STNumInteriorRing() IS NOT NULL) THEN NULL
    ELSE [p].[Polygon].STInteriorRingN(0 + 1)
END AS [InteriorRing0]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task GetPointN(bool isAsync)
        {
            await base.GetPointN(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[LineString].STPointN(0 + 1) AS [Point0]
//FROM [LineStringEntity] AS [e]");
        }

        public override async Task InteriorPoint(bool isAsync)
        {
            await base.InteriorPoint(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STPointOnSurface() AS [InteriorPoint], [p].[Polygon]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task Intersection(bool isAsync)
        {
            await base.Intersection(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__polygon_0='0x0000000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STIntersection(@__polygon_0) AS [Intersection]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task Intersects(bool isAsync)
        {
            await base.Intersects(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__lineString_0='0x000000000114000000000000E03F000000000000E0BF000000000000E03F0000...' (Size = 38) (DbType = Object)

//SELECT [e].[Id], [e].[LineString].STIntersects(@__lineString_0) AS [Intersects]
//FROM [LineStringEntity] AS [e]");
        }

        public override async Task ICurve_IsClosed(bool isAsync)
        {
            await base.ICurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STIsClosed() AS [IsClosed]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task IMultiCurve_IsClosed(bool isAsync)
        {
            await base.IMultiCurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[MultiLineString].STIsClosed() AS [IsClosed]
FROM [MultiLineStringEntity] AS [m]");
        }

        public override async Task IsEmpty(bool isAsync)
        {
            await base.IsEmpty(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[MultiLineString].STIsEmpty() AS [IsEmpty]
FROM [MultiLineStringEntity] AS [m]");
        }

        public override async Task IsRing(bool isAsync)
        {
            await base.IsRing(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STIsRing() AS [IsRing]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task IsSimple(bool isAsync)
        {
            await base.IsSimple(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STIsSimple() AS [IsSimple]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task IsValid(bool isAsync)
        {
            await base.IsValid(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STIsValid() AS [IsValid]
FROM [PointEntity] AS [p]");
        }

        public override async Task IsWithinDistance(bool isAsync)
        {
            await base.IsWithinDistance(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__point_0='0x00000000010C0000000000000000000000000000F03F' (Size = 22) (DbType = Object)

//SELECT [e].[Id], CASE
//    WHEN [e].[Point].STDistance(@__point_0) <= 1.0E0
//    THEN CAST(1 AS bit) ELSE CAST(0 AS bit)
//END AS [IsWithinDistance]
//FROM [PointEntity] AS [e]");
        }

        public override async Task Item(bool isAsync)
        {
            await base.Item(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[MultiLineString].STGeometryN(0 + 1) AS [Item0]
//FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task Length(bool isAsync)
        {
            await base.Length(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STLength() AS [Length]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task M(bool isAsync)
        {
            await base.M(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].M AS [M]
FROM [PointEntity] AS [p]");
        }

        public override async Task NumGeometries(bool isAsync)
        {
            await base.NumGeometries(isAsync);

            AssertSql(
                @"SELECT [m].[Id], [m].[MultiLineString].STNumGeometries() AS [NumGeometries]
FROM [MultiLineStringEntity] AS [m]");
        }

        public override async Task NumInteriorRings(bool isAsync)
        {
            await base.NumInteriorRings(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STNumInteriorRing() AS [NumInteriorRings]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task NumPoints(bool isAsync)
        {
            await base.NumPoints(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STNumPoints() AS [NumPoints]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task OgcGeometryType(bool isAsync)
        {
            await base.OgcGeometryType(isAsync);

            AssertSql(
                @"SELECT [p].[Id], CASE [p].[Point].STGeometryType()
    WHEN N'Point' THEN 1
    WHEN N'LineString' THEN 2
    WHEN N'Polygon' THEN 3
    WHEN N'MultiPoint' THEN 4
    WHEN N'MultiLineString' THEN 5
    WHEN N'MultiPolygon' THEN 6
    WHEN N'GeometryCollection' THEN 7
    WHEN N'CircularString' THEN 8
    WHEN N'CompoundCurve' THEN 9
    WHEN N'CurvePolygon' THEN 10
END AS [OgcGeometryType]
FROM [PointEntity] AS [p]");
        }

        public override async Task Overlaps(bool isAsync)
        {
            await base.Overlaps(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__polygon_0='0x0000000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STOverlaps(@__polygon_0) AS [Overlaps]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task PointOnSurface(bool isAsync)
        {
            await base.PointOnSurface(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STPointOnSurface() AS [PointOnSurface], [p].[Polygon]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task Relate(bool isAsync)
        {
            await base.Relate(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__polygon_0='0x0000000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STRelate(@__polygon_0, N'212111212') AS [Relate]
//FROM [PolygonEntity] AS [e]");
        }

        [ConditionalTheory(Skip = "No Server Translation.")]
        public override Task Reverse(bool isAsync)
        {
            return base.Reverse(isAsync);
        }

        public override async Task SRID(bool isAsync)
        {
            await base.SRID(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STSrid AS [SRID]
FROM [PointEntity] AS [p]");
        }

        public override async Task SRID_geometry(bool isAsync)
        {
            await base.SRID_geometry(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Geometry].STSrid AS [SRID]
FROM [PointEntity] AS [p]");
        }

        public override async Task StartPoint(bool isAsync)
        {
            await base.StartPoint(isAsync);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STStartPoint() AS [StartPoint]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task SymmetricDifference(bool isAsync)
        {
            await base.SymmetricDifference(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__polygon_0='0x0000000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STSymDifference(@__polygon_0) AS [SymmetricDifference]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task ToBinary(bool isAsync)
        {
            await base.ToBinary(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].STAsBinary() AS [Binary]
//FROM [PointEntity] AS [e]");
        }

        public override async Task ToText(bool isAsync)
        {
            await base.ToText(isAsync);

            // issue #15994
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].AsTextZM() AS [Text]
//FROM [PointEntity] AS [e]");
        }

        public override async Task Touches(bool isAsync)
        {
            await base.Touches(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__polygon_0='0x000000000104040000000000000000000000000000000000F03F000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STTouches(@__polygon_0) AS [Touches]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task Union(bool isAsync)
        {
            await base.Union(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__polygon_0='0x0000000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STUnion(@__polygon_0) AS [Union]
//FROM [PolygonEntity] AS [e]");
        }

        [ConditionalTheory(Skip = "No Server Translation.")]
        public override Task Union_void(bool isAsync)
        {
            return base.Union_void(isAsync);
        }

        public override async Task Within(bool isAsync)
        {
            await base.Within(isAsync);

            // issue #15994
//            AssertSql(
//                @"@__polygon_0='0x00000000010405000000000000000000F0BF000000000000F0BF000000000000...' (Size = 112) (DbType = Object)

//SELECT [e].[Id], [e].[Point].STWithin(@__polygon_0) AS [Within]
//FROM [PointEntity] AS [e]");
        }

        public override async Task X(bool isAsync)
        {
            await base.X(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STX AS [X]
FROM [PointEntity] AS [p]");
        }

        public override async Task Y(bool isAsync)
        {
            await base.Y(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STY AS [Y]
FROM [PointEntity] AS [p]");
        }

        public override async Task Z(bool isAsync)
        {
            await base.Z(isAsync);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].Z AS [Z]
FROM [PointEntity] AS [p]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
