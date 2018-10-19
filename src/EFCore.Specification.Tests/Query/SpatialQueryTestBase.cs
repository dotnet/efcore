// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using NetTopologySuite.Geometries;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
#if !Test21
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
        public virtual Task Area(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.Polygon.Area }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.Area, a.Area);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task AsBinary(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Where(e => e.Id == PointEntity.WellKnownId).Select(e => new { e.Id, Binary = e.Point.AsBinary() }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Binary, a.Binary);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task AsText(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Where(e => e.Id == PointEntity.WellKnownId).Select(e => new { e.Id, Text = e.Point.AsText() }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Text, a.Text, WKTComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Boundary(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.Polygon.Boundary }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Boundary, a.Boundary, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Buffer(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Buffer = e.Polygon.Buffer(1.0) }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Buffer.Centroid, a.Buffer.Centroid, GeometryComparer.Instance);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.Buffer.Area, a.Buffer.Area, precision: 0);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Buffer_quadrantSegments(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Buffer = e.Polygon.Buffer(1.0, 8) }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Buffer.Centroid, a.Buffer.Centroid, GeometryComparer.Instance);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.Buffer.Area, a.Buffer.Area, precision: 0);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Centroid(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.Polygon.Centroid }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Centroid, a.Centroid, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0.25, 0.25));

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Contains = e.Polygon.Contains(point)
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ConvexHull(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, ConvexHull = e.Polygon.ConvexHull() }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.ConvexHull, a.ConvexHull, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IGeometryCollection_Count(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.MultiLineString.Count }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LineString_Count(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, ((LineString)e.LineString).Count }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task CoveredBy(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(-1, -1),
                    new Coordinate(2, -1),
                    new Coordinate(2, 2),
                    new Coordinate(-1, 2),
                    new Coordinate(-1, -1)
                });

            return AssertQuery<PointEntity>(
                isAsync,
                es => es
                    .Where(e => e.Id == PointEntity.WellKnownId)
                    .Select(
                        e => new
                        {
                            e.Id,
                            CoveredBy = e.Point.CoveredBy(polygon)
                        }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Covers(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0.25, 0.25));

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Covers = e.Polygon.Covers(point) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Crosses(bool isAsync)
        {
            var lineString = Fixture.GeometryFactory.CreateLineString(
                new[]
                {
                    new Coordinate(0.5, -0.5),
                    new Coordinate(0.5, 0.5)
                });

            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Crosses = e.LineString.Crosses(lineString)
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Difference(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(1, 1),
                    new Coordinate(0, 0)
                });

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Difference = e.Polygon.Difference(polygon)
                    }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Difference, a.Difference, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Dimension(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Where(e => e.Id == PointEntity.WellKnownId).Select(e => new { e.Id, e.Point.Dimension }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Disjoint(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(1, 1));

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Disjoint = e.Polygon.Disjoint(point) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distance(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Point == null ? -1 : e.Point.Distance(point)
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.Distance, a.Distance);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distance_constant(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Point == null ? -1 : e.Point.Distance(new Point(0, 1))
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.Distance, a.Distance);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distance_constant_srid_4326(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Point == null ? -1 : e.Point.Distance(new Point(0, 1) { SRID = 4326 })
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.Distance, a.Distance);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distance_constant_lhs(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Point == null ? -1 : new Point(0, 1).Distance(e.Point)
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.Distance, a.Distance);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task EndPoint(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.EndPoint }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Envelope(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.Polygon.Envelope }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Envelope, a.Envelope, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task EqualsTopologically(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 0));

            return AssertQuery<PointEntity>(
                isAsync,
                es => es
                    .Where(e => e.Id == PointEntity.WellKnownId)
                    .Select(e => new { e.Id, EqualsTopologically = e.Point.EqualsTopologically(point) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ExteriorRing(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(isAsync, es => es.Select(e => new { e.Id, e.Polygon.ExteriorRing }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GeometryType(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new { e.Id, GeometryType = e.Point == null ? null : e.Point.GeometryType }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetGeometryN(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Geometry0 = e.MultiLineString.GetGeometryN(0) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetInteriorRingN(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es =>
                    from e in es
                    where e.Polygon.NumInteriorRings > 0
                    select new { e.Id, InteriorRing0 = e.Polygon.GetInteriorRingN(0) });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetPointN(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Point0 = e.LineString.GetPointN(0) }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task InteriorPoint(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.Polygon.InteriorPoint, e.Polygon }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.True(a.Polygon.Contains(e.InteriorPoint));
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersection(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(1, 1),
                    new Coordinate(0, 0)
                });

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Intersection = e.Polygon.Intersection(polygon)
                    }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Intersection, a.Intersection, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersects(bool isAsync)
        {
            var lineString = Fixture.GeometryFactory.CreateLineString(
                new[]
                {
                    new Coordinate(0.5, -0.5),
                    new Coordinate(0.5, 0.5)
                });

            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Intersects = e.LineString.Intersects(lineString)
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ICurve_IsClosed(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.IsClosed }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IMultiCurve_IsClosed(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.MultiLineString.IsClosed }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsEmpty(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.MultiLineString.IsEmpty }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsRing(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.IsRing }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsSimple(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.IsSimple }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsValid(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es
                    .Where(e => e.Id == PointEntity.WellKnownId)
                    .Select(e => new { e.Id, e.Point.IsValid }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsWithinDistance(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, IsWithinDistance = e.Point != null && e.Point.IsWithinDistance(point, 1) }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.IsWithinDistance, a.IsWithinDistance);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Item(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Item0 = e.MultiLineString[0] }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Length(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.LineString.Length }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    if (AssertDistances)
                    {
                        Assert.Equal(e.Length, a.Length);
                    }
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task M(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, M = e.Point == null ? null : (double?)e.Point.M }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.M ?? double.NaN, a.M ?? double.NaN);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumGeometries(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.MultiLineString.NumGeometries }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumInteriorRings(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.Polygon.NumInteriorRings }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumPoints(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.NumPoints }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task OgcGeometryType(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        OgcGeometryType = e.Point == null ? (OgcGeometryType)0 : e.Point.OgcGeometryType
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Overlaps(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(1, 1),
                    new Coordinate(0, 0)
                });

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Overlaps = e.Polygon.Overlaps(polygon)
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task PointOnSurface(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.Polygon.PointOnSurface, e.Polygon }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.True(a.Polygon.Contains(e.PointOnSurface));
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Relate(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(1, 1),
                    new Coordinate(0, 0)
                });

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Relate = e.Polygon.Relate(polygon, "212111212")
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Reverse(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Reverse = e.LineString.Reverse() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SRID(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        SRID = e.Point == null ? -1 : e.Point.SRID
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task StartPoint(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.StartPoint }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SymmetricDifference(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(1, 1),
                    new Coordinate(0, 0)
                });

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        SymmetricDifference = e.Polygon.SymmetricDifference(polygon)
                    }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(
                        (IGeometry)e.SymmetricDifference,
                        (IGeometry)a.SymmetricDifference,
                        GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ToBinary(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Binary = e.Point == null ? null : ((Geometry)e.Point).ToBinary() }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Binary, a.Binary);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ToText(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Text = e.Point == null ? null : ((Geometry)e.Point).ToText() }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((string)e.Text, (string)a.Text, WKTComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Touches(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(0, 1),
                    new Coordinate(1, 0),
                    new Coordinate(1, 1),
                    new Coordinate(0, 1)
                });

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Touches = e.Polygon.Touches(polygon)
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(1, 1),
                    new Coordinate(0, 0)
                });

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Union = e.Polygon.Union(polygon)
                    }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Union, a.Union, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_void(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Union = e.MultiLineString.Union() }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Within(bool isAsync)
        {
            var polygon = Fixture.GeometryFactory.CreatePolygon(
                new[]
                {
                    new Coordinate(-1, -1),
                    new Coordinate(2, -1),
                    new Coordinate(2, 2),
                    new Coordinate(-1, 2),
                    new Coordinate(-1, -1)
                });

            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Within = e.Point != null && e.Point.Within(polygon)
                    }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task X(bool isAsync)
        {
            return AssertQuery<PointEntity>(isAsync, es => es.Select(e => new { e.Id, X = e.Point == null ? -1.0 : e.Point.X }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Y(bool isAsync)
        {
            return AssertQuery<PointEntity>(isAsync, es => es.Select(e => new { e.Id, Y = e.Point == null ? -1.0 : e.Point.Y }));
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Z(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Z = e.Point == null ? -1.0 : (double?)e.Point.Z }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((double?)e.Z, (double?)(a.Z ?? double.NaN));
                });
        }
    }
#endif
}
