// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class NorthwindBulkUpdatesSqlServerTest(
    NorthwindBulkUpdatesSqlServerFixture<NoopModelCustomizer> fixture,
    ITestOutputHelper testOutputHelper)
    : NorthwindBulkUpdatesRelationalTestBase<NorthwindBulkUpdatesSqlServerFixture<NoopModelCustomizer>>(fixture, testOutputHelper)
{
    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Delete_Where_TagWith(bool async)
    {
        await base.Delete_Where_TagWith(async);

        AssertSql(
            """
-- MyDelete

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Delete_Where(bool async)
    {
        await base.Delete_Where(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Delete_Where_parameter(bool async)
    {
        await base.Delete_Where_parameter(async);

        AssertSql(
            """
@quantity='1' (Nullable = true) (DbType = Int16)

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[Quantity] = @quantity
""",
            //
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE 0 = 1
""");
    }

    public override async Task Delete_Where_OrderBy(bool async)
    {
        await base.Delete_Where_OrderBy(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Order Details] AS [o0]
    WHERE [o0].[OrderID] < 10300 AND [o0].[OrderID] = [o].[OrderID] AND [o0].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_OrderBy_Skip(bool async)
    {
        await base.Delete_Where_OrderBy_Skip(async);

        AssertSql(
            """
@p='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY [o0].[OrderID]
        OFFSET @p ROWS
    ) AS [o1]
    WHERE [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_OrderBy_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Take(async);

        AssertSql(
            """
@p='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(@p) [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY [o0].[OrderID]
    ) AS [o1]
    WHERE [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_OrderBy_Skip_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Skip_Take(async);

        AssertSql(
            """
@p='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY [o0].[OrderID]
        OFFSET @p ROWS FETCH NEXT @p ROWS ONLY
    ) AS [o1]
    WHERE [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_Skip(bool async)
    {
        await base.Delete_Where_Skip(async);

        AssertSql(
            """
@p='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY (SELECT 1)
        OFFSET @p ROWS
    ) AS [o1]
    WHERE [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_Take(bool async)
    {
        await base.Delete_Where_Take(async);

        AssertSql(
            """
@p='100'

DELETE TOP(@p) FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Delete_Where_Skip_Take(bool async)
    {
        await base.Delete_Where_Skip_Take(async);

        AssertSql(
            """
@p='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY (SELECT 1)
        OFFSET @p ROWS FETCH NEXT @p ROWS ONLY
    ) AS [o1]
    WHERE [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_predicate_with_GroupBy_aggregate(bool async)
    {
        await base.Delete_Where_predicate_with_GroupBy_aggregate(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < (
    SELECT TOP(1) (
        SELECT TOP(1) [o1].[OrderID]
        FROM [Orders] AS [o1]
        WHERE [o0].[CustomerID] = [o1].[CustomerID] OR ([o0].[CustomerID] IS NULL AND [o1].[CustomerID] IS NULL))
    FROM [Orders] AS [o0]
    GROUP BY [o0].[CustomerID]
    HAVING COUNT(*) > 11)
""");
    }

    public override async Task Delete_Where_predicate_with_GroupBy_aggregate_2(bool async)
    {
        await base.Delete_Where_predicate_with_GroupBy_aggregate_2(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o0].[OrderID] IN (
    SELECT (
        SELECT TOP(1) [o2].[OrderID]
        FROM [Orders] AS [o2]
        WHERE [o1].[CustomerID] = [o2].[CustomerID] OR ([o1].[CustomerID] IS NULL AND [o2].[CustomerID] IS NULL))
    FROM [Orders] AS [o1]
    GROUP BY [o1].[CustomerID]
    HAVING COUNT(*) > 9
)
""");
    }

    public override async Task Delete_GroupBy_Where_Select(bool async)
    {
        await base.Delete_GroupBy_Where_Select(async);

        AssertSql();
    }

    public override async Task Delete_GroupBy_Where_Select_2(bool async)
    {
        await base.Delete_GroupBy_Where_Select_2(async);

        AssertSql();
    }

    public override async Task Delete_Where_Skip_Take_Skip_Take_causing_subquery(bool async)
    {
        await base.Delete_Where_Skip_Take_Skip_Take_causing_subquery(async);

        AssertSql(
            """
@p='100'
@p1='20'
@p2='5'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM (
            SELECT [o1].[OrderID], [o1].[ProductID]
            FROM [Order Details] AS [o1]
            WHERE [o1].[OrderID] < 10300
            ORDER BY (SELECT 1)
            OFFSET @p ROWS FETCH NEXT @p ROWS ONLY
        ) AS [o0]
        ORDER BY (SELECT 1)
        OFFSET @p1 ROWS FETCH NEXT @p2 ROWS ONLY
    ) AS [o2]
    WHERE [o2].[OrderID] = [o].[OrderID] AND [o2].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_Distinct(bool async)
    {
        await base.Delete_Where_Distinct(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Delete_SelectMany(bool async)
    {
        await base.Delete_SelectMany(async);

        AssertSql(
            """
DELETE FROM [o0]
FROM [Orders] AS [o]
INNER JOIN [Order Details] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [o].[OrderID] < 10250
""");
    }

    public override async Task Delete_SelectMany_subquery(bool async)
    {
        await base.Delete_SelectMany_subquery(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM [Orders] AS [o0]
    INNER JOIN (
        SELECT [o2].[OrderID], [o2].[ProductID]
        FROM [Order Details] AS [o2]
        WHERE [o2].[ProductID] > 0
    ) AS [o1] ON [o0].[OrderID] = [o1].[OrderID]
    WHERE [o0].[OrderID] < 10250 AND [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_using_navigation(bool async)
    {
        await base.Delete_Where_using_navigation(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE DATEPART(year, [o0].[OrderDate]) = 2000
""");
    }

    public override async Task Delete_Where_using_navigation_2(bool async)
    {
        await base.Delete_Where_using_navigation_2(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Delete_Union(bool async)
    {
        await base.Delete_Union(async);

        AssertSql(
            """
DELETE FROM [o]
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
    ) AS [u]
    WHERE [u].[OrderID] = [o].[OrderID] AND [u].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Concat(bool async)
    {
        await base.Delete_Concat(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10250
        UNION ALL
        SELECT [o1].[OrderID], [o1].[ProductID]
        FROM [Order Details] AS [o1]
        WHERE [o1].[OrderID] > 11250
    ) AS [u]
    WHERE [u].[OrderID] = [o].[OrderID] AND [u].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Intersect(bool async)
    {
        await base.Delete_Intersect(async);

        AssertSql(
            """
DELETE FROM [o]
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
    ) AS [i]
    WHERE [i].[OrderID] = [o].[OrderID] AND [i].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Except(bool async)
    {
        await base.Delete_Except(async);

        AssertSql(
            """
DELETE FROM [o]
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
    ) AS [e]
    WHERE [e].[OrderID] = [o].[OrderID] AND [e].[ProductID] = [o].[ProductID])
""");
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
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "OrderID", "ProductID", "UnitPrice", "Quantity", "Discount"
        FROM "Order Details"
        WHERE "OrderID" < 10300
    ) AS [m]
    WHERE [m].[OrderID] = [o].[OrderID] AND [m].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_optional_navigation_predicate(bool async)
    {
        await base.Delete_Where_optional_navigation_predicate(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] LIKE N'Se%'
""");
    }

    public override async Task Delete_with_join(bool async)
    {
        await base.Delete_with_join(async);

        AssertSql(
            """
@p='0'
@p0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET @p ROWS FETCH NEXT @p0 ROWS ONLY
) AS [o1] ON [o].[OrderID] = [o1].[OrderID]
""");
    }

    public override async Task Delete_with_LeftJoin(bool async)
    {
        await base.Delete_with_LeftJoin(async);

        AssertSql(
            """
@p='0'
@p0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
LEFT JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET @p ROWS FETCH NEXT @p0 ROWS ONLY
) AS [o1] ON [o].[OrderID] = [o1].[OrderID]
WHERE [o].[OrderID] < 10276
""");
    }

    public override async Task Delete_with_LeftJoin_via_flattened_GroupJoin(bool async)
    {
        await base.Delete_with_LeftJoin_via_flattened_GroupJoin(async);

        AssertSql(
            """
@p='0'
@p0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
LEFT JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET @p ROWS FETCH NEXT @p0 ROWS ONLY
) AS [o1] ON [o].[OrderID] = [o1].[OrderID]
WHERE [o].[OrderID] < 10276
""");
    }

    public override async Task Delete_with_cross_join(bool async)
    {
        await base.Delete_with_cross_join(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
CROSS JOIN (
    SELECT 1 AS empty
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY
) AS [o1]
WHERE [o].[OrderID] < 10276
""");
    }

    public override async Task Delete_with_cross_apply(bool async)
    {
        await base.Delete_with_cross_apply(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
CROSS APPLY (
    SELECT 1 AS empty
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < [o].[OrderID]
    ORDER BY [o0].[OrderID]
    OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY
) AS [o1]
WHERE [o].[OrderID] < 10276
""");
    }

    public override async Task Delete_with_outer_apply(bool async)
    {
        await base.Delete_with_outer_apply(async);

        AssertSql(
            """
DELETE FROM [o]
FROM [Order Details] AS [o]
OUTER APPLY (
    SELECT 1 AS empty
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < [o].[OrderID]
    ORDER BY [o0].[OrderID]
    OFFSET 0 ROWS FETCH NEXT 100 ROWS ONLY
) AS [o1]
WHERE [o].[OrderID] < 10276
""");
    }

    public override async Task Delete_with_RightJoin(bool async)
    {
        await base.Delete_with_RightJoin(async);

        AssertSql(
            """
@p='0'
@p0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
RIGHT JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET @p ROWS FETCH NEXT @p0 ROWS ONLY
) AS [o1] ON [o].[OrderID] = [o1].[OrderID]
WHERE [o].[OrderID] < 10276
""");
    }

    public override async Task Update_Where_set_constant_TagWith(bool async)
    {
        await base.Update_Where_set_constant_TagWith(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

-- MyUpdate

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_constant(bool async)
    {
        await base.Update_Where_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_constant_via_lambda(bool async)
    {
        await base.Update_Where_set_constant_via_lambda(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ContactName] = N'Updated'
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_parameter_set_constant(bool async)
    {
        await base.Update_Where_parameter_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)
@customer='ALFKI' (Size = 5) (DbType = StringFixedLength)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customer
""",
            //
            """
@customer='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @customer
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""",
            //
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Update_Where_set_parameter(bool async)
    {
        await base.Update_Where_set_parameter(async);

        AssertExecuteUpdateSql(
            """
@p='Abc' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_closure_array(bool async)
    {
        await base.Update_Where_set_parameter_from_closure_array(async);

        AssertExecuteUpdateSql(
            """
@p='Abc' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_inline_list(bool async)
    {
        await base.Update_Where_set_parameter_from_inline_list(async);

        AssertExecuteUpdateSql(
            """
@p='Abc' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_multilevel_property_access(bool async)
    {
        await base.Update_Where_set_parameter_from_multilevel_property_access(async);

        AssertExecuteUpdateSql(
            """
@p='Abc' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_Skip_set_constant(bool async)
    {
        await base.Update_Where_Skip_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p0='Updated' (Size = 30)
@p='4'

UPDATE [c0]
SET [c0].[ContactName] = @p0
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY (SELECT 1)
    OFFSET @p ROWS
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_Take_set_constant(bool async)
    {
        await base.Update_Where_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='4'
@p0='Updated' (Size = 30)

UPDATE TOP(@p) [c]
SET [c].[ContactName] = @p0
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_Skip_Take_set_constant(bool async)
    {
        await base.Update_Where_Skip_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p1='Updated' (Size = 30)
@p='2'
@p0='4'

UPDATE [c0]
SET [c0].[ContactName] = @p1
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY (SELECT 1)
    OFFSET @p ROWS FETCH NEXT @p0 ROWS ONLY
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_OrderBy_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c0]
SET [c0].[ContactName] = @p
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_OrderBy_Skip_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Skip_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p0='Updated' (Size = 30)
@p='4'

UPDATE [c0]
SET [c0].[ContactName] = @p0
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[City]
    OFFSET @p ROWS
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_OrderBy_Take_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p0='Updated' (Size = 30)
@p='4'

UPDATE [c0]
SET [c0].[ContactName] = @p0
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT TOP(@p) [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[City]
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_OrderBy_Skip_Take_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Skip_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p1='Updated' (Size = 30)
@p='2'
@p0='4'

UPDATE [c0]
SET [c0].[ContactName] = @p1
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[City]
    OFFSET @p ROWS FETCH NEXT @p0 ROWS ONLY
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p3='Updated' (Size = 30)
@p='2'
@p0='6'

UPDATE [c1]
SET [c1].[ContactName] = @p3
FROM [Customers] AS [c1]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM (
        SELECT [c].[CustomerID], [c].[City]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] LIKE N'F%'
        ORDER BY [c].[City]
        OFFSET @p ROWS FETCH NEXT @p0 ROWS ONLY
    ) AS [c0]
    ORDER BY [c0].[City]
    OFFSET @p ROWS FETCH NEXT @p ROWS ONLY
) AS [c2] ON [c1].[CustomerID] = [c2].[CustomerID]
""");
    }

    public override async Task Update_Where_GroupBy_aggregate_set_constant(bool async)
    {
        await base.Update_Where_GroupBy_aggregate_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = (
    SELECT TOP(1) [o].[CustomerID]
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 11)
""");
    }

    public override async Task Update_Where_GroupBy_First_set_constant(bool async)
    {
        await base.Update_Where_GroupBy_First_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = (
    SELECT TOP(1) (
        SELECT TOP(1) [o0].[CustomerID]
        FROM [Orders] AS [o0]
        WHERE [o].[CustomerID] = [o0].[CustomerID] OR ([o].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 11)
""");
    }

    public override async Task Update_Where_GroupBy_First_set_constant_2(bool async)
    {
        await base.Update_Where_GroupBy_First_set_constant_2(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_Where_GroupBy_First_set_constant_3(bool async)
    {
        await base.Update_Where_GroupBy_First_set_constant_3(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT (
        SELECT TOP(1) [c0].[CustomerID]
        FROM [Orders] AS [o0]
        LEFT JOIN [Customers] AS [c0] ON [o0].[CustomerID] = [c0].[CustomerID]
        WHERE [o].[CustomerID] = [o0].[CustomerID] OR ([o].[CustomerID] IS NULL AND [o0].[CustomerID] IS NULL))
    FROM [Orders] AS [o]
    GROUP BY [o].[CustomerID]
    HAVING COUNT(*) > 11
)
""");
    }

    public override async Task Update_Where_Distinct_set_constant(bool async)
    {
        await base.Update_Where_Distinct_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c0]
SET [c0].[ContactName] = @p
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT DISTINCT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_using_navigation_set_null(bool async)
    {
        await base.Update_Where_using_navigation_set_null(async);

        AssertExecuteUpdateSql(
            """
UPDATE [o]
SET [o].[OrderDate] = NULL
FROM [Orders] AS [o]
LEFT JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Seattle'
""");
    }

    public override async Task Update_Where_using_navigation_2_set_constant(bool async)
    {
        await base.Update_Where_using_navigation_2_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='1'

UPDATE [o]
SET [o].[Quantity] = CAST(@p AS smallint)
FROM [Order Details] AS [o]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
LEFT JOIN [Customers] AS [c] ON [o0].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Seattle'
""");
    }

    public override async Task Update_Where_SelectMany_set_null(bool async)
    {
        await base.Update_Where_SelectMany_set_null(async);

        AssertExecuteUpdateSql(
            """
UPDATE [o]
SET [o].[OrderDate] = NULL
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_property_plus_constant(bool async)
    {
        await base.Update_Where_set_property_plus_constant(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ContactName] = COALESCE([c].[ContactName], N'') + N'Abc'
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_property_plus_parameter(bool async)
    {
        await base.Update_Where_set_property_plus_parameter(async);

        AssertExecuteUpdateSql(
            """
@value='Abc' (Size = 4000)

UPDATE [c]
SET [c].[ContactName] = COALESCE([c].[ContactName], N'') + @value
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_property_plus_property(bool async)
    {
        await base.Update_Where_set_property_plus_property(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ContactName] = COALESCE([c].[ContactName], N'') + [c].[CustomerID]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_constant_using_ef_property(bool async)
    {
        await base.Update_Where_set_constant_using_ef_property(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_null(bool async)
    {
        await base.Update_Where_set_null(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ContactName] = NULL
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_without_property_to_set_throws(bool async)
    {
        await base.Update_without_property_to_set_throws(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_Where_multiple_set(bool async)
    {
        await base.Update_Where_multiple_set(async);

        AssertExecuteUpdateSql(
            """
@value='Abc' (Size = 30)
@p='Seattle' (Size = 15)

UPDATE [c]
SET [c].[ContactName] = @value,
    [c].[City] = @p
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_invalid_lambda_in_set_property_throws(bool async)
    {
        await base.Update_with_invalid_lambda_in_set_property_throws(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_multiple_tables_throws(bool async)
    {
        await base.Update_multiple_tables_throws(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_unmapped_property_throws(bool async)
    {
        await base.Update_unmapped_property_throws(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_Union_set_constant(bool async)
    {
        await base.Update_Union_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c1]
SET [c1].[ContactName] = @p
FROM [Customers] AS [c1]
INNER JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    UNION
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A%'
) AS [u] ON [c1].[CustomerID] = [u].[CustomerID]
""");
    }

    public override async Task Update_Concat_set_constant(bool async)
    {
        await base.Update_Concat_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c1]
SET [c1].[ContactName] = @p
FROM [Customers] AS [c1]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    UNION ALL
    SELECT [c0].[CustomerID]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A%'
) AS [u] ON [c1].[CustomerID] = [u].[CustomerID]
""");
    }

    public override async Task Update_Except_set_constant(bool async)
    {
        await base.Update_Except_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c1]
SET [c1].[ContactName] = @p
FROM [Customers] AS [c1]
INNER JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    EXCEPT
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A%'
) AS [e] ON [c1].[CustomerID] = [e].[CustomerID]
""");
    }

    public override async Task Update_Intersect_set_constant(bool async)
    {
        await base.Update_Intersect_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c1]
SET [c1].[ContactName] = @p
FROM [Customers] AS [c1]
INNER JOIN (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    INTERSECT
    SELECT [c0].[CustomerID], [c0].[Address], [c0].[City], [c0].[CompanyName], [c0].[ContactName], [c0].[ContactTitle], [c0].[Country], [c0].[Fax], [c0].[Phone], [c0].[PostalCode], [c0].[Region]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] LIKE N'A%'
) AS [i] ON [c1].[CustomerID] = [i].[CustomerID]
""");
    }

    public override async Task Update_with_join_set_constant(bool async)
    {
        await base.Update_with_join_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_LeftJoin(bool async)
    {
        await base.Update_with_LeftJoin(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_LeftJoin_via_flattened_GroupJoin(bool async)
    {
        await base.Update_with_LeftJoin_via_flattened_GroupJoin(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_RightJoin(bool async)
    {
        await base.Update_with_RightJoin(async);

        AssertExecuteUpdateSql(
            """
@p='2020-01-01T00:00:00.0000000Z' (Nullable = true) (DbType = DateTime)

UPDATE [o]
SET [o].[OrderDate] = @p
FROM [Orders] AS [o]
RIGHT JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
) AS [c0] ON [o].[CustomerID] = [c0].[CustomerID]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Update_with_cross_join_set_constant(bool async)
    {
        await base.Update_with_cross_join_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_cross_apply_set_constant(bool async)
    {
        await base.Update_with_cross_apply_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300 AND DATEPART(year, [o].[OrderDate]) < CAST(LEN([c].[ContactName]) AS int)
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_outer_apply_set_constant(bool async)
    {
        await base.Update_with_outer_apply_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
OUTER APPLY (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300 AND DATEPART(year, [o].[OrderDate]) < CAST(LEN([c].[ContactName]) AS int)
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_cross_join_left_join_set_constant(bool async)
    {
        await base.Update_with_cross_join_left_join_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT 1 AS empty
    FROM [Customers] AS [c0]
    WHERE [c0].[City] LIKE N'S%'
) AS [c1]
LEFT JOIN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_cross_join_cross_apply_set_constant(bool async)
    {
        await base.Update_with_cross_join_cross_apply_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT 1 AS empty
    FROM [Customers] AS [c0]
    WHERE [c0].[City] LIKE N'S%'
) AS [c1]
CROSS APPLY (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300 AND DATEPART(year, [o].[OrderDate]) < CAST(LEN([c].[ContactName]) AS int)
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_cross_join_outer_apply_set_constant(bool async)
    {
        await base.Update_with_cross_join_outer_apply_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @p
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT 1 AS empty
    FROM [Customers] AS [c0]
    WHERE [c0].[City] LIKE N'S%'
) AS [c1]
OUTER APPLY (
    SELECT 1 AS empty
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300 AND DATEPART(year, [o].[OrderDate]) < CAST(LEN([c].[ContactName]) AS int)
) AS [o0]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_FromSql_set_constant(bool async)
    {
        await base.Update_FromSql_set_constant(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_Where_SelectMany_subquery_set_null(bool async)
    {
        await base.Update_Where_SelectMany_subquery_set_null(async);

        AssertExecuteUpdateSql(
            """
UPDATE [o1]
SET [o1].[OrderDate] = NULL
FROM [Orders] AS [o1]
INNER JOIN (
    SELECT [o0].[OrderID]
    FROM [Customers] AS [c]
    INNER JOIN (
        SELECT [o].[OrderID], [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE DATEPART(year, [o].[OrderDate]) = 1997
    ) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
    WHERE [c].[CustomerID] LIKE N'F%'
) AS [s] ON [o1].[OrderID] = [s].[OrderID]
""");
    }

    public override async Task Update_Where_Join_set_property_from_joined_single_result_table(bool async)
    {
        await base.Update_Where_Join_set_property_from_joined_single_result_table(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[City] = COALESCE(CONVERT(varchar(11), DATEPART(year, (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate] DESC))), '')
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_Join_set_property_from_joined_table(bool async)
    {
        await base.Update_Where_Join_set_property_from_joined_table(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[City] = [c1].[City]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT [c0].[City]
    FROM [Customers] AS [c0]
    WHERE [c0].[CustomerID] = N'ALFKI'
) AS [c1]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_Join_set_property_from_joined_single_result_scalar(bool async)
    {
        await base.Update_Where_Join_set_property_from_joined_single_result_scalar(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[City] = COALESCE(CONVERT(varchar(11), DATEPART(year, (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate] DESC))), '')
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Update_with_two_inner_joins(bool async)
    {
        await base.Update_with_two_inner_joins(async);

        AssertExecuteUpdateSql(
            """
@p='1'

UPDATE [o]
SET [o].[Quantity] = CAST(@p AS smallint)
FROM [Order Details] AS [o]
INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
INNER JOIN [Orders] AS [o0] ON [o].[OrderID] = [o0].[OrderID]
WHERE [p].[Discontinued] = CAST(1 AS bit) AND [o0].[OrderDate] > '1990-01-01T00:00:00.000'
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
