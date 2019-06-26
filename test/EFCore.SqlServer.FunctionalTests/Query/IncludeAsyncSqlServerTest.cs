// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeAsyncSqlServerTest : IncludeAsyncTestBase<IncludeSqlServerFixture>
    {
        public IncludeAsyncSqlServerTest(IncludeSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        public override async Task Include_collection_order_by_subquery()
        {
            await base.Include_collection_order_by_subquery();

            AssertSql(
                @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE ([c].[CustomerID] = [o].[CustomerID]) AND [o].[CustomerID] IS NOT NULL
        ORDER BY [o].[EmployeeID]) AS [c]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] = N'ALFKI'
    ORDER BY (
        SELECT TOP(1) [o].[OrderDate]
        FROM [Orders] AS [o]
        WHERE ([c].[CustomerID] = [o].[CustomerID]) AND [o].[CustomerID] IS NOT NULL
        ORDER BY [o].[EmployeeID])
) AS [t]
LEFT JOIN [Orders] AS [o0] ON [t].[CustomerID] = [o0].[CustomerID]
ORDER BY [t].[c], [t].[CustomerID], [o0].[OrderID]");
        }

        public override async Task Include_collection_then_include_collection()
        {
            await base.Include_collection_then_include_collection();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [t].[OrderID0], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID] AS [OrderID0], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
    FROM [Orders] AS [o]
    LEFT JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
ORDER BY [c].[CustomerID], [t].[OrderID], [t].[OrderID0], [t].[ProductID]");
        }

        [SqlServerCondition(SqlServerCondition.SupportsOffset)]
        // Test does not pass on SqlServer 2008. TODO: See issue#7160
        public override Task Include_duplicate_reference()
        {
            return base.Include_duplicate_reference();
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
    }
}
