// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Configuration;
using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.FunctionalTests;
using Northwind;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;

        public IModel CreateAzureTableStorageModel()
        {
            var model = CreateModel();
            var tableSuffix = DateTime.UtcNow.ToBinary(); //keep separate from other tests
            var builder = new ModelBuilder(model);
            builder.Entity<Customer>()
                .AzureTableProperties(atp =>
                {
                    atp.PartitionKey(s => s.City);
                    atp.RowKey(s => s.CustomerID);
                    atp.Timestamp("Timestamp", true);
                })
                .StorageName("Customer" + tableSuffix);
            builder.Entity<Employee>()
                .AzureTableProperties(atp =>
                {
                    atp.PartitionKey(s => s.City);
                    atp.RowKey(s => s.EmployeeID);
                    atp.Timestamp("Timestamp", true);
                })
                .StorageName("Employee" + tableSuffix);
            builder.Entity<Order>()
                .AzureTableProperties(atp =>
                {
                    atp.PartitionKey(s => s.CustomerID);
                    atp.RowKey(s => s.OrderID);
                    atp.Timestamp("Timestamp", true);
                })
                .StorageName("Order" + tableSuffix);
            builder.Entity<Product>()
                .AzureTableProperties(atp =>
                {
                    atp.PartitionKey(s => s.SupplierID);
                    atp.RowKey(s => s.ProductID);
                    atp.Timestamp("Timestamp", true);
                })
                .StorageName("Product" + tableSuffix);
            builder.Entity<OrderDetail>()
                .AzureTableProperties(atp =>
                {
                    atp.PartitionKey(s => s.OrderID);
                    atp.RowKey(s => s.ProductID);
                    atp.Timestamp("Timestamp", true);
                })
                .StorageName("OrderDetail" + tableSuffix);

            return builder.Model;
        }

        public NorthwindQueryFixture()
        {
            _options
                = new DbContextOptions()
                    .UseModel(CreateAzureTableStorageModel())
                    .UseAzureTableStorage(ConfigurationManager.AppSettings["TestConnectionString"], batchRequests: false);

            using (var context = new DbContext(_options))
            {
                context.Database.AsAzureTableStorageDatabase().CreateTables();
                context.Set<Customer>().AddRange(NorthwindData.Customers);
                context.Set<Employee>().AddRange(NorthwindData.Employees);
                context.Set<Order>().AddRange(NorthwindData.Orders);
                context.Set<Product>().AddRange(NorthwindData.Products);
                context.Set<OrderDetail>().AddRange(NorthwindData.OrderDetails);
                context.SaveChanges();
            }
        }

        public DbContext CreateContext()
        {
            return new DbContext(_options);
        }

        public void Dispose()
        {
            using (var context = new DbContext(_options))
            {
                context.Database.AsAzureTableStorageDatabase().DeleteTables();
            }
        }
    }
}
