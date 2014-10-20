// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Northwind;
using Xunit;

#if ASPNETCORE50
using System.Threading;
#endif

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class NorthwindQueryLoggingTest : IClassFixture<NorthwindQueryFixture>
    {
        [Fact]
        public virtual void Queryable_simple()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Set<Customer>()
                        .ToList();

                Assert.NotNull(customers);
                Assert.Equal(@"Compiling query model: 'value(Microsoft.Data.Entity.Query.EntityQueryable`1[Northwind.Customer])'
    Optimized query model: 'value(Microsoft.Data.Entity.Query.EntityQueryable`1[Northwind.Customer])'
    Tracking query sources: [<generated>_0]
    Compiled query expression.
", _fixture.Log);
            }
        }

        [Fact]
        public virtual void Include_navigation()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Set<Customer>()
                        .Include(c => c.Orders)
                        .ToList();

                Assert.NotNull(customers);
                Assert.Equal(@"Compiling query model: 'value(Microsoft.Data.Entity.Query.EntityQueryable`1[Northwind.Customer]) => Include([c].Orders)'
    Optimized query model: 'value(Microsoft.Data.Entity.Query.EntityQueryable`1[Northwind.Customer])'
    Including navigation: 'Northwind.Customer.Orders'
    Tracking query sources: [c]
    Compiled query expression.
", _fixture.Log);
            }
        }

        private readonly NorthwindQueryFixture _fixture;

        public NorthwindQueryLoggingTest(NorthwindQueryFixture fixture)
        {
            _fixture = fixture;
            _fixture.InitLogger();
        }

        protected DbContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
