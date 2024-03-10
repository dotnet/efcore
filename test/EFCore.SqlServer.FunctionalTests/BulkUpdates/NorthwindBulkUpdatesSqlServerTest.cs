// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class NorthwindBulkUpdatesSqlServerTest(
    NorthwindBulkUpdatesSqlServerFixture<NoopModelCustomizer> fixture,
    ITestOutputHelper testOutputHelper) : NorthwindBulkUpdatesTestBase<NorthwindBulkUpdatesSqlServerFixture<NoopModelCustomizer>>(fixture, testOutputHelper)
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
@__quantity_0='1' (Nullable = true) (DbType = Int16)

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[Quantity] = @__quantity_0
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
@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY [o0].[OrderID]
        OFFSET @__p_0 ROWS
    ) AS [o1]
    WHERE [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_OrderBy_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Take(async);

        AssertSql(
            """
@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT TOP(@__p_0) [o0].[OrderID], [o0].[ProductID]
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
@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY [o0].[OrderID]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
    ) AS [o1]
    WHERE [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_Skip(bool async)
    {
        await base.Delete_Where_Skip(async);

        AssertSql(
            """
@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY (SELECT 1)
        OFFSET @__p_0 ROWS
    ) AS [o1]
    WHERE [o1].[OrderID] = [o].[OrderID] AND [o1].[ProductID] = [o].[ProductID])
""");
    }

    public override async Task Delete_Where_Take(bool async)
    {
        await base.Delete_Where_Take(async);

        AssertSql(
            """
@__p_0='100'

DELETE TOP(@__p_0) FROM [o]
FROM [Order Details] AS [o]
WHERE [o].[OrderID] < 10300
""");
    }

    public override async Task Delete_Where_Skip_Take(bool async)
    {
        await base.Delete_Where_Skip_Take(async);

        AssertSql(
            """
@__p_0='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [o0].[OrderID], [o0].[ProductID]
        FROM [Order Details] AS [o0]
        WHERE [o0].[OrderID] < 10300
        ORDER BY (SELECT 1)
        OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
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
@__p_0='100'
@__p_1='20'
@__p_2='5'

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
            OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
        ) AS [o0]
        ORDER BY (SELECT 1)
        OFFSET @__p_1 ROWS FETCH NEXT @__p_2 ROWS ONLY
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
@__p_0='0'
@__p_1='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
INNER JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [o1] ON [o].[OrderID] = [o1].[OrderID]
""");
    }

    public override async Task Delete_with_left_join(bool async)
    {
        await base.Delete_with_left_join(async);

        AssertSql(
            """
@__p_0='0'
@__p_1='100'

