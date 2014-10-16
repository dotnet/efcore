// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        protected override void OnModelCreating(BasicModelBuilder modelBuilder)
        {
            const string tableSuffix = "FunctionalTests";

            modelBuilder.Entity<Customer>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.City, s => s.CustomerID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("Customer" + tableSuffix);
                    });

            modelBuilder.Entity<Employee>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.City, s => s.EmployeeID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("Employee" + tableSuffix);
                    });

            modelBuilder.Entity<Order>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.CustomerID, s => s.OrderID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("Order" + tableSuffix);
                    });

            modelBuilder.Entity<Product>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.SupplierID, s => s.ProductID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("Product" + tableSuffix);
                    });

            modelBuilder.Entity<OrderDetail>()
                .ForAzureTableStorage(ab =>
                    {
                        ab.PartitionAndRowKey(s => s.OrderID, s => s.ProductID);
                        ab.Timestamp("Timestamp", true);
                        ab.Table("OrderDetail" + tableSuffix);
                    });
        }

        public NorthwindQueryFixture()
        {
            var model = CreateModel();

            _options = new DbContextOptions()
                .UseModel(model)
                .UseAzureTableStorage(TestConfig.Instance.ConnectionString, batchRequests: false);

            var services = new ServiceCollection();

            services
                .AddEntityFramework()
                .UseLoggerFactory(TestFileLogger.Factory)
                .AddAzureTableStorage();

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
    }
}
