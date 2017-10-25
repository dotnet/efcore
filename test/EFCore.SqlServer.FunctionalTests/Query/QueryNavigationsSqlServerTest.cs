// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class QueryNavigationsSqlServerTest : QueryNavigationsTestBase<NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public QueryNavigationsSqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Join_with_nav_projected_in_subquery_when_client_eval()
        {
            base.Join_with_nav_projected_in_subquery_when_client_eval();

            AssertContainsSql(
                @"SELECT [od0].[OrderID], [od0].[ProductID], [od0].[Discount], [od0].[Quantity], [od0].[UnitPrice], [od.Product0].[ProductID], [od.Product0].[Discontinued], [od.Product0].[ProductName], [od.Product0].[SupplierID], [od.Product0].[UnitPrice], [od.Product0].[UnitsInStock]
FROM [Order Details] AS [od0]
INNER JOIN [Products] AS [od.Product0] ON [od0].[ProductID] = [od.Product0].[ProductID]",
                //
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o.Customer0].[CustomerID], [o.Customer0].[Address], [o.Customer0].[City], [o.Customer0].[CompanyName], [o.Customer0].[ContactName], [o.Customer0].[ContactTitle], [o.Customer0].[Country], [o.Customer0].[Fax], [o.Customer0].[Phone], [o.Customer0].[PostalCode], [o.Customer0].[Region]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void GroupJoin_with_nav_projected_in_subquery_when_client_eval()
        {
            base.GroupJoin_with_nav_projected_in_subquery_when_client_eval();

            AssertContainsSql(
                @"SELECT [od0].[OrderID], [od0].[ProductID], [od0].[Discount], [od0].[Quantity], [od0].[UnitPrice], [od.Product0].[ProductID], [od.Product0].[Discontinued], [od.Product0].[ProductName], [od.Product0].[SupplierID], [od.Product0].[UnitPrice], [od.Product0].[UnitsInStock]
FROM [Order Details] AS [od0]
INNER JOIN [Products] AS [od.Product0] ON [od0].[ProductID] = [od.Product0].[ProductID]",
                //
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o.Customer0].[CustomerID], [o.Customer0].[Address], [o.Customer0].[City], [o.Customer0].[CompanyName], [o.Customer0].[ContactName], [o.Customer0].[ContactTitle], [o.Customer0].[Country], [o.Customer0].[Fax], [o.Customer0].[Phone], [o.Customer0].[PostalCode], [o.Customer0].[Region]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Join_with_nav_in_predicate_in_subquery_when_client_eval()
        {
            base.Join_with_nav_in_predicate_in_subquery_when_client_eval();

            AssertContainsSql(
                @"SELECT [od0].[OrderID], [od0].[ProductID], [od0].[Discount], [od0].[Quantity], [od0].[UnitPrice], [od.Product0].[ProductID], [od.Product0].[Discontinued], [od.Product0].[ProductName], [od.Product0].[SupplierID], [od.Product0].[UnitPrice], [od.Product0].[UnitsInStock]
FROM [Order Details] AS [od0]
INNER JOIN [Products] AS [od.Product0] ON [od0].[ProductID] = [od.Product0].[ProductID]",
                //
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o.Customer0].[CustomerID], [o.Customer0].[Address], [o.Customer0].[City], [o.Customer0].[CompanyName], [o.Customer0].[ContactName], [o.Customer0].[ContactTitle], [o.Customer0].[Country], [o.Customer0].[Fax], [o.Customer0].[Phone], [o.Customer0].[PostalCode], [o.Customer0].[Region]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void GroupJoin_with_nav_in_predicate_in_subquery_when_client_eval()
        {
            base.GroupJoin_with_nav_in_predicate_in_subquery_when_client_eval();

            AssertContainsSql(
                @"SELECT [od0].[OrderID], [od0].[ProductID], [od0].[Discount], [od0].[Quantity], [od0].[UnitPrice], [od.Product0].[ProductID], [od.Product0].[Discontinued], [od.Product0].[ProductName], [od.Product0].[SupplierID], [od.Product0].[UnitPrice], [od.Product0].[UnitsInStock]
FROM [Order Details] AS [od0]
INNER JOIN [Products] AS [od.Product0] ON [od0].[ProductID] = [od.Product0].[ProductID]",
                //
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o.Customer0].[CustomerID], [o.Customer0].[Address], [o.Customer0].[City], [o.Customer0].[CompanyName], [o.Customer0].[ContactName], [o.Customer0].[ContactTitle], [o.Customer0].[Country], [o.Customer0].[Fax], [o.Customer0].[Phone], [o.Customer0].[PostalCode], [o.Customer0].[Region]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Join_with_nav_in_orderby_in_subquery_when_client_eval()
        {
            base.Join_with_nav_in_orderby_in_subquery_when_client_eval();

            AssertContainsSql(
                @"SELECT [od0].[OrderID], [od0].[ProductID], [od0].[Discount], [od0].[Quantity], [od0].[UnitPrice], [od.Product0].[ProductID], [od.Product0].[Discontinued], [od.Product0].[ProductName], [od.Product0].[SupplierID], [od.Product0].[UnitPrice], [od.Product0].[UnitsInStock]
FROM [Order Details] AS [od0]
INNER JOIN [Products] AS [od.Product0] ON [od0].[ProductID] = [od.Product0].[ProductID]",
                //
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o.Customer0].[CustomerID], [o.Customer0].[Address], [o.Customer0].[City], [o.Customer0].[CompanyName], [o.Customer0].[ContactName], [o.Customer0].[ContactTitle], [o.Customer0].[Country], [o.Customer0].[Fax], [o.Customer0].[Phone], [o.Customer0].[PostalCode], [o.Customer0].[Region]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void GroupJoin_with_nav_in_orderby_in_subquery_when_client_eval()
        {
            base.GroupJoin_with_nav_in_orderby_in_subquery_when_client_eval();

            AssertContainsSql(
                @"SELECT [od0].[OrderID], [od0].[ProductID], [od0].[Discount], [od0].[Quantity], [od0].[UnitPrice], [od.Product0].[ProductID], [od.Product0].[Discontinued], [od.Product0].[ProductName], [od.Product0].[SupplierID], [od.Product0].[UnitPrice], [od.Product0].[UnitsInStock]
FROM [Order Details] AS [od0]
INNER JOIN [Products] AS [od.Product0] ON [od0].[ProductID] = [od.Product0].[ProductID]",
                //
                @"SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate], [o.Customer0].[CustomerID], [o.Customer0].[Address], [o.Customer0].[City], [o.Customer0].[CompanyName], [o.Customer0].[ContactName], [o.Customer0].[ContactTitle], [o.Customer0].[Country], [o.Customer0].[Fax], [o.Customer0].[Phone], [o.Customer0].[PostalCode], [o.Customer0].[Region]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o.Customer].[City] = N'Seattle'");
        }

        public override void Select_Where_Navigation_Contains()
        {
            base.Select_Where_Navigation_Contains();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE CHARINDEX(N'Sea', [o.Customer].[City]) > 0");
        }

        public override void Select_Where_Navigation_Deep()
        {
            base.Select_Where_Navigation_Deep();

            AssertSql(
                @"@__p_0='1'

SELECT TOP(@__p_0) [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
WHERE [od.Order.Customer].[City] = N'Seattle'
ORDER BY [od].[OrderID], [od].[ProductID]");
        }

        public override void Take_Select_Navigation()
        {
            base.Take_Select_Navigation();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override void Select_collection_FirstOrDefault_project_single_column1()
        {
            base.Select_collection_FirstOrDefault_project_single_column1();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
)
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_collection_FirstOrDefault_project_single_column2()
        {
            base.Select_collection_FirstOrDefault_project_single_column2();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
)
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]");
        }

        public override void Select_collection_FirstOrDefault_project_anonymous_type()
        {
            base.Select_collection_FirstOrDefault_project_anonymous_type();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT TOP(1) [o].[CustomerID], [o].[OrderID]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override void Select_collection_FirstOrDefault_project_entity()
        {
            base.Select_collection_FirstOrDefault_project_entity();

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override void Skip_Select_Navigation()
        {
            base.Skip_Select_Navigation();

            if (TestEnvironment.GetFlag(nameof(SqlServerCondition.SupportsOffset)) ?? true)
            {
                AssertSql(
                    @"@__p_0='20'

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]
OFFSET @__p_0 ROWS",
                    //
                    @"@_outer_CustomerID='FAMIA' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                    //
                    @"@_outer_CustomerID='FISSA' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]");
            }
        }

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o.Customer].[City] = N'Seattle'");
        }

        public override void Include_with_multiple_optional_navigations()
        {
            base.Include_with_multiple_optional_navigations();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order].[OrderID], [od.Order].[CustomerID], [od.Order].[EmployeeID], [od.Order].[OrderDate], [od.Order.Customer].[CustomerID], [od.Order.Customer].[Address], [od.Order.Customer].[City], [od.Order.Customer].[CompanyName], [od.Order.Customer].[ContactName], [od.Order.Customer].[ContactTitle], [od.Order.Customer].[Country], [od.Order.Customer].[Fax], [od.Order.Customer].[Phone], [od.Order.Customer].[PostalCode], [od.Order.Customer].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
WHERE [od.Order.Customer].[City] = N'London'");
        }

        public override void Select_Navigation()
        {
            base.Select_Navigation();

            AssertSql(
                @"SELECT [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]");
        }

        public override void Select_Navigations()
        {
            base.Select_Navigations();

            AssertSql(
                @"SELECT [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]");
        }

        public override void Select_Where_Navigation_Multiple_Access()
        {
            base.Select_Where_Navigation_Multiple_Access();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE ([o.Customer].[City] = N'Seattle') AND (([o.Customer].[Phone] <> N'555 555 5555') OR [o.Customer].[Phone] IS NULL)");
        }

        public override void Select_Navigations_Where_Navigations()
        {
            base.Select_Navigations_Where_Navigations();

            AssertSql(
                @"SELECT [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE ([o.Customer].[City] = N'Seattle') AND (([o.Customer].[Phone] <> N'555 555 5555') OR [o.Customer].[Phone] IS NULL)");
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]");
        }

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();

            AssertSql(
                @"SELECT [o.Customer].[CustomerID], [o.Customer].[Address], [o.Customer].[City] AS [B], [o.Customer].[CompanyName], [o.Customer].[ContactName], [o.Customer].[ContactTitle], [o.Customer].[Country], [o.Customer].[Fax], [o.Customer].[Phone], [o.Customer].[PostalCode], [o.Customer].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE ([o.Customer].[City] = N'Seattle') AND (([o.Customer].[Phone] <> N'555 555 5555') OR [o.Customer].[Phone] IS NULL)");
        }

        public override void Select_count_plus_sum()
        {
            base.Select_count_plus_sum();

            AssertSql(
                @"SELECT (
    SELECT SUM([od].[Quantity])
    FROM [Order Details] AS [od]
    WHERE [o].[OrderID] = [od].[OrderID]
) + (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]
) AS [Total]
FROM [Orders] AS [o]");
        }

        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();

            AssertSql(
                @"SELECT [o.Customer].[City] AS [B]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE ([o.Customer].[City] = N'Seattle') AND (([o.Customer].[Phone] <> N'555 555 5555') OR [o.Customer].[Phone] IS NULL)");
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
CROSS JOIN [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
WHERE (([o].[OrderID] < 10300) AND ([o0].[OrderID] < 10400)) AND (([o.Customer].[City] = [o.Customer0].[City]) OR ([o.Customer].[City] IS NULL AND [o.Customer0].[City] IS NULL))");
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();

            AssertSql(
                @"SELECT [o].[CustomerID] AS [CustomerID0], [o0].[CustomerID] AS [C2]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
CROSS JOIN [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
WHERE (([o].[OrderID] < 10300) AND ([o0].[OrderID] < 10400)) AND (([o.Customer].[City] = [o.Customer0].[City]) OR ([o.Customer].[City] IS NULL AND [o.Customer0].[City] IS NULL))");
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();

            AssertSql(
                @"SELECT [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Orders] AS [o1]
CROSS JOIN [Orders] AS [o2]
WHERE (([o1].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o1].[CustomerID], LEN(N'A')) = N'A')) AND ([o2].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o2].[CustomerID], LEN(N'A')) = N'A'))) AND (([o1].[CustomerID] = [o2].[CustomerID]) OR ([o1].[CustomerID] IS NULL AND [o2].[CustomerID] IS NULL))");
        }

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override void Select_Where_Navigation_Null_Deep()
        {
            base.Select_Where_Navigation_Null_Deep();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
LEFT JOIN [Employees] AS [e.Manager] ON [e].[ReportsTo] = [e.Manager].[EmployeeID]
WHERE [e.Manager].[ReportsTo] IS NULL");
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Employees] AS [e]
WHERE [e].[ReportsTo] IS NULL");
        }

        public override void Select_collection_navigation_simple()
        {
            base.Select_collection_navigation_simple();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate], [t].[CustomerID]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c0].[CustomerID], LEN(N'A')) = N'A')
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[CustomerID]");
        }

        public override void Select_collection_navigation_multi_part()
        {
            base.Select_collection_navigation_multi_part();

            AssertSql(
                @"SELECT [o].[OrderID], [o.Customer].[CustomerID]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [o].[OrderID], [o.Customer].[CustomerID]",
                //
                @"SELECT [o.Customer.Orders].[OrderID], [o.Customer.Orders].[CustomerID], [o.Customer.Orders].[EmployeeID], [o.Customer.Orders].[OrderDate], [t].[OrderID], [t].[CustomerID]
FROM [Orders] AS [o.Customer.Orders]
INNER JOIN (
    SELECT [o0].[OrderID], [o.Customer0].[CustomerID]
    FROM [Orders] AS [o0]
    LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
    WHERE [o0].[CustomerID] = N'ALFKI'
) AS [t] ON [o.Customer.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[OrderID], [t].[CustomerID]");
        }

        public override void Select_collection_navigation_multi_part2()
        {
            base.Select_collection_navigation_multi_part2();

            AssertSql(
                @"SELECT [od.Order.Customer].[CustomerID]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
WHERE [od.Order].[CustomerID] IN (N'ALFKI', N'ANTON')
ORDER BY [od].[OrderID], [od].[ProductID], [od.Order.Customer].[CustomerID]",
                //
                @"SELECT [od.Order.Customer.Orders].[OrderID], [od.Order.Customer.Orders].[CustomerID], [od.Order.Customer.Orders].[EmployeeID], [od.Order.Customer.Orders].[OrderDate], [t].[OrderID], [t].[ProductID], [t].[CustomerID]
FROM [Orders] AS [od.Order.Customer.Orders]
INNER JOIN (
    SELECT [od0].[OrderID], [od0].[ProductID], [od.Order.Customer0].[CustomerID]
    FROM [Order Details] AS [od0]
    INNER JOIN [Orders] AS [od.Order0] ON [od0].[OrderID] = [od.Order0].[OrderID]
    LEFT JOIN [Customers] AS [od.Order.Customer0] ON [od.Order0].[CustomerID] = [od.Order.Customer0].[CustomerID]
    WHERE [od.Order0].[CustomerID] IN (N'ALFKI', N'ANTON')
) AS [t] ON [od.Order.Customer.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[OrderID], [t].[ProductID], [t].[CustomerID]");
        }

        public override void Collection_select_nav_prop_any()
        {
            base.Collection_select_nav_prop_any();

            AssertSql(
                @"SELECT (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Orders] AS [o]
            WHERE [c].[CustomerID] = [o].[CustomerID])
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
) AS [Any]
FROM [Customers] AS [c]");
        }

        public override void Collection_select_nav_prop_predicate()
        {
            base.Collection_select_nav_prop_predicate();

            AssertSql(
                @"SELECT CASE
    WHEN (
        SELECT COUNT(*)
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) > 0
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END
FROM [Customers] AS [c]");
        }

        public override void Collection_where_nav_prop_any()
        {
            base.Collection_where_nav_prop_any();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID])");
        }

        public override void Collection_where_nav_prop_any_predicate()
        {
            base.Collection_where_nav_prop_any_predicate();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[OrderID] > 0) AND ([c].[CustomerID] = [o].[CustomerID]))");
        }

        public override void Collection_select_nav_prop_all()
        {
            base.Collection_select_nav_prop_all();

            AssertSql(
                @"SELECT (
    SELECT CASE
        WHEN NOT EXISTS (
            SELECT 1
            FROM [Orders] AS [o]
            WHERE ([c].[CustomerID] = [o].[CustomerID]) AND (([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL))
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
) AS [All]
FROM [Customers] AS [c]");
        }

        public override void Collection_select_nav_prop_all_client()
        {
            base.Collection_select_nav_prop_all_client();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
WHERE @_outer_CustomerID = [o0].[CustomerID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o0]
WHERE @_outer_CustomerID = [o0].[CustomerID]");
        }

        public override void Collection_where_nav_prop_all()
        {
            base.Collection_where_nav_prop_all();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE NOT EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([c].[CustomerID] = [o].[CustomerID]) AND (([o].[CustomerID] <> N'ALFKI') OR [o].[CustomerID] IS NULL))");
        }

        public override void Collection_where_nav_prop_all_client()
        {
            base.Collection_where_nav_prop_all_client();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]");
        }

        public override void Collection_select_nav_prop_count()
        {
            base.Collection_select_nav_prop_count();

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [Count]
FROM [Customers] AS [c]");
        }

        public override void Collection_where_nav_prop_count()
        {
            base.Collection_where_nav_prop_count();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) > 5");
        }

        public override void Collection_where_nav_prop_count_reverse()
        {
            base.Collection_where_nav_prop_count_reverse();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 5 < (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)");
        }

        public override void Collection_orderby_nav_prop_count()
        {
            base.Collection_orderby_nav_prop_count();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
)");
        }

        public override void Collection_select_nav_prop_long_count()
        {
            base.Collection_select_nav_prop_long_count();

            AssertSql(
                @"SELECT (
    SELECT COUNT_BIG(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [C]
FROM [Customers] AS [c]");
        }

        public override void Select_multiple_complex_projections()
        {
            base.Select_multiple_complex_projections();

            AssertSql(
                @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]
) AS [collection1], [o].[OrderDate] AS [scalar1], (
    SELECT CASE
        WHEN EXISTS (
            SELECT 1
            FROM [Order Details] AS [od]
            WHERE ([od].[UnitPrice] > 10.0) AND ([o].[OrderID] = [od].[OrderID]))
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
) AS [any], CASE
    WHEN [o].[CustomerID] = N'ALFKI'
    THEN N'50' ELSE N'10'
END AS [conditional], [o].[OrderID] AS [scalar2], (
    SELECT CASE
        WHEN NOT EXISTS (
            SELECT 1
            FROM [Order Details] AS [od0]
            WHERE ([o].[OrderID] = [od0].[OrderID]) AND ([od0].[OrderID] <> 42))
        THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
    END
) AS [all], (
    SELECT COUNT_BIG(*)
    FROM [Order Details] AS [o1]
    WHERE [o].[OrderID] = [o1].[OrderID]
) AS [collection2]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] LIKE N'A' + N'%' AND (LEFT([o].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Collection_select_nav_prop_sum()
        {
            base.Collection_select_nav_prop_sum();

            AssertSql(
                @"SELECT (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [Sum]
FROM [Customers] AS [c]");
        }

        public override void Collection_where_nav_prop_sum()
        {
            base.Collection_where_nav_prop_sum();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (
    SELECT SUM([o].[OrderID])
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) > 1000");
        }

        public override void Collection_select_nav_prop_first_or_default()
        {
            base.Collection_select_nav_prop_first_or_default();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]");
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop();

            AssertSql(
                @"SELECT [e].[CustomerID]
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (LEFT([e].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [e].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(1) [e.Customer].[CustomerID], [e.Customer].[Address], [e.Customer].[City], [e.Customer].[CompanyName], [e.Customer].[ContactName], [e.Customer].[ContactTitle], [e.Customer].[Country], [e.Customer].[Fax], [e.Customer].[Phone], [e.Customer].[PostalCode], [e.Customer].[Region]
FROM [Orders] AS [e0]
LEFT JOIN [Customers] AS [e.Customer] ON [e0].[CustomerID] = [e.Customer].[CustomerID]
WHERE [e0].[OrderID] IN (10643, 10692, 10702, 10835, 10952, 11011) AND (@_outer_CustomerID = [e0].[CustomerID])",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT TOP(1) [e.Customer].[CustomerID], [e.Customer].[Address], [e.Customer].[City], [e.Customer].[CompanyName], [e.Customer].[ContactName], [e.Customer].[ContactTitle], [e.Customer].[Country], [e.Customer].[Fax], [e.Customer].[Phone], [e.Customer].[PostalCode], [e.Customer].[Region]
FROM [Orders] AS [e0]
LEFT JOIN [Customers] AS [e.Customer] ON [e0].[CustomerID] = [e.Customer].[CustomerID]
WHERE [e0].[OrderID] IN (10643, 10692, 10702, 10835, 10952, 11011) AND (@_outer_CustomerID = [e0].[CustomerID])",
                //
                @"@_outer_CustomerID='ANTON' (Size = 5)

SELECT TOP(1) [e.Customer].[CustomerID], [e.Customer].[Address], [e.Customer].[City], [e.Customer].[CompanyName], [e.Customer].[ContactName], [e.Customer].[ContactTitle], [e.Customer].[Country], [e.Customer].[Fax], [e.Customer].[Phone], [e.Customer].[PostalCode], [e.Customer].[Region]
FROM [Orders] AS [e0]
LEFT JOIN [Customers] AS [e.Customer] ON [e0].[CustomerID] = [e.Customer].[CustomerID]
WHERE [e0].[OrderID] IN (10643, 10692, 10702, 10835, 10952, 11011) AND (@_outer_CustomerID = [e0].[CustomerID])",
                //
                @"@_outer_CustomerID='AROUT' (Size = 5)

SELECT TOP(1) [e.Customer].[CustomerID], [e.Customer].[Address], [e.Customer].[City], [e.Customer].[CompanyName], [e.Customer].[ContactName], [e.Customer].[ContactTitle], [e.Customer].[Country], [e.Customer].[Fax], [e.Customer].[Phone], [e.Customer].[PostalCode], [e.Customer].[Region]
FROM [Orders] AS [e0]
LEFT JOIN [Customers] AS [e.Customer] ON [e0].[CustomerID] = [e.Customer].[CustomerID]
WHERE [e0].[OrderID] IN (10643, 10692, 10702, 10835, 10952, 11011) AND (@_outer_CustomerID = [e0].[CustomerID])");
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o.Customer].[City]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
    WHERE [o].[CustomerID] = N'ALFKI'
)
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (LEFT([e].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Collection_select_nav_prop_single_or_default_then_nav_prop_nested()
        {
            base.Collection_select_nav_prop_single_or_default_then_nav_prop_nested();

            AssertSql(
                @"SELECT 1
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (LEFT([e].[CustomerID], LEN(N'A')) = N'A')",
                //
                @"SELECT TOP(2) [o.Customer0].[City]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
WHERE [o0].[OrderID] = 10643",
                //
                @"SELECT TOP(2) [o.Customer0].[City]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
WHERE [o0].[OrderID] = 10643",
                //
                @"SELECT TOP(2) [o.Customer0].[City]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
WHERE [o0].[OrderID] = 10643",
                //
                @"SELECT TOP(2) [o.Customer0].[City]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [o.Customer0] ON [o0].[CustomerID] = [o.Customer0].[CustomerID]
WHERE [o0].[OrderID] = 10643");
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested_using_property_method();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [oo.Customer].[City]
    FROM [Orders] AS [oo]
    LEFT JOIN [Customers] AS [oo.Customer] ON [oo].[CustomerID] = [oo.Customer].[CustomerID]
    WHERE [oo].[CustomerID] = N'ALFKI'
)
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (LEFT([e].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby()
        {
            base.Collection_select_nav_prop_first_or_default_then_nav_prop_nested_with_orderby();

            AssertSql(
                @"SELECT (
    SELECT TOP(1) [o.Customer].[City]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
    WHERE [o].[CustomerID] = N'ALFKI'
    ORDER BY [o].[CustomerID]
)
FROM [Customers] AS [e]
WHERE [e].[CustomerID] LIKE N'A' + N'%' AND (LEFT([e].[CustomerID], LEN(N'A')) = N'A')");
        }

        public override void Navigation_fk_based_inside_contains()
        {
            base.Navigation_fk_based_inside_contains();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IN (N'ALFKI')");
        }

        public override void Navigation_inside_contains()
        {
            base.Navigation_inside_contains();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o.Customer].[City] IN (N'Novigrad', N'Seattle')");
        }

        public override void Navigation_inside_contains_nested()
        {
            base.Navigation_inside_contains_nested();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
WHERE [od.Order.Customer].[City] IN (N'Novigrad', N'Seattle')");
        }

        public override void Navigation_from_join_clause_inside_contains()
        {
            base.Navigation_from_join_clause_inside_contains();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE [o.Customer].[Country] IN (N'USA', N'Redania')");
        }

        public override void Where_subquery_on_navigation()
        {
            base.Where_subquery_on_navigation();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o].[OrderID], [o].[ProductID]
        FROM [Order Details] AS [o]
        WHERE [p].[ProductID] = [o].[ProductID]
    ) AS [t0]
    INNER JOIN (
        SELECT TOP(1) [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[Quantity] = 1
        ORDER BY [o0].[OrderID] DESC, [o0].[ProductID]
    ) AS [t1] ON ([t0].[OrderID] = [t1].[OrderID]) AND ([t0].[ProductID] = [t1].[ProductID]))");
        }

        public override void Where_subquery_on_navigation2()
        {
            base.Where_subquery_on_navigation2();

            AssertSql(
                @"SELECT [p].[ProductID], [p].[Discontinued], [p].[ProductName], [p].[SupplierID], [p].[UnitPrice], [p].[UnitsInStock]
FROM [Products] AS [p]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o].[OrderID], [o].[ProductID]
        FROM [Order Details] AS [o]
        WHERE [p].[ProductID] = [o].[ProductID]
    ) AS [t0]
    INNER JOIN (
        SELECT TOP(1) [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        ORDER BY [o0].[OrderID] DESC, [o0].[ProductID]
    ) AS [t1] ON ([t0].[OrderID] = [t1].[OrderID]) AND ([t0].[ProductID] = [t1].[ProductID]))");
        }

        public override void Where_subquery_on_navigation_client_eval()
        {
            base.Where_subquery_on_navigation_client_eval();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID]",
                //
                @"SELECT [o4].[OrderID]
FROM [Orders] AS [o4]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT [o2].[OrderID]
FROM [Orders] AS [o2]
WHERE @_outer_CustomerID = [o2].[CustomerID]",
                //
                @"SELECT [o4].[OrderID]
FROM [Orders] AS [o4]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT [o2].[OrderID]
FROM [Orders] AS [o2]
WHERE @_outer_CustomerID = [o2].[CustomerID]",
                //
                @"SELECT [o4].[OrderID]
FROM [Orders] AS [o4]");
        }

        public override void Navigation_in_subquery_referencing_outer_query()
        {
            base.Navigation_in_subquery_referencing_outer_query();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE ((
    SELECT COUNT(*)
    FROM [Order Details] AS [od]
    INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
    LEFT JOIN [Customers] AS [od.Order.Customer] ON [od.Order].[CustomerID] = [od.Order.Customer].[CustomerID]
    WHERE ([o.Customer].[Country] = [od.Order.Customer].[Country]) OR ([o.Customer].[Country] IS NULL AND [od.Order.Customer].[Country] IS NULL)
) > 0) AND [o].[OrderID] IN (10643, 10692)");
        }

        public override void GroupBy_on_nav_prop()
        {
            base.GroupBy_on_nav_prop();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o.Customer].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o.Customer].[City]");
        }

        public override void Where_nav_prop_group_by()
        {
            base.Where_nav_prop_group_by();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
WHERE [od.Order].[CustomerID] = N'ALFKI'
ORDER BY [od].[Quantity]");
        }

        public override void Let_group_by_nav_prop()
        {
            base.Let_group_by_nav_prop();

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [od.Order].[CustomerID] AS [customer]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
ORDER BY [od.Order].[CustomerID]");
        }

        public override void Project_first_or_default_on_empty_collection_of_value_types_returns_proper_default()
        {
            base.Project_first_or_default_on_empty_collection_of_value_types_returns_proper_default();

            AssertSql(
                "");
        }

        public override void Project_single_scalar_value_subquery_is_properly_inlined()
        {
            base.Project_single_scalar_value_subquery_is_properly_inlined();

            AssertSql(
                @"SELECT [c].[CustomerID], (
    SELECT TOP(1) [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID]
) AS [OrderId]
FROM [Customers] AS [c]");
        }

        public override void Project_single_entity_value_subquery_works()
        {
            base.Project_single_entity_value_subquery_works();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_CustomerID='ALFKI' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='ANATR' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='ANTON' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]",
                //
                @"@_outer_CustomerID='AROUT' (Size = 5)

SELECT TOP(1) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
WHERE @_outer_CustomerID = [o].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override void Project_single_scalar_value_subquery_in_query_with_optional_navigation_works()
        {
            base.Project_single_scalar_value_subquery_in_query_with_optional_navigation_works();

            AssertSql(
                @"@__p_0='3'

SELECT TOP(@__p_0) [o].[OrderID], (
    SELECT TOP(1) [od].[OrderID]
    FROM [Order Details] AS [od]
    WHERE [o].[OrderID] = [od].[OrderID]
    ORDER BY [od].[OrderID], [od].[ProductID]
) AS [OrderDetail], [o.Customer].[City]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
ORDER BY [o].[OrderID]");
        }

        public override void GroupJoin_with_complex_subquery_and_LOJ_gets_flattened()
        {
            base.GroupJoin_with_complex_subquery_and_LOJ_gets_flattened();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [c2].*
    FROM [Order Details] AS [od]
    INNER JOIN [Orders] AS [o] ON [od].[OrderID] = 10260
    INNER JOIN [Customers] AS [c2] ON [o].[CustomerID] = [c2].[CustomerID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override void GroupJoin_with_complex_subquery_and_LOJ_gets_flattened2()
        {
            base.GroupJoin_with_complex_subquery_and_LOJ_gets_flattened2();

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [c2].*
    FROM [Order Details] AS [od]
    INNER JOIN [Orders] AS [o] ON [od].[OrderID] = 10260
    INNER JOIN [Customers] AS [c2] ON [o].[CustomerID] = [c2].[CustomerID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override void Navigation_with_collection_with_nullable_type_key()
        {
            base.Navigation_with_collection_with_nullable_type_key();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [o.Customer] ON [o].[CustomerID] = [o.Customer].[CustomerID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [oo]
    WHERE ([oo].[OrderID] > 10260) AND ([o.Customer].[CustomerID] = [oo].[CustomerID])
) > 30");
        }

        public override void Client_groupjoin_with_orderby_key_descending()
        {
            base.Client_groupjoin_with_orderby_key_descending();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID] DESC");
        }

        public override void Navigation_projection_on_groupjoin_qsre()
        {
            base.Navigation_projection_on_groupjoin_qsre();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_OrderID='10643'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10692'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10702'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10835'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10952'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='11011'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]");
        }

        public override void Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result()
        {
            base.Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_OrderID='10643'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10692'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10702'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10835'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10952'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='11011'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]");
        }

        public override void Navigation_projection_on_groupjoin_qsre_with_empty_grouping()
        {
            base.Navigation_projection_on_groupjoin_qsre_with_empty_grouping();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [oo].[OrderID], [oo].[CustomerID], [oo].[EmployeeID], [oo].[OrderDate]
    FROM [Orders] AS [oo]
    WHERE [oo].[OrderID] NOT IN (10308, 10625, 10759, 10926)
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_OrderID='10643'

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]",
                //
                @"@_outer_OrderID='10692'

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]",
                //
                @"@_outer_OrderID='10702'

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]",
                //
                @"@_outer_OrderID='10835'

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]",
                //
                @"@_outer_OrderID='10952'

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]",
                //
                @"@_outer_OrderID='11011'

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]",
                //
                @"@_outer_OrderID='10365'

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]",
                //
                @"@_outer_OrderID='10507'

