// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Xunit;

#if ASPNETCORE50
using System.Threading;
#endif

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerQueryLoggingTest : IClassFixture<SqlServerNorthwindQueryFixture>
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
                Assert.Equal(@"Compiling query model: 'value(Microsoft.Data.Entity.Query.EntityQueryable`1[Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer])'
    Optimized query model: 'value(Microsoft.Data.Entity.Query.EntityQueryable`1[Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer])'
    Tracking query sources: [<generated>_0]
    Compiled query expression.
", TestSqlLoggerFactory.Log);
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
                Assert.Equal(@"Compiling query model: 'value(Microsoft.Data.Entity.Query.EntityQueryable`1[Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer]) => Include([c].Orders)'
    Optimized query model: 'value(Microsoft.Data.Entity.Query.EntityQueryable`1[Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer])'
    Including navigation: 'Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer.Orders'
    Tracking query sources: [c]
    Compiled query expression.
", TestSqlLoggerFactory.Log);
            }
        }

        private readonly SqlServerNorthwindQueryFixture _fixture;

        public SqlServerQueryLoggingTest(SqlServerNorthwindQueryFixture fixture)
        {
            _fixture = fixture;
        }

        protected NorthwindContext CreateContext()
        {
            return _fixture.CreateContext();
        }
    }
}
