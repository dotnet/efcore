// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsODataQueryTestFixture : ComplexNavigationsQuerySqlServerFixture, IODataQueryTestFixture
    {
        private IHost _selfHostServer = null;

        protected override string StoreName { get; } = "ODataComplexNavigations";

        public ComplexNavigationsODataQueryTestFixture()
        {
            var controllers = new Type[]
            {
                typeof(LevelOneController),
                typeof(LevelTwoController),
                typeof(LevelThreeController),
                typeof(LevelFourController),
            };

            (BaseAddress, ClientFactory, _selfHostServer)
                = ODataQueryTestFixtureInitializer.Initialize<ComplexNavigationsODataContext>(StoreName, controllers, GetEdmModel());
        }

        public void UpdateConfigureServices<TContext>(IServiceCollection services, string storeName)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(b =>
                b.UseSqlServer(
                    SqlServerTestStore.CreateConnectionString(storeName)));
        }

        protected void UpdateConfigure(EndpointRouteConfiguration configuration, Type[] controllers, IEdmModel edmModel)
        {
            configuration.AddControllers(controllers);
            configuration.MaxTop(2).Expand().Select().OrderBy().Filter();

            configuration.MapODataRoute("odata", "odata",
                edmModel,
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler());
        }

        private static IEdmModel GetEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Level1>("LevelOne");
            modelBuilder.EntitySet<Level2>("LevelTwo");
            modelBuilder.EntitySet<Level3>("LevelThree");
            modelBuilder.EntitySet<Level4>("LevelFour");

            return modelBuilder.GetEdmModel();
        }

        public string BaseAddress { get; private set; }

        public IHttpClientFactory ClientFactory { get; private set; }

        public override void Dispose()
        {
            if (_selfHostServer != null)
            {
                //issue: dotnet/runtime #35990
                _selfHostServer.StopAsync();
                System.Threading.Thread.Sleep(5000);
                _selfHostServer.Dispose();

                _selfHostServer = null;
            }
        }
    }
}
