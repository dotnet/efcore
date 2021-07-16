// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindODataQueryTestFixture : NorthwindQuerySqlServerFixture<NoopModelCustomizer>, IODataQueryTestFixture
    {
        private IHost _selfHostServer;

        protected override string StoreName { get; } = "ODataNorthwind";

        public NorthwindODataQueryTestFixture()
        {
            (BaseAddress, ClientFactory, _selfHostServer)
                = ODataQueryTestFixtureInitializer.Initialize<NorthwindODataContext>(
                    StoreName,
                    GetEdmModel(),
                    new List<IODataRoutingConvention> { new OrderDetailsRoutingConvention() });
        }

        private static IEdmModel GetEdmModel()
        {
            var modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Customer>("Customers");
            modelBuilder.EntitySet<Order>("Orders");
            modelBuilder.EntityType<OrderDetail>().HasKey(e => new { e.OrderID, e.ProductID });
            modelBuilder.EntitySet<OrderDetail>("Order Details");

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

    public class OrderDetailsRoutingConvention : IODataRoutingConvention
    {
        public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        {
            var odataPath = routeContext.HttpContext.ODataFeature().Path;
            if (odataPath == null)
            {
                return null;
            }

            if (odataPath.PathTemplate == "~/entityset"
                && routeContext.HttpContext.Request.Method.Equals("get", StringComparison.OrdinalIgnoreCase)
                && ((EntitySetSegment)odataPath.Segments[0]).EntitySet.Name == "Order Details")
            {
                return routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>()
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == "OrderDetails" && c.ActionName == "Get");
            }

            return null;
        }
    }
}
