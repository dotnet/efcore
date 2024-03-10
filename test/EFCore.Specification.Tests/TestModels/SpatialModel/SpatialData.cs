// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

#nullable disable

public class SpatialData(GeometryFactory factory) : ISetSource
{
    private readonly IReadOnlyList<PointEntity> _pointEntities = CreatePointEntities(factory);
    private readonly IReadOnlyList<GeoPointEntity> _geoPointEntities = CreateGeoPointEntities();
    private readonly IReadOnlyList<LineStringEntity> _lineStringEntities = CreateLineStringEntities(factory);
    private readonly IReadOnlyList<PolygonEntity> _polygonEntities = CreatePolygonEntities(factory);
    private readonly IReadOnlyList<MultiLineStringEntity> _multiLineStringEntities = CreateMultiLineStringEntities(factory);

    public virtual IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(PointEntity))
        {
            return (IQueryable<TEntity>)_pointEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(GeoPointEntity))
        {
            return (IQueryable<TEntity>)_geoPointEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(LineStringEntity))
        {
            return (IQueryable<TEntity>)_lineStringEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(PolygonEntity))
        {
            return (IQueryable<TEntity>)_polygonEntities.AsQueryable();
        }

        if (typeof(TEntity) == typeof(MultiLineStringEntity))
        {
            return (IQueryable<TEntity>)_multiLineStringEntities.AsQueryable();
        }

        throw new InvalidOperationException("Unknown entity type: " + typeof(TEntity));
    }

    public static IReadOnlyList<PointEntity> CreatePointEntities(GeometryFactory factory)
    {
        var entities = new[]
        {
            new PointEntity
            {
                Id = PointEntity.WellKnownId,
                Group = "A",
                Point = factory.CreatePoint(new Coordinate(0, 0)),
                PointZ = factory.CreatePoint(new CoordinateZ(0, 0, 0)),
                PointM = factory.CreatePoint(new CoordinateM(0, 0, 0)),
                PointZM = factory.CreatePoint(new CoordinateZM(0, 0, 0, 0))
            },
            new PointEntity
            {
                Id = Guid.Parse("2F39AADE-4D8D-42D2-88CE-775C84AB83B2"),
                Group = "A",
                Point = factory.CreatePoint(new Coordinate(1, 1)),
                PointZ = factory.CreatePoint(new CoordinateZ(1, 1, 1)),
                PointM = factory.CreatePoint(new CoordinateM(1, 1, 1)),
                PointZM = factory.CreatePoint(new CoordinateZM(1, 1, 1, 1))
            },
            new PointEntity
            {
                Id = Guid.Parse("67A54C9B-4C3B-4B27-8B4E-C0335E50E551"),
                Group = "B",
                Point = null
            }
        };

        foreach (var entity in entities)
        {
            entity.Geometry = entity.Point?.Copy();
        }

        return entities;
    }

    public static IReadOnlyList<GeoPointEntity> CreateGeoPointEntities()
        => new[]
        {
            new GeoPointEntity
            {
                Id = Guid.Parse("67A54C9B-4C3B-4B27-8B4E-C0335E50E552"), Location = new GeoPoint(47.6233355, -122.34877)
            },
            new GeoPointEntity
            {
                Id = Guid.Parse("67A54C9B-4C3B-4B27-8B4E-C0335E50E553"), Location = new GeoPoint(47.5978429, -122.3308366)
            }
        };

    public static IReadOnlyList<LineStringEntity> CreateLineStringEntities(GeometryFactory factory)
        => new[]
        {
            new LineStringEntity
            {
                Id = 1,
                LineString = factory.CreateLineString([new(0, 0), new(1, 0)])
            },
            new LineStringEntity { Id = 2, LineString = null }
        };

    public static IReadOnlyList<PolygonEntity> CreatePolygonEntities(GeometryFactory factory)
        => new[]
        {
            new PolygonEntity
            {
                Id = Guid.Parse("2F39AADE-4D8D-42D2-88CE-775C84AB83B1"),
                Polygon = factory.CreatePolygon(
                    [new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1), new Coordinate(0, 0)])
            },
            new PolygonEntity { Id = Guid.Parse("F1B00CB9-862B-417B-955A-F1F7688B2AB5"), Polygon = null }
        };

    public static IReadOnlyList<MultiLineStringEntity> CreateMultiLineStringEntities(GeometryFactory factory)
        => new[]
        {
            new MultiLineStringEntity
            {
                Id = 1,
                MultiLineString = factory.CreateMultiLineString(
                [
                    factory.CreateLineString([new(0, 0), new(0, 1)]),
                        factory.CreateLineString([new(1, 0), new(1, 1)])
                ])
            },
            new MultiLineStringEntity { Id = 2, MultiLineString = null }
        };
}
