// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Northwind;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public IModel CreateAzureTableStorageModel(string tableSuffix)
        {
            var model = CreateModel();

            // TODO: Need to do this because we're not using the ATS conventions and so need to redefine keys manually
            var productType = model.GetEntityType(typeof(Product));
            var orderDetailType = model.GetEntityType(typeof(OrderDetail));
            var customerType = model.GetEntityType(typeof(Customer));
            var orderType = model.GetEntityType(typeof(Order));

            orderDetailType.RemoveNavigation(orderDetailType.Navigations.Single());
            productType.RemoveNavigation(productType.Navigations.Single());
            customerType.RemoveNavigation(customerType.Navigations.Single());
            orderType.RemoveNavigation(orderType.Navigations.Single());

            orderDetailType.RemoveForeignKey(orderDetailType.ForeignKeys.Single());
            orderType.RemoveForeignKey(orderType.ForeignKeys.Single());

            var builder = new BasicModelBuilder(model);
            builder.Entity<Customer>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.City, s => s.CustomerID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("Customer" + tableSuffix);
                    })
                .Key(e => e.CustomerID); // See issue #632
            builder.Entity<Employee>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.City, s => s.EmployeeID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("Employee" + tableSuffix);
                    })
                .Key(e => e.EmployeeID); // See issue #632
            builder.Entity<Order>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.CustomerID, s => s.OrderID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("Order" + tableSuffix);
                    })
                .Key(e => e.OrderID); // See issue #632
            builder.Entity<Product>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.SupplierID, s => s.ProductID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("Product" + tableSuffix);
                    })
                .Key(e => e.ProductID); // See issue #632
            builder.Entity<OrderDetail>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.OrderID, s => s.ProductID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("OrderDetail" + tableSuffix);
                    });

            var modelBuilder = new BasicModelBuilder(model);

            modelBuilder.Entity<OrderDetail>().ForeignKey<Product>(od => od.ProductID);
            modelBuilder.Entity<Order>().ForeignKey<Customer>(o => o.CustomerID);

            var productIdFk = orderDetailType.ForeignKeys.Single();
            orderDetailType.AddNavigation("Product", productIdFk, pointsToPrincipal: true);
            productType.AddNavigation("OrderDetails", productIdFk, pointsToPrincipal: false);

            var customerIdFk = orderType.ForeignKeys.Single();
            orderType.AddNavigation("Customer", customerIdFk, pointsToPrincipal: true);
            customerType.AddNavigation("Orders", customerIdFk, pointsToPrincipal: false);

            return builder.Model;
        }

        public NorthwindQueryFixture()
        {
            var model = CreateAzureTableStorageModel("FunctionalTests");

            _options = new DbContextOptions()
                .UseModel(model)
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString, batchRequests: false);

            var services = new ServiceCollection();
            services.AddEntityFramework().UseLoggerFactory(TestFileLogger.Factory).AddAzureTableStorage();
            _serviceProvider = services.BuildServiceProvider();

            using (var context = new DbContext(_options))
            {
                if (!context.Database.EnsureCreated())
                {
                    return;
                }

                context.Set<Customer>().AddRange(NorthwindData.CreateCustomers());

                var titleProperty = model.GetEntityType(typeof(Employee)).GetProperty("Title");
                foreach (var employee in NorthwindData.CreateEmployees())
                {
                    context.Set<Employee>().Add(employee);
                    context.ChangeTracker.Entry(employee).StateEntry[titleProperty] = employee.Title;
                }

                context.Set<Order>().AddRange(NorthwindData.CreateOrders());
                context.Set<Product>().AddRange(NorthwindData.CreateProducts());
                context.Set<OrderDetail>().AddRange(NorthwindData.CreateOrderDetails());
                context.SaveChanges();
            }
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        public DbContext CreateContext(string suffix)
        {
            var model = CreateAzureTableStorageModel(suffix);

            var options = new DbContextOptions()
                .UseModel(model)
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString, batchRequests: false);

            return new DbContext(_serviceProvider, options);
        }
    }
}
