// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Northwind;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
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

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly DbContextOptions _options;

        public NorthwindQueryFixture()
        {
            var model = CreateModel();

            var titleProperty
                = model.GetEntityType(typeof(Employee)).GetProperty("Title");

            _options
                = new DbContextOptions()
                    .UseModel(model)
                    .UseInMemoryStore();

            using (var context = new DbContext(_options))
            {
                context.Set<Customer>().AddRange(NorthwindData.Customers);

                foreach (var employee in NorthwindData.Employees)
                {
                    context.Set<Employee>().Add(employee);
                    context.ChangeTracker.Entry(employee).StateEntry[titleProperty] = employee.Title;
                }

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
