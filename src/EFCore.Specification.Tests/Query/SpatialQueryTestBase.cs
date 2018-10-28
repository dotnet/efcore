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
        public virtual Task SimpleSelect(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es,
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    Assert.Equal((IGeometry)e.Geometry, (IGeometry)a.Geometry, GeometryComparer.Instance);
                    Assert.Equal((IPoint)e.Point, (IPoint)a.Point, GeometryComparer.Instance);
                    Assert.Equal((Point)e.ConcretePoint, (Point)a.ConcretePoint, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task WithConversion(bool isAsync)
        {
            return AssertQuery<GeoPointEntity>(
                isAsync,
                es => es,
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);

                    Assert.Equal(e.Location.Lat, a.Location.Lat);
                    Assert.Equal(e.Location.Lon, a.Location.Lon);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Area(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Area = e.Polygon == null ? (double?)null : e.Polygon.Area }),
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
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task AsBinary(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Binary = e.Point == null ? null : e.Point.AsBinary() }),
                elementSorter: x => x.Id,
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
                es => es.Select(e => new { e.Id, Text = e.Point == null ? null : e.Point.AsText() }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((string)e.Text, (string)a.Text, WKTComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Boundary(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Boundary = e.Polygon == null ? null : e.Polygon.Boundary }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IGeometry)e.Boundary, (IGeometry)a.Boundary, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Buffer(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Buffer = e.Polygon == null ? null : e.Polygon.Buffer(1.0) }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IPoint)e.Buffer?.Centroid, (IPoint)a.Buffer?.Centroid, GeometryComparer.Instance);

                    if (e.Buffer == null)
                    {
                        Assert.Null(a.Buffer);
                    }
                    else if (AssertDistances)
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
                es => es.Select(e => new { e.Id, Buffer = e.Polygon == null ? null : e.Polygon.Buffer(1.0, 8) }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IPoint)e.Buffer?.Centroid, (IPoint)a.Buffer?.Centroid, GeometryComparer.Instance);

                    if (e.Buffer == null)
                    {
                        Assert.Null(a.Buffer);
                    }
                    else if (AssertDistances)
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
                es => es.Select(e => new { e.Id, Centroid = e.Polygon == null ? null : e.Polygon.Centroid }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IPoint)e.Centroid, (IPoint)a.Centroid, GeometryComparer.Instance);
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
                        Contains = e.Polygon == null ? (bool?)null : e.Polygon.Contains(point)
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ConvexHull(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, ConvexHull = e.Polygon == null ? null : e.Polygon.ConvexHull() }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IGeometry)e.ConvexHull, (IGeometry)a.ConvexHull, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IGeometryCollection_Count(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Count = e.MultiLineString == null ? (int?)null : e.MultiLineString.Count
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task LineString_Count(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Count = e.LineString == null ? (int?)null : ((LineString)e.LineString).Count
                    }),
                elementSorter: x => x.Id);
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
                    .Select(
                        e => new
                        {
                            e.Id,
                            CoveredBy = e.Point == null ? (bool?)null : e.Point.CoveredBy(polygon)
                        }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Covers(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0.25, 0.25));

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Covers = e.Polygon == null ? (bool?)null : e.Polygon.Covers(point) }),
                elementSorter: x => x.Id);
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
                        Crosses = e.LineString == null ? (bool?)null : e.LineString.Crosses(lineString)
                    }),
                elementSorter: x => x.Id);
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
                        Difference = e.Polygon == null ? null : e.Polygon.Difference(polygon)
                    }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IGeometry)e.Difference, (IGeometry)a.Difference, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Dimension(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Dimension = e.Point == null ? (Dimension?)null : e.Point.Dimension }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Disjoint(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(1, 1));

            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Disjoint = e.Polygon == null ? (bool?)null : e.Polygon.Disjoint(point)
                    }),
                elementSorter: x => x.Id);
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
                        Distance = e.Point == null ? (double?)null : e.Point.Distance(point)
                    }),
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
        public virtual Task Distance_geometry(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Geometry == null ? (double?)null : e.Geometry.Distance(point)
                    }),
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
        public virtual Task Distance_concrete(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.ConcretePoint == null ? (double?)null : e.ConcretePoint.Distance(point)
                    }),
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
        public virtual Task Distance_constant(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Point == null ? (double?)null : e.Point.Distance(new Point(0, 1))
                    }),
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
        public virtual Task Distance_constant_srid_4326(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Point == null ? (double?)null : e.Point.Distance(new Point(0, 1) { SRID = 4326 })
                    }),
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
        public virtual Task Distance_constant_lhs(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Point == null ? (double?)null : new Point(0, 1).Distance(e.Point)
                    }),
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
        public virtual Task Distance_on_converted_geometry_type(bool isAsync)
        {
            var point = new GeoPoint(1, 0);

            return AssertQuery<GeoPointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Location.Distance(point)
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distance_on_converted_geometry_type_constant(bool isAsync)
        {
            return AssertQuery<GeoPointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = e.Location.Distance(new GeoPoint(1, 0))
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Distance_on_converted_geometry_type_constant_lhs(bool isAsync)
        {
            return AssertQuery<GeoPointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Distance = new GeoPoint(1, 0).Distance(e.Location)
                    }),
                elementSorter: e => e.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task EndPoint(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, EndPoint = e.LineString == null ? null : e.LineString.EndPoint }),
                elementSorter: e => e.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Envelope(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Envelope = e.Polygon == null ? null : e.Polygon.Envelope }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IGeometry)e.Envelope, (IGeometry)a.Envelope, GeometryComparer.Instance);
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
                    .Select(
                    e => new
                    {
                        e.Id,
                        EqualsTopologically = e.Point == null ? (bool?)null : e.Point.EqualsTopologically(point)
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ExteriorRing(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, ExteriorRing = e.Polygon == null ? null : e.Polygon.ExteriorRing }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GeometryType(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new { e.Id, GeometryType = e.Point == null ? null : e.Point.GeometryType }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetGeometryN(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Geometry0 = e.MultiLineString == null ? null : e.MultiLineString.GetGeometryN(0)
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetInteriorRingN(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        InteriorRing0 = e.Polygon == null || e.Polygon.NumInteriorRings == 0
                            ? null
                            : e.Polygon.GetInteriorRingN(0)
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task GetPointN(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Point0 = e.LineString == null ? null : e.LineString.GetPointN(0) }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task InteriorPoint(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
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
                        Intersection = e.Polygon == null ? null : e.Polygon.Intersection(polygon)
                    }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IGeometry)e.Intersection, (IGeometry)a.Intersection, GeometryComparer.Instance);
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
                        Intersects = e.LineString == null ? (bool?)null : e.LineString.Intersects(lineString)
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ICurve_IsClosed(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        IsClosed = e.LineString == null ? (bool?)null : e.LineString.IsClosed
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IMultiCurve_IsClosed(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        IsClosed = e.MultiLineString == null ? (bool?)null : e.MultiLineString.IsClosed
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsEmpty(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        IsEmpty = e.MultiLineString == null ? (bool?)null : e.MultiLineString.IsEmpty
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsRing(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, IsRing = e.LineString == null ? (bool?)null : e.LineString.IsRing }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsSimple(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(
                    e =>
                    new
                    {
                        e.Id,
                        IsSimple = e.LineString == null ? (bool?)null : e.LineString.IsSimple
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsValid(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es
                    .Select(e => new { e.Id, IsValid = e.Point == null ? (bool?)null : e.Point.IsValid }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task IsWithinDistance(bool isAsync)
        {
            var point = Fixture.GeometryFactory.CreatePoint(new Coordinate(0, 1));

            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        IsWithinDistance = e.Point == null ? (bool?)null : e.Point.IsWithinDistance(point, 1)
                    }),
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
        public virtual Task Item(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Item0 = e.MultiLineString == null ? null : e.MultiLineString[0]
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Length(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Length = e.LineString == null ? (double?)null : e.LineString.Length }),
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
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task M(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, M = e.Point == null ? (double?)null : e.Point.M }),
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
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumGeometries(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        NumGeometries = e.MultiLineString == null ? (int?)null : e.MultiLineString.NumGeometries
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumInteriorRings(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        NumInteriorRings = e.Polygon == null ? (int?)null : e.Polygon.NumInteriorRings
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task NumPoints(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        NumPoints = e.LineString == null ? (int?)null : e.LineString.NumPoints
                    }),
                elementSorter: x => x.Id);
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
                        OgcGeometryType = e.Point == null ? (OgcGeometryType?)null : e.Point.OgcGeometryType
                    }),
                elementSorter: x => x.Id);
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
                        Overlaps = e.Polygon == null ? (bool?)null : e.Polygon.Overlaps(polygon)
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task PointOnSurface(bool isAsync)
        {
            return AssertQuery<PolygonEntity>(
                isAsync,
                es => es.Select(
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
                        Relate = e.Polygon == null ? (bool?)null : e.Polygon.Relate(polygon, "212111212")
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Reverse(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Reverse = e.LineString == null ? null : e.LineString.Reverse() }),
                elementSorter: x => x.Id);
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
                        SRID = e.Point == null ? (int?)null : e.Point.SRID
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SRID_geometry(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        SRID = e.Geometry == null ? (int?)null : e.Geometry.SRID
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task SRID_concrete(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        SRID = e.ConcretePoint == null ? (int?)null : e.ConcretePoint.SRID
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task StartPoint(bool isAsync)
        {
            return AssertQuery<LineStringEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, StartPoint = e.LineString == null ? null : e.LineString.StartPoint }),
                elementSorter: x => x.Id);
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
                        SymmetricDifference = e.Polygon == null ? null : e.Polygon.SymmetricDifference(polygon)
                    }),
                elementSorter: x => x.Id,
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
                        Touches = e.Polygon == null ? (bool?)null : e.Polygon.Touches(polygon)
                    }),
                elementSorter: x => x.Id);
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
                        Union = e.Polygon == null ? null : e.Polygon.Union(polygon)
                    }),
                elementSorter: x => x.Id,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.Id, a.Id);
                    Assert.Equal((IGeometry)e.Union, (IGeometry)a.Union, GeometryComparer.Instance);
                });
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Union_void(bool isAsync)
        {
            return AssertQuery<MultiLineStringEntity>(
                isAsync,
                es => es.Select(
                    e => new
                    {
                        e.Id,
                        Union = e.MultiLineString == null ? null : e.MultiLineString.Union()
                    }),
                elementSorter: x => x.Id);
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
                        Within = e.Point == null ? (bool?)null : e.Point.Within(polygon)
                    }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task X(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, X = e.Point == null ? (double?)null : e.Point.X }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Y(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Y = e.Point == null ? (double?)null : e.Point.Y }),
                elementSorter: x => x.Id);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Z(bool isAsync)
        {
            return AssertQuery<PointEntity>(
                isAsync,
                es => es.Select(e => new { e.Id, Z = e.Point == null ? (double?)null : e.Point.Z }),
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
        }
    }
#endif
}
