// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class SpatialQueryFixtureBase : SharedStoreFixtureBase<SpatialContext>, IQueryFixtureBase
{
    private GeometryFactory _geometryFactory;

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
        => new SpatialData(GeometryFactory);

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(PointEntity), e => ((PointEntity)e)?.Id },
        { typeof(LineStringEntity), e => ((LineStringEntity)e)?.Id },
        { typeof(PolygonEntity), e => ((PolygonEntity)e)?.Id },
        { typeof(MultiLineStringEntity), e => ((MultiLineStringEntity)e)?.Id },
        { typeof(GeoPointEntity), e => ((GeoPointEntity)e)?.Id },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(PointEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (PointEntity)e;
                    var aa = (PointEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Geometry, aa.Geometry, GeometryComparer.Instance);
                    Assert.Equal(ee.Point, aa.Point, GeometryComparer.Instance);
                    Assert.Equal(ee.PointZ, aa.PointZ, GeometryComparer.Instance);
                    Assert.Equal(ee.PointM, aa.PointM, GeometryComparer.Instance);
                    Assert.Equal(ee.PointZM, aa.PointZM, GeometryComparer.Instance);
                }
            }
        },
        {
            typeof(LineStringEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (LineStringEntity)e;
                    var aa = (LineStringEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.LineString, aa.LineString, GeometryComparer.Instance);
                }
            }
        },
        {
            typeof(PolygonEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (PolygonEntity)e;
                    var aa = (PolygonEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Polygon, aa.Polygon, GeometryComparer.Instance);
                }
            }
        },
        {
            typeof(MultiLineStringEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (MultiLineStringEntity)e;
                    var aa = (MultiLineStringEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.MultiLineString != null, aa.MultiLineString != null);
                    if (ee.MultiLineString != null)
                    {
                        Assert.Equal(ee.MultiLineString.Count, aa.MultiLineString.Count);
                        Assert.Equal(ee.MultiLineString.Area, aa.MultiLineString.Area);
                        for (var i = 0; i < ee.MultiLineString.Count; i++)
                        {
                            Assert.Equal(ee.MultiLineString[i], aa.MultiLineString[i], GeometryComparer.Instance);
                        }
                    }
                }
            }
        },
        {
            typeof(GeoPointEntity), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (GeoPointEntity)e;
                    var aa = (GeoPointEntity)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Location.Lat, aa.Location.Lat);
                    Assert.Equal(ee.Location.Lon, aa.Location.Lon);
                }
            }
        },
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public virtual GeometryFactory GeometryFactory
        => LazyInitializer.EnsureInitialized(
            ref _geometryFactory,
            () => NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0));

    protected override string StoreName
        => "SpatialQueryTest";

    public override SpatialContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return context;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<PointEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<LineStringEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<PolygonEntity>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<MultiLineStringEntity>().Property(e => e.Id).ValueGeneratedNever();

        modelBuilder.Entity<GeoPointEntity>(
            b =>
            {
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.Location).HasConversion(new GeoPointConverter(GeometryFactory));
            });
    }

    protected override Task SeedAsync(SpatialContext context)
        => SpatialContext.SeedAsync(context, GeometryFactory);
}
