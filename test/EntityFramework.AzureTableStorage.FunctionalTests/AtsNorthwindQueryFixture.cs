// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class AtsNorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly static string _tableSuffix = NorthwindContext.StoreName;

        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public AtsNorthwindQueryFixture()
        {
            _options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseAzureTableStorage(TestConfig.Instance.ConnectionString, batchRequests: false);

            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddAzureTableStorage()
                    .ServiceCollection
                    .BuildServiceProvider();

            AtsTestStore.GetOrCreateSharedAsync(_tableSuffix, () =>
            {
                using (var context = CreateContext())
                {
                    if (context.Database.EnsureCreated())
                    {
                        NorthwindData.Seed(context);
                    }
                }
                return Task.FromResult(true);
            }).Wait();
        }
        
        public override void OnModelCreating(BasicModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Customer>()
                .ForAzureTableStorage(ab =>
                {
                    ab.PartitionAndRowKey(s => s.City, s => s.CustomerID);
                    ab.Timestamp("Timestamp", true);
                    ab.Table("Customer" + _tableSuffix);
                });

            modelBuilder.Entity<Employee>()
                .ForAzureTableStorage(ab =>
                {
                    ab.PartitionAndRowKey(s => s.City, s => s.EmployeeID);
                    ab.Timestamp("Timestamp", true);
                    ab.Table("Employee" + _tableSuffix);
                });

            modelBuilder.Entity<Order>()
                .ForAzureTableStorage(ab =>
                {
                    ab.PartitionAndRowKey(s => s.CustomerID, s => s.OrderID);
                    ab.Timestamp("Timestamp", true);
                    ab.Table("Order" + _tableSuffix);
                });

            modelBuilder.Entity<Product>()
                .ForAzureTableStorage(ab =>
                {
                    ab.PartitionAndRowKey(s => s.SupplierID, s => s.ProductID);
                    ab.Timestamp("Timestamp", true);
                    ab.Table("Product" + _tableSuffix);
                });

            modelBuilder.Entity<OrderDetail>()
                .ForAzureTableStorage(ab =>
                {
                    ab.PartitionAndRowKey(s => s.OrderID, s => s.ProductID);
                    ab.Timestamp("Timestamp", true);
                    ab.Table("OrderDetail" + _tableSuffix);
                });

            base.OnModelCreating(modelBuilder);
        }

        public override NorthwindContext CreateContext()
        {
            return new NorthwindContext(_serviceProvider, _options);
        }
    }
}
