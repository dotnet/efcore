// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class QueryNavigationsSqlServerTest : QueryNavigationsTestBase<NorthwindQuerySqlServerFixture>
    {
        public QueryNavigationsSqlServerTest(
            NorthwindQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o.Customer].[City] = N'Seattle'
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_Where_Navigation_Contains()
        {
            base.Select_Where_Navigation_Contains();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE CHARINDEX(N'Sea', [o.Customer].[City]) > 0
ORDER BY [o].[CustomerID]",
            Sql);
        }

        public override void Select_Where_Navigation_Deep()
        {
            base.Select_Where_Navigation_Deep();

            Assert.Equal(
                @"@__p_0: 1

SELECT TOP(@__p_0) [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order.Customer].[CustomerID], [od.Order.Customer].[Address], [od.Order.Customer].[City], [od.Order.Customer].[CompanyName], [od.Order.Customer].[ContactName], [od.Order.Customer].[ContactTitle], [od.Order.Customer].[Country], [od.Order.Customer].[Fax], [od.Order.Customer].[Phone], [od.Order.Customer].[PostalCode], [od.Order.Customer].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
WHERE [od.Order.Customer].[City] = N'Seattle'
ORDER BY [od].[OrderID], [od].[ProductID], [od.Order].[CustomerID]",
                Sql);
        }

        public override void Take_Select_Navigation()
        {
            base.Take_Select_Navigation();

            Assert.Equal(
                @"@__p_0: 2

SELECT [t].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]",
                Sql);
        }

        public override void Select_collection_FirstOrDefault_project_single_column1()
        {
            base.Select_collection_FirstOrDefault_project_single_column1();

            Assert.Equal(
                @"@__p_0: 2

SELECT (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [t].[CustomerID] = [o].[CustomerID]
)
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]",
                Sql);
        }

        public override void Select_collection_FirstOrDefault_project_single_column2()
        {
            base.Select_collection_FirstOrDefault_project_single_column2();

            Assert.Equal(
                @"@__p_0: 2

SELECT (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [t].[CustomerID] = [o].[CustomerID]
)
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]",
                Sql);
        }

        public override void Select_collection_FirstOrDefault_project_anonymous_type()
        {
            base.Select_collection_FirstOrDefault_project_anonymous_type();

            Assert.Equal(
                @"@__p_0: 2

SELECT [t].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]",
                Sql);
        }

        public override void Select_collection_FirstOrDefault_project_entity()
        {
            base.Select_collection_FirstOrDefault_project_entity();

            Assert.Equal(
                @"@__p_0: 2

SELECT [t].[CustomerID]
FROM (
    SELECT TOP(@__p_0) [c0].*
    FROM [Customers] AS [c0]
    ORDER BY [c0].[CustomerID]
) AS [t]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]",
                Sql);
        }

        public override void Skip_Select_Navigation()
        {
            base.Skip_Select_Navigation();

            if (TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true)
            {
                Assert.StartsWith(
                    @"@__p_0: 20

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
ORDER BY [o].[OrderID]
",
                    Sql);
            }
        }

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o.Customer].[City] = N'Seattle'
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Include_with_multiple_optional_navigations()
        {
            base.Include_with_multiple_optional_navigations();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order.Customer].[CustomerID], [od.Order.Customer].[Address], [od.Order.Customer].[City], [od.Order.Customer].[CompanyName], [od.Order.Customer].[ContactName], [od.Order.Customer].[ContactTitle], [od.Order.Customer].[Country], [od.Order.Customer].[Fax], [od.Order.Customer].[Phone], [od.Order.Customer].[PostalCode], [od.Order.Customer].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [od.Order.Customer].[City] = N'London'
