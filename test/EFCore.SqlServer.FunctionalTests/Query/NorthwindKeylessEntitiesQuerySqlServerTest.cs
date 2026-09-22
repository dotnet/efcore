// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindKeylessEntitiesQuerySqlServerTest : NorthwindKeylessEntitiesQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindKeylessEntitiesQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task KeylessEntity_simple(bool async)
    {
        await base.KeylessEntity_simple(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
""");
    }

    public override async Task KeylessEntity_where_simple(bool async)
    {
        await base.KeylessEntity_where_simple(async);

        AssertSql(
            """
SELECT [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
WHERE [m].[City] = N'London'
""");
    }

    public override async Task KeylessEntity_by_database_view(bool async)
    {
        await base.KeylessEntity_by_database_view(async);

        AssertSql(
            """
SELECT [a].[CategoryName], [a].[ProductID], [a].[ProductName]
FROM [Alphabetical list of products] AS [a]
""");
    }

    public override async Task KeylessEntity_with_nav_defining_query(bool async)
    {
        // FromSql mapping. Issue #21627.
        await Assert.ThrowsAsync<SqlException>(() => base.KeylessEntity_with_nav_defining_query(async));

        AssertSql(
            """
SELECT [c].[CompanyName], [c].[OrderCount], [c].[SearchTerm]
FROM [CustomerQueryWithQueryFilter] AS [c]
WHERE [c].[OrderCount] > 0
""");
    }

    public override async Task KeylessEntity_with_mixed_tracking(bool async)
    {
        await base.KeylessEntity_with_mixed_tracking(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [m].[CustomerID]
FROM [Customers] AS [c]
INNER JOIN (
    select * from "Orders"
) AS [m] ON [c].[CustomerID] = [m].[CustomerID]
""");
    }

    public override async Task KeylessEntity_with_defining_query(bool async)
    {
        await base.KeylessEntity_with_defining_query(async);

        AssertSql(
            """
SELECT [m].[CustomerID]
FROM (
    select * from "Orders"
) AS [m]
WHERE [m].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task KeylessEntity_select_where_navigation(bool async)
    {
        await base.KeylessEntity_select_where_navigation(async);

        AssertSql(
            """
SELECT [m].[CustomerID]
FROM (
    select * from "Orders"
) AS [m]
LEFT JOIN [Customers] AS [c] ON [m].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Seattle'
""");
    }

    public override async Task KeylessEntity_select_where_navigation_multi_level(bool async)
    {
        await base.KeylessEntity_select_where_navigation_multi_level(async);

        AssertSql(
            """
SELECT [m].[CustomerID]
FROM (
    select * from "Orders"
) AS [m]
LEFT JOIN [Customers] AS [c] ON [m].[CustomerID] = [c].[CustomerID]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] IS NOT NULL AND [c].[CustomerID] = [o].[CustomerID])
""");
    }

    public override async Task Auto_initialized_view_set(bool async)
    {
        await base.Auto_initialized_view_set(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
""");
    }

    public override async Task KeylessEntity_groupby(bool async)
    {
        await base.KeylessEntity_groupby(async);

        AssertSql(
            """
SELECT [m].[City] AS [Key], COUNT(*) AS [Count], COALESCE(SUM(CAST(LEN([m].[Address]) AS int)), 0) AS [Sum]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
GROUP BY [m].[City]
""");
    }

    public override async Task Entity_mapped_to_view_on_right_side_of_join(bool async)
    {
        await base.Entity_mapped_to_view_on_right_side_of_join(async);

        AssertSql(
            """
SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [a].[CategoryName], [a].[ProductID], [a].[ProductName]
FROM [Orders] AS [o]
LEFT JOIN [Alphabetical list of products] AS [a] ON [o].[CustomerID] = [a].[CategoryName]
""");
    }

    public override async Task Collection_correlated_with_keyless_entity_in_predicate_works(bool async)
    {
        await base.Collection_correlated_with_keyless_entity_in_predicate_works(async);

        AssertSql(
            """
@__p_0='2'

SELECT TOP(@__p_0) [m].[City], [m].[ContactName]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
WHERE EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE [c].[City] = [m].[City] OR ([c].[City] IS NULL AND [m].[City] IS NULL))
ORDER BY [m].[ContactName]
""");
    }

    public override async Task Projecting_collection_correlated_with_keyless_entity_throws(bool async)
    {
        await base.Projecting_collection_correlated_with_keyless_entity_throws(async);

        AssertSql();
    }

    public override async Task Collection_of_entities_projecting_correlated_collection_of_keyless_entities(bool async)
    {
        await base.Collection_of_entities_projecting_correlated_collection_of_keyless_entities(async);

        AssertSql();
    }

    public override async Task KeylessEntity_with_included_navs_multi_level(bool async)
    {
        await base.KeylessEntity_with_included_navs_multi_level(async);

        AssertSql();
    }

    public override async Task KeylessEntity_with_defining_query_and_correlated_collection(bool async)
    {
        await base.KeylessEntity_with_defining_query_and_correlated_collection(async);

        AssertSql();
    }

    public override async Task KeylessEntity_with_included_nav(bool async)
    {
        await base.KeylessEntity_with_included_nav(async);

        AssertSql(
            """
SELECT [m].[CustomerID], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM (
    select * from "Orders"
) AS [m]
LEFT JOIN [Customers] AS [c] ON [m].[CustomerID] = [c].[CustomerID]
WHERE [m].[CustomerID] = N'ALFKI'
""");
    }

    public override async Task Count_over_keyless_entity(bool async)
    {
        await base.Count_over_keyless_entity(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
) AS [m]
""");
    }

    public override async Task Count_over_keyless_entity_with_pushdown(bool async)
    {
        await base.Count_over_keyless_entity_with_pushdown(async);

        AssertSql(
            """
@__p_0='10'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
    ) AS [m]
    ORDER BY [m].[ContactTitle]
) AS [m0]
""");
    }

    public override async Task Count_over_keyless_entity_with_pushdown_empty_projection(bool async)
    {
        await base.Count_over_keyless_entity_with_pushdown_empty_projection(async);

        AssertSql(
            """
@__p_0='10'

SELECT COUNT(*)
FROM (
    SELECT TOP(@__p_0) 1 AS empty
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region] FROM [Customers] AS [c]
    ) AS [m]
) AS [m0]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
