// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class SpatialQuerySqlServerFixture : SpatialQueryRelationalFixture
{
    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddEntityFrameworkSqlServerNetTopologySuite();

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new SqlServerDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();

        return optionsBuilder;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.HasDbFunction(
            typeof(GeoExtensions).GetMethod(nameof(GeoExtensions.Distance)),
            b => b.HasTranslation(
                e => new SqlFunctionExpression(
                    instance: e[0],
                    "STDistance",
                    arguments: e.Skip(1),
                    nullable: true,
                    instancePropagatesNullability: true,
                    argumentsPropagateNullability: e.Skip(1).Select(a => true),
                    typeof(double),
                    null)));
    }
}