ORDER BY [od.Order].[CustomerID]",
                Sql);
        }

        public override void Select_Navigation()
        {
            base.Select_Navigation();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_Navigations()
        {
            base.Select_Navigations();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_Where_Navigation_Multiple_Access()
        {
            base.Select_Where_Navigation_Multiple_Access();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE (([o.Customer].[City] = N'Seattle') AND [o.Customer].[City] IS NOT NULL) AND (([o.Customer].[Phone] <> N'555 555 5555') OR [o.Customer].[Phone] IS NULL)
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_Navigations_Where_Navigations()
        {
            base.Select_Navigations_Where_Navigations();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE (([o.Customer].[City] = N'Seattle') AND [o.Customer].[City] IS NOT NULL) AND (([o.Customer].[Phone] <> N'555 555 5555') OR [o.Customer].[Phone] IS NULL)
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE (([o.Customer].[City] = N'Seattle') AND [o.Customer].[City] IS NOT NULL) AND (([o.Customer].[Phone] <> N'555 555 5555') OR [o.Customer].[Phone] IS NULL)
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_count_plus_sum()
        {
            base.Select_count_plus_sum();

            Assert.Equal(
                @"SELECT (
    SELECT SUM([od0].[Quantity])
    FROM [Order Details] AS [od0]
    WHERE [o].[OrderID] = [od0].[OrderID]
) + (
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o].[OrderID] = [o1].[OrderID]
)
FROM [Orders] AS [o]",
                Sql);
        }

        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE (([o.Customer].[City] = N'Seattle') AND [o.Customer].[City] IS NOT NULL) AND (([o.Customer].[Phone] <> N'555 555 5555') OR [o.Customer].[Phone] IS NULL)
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o.Customer1].[CustomerID], [o.Customer1].[Address], [o.Customer1].[City], [o.Customer1].[CompanyName], [o.Customer1].[ContactName], [o.Customer1].[ContactTitle], [o.Customer1].[Country], [o.Customer1].[Fax], [o.Customer1].[Phone], [o.Customer1].[PostalCode], [o.Customer1].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
CROSS JOIN [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer1] ON [o0].[CustomerID] = [o.Customer1].[CustomerID]
WHERE (([o].[OrderID] < 10300) AND ([o0].[OrderID] < 10400)) AND (([o.Customer].[City] = [o.Customer1].[City]) OR ([o.Customer].[City] IS NULL AND [o.Customer1].[City] IS NULL))
ORDER BY [o].[CustomerID], [o0].[CustomerID]",
                Sql);
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region], [o.Customer1].[CustomerID], [o.Customer1].[Address], [o.Customer1].[City], [o.Customer1].[CompanyName], [o.Customer1].[ContactName], [o.Customer1].[ContactTitle], [o.Customer1].[Country], [o.Customer1].[Fax], [o.Customer1].[Phone], [o.Customer1].[PostalCode], [o.Customer1].[Region], [o0].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
CROSS JOIN [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer1] ON [o0].[CustomerID] = [o.Customer1].[CustomerID]
WHERE (([o].[OrderID] < 10300) AND ([o0].[OrderID] < 10400)) AND (([o.Customer].[City] = [o.Customer1].[City]) OR ([o.Customer].[City] IS NULL AND [o.Customer1].[City] IS NULL))
ORDER BY [o].[CustomerID], [o0].[CustomerID]",
                Sql);
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            Assert.Equal(
                @"SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Orders] AS [o1]
CROSS JOIN [Orders] AS [o2]
WHERE ([o1].[CustomerID] = [o2].[CustomerID]) OR ([o1].[CustomerID] IS NULL AND [o2].[CustomerID] IS NULL)",
                Sql);
        }

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                Sql);
        }

        public override void Select_Where_Navigation_Null_Deep()
        {
            base.Select_Where_Navigation_Null_Deep();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [e.Manager].[EmployeeID], [e.Manager].[City], [e.Manager].[Country], [e.Manager].[FirstName], [e.Manager].[ReportsTo], [e.Manager].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e.Manager] ON [e].[ReportsTo] = [e.Manager].[EmployeeID]
