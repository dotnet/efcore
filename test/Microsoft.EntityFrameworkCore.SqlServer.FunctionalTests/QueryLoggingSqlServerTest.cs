// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

#if NETCOREAPP1_0
using System.Threading;
#endif

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class QueryLoggingSqlServerTest : IClassFixture<NorthwindQuerySqlServerFixture>
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
                Assert.StartsWith(
                    @"    Compiling query model: 'value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Customer])'
    Optimized query model: 'value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Customer])'
    TRACKED: True
(QueryContext queryContext) => IEnumerable<Customer> _ShapedQuery(
    queryContext: queryContext, 
    shaperCommandContext: SelectExpression: 
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
    , 
    shaper: UnbufferedEntityShaper<Customer>
)",
                    TestSqlLoggerFactory.Log);
            }
        }

        [Fact]
        public virtual void Queryable_with_parameter_outputs_parameter_value_logging_warning()
        {
            using (var context = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var city = "Redmond";

                var customers
                    = context.Customers
                        .Where(c => c.City == city)
                        .ToList();

                Assert.NotNull(customers);
                Assert.Contains(CoreStrings.SensitiveDataLoggingEnabled, TestSqlLoggerFactory.Log);
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
                Assert.StartsWith(@"    Compiling query model: 'value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Customer]) => Include([c].Orders)'
    Optimized query model: 'value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Customer])'
    Including navigation: 'Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Customer.Orders'
    TRACKED: True
(QueryContext queryContext) => IEnumerable<Customer> _Include(
    queryContext: (RelationalQueryContext) queryContext, 
    innerResults: IEnumerable<Customer> _ShapedQuery(
        queryContext: queryContext, 
        shaperCommandContext: SelectExpression: 
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
        , 
        shaper: BufferedEntityShaper<Customer>
    )
    , 
    entityAccessor: default(System.Func`2[Specification.Tests.TestModels.Northwind.Customer,System.Object]), 
    navigationPath: INavigation[] { Customer.Orders, }, 
    relatedEntitiesLoaderFactories: List<Func<QueryContext, IRelatedEntitiesLoader>> 
    { 
        System.Func`2[QueryContext,Internal.IRelatedEntitiesLoader], 
    }
    , 
    querySourceRequiresTracking: True
)",
                    TestSqlLoggerFactory.Log);
            }
        }

        private readonly NorthwindQuerySqlServerFixture _fixture;

        public QueryLoggingSqlServerTest(NorthwindQuerySqlServerFixture fixture)
        {
            _fixture = fixture;
        }

        protected NorthwindContext CreateContext() => _fixture.CreateContext();
    }
}