SELECT [o].[OrderID], [o].[ProductID], [o].[Discount], [o].[Quantity], [o].[UnitPrice]
FROM [Order Details] AS [o]
WHERE @_outer_OrderID = [o].[OrderID]");
        }

        public override void Include_on_inner_projecting_groupjoin()
        {
            base.Include_on_inner_projecting_groupjoin();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_OrderID='10643'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10692'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10702'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10835'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10952'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='11011'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
FROM [Order Details] AS [o0]
WHERE @_outer_OrderID = [o0].[OrderID]");
        }

        public override void Include_on_inner_projecting_groupjoin_complex()
        {
            base.Include_on_inner_projecting_groupjoin_complex();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID]",
                //
                @"@_outer_OrderID='10643'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o0]
INNER JOIN [Products] AS [o.Product] ON [o0].[ProductID] = [o.Product].[ProductID]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10692'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o0]
INNER JOIN [Products] AS [o.Product] ON [o0].[ProductID] = [o.Product].[ProductID]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10702'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o0]
INNER JOIN [Products] AS [o.Product] ON [o0].[ProductID] = [o.Product].[ProductID]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10835'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o0]
INNER JOIN [Products] AS [o.Product] ON [o0].[ProductID] = [o.Product].[ProductID]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='10952'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o0]
INNER JOIN [Products] AS [o.Product] ON [o0].[ProductID] = [o.Product].[ProductID]
WHERE @_outer_OrderID = [o0].[OrderID]",
                //
                @"@_outer_OrderID='11011'

SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice], [o.Product].[ProductID], [o.Product].[Discontinued], [o.Product].[ProductName], [o.Product].[SupplierID], [o.Product].[UnitPrice], [o.Product].[UnitsInStock]
FROM [Order Details] AS [o0]
INNER JOIN [Products] AS [o.Product] ON [o0].[ProductID] = [o.Product].[ProductID]
WHERE @_outer_OrderID = [o0].[OrderID]");
        }

        public override void Group_join_doesnt_get_bound_directly_to_group_join_qsre()
        {
            base.Group_join_doesnt_get_bound_directly_to_group_join_qsre();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        private void AssertContainsSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, assertOrder: false);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