WHERE [e.Manager].[ReportsTo] IS NULL
ORDER BY [e].[ReportsTo]",
                Sql);
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            Assert.Equal(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL",
                Sql);
        }

        public override void Select_collection_navigation_simple()
        {
            base.Select_collection_navigation_simple();

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [c].[CustomerID]) = 1)
ORDER BY [c].[CustomerID]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]

@_outer_CustomerID: ANTON (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]

@_outer_CustomerID: AROUT (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]",
                Sql);
        }

        public override void Select_collection_navigation_multi_part()
        {
            base.Select_collection_navigation_multi_part();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[CustomerID]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]",
                Sql);
        }

        public override void Collection_select_nav_prop_any()
        {
            base.Collection_select_nav_prop_any();

            Assert.Equal(
                @"SELECT (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Orders] AS [o0]
            WHERE [c].[CustomerID] = [o0].[CustomerID])
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
)
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Collection_select_nav_prop_predicate()
        {
            base.Collection_select_nav_prop_predicate();

            Assert.Equal(
                @"SELECT CASE
    WHEN (
        SELECT COUNT(*)
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) > 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Collection_where_nav_prop_any()
        {
            base.Collection_where_nav_prop_any();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])",
                Sql);
        }

        public override void Collection_where_nav_prop_any_predicate()
        {
            base.Collection_where_nav_prop_any_predicate();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] > 0) AND ([c].[CustomerID] = [o].[CustomerID]))",
                Sql);
        }

        public override void Collection_select_nav_prop_all()
        {
            base.Collection_select_nav_prop_all();

            Assert.Equal(
                @"SELECT (
    SELECT CASE
        WHEN NOT EXISTS (
            SELECT 1
            FROM [Orders] AS [o0]
            WHERE (([c].[CustomerID] = [o0].[CustomerID]) AND [o0].[CustomerID] IS NOT NULL) AND (([o0].[CustomerID] <> N'ALFKI') OR [o0].[CustomerID] IS NULL))
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
)
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Collection_select_nav_prop_all_client()
        {
            base.Collection_select_nav_prop_all_client();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM [Orders] AS [o1]
WHERE @_outer_CustomerID = [o1].[CustomerID]",
                Sql);
        }

        public override void Collection_where_nav_prop_all()
        {
            base.Collection_where_nav_prop_all();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE (([c].[CustomerID] = [o].[CustomerID]) AND [o].[CustomerID] IS NOT NULL) AND (([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL))",
                Sql);
        }

        public override void Collection_where_nav_prop_all_client()
        {
            base.Collection_where_nav_prop_all_client();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]",
                Sql);
        }

        public override void Collection_select_nav_prop_count()
        {
            base.Collection_select_nav_prop_count();

            Assert.Equal(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
)
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Collection_where_nav_prop_count()
        {
            base.Collection_where_nav_prop_count();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) > 5",
                Sql);
        }

        public override void Collection_where_nav_prop_count_reverse()
        {
            base.Collection_where_nav_prop_count_reverse();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 5 < (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)",
                Sql);
        }

        public override void Collection_orderby_nav_prop_count()
        {
            base.Collection_orderby_nav_prop_count();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)",
                Sql);
        }

        public override void Collection_select_nav_prop_long_count()
        {
            base.Collection_select_nav_prop_long_count();

            Assert.Equal(
                @"SELECT (
    SELECT COUNT_BIG(*)
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
)
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Select_multiple_complex_projections()
        {
            base.Select_multiple_complex_projections();

            Assert.Equal(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o2]
    WHERE [o].[OrderID] = [o2].[OrderID]
), [o].[OrderDate], (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Order Details] AS [od1]
            WHERE ([od1].[UnitPrice] > 10.0) AND ([o].[OrderID] = [od1].[OrderID]))
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
), CASE
    WHEN [o].[CustomerID] = N'ALFKI'
    THEN N'50' ELSE N'10'
