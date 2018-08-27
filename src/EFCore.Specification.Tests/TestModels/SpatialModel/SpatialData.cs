// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.TestModels.SpatialModel
{
    public class SpatialData : IExpectedData
    {
        private readonly IReadOnlyList<PointEntity> _pointEntities;
        private readonly IReadOnlyList<LineStringEntity> _lineStringEntities;
        private readonly IReadOnlyList<PolygonEntity> _polygonEntities;
        private readonly IReadOnlyList<MultiLineStringEntity> _multiLineStringEntities;

        public SpatialData()
        {
            _pointEntities = CreatePointEntities();
            _lineStringEntities = CreateLineStringEntities();
            _polygonEntities = CreatePolygonEntities();
            _multiLineStringEntities = CreateMultiLineStringEntities();
        }

        public virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(PointEntity))
            {
                return (IQueryable<TEntity>)_pointEntities.AsQueryable();
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

        public static IReadOnlyList<PointEntity> CreatePointEntities()
            => new[]
            {
                new PointEntity
                {
                    Id = 1,
                    Point = new Point(0, 0)
                }
            };

        public static IReadOnlyList<LineStringEntity> CreateLineStringEntities()
            => new[]
            {
                new LineStringEntity
                {
                    Id = 1,
                    LineString = new LineString(
                        new[]
                        {
                            new Coordinate(0, 0),
                            new Coordinate(1, 0)
                        })
                }
            };

        public static IReadOnlyList<PolygonEntity> CreatePolygonEntities()
            => new[]
            {
                new PolygonEntity
                {
                    Id = 1,
                    Polygon = new Polygon(
                        new LinearRing(
                            new[]
                            {
                                new Coordinate(0, 0),
                                new Coordinate(0, 1),
                                new Coordinate(1, 0),
                                new Coordinate(0, 0)
                            }))
                }
            };

        public static IReadOnlyList<MultiLineStringEntity> CreateMultiLineStringEntities()
            => new[]
            {
                new MultiLineStringEntity
                {
                    Id = 1,
                    MultiLineString = new MultiLineString(
                        new[]
                        {
                            new LineString(
                                new[]
                                {
                                    new Coordinate(0, 0),
                                    new Coordinate(0, 1)
                                }),
                            new LineString(
                                new[]
                                {
                                    new Coordinate(1, 0),
                                    new Coordinate(1, 1)
                                })
                        })
                }
            };
    }
}
