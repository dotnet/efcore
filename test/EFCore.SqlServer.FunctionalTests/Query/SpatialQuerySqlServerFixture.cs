// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
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

        protected override bool CanExecuteQueryString => true;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasDbFunction(
                typeof(GeoExtensions).GetMethod(nameof(GeoExtensions.Distance)),
                b => b.HasTranslation(
                    e => SqlFunctionExpression.Create(
                        instance: e.First(),
                        "STDistance",
                        arguments: e.Skip(1),
                        nullResultAllowed: true,
                        instancPropagatesNullability: true,
                        argumentsPropagateNullability: e.Skip(1).Select(a => true),
                        typeof(double),
                        null)));
        }
    }
}
