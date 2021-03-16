// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ODataQueryTestFixtureInitializer
    {
        public static (string BaseAddress, IHttpClientFactory ClientFactory, IHost SelfHostServer) Initialize<TContext>(
            string storeName,
            IEdmModel edmModel,
            List<IODataRoutingConvention> customRoutingConventions = null)
            where TContext : DbContext
        {
            var selfHostServer = Host.CreateDefaultBuilder()
                .ConfigureServices(services => services.AddSingleton<IHostLifetime, NoopHostLifetime>())
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseKestrel(options => options.Listen(IPAddress.Loopback, 0))
                    .ConfigureServices(services =>
                    {
                        services.AddHttpClient();
                        services.AddOData();
                        services.AddRouting();

                        UpdateConfigureServices<TContext>(services, storeName);
                    })
                    .Configure(app =>
                    {
                        app.UseODataBatching();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            var conventions = ODataRoutingConventions.CreateDefault();
                            if (customRoutingConventions != null)
                            {
                                foreach (var customRoutingConvention in customRoutingConventions)
                                {
                                    conventions.Insert(0, customRoutingConvention);
                                }
                            }

                            endpoints.MaxTop(null).Expand().Select().OrderBy().Filter().Count();
                            endpoints.MapODataRoute("odata", "odata",
                                edmModel,
                                new DefaultODataPathHandler(),
                                conventions,
                                new DefaultODataBatchHandler());
                        });
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddDebug();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    }
                )).Build();

            selfHostServer.Start();

            var baseAddress = selfHostServer.Services.GetService<IServer>().Features.Get<IServerAddressesFeature>().Addresses.First();
            var clientFactory = selfHostServer.Services.GetRequiredService<IHttpClientFactory>();

            return (baseAddress, clientFactory, selfHostServer);
        }

        public static void UpdateConfigureServices<TContext>(IServiceCollection services, string storeName)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(b =>
                b.UseSqlServer(
                    SqlServerTestStore.CreateConnectionString(storeName)));
        }

        private class NoopHostLifetime : IHostLifetime
        {
            public Task StopAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public Task WaitForStartAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}

