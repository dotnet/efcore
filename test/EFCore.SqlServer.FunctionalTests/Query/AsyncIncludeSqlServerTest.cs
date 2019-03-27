// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class AsyncIncludeSqlServerTest : IncludeAsyncTestBase<IncludeSqlServerFixture>
    {
        public AsyncIncludeSqlServerTest(IncludeSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Include_collection_order_by_subquery()
        {
            await base.Include_collection_order_by_subquery();

            AssertSql(
                @"SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[EmployeeID]
), [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT TOP(1) [c0].[CustomerID], (
        SELECT TOP(1) [o1].[OrderDate]
        FROM [Orders] AS [o1]
        WHERE [c0].[CustomerID] = [o1].[CustomerID]
        ORDER BY [o1].[EmployeeID]
    ) AS [c]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
    ORDER BY (
        SELECT TOP(1) [o0].[OrderDate]
        FROM [Orders] AS [o0]
        WHERE [c0].[CustomerID] = [o0].[CustomerID]
        ORDER BY [o0].[EmployeeID]
    ), [c0].[CustomerID]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID]");
        }

        public override async Task Include_collection_then_include_collection()
        {
            await base.Include_collection_then_include_collection();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID], [c.Orders].[OrderID]",
                //
                @"SELECT [c.Orders.OrderDetails].[OrderID], [c.Orders.OrderDetails].[ProductID], [c.Orders.OrderDetails].[Discount], [c.Orders.OrderDetails].[Quantity], [c.Orders.OrderDetails].[UnitPrice]
FROM [Order Details] AS [c.Orders.OrderDetails]
INNER JOIN (
    SELECT DISTINCT [c.Orders0].[OrderID], [t0].[CustomerID]
    FROM [Orders] AS [c.Orders0]
    INNER JOIN (
        SELECT [c1].[CustomerID]
        FROM [Customers] AS [c1]
    ) AS [t0] ON [c.Orders0].[CustomerID] = [t0].[CustomerID]
) AS [t1] ON [c.Orders.OrderDetails].[OrderID] = [t1].[OrderID]
ORDER BY [t1].[CustomerID], [t1].[OrderID]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)] // Test does not pass on SqlServer 2008. TODO: See issue#7160
        public override Task Include_duplicate_reference()
        {
            return base.Include_duplicate_reference();
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
