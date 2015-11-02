// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Internal;
using Xunit;

#if DNXCORE50
using System.Threading;
#endif

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
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
                    @"    Compiling query model: 'value(Microsoft.Data.Entity.Query.Internal.EntityQueryable`1[Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer])'
    Optimized query model: 'value(Microsoft.Data.Entity.Query.Internal.EntityQueryable`1[Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer])'
    TRACKED: True
(QueryContext prm0) => IEnumerable<Customer> _ShapedQuery(
    queryContext: prm0, 
    commandBuilder: SelectExpression: 
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
    , 
    shaper: EntityShaper`1
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
                Assert.Contains(RelationalStrings.ParameterLoggingEnabled, TestSqlLoggerFactory.Log);
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
                Assert.StartsWith(@"    Compiling query model: 'value(Microsoft.Data.Entity.Query.Internal.EntityQueryable`1[Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer]) => Include([c].Orders)'
    Optimized query model: 'value(Microsoft.Data.Entity.Query.Internal.EntityQueryable`1[Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer])'
    Including navigation: 'Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind.Customer.Orders'
    TRACKED: True
(QueryContext prm0) => IEnumerable<Customer> _Include(
    queryContext: (RelationalQueryContext) prm0, 
    innerResults: IEnumerable<Customer> _ShapedQuery(
        queryContext: prm0, 
        commandBuilder: SelectExpression: 
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
        , 
        shaper: EntityShaper`1
    )
    , 
    entityAccessor: Unhandled expression type: Default
    , 
    navigationPath: INavigation[] { Customer.Orders, }, 
    includeRelatedValuesStrategyFactories: new Func<IIncludeRelatedValuesStrategy>[]{ () => IIncludeRelatedValuesStrategy _CreateCollectionIncludeStrategy(
            relatedValueBuffers: IEnumerable<ValueBuffer> _Query(
                queryContext: prm0, 
                commandBuilder: SelectExpression: 
                    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
                    FROM [Orders] AS [o]
                    INNER JOIN (
                        SELECT DISTINCT [c].[CustomerID]
                        FROM [Customers] AS [c]
                    ) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
                    ORDER BY [c].[CustomerID]
                , 
                queryIndex: 1
            )
            , 
            materializer: (ValueBuffer prm1) => 
            {
                var var2
                var2 = new Order()
                var2.OrderID = (int) object prm1.get_Item(0)
                var2.CustomerID = (string) object prm1.get_Item(1)
                var2.EmployeeID = (Nullable<int>) object prm1.get_Item(2)
                var2.OrderDate = (Nullable<DateTime>) object prm1.get_Item(3)
                var2
            }
        )
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
