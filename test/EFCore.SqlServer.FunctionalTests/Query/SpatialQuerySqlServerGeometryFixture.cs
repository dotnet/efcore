// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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
                x.Property(e => e.PointZ).HasColumnType("geometry");
                x.Property(e => e.PointM).HasColumnType("geometry");
                x.Property(e => e.PointZM).HasColumnType("geometry");
            });
        modelBuilder.Entity<PolygonEntity>().Property(e => e.Polygon).HasColumnType("geometry");
        modelBuilder.Entity<GeoPointEntity>().Property(e => e.Location).HasColumnType("geometry");
    }

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddSingleton<IRelationalTypeMappingSource, ReplacementTypeMappingSource>();

    protected class ReplacementTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies) : SqlServerTypeMappingSource(dependencies, relationalDependencies)
    {
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => mappingInfo.ClrType == typeof(GeoPoint)
                ? ((RelationalTypeMapping)base.FindMapping(typeof(Point))
                    .WithComposedConverter(new GeoPointConverter())).WithStoreTypeAndSize("geometry", null)
                : base.FindMapping(mappingInfo);
    }
}
