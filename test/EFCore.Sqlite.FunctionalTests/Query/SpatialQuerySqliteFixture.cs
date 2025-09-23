// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class SpatialQuerySqliteFixture : SpatialQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => SqliteTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddEntityFrameworkSqliteNetTopologySuite()
            .AddSingleton<IRelationalTypeMappingSource, ReplacementTypeMappingSource>();

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new SqliteDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();

        return optionsBuilder;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.HasDbFunction(
            typeof(GeoExtensions).GetMethod(nameof(GeoExtensions.Distance)),
            b => b.HasTranslation(
                e => new SqlFunctionExpression(
                    "Distance",
                    arguments: e,
                    nullable: true,
                    argumentsPropagateNullability: e.Select(a => true).ToList(),
                    typeof(double),
                    null)));
    }

    private class ReplacementTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies) : SqliteTypeMappingSource(dependencies, relationalDependencies)
    {
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
            => mappingInfo.ClrType == typeof(GeoPoint)
                ? ((RelationalTypeMapping)base.FindMapping(typeof(Point))
                    .WithComposedConverter(new GeoPointConverter()))
                .WithStoreTypeAndSize("geometry", null)
                : base.FindMapping(mappingInfo);
    }
}
