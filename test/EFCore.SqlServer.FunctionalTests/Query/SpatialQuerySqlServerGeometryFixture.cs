// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SpatialQuerySqlServerGeometryFixture : SpatialQuerySqlServerFixture
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<LineStringEntity>().Property(e => e.LineString).HasColumnType("geometry");
            modelBuilder.Entity<MultiLineStringEntity>().Property(e => e.MultiLineString).HasColumnType("geometry");
            modelBuilder.Entity<PointEntity>(
                x =>
                {
                    x.Property(e => e.Geometry).HasColumnType("geometry");
                    x.Property(e => e.Point).HasColumnType("geometry");
                    x.Property(e => e.ConcretePoint).HasColumnType("geometry");
                });
            modelBuilder.Entity<PolygonEntity>().Property(e => e.Polygon).HasColumnType("geometry");
            modelBuilder.Entity<GeoPointEntity>().Property(e => e.Location).HasColumnType("geometry");
        }

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton<IRelationalTypeMappingSource, ReplacementTypeMappingSource>();

        protected class ReplacementTypeMappingSource : SqlServerTypeMappingSource
        {
            public ReplacementTypeMappingSource(
                TypeMappingSourceDependencies dependencies,
                RelationalTypeMappingSourceDependencies relationalDependencies)
                : base(dependencies, relationalDependencies)
            {
            }

            protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
                => mappingInfo.ClrType == typeof(GeoPoint)
                    ? ((RelationalTypeMapping)base.FindMapping(typeof(IPoint))
                        .Clone(new GeoPointConverter())).Clone("geometry", null)
                    : base.FindMapping(mappingInfo);
        }
    }
}
