// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class NorthwindBulkUpdatesSqlServerTest : NorthwindBulkUpdatesTestBase<NorthwindBulkUpdatesSqlServerFixture<NoopModelCustomizer>>
{
    public NorthwindBulkUpdatesSqlServerTest(NorthwindBulkUpdatesSqlServerFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_Where(bool async)
    {
        await base.Delete_Where(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10300");
    }

    public override async Task Delete_Where_parameter(bool async)
    {
        await base.Delete_Where_parameter(async);

        AssertSql(
            @"@__quantity_0='1' (Nullable = true) (DbType = Int16)

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[Quantity] = @__quantity_0",
                //
                @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE 0 = 1");
    }

    public override async Task Delete_Where_OrderBy(bool async)
    {
        await base.Delete_Where_OrderBy(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Order Details] AS [o0]
    WHERE [o0].[OrderID] < 10300 AND [o0].[OrderID] = [o].[OrderID] AND [o0].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Where_OrderBy_Skip(bool async)
    {
        await base.Delete_Where_OrderBy_Skip(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY [o0].[OrderID]
        OFFSET @__p_0 ROWS
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Where_OrderBy_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(@__p_0) [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY [o0].[OrderID]
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Where_OrderBy_Skip_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Skip_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY [o0].[OrderID]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Where_Skip(bool async)
    {
        await base.Delete_Where_Skip(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY (SELECT 1)
        OFFSET @__p_0 ROWS
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Where_Take(bool async)
    {
        await base.Delete_Where_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE TOP(@__p_0) FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10300");
    }

    public override async Task Delete_Where_Skip_Take(bool async)
    {
        await base.Delete_Where_Skip_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY (SELECT 1)
        OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Where_predicate_with_group_by_aggregate(bool async)
    {
        await base.Delete_Where_predicate_with_group_by_aggregate(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < (
    SELECT TOP(1) (
        SELECT TOP(1) [o1].[OrderID]
        FROM [Orders] AS [o1]
        WHERE [o0].[CustomerID] = [o1].[CustomerID] OR ([o0].[CustomerID] IS NULL AND [o1].[CustomerID] IS NULL))
    FROM [Orders] AS [o0]
    GROUP BY [o0].[CustomerID]
    HAVING COUNT(*) > 11)");
    }

    public override async Task Delete_Where_predicate_with_group_by_aggregate_2(bool async)
    {
        await base.Delete_Where_predicate_with_group_by_aggregate_2(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select(bool async)
    {
        await base.Delete_GroupBy_Where_Select(async);

        AssertSql();
    }

    public override async Task Delete_Where_Skip_Take_Skip_Take_causing_subquery(bool async)
    {
        await base.Delete_Where_Skip_Take_Skip_Take_causing_subquery(async);

        AssertSql(
            @"@__p_0='100'
@__p_1='20'
@__p_2='5'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [t].[OrderID], [t].[ProductID], [t].[Discount], [t].[Quantity], [t].[UnitPrice]
        FROM (
            SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
            FROM [Order Details] AS [o0]
            WHERE [o0].[OrderID] < 10300
            ORDER BY (SELECT 1)
            OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
        ) AS [t]
        ORDER BY (SELECT 1)
        OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY
    ) AS [t0]
    WHERE [t0].[OrderID] = [o].[OrderID] AND [t0].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Where_Distinct(bool async)
    {
        await base.Delete_Where_Distinct(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10300");
    }

    public override async Task Delete_SelectMany(bool async)
    {
        await base.Delete_SelectMany(async);

        AssertSql(
            @"DELETE FROM [o0]
FROM [Orders] AS [o]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[OrderID] < 10250");
    }

    public override async Task Delete_SelectMany_subquery(bool async)
    {
        await base.Delete_SelectMany_subquery(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    INNER JOIN (
        SELECT [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
        FROM [Order Details] AS [o1]
        WHERE [o1].[ProductID] > 0
    ) AS [t] ON [o0].[OrderID] = [t].[OrderID]
    WHERE [o0].[OrderID] < 10250 AND [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Where_using_navigation(bool async)
    {
        await base.Delete_Where_using_navigation(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE DATEPART(year, [o0].[OrderDate]) = 2000");
    }

    public override async Task Delete_Where_using_navigation_2(bool async)
    {
        await base.Delete_Where_using_navigation_2(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] IS NOT NULL AND ([c].[CustomerID] LIKE N'F%')");
    }

    public override async Task Delete_Union(bool async)
    {
        await base.Delete_Union(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10250
        UNION
        SELECT [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
        FROM [Order Details] AS [o1]
        WHERE [o1].[OrderID] > 11250
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Concat(bool async)
    {
        await base.Delete_Concat(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10250
        UNION ALL
        SELECT [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
        FROM [Order Details] AS [o1]
        WHERE [o1].[OrderID] > 11250
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Intersect(bool async)
    {
        await base.Delete_Intersect(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10250
        INTERSECT
        SELECT [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
        FROM [Order Details] AS [o1]
        WHERE [o1].[OrderID] > 11250
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_Except(bool async)
    {
        await base.Delete_Except(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID], [o0].[Discount], [o0].[Quantity], [o0].[UnitPrice]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10250
        EXCEPT
        SELECT [o1].[OrderID], [o1].[ProductID], [o1].[Discount], [o1].[Quantity], [o1].[UnitPrice]
        FROM [Order Details] AS [o1]
        WHERE [o1].[OrderID] > 11250
    ) AS [t]
    WHERE [t].[OrderID] = [o].[OrderID] AND [t].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_non_entity_projection(bool async)
    {
        await base.Delete_non_entity_projection(async);

        AssertSql();
    }

    public override async Task Delete_non_entity_projection_2(bool async)
    {
        await base.Delete_non_entity_projection_2(async);

        AssertSql();
    }

    public override async Task Delete_non_entity_projection_3(bool async)
    {
        await base.Delete_non_entity_projection_3(async);

        AssertSql();
    }

    public override async Task Delete_FromSql_converted_to_subquery(bool async)
    {
        await base.Delete_FromSql_converted_to_subquery(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""OrderID"", ""ProductID"", ""UnitPrice"", ""Quantity"", ""Discount""
        FROM ""Order Details""
        WHERE ""OrderID"" < 10300
    ) AS [m]
    WHERE [m].[OrderID] = [o].[OrderID] AND [m].[ProductID] = [o].[ProductID])");
    }

    public override async Task Delete_with_join(bool async)
    {
        await base.Delete_with_join(async);

        AssertSql(
            @"@__p_0='0'
@__p_1='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t] ON [o].[OrderID] = [t].[OrderID]");
    }

    public override async Task Delete_with_left_join(bool async)
    {
        await base.Delete_with_left_join(async);

        AssertSql(
            @"@__p_0='0'
@__p_1='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
LEFT JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [t] ON [o].[OrderID] = [t].[OrderID]
WHERE [o].[OrderID] < 10276");
    }

    public override async Task Delete_with_cross_join(bool async)
    {
        await base.Delete_with_cross_join(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
CROSS JOIN (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY
) AS [t]
WHERE [o].[OrderID] < 10276");
    }

    public override async Task Delete_with_cross_apply(bool async)
    {
        await base.Delete_with_cross_apply(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
CROSS APPLY (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < [o].[OrderID]
    ORDER BY [o0].[OrderID]
    OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY
) AS [t]
WHERE [o].[OrderID] < 10276");
    }

    public override async Task Delete_with_outer_apply(bool async)
    {
        await base.Delete_with_outer_apply(async);

        AssertSql(
            @"DELETE FROM [o]
FROM [Order Details] AS [o]
OUTER APPLY (
    SELECT [o0].[OrderID], [o0].[CustomerID], [o0].[EmployeeID], [o0].[OrderDate]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < [o].[OrderID]
    ORDER BY [o0].[OrderID]
    OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY
) AS [t]
WHERE [o].[OrderID] < 10276");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
