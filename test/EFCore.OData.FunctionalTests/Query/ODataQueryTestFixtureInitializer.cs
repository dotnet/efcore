using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            Type[] controllers,
            IEdmModel edmModel)
            where TContext : DbContext
        {
            var port = PortArranger.Reserve();
            var baseAddress = string.Format("http://localhost:{0}", port.ToString());

            var clientFactory = default(IHttpClientFactory);
            var selfHostServer = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseKestrel(options => options.Listen(IPAddress.Loopback, port))
                    .ConfigureServices(services =>
                    {
                        services.AddHttpClient();
                        services.AddOData();
                        services.AddRouting();

                        UpdateConfigureServices<TContext>(services, storeName);
                    })
                    .Configure(app =>
                    {
                         clientFactory = app.ApplicationServices.GetRequiredService<IHttpClientFactory>();

                        app.UseODataBatching();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            var config = new EndpointRouteConfiguration(endpoints);
                            UpdateConfigure(config, controllers, edmModel);
                        });
                    })
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddDebug();
                        logging.SetMinimumLevel(LogLevel.Warning);
                    }
                )).Build();

            selfHostServer.Start();

            return (baseAddress, clientFactory, selfHostServer);
        }

        public static void UpdateConfigureServices<TContext>(IServiceCollection services, string storeName)
            where TContext : DbContext
        {
            services.AddDbContext<TContext>(b =>
                b.UseSqlServer(
                    SqlServerTestStore.CreateConnectionString(storeName)));
        }

        protected static void UpdateConfigure(EndpointRouteConfiguration configuration, Type[] controllers, IEdmModel edmModel)
        {
            configuration.AddControllers(controllers);
            configuration.MaxTop(2).Expand().Select().OrderBy().Filter();

            configuration.MapODataRoute("odata", "odata",
                edmModel,
                new DefaultODataPathHandler(),
                ODataRoutingConventions.CreateDefault(),
                new DefaultODataBatchHandler());
        }

    }
}

