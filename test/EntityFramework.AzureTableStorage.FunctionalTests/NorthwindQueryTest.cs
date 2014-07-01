// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.FunctionalTests;
using Northwind;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
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

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;

        public IModel CreateAzureTableStorageModel()
        {
            var model = CreateModel();
            const string tableSuffix = "FunctionalTests";
            var builder = new ModelBuilder(model);
            builder.Entity<Customer>()
                .PartitionAndRowKey(s => s.City, s => s.CustomerID)
                .Timestamp("Timestamp", true)
                .TableName("Customer" + tableSuffix);
            builder.Entity<Employee>()
                .PartitionAndRowKey(s => s.City, s => s.EmployeeID)
                .Timestamp("Timestamp", true)
                .TableName("Employee" + tableSuffix);
            builder.Entity<Order>()
                .PartitionAndRowKey(s => s.CustomerID, s => s.OrderID)
                .Timestamp("Timestamp", true)
                .TableName("Order" + tableSuffix);
            builder.Entity<Product>()
                .PartitionAndRowKey(s => s.SupplierID, s => s.ProductID)
                .Timestamp("Timestamp", true)
                .TableName("Product" + tableSuffix);
            builder.Entity<OrderDetail>()
                .PartitionAndRowKey(s => s.OrderID, s => s.ProductID)
                .Timestamp("Timestamp", true)
                .TableName("OrderDetail" + tableSuffix);

            return builder.Model;
        }

        public NorthwindQueryFixture()
        {
            _options
                = new DbContextOptions()
                    .UseModel(CreateAzureTableStorageModel())
                    .UseAzureTableStorage(TestConfig.Instance.ConnectionString, batchRequests: false);

            using (var context = new DbContext(_options))
            {
                if (!context.Database.EnsureCreated())
                {
                    return;
                }
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
    }
}
