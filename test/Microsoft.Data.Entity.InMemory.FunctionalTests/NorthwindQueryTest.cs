// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.FunctionalTests;
using Northwind;
using Xunit;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        public override void Queryable_simple()
        {
            base.Queryable_simple();
        }

        public override void OrderBy_Join()
        {
            base.OrderBy_Join();
        }

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
        private readonly ImmutableDbContextOptions _configuration;

        public NorthwindQueryFixture()
        {
            _configuration
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseInMemoryStore()
                    .BuildConfiguration();

            using (var context = new DbContext(_configuration))
            {
                context.Set<Customer>().AddRange(NorthwindData.Customers);
                context.Set<Employee>().AddRange(NorthwindData.Employees);
                context.Set<Order>().AddRange(NorthwindData.Orders);
                context.Set<Product>().AddRange(NorthwindData.Products);
                //context.Set<OrderDetail>().AddRange(NorthwindData.OrderDetails); // composite keys
                context.SaveChanges();
            }
        }

        public DbContext CreateContext()
        {
            return new DbContext(_configuration);
        }
    }
}
