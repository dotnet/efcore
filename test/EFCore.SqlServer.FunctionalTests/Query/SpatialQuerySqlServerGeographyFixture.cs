// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using GeoAPI;
using GeoAPI.Geometries;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;

namespace Microsoft.EntityFrameworkCore.Query
{
#if !Test21
    public class SpatialQuerySqlServerGeographyFixture : SpatialQueryRelationalFixture
    {
        private IGeometryServices _geometryServices;
        private IGeometryFactory _geometryFactory;

        public IGeometryServices GeometryServices
            => LazyInitializer.EnsureInitialized(
                ref _geometryServices,
                () => new NtsGeometryServices(
                    NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory,
                    NtsGeometryServices.Instance.DefaultPrecisionModel,
                    4326));

        public override IGeometryFactory GeometryFactory
            => LazyInitializer.EnsureInitialized(
                ref _geometryFactory,
                () => GeometryServices.CreateGeometryFactory());

        protected override string StoreName
            => "SpatialQueryGeographyTest";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddSingleton(GeometryServices)
                .AddEntityFrameworkSqlServerNetTopologySuite();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            new SqlServerDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();

            return optionsBuilder;
        }
    }
#endif
}
