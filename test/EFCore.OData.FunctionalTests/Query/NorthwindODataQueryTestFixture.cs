// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindODataQueryTestFixture : NorthwindQuerySqlServerFixture<NoopModelCustomizer>, IODataQueryTestFixture
{
    private IHost _selfHostServer;

    protected override string StoreName
        => "ODataNorthwind";

    public NorthwindODataQueryTestFixture()
    {
        (BaseAddress, ClientFactory, _selfHostServer)
            = ODataQueryTestFixtureInitializer.Initialize<NorthwindODataContext>(
                StoreName,
                GetEdmModel(),
                [new OrderDetailsControllerActionConvention()]);
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

    public string BaseAddress { get; }

    public IHttpClientFactory ClientFactory { get; }

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

public class OrderDetailsControllerActionConvention : IODataControllerActionConvention
{
    public int Order
        => 0;

    public bool AppliesToController(ODataControllerActionContext context)
        => context.Controller.ControllerName == "OrderDetails";

    public bool AppliesToAction(ODataControllerActionContext context)
    {
        if (context.Action.ActionName == "Get")
        {
            var entitySet = context.Model.EntityContainer.FindEntitySet("Order Details");
            var route = new EntitySetSegmentTemplate(entitySet);
            var parameters = context.Action.ActionMethod.GetParameters();
            if (parameters.Length == 0)
            {
                var path = new ODataPathTemplate(route);
                context.Action.AddSelector("get", context.Prefix, context.Model, path, context.Options.RouteOptions);

                return true;
            }

            if (parameters.Length == 2
                && parameters[0].Name == "keyOrderId"
                && parameters[1].Name == "keyProductId")
            {
                var keys = new Dictionary<string, string> { { "OrderID", "{keyOrderId}" }, { "ProductID", "{keyProductId}" } };

                var keyTemplate = new KeySegmentTemplate(keys, entitySet.EntityType(), entitySet);

                var path = new ODataPathTemplate(route, keyTemplate);
                context.Action.AddSelector("get", context.Prefix, context.Model, path, context.Options.RouteOptions);

                return true;
            }
        }

        return false;
    }
}
