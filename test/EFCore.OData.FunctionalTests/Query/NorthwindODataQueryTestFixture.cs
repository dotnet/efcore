// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindODataQueryTestFixture : NorthwindQuerySqlServerFixture<NoopModelCustomizer>, IODataQueryTestFixture
    {
        private IHost _selfHostServer = null;

        protected override string StoreName { get; } = "ODataNorthwind";

        public NorthwindODataQueryTestFixture()
        {
            var controllers = new Type[]
            {
                typeof(CustomersController),
                typeof(OrdersController),
                typeof(OrderDetailsController),
                typeof(EmployeesController),
                typeof(ProductsController),
            };

            (BaseAddress, ClientFactory, _selfHostServer)
                = ODataQueryTestFixtureInitializer.Initialize<NorthwindODataContext>(StoreName, controllers, GetEdmModel());
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

        public override void Dispose()
        {
            if (_selfHostServer != null)
            {
                //issue: dotnet/runtime #35990
                _selfHostServer.StopAsync();
                System.Threading.Thread.Sleep(5000);
                _selfHostServer.Dispose();
                //_selfHostServer.WaitForShutdown();

                _selfHostServer = null;
            }
        }
    }
}
