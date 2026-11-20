// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindQueryFiltersQuerySqlServerTest : NorthwindQueryFiltersQueryTestBase<
    NorthwindQuerySqlServerFixture<NorthwindQueryFiltersCustomizer>>
{
    public NorthwindQueryFiltersQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NorthwindQueryFiltersCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Count_query(bool async)
    {
        await base.Count_query(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
""");
    }

    public override async Task Materialized_query(bool async)
    {
        await base.Materialized_query(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
""");
    }

    public override async Task Find(bool async)
    {
        await base.Find(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)
@p='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT TOP(1) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\' AND [c].[CustomerID] = @p
""");
    }

    public override async Task Materialized_query_parameter(bool async)
    {
        await base.Materialized_query_parameter(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='F%' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
""");
    }

    public override async Task Materialized_query_parameter_new_context(bool async)
    {
        await base.Materialized_query_parameter_new_context(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
""",
            //
            """
@ef_filter__TenantPrefix_startswith='T%' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
""");
    }

    public override async Task Projection_query_parameter(bool async)
    {
        await base.Projection_query_parameter(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='F%' (Size = 40)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
""");
    }

    public override async Task Projection_query(bool async)
    {
        await base.Projection_query(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
""");
    }

    public override async Task Include_query(bool async)
    {
        await base.Include_query(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [s].[OrderID], [s].[CustomerID], [s].[EmployeeID], [s].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
    FROM [Orders] AS [o]
    LEFT JOIN (
        SELECT [c0].[CustomerID], [c0].[CompanyName]
        FROM [Customers] AS [c0]
        WHERE [c0].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
    ) AS [c1] ON [o].[CustomerID] = [c1].[CustomerID]
    WHERE [c1].[CustomerID] IS NOT NULL AND [c1].[CompanyName] IS NOT NULL
) AS [s] ON [c].[CustomerID] = [s].[CustomerID]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Include_query_opt_out(bool async)
    {
        await base.Include_query_opt_out(async);

        AssertSql(
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Customers] AS [c]
LEFT JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [c].[CustomerID]
""");
    }

    public override async Task Included_many_to_one_query(bool async)
    {
        await base.Included_many_to_one_query(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
""");
    }

    public override async Task Project_reference_that_itself_has_query_filter_with_another_reference(bool async)
    {
        await base.Project_reference_that_itself_has_query_filter_with_another_reference(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)
@ef_filter___quantity='50'

SELECT [s].[OrderID], [s].[CustomerID], [s].[EmployeeID], [s].[OrderDate]
FROM [Order Details] AS [o]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    LEFT JOIN (
        SELECT [c].[CustomerID], [c].[CompanyName]
        FROM [Customers] AS [c]
        WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
    ) AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
    WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
) AS [s] ON [o].[OrderID] = [s].[OrderID]
WHERE [o].[Quantity] > @ef_filter___quantity
""");
    }

    public override async Task Navs_query(bool async)
    {
        await base.Navs_query(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)
@ef_filter___quantity='50'

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[OrderID], [o].[CustomerID]
    FROM [Orders] AS [o]
    LEFT JOIN (
        SELECT [c0].[CustomerID], [c0].[CompanyName]
        FROM [Customers] AS [c0]
        WHERE [c0].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
    ) AS [c1] ON [o].[CustomerID] = [c1].[CustomerID]
    WHERE [c1].[CustomerID] IS NOT NULL AND [c1].[CompanyName] IS NOT NULL
) AS [s] ON [c].[CustomerID] = [s].[CustomerID]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[Discount]
    FROM [Order Details] AS [o0]
    INNER JOIN (
        SELECT [o1].[OrderID]
        FROM [Orders] AS [o1]
        LEFT JOIN (
            SELECT [c2].[CustomerID], [c2].[CompanyName]
            FROM [Customers] AS [c2]
            WHERE [c2].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
        ) AS [c3] ON [o1].[CustomerID] = [c3].[CustomerID]
        WHERE [c3].[CustomerID] IS NOT NULL AND [c3].[CompanyName] IS NOT NULL
    ) AS [s0] ON [o0].[OrderID] = [s0].[OrderID]
    WHERE [o0].[Quantity] > @ef_filter___quantity
) AS [s1] ON [s].[OrderID] = [s1].[OrderID]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\' AND [s1].[Discount] < CAST(10 AS real)
""");
    }

    [Fact]
    public void FromSql_is_composed()
    {
        using (var context = Fixture.CreateContext())
        {
            var results = context.Customers.FromSqlRaw("select * from Customers").ToList();

            Assert.Equal(7, results.Count);
        }

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [m].[CustomerID], [m].[Address], [m].[City], [m].[CompanyName], [m].[ContactName], [m].[ContactTitle], [m].[Country], [m].[Fax], [m].[Phone], [m].[PostalCode], [m].[Region]
FROM (
    select * from Customers
) AS [m]
WHERE [m].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
""");
    }

    [Fact]
    public void FromSql_is_composed_when_filter_has_navigation()
    {
        using (var context = Fixture.CreateContext())
        {
            var results = context.Orders.FromSqlRaw("select * from Orders").ToList();

            Assert.Equal(80, results.Count);
        }

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [m].[OrderID], [m].[CustomerID], [m].[EmployeeID], [m].[OrderDate]
FROM (
    select * from Orders
) AS [m]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [m].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
""");
    }

    public override void Compiled_query()
    {
        base.Compiled_query();

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)
@customerID='BERGS' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\' AND [c].[CustomerID] = @customerID
""",
            //
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)
@customerID='BLAUS' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\' AND [c].[CustomerID] = @customerID
""");
    }

    public override async Task Entity_Equality(bool async)
    {
        await base.Entity_Equality(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
""");
    }

    public override async Task Client_eval(bool async)
    {
        await base.Client_eval(async);

        AssertSql();
    }

    public override async Task GroupBy_aggregate_through_filtered_navigation(bool async)
    {
        await base.GroupBy_aggregate_through_filtered_navigation(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [o].[EmployeeID] AS [Key], COUNT(CASE
    WHEN [c0].[City] = N'London' THEN 1
END) AS [Londons]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_aggregate_through_filtered_navigation_with_total(bool async)
    {
        await base.GroupBy_aggregate_through_filtered_navigation_with_total(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [o].[EmployeeID] AS [Key], COUNT(*) AS [Total], COUNT(CASE
    WHEN [c0].[City] = N'London' THEN 1
END) AS [Londons]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_aggregate_through_filtered_navigation_ignore_query_filters(bool async)
    {
        await base.GroupBy_aggregate_through_filtered_navigation_ignore_query_filters(async);

        AssertSql(
            """
SELECT [o].[EmployeeID] AS [Key], COUNT(CASE
    WHEN [c].[City] = N'London' THEN 1
END) AS [Londons]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Any_through_filtered_navigation(bool async)
    {
        await base.GroupBy_Any_through_filtered_navigation(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [o].[EmployeeID] AS [Key], CASE
    WHEN COUNT_BIG(CASE
        WHEN [c0].[City] = N'London' THEN 1
    END) > CAST(0 AS bigint) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Londons]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Any_through_filtered_navigation_with_total(bool async)
    {
        await base.GroupBy_Any_through_filtered_navigation_with_total(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [o].[EmployeeID] AS [Key], COUNT(*) AS [Total], CASE
    WHEN COUNT_BIG(CASE
        WHEN [c0].[City] = N'London' THEN 1
    END) > CAST(0 AS bigint) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Londons]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task GroupBy_Any_through_filtered_navigation_ignore_query_filters(bool async)
    {
        await base.GroupBy_Any_through_filtered_navigation_ignore_query_filters(async);

        AssertSql(
            """
SELECT [o].[EmployeeID] AS [Key], CASE
    WHEN COUNT_BIG(CASE
        WHEN [c].[City] = N'London' THEN 1
    END) > CAST(0 AS bigint) THEN CAST(1 AS bit)
    ELSE CAST(0 AS bit)
END AS [Londons]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[EmployeeID]
""");
    }

    public override async Task Included_many_to_one_query2(bool async)
    {
        await base.Included_many_to_one_query2(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
""");
    }

    public override async Task Included_one_to_many_query_with_client_eval(bool async)
    {
        await base.Included_one_to_many_query_with_client_eval(async);

        AssertSql();
    }

    public override async Task GroupBy_lifted_aggregate_with_subquery_over_filtered_entity(bool async)
    {
        await base.GroupBy_lifted_aggregate_with_subquery_over_filtered_entity(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c0].[City], MAX([c0].[CompanyName]) AS [LastCompany], (
    SELECT COUNT(*)
    FROM [Customers] AS [c1]
    WHERE [c1].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\' AND ([c1].[City] = [c0].[City] OR ([c1].[City] IS NULL AND [c0].[City] IS NULL))) AS [Customers]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [c0].[City]
""");
    }

    public override async Task GroupBy_lifted_aggregate_with_subquery_using_navigation(bool async)
    {
        await base.GroupBy_lifted_aggregate_with_subquery_using_navigation(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c0].[City], MAX([c0].[CompanyName]) AS [LastCompany], (
    SELECT COUNT(*)
    FROM [Customers] AS [c1]
    WHERE [c1].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\' AND ([c1].[City] = [c0].[City] OR ([c1].[City] IS NULL AND [c0].[City] IS NULL)) AND EXISTS (
        SELECT 1
        FROM [Orders] AS [o0]
        LEFT JOIN (
            SELECT [c2].[CustomerID], [c2].[CompanyName]
            FROM [Customers] AS [c2]
            WHERE [c2].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
        ) AS [c3] ON [o0].[CustomerID] = [c3].[CustomerID]
        WHERE [c3].[CustomerID] IS NOT NULL AND [c3].[CompanyName] IS NOT NULL AND [c1].[CustomerID] = [o0].[CustomerID])) AS [Buyers]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [c0].[City]
""");
    }

    public override async Task GroupBy_lifted_aggregate_with_subquery_over_grouped_entity(bool async)
    {
        await base.GroupBy_lifted_aggregate_with_subquery_over_grouped_entity(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c0].[City], MAX([c0].[CompanyName]) AS [LastCompany], (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    LEFT JOIN (
        SELECT [c1].[CustomerID], [c1].[City], [c1].[CompanyName]
        FROM [Customers] AS [c1]
        WHERE [c1].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
    ) AS [c2] ON [o0].[CustomerID] = [c2].[CustomerID]
    WHERE [c2].[CustomerID] IS NOT NULL AND [c2].[CompanyName] IS NOT NULL AND ([c2].[City] = [c0].[City] OR ([c2].[City] IS NULL AND [c0].[City] IS NULL))) AS [Orders]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [c0].[City]
""");
    }

    public override async Task GroupBy_lifted_aggregate_inside_subquery_over_filtered_entity(bool async)
    {
        await base.GroupBy_lifted_aggregate_inside_subquery_over_filtered_entity(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c0].[City], (
    SELECT COUNT(*)
    FROM [Customers] AS [c1]
    WHERE [c1].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\' AND ([c1].[City] = [c0].[City] OR ([c1].[City] IS NULL AND [c0].[City] IS NULL)) AND ([c1].[CompanyName] <> MAX([c0].[CompanyName]) OR MAX([c0].[CompanyName]) IS NULL)) AS [OtherCustomers]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [c0].[City]
""");
    }

    public override async Task GroupBy_lifted_aggregate_with_entity_key_and_subquery(bool async)
    {
        await base.GroupBy_lifted_aggregate_with_entity_key_and_subquery(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [c0].[CustomerID], MAX([c0].[City]) AS [City], (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    LEFT JOIN (
        SELECT [c1].[CustomerID], [c1].[CompanyName]
        FROM [Customers] AS [c1]
        WHERE [c1].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
    ) AS [c2] ON [o0].[CustomerID] = [c2].[CustomerID]
    WHERE [c2].[CustomerID] IS NOT NULL AND [c2].[CompanyName] IS NOT NULL AND [c2].[CustomerID] = [c0].[CustomerID]) AS [Orders], (
    SELECT COUNT(*)
    FROM [Customers] AS [c3]
    WHERE [c3].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\' AND ([c3].[Country] = [c0].[Country] OR ([c3].[Country] IS NULL AND [c0].[Country] IS NULL))) AS [Compatriots]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
GROUP BY [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
""");
    }

    public override async Task GroupBy_lifted_aggregate_with_subquery_projecting_entity_and_collection(bool async)
    {
        await base.GroupBy_lifted_aggregate_with_subquery_projecting_entity_and_collection(async);

        AssertSql(
            """
@ef_filter__TenantPrefix_startswith='B%' (Size = 40)

SELECT [s].[City], [s].[c], [c5].[CustomerID], [c4].[CustomerID], [c4].[Address], [c4].[City], [c4].[CompanyName], [c4].[ContactName], [c4].[ContactTitle], [c4].[Country], [c4].[Fax], [c4].[Phone], [c4].[PostalCode], [c4].[Region]
FROM (
    SELECT [c0].[City], MAX([c0].[CompanyName]) AS [c]
    FROM [Orders] AS [o]
    LEFT JOIN (
        SELECT [c].[CustomerID], [c].[City], [c].[CompanyName]
        FROM [Customers] AS [c]
        WHERE [c].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
    ) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
    WHERE [c0].[CustomerID] IS NOT NULL AND [c0].[CompanyName] IS NOT NULL
    GROUP BY [c0].[City]
) AS [s]
LEFT JOIN (
    SELECT [c3].[CustomerID], [c3].[Address], [c3].[City], [c3].[CompanyName], [c3].[ContactName], [c3].[ContactTitle], [c3].[Country], [c3].[Fax], [c3].[Phone], [c3].[PostalCode], [c3].[Region]
    FROM (
        SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region], ROW_NUMBER() OVER(PARTITION BY [c1].[City] ORDER BY [c1].[CustomerID]) AS [row]
        FROM [Customers] AS [c1]
        WHERE [c1].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
    ) AS [c3]
    WHERE [c3].[row] <= 1
) AS [c4] ON [s].[City] = [c4].[City] OR ([s].[City] IS NULL AND [c4].[City] IS NULL)
LEFT JOIN (
    SELECT [c2].[CustomerID], [c2].[City]
    FROM [Customers] AS [c2]
    WHERE [c2].[CompanyName] LIKE @ef_filter__TenantPrefix_startswith ESCAPE N'\'
) AS [c5] ON [s].[City] = [c5].[City] OR ([s].[City] IS NULL AND [c5].[City] IS NULL)
ORDER BY [s].[City]
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
