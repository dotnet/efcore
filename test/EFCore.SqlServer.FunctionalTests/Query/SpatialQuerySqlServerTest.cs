// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SpatialQuerySqlServerTest : SpatialQueryTestBase<SpatialQuerySqlServerFixture>
    {
        public SpatialQuerySqlServerTest(SpatialQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Area(bool isAsync)
        {
            await base.Area(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STArea() AS [Area]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task AsBinary(bool isAsync)
        {
            await base.AsBinary(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STAsBinary() AS [Binary]
FROM [PointEntity] AS [e]");
        }

        public override async Task AsText(bool isAsync)
        {
            await base.AsText(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].AsTextZM() AS [Text]
FROM [PointEntity] AS [e]");
        }

        public override async Task Boundary(bool isAsync)
        {
            await base.Boundary(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STBoundary() AS [Boundary]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Buffer(bool isAsync)
        {
            await base.Buffer(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STBuffer(1.0E0) AS [Buffer]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Centroid(bool isAsync)
        {
            await base.Centroid(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STCentroid() AS [Centroid]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Contains(bool isAsync)
        {
            await base.Contains(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STContains('POINT (0.5 0.25)') AS [Contains]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task ConvexHull(bool isAsync)
        {
            await base.ConvexHull(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STConvexHull() AS [ConvexHull]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task IGeometryCollection_Count(bool isAsync)
        {
            await base.IGeometryCollection_Count(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[MultiLineString].STNumGeometries() AS [Count]
FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task LineString_Count(bool isAsync)
        {
            await base.LineString_Count(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STNumPoints() AS [Count]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task Crosses(bool isAsync)
        {
            await base.Crosses(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STCrosses('LINESTRING (0.5 -0.5, 0.5 0.5)') AS [Crosses]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task Difference(bool isAsync)
        {
            await base.Difference(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STDifference('POLYGON ((0 0, 1 0, 1 1, 0 0))') AS [Difference]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Dimension(bool isAsync)
        {
            await base.Dimension(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STDimension() AS [Dimension]
FROM [PointEntity] AS [e]");
        }

        public override async Task Disjoint(bool isAsync)
        {
            await base.Disjoint(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STDisjoint('POINT (1 0)') AS [Disjoint]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Distance(bool isAsync)
        {
            await base.Distance(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STDistance('POINT (0 1)') AS [Distance]
FROM [PointEntity] AS [e]");
        }

        public override async Task EndPoint(bool isAsync)
        {
            await base.EndPoint(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STEndPoint() AS [EndPoint]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task Envelope(bool isAsync)
        {
            await base.Envelope(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STEnvelope() AS [Envelope]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task EqualsTopologically(bool isAsync)
        {
            await base.EqualsTopologically(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STEquals('POINT (0 0)') AS [EqualsTopologically]
FROM [PointEntity] AS [e]");
        }

        public override async Task ExteriorRing(bool isAsync)
        {
            await base.ExteriorRing(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STExteriorRing() AS [ExteriorRing]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task GeometryType(bool isAsync)
        {
            await base.GeometryType(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STGeometryType() AS [GeometryType]
FROM [PointEntity] AS [e]");
        }

        public override async Task GetGeometryN(bool isAsync)
        {
            await base.GetGeometryN(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[MultiLineString].STGeometryN(0 + 1) AS [Geometry0]
FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task GetInteriorRingN(bool isAsync)
        {
            await base.GetInteriorRingN(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STInteriorRingN(0 + 1) AS [InteriorRing0]
FROM [PolygonEntity] AS [e]
WHERE [e].[Polygon].STNumInteriorRing() > 0");
        }

        public override async Task GetPointN(bool isAsync)
        {
            await base.GetPointN(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STPointN(0 + 1) AS [Point0]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task Intersection(bool isAsync)
        {
            await base.Intersection(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STIntersection('POLYGON ((0 0, 1 0, 1 1, 0 0))') AS [Intersection]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Intersects(bool isAsync)
        {
            await base.Intersects(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STIntersects('LINESTRING (0.5 -0.5, 0.5 0.5)') AS [Intersects]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task ICurve_IsClosed(bool isAsync)
        {
            await base.ICurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STIsClosed() AS [IsClosed]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task IMultiCurve_IsClosed(bool isAsync)
        {
            await base.IMultiCurve_IsClosed(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[MultiLineString].STIsClosed() AS [IsClosed]
FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task IsEmpty(bool isAsync)
        {
            await base.IsEmpty(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[MultiLineString].STIsEmpty() AS [IsEmpty]
FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task IsRing(bool isAsync)
        {
            await base.IsRing(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STIsRing() AS [IsRing]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task IsSimple(bool isAsync)
        {
            await base.IsSimple(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STIsSimple() AS [IsSimple]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task IsValid(bool isAsync)
        {
            await base.IsValid(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STIsValid() AS [IsValid]
FROM [PointEntity] AS [e]");
        }

        public override async Task Item(bool isAsync)
        {
            await base.Item(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[MultiLineString].STGeometryN(0 + 1) AS [Item0]
FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task Length(bool isAsync)
        {
            await base.Length(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STLength() AS [Length]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task M(bool isAsync)
        {
            await base.M(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].M AS [M]
FROM [PointEntity] AS [e]");
        }

        public override async Task NumGeometries(bool isAsync)
        {
            await base.NumGeometries(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[MultiLineString].STNumGeometries() AS [NumGeometries]
FROM [MultiLineStringEntity] AS [e]");
        }

        public override async Task NumInteriorRings(bool isAsync)
        {
            await base.NumInteriorRings(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STNumInteriorRing() AS [NumInteriorRings]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task NumPoints(bool isAsync)
        {
            await base.NumPoints(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STNumPoints() AS [NumPoints]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task Overlaps(bool isAsync)
        {
            await base.Overlaps(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STOverlaps('POLYGON ((0 0, 1 0, 1 1, 0 0))') AS [Overlaps]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task PointOnSurface(bool isAsync)
        {
            await base.PointOnSurface(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STPointOnSurface() AS [PointOnSurface], [e].[Polygon]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Relate(bool isAsync)
        {
            await base.Relate(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STRelate('POLYGON ((0 0, 1 0, 1 1, 0 0))', N'212111212') AS [Relate]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task SRID(bool isAsync)
        {
            await base.SRID(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STSrid AS [SRID]
FROM [PointEntity] AS [e]");
        }

        public override async Task StartPoint(bool isAsync)
        {
            await base.StartPoint(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[LineString].STStartPoint() AS [StartPoint]
FROM [LineStringEntity] AS [e]");
        }

        public override async Task SymmetricDifference(bool isAsync)
        {
            await base.SymmetricDifference(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STSymDifference('POLYGON ((0 0, 1 0, 1 1, 0 0))') AS [SymmetricDifference]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task ToBinary(bool isAsync)
        {
            await base.ToBinary(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STAsBinary() AS [Binary]
FROM [PointEntity] AS [e]");
        }

        public override async Task ToText(bool isAsync)
        {
            await base.ToText(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].AsTextZM() AS [Text]
FROM [PointEntity] AS [e]");
        }

        public override async Task Touches(bool isAsync)
        {
            await base.Touches(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STTouches('POLYGON ((0 1, 1 1, 1 0, 0 1))') AS [Touches]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Union(bool isAsync)
        {
            await base.Union(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Polygon].STUnion('POLYGON ((0 0, 1 0, 1 1, 0 0))') AS [Union]
FROM [PolygonEntity] AS [e]");
        }

        public override async Task Within(bool isAsync)
        {
            await base.Within(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STWithin('POLYGON ((-1 -1, -1 2, 2 2, 2 -1, -1 -1))') AS [Within]
FROM [PointEntity] AS [e]");
        }

        public override async Task X(bool isAsync)
        {
            await base.X(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STX AS [X]
FROM [PointEntity] AS [e]");
        }

        public override async Task Y(bool isAsync)
        {
            await base.Y(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].STY AS [Y]
FROM [PointEntity] AS [e]");
        }

        public override async Task Z(bool isAsync)
        {
            await base.Z(isAsync);

            AssertSql(
                @"SELECT [e].[Id], [e].[Point].Z AS [Z]
FROM [PointEntity] AS [e]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
