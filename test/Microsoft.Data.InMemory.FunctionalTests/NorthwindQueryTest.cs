// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.AspNet.DependencyInjection;
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

        protected override ImmutableDbContextOptions Configuration
        {
            get { return _fixture.Configuration; }
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

        public override ImmutableDbContextOptions Configuration
        {
            get { return _configuration; }
        }
    }
}
