// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
    public class SpatialData : IExpectedData
    {
        private readonly IReadOnlyList<PointEntity> _pointEntities;
        private readonly IReadOnlyList<GeoPointEntity> _geoPointEntities;
        private readonly IReadOnlyList<LineStringEntity> _lineStringEntities;
        private readonly IReadOnlyList<PolygonEntity> _polygonEntities;
        private readonly IReadOnlyList<MultiLineStringEntity> _multiLineStringEntities;

        public SpatialData(GeometryFactory factory)
        {
            _pointEntities = CreatePointEntities(factory);
            _geoPointEntities = CreateGeoPointEntities();
            _lineStringEntities = CreateLineStringEntities(factory);
            _polygonEntities = CreatePolygonEntities(factory);
            _multiLineStringEntities = CreateMultiLineStringEntities(factory);
        }

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
                    Point = factory.CreatePoint(
                        new Coordinate(0, 0))
                },
                new PointEntity
                {
                    Id = Guid.Parse("67A54C9B-4C3B-4B27-8B4E-C0335E50E551"),
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
                    Id = Guid.Parse("67A54C9B-4C3B-4B27-8B4E-C0335E50E552"),
                    Location = new GeoPoint(47.6233355, -122.34877)
                },
                new GeoPointEntity
                {
                    Id = Guid.Parse("67A54C9B-4C3B-4B27-8B4E-C0335E50E553"),
                    Location = new GeoPoint(47.5978429, -122.3308366)
                }
            };

        public static IReadOnlyList<LineStringEntity> CreateLineStringEntities(GeometryFactory factory)
            => new[]
            {
                new LineStringEntity
                {
                    Id = 1,
                    LineString = factory.CreateLineString(
                        new[]
                        {
                            new Coordinate(0, 0),
                            new Coordinate(1, 0)
                        })
                },
                new LineStringEntity
                {
                    Id = 2,
                    LineString = null
                }
            };

        public static IReadOnlyList<PolygonEntity> CreatePolygonEntities(GeometryFactory factory)
            => new[]
            {
                new PolygonEntity
                {
                    Id = Guid.Parse("2F39AADE-4D8D-42D2-88CE-775C84AB83B1"),
                    Polygon = factory.CreatePolygon(
                        new[]
                        {
                            new Coordinate(0, 0),
                            new Coordinate(1, 0),
                            new Coordinate(0, 1),
                            new Coordinate(0, 0)
                        })
                },
                new PolygonEntity
                {
                    Id = Guid.Parse("F1B00CB9-862B-417B-955A-F1F7688B2AB5"),
                    Polygon = null
                }
            };

        public static IReadOnlyList<MultiLineStringEntity> CreateMultiLineStringEntities(GeometryFactory factory)
            => new[]
            {
                new MultiLineStringEntity
                {
                    Id = 1,
                    MultiLineString = factory.CreateMultiLineString(
                        new[]
                        {
                            factory.CreateLineString(
                                new[]
                                {
                                    new Coordinate(0, 0),
                                    new Coordinate(0, 1)
                                }),
                            factory.CreateLineString(
                                new[]
                                {
                                    new Coordinate(1, 0),
                                    new Coordinate(1, 1)
                                })
                        })
                },
                new MultiLineStringEntity
                {
                    Id = 2,
                    MultiLineString = null
                }
            };
    }
}
