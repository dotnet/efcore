// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Area(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(isAsync, es => es.Select(e => new { e.Id, e.Polygon.Area }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task AsBinary(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Binary = e.Point.AsBinary() }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Binary, a.Binary);
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task AsText(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Text = e.Point.AsText() }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Text, a.Text, WKTComparer.Instance);
                });
        }

        [Theory]
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Buffer(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Buffer = e.Polygon.Buffer(1.0) }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Buffer, a.Buffer, GeometryComparer.Instance);
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Buffer_quadrantSegments(bool isAsync)
        {
            await AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Buffer = e.Polygon.Buffer(1.0, 8) }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(0, e.Buffer.SymmetricDifference(a.Buffer).Area);
                });
        }

        [Theory]
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Contains(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Contains = e.Polygon.Contains(new Point(0.5, 0.25)) }));
        }

        [Theory]
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IGeometryCollection_Count(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.MultiLineString.Count }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LineString_Count(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, ((LineString)e.LineString).Count }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task CoveredBy(bool isAsync)
        {
            await AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        CoveredBy = e.Point.CoveredBy(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(-1, -1),
                                        new Coordinate(-1, 2),
                                        new Coordinate(2, 2),
                                        new Coordinate(2, -1),
                                        new Coordinate(-1, -1)
                                    })))
                    }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Covers(bool isAsync)
        {
            await AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Covers = e.Polygon.Covers(new Point(0.5, 0.25)) }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Crosses(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Crosses = e.LineString.Crosses(
                            new LineString(
                                new[]
                                {
                                    new Coordinate(0.5, -0.5),
                                    new Coordinate(0.5, 0.5)
                                }))
                    }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Difference(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Difference = e.Polygon.Difference(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(0, 0),
                                        new Coordinate(1, 0),
                                        new Coordinate(1, 1),
                                        new Coordinate(0, 0)
                                    })))
                    }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Difference, a.Difference, GeometryComparer.Instance);
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Dimension(bool isAsync)
        {
            return AssertQuery<PointEntity>(isAsync, es => es.Select(e => new { e.Id, e.Point.Dimension }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Disjoint(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Disjoint = e.Polygon.Disjoint(new Point(1, 0)) }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distance(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Distance = e.Point.Distance(new Point(0, 1)) }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task EndPoint(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.EndPoint }));
        }

        [Theory]
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task EqualsTopologically(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, EqualsTopologically = e.Point.EqualsTopologically(new Point(0, 0)) }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ExteriorRing(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(isAsync, es => es.Select(e => new { e.Id, e.Polygon.ExteriorRing }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GeometryType(bool isAsync)
        {
            return AssertQuery<PointEntity>(isAsync, es => es.Select(e => new { e.Id, e.Point.GeometryType }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetGeometryN(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Geometry0 = e.MultiLineString.GetGeometryN(0) }));
        }

        [Theory]
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetPointN(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Point0 = e.LineString.GetPointN(0) }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersection(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Intersection = e.Polygon.Intersection(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(0, 0),
                                        new Coordinate(1, 0),
                                        new Coordinate(1, 1),
                                        new Coordinate(0, 0)
                                    })))
                    }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Intersection, a.Intersection, GeometryComparer.Instance);
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Intersects(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Intersects = e.LineString.Intersects(
                            new LineString(
                                new[]
                                {
                                    new Coordinate(0.5, -0.5),
                                    new Coordinate(0.5, 0.5)
                                }))
                    }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ICurve_IsClosed(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.IsClosed }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IMultiCurve_IsClosed(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.MultiLineString.IsClosed }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsEmpty(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.MultiLineString.IsEmpty }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsRing(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.IsRing }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsSimple(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.IsSimple }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsValid(bool isAsync)
        {
            return AssertQuery<PointEntity>(isAsync, es => es.Select(e => new { e.Id, e.Point.IsValid }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Item(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Item0 = e.MultiLineString[0] }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Length(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.Length }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task M(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, M = (double?)e.Point.M }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.M, a.M ?? double.NaN);
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumGeometries(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.MultiLineString.NumGeometries }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumInteriorRings(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, e.Polygon.NumInteriorRings }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumPoints(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.NumPoints }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Overlaps(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Overlaps = e.Polygon.Overlaps(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(0, 0),
                                        new Coordinate(1, 0),
                                        new Coordinate(1, 1),
                                        new Coordinate(0, 0)
                                    })))
                    }));
        }

        [Theory]
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Relate(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Relate = e.Polygon.Relate(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(0, 0),
                                        new Coordinate(1, 0),
                                        new Coordinate(1, 1),
                                        new Coordinate(0, 0)
                                    })),
                            "212111212")
                    }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Reverse(bool isAsync)
        {
            await AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Reverse = e.LineString.Reverse() }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SRID(bool isAsync)
        {
            return AssertQuery<PointEntity>(isAsync, es => es.Select(e => new { e.Id, e.Point.SRID }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task StartPoint(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(isAsync, es => es.Select(e => new { e.Id, e.LineString.StartPoint }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SymmetricDifference(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        SymmetricDifference = e.Polygon.SymmetricDifference(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(0, 0),
                                        new Coordinate(1, 0),
                                        new Coordinate(1, 1),
                                        new Coordinate(0, 0)
                                    })))
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

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ToBinary(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Binary = ((Geometry)e.Point).ToBinary() }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Binary, a.Binary);
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ToText(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Text = ((Geometry)e.Point).ToText() }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Text, a.Text, WKTComparer.Instance);
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Touches(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Touches = e.Polygon.Touches(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(0, 1),
                                        new Coordinate(1, 1),
                                        new Coordinate(1, 0),
                                        new Coordinate(0, 1)
                                    })))
                    }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Union = e.Polygon.Union(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(0, 0),
                                        new Coordinate(1, 0),
                                        new Coordinate(1, 1),
                                        new Coordinate(0, 0)
                                    })))
                    }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Union, a.Union, GeometryComparer.Instance);
                });
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Within(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Within = e.Point.Within(
                            new Polygon(
                                new LinearRing(
                                    new[]
                                    {
                                        new Coordinate(-1, -1),
                                        new Coordinate(-1, 2),
                                        new Coordinate(2, 2),
                                        new Coordinate(2, -1),
                                        new Coordinate(-1, -1)
                                    })))
                    }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task X(bool isAsync)
        {
            return AssertQuery<PointEntity>(isAsync, es => es.Select(e => new { e.Id, e.Point.X }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Y(bool isAsync)
        {
            return AssertQuery<PointEntity>(isAsync, es => es.Select(e => new { e.Id, e.Point.Y }));
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Z(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Z = (double?)e.Point.Z }),
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal(e.Z, a.Z ?? double.NaN);
                });
        }
    }
#endif
}