END, [o].[OrderID], (
    SELECT CASE
        WHEN NOT EXISTS (
            SELECT 1
            FROM [Order Details] AS [od2]
            WHERE ([o].[OrderID] = [od2].[OrderID]) AND ([od2].[OrderID] <> 42))
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
), (
    SELECT COUNT_BIG(*)
    FROM [Order Details] AS [o3]
    WHERE [o].[OrderID] = [o3].[OrderID]
)
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [o].[CustomerID]) = 1)",
                Sql);
        }

        public override void Collection_select_nav_prop_sum()
        {
            base.Collection_select_nav_prop_sum();

            Assert.Equal(
                @"SELECT (
    SELECT SUM([o0].[OrderID])
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
)
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Collection_where_nav_prop_sum()
        {
            base.Collection_where_nav_prop_sum();

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) > 1000",
                Sql);
        }

        public override void Collection_select_nav_prop_first_or_default()
        {
            base.Collection_select_nav_prop_first_or_default();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]",
                Sql);
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop();

            Assert.StartsWith(
                @"SELECT [e].[CustomerID]
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [e].[CustomerID]) = 1)
ORDER BY [e].[CustomerID]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT TOP(1) [e0].[OrderID], [e0].[CustomerID], [e0].[EmployeeID], [e0].[OrderDate], [e.Customer].[CustomerID], [e.Customer].[Address], [e.Customer].[City], [e.Customer].[CompanyName], [e.Customer].[ContactName], [e.Customer].[ContactTitle], [e.Customer].[Country], [e.Customer].[Fax], [e.Customer].[Phone], [e.Customer].[PostalCode], [e.Customer].[Region]
FROM [Orders] AS [e0]
LEFT JOIN [Customers] AS [e.Customer] ON [e0].[CustomerID] = [e.Customer].[CustomerID]
WHERE [e0].[OrderID] IN (10643, 10692, 10702, 10835, 10952, 11011) AND (@_outer_CustomerID = [e0].[CustomerID])
ORDER BY [e0].[CustomerID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT TOP(1) [e0].[OrderID], [e0].[CustomerID], [e0].[EmployeeID], [e0].[OrderDate], [e.Customer].[CustomerID], [e.Customer].[Address], [e.Customer].[City], [e.Customer].[CompanyName], [e.Customer].[ContactName], [e.Customer].[ContactTitle], [e.Customer].[Country], [e.Customer].[Fax], [e.Customer].[Phone], [e.Customer].[PostalCode], [e.Customer].[Region]
FROM [Orders] AS [e0]
LEFT JOIN [Customers] AS [e.Customer] ON [e0].[CustomerID] = [e.Customer].[CustomerID]
WHERE [e0].[OrderID] IN (10643, 10692, 10702, 10835, 10952, 11011) AND (@_outer_CustomerID = [e0].[CustomerID])
ORDER BY [e0].[CustomerID]",
                Sql);
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested();

            Assert.StartsWith(
                @"SELECT 1
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [e].[CustomerID]) = 1)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Collection_select_nav_prop_single_or_default_then_nav_prop_nested()
        {
            base.Collection_select_nav_prop_single_or_default_then_nav_prop_nested();

            Assert.StartsWith(
                @"SELECT 1
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [e].[CustomerID]) = 1)

SELECT TOP(2) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[OrderID] = 10643
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method();

            Assert.StartsWith(
                @"SELECT 1
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [e].[CustomerID]) = 1)

