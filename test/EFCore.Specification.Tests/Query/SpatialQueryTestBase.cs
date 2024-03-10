// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Union;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class SpatialQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : SpatialQueryFixtureBase, new()
{
    protected SpatialQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected virtual bool AssertDistances
        => true;

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task SimpleSelect(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<PointEntity>());

        await AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>());

        await AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>());

        await AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>());
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task WithConversion(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<GeoPointEntity>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Area(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Area = e.Polygon == null ? (double?)null : e.Polygon.Area }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Area == null)
                {
                    Assert.Null(a.Area);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Area, a.Area);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AsBinary(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Binary = e.Point.AsBinary() }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Binary = e.Point == null ? null : e.Point.AsBinary() }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Binary, a.Binary);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AsBinary_with_null_check(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Binary = e.Point == null ? null : e.Point.AsBinary() }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Binary, a.Binary);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task AsText(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Text = e.Point.AsText() }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Text = e.Point == null ? null : e.Point.AsText() }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Text, a.Text, WktComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Boundary(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Boundary = e.Polygon == null ? null : e.Polygon.Boundary }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Boundary, a.Boundary, GeometryComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Buffer(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Buffer = e.Polygon.Buffer(1.0) }),
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Buffer = e.Polygon == null ? null : e.Polygon.Buffer(1.0) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Buffer?.Centroid, a.Buffer?.Centroid, GeometryComparer.Instance);

                if (e.Buffer == null)
                {
                    Assert.Null(a.Buffer);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Buffer.Area, a.Buffer.Area, precision: 0);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Buffer_quadrantSegments(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Buffer = e.Polygon.Buffer(1.0, 8) }),
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Buffer = e.Polygon == null ? null : e.Polygon.Buffer(1.0, 8) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Buffer?.Centroid, a.Buffer?.Centroid, GeometryComparer.Instance);

                if (e.Buffer == null)
                {
                    Assert.Null(a.Buffer);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Buffer.Area, a.Buffer.Area, precision: 0);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Centroid(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Centroid = e.Polygon == null ? null : e.Polygon.Centroid }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Centroid, a.Centroid, GeometryComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Combine_aggregate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>()
                .Where(e => e.Point != null)
                .GroupBy(e => e.Group)
                .Select(g => new { Id = g.Key, Combined = GeometryCombiner.Combine(g.Select(e => e.Point)) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                // Note that NTS returns a MultiPoint (which is a subclass of GeometryCollection), whereas SQL Server returns a
                // GeometryCollection.
                var eCollection = (GeometryCollection)e.Combined;
                var aCollection = (GeometryCollection)a.Combined;

                Assert.Equal(eCollection.Geometries, aCollection.Geometries);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EnvelopeCombine_aggregate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>()
                .Where(e => e.Point != null)
                .GroupBy(e => e.Group)
                .Select(g => new { Id = g.Key, Combined = EnvelopeCombiner.CombineAsGeometry(g.Select(e => e.Point)) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Combined, a.Combined, GeometryComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Contains(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0.25, 0.25));

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Contains = (bool?)e.Polygon.Contains(point) }),
            ss => ss.Set<PolygonEntity>()
                .Select(e => new { e.Id, Contains = e.Polygon == null ? (bool?)null : e.Polygon.Contains(point) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ConvexHull(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, ConvexHull = e.Polygon.ConvexHull() }),
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, ConvexHull = e.Polygon == null ? null : e.Polygon.ConvexHull() }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.ConvexHull, a.ConvexHull, GeometryComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ConvexHull_aggregate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>()
                .Where(e => e.Point != null)
                .GroupBy(e => e.Group)
                .Select(g => new { Id = g.Key, ConvexHull = NetTopologySuite.Algorithm.ConvexHull.Create(g.Select(e => e.Point)) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.ConvexHull, a.ConvexHull, GeometryComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IGeometryCollection_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(
                e => new { e.Id, Count = e.MultiLineString == null ? (int?)null : e.MultiLineString.Count }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task LineString_Count(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(
                e => new { e.Id, Count = e.LineString == null ? (int?)null : e.LineString.Count }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task CoveredBy(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(-1, -1), new(2, -1), new(2, 2), new(-1, 2), new(-1, -1)]);

        return AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, CoveredBy = (bool?)e.Point.CoveredBy(polygon) }),
            ss => ss.Set<PointEntity>()
                .Select(e => new { e.Id, CoveredBy = e.Point == null ? (bool?)null : e.Point.CoveredBy(polygon) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Covers(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0.25, 0.25));

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Covers = (bool?)e.Polygon.Covers(point) }),
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Covers = e.Polygon == null ? (bool?)null : e.Polygon.Covers(point) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Crosses(bool async)
    {
        var lineString = Fixture.GeometryFactory.CreateLineString([new(0.5, -0.5), new(0.5, 0.5)]);

        return AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Crosses = (bool?)e.LineString.Crosses(lineString) }),
            ss => ss.Set<LineStringEntity>().Select(
                e => new { e.Id, Crosses = e.LineString == null ? (bool?)null : e.LineString.Crosses(lineString) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Difference(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(0, 0), new(1, 0), new(1, 1), new(0, 0)]);

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Difference = e.Polygon.Difference(polygon) }),
            ss => ss.Set<PolygonEntity>()
                .Select(e => new { e.Id, Difference = e.Polygon == null ? null : e.Polygon.Difference(polygon) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Difference, a.Difference, GeometryComparer.Instance);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Dimension(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Dimension = e.Point == null ? (Dimension?)null : e.Point.Dimension }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Disjoint_with_cast_to_nullable(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(1, 1));

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Disjoint = (bool?)e.Polygon.Disjoint(point) }),
            ss => ss.Set<PolygonEntity>()
                .Select(e => new { e.Id, Disjoint = e.Polygon == null ? (bool?)null : e.Polygon.Disjoint(point) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Disjoint_with_null_check(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(1, 1));

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>()
                .Select(e => new { e.Id, Disjoint = e.Polygon == null ? (bool?)null : e.Polygon.Disjoint(point) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_with_null_check(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

        return AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Distance = (double?)e.Point.Distance(point) }),
            ss => ss.Set<PointEntity>()
                .Select(e => new { e.Id, Distance = (e.Point == null ? (double?)null : e.Point.Distance(point)) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Distance == null)
                {
                    Assert.Null(a.Distance);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Distance, a.Distance);
                }
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_with_cast_to_nullable(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

        return AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Distance = (double?)e.Point.Distance(point) }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Distance = e.Point == null ? (double?)null : e.Point.Distance(point) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Distance == null)
                {
                    Assert.Null(a.Distance);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Distance, a.Distance);
                }
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_geometry(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

        return AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Distance = (double?)e.Geometry.Distance(point) }),
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, Distance = e.Geometry == null ? (double?)null : e.Geometry.Distance(point) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Distance == null)
                {
                    Assert.Null(a.Distance);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Distance, a.Distance);
                }
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Distance = (double?)e.Point.Distance(new Point(0, 1)) }),
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, Distance = e.Point == null ? (double?)null : e.Point.Distance(new Point(0, 1)) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Distance == null)
                {
                    Assert.Null(a.Distance);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Distance, a.Distance);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_constant_srid_4326(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>()
                .Select(e => new { e.Id, Distance = (double?)e.Point.Distance(new Point(1, 1) { SRID = 4326 }) }),
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, Distance = e.Point == null ? (double?)null : e.Point.Distance(new Point(1, 1) { SRID = 4326 }) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Distance == null)
                {
                    Assert.Null(a.Distance);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Distance, a.Distance);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_constant_lhs(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Distance = (double?)new Point(0, 1).Distance(e.Point) }),
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, Distance = e.Point == null ? (double?)null : new Point(0, 1).Distance(e.Point) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Distance == null)
                {
                    Assert.Null(a.Distance);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Distance, a.Distance);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_on_converted_geometry_type(bool async)
    {
        var point = new GeoPoint(1, 0);

        return AssertQuery(
            async,
            ss => ss.Set<GeoPointEntity>().Select(
                e => new { e.Id, Distance = e.Location.Distance(point) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) => { Assert.Equal(e.Id, a.Id); });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_on_converted_geometry_type_lhs(bool async)
    {
        var point = new GeoPoint(1, 0);

        return AssertQuery(
            async,
            ss => ss.Set<GeoPointEntity>().Select(
                e => new { e.Id, Distance = point.Distance(e.Location) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) => { Assert.Equal(e.Id, a.Id); });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_on_converted_geometry_type_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<GeoPointEntity>().Select(
                e => new { e.Id, Distance = e.Location.Distance(new GeoPoint(1, 0)) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                if (AssertDistances)
                {
                    Assert.Equal(e.Distance, a.Distance);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Distance_on_converted_geometry_type_constant_lhs(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<GeoPointEntity>().Select(
                e => new { e.Id, Distance = new GeoPoint(1, 0).Distance(e.Location) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                if (AssertDistances)
                {
                    Assert.Equal(e.Distance, a.Distance);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EndPoint(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, EndPoint = e.LineString == null ? null : e.LineString.EndPoint }),
            elementSorter: e => e.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Envelope(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Envelope = e.Polygon == null ? null : e.Polygon.Envelope }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Envelope, a.Envelope, GeometryComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task EqualsTopologically(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 0));

        return AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, EqualsTopologically = (bool?)e.Point.EqualsTopologically(point) }),
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, EqualsTopologically = e.Point == null ? (bool?)null : e.Point.EqualsTopologically(point) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ExteriorRing(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, ExteriorRing = e.Polygon == null ? null : e.Polygon.ExteriorRing }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GeometryType(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, GeometryType = e.Point == null ? null : e.Point.GeometryType }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetGeometryN(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(e => new { e.Id, Geometry0 = e.MultiLineString.GetGeometryN(0) }),
            ss => ss.Set<MultiLineStringEntity>().Select(
                e => new { e.Id, Geometry0 = e.MultiLineString == null ? null : e.MultiLineString.GetGeometryN(0) }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetGeometryN_with_null_argument(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(
                e => new
                {
                    e.Id,
                    Geometry0 = e.MultiLineString.GetGeometryN(ss.Set<MultiLineStringEntity>().Where(ee => false).Max(ee => ee.Id))
                }),
            ss => ss.Set<MultiLineStringEntity>().Select(e => new { e.Id, Geometry0 = default(Geometry) }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetInteriorRingN(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(
                e => new
                {
                    e.Id,
                    InteriorRing0 = e.Polygon.NumInteriorRings == 0
                        ? null
                        : e.Polygon.GetInteriorRingN(0)
                }),
            ss => ss.Set<PolygonEntity>().Select(
                e => new
                {
                    e.Id,
                    InteriorRing0 = e.Polygon == null || e.Polygon.NumInteriorRings == 0
                        ? null
                        : e.Polygon.GetInteriorRingN(0)
                }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task GetPointN(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Point0 = e.LineString.GetPointN(0) }),
            ss => ss.Set<LineStringEntity>()
                .Select(e => new { e.Id, Point0 = e.LineString == null ? null : e.LineString.GetPointN(0) }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task InteriorPoint(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(
                e => new
                {
                    e.Id,
                    InteriorPoint = e.Polygon == null ? null : e.Polygon.InteriorPoint,
                    e.Polygon
                }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.InteriorPoint == null)
                {
                    Assert.Null(e.InteriorPoint);
                }
                else
                {
                    Assert.True(a.Polygon.Contains(e.InteriorPoint));
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Intersection(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(0, 0), new(1, 0), new(1, 1), new(0, 0)]);

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Intersection = e.Polygon.Intersection(polygon) }),
            ss => ss.Set<PolygonEntity>().Select(
                e => new { e.Id, Intersection = e.Polygon == null ? null : e.Polygon.Intersection(polygon) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Intersection, a.Intersection, GeometryComparer.Instance);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Intersects(bool async)
    {
        var lineString = Fixture.GeometryFactory.CreateLineString([new(0.5, -0.5), new(0.5, 0.5)]);

        return AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Intersects = (bool?)e.LineString.Intersects(lineString) }),
            ss => ss.Set<LineStringEntity>().Select(
                e => new { e.Id, Intersects = e.LineString == null ? (bool?)null : e.LineString.Intersects(lineString) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ICurve_IsClosed(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(
                e => new { e.Id, IsClosed = e.LineString == null ? (bool?)null : e.LineString.IsClosed }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IMultiCurve_IsClosed(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(
                e => new { e.Id, IsClosed = e.MultiLineString == null ? (bool?)null : e.MultiLineString.IsClosed }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsEmpty(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(
                e => new { e.Id, IsEmpty = e.MultiLineString == null ? (bool?)null : e.MultiLineString.IsEmpty }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsRing(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>()
                .Select(e => new { e.Id, IsRing = e.LineString == null ? (bool?)null : e.LineString.IsRing }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsSimple(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(
                e =>
                    new { e.Id, IsSimple = e.LineString == null ? (bool?)null : e.LineString.IsSimple }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsValid(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>()
                .Select(e => new { e.Id, IsValid = e.Point == null ? (bool?)null : e.Point.IsValid }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsWithinDistance(bool async)
    {
        var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

        return AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, IsWithinDistance = (bool?)e.Point.IsWithinDistance(point, 1) }),
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, IsWithinDistance = e.Point == null ? (bool?)null : e.Point.IsWithinDistance(point, 1) }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.IsWithinDistance == null)
                {
                    Assert.False(a.IsWithinDistance ?? false);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.IsWithinDistance, a.IsWithinDistance);
                }
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Item(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(e => new { e.Id, Item0 = e.MultiLineString[0] }),
            ss => ss.Set<MultiLineStringEntity>()
                .Select(e => new { e.Id, Item0 = e.MultiLineString == null ? null : e.MultiLineString[0] }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>()
                .Select(e => new { e.Id, Length = e.LineString == null ? (double?)null : e.LineString.Length }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Length == null)
                {
                    Assert.Null(a.Length);
                }
                else if (AssertDistances)
                {
                    Assert.Equal(e.Length, a.Length);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task M(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, M = e.Point == null ? (double?)null : e.Point.M }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.M == null)
                {
                    Assert.Null(a.M);
                }
                else
                {
                    Assert.Equal(e.M, a.M ?? double.NaN);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Normalized(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Normalized = e.Polygon.Normalized() }),
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Normalized = e.Polygon == null ? null : e.Polygon.Normalized() }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Normalized, a.Normalized, GeometryComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task NumGeometries(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(
                e => new { e.Id, NumGeometries = e.MultiLineString == null ? (int?)null : e.MultiLineString.NumGeometries }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task NumInteriorRings(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(
                e => new { e.Id, NumInteriorRings = e.Polygon == null ? (int?)null : e.Polygon.NumInteriorRings }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task NumPoints(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(
                e => new { e.Id, NumPoints = e.LineString == null ? (int?)null : e.LineString.NumPoints }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task OgcGeometryType(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, OgcGeometryType = e.Point == null ? (OgcGeometryType?)null : e.Point.OgcGeometryType }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Overlaps(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(0, 0), new(1, 0), new(1, 1), new(0, 0)]);

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Overlaps = (bool?)e.Polygon.Overlaps(polygon) }),
            ss => ss.Set<PolygonEntity>()
                .Select(e => new { e.Id, Overlaps = e.Polygon == null ? (bool?)null : e.Polygon.Overlaps(polygon) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task PointOnSurface(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(
                e => new
                {
                    e.Id,
                    PointOnSurface = e.Polygon == null ? null : e.Polygon.PointOnSurface,
                    e.Polygon
                }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.PointOnSurface == null)
                {
                    Assert.Null(a.PointOnSurface);
                }
                else
                {
                    Assert.True(a.Polygon.Contains(e.PointOnSurface));
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Relate(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(0, 0), new(1, 0), new(1, 1), new(0, 0)]);

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Relate = (bool?)e.Polygon.Relate(polygon, "212111212") }),
            ss => ss.Set<PolygonEntity>().Select(
                e => new { e.Id, Relate = e.Polygon == null ? (bool?)null : e.Polygon.Relate(polygon, "212111212") }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Reverse(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Reverse = e.LineString.Reverse() }),
            ss => ss.Set<LineStringEntity>().Select(e => new { e.Id, Reverse = e.LineString == null ? null : e.LineString.Reverse() }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SRID(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, SRID = e.Point == null ? (int?)null : e.Point.SRID }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SRID_geometry(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(
                e => new { e.Id, SRID = e.Geometry == null ? (int?)null : e.Geometry.SRID }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task StartPoint(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<LineStringEntity>()
                .Select(e => new { e.Id, StartPoint = e.LineString == null ? null : e.LineString.StartPoint }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SymmetricDifference(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(0, 0), new(1, 0), new(1, 1), new(0, 0)]);

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, SymmetricDifference = e.Polygon.SymmetricDifference(polygon) }),
            ss => ss.Set<PolygonEntity>().Select(
                e => new { e.Id, SymmetricDifference = e.Polygon == null ? null : e.Polygon.SymmetricDifference(polygon) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.SymmetricDifference, a.SymmetricDifference, GeometryComparer.Instance);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToBinary(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Binary = e.Point.ToBinary() }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Binary = e.Point == null ? null : e.Point.ToBinary() }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Binary, a.Binary);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task ToText(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Text = e.Point.ToText() }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Text = e.Point == null ? null : e.Point.ToText() }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Text, a.Text, WktComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Touches(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(0, 1), new(1, 0), new(1, 1), new(0, 1)]);

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Touches = (bool?)e.Polygon.Touches(polygon) }),
            ss => ss.Set<PolygonEntity>()
                .Select(e => new { e.Id, Touches = e.Polygon == null ? (bool?)null : e.Polygon.Touches(polygon) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(0, 0), new(1, 0), new(1, 1), new(0, 0)]);

        return AssertQuery(
            async,
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Union = e.Polygon.Union(polygon) }),
            ss => ss.Set<PolygonEntity>().Select(e => new { e.Id, Union = e.Polygon == null ? null : e.Polygon.Union(polygon) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Union, a.Union, GeometryComparer.Instance);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_aggregate(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>()
                .Where(e => e.Point != null)
                .GroupBy(e => e.Group)
                .Select(g => new { Id = g.Key, Union = UnaryUnionOp.Union(g.Select(e => e.Point)) }),
            elementSorter: x => x.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                Assert.Equal(e.Union, a.Union, GeometryComparer.Instance);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Union_void(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<MultiLineStringEntity>().Select(e => new { e.Id, Union = e.MultiLineString.Union() }),
            ss => ss.Set<MultiLineStringEntity>()
                .Select(e => new { e.Id, Union = e.MultiLineString == null ? null : e.MultiLineString.Union() }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Within(bool async)
    {
        var polygon = Fixture.GeometryFactory.CreatePolygon([new(-1, -1), new(2, -1), new(2, 2), new(-1, 2), new(-1, -1)]);

        return AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Within = (bool?)e.Point.Within(polygon) }),
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Within = e.Point == null ? (bool?)null : e.Point.Within(polygon) }),
            elementSorter: x => x.Id);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task X(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, X = e.Point == null ? (double?)null : e.Point.X }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Y(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Y = e.Point == null ? (double?)null : e.Point.Y }),
            elementSorter: x => x.Id);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Z(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PointEntity>().Select(e => new { e.Id, Z = e.Point == null ? (double?)null : e.Point.Z }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);

                if (e.Z == null)
                {
                    Assert.Null(a.Z);
                }
                else
                {
                    Assert.Equal(e.Z, a.Z ?? double.NaN);
                }
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task XY_with_collection_join(bool async)
        => AssertFirstOrDefault(
            async,
            ss => ss.Set<PointEntity>()
                .OrderBy(e => e.Id)
                .Select(
                    e => new
                    {
                        e.Id,
                        I = new { X = e.Point == null ? (double?)null : e.Point.X, Y = e.Point == null ? (double?)null : e.Point.Y },
                        List = ss.Set<PointEntity>().Where(i => i.Id == e.Id).ToList()
                    }),
            asserter: (e, a) =>
            {
                AssertEqual(e.Id, a.Id);
                AssertEqual(e.I, a.I);
                AssertCollection(e.List, a.List);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsEmpty_equal_to_null(bool async)
    {
        return AssertQueryScalar(
            async,
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<PointEntity>().Where(e => e.Point.IsEmpty == null).Select(e => e.Id),
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<PointEntity>().Where(e => e.Point == null).Select(e => e.Id));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task IsEmpty_not_equal_to_null(bool async)
    {
        return AssertQueryScalar(
            async,
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<PointEntity>().Where(e => e.Point.IsEmpty != null).Select(e => e.Id),
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<PointEntity>().Where(e => e.Point != null).Select(e => e.Id));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Intersects_equal_to_null(bool async)
    {
        var lineString = Fixture.GeometryFactory.CreateLineString([new(0.5, -0.5), new(0.5, 0.5)]);

        await AssertQueryScalar(
            async,
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<LineStringEntity>().Where(e => e.LineString.Intersects(lineString) == null).Select(e => e.Id),
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<LineStringEntity>().Where(e => e.LineString == null).Select(e => e.Id));

        await AssertQueryScalar(
            async,
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<LineStringEntity>().Where(e => lineString.Intersects(e.LineString) == null).Select(e => e.Id),
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<LineStringEntity>().Where(e => e.LineString == null).Select(e => e.Id));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Intersects_not_equal_to_null(bool async)
    {
        var lineString = Fixture.GeometryFactory.CreateLineString([new(0.5, -0.5), new(0.5, 0.5)]);

        await AssertQueryScalar(
            async,
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<LineStringEntity>().Where(e => e.LineString.Intersects(lineString) != null).Select(e => e.Id),
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<LineStringEntity>().Where(e => e.LineString != null).Select(e => e.Id));

        await AssertQueryScalar(
            async,
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<LineStringEntity>().Where(e => lineString.Intersects(e.LineString) != null).Select(e => e.Id),
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ss => ss.Set<LineStringEntity>().Where(e => e.LineString != null).Select(e => e.Id));
    }
}
