// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsODataQueryTestFixture : ComplexNavigationsQuerySqlServerFixture, IODataQueryTestFixture
    {
        private IHost _selfHostServer;

        protected override string StoreName { get; } = "ODataComplexNavigations";

        public ComplexNavigationsODataQueryTestFixture()
        {
            (BaseAddress, ClientFactory, _selfHostServer)
                = ODataQueryTestFixtureInitializer.Initialize<ComplexNavigationsODataContext>(StoreName, GetEdmModel());
        }

        public void UpdateConfigureServices<TContext>(IServiceCollection services, string storeName)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(b =>
                b.UseSqlServer(
                    SqlServerTestStore.CreateConnectionString(storeName)));
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

        public override async Task DisposeAsync()
        {
            if (_selfHostServer != null)
            {
                await _selfHostServer.StopAsync();
                _selfHostServer.Dispose();
                _selfHostServer = null;
            }
        }
    }
}
