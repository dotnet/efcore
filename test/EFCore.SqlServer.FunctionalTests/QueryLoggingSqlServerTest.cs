// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Xunit;

#if NETCOREAPP1_1
using System.Threading;
#endif
namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class QueryLoggingSqlServerTest : IClassFixture<NorthwindQuerySqlServerFixture>
    {
        private const string FileLineEnding = @"
";

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
                    @"    Compiling query model: 
'from Customer <generated>_0 in DbSet<Customer>
select <generated>_0'
    Optimized query model: 
'from Customer <generated>_0 in DbSet<Customer>
select <generated>_0'
    TRACKED: True
(QueryContext queryContext) => IEnumerable<Customer> _ShapedQuery(
    queryContext: queryContext, 
    shaperCommandContext: SelectExpression: 
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
    , 
    shaper: UnbufferedEntityShaper<Customer>
)",
                    TestSqlLoggerFactory.Log.Replace(Environment.NewLine, FileLineEnding));
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
        public virtual void Query_with_ignored_include_should_log_warning()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Customers
                        .Include(c => c.Orders)
                        .Select(c => c.CustomerID)
                        .ToList();

                Assert.NotNull(customers);
                Assert.Contains(CoreStrings.LogIgnoredInclude("c.Orders"), TestSqlLoggerFactory.Log);
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
                Assert.StartsWith(@"    Compiling query model: 
'(from Customer c in DbSet<Customer>
select c)
.Include(""Orders"")'
    Including navigation: '[c].Orders'
    Optimized query model: 
'from Customer c in DbSet<Customer>
order by Property(c, ""CustomerID"") asc
select Customer _Include(
    entity: c, 
    included: new object[]{ () => 
            from Order c.Orders in DbSet<Order>
            join Customer _c in 
                from Customer c in DbSet<Customer>
                select c
            on Property(c.Orders, ""CustomerID"") equals Property(_c, ""CustomerID"")
            order by Property(_c, ""CustomerID"") asc
            select c.Orders }
    , 
    fixup: (Customer entity | Object[] included) => 
    {
        Void queryContext.QueryBuffer.StartTracking(
            entity: entity, 
            entityType: EntityType: Customer
        )

        return queryContext.QueryBuffer.IncludeCollection(0, Navigation: Customer.Orders (<Orders>k__BackingField, ICollection<Order>) Collection ToDependent Order Inverse: Customer 0 -1 1 -1 -1, Navigation: Order.Customer (<Customer>k__BackingField, Customer) ToPrincipal Customer Inverse: Orders 0 -1 2 -1 -1, EntityType: Order, value(Microsoft.EntityFrameworkCore.Metadata.Internal.ClrICollectionAccessor`3[Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Customer,System.Collections.Generic.ICollection`1[Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Order],Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Order]), value(Microsoft.EntityFrameworkCore.Metadata.Internal.ClrPropertySetter`2[Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Order,Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind.Customer]), True, entity, Convert(included[0]))
    }

)
'
    TRACKED: True
(QueryContext queryContext) => IEnumerable<Customer> _Select(
    source: IEnumerable<Customer> _ShapedQuery(
        queryContext: queryContext, 
        shaperCommandContext: SelectExpression: 
            SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[CustomerID]
        , 
        shaper: BufferedEntityShaper<Customer>
    )
    , 
    selector: (Customer c) => Customer _Include(
        entity: c, 
        included: new object[]{ () => IEnumerable<Order> _ShapedQuery(
                queryContext: queryContext, 
                shaperCommandContext: SelectExpression: 
                    SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
                    FROM [Orders] AS [c.Orders]
                    INNER JOIN [Customers] AS [c0] ON [c.Orders].[CustomerID] = [c0].[CustomerID]
                    ORDER BY [c0].[CustomerID]
                , 
                shaper: BufferedOffsetEntityShaper<Order>
            )
             }
        , 
        fixup: (Customer entity | Object[] included) => 
        {
            Void queryContext.QueryBuffer.StartTracking(
                entity: entity, 
                entityType: EntityType: Customer
            )
            return queryContext.QueryBuffer.IncludeCollection(0, Navigation: Customer.Orders (<Orders>k__BackingField, ICollection<Order>) Collection ToDependent Order Inverse: Customer 0 -1 1 -1 -1, Navigation: Order.Customer (<Customer>k__BackingField, Customer) ToPrincipal Customer Inverse: Orders 0 -1 2 -1 -1, EntityType: Order, value(Metadata.Internal.ClrICollectionAccessor`3[Specification.Tests.TestModels.Northwind.Customer,System.Collections.Generic.ICollection`1[Specification.Tests.TestModels.Northwind.Order],Specification.Tests.TestModels.Northwind.Order]), value(Metadata.Internal.ClrPropertySetter`2[Specification.Tests.TestModels.Northwind.Order,Specification.Tests.TestModels.Northwind.Customer]), True, entity, Convert(included[0]))
        }
    )
)",
                    TestSqlLoggerFactory.Log.Replace(Environment.NewLine, FileLineEnding));
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