SELECT TOP(1) [oo].[OrderID], [oo].[CustomerID], [oo].[EmployeeID], [oo].[OrderDate], [oo.Customer].[CustomerID], [oo.Customer].[Address], [oo.Customer].[City], [oo.Customer].[CompanyName], [oo.Customer].[ContactName], [oo.Customer].[ContactTitle], [oo.Customer].[Country], [oo.Customer].[Fax], [oo.Customer].[Phone], [oo.Customer].[PostalCode], [oo.Customer].[Region]
FROM [Orders] AS [oo]
LEFT JOIN [Customers] AS [oo.Customer] ON [oo].[CustomerID] = [oo.Customer].[CustomerID]
WHERE [oo].[CustomerID] = N'ALFKI'
ORDER BY [oo].[CustomerID]",
                Sql);
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby();

            Assert.StartsWith(
                @"SELECT 1
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [e].[CustomerID]) = 1)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Navigation_fk_based_inside_contains()
        {
            base.Navigation_fk_based_inside_contains();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IN (N'ALFKI')",
                Sql);
        }

        public override void Navigation_inside_contains()
        {
            base.Navigation_inside_contains();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Navigation_inside_contains_nested()
        {
            base.Navigation_inside_contains_nested();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order.Customer].[CustomerID], [od.Order.Customer].[Address], [od.Order.Customer].[City], [od.Order.Customer].[CompanyName], [od.Order.Customer].[ContactName], [od.Order.Customer].[ContactTitle], [od.Order.Customer].[Country], [od.Order.Customer].[Fax], [od.Order.Customer].[Phone], [od.Order.Customer].[PostalCode], [od.Order.Customer].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
ORDER BY [od.Order].[CustomerID]",
                Sql);
        }

        public override void Navigation_from_join_clause_inside_contains()
        {
            base.Navigation_from_join_clause_inside_contains();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[CustomerID]",
                Sql);
        }

        public override void Where_subquery_on_navigation()
        {
            base.Where_subquery_on_navigation();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o].[OrderID], [o].[ProductID]
        FROM [Order Details] AS [o]
        WHERE [p].[ProductID] = [o].[ProductID]
    ) AS [t00]
    INNER JOIN (
        SELECT TOP(1) [orderDetail].[OrderID], [orderDetail].[ProductID]
        FROM [Order Details] AS [orderDetail]
        WHERE [orderDetail].[Quantity] = 1
    ) AS [t1] ON ([t00].[OrderID] = [t1].[OrderID]) AND ([t00].[ProductID] = [t1].[ProductID]))",
                Sql);
        }

        public override void Where_subquery_on_navigation2()
        {
            base.Where_subquery_on_navigation2();

            Assert.Equal(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o].[OrderID], [o].[ProductID]
        FROM [Order Details] AS [o]
        WHERE [p].[ProductID] = [o].[ProductID]
    ) AS [t00]
    INNER JOIN (
        SELECT TOP(1) [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        ORDER BY [o0].[OrderID] DESC, [o0].[ProductID]
    ) AS [t1] ON ([t00].[OrderID] = [t1].[OrderID]) AND ([t00].[ProductID] = [t1].[ProductID]))",
                Sql);
        }

        public override void Where_subquery_on_navigation_client_eval()
        {
            base.Where_subquery_on_navigation_client_eval();

            Assert.StartsWith(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]

SELECT [o4].[OrderID]
FROM [Orders] AS [o4]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT [o2].[OrderID]
FROM [Orders] AS [o2]
WHERE @_outer_CustomerID = [o2].[CustomerID]

SELECT [o4].[OrderID]
FROM [Orders] AS [o4]

@_outer_CustomerID: ANATR (Size = 450)

SELECT [o2].[OrderID]
FROM [Orders] AS [o2]
WHERE @_outer_CustomerID = [o2].[CustomerID]

SELECT [o4].[OrderID]
FROM [Orders] AS [o4]",
                Sql);
        }

        public override void Navigation_in_subquery_referencing_outer_query()
        {
            base.Navigation_in_subquery_referencing_outer_query();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[OrderID] IN (10643, 10692)
ORDER BY [o].[CustomerID]

SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order.Customer].[CustomerID], [od.Order.Customer].[Address], [od.Order.Customer].[City], [od.Order.Customer].[CompanyName], [od.Order.Customer].[ContactName], [od.Order.Customer].[ContactTitle], [od.Order.Customer].[Country], [od.Order.Customer].[Fax], [od.Order.Customer].[Phone], [od.Order.Customer].[PostalCode], [od.Order.Customer].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
ORDER BY [od.Order].[CustomerID]

SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order.Customer].[CustomerID], [od.Order.Customer].[Address], [od.Order.Customer].[City], [od.Order.Customer].[CompanyName], [od.Order.Customer].[ContactName], [od.Order.Customer].[ContactTitle], [od.Order.Customer].[Country], [od.Order.Customer].[Fax], [od.Order.Customer].[Phone], [od.Order.Customer].[PostalCode], [od.Order.Customer].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
ORDER BY [od.Order].[CustomerID]",
                Sql);
        }

        public override void GroupBy_on_nav_prop()
        {
            base.GroupBy_on_nav_prop();

            Assert.Equal(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o.Customer].[City]",
                Sql);
        }

        public override void Where_nav_prop_group_by()
        {
            base.Where_nav_prop_group_by();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
WHERE [od.Order].[CustomerID] = N'ALFKI'
ORDER BY [od].[Quantity]",
                Sql);
        }

        public override void Let_group_by_nav_prop()
        {
            base.Let_group_by_nav_prop();

            Assert.Equal(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order].[CustomerID]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
ORDER BY [od.Order].[CustomerID]",
                Sql);
        }

        public override void Project_first_or_default_on_empty_collection_of_value_types_returns_proper_default()
        {
            base.Project_first_or_default_on_empty_collection_of_value_types_returns_proper_default();

            Assert.Equal(
                @"",
                Sql);
        }

        public override void Project_single_scalar_value_subquery_is_properly_inlined()
        {
            base.Project_single_scalar_value_subquery_is_properly_inlined();

            Assert.Equal(
                @"SELECT [c].[CustomerID], (
    SELECT TOP(1) [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] = [o0].[CustomerID]
    ORDER BY [o0].[OrderID]
)
FROM [Customers] AS [c]",
                Sql);
        }

        public override void Project_single_entity_value_subquery_works()
        {
            base.Project_single_entity_value_subquery_works();

            Assert.Equal(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [c].[CustomerID]) = 1)
ORDER BY [c].[CustomerID]

@_outer_CustomerID: ALFKI (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]

@_outer_CustomerID: ANATR (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]

@_outer_CustomerID: ANTON (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]

@_outer_CustomerID: AROUT (Size = 450)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                Sql);
        }

        public override void Project_single_scalar_value_subquery_in_query_with_optional_navigation_works()
        {
            base.Project_single_scalar_value_subquery_in_query_with_optional_navigation_works();

            Assert.Equal(
                @"@__p_0: 3

SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[OrderID], [o].[CustomerID]

@_outer_OrderID: 10248

SELECT TOP(1) [od1].[OrderID]
FROM [Order Details] AS [od1]
WHERE @_outer_OrderID = [od1].[OrderID]
ORDER BY [od1].[OrderID], [od1].[ProductID]

@_outer_OrderID: 10249

SELECT TOP(1) [od1].[OrderID]
FROM [Order Details] AS [od1]
WHERE @_outer_OrderID = [od1].[OrderID]
ORDER BY [od1].[OrderID], [od1].[ProductID]

@_outer_OrderID: 10250

SELECT TOP(1) [od1].[OrderID]
FROM [Order Details] AS [od1]
WHERE @_outer_OrderID = [od1].[OrderID]
ORDER BY [od1].[OrderID], [od1].[ProductID]",
                Sql);
        }

        public override void GroupJoin_with_complex_subquery_and_LOJ_does_not_get_flattened()
        {
            base.GroupJoin_with_complex_subquery_and_LOJ_does_not_get_flattened();

            Assert.Contains(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT [c20].*
    FROM [Order Details] AS [od0]
    INNER JOIN [Orders] AS [o0] ON [od0].[OrderID] = 10260
    INNER JOIN [Customers] AS [c20] ON [o0].[CustomerID] = [c20].[CustomerID]
) AS [t]",
                Sql);

            Assert.Contains(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]",
                Sql);
        }

        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
