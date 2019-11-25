// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SpatialQuerySqlServerGeographyTest : SpatialQueryTestBase<SpatialQuerySqlServerGeographyFixture>
    {
        public SpatialQuerySqlServerGeographyTest(SpatialQuerySqlServerGeographyFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // TODO: Remove after NetTopologySuite/NetTopologySuite#233
        protected override bool AssertDistances
            => false;

        public override async Task SimpleSelect(bool async)
        {
            await base.SimpleSelect(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Geometry], [p].[Point]
FROM [PointEntity] AS [p]");
        }

        public override async Task WithConversion(bool async)
        {
            await base.WithConversion(async);

            AssertSql(
                @"SELECT [g].[Id], [g].[Location]
FROM [GeoPointEntity] AS [g]");
        }

        public override async Task Area(bool async)
        {
            await base.Area(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].STArea() AS [Area]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task AsBinary(bool async)
        {
            await base.AsBinary(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].STAsBinary() AS [Binary]
//FROM [PointEntity] AS [e]");
        }

        public override async Task AsText(bool async)
        {
            await base.AsText(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].AsTextZM() AS [Text]
//FROM [PointEntity] AS [e]");
        }

        // No SqlServer Translation
        public override Task Boundary(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task Buffer(bool async)
        {
            await base.Buffer(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Polygon].STBuffer(1.0E0) AS [Buffer]
//FROM [PolygonEntity] AS [e]");
        }

        // No SqlServer Translation
        public override Task Buffer_quadrantSegments(bool async)
        {
            return Task.CompletedTask;
        }

        // No SqlServer Translation
        public override Task Centroid(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task Contains(bool async)
        {
            await base.Contains(async);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0xE6100000010C000000000000D03F000000000000D03F' (Size = 22) (DbType = Binary)

//SELECT [e].[Id], [e].[Polygon].STContains(@__point_0) AS [Contains]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task ConvexHull(bool async)
        {
            await base.ConvexHull(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Polygon].STConvexHull() AS [ConvexHull]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task IGeometryCollection_Count(bool async)
        {
            await base.IGeometryCollection_Count(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[MultiLineString].STNumGeometries() AS [Count]
FROM [MultiLineStringEntity] AS [m]");
        }

        public override async Task LineString_Count(bool async)
        {
            await base.LineString_Count(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STNumPoints() AS [Count]
FROM [LineStringEntity] AS [l]");
        }

        // No SqlServer Translation
        public override Task CoveredBy(bool async)
        {
            return Task.CompletedTask;
        }

        // No SqlServer Translation
        public override Task Covers(bool async)
        {
            return Task.CompletedTask;
        }

        // No SqlServer Translation
        public override Task Crosses(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task Difference(bool async)
        {
            await base.Difference(async);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0xE610000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Binary)

//SELECT [e].[Id], [e].[Polygon].STDifference(@__polygon_0) AS [Difference]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task Dimension(bool async)
        {
            await base.Dimension(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STDimension() AS [Dimension]
FROM [PointEntity] AS [p]");
        }

        public override async Task Disjoint(bool async)
        {
            await base.Disjoint(async);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0xE6100000010C000000000000F03F000000000000F03F' (Size = 22) (DbType = Binary)

//SELECT [e].[Id], [e].[Polygon].STDisjoint(@__point_0) AS [Disjoint]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task Distance(bool async)
        {
            await base.Distance(async);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0xE6100000010C000000000000F03F0000000000000000' (Size = 22) (DbType = Binary)

//SELECT [e].[Id], [e].[Point].STDistance(@__point_0) AS [Distance]
//FROM [PointEntity] AS [e]");
        }

        public override async Task Distance_geometry(bool async)
        {
            await base.Distance_geometry(async);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0xE6100000010C000000000000F03F0000000000000000' (Size = 22) (DbType = Binary)

//SELECT [e].[Id], [e].[Geometry].STDistance(@__point_0) AS [Distance]
//FROM [PointEntity] AS [e]");
        }

        // Mixing SRIDs not supported
        public override Task Distance_constant(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task Distance_constant_srid_4326(bool async)
        {
            await base.Distance_constant_srid_4326(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].STDistance('POINT (1 1)') AS [Distance]
//FROM [PointEntity] AS [e]");
        }

        // Mixing SRIDs not supported
        public override Task Distance_constant_lhs(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task Distance_on_converted_geometry_type(bool async)
        {
            await base.Distance_on_converted_geometry_type(async);

            AssertSql(
                @"@__point_0='0xE6100000010C000000000000F03F0000000000000000' (Nullable = false) (Size = 22) (DbType = Object)

SELECT [g].[Id], [g].[Location].STDistance(@__point_0) AS [Distance]
FROM [GeoPointEntity] AS [g]");
        }

        public override async Task Distance_on_converted_geometry_type_lhs(bool async)
        {
            await base.Distance_on_converted_geometry_type_lhs(async);

            AssertSql(
                @"@__point_0='0xE6100000010C000000000000F03F0000000000000000' (Nullable = false) (Size = 22) (DbType = Object)

SELECT [g].[Id], @__point_0.STDistance([g].[Location]) AS [Distance]
FROM [GeoPointEntity] AS [g]");
        }

        // Mixing SRIDs not supported
        public override Task Distance_on_converted_geometry_type_constant(bool async)
        {
            return Task.CompletedTask;
        }

        // Mixing SRIDs not supported
        public override Task Distance_on_converted_geometry_type_constant_lhs(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task EndPoint(bool async)
        {
            await base.EndPoint(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STEndPoint() AS [EndPoint]
FROM [LineStringEntity] AS [l]");
        }

        // No SqlServer Translation
        public override Task Envelope(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task EqualsTopologically(bool async)
        {
            await base.EqualsTopologically(async);

            // issue #16050
//            AssertSql(
//                @"@__point_0='0xE6100000010C00000000000000000000000000000000' (Size = 22) (DbType = Object)

//SELECT [e].[Id], [e].[Point].STEquals(@__point_0) AS [EqualsTopologically]
//FROM [PointEntity] AS [e]");
        }

        public override async Task ExteriorRing(bool async)
        {
            await base.ExteriorRing(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].RingN(1) AS [ExteriorRing]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task GeometryType(bool async)
        {
            await base.GeometryType(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STGeometryType() AS [GeometryType]
FROM [PointEntity] AS [p]");
        }

        public override async Task GetGeometryN(bool async)
        {
            await base.GetGeometryN(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[MultiLineString].STGeometryN(0 + 1) AS [Geometry0]
//FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task GetInteriorRingN(bool async)
        {
            await base.GetInteriorRingN(async);

            AssertSql(
                @"SELECT [p].[Id], CASE
    WHEN [p].[Polygon] IS NULL OR (([p].[Polygon].NumRings() - 1) = 0) THEN NULL
    ELSE [p].[Polygon].RingN(0 + 2)
END AS [InteriorRing0]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task GetPointN(bool async)
        {
            await base.GetPointN(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[LineString].STPointN(0 + 1) AS [Point0]
//FROM [LineStringEntity] AS [e]");
        }

        // No SqlServer Translation
        public override Task InteriorPoint(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task Intersection(bool async)
        {
            await base.Intersection(async);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0xE610000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STIntersection(@__polygon_0) AS [Intersection]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task Intersects(bool async)
        {
            await base.Intersects(async);

            // issue #16050
//            AssertSql(
//                @"@__lineString_0='0xE61000000114000000000000E0BF000000000000E03F000000000000E03F0000...' (Size = 38) (DbType = Object)

//SELECT [e].[Id], [e].[LineString].STIntersects(@__lineString_0) AS [Intersects]
//FROM [LineStringEntity] AS [e]");
        }

        public override async Task ICurve_IsClosed(bool async)
        {
            await base.ICurve_IsClosed(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STIsClosed() AS [IsClosed]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task IMultiCurve_IsClosed(bool async)
        {
            await base.IMultiCurve_IsClosed(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[MultiLineString].STIsClosed() AS [IsClosed]
FROM [MultiLineStringEntity] AS [m]");
        }

        public override async Task IsEmpty(bool async)
        {
            await base.IsEmpty(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[MultiLineString].STIsEmpty() AS [IsEmpty]
FROM [MultiLineStringEntity] AS [m]");
        }

        // No SqlServer Translation
        public override Task IsRing(bool async)
        {
            return Task.CompletedTask;
        }

        // No SqlServer Translation
        public override Task IsSimple(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task IsValid(bool async)
        {
            await base.IsValid(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STIsValid() AS [IsValid]
FROM [PointEntity] AS [p]");
        }

        public override async Task IsWithinDistance(bool async)
        {
            await base.IsWithinDistance(async);

            AssertSql(
                @"@__point_0='0xE6100000010C000000000000F03F0000000000000000' (Size = 22) (DbType = Object)

SELECT [p].[Id], CASE
    WHEN [p].[Point] IS NULL THEN NULL
    ELSE CASE
        WHEN [p].[Point].STDistance(@__point_0) <= 1.0E0 THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END
END AS [IsWithinDistance]
FROM [PointEntity] AS [p]");
        }

        public override async Task Item(bool async)
        {
            await base.Item(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[MultiLineString].STGeometryN(0 + 1) AS [Item0]
//FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task Length(bool async)
        {
            await base.Length(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STLength() AS [Length]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task M(bool async)
        {
            await base.M(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].M AS [M]
FROM [PointEntity] AS [p]");
        }

        public override async Task NumGeometries(bool async)
        {
            await base.NumGeometries(async);

            AssertSql(
                @"SELECT [m].[Id], [m].[MultiLineString].STNumGeometries() AS [NumGeometries]
FROM [MultiLineStringEntity] AS [m]");
        }

        public override async Task NumInteriorRings(bool async)
        {
            await base.NumInteriorRings(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Polygon].NumRings() - 1 AS [NumInteriorRings]
FROM [PolygonEntity] AS [p]");
        }

        public override async Task NumPoints(bool async)
        {
            await base.NumPoints(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STNumPoints() AS [NumPoints]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task OgcGeometryType(bool async)
        {
            await base.OgcGeometryType(async);

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
    WHEN N'FullGlobe' THEN 126
END AS [OgcGeometryType]
FROM [PointEntity] AS [p]");
        }

        public override async Task Overlaps(bool async)
        {
            await base.Overlaps(async);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0xE610000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STOverlaps(@__polygon_0) AS [Overlaps]
//FROM [PolygonEntity] AS [e]");
        }

        // No SqlServer Translation
        public override Task PointOnSurface(bool async)
        {
            return Task.CompletedTask;
        }

        // No SqlServer Translation
        public override Task Relate(bool async)
        {
            return Task.CompletedTask;
        }

        // No SqlServer Translation
        public override Task Reverse(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task SRID(bool async)
        {
            await base.SRID(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].STSrid AS [SRID]
FROM [PointEntity] AS [p]");
        }

        public override async Task SRID_geometry(bool async)
        {
            await base.SRID_geometry(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Geometry].STSrid AS [SRID]
FROM [PointEntity] AS [p]");
        }

        public override async Task StartPoint(bool async)
        {
            await base.StartPoint(async);

            AssertSql(
                @"SELECT [l].[Id], [l].[LineString].STStartPoint() AS [StartPoint]
FROM [LineStringEntity] AS [l]");
        }

        public override async Task SymmetricDifference(bool async)
        {
            await base.SymmetricDifference(async);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0xE610000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STSymDifference(@__polygon_0) AS [SymmetricDifference]
//FROM [PolygonEntity] AS [e]");
        }

        public override async Task ToBinary(bool async)
        {
            await base.ToBinary(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].STAsBinary() AS [Binary]
//FROM [PointEntity] AS [e]");
        }

        public override async Task ToText(bool async)
        {
            await base.ToText(async);

            // issue #16050
//            AssertSql(
//                @"SELECT [e].[Id], [e].[Point].AsTextZM() AS [Text]
//FROM [PointEntity] AS [e]");
        }

        // No SqlServer Translation
        public override Task Touches(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task Union(bool async)
        {
            await base.Union(async);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0xE610000001040400000000000000000000000000000000000000000000000000...' (Size = 96) (DbType = Object)

//SELECT [e].[Id], [e].[Polygon].STUnion(@__polygon_0) AS [Union]
//FROM [PolygonEntity] AS [e]");
        }

        // No SqlServer Translation
        public override Task Union_void(bool async)
        {
            return Task.CompletedTask;
        }

        public override async Task Within(bool async)
        {
            await base.Within(async);

            // issue #16050
//            AssertSql(
//                @"@__polygon_0='0xE6100000010405000000000000000000F0BF000000000000F0BF000000000000...' (Size = 112) (DbType = Object)

//SELECT [e].[Id], [e].[Point].STWithin(@__polygon_0) AS [Within]
//FROM [PointEntity] AS [e]");
        }

        public override async Task X(bool async)
        {
            await base.X(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].Long AS [X]
FROM [PointEntity] AS [p]");
        }

        public override async Task Y(bool async)
        {
            await base.Y(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].Lat AS [Y]
FROM [PointEntity] AS [p]");
        }

        public override async Task Z(bool async)
        {
            await base.Z(async);

            AssertSql(
                @"SELECT [p].[Id], [p].[Point].Z AS [Z]
FROM [PointEntity] AS [p]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
