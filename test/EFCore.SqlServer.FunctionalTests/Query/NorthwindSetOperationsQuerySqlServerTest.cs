// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindSetOperationsQuerySqlServerTest : NorthwindSetOperationsQueryRelationalTestBase<
    NorthwindQuerySqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindSetOperationsQuerySqlServerTest(
        NorthwindQuerySqlServerFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override bool CanExecuteQueryString
        => true;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Union(bool async)
    {
        await base.Union(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'");
    }

    public override async Task Concat(bool async)
    {
        await base.Concat(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
UNION ALL
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'");
    }

    public override async Task Intersect(bool async)
    {
        await base.Intersect(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
INTERSECT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[ContactName] LIKE N'%Thomas%'");
    }

    public override async Task Except(bool async)
    {
        await base.Except(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'London'
EXCEPT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[ContactName] LIKE N'%Thomas%'");
    }

    public override async Task Union_OrderBy_Skip_Take(bool async)
    {
        await base.Union_OrderBy_Skip_Take(async);

        AssertSql(
            @"@__p_0='1'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
ORDER BY [t].[ContactName]
OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY");
    }

    public override async Task Union_Where(bool async)
    {
        await base.Union_Where(async);

        AssertSql(
            @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
WHERE [t].[ContactName] LIKE N'%Thomas%'");
    }

    public override async Task Union_Skip_Take_OrderBy_ThenBy_Where(bool async)
    {
        await base.Union_Skip_Take_OrderBy_ThenBy_Where(async);

        AssertSql(
            @"@__p_0='0'

SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        WHERE [c].[City] = N'Berlin'
        UNION
        SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        WHERE [c0].[City] = N'London'
    ) AS [t]
    ORDER BY [t].[Region], [t].[City]
    OFFSET @__p_0 ROWS
) AS [t0]
WHERE [t0].[ContactName] LIKE N'%Thomas%'
ORDER BY [t0].[Region], [t0].[City]");
    }

    public override async Task Union_Union(bool async)
    {
        await base.Union_Union(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'
UNION
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[City] = N'Mannheim'");
    }

    public override async Task Union_Intersect(bool async)
    {
        await base.Union_Intersect(async);

        AssertSql(
            @"(
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
)
INTERSECT
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[ContactName] LIKE N'%Thomas%'");
    }

    public override async Task Union_Take_Union_Take(bool async)
    {
        await base.Union_Take_Union_Take(async);

        AssertSql(
            @"@__p_0='1'

SELECT [t2].[CustomerID], [t2].[Address], [t2].[City], [t2].[CompanyName], [t2].[ContactName], [t2].[ContactTitle], [t2].[Country], [t2].[Fax], [t2].[Phone], [t2].[PostalCode], [t2].[Region]
FROM (
    SELECT TOP(@__p_0) [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
    FROM (
        SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
        FROM (
            SELECT TOP(@__p_0) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
            FROM (
                SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
                FROM [Customers] AS [c]
                WHERE [c].[City] = N'Berlin'
                UNION
                SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
                FROM [Customers] AS [c0]
                WHERE [c0].[City] = N'London'
            ) AS [t]
            ORDER BY [t].[CustomerID]
        ) AS [t0]
        UNION
        SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
        FROM [Customers] AS [c1]
        WHERE [c1].[City] = N'Mannheim'
    ) AS [t1]
) AS [t2]
ORDER BY [t2].[CustomerID]");
    }

    public override async Task Select_Union(bool async)
    {
        await base.Select_Union(async);

        AssertSql(
            @"SELECT [c].[Address]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
UNION
SELECT [c0].[Address]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'");
    }

    public override async Task Union_Select(bool async)
    {
        await base.Union_Select(async);

        AssertSql(
            @"SELECT [t].[Address]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
WHERE [t].[Address] LIKE N'%Hanover%'");
    }

    public override async Task Union_Select_scalar(bool async)
    {
        await base.Union_Select_scalar(async);

        AssertSql(
            @"SELECT 1
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    EXCEPT
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
) AS [t]");
    }

    public override async Task Union_with_anonymous_type_projection(bool async)
    {
        await base.Union_with_anonymous_type_projection(async);

        AssertSql(
            @"SELECT [t].[CustomerID] AS [Id]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CompanyName] IS NOT NULL AND ([c].[CompanyName] LIKE N'A%')
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CompanyName] IS NOT NULL AND ([c0].[CompanyName] LIKE N'B%')
) AS [t]");
    }

    public override async Task Select_Union_unrelated(bool async)
    {
        await base.Select_Union_unrelated(async);

        AssertSql(
            @"SELECT [t].[ContactName]
FROM (
    SELECT [c].[ContactName]
    FROM [Customers] AS [c]
    UNION
    SELECT [p].[ProductName] AS [ContactName]
    FROM [Products] AS [p]
) AS [t]
WHERE [t].[ContactName] IS NOT NULL AND ([t].[ContactName] LIKE N'C%')
ORDER BY [t].[ContactName]");
    }

    public override async Task Select_Union_different_fields_in_anonymous_with_subquery(bool async)
    {
        await base.Select_Union_different_fields_in_anonymous_with_subquery(async);

        AssertSql(
            @"@__p_0='1'
@__p_1='10'

SELECT [t0].[Foo], [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[Foo], [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
    FROM (
        SELECT [c].[City] AS [Foo], [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        WHERE [c].[City] = N'Berlin'
        UNION
        SELECT [c0].[Region] AS [Foo], [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
        FROM [Customers] AS [c0]
        WHERE [c0].[City] = N'London'
    ) AS [t]
    ORDER BY [t].[Foo]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t0]
WHERE [t0].[Foo] = N'Berlin'
ORDER BY [t0].[Foo]");
    }

    public override async Task Union_Include(bool async)
    {
        await base.Union_Include(async);

        AssertSql(
            @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID]");
    }

    public override async Task Include_Union(bool async)
    {
        await base.Include_Union(async);

        AssertSql(
            @"SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Berlin'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[City] = N'London'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID]");
    }

    public override async Task Select_Except_reference_projection(bool async)
    {
        await base.Select_Except_reference_projection(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
EXCEPT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Orders] AS [o0]
LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
WHERE [o0].[CustomerID] = N'ALFKI'");
    }

    public override async Task SubSelect_Union(bool async)
    {
        await base.SubSelect_Union(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], (
    SELECT COUNT(*)
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]) AS [Orders]
FROM [Customers] AS [c]
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region], (
    SELECT COUNT(*)
    FROM [Orders] AS [o0]
    WHERE [c0].[CustomerID] = [o0].[CustomerID]) AS [Orders]
FROM [Customers] AS [c0]");
    }

    public override async Task GroupBy_Select_Union(bool async)
    {
        await base.GroupBy_Select_Union(async);

        AssertSql(
            @"SELECT [c].[CustomerID], COUNT(*) AS [Count]
FROM [Customers] AS [c]
WHERE [c].[City] = N'Berlin'
GROUP BY [c].[CustomerID]
UNION
SELECT [c0].[CustomerID], COUNT(*) AS [Count]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'London'
GROUP BY [c0].[CustomerID]");
    }

    public override async Task Union_over_columns_with_different_nullability(bool async)
    {
        await base.Union_over_columns_with_different_nullability(async);

        AssertSql(
            @"SELECT N'NonNullableConstant' AS [c]
FROM [Customers] AS [c]
UNION ALL
SELECT NULL AS [c]
FROM [Customers] AS [c0]");
    }

    public override async Task Union_over_column_column(bool async)
    {
        await base.Union_over_column_column(async);

        AssertSql(
            @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
UNION
SELECT [o0].[OrderID]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_column_function(bool async)
    {
        await base.Union_over_column_function(async);

        AssertSql(
            @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
UNION
SELECT COUNT(*) AS [OrderID]
FROM [Orders] AS [o0]
GROUP BY [o0].[OrderID]");
    }

    public override async Task Union_over_column_constant(bool async)
    {
        await base.Union_over_column_constant(async);

        AssertSql(
            @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
UNION
SELECT 8 AS [OrderID]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_column_unary(bool async)
    {
        await base.Union_over_column_unary(async);

        AssertSql(
            @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
UNION
SELECT -[o0].[OrderID] AS [OrderID]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_column_binary(bool async)
    {
        await base.Union_over_column_binary(async);

        AssertSql(
            @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
UNION
SELECT [o0].[OrderID] + 1 AS [OrderID]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_column_scalarsubquery(bool async)
    {
        await base.Union_over_column_scalarsubquery(async);

        AssertSql(
            @"SELECT [o].[OrderID]
FROM [Orders] AS [o]
UNION
SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o0].[OrderID] = [o1].[OrderID]) AS [OrderID]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_function_column(bool async)
    {
        await base.Union_over_function_column(async);

        AssertSql(
            @"SELECT COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]
UNION
SELECT [o0].[OrderID] AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_function_function(bool async)
    {
        await base.Union_over_function_function(async);

        AssertSql(
            @"SELECT COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]
UNION
SELECT COUNT(*) AS [c]
FROM [Orders] AS [o0]
GROUP BY [o0].[OrderID]");
    }

    public override async Task Union_over_function_constant(bool async)
    {
        await base.Union_over_function_constant(async);

        AssertSql(
            @"SELECT COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]
UNION
SELECT 8 AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_function_unary(bool async)
    {
        await base.Union_over_function_unary(async);

        AssertSql(
            @"SELECT COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]
UNION
SELECT -[o0].[OrderID] AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_function_binary(bool async)
    {
        await base.Union_over_function_binary(async);

        AssertSql(
            @"SELECT COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]
UNION
SELECT [o0].[OrderID] + 1 AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_function_scalarsubquery(bool async)
    {
        await base.Union_over_function_scalarsubquery(async);

        AssertSql(
            @"SELECT COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[OrderID]
UNION
SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o0].[OrderID] = [o1].[OrderID]) AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_constant_column(bool async)
    {
        await base.Union_over_constant_column(async);

        AssertSql(
            @"SELECT 8 AS [c]
FROM [Orders] AS [o]
UNION
SELECT [o0].[OrderID] AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_constant_function(bool async)
    {
        await base.Union_over_constant_function(async);

        AssertSql(
            @"SELECT 8 AS [c]
FROM [Orders] AS [o]
UNION
SELECT COUNT(*) AS [c]
FROM [Orders] AS [o0]
GROUP BY [o0].[OrderID]");
    }

    public override async Task Union_over_constant_constant(bool async)
    {
        await base.Union_over_constant_constant(async);

        AssertSql(
            @"SELECT 8 AS [c]
FROM [Orders] AS [o]
UNION
SELECT 8 AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_constant_unary(bool async)
    {
        await base.Union_over_constant_unary(async);

        AssertSql(
            @"SELECT 8 AS [c]
FROM [Orders] AS [o]
UNION
SELECT -[o0].[OrderID] AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_constant_binary(bool async)
    {
        await base.Union_over_constant_binary(async);

        AssertSql(
            @"SELECT 8 AS [c]
FROM [Orders] AS [o]
UNION
SELECT [o0].[OrderID] + 1 AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_constant_scalarsubquery(bool async)
    {
        await base.Union_over_constant_scalarsubquery(async);

        AssertSql(
            @"SELECT 8 AS [c]
FROM [Orders] AS [o]
UNION
SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o0].[OrderID] = [o1].[OrderID]) AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_unary_column(bool async)
    {
        await base.Union_over_unary_column(async);

        AssertSql(
            @"SELECT -[o].[OrderID] AS [c]
FROM [Orders] AS [o]
UNION
SELECT [o0].[OrderID] AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_unary_function(bool async)
    {
        await base.Union_over_unary_function(async);

        AssertSql(
            @"SELECT -[o].[OrderID] AS [c]
FROM [Orders] AS [o]
UNION
SELECT COUNT(*) AS [c]
FROM [Orders] AS [o0]
GROUP BY [o0].[OrderID]");
    }

    public override async Task Union_over_unary_constant(bool async)
    {
        await base.Union_over_unary_constant(async);

        AssertSql(
            @"SELECT -[o].[OrderID] AS [c]
FROM [Orders] AS [o]
UNION
SELECT 8 AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_unary_unary(bool async)
    {
        await base.Union_over_unary_unary(async);

        AssertSql(
            @"SELECT -[o].[OrderID] AS [c]
FROM [Orders] AS [o]
UNION
SELECT -[o0].[OrderID] AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_unary_binary(bool async)
    {
        await base.Union_over_unary_binary(async);

        AssertSql(
            @"SELECT -[o].[OrderID] AS [c]
FROM [Orders] AS [o]
UNION
SELECT [o0].[OrderID] + 1 AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_unary_scalarsubquery(bool async)
    {
        await base.Union_over_unary_scalarsubquery(async);

        AssertSql(
            @"SELECT -[o].[OrderID] AS [c]
FROM [Orders] AS [o]
UNION
SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o0].[OrderID] = [o1].[OrderID]) AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_binary_column(bool async)
    {
        await base.Union_over_binary_column(async);

        AssertSql(
            @"SELECT [o].[OrderID] + 1 AS [c]
FROM [Orders] AS [o]
UNION
SELECT [o0].[OrderID] AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_binary_function(bool async)
    {
        await base.Union_over_binary_function(async);

        AssertSql(
            @"SELECT [o].[OrderID] + 1 AS [c]
FROM [Orders] AS [o]
UNION
SELECT COUNT(*) AS [c]
FROM [Orders] AS [o0]
GROUP BY [o0].[OrderID]");
    }

    public override async Task Union_over_binary_constant(bool async)
    {
        await base.Union_over_binary_constant(async);

        AssertSql(
            @"SELECT [o].[OrderID] + 1 AS [c]
FROM [Orders] AS [o]
UNION
SELECT 8 AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_binary_unary(bool async)
    {
        await base.Union_over_binary_unary(async);

        AssertSql(
            @"SELECT [o].[OrderID] + 1 AS [c]
FROM [Orders] AS [o]
UNION
SELECT -[o0].[OrderID] AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_binary_binary(bool async)
    {
        await base.Union_over_binary_binary(async);

        AssertSql(
            @"SELECT [o].[OrderID] + 1 AS [c]
FROM [Orders] AS [o]
UNION
SELECT [o0].[OrderID] + 1 AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_binary_scalarsubquery(bool async)
    {
        await base.Union_over_binary_scalarsubquery(async);

        AssertSql(
            @"SELECT [o].[OrderID] + 1 AS [c]
FROM [Orders] AS [o]
UNION
SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o1]
    WHERE [o0].[OrderID] = [o1].[OrderID]) AS [c]
FROM [Orders] AS [o0]");
    }

    public override async Task Union_over_scalarsubquery_column(bool async)
    {
        await base.Union_over_scalarsubquery_column(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [c]
FROM [Orders] AS [o]
UNION
SELECT [o1].[OrderID] AS [c]
FROM [Orders] AS [o1]");
    }

    public override async Task Union_over_scalarsubquery_function(bool async)
    {
        await base.Union_over_scalarsubquery_function(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [c]
FROM [Orders] AS [o]
UNION
SELECT COUNT(*) AS [c]
FROM [Orders] AS [o1]
GROUP BY [o1].[OrderID]");
    }

    public override async Task Union_over_scalarsubquery_constant(bool async)
    {
        await base.Union_over_scalarsubquery_constant(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [c]
FROM [Orders] AS [o]
UNION
SELECT 8 AS [c]
FROM [Orders] AS [o1]");
    }

    public override async Task Union_over_scalarsubquery_unary(bool async)
    {
        await base.Union_over_scalarsubquery_unary(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [c]
FROM [Orders] AS [o]
UNION
SELECT -[o1].[OrderID] AS [c]
FROM [Orders] AS [o1]");
    }

    public override async Task Union_over_scalarsubquery_binary(bool async)
    {
        await base.Union_over_scalarsubquery_binary(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [c]
FROM [Orders] AS [o]
UNION
SELECT [o1].[OrderID] + 1 AS [c]
FROM [Orders] AS [o1]");
    }

    public override async Task Union_over_scalarsubquery_scalarsubquery(bool async)
    {
        await base.Union_over_scalarsubquery_scalarsubquery(async);

        AssertSql(
            @"SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o0]
    WHERE [o].[OrderID] = [o0].[OrderID]) AS [c]
FROM [Orders] AS [o]
UNION
SELECT (
    SELECT COUNT(*)
    FROM [Order Details] AS [o2]
    WHERE [o1].[OrderID] = [o2].[OrderID]) AS [c]
FROM [Orders] AS [o1]");
    }

    public override async Task OrderBy_Take_Union(bool async)
    {
        await base.OrderBy_Take_Union(async);

        AssertSql(
            @"@__p_0='1'

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName]
) AS [t]
UNION
SELECT [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
FROM (
    SELECT TOP(@__p_0) [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    ORDER BY [c0].[ContactName]
) AS [t1]");
    }

    public override async Task Collection_projection_after_set_operation(bool async)
    {
        await base.Collection_projection_after_set_operation(async);

        AssertSql(
            @"SELECT [t].[CustomerID], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'Seatte'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'F%'
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[CustomerID]");
    }

    public override async Task Concat_with_one_side_being_GroupBy_aggregate(bool async)
    {
        await base.Concat_with_one_side_being_GroupBy_aggregate(async);

        AssertSql(
            @"SELECT [o].[OrderDate]
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Seatte'
UNION
SELECT MAX([o0].[OrderDate]) AS [OrderDate]
FROM [Orders] AS [o0]
GROUP BY [o0].[CustomerID]");
    }

    public override async Task Union_on_entity_with_correlated_collection(bool async)
    {
        await base.Union_on_entity_with_correlated_collection(async);

        AssertSql(
            @"SELECT [t].[CustomerID], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [c].[City] = N'Seatte'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Orders] AS [o0]
    LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
    WHERE [o0].[OrderID] < 10250
) AS [t]
LEFT JOIN [Orders] AS [o1] ON [t].[CustomerID] = [o1].[CustomerID]
ORDER BY [t].[CustomerID]");
    }

    public override async Task Union_on_entity_plus_other_column_with_correlated_collection(bool async)
    {
        await base.Union_on_entity_plus_other_column_with_correlated_collection(async);

        AssertSql(
            @"SELECT [t].[OrderDate], [t].[CustomerID], [o1].[OrderID], [o1].[CustomerID], [o1].[EmployeeID], [o1].[OrderDate]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], [o].[OrderDate]
    FROM [Orders] AS [o]
    LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [c].[City] = N'Seatte'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
    WHERE [o0].[OrderID] < 10250
) AS [t]
LEFT JOIN [Orders] AS [o1] ON [t].[CustomerID] = [o1].[CustomerID]
ORDER BY [t].[CustomerID], [t].[OrderDate]");
    }

    public override async Task Except_non_entity(bool async)
    {
        await base.Except_non_entity(async);

        AssertSql(
            @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[ContactTitle] = N'Owner'
EXCEPT
SELECT [c0].[CustomerID]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'México D.F.'");
    }

    public override async Task Except_simple_followed_by_projecting_constant(bool async)
    {
        await base.Except_simple_followed_by_projecting_constant(async);

        AssertSql(
            @"SELECT 1
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    EXCEPT
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
) AS [t]");
    }

    public override async Task Except_nested(bool async)
    {
        await base.Except_nested(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactTitle] = N'Owner'
EXCEPT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'México D.F.'
EXCEPT
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[City] = N'Seattle'");
    }

    public override async Task Intersect_non_entity(bool async)
    {
        await base.Intersect_non_entity(async);

        AssertSql(
            @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.'
INTERSECT
SELECT [c0].[CustomerID]
FROM [Customers] AS [c0]
WHERE [c0].[ContactTitle] = N'Owner'");
    }

    public override async Task Intersect_nested(bool async)
    {
        await base.Intersect_nested(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.'
INTERSECT
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[ContactTitle] = N'Owner'
INTERSECT
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[Fax] IS NOT NULL");
    }

    public override async Task Concat_nested(bool async)
    {
        await base.Concat_nested(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.'
UNION ALL
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'Berlin'
UNION ALL
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[City] = N'London'");
    }

    public override async Task Union_nested(bool async)
    {
        await base.Union_nested(async);

        AssertSql(
            @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[ContactTitle] = N'Owner'
UNION
SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'México D.F.'
UNION
SELECT [c1].[CustomerID], [c1].[Address], [c1].[City], [c1].[CompanyName], [c1].[ContactName], [c1].[ContactTitle], [c1].[Country], [c1].[Fax], [c1].[Phone], [c1].[PostalCode], [c1].[Region]
FROM [Customers] AS [c1]
WHERE [c1].[City] = N'London'");
    }

    public override async Task Union_non_entity(bool async)
    {
        await base.Union_non_entity(async);

        AssertSql(
            @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[ContactTitle] = N'Owner'
UNION
SELECT [c0].[CustomerID]
FROM [Customers] AS [c0]
WHERE [c0].[City] = N'México D.F.'");
    }

    public override async Task Concat_non_entity(bool async)
    {
        await base.Concat_non_entity(async);

        AssertSql(
            @"SELECT [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.'
UNION ALL
SELECT [c0].[CustomerID]
FROM [Customers] AS [c0]
WHERE [c0].[ContactTitle] = N'Owner'");
    }

    public override async Task Collection_projection_after_set_operation_fails_if_distinct(bool async)
    {
        await base.Collection_projection_after_set_operation_fails_if_distinct(async);

        AssertSql();
    }

    public override async Task Collection_projection_before_set_operation_fails(bool async)
    {
        await base.Collection_projection_before_set_operation_fails(async);

        AssertSql();
    }

    public override async Task Include_Union_only_on_one_side_throws(bool async)
    {
        await base.Include_Union_only_on_one_side_throws(async);

        AssertSql();
    }

    public override async Task Include_Union_different_includes_throws(bool async)
    {
        await base.Include_Union_different_includes_throws(async);

        AssertSql();
    }

    public override async Task Client_eval_Union_FirstOrDefault(bool async)
    {
        // Client evaluation in projection. Issue #16243.
        Assert.Equal(
            RelationalStrings.SetOperationsNotAllowedAfterClientEvaluation,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_eval_Union_FirstOrDefault(async))).Message);

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
