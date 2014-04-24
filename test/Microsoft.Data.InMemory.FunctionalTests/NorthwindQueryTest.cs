// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Microsoft.Data.FunctionalTests;
using Northwind;
using Xunit;

namespace Microsoft.Data.InMemory.FunctionalTests
{
    public class NorthwindQueryTest : NorthwindQueryTestBase, IClassFixture<NorthwindQueryFixture>
    {
        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected override EntityConfiguration Configuration
        {
            get { return _fixture.Configuration; }
        }
    }

    public class NorthwindQueryFixture : NorthwindQueryFixtureBase
    {
        private readonly EntityConfiguration _configuration;

        public NorthwindQueryFixture()
        {
            _configuration
                = new EntityConfigurationBuilder()
                    .UseModel(CreateModel())
                    .UseInMemoryStore()
                    .BuildConfiguration();

            using (var context = new EntityContext(_configuration))
            {
                context.Set<Customer>().AddRange(NorthwindData.Customers);
                context.Set<Employee>().AddRange(NorthwindData.Employees);
                context.Set<Order>().AddRange(NorthwindData.Orders);
                context.Set<Product>().AddRange(NorthwindData.Products);
                //context.Set<OrderDetail>().AddRange(NorthwindData.OrderDetails); // composite keys
                context.SaveChanges();
            }
        }

        public override EntityConfiguration Configuration
        {
            get { return _configuration; }
        }
    }
}
