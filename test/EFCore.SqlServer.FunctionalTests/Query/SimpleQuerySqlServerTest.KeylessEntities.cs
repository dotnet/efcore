// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQuerySqlServerTest
    {
        [ConditionalTheory]
        public override async Task KeylessEntity_simple(bool isAsync)
        {
            await base.KeylessEntity_simple(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID] + N'' as [CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]");
        }

        [ConditionalTheory]
        public override async Task KeylessEntity_where_simple(bool isAsync)
        {
            await base.KeylessEntity_where_simple(isAsync);

            AssertSql(
                @"SELECT [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle]
FROM (
    SELECT [c].[CustomerID] + N'' as [CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [c]
WHERE [c].[City] = N'London'");
        }

        public override void KeylessEntity_by_database_view()
        {
            base.KeylessEntity_by_database_view();

            // See issue#17804
            // when we have defining query and ToView, defining query wins
            //            AssertSql(
            //                @"SELECT [a].[CategoryName], [a].[ProductID], [a].[ProductName]
            //FROM [Alphabetical list of products] AS [a]");
            AssertSql(
                @"SELECT [p].[ProductID], [p].[ProductName], N'Food' AS [CategoryName]
FROM [Products] AS [p]
WHERE [p].[Discontinued] <> CAST(1 AS bit)");
        }

        public override void KeylessEntity_with_nav_defining_query()
        {
            base.KeylessEntity_with_nav_defining_query();

            AssertSql(
                @"@__ef_filter___searchTerm_0='A' (Size = 4000)
@__ef_filter___searchTerm_1='A' (Size = 4000)

SELECT [c].[CompanyName], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [OrderCount], @__ef_filter___searchTerm_0 AS [SearchTerm]
FROM [Customers] AS [c]
WHERE ((@__ef_filter___searchTerm_1 = N'') OR ([c].[CompanyName] IS NOT NULL AND (LEFT([c].[CompanyName], LEN(@__ef_filter___searchTerm_1)) = @__ef_filter___searchTerm_1))) AND ((
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) > 0)");
        }

        public override async Task KeylessEntity_with_mixed_tracking(bool isAsync)
        {
            await base.KeylessEntity_with_mixed_tracking(isAsync);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN (
    select * from ""Orders""
) AS [o] ON [c].[CustomerID] = [o].[CustomerID]");
        }

        public override async Task KeylessEntity_with_defining_query(bool isAsync)
        {
            await base.KeylessEntity_with_defining_query(isAsync);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM (
    select * from ""Orders""
) AS [o]
WHERE [o].[CustomerID] = N'ALFKI'");
        }

        public override async Task KeylessEntity_with_defining_query_and_correlated_collection(bool isAsync)
        {
            await base.KeylessEntity_with_defining_query_and_correlated_collection(isAsync);

            AssertSql(
                @"SELECT [o].[OrderID], [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
FROM (
    select * from ""Orders""
) AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
LEFT JOIN [Orders] AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [o].[CustomerID] = N'ALFKI'
ORDER BY [c].[CustomerID], [o].[OrderID], [o0].[OrderID]");
        }

        public override async Task KeylessEntity_select_where_navigation(bool isAsync)
        {
            await base.KeylessEntity_select_where_navigation(isAsync);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM (
    select * from ""Orders""
) AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Seattle'");
        }

        public override async Task KeylessEntity_select_where_navigation_multi_level(bool isAsync)
        {
            await base.KeylessEntity_select_where_navigation_multi_level(isAsync);

            AssertSql(
                @"SELECT [o].[CustomerID]
FROM (
    select * from ""Orders""
) AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    WHERE [c].[CustomerID] IS NOT NULL AND ([c].[CustomerID] = [o0].[CustomerID]))");
        }

        [ConditionalFact]
        public override void Auto_initialized_view_set()
        {
            base.Auto_initialized_view_set();

            AssertSql(
                @"SELECT [c].[CustomerID] + N'' as [CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]");
        }
    }
}
