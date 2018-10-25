// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        public override async Task QueryType_simple(bool isAsync)
        {
            await base.QueryType_simple(isAsync);

            AssertSql(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle]
FROM [Customers] AS [c]");
        }

        public override async Task QueryType_where_simple(bool isAsync)
        {
            await base.QueryType_where_simple(isAsync);

            AssertSql(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'");
        }

        public override void Query_backed_by_database_view()
        {
            base.Query_backed_by_database_view();

            AssertSql(
                @"SELECT [a].[CategoryName], [a].[ProductID], [a].[ProductName]
FROM [Alphabetical list of products] AS [a]");
        }

        public override void QueryType_with_nav_defining_query()
        {
            base.QueryType_with_nav_defining_query();

            AssertSql(
                @"@__ef_filter___searchTerm_0='A' (Size = 4000)
@__ef_filter___searchTerm_1='A' (Size = 4000)

SELECT [t].[CompanyName], [t].[OrderCount], [t].[SearchTerm]
FROM (
    SELECT [c].[CompanyName], (
        SELECT COUNT(*)
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) AS [OrderCount], @__ef_filter___searchTerm_0 AS [SearchTerm]
    FROM [Customers] AS [c]
) AS [t]
WHERE (([t].[CompanyName] LIKE @__ef_filter___searchTerm_1 + N'%' AND (LEFT([t].[CompanyName], LEN(@__ef_filter___searchTerm_1)) = @__ef_filter___searchTerm_1)) OR (@__ef_filter___searchTerm_1 = N'')) AND ([t].[OrderCount] > 0)");
        }

        public override async Task QueryType_with_mixed_tracking(bool isAsync)
        {
            await base.QueryType_with_mixed_tracking(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [t].[CustomerID]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [o].[CustomerID]
    FROM (
        select * from ""Orders""
    ) AS [o]
) AS [t]
WHERE [t].[CustomerID] = [c].[CustomerID]");
        }

        public override async Task QueryType_with_defining_query(bool isAsync)
        {
            await base.QueryType_with_defining_query(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT [o].[CustomerID]
    FROM (
        select * from ""Orders""
    ) AS [o]
) AS [t]
WHERE [t].[CustomerID] = N'ALFKI'");
        }

        public override async Task QueryType_with_defining_query_and_correlated_collection(bool isAsync)
        {
            await base.QueryType_with_defining_query_and_correlated_collection(isAsync);

            AssertSql(
                "");
        }

        public override async Task QueryType_with_included_nav(bool isAsync)
        {
            await base.QueryType_with_included_nav(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID], [ov.Customer].[CustomerID], [ov.Customer].[Address], [ov.Customer].[City], [ov.Customer].[CompanyName], [ov.Customer].[ContactName], [ov.Customer].[ContactTitle], [ov.Customer].[Country], [ov.Customer].[Fax], [ov.Customer].[Phone], [ov.Customer].[PostalCode], [ov.Customer].[Region]
FROM (
    SELECT [o].[CustomerID]
    FROM (
        select * from ""Orders""
    ) AS [o]
) AS [t]
LEFT JOIN [Customers] AS [ov.Customer] ON [t].[CustomerID] = [ov.Customer].[CustomerID]
WHERE [t].[CustomerID] = N'ALFKI'");
        }

        public override async Task QueryType_with_included_navs_multi_level(bool isAsync)
        {
            await base.QueryType_with_included_navs_multi_level(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID], [ov.Customer].[CustomerID], [ov.Customer].[Address], [ov.Customer].[City], [ov.Customer].[CompanyName], [ov.Customer].[ContactName], [ov.Customer].[ContactTitle], [ov.Customer].[Country], [ov.Customer].[Fax], [ov.Customer].[Phone], [ov.Customer].[PostalCode], [ov.Customer].[Region]
FROM (
    SELECT [o].[CustomerID]
    FROM (
        select * from ""Orders""
    ) AS [o]
) AS [t]
LEFT JOIN [Customers] AS [ov.Customer] ON [t].[CustomerID] = [ov.Customer].[CustomerID]
WHERE [t].[CustomerID] = N'ALFKI'
ORDER BY [ov.Customer].[CustomerID]",
                //
                @"SELECT [ov.Customer.Orders].[OrderID], [ov.Customer.Orders].[CustomerID], [ov.Customer.Orders].[EmployeeID], [ov.Customer.Orders].[OrderDate]
FROM [Orders] AS [ov.Customer.Orders]
INNER JOIN (
    SELECT DISTINCT [t0].[CustomerID], [ov.Customer0].[CustomerID] AS [CustomerID0]
    FROM (
        SELECT [o0].[CustomerID]
        FROM (
            select * from ""Orders""
        ) AS [o0]
    ) AS [t0]
    LEFT JOIN [Customers] AS [ov.Customer0] ON [t0].[CustomerID] = [ov.Customer0].[CustomerID]
    WHERE [t0].[CustomerID] = N'ALFKI'
) AS [t1] ON [ov.Customer.Orders].[CustomerID] = [t1].[CustomerID]
ORDER BY [t1].[CustomerID]");
        }

        public override async Task QueryType_select_where_navigation(bool isAsync)
        {
            await base.QueryType_select_where_navigation(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT [o].[CustomerID]
    FROM (
        select * from ""Orders""
    ) AS [o]
) AS [t]
LEFT JOIN [Customers] AS [ov.Customer] ON [t].[CustomerID] = [ov.Customer].[CustomerID]
WHERE [ov.Customer].[City] = N'Seattle'");
        }

        public override async Task QueryType_select_where_navigation_multi_level(bool isAsync)
        {
            await base.QueryType_select_where_navigation_multi_level(isAsync);

            AssertSql(
                @"SELECT [t].[CustomerID]
FROM (
    SELECT [o].[CustomerID]
    FROM (
        select * from ""Orders""
    ) AS [o]
) AS [t]
LEFT JOIN [Customers] AS [ov.Customer] ON [t].[CustomerID] = [ov.Customer].[CustomerID]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    WHERE [ov.Customer].[CustomerID] = [o0].[CustomerID])");
        }
    }
}
