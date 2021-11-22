// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;

namespace Microsoft.EntityFrameworkCore.Query;

public class ODataQueryTestFixtureInitializer
{
    public static (string BaseAddress, IHttpClientFactory ClientFactory, IHost SelfHostServer) Initialize<TContext>(
        string storeName,
        IEdmModel edmModel,
        List<IODataControllerActionConvention> customRoutingConventions = null)
        where TContext : DbContext
    {
        var selfHostServer = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddSingleton<IHostLifetime, NoopHostLifetime>())
            .ConfigureWebHostDefaults(
                webBuilder => webBuilder
                    .UseKestrel(options => options.Listen(IPAddress.Loopback, 0))
                    .ConfigureServices(
                        services =>
                        {
                            services.AddDbContext<TContext>(
                                o => o.UseSqlServer(
                                    SqlServerTestStore.CreateConnectionString(storeName)));

                            services.AddControllers().AddOData(
                                o =>
                                {
                                    o.AddRouteComponents("odata", edmModel)
                                        .SetMaxTop(null)
                                        .Expand()
                                        .Select()
                                        .OrderBy()
                                        .Filter()
                                        .Count();

                                    if (customRoutingConventions != null)
                                    {
                                        foreach (var customRoutingConvention in customRoutingConventions)
                                        {
                                            o.Conventions.Add(customRoutingConvention);
                                        }
                                    }
                                });

                            services.AddHttpClient();
                        })
                    .Configure(
                        app =>
                        {
                            app.UseRouting();
                            app.UseEndpoints(
                                endpoints =>
                                {
                                    endpoints.MapControllers();
                                });
                        })
                    .ConfigureLogging(
                        (hostingContext, logging) =>
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

    private class NoopHostLifetime : IHostLifetime
    {
        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task WaitForStartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