DELETE FROM [o]
FROM [Order Details] AS [o]
LEFT JOIN (
    SELECT [o0].[OrderID]
    FROM [Orders] AS [o0]
    WHERE [o0].[OrderID] < 10300
    ORDER BY [o0].[OrderID]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
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

    public override async Task Update_Where_set_constant_TagWith(bool async)
    {
        await base.Update_Where_set_constant_TagWith(async);

        AssertExecuteUpdateSql(
            """
-- MyUpdate

UPDATE [c]
SET [c].[ContactName] = N'Updated'
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_constant(bool async)
    {
        await base.Update_Where_set_constant(async);

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
@__customer_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

UPDATE [c]
SET [c].[ContactName] = N'Updated'
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customer_0
""",
            //
            """
@__customer_0='ALFKI' (Size = 5) (DbType = StringFixedLength)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = @__customer_0
""",
            //
            """
SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE 0 = 1
""",
            //
            """
UPDATE [c]
SET [c].[ContactName] = N'Updated'
FROM [Customers] AS [c]
WHERE 0 = 1
""");
    }

    public override async Task Update_Where_set_parameter(bool async)
    {
        await base.Update_Where_set_parameter(async);

        AssertExecuteUpdateSql(
            """
@__value_0='Abc' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @__value_0
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_closure_array(bool async)
    {
        await base.Update_Where_set_parameter_from_closure_array(async);

        AssertExecuteUpdateSql(
            """
@__p_0='Abc' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @__p_0
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_inline_list(bool async)
    {
        await base.Update_Where_set_parameter_from_inline_list(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ContactName] = N'Abc'
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_multilevel_property_access(bool async)
    {
        await base.Update_Where_set_parameter_from_multilevel_property_access(async);

        AssertExecuteUpdateSql(
            """
@__container_Containee_Property_0='Abc' (Size = 30)

UPDATE [c]
SET [c].[ContactName] = @__container_Containee_Property_0
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_Skip_set_constant(bool async)
    {
        await base.Update_Where_Skip_set_constant(async);

        AssertExecuteUpdateSql(
            """
@__p_0='4'

UPDATE [c0]
SET [c0].[ContactName] = N'Updated'
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_Take_set_constant(bool async)
    {
        await base.Update_Where_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@__p_0='4'

UPDATE TOP(@__p_0) [c]
SET [c].[ContactName] = N'Updated'
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_Where_Skip_Take_set_constant(bool async)
    {
        await base.Update_Where_Skip_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@__p_0='2'
@__p_1='4'

UPDATE [c0]
SET [c0].[ContactName] = N'Updated'
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY (SELECT 1)
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_OrderBy_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_set_constant(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c0]
SET [c0].[ContactName] = N'Updated'
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
@__p_0='4'

UPDATE [c0]
SET [c0].[ContactName] = N'Updated'
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[City]
    OFFSET @__p_0 ROWS
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_OrderBy_Take_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@__p_0='4'

UPDATE [c0]
SET [c0].[ContactName] = N'Updated'
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT TOP(@__p_0) [c].[CustomerID]
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
@__p_0='2'
@__p_1='4'

UPDATE [c0]
SET [c0].[ContactName] = N'Updated'
FROM [Customers] AS [c0]
INNER JOIN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] LIKE N'F%'
    ORDER BY [c].[City]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
) AS [c1] ON [c0].[CustomerID] = [c1].[CustomerID]
""");
    }

    public override async Task Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@__p_0='2'
@__p_1='6'

UPDATE [c1]
SET [c1].[ContactName] = N'Updated'
FROM [Customers] AS [c1]
INNER JOIN (
    SELECT [c0].[CustomerID]
    FROM (
        SELECT [c].[CustomerID], [c].[City]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] LIKE N'F%'
        ORDER BY [c].[City]
        OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
    ) AS [c0]
    ORDER BY [c0].[City]
    OFFSET @__p_0 ROWS FETCH NEXT @__p_0 ROWS ONLY
) AS [c2] ON [c1].[CustomerID] = [c2].[CustomerID]
""");
    }

    public override async Task Update_Where_GroupBy_aggregate_set_constant(bool async)
    {
        await base.Update_Where_GroupBy_aggregate_set_constant(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
UPDATE [c0]
SET [c0].[ContactName] = N'Updated'
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
UPDATE [o]
SET [o].[Quantity] = CAST(1 AS smallint)
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
@__value_0='Abc' (Size = 4000)

UPDATE [c]
SET [c].[ContactName] = COALESCE([c].[ContactName], N'') + @__value_0
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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

    public override async Task Update_with_invalid_lambda_throws(bool async)
    {
        await base.Update_with_invalid_lambda_throws(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_Where_multiple_set(bool async)
    {
        await base.Update_Where_multiple_set(async);

        AssertExecuteUpdateSql(
            """
@__value_0='Abc' (Size = 30)

UPDATE [c]
SET [c].[City] = N'Seattle',
    [c].[ContactName] = @__value_0
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
UPDATE [c1]
SET [c1].[ContactName] = N'Updated'
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
UPDATE [c1]
SET [c1].[ContactName] = N'Updated'
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
UPDATE [c1]
SET [c1].[ContactName] = N'Updated'
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
UPDATE [c1]
SET [c1].[ContactName] = N'Updated'
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_left_join_set_constant(bool async)
    {
        await base.Update_with_left_join_set_constant(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ContactName] = N'Updated'
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o0] ON [c].[CustomerID] = [o0].[CustomerID]
WHERE [c].[CustomerID] LIKE N'F%'
""");
    }

    public override async Task Update_with_cross_join_set_constant(bool async)
    {
        await base.Update_with_cross_join_set_constant(async);

        AssertExecuteUpdateSql(
            """
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
UPDATE [c]
SET [c].[ContactName] = N'Updated'
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
SET [c].[City] = CONVERT(varchar(11), DATEPART(year, (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate] DESC)))
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
SET [c].[City] = CONVERT(varchar(11), DATEPART(year, (
    SELECT TOP(1) [o].[OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderDate] DESC)))
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
UPDATE [o]
SET [o].[Quantity] = CAST(1 AS smallint)
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
