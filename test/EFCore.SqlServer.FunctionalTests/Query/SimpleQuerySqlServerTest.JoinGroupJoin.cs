// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override async Task Join_customers_orders_projection(bool isAsync)
        {
            await base.Join_customers_orders_projection(isAsync);

            AssertSql(
                @"SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task Join_customers_orders_entities(bool isAsync)
        {
            await base.Join_customers_orders_entities(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task Join_select_many(bool isAsync)
        {
            await base.Join_select_many(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
CROSS JOIN [Employees] AS [e]");
        }

        public override async Task Client_Join_select_many(bool isAsync)
        {
            await base.Client_Join_select_many(isAsync);

            AssertContainsSql(
                @"@__p_0='2'

SELECT [t0].[EmployeeID], [t0].[City], [t0].[Country], [t0].[FirstName], [t0].[ReportsTo], [t0].[Title]
FROM (
    SELECT TOP(@__p_0) [e0].[EmployeeID], [e0].[City], [e0].[Country], [e0].[FirstName], [e0].[ReportsTo], [e0].[Title]
    FROM [Employees] AS [e0]
    ORDER BY [e0].[EmployeeID]
) AS [t0]",
                //
                @"@__p_0='2'

SELECT [t].[EmployeeID], [t].[City], [t].[Country], [t].[FirstName], [t].[ReportsTo], [t].[Title]
FROM (
    SELECT TOP(@__p_0) [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[EmployeeID]
) AS [t]",
                //
                @"SELECT [t1].[EmployeeID], [t1].[City], [t1].[Country], [t1].[FirstName], [t1].[ReportsTo], [t1].[Title]
FROM (
    SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
    FROM [Employees] AS [e1]
    ORDER BY [e1].[EmployeeID]
    OFFSET 6 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t1]",
                //
                @"SELECT [t1].[EmployeeID], [t1].[City], [t1].[Country], [t1].[FirstName], [t1].[ReportsTo], [t1].[Title]
FROM (
    SELECT [e1].[EmployeeID], [e1].[City], [e1].[Country], [e1].[FirstName], [e1].[ReportsTo], [e1].[Title]
    FROM [Employees] AS [e1]
    ORDER BY [e1].[EmployeeID]
    OFFSET 6 ROWS FETCH NEXT 2 ROWS ONLY
) AS [t1]");
        }

        public override async Task Join_customers_orders_select(bool isAsync)
        {
            await base.Join_customers_orders_select(isAsync);

            AssertSql(
                @"SELECT [c].[ContactName], [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task Join_customers_orders_with_subquery(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery(isAsync);

            AssertContainsSql(
                @"SELECT [o20].[CustomerID], [o20].[OrderID]
FROM [Orders] AS [o20]
ORDER BY [o20].[OrderID]",
                //
                @"SELECT [c].[CustomerID], [c].[ContactName]
FROM [Customers] AS [c]");
        }

        public override async Task Join_customers_orders_with_subquery_with_take(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_with_take(isAsync);

            AssertSql(
                @"@__p_0='5'

SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP(@__p_0) [o2].*
    FROM [Orders] AS [o2]
    ORDER BY [o2].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] = N'ALFKI'");
        }

        public override async Task Join_customers_orders_with_subquery_anonymous_property_method(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_anonymous_property_method(isAsync);

            AssertContainsSql(
                @"SELECT [o20].[OrderID], [o20].[CustomerID], [o20].[EmployeeID], [o20].[OrderDate]
FROM [Orders] AS [o20]
ORDER BY [o20].[OrderID]",
                //
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override async Task Join_customers_orders_with_subquery_anonymous_property_method_with_take(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_anonymous_property_method_with_take(isAsync);

            AssertContainsSql(
                @"@__p_0='5'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
    FROM [Orders] AS [o2]
    ORDER BY [o2].[OrderID]
) AS [t]",
                //
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]");
        }

        public override async Task Join_customers_orders_with_subquery_predicate(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_predicate(isAsync);

            AssertContainsSql(
                @"SELECT [o20].[CustomerID], [o20].[OrderID]
FROM [Orders] AS [o20]
WHERE [o20].[OrderID] > 0
ORDER BY [o20].[OrderID]",
                //
                @"SELECT [c].[CustomerID], [c].[ContactName]
FROM [Customers] AS [c]");
        }

        public override async Task Join_customers_orders_with_subquery_predicate_with_take(bool isAsync)
        {
            await base.Join_customers_orders_with_subquery_predicate_with_take(isAsync);

            AssertSql(
                @"@__p_0='5'

SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP(@__p_0) [o2].*
    FROM [Orders] AS [o2]
    WHERE [o2].[OrderID] > 0
    ORDER BY [o2].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]
WHERE [t].[CustomerID] = N'ALFKI'");
        }

        public override async Task Join_composite_key(bool isAsync)
        {
            await base.Join_composite_key(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON ([c].[CustomerID] = [o].[CustomerID]) AND ([c].[CustomerID] = [o].[CustomerID])");
        }

        public override async Task Join_complex_condition(bool isAsync)
        {
            await base.Join_complex_condition(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].*
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10250
) AS [t] ON 1 = 1
WHERE [c].[CustomerID] = N'ALFKI'");
        }

        public override async Task Join_client_new_expression(bool isAsync)
        {
            await base.Join_client_new_expression(isAsync);

            AssertContainsSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]",
                //
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]");
        }

        public override async Task Join_same_collection_multiple(bool isAsync)
        {
            await base.Join_same_collection_multiple(isAsync);

            AssertSql(
                @"SELECT [c3].[CustomerID], [c3].[Address], [c3].[City], [c3].[CompanyName], [c3].[ContactName], [c3].[ContactTitle], [c3].[Country], [c3].[Fax], [c3].[Phone], [c3].[PostalCode], [c3].[Region]
FROM [Customers] AS [o]
INNER JOIN [Customers] AS [c2] ON [o].[CustomerID] = [c2].[CustomerID]
INNER JOIN [Customers] AS [c3] ON [o].[CustomerID] = [c3].[CustomerID]");
        }

        public override async Task Join_same_collection_force_alias_uniquefication(bool isAsync)
        {
            await base.Join_same_collection_force_alias_uniquefication(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[CustomerID] = [o0].[CustomerID]");
        }

        public override async Task GroupJoin_customers_orders_count(bool isAsync)
        {
            await base.GroupJoin_customers_orders_count(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override async Task GroupJoin_customers_orders_count_preserves_ordering(bool isAsync)
        {
            await base.GroupJoin_customers_orders_count_preserves_ordering(isAsync);

            AssertSql(
                @"@__p_0='5'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] NOT IN (N'VAFFE', N'DRACD')
    ORDER BY [c].[City]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]");
        }

        public override async Task GroupJoin_simple(bool isAsync)
        {
            await base.GroupJoin_simple(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task GroupJoin_simple2(bool isAsync)
        {
            await base.GroupJoin_simple2(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task GroupJoin_simple3(bool isAsync)
        {
            await base.GroupJoin_simple3(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task GroupJoin_tracking_groups(bool isAsync)
        {
            await base.GroupJoin_tracking_groups(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override async Task GroupJoin_simple_ordering(bool isAsync)
        {
            await base.GroupJoin_simple_ordering(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[City]");
        }

        public override async Task GroupJoin_simple_subquery(bool isAsync)
        {
            await base.GroupJoin_simple_subquery(isAsync);

            AssertSql(
                @"@__p_0='4'

SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP(@__p_0) [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID]
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override async Task GroupJoin_DefaultIfEmpty(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task GroupJoin_DefaultIfEmpty_multiple(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty_multiple(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate], [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
LEFT JOIN [Orders] AS [o2] ON [c].[CustomerID] = [o2].[CustomerID]");
        }

        public override async Task GroupJoin_DefaultIfEmpty2(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty2(isAsync);

            AssertSql(
                @"SELECT [e].[EmployeeID], [e].[City], [e].[Country], [e].[FirstName], [e].[ReportsTo], [e].[Title], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Employees] AS [e]
LEFT JOIN [Orders] AS [o] ON [e].[EmployeeID] = [o].[EmployeeID]");
        }

        public override async Task GroupJoin_DefaultIfEmpty3(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty3(isAsync);

            AssertSql(
                @"@__p_0='1'

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].*
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task GroupJoin_Where(bool isAsync)
        {
            await base.GroupJoin_Where(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task GroupJoin_Where_OrderBy(bool isAsync)
        {
            await base.GroupJoin_Where_OrderBy(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE ([o].[CustomerID] = N'ALFKI') OR ([c].[CustomerID] = N'ANATR')
ORDER BY [c].[City]");
        }

        public override async Task GroupJoin_DefaultIfEmpty_Where(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty_Where(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[OrderID] IS NOT NULL AND ([o].[CustomerID] = N'ALFKI')");
        }

        public override async Task Join_GroupJoin_DefaultIfEmpty_Where(bool isAsync)
        {
            await base.Join_GroupJoin_DefaultIfEmpty_Where(isAsync);

            AssertSql(
                @"SELECT [o2].[OrderID], [o2].[CustomerID], [o2].[EmployeeID], [o2].[OrderDate]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
LEFT JOIN [Orders] AS [o2] ON [c].[CustomerID] = [o2].[CustomerID]
WHERE [o2].[OrderID] IS NOT NULL AND ([o2].[CustomerID] = N'ALFKI')");
        }

        public override async Task GroupJoin_DefaultIfEmpty_Project(bool isAsync)
        {
            await base.GroupJoin_DefaultIfEmpty_Project(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task GroupJoin_with_different_outer_elements_with_same_key(bool isAsync)
        {
            await base.GroupJoin_with_different_outer_elements_with_same_key(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task GroupJoin_with_different_outer_elements_with_same_key_with_predicate(bool isAsync)
        {
            await base.GroupJoin_with_different_outer_elements_with_same_key_with_predicate(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [o].[OrderID] > 11500");
        }

        public override async Task GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity(bool isAsync)
        {
            await base.GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity(isAsync);

            AssertSql(
                @"SELECT [od].[OrderID], [od].[ProductID], [od].[Discount], [od].[Quantity], [od].[UnitPrice], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [od.Order] ON [od].[OrderID] = [od.Order].[OrderID]
LEFT JOIN [Customers] AS [c] ON [od.Order].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task GroupJoin_SelectMany_subquery_with_filter(bool isAsync)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter(isAsync);

            AssertSql(
                @"SELECT [c].[ContactName], [t].[OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].*
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override async Task GroupJoin_SelectMany_subquery_with_filter_orderby(bool isAsync)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_orderby(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override async Task GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(bool isAsync)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT [t].[OrderID], [t].[CustomerID], [t].[EmployeeID], [t].[OrderDate], [c].[ContactName]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] > 5
) AS [t] ON [c].[CustomerID] = [t].[CustomerID]");
        }

        public override async Task GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(bool isAsync)
        {
            await base.GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]");
        }

        public override async Task GroupJoin_with_order_by_key_descending1(bool isAsync)
        {
            await base.GroupJoin_with_order_by_key_descending1(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID] DESC");
        }

        public override async Task GroupJoin_with_order_by_key_descending2(bool isAsync)
        {
            await base.GroupJoin_with_order_by_key_descending2(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'A' + N'%' AND (LEFT([c].[CustomerID], LEN(N'A')) = N'A')
ORDER BY [c].[CustomerID] DESC");
        }
    }
}
