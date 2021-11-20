// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindKeylessEntitiesQuerySqlServerTest : NorthwindKeylessEntitiesQueryRelationalTestBase<
        NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
    {
        public NorthwindKeylessEntitiesQuerySqlServerTest(
            NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
            ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        protected override bool CanExecuteQueryString
            => true;

        [ConditionalTheory]
        public override async Task KeylessEntity_simple(bool async)
        {
            await base.KeylessEntity_simple(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]");
        }

        [ConditionalTheory]
        public override async Task KeylessEntity_where_simple(bool async)
        {
            await base.KeylessEntity_where_simple(async);

            AssertSql(
                @"SELECT [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
WHERE [m].[City] = N'London'");
        }

        public override void KeylessEntity_by_database_view()
        {
            base.KeylessEntity_by_database_view();

            AssertSql(
                @"SELECT [a].[CategoryName], [a].[ProductID], [a].[ProductName]
FROM [Alphabetical list of products] AS [a]");
        }

        [ConditionalFact(Skip = "Issue#21627")]
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

        public override async Task KeylessEntity_with_mixed_tracking(bool async)
        {
            await base.KeylessEntity_with_mixed_tracking(async);

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [m].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN (
    select * from ""Orders""
) AS [m] ON [c].[CustomerID] = [m].[CustomerID]");
        }

        public override async Task KeylessEntity_with_defining_query(bool async)
        {
            await base.KeylessEntity_with_defining_query(async);

            AssertSql(
                @"SELECT [m].[CustomerID]
FROM (
    select * from ""Orders""
) AS [m]
WHERE [m].[CustomerID] = N'ALFKI'");
        }

        public override async Task KeylessEntity_select_where_navigation(bool async)
        {
            await base.KeylessEntity_select_where_navigation(async);

            AssertSql(
                @"SELECT [m].[CustomerID]
FROM (
    select * from ""Orders""
) AS [m]
LEFT JOIN [Customers] AS [c] ON [m].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Seattle'");
        }

        public override async Task KeylessEntity_select_where_navigation_multi_level(bool async)
        {
            await base.KeylessEntity_select_where_navigation_multi_level(async);

            AssertSql(
                @"SELECT [m].[CustomerID]
FROM (
    select * from ""Orders""
) AS [m]
LEFT JOIN [Customers] AS [c] ON [m].[CustomerID] = [c].[CustomerID]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] IS NOT NULL AND [c].[CustomerID] = [o].[CustomerID])");
        }

        [ConditionalFact]
        public override void Auto_initialized_view_set()
        {
            base.Auto_initialized_view_set();

            AssertSql(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]");
        }

        public override async Task KeylessEntity_groupby(bool async)
        {
            await base.KeylessEntity_groupby(async);

            AssertSql(
                @"SELECT [m].[City] AS [Key], COUNT(*) AS [Count], COALESCE(SUM(CAST(LEN([m].[Address]) AS int)), 0) AS [Sum]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
GROUP BY [m].[City]");
        }

        public override void Entity_mapped_to_view_on_right_side_of_join()
        {
            base.Entity_mapped_to_view_on_right_side_of_join();

            AssertSql(
                @"SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [a].[CategoryName], [a].[ProductID], [a].[ProductName]
FROM [Orders] AS [o]
LEFT JOIN [Alphabetical list of products] AS [a] ON [o].[CustomerID] = [a].[CategoryName]");
        }

        public override async Task Collection_correlated_with_keyless_entity_in_predicate_works(bool async)
        {
            await base.Collection_correlated_with_keyless_entity_in_predicate_works(async);

            AssertSql(
                @"@__p_0='2'

SELECT TOP(@__p_0) [m].[City], [m].[ContactName]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE [c].[City] = [m].[City] OR ([c].[City] IS NULL AND [m].[City] IS NULL))
ORDER BY [m].[ContactName]");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
