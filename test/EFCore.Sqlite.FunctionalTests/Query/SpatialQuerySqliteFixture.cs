// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore.Query
{
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
                    e => SqlFunctionExpression.Create(
                        "Distance",
                        arguments: e,
                        nullResultAllowed: true,
                        argumentsPropagateNullability: e.Select(a => true).ToList(),
                        typeof(double),
                        null)));
        }

        protected override void Clean(DbContext context)
        {
            context.Database.ExecuteSqlRaw("DROP VIEW IF EXISTS vector_layers");
            context.Database.ExecuteSqlRaw("DROP VIEW IF EXISTS vector_layers_auth");
            context.Database.ExecuteSqlRaw("DROP VIEW IF EXISTS vector_layers_statistics");
            context.Database.ExecuteSqlRaw("DROP VIEW IF EXISTS vector_layers_field_infos");
        }

        private class ReplacementTypeMappingSource : SqliteTypeMappingSource
        {
            public ReplacementTypeMappingSource(
                TypeMappingSourceDependencies dependencies,
                RelationalTypeMappingSourceDependencies relationalDependencies)
                : base(dependencies, relationalDependencies)
            {
            }

            protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
                => mappingInfo.ClrType == typeof(GeoPoint)
                    ? ((RelationalTypeMapping)base.FindMapping(typeof(Point))
                        .Clone(new GeoPointConverter()))
                    .Clone("geometry", null)
                    : base.FindMapping(mappingInfo);
        }
    }
}
