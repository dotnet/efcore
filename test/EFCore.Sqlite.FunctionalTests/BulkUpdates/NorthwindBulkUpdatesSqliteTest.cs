// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

#nullable disable

public class NorthwindBulkUpdatesSqliteTest(
    NorthwindBulkUpdatesSqliteFixture<NoopModelCustomizer> fixture,
    ITestOutputHelper testOutputHelper)
    : NorthwindBulkUpdatesRelationalTestBase<NorthwindBulkUpdatesSqliteFixture<NoopModelCustomizer>>(fixture, testOutputHelper)
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

DELETE FROM "Order Details" AS "o"
WHERE "o"."OrderID" < 10300
""");
    }

    public override async Task Delete_Where(bool async)
    {
        await base.Delete_Where(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE "o"."OrderID" < 10300
""");
    }

    public override async Task Delete_Where_parameter(bool async)
    {
        await base.Delete_Where_parameter(async);

        AssertSql(
            """
@quantity='1' (Nullable = true) (DbType = Int16)

DELETE FROM "Order Details" AS "o"
WHERE "o"."Quantity" = @quantity
""",
            //
            """
DELETE FROM "Order Details" AS "o"
WHERE 0
""");
    }

    public override async Task Delete_Where_OrderBy(bool async)
    {
        await base.Delete_Where_OrderBy(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    WHERE "o0"."OrderID" < 10300 AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_OrderBy_Skip(bool async)
    {
        await base.Delete_Where_OrderBy_Skip(async);

        AssertSql(
            """
@p='100'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10300
        ORDER BY "o0"."OrderID"
        LIMIT -1 OFFSET @p
    ) AS "o1"
    WHERE "o1"."OrderID" = "o"."OrderID" AND "o1"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_OrderBy_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Take(async);

        AssertSql(
            """
@p='100'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10300
        ORDER BY "o0"."OrderID"
        LIMIT @p
    ) AS "o1"
    WHERE "o1"."OrderID" = "o"."OrderID" AND "o1"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_OrderBy_Skip_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Skip_Take(async);

        AssertSql(
            """
@p='100'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10300
        ORDER BY "o0"."OrderID"
        LIMIT @p OFFSET @p
    ) AS "o1"
    WHERE "o1"."OrderID" = "o"."OrderID" AND "o1"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_Skip(bool async)
    {
        await base.Delete_Where_Skip(async);

        AssertSql(
            """
@p='100'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10300
        LIMIT -1 OFFSET @p
    ) AS "o1"
    WHERE "o1"."OrderID" = "o"."OrderID" AND "o1"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_Take(bool async)
    {
        await base.Delete_Where_Take(async);

        AssertSql(
            """
@p='100'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10300
        LIMIT @p
    ) AS "o1"
    WHERE "o1"."OrderID" = "o"."OrderID" AND "o1"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_Skip_Take(bool async)
    {
        await base.Delete_Where_Skip_Take(async);

        AssertSql(
            """
@p='100'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10300
        LIMIT @p OFFSET @p
    ) AS "o1"
    WHERE "o1"."OrderID" = "o"."OrderID" AND "o1"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_predicate_with_GroupBy_aggregate(bool async)
    {
        await base.Delete_Where_predicate_with_GroupBy_aggregate(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE "o"."OrderID" < (
    SELECT (
        SELECT "o1"."OrderID"
        FROM "Orders" AS "o1"
        WHERE "o0"."CustomerID" = "o1"."CustomerID" OR ("o0"."CustomerID" IS NULL AND "o1"."CustomerID" IS NULL)
        LIMIT 1)
    FROM "Orders" AS "o0"
    GROUP BY "o0"."CustomerID"
    HAVING COUNT(*) > 11
    LIMIT 1)
""");
    }

    public override async Task Delete_Where_predicate_with_GroupBy_aggregate_2(bool async)
    {
        await base.Delete_Where_predicate_with_GroupBy_aggregate_2(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    INNER JOIN "Orders" AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    WHERE "o1"."OrderID" IN (
        SELECT (
            SELECT "o3"."OrderID"
            FROM "Orders" AS "o3"
            WHERE "o2"."CustomerID" = "o3"."CustomerID" OR ("o2"."CustomerID" IS NULL AND "o3"."CustomerID" IS NULL)
            LIMIT 1)
        FROM "Orders" AS "o2"
        GROUP BY "o2"."CustomerID"
        HAVING COUNT(*) > 9
    ) AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
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
@p2='5'
@p1='20'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID"
        FROM (
            SELECT "o1"."OrderID", "o1"."ProductID"
            FROM "Order Details" AS "o1"
            WHERE "o1"."OrderID" < 10300
            LIMIT @p OFFSET @p
        ) AS "o0"
        LIMIT @p2 OFFSET @p1
    ) AS "o2"
    WHERE "o2"."OrderID" = "o"."OrderID" AND "o2"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_Distinct(bool async)
    {
        await base.Delete_Where_Distinct(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE "o"."OrderID" < 10300
""");
    }

    public override async Task Delete_SelectMany(bool async)
    {
        await base.Delete_SelectMany(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Orders" AS "o0"
    INNER JOIN "Order Details" AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    WHERE "o0"."OrderID" < 10250 AND "o1"."OrderID" = "o"."OrderID" AND "o1"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_SelectMany_subquery(bool async)
    {
        await base.Delete_SelectMany_subquery(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Orders" AS "o0"
    INNER JOIN (
        SELECT "o2"."OrderID", "o2"."ProductID"
        FROM "Order Details" AS "o2"
        WHERE "o2"."ProductID" > 0
    ) AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    WHERE "o0"."OrderID" < 10250 AND "o1"."OrderID" = "o"."OrderID" AND "o1"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_using_navigation(bool async)
    {
        await base.Delete_Where_using_navigation(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    INNER JOIN "Orders" AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    WHERE CAST(strftime('%Y', "o1"."OrderDate") AS INTEGER) = 2000 AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_using_navigation_2(bool async)
    {
        await base.Delete_Where_using_navigation_2(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    INNER JOIN "Orders" AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    LEFT JOIN "Customers" AS "c" ON "o1"."CustomerID" = "c"."CustomerID"
    WHERE "c"."CustomerID" LIKE 'F%' AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Union(bool async)
    {
        await base.Delete_Union(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID", "o0"."Discount", "o0"."Quantity", "o0"."UnitPrice"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10250
        UNION
        SELECT "o1"."OrderID", "o1"."ProductID", "o1"."Discount", "o1"."Quantity", "o1"."UnitPrice"
        FROM "Order Details" AS "o1"
        WHERE "o1"."OrderID" > 11250
    ) AS "u"
    WHERE "u"."OrderID" = "o"."OrderID" AND "u"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Concat(bool async)
    {
        await base.Delete_Concat(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10250
        UNION ALL
        SELECT "o1"."OrderID", "o1"."ProductID"
        FROM "Order Details" AS "o1"
        WHERE "o1"."OrderID" > 11250
    ) AS "u"
    WHERE "u"."OrderID" = "o"."OrderID" AND "u"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Intersect(bool async)
    {
        await base.Delete_Intersect(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID", "o0"."Discount", "o0"."Quantity", "o0"."UnitPrice"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10250
        INTERSECT
        SELECT "o1"."OrderID", "o1"."ProductID", "o1"."Discount", "o1"."Quantity", "o1"."UnitPrice"
        FROM "Order Details" AS "o1"
        WHERE "o1"."OrderID" > 11250
    ) AS "i"
    WHERE "i"."OrderID" = "o"."OrderID" AND "i"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Except(bool async)
    {
        await base.Delete_Except(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "o0"."OrderID", "o0"."ProductID", "o0"."Discount", "o0"."Quantity", "o0"."UnitPrice"
        FROM "Order Details" AS "o0"
        WHERE "o0"."OrderID" < 10250
        EXCEPT
        SELECT "o1"."OrderID", "o1"."ProductID", "o1"."Discount", "o1"."Quantity", "o1"."UnitPrice"
        FROM "Order Details" AS "o1"
        WHERE "o1"."OrderID" > 11250
    ) AS "e"
    WHERE "e"."OrderID" = "o"."OrderID" AND "e"."ProductID" = "o"."ProductID")
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
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT "OrderID", "ProductID", "UnitPrice", "Quantity", "Discount"
        FROM "Order Details"
        WHERE "OrderID" < 10300
    ) AS "m"
    WHERE "m"."OrderID" = "o"."OrderID" AND "m"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_Where_optional_navigation_predicate(bool async)
    {
        await base.Delete_Where_optional_navigation_predicate(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    INNER JOIN "Orders" AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    LEFT JOIN "Customers" AS "c" ON "o1"."CustomerID" = "c"."CustomerID"
    WHERE "c"."City" LIKE 'Se%' AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_with_join(bool async)
    {
        await base.Delete_with_join(async);

        AssertSql(
            """
@p0='100'
@p='0'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    INNER JOIN (
        SELECT "o2"."OrderID"
        FROM "Orders" AS "o2"
        WHERE "o2"."OrderID" < 10300
        ORDER BY "o2"."OrderID"
        LIMIT @p0 OFFSET @p
    ) AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    WHERE "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_with_LeftJoin(bool async)
    {
        await base.Delete_with_LeftJoin(async);

        AssertSql(
            """
@p0='100'
@p='0'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    LEFT JOIN (
        SELECT "o2"."OrderID"
        FROM "Orders" AS "o2"
        WHERE "o2"."OrderID" < 10300
        ORDER BY "o2"."OrderID"
        LIMIT @p0 OFFSET @p
    ) AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    WHERE "o0"."OrderID" < 10276 AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_with_LeftJoin_via_flattened_GroupJoin(bool async)
    {
        await base.Delete_with_LeftJoin_via_flattened_GroupJoin(async);

        AssertSql(
            """
@p0='100'
@p='0'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    LEFT JOIN (
        SELECT "o2"."OrderID"
        FROM "Orders" AS "o2"
        WHERE "o2"."OrderID" < 10300
        ORDER BY "o2"."OrderID"
        LIMIT @p0 OFFSET @p
    ) AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    WHERE "o0"."OrderID" < 10276 AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_with_cross_join(bool async)
    {
        await base.Delete_with_cross_join(async);

        AssertSql(
            """
DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    CROSS JOIN (
        SELECT 1
        FROM "Orders" AS "o2"
        WHERE "o2"."OrderID" < 10300
        ORDER BY "o2"."OrderID"
        LIMIT 100 OFFSET 0
    ) AS "o1"
    WHERE "o0"."OrderID" < 10276 AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Delete_with_cross_apply(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Delete_with_cross_apply(async))).Message);

    public override async Task Delete_with_outer_apply(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Delete_with_outer_apply(async))).Message);

    public override async Task Delete_with_RightJoin(bool async)
    {
        await base.Delete_with_RightJoin(async);

        AssertSql(
            """
@p0='100'
@p='0'

DELETE FROM "Order Details" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Order Details" AS "o0"
    RIGHT JOIN (
        SELECT "o2"."OrderID"
        FROM "Orders" AS "o2"
        WHERE "o2"."OrderID" < 10300
        ORDER BY "o2"."OrderID"
        LIMIT @p0 OFFSET @p
    ) AS "o1" ON "o0"."OrderID" = "o1"."OrderID"
    WHERE "o0"."OrderID" < 10276 AND "o0"."OrderID" = "o"."OrderID" AND "o0"."ProductID" = "o"."ProductID")
""");
    }

    public override async Task Update_Where_set_constant_TagWith(bool async)
    {
        await base.Update_Where_set_constant_TagWith(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

-- MyUpdate

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_constant(bool async)
    {
        await base.Update_Where_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_constant_via_lambda(bool async)
    {
        await base.Update_Where_set_constant_via_lambda(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customers" AS "c"
SET "ContactName" = 'Updated'
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_parameter_set_constant(bool async)
    {
        await base.Update_Where_parameter_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)
@customer='ALFKI' (Size = 5)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" = @customer
""",
            //
            """
@customer='ALFKI' (Size = 5)

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."CustomerID" = @customer
""",
            //
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE 0
""",
            //
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE 0
""");
    }

    public override async Task Update_Where_set_parameter(bool async)
    {
        await base.Update_Where_set_parameter(async);

        AssertExecuteUpdateSql(
            """
@p='Abc' (Size = 3)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_closure_array(bool async)
    {
        await base.Update_Where_set_parameter_from_closure_array(async);

        AssertExecuteUpdateSql(
            """
@p='Abc' (Size = 3)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_inline_list(bool async)
    {
        await base.Update_Where_set_parameter_from_inline_list(async);

        AssertExecuteUpdateSql(
            """
@p='Abc' (Size = 3)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_parameter_from_multilevel_property_access(bool async)
    {
        await base.Update_Where_set_parameter_from_multilevel_property_access(async);

        AssertExecuteUpdateSql(
            """
@p='Abc' (Size = 3)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_Skip_set_constant(bool async)
    {
        await base.Update_Where_Skip_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p0='Updated' (Size = 7)
@p='4'

UPDATE "Customers" AS "c0"
SET "ContactName" = @p0
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    LIMIT -1 OFFSET @p
) AS "c1"
WHERE "c0"."CustomerID" = "c1"."CustomerID"
""");
    }

    public override async Task Update_Where_Take_set_constant(bool async)
    {
        await base.Update_Where_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p0='Updated' (Size = 7)
@p='4'

UPDATE "Customers" AS "c0"
SET "ContactName" = @p0
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    LIMIT @p
) AS "c1"
WHERE "c0"."CustomerID" = "c1"."CustomerID"
""");
    }

    public override async Task Update_Where_Skip_Take_set_constant(bool async)
    {
        await base.Update_Where_Skip_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p1='Updated' (Size = 7)
@p0='4'
@p='2'

UPDATE "Customers" AS "c0"
SET "ContactName" = @p1
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    LIMIT @p0 OFFSET @p
) AS "c1"
WHERE "c0"."CustomerID" = "c1"."CustomerID"
""");
    }

    public override async Task Update_Where_OrderBy_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c0"
SET "ContactName" = @p
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
) AS "c1"
WHERE "c0"."CustomerID" = "c1"."CustomerID"
""");
    }

    public override async Task Update_Where_OrderBy_Skip_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Skip_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p0='Updated' (Size = 7)
@p='4'

UPDATE "Customers" AS "c0"
SET "ContactName" = @p0
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    ORDER BY "c"."City"
    LIMIT -1 OFFSET @p
) AS "c1"
WHERE "c0"."CustomerID" = "c1"."CustomerID"
""");
    }

    public override async Task Update_Where_OrderBy_Take_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p0='Updated' (Size = 7)
@p='4'

UPDATE "Customers" AS "c0"
SET "ContactName" = @p0
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    ORDER BY "c"."City"
    LIMIT @p
) AS "c1"
WHERE "c0"."CustomerID" = "c1"."CustomerID"
""");
    }

    public override async Task Update_Where_OrderBy_Skip_Take_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Skip_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p1='Updated' (Size = 7)
@p0='4'
@p='2'

UPDATE "Customers" AS "c0"
SET "ContactName" = @p1
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    ORDER BY "c"."City"
    LIMIT @p0 OFFSET @p
) AS "c1"
WHERE "c0"."CustomerID" = "c1"."CustomerID"
""");
    }

    public override async Task Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(bool async)
    {
        await base.Update_Where_OrderBy_Skip_Take_Skip_Take_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p3='Updated' (Size = 7)
@p0='6'
@p='2'

UPDATE "Customers" AS "c1"
SET "ContactName" = @p3
FROM (
    SELECT "c0"."CustomerID"
    FROM (
        SELECT "c"."CustomerID", "c"."City"
        FROM "Customers" AS "c"
        WHERE "c"."CustomerID" LIKE 'F%'
        ORDER BY "c"."City"
        LIMIT @p0 OFFSET @p
    ) AS "c0"
    ORDER BY "c0"."City"
    LIMIT @p OFFSET @p
) AS "c2"
WHERE "c1"."CustomerID" = "c2"."CustomerID"
""");
    }

    public override async Task Update_Where_GroupBy_aggregate_set_constant(bool async)
    {
        await base.Update_Where_GroupBy_aggregate_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" = (
    SELECT "o"."CustomerID"
    FROM "Orders" AS "o"
    GROUP BY "o"."CustomerID"
    HAVING COUNT(*) > 11
    LIMIT 1)
""");
    }

    public override async Task Update_Where_GroupBy_First_set_constant(bool async)
    {
        await base.Update_Where_GroupBy_First_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" = (
    SELECT (
        SELECT "o0"."CustomerID"
        FROM "Orders" AS "o0"
        WHERE "o"."CustomerID" = "o0"."CustomerID" OR ("o"."CustomerID" IS NULL AND "o0"."CustomerID" IS NULL)
        LIMIT 1)
    FROM "Orders" AS "o"
    GROUP BY "o"."CustomerID"
    HAVING COUNT(*) > 11
    LIMIT 1)
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
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" IN (
    SELECT (
        SELECT "c0"."CustomerID"
        FROM "Orders" AS "o0"
        LEFT JOIN "Customers" AS "c0" ON "o0"."CustomerID" = "c0"."CustomerID"
        WHERE "o"."CustomerID" = "o0"."CustomerID" OR ("o"."CustomerID" IS NULL AND "o0"."CustomerID" IS NULL)
        LIMIT 1)
    FROM "Orders" AS "o"
    GROUP BY "o"."CustomerID"
    HAVING COUNT(*) > 11
)
""");
    }

    public override async Task Update_Where_Distinct_set_constant(bool async)
    {
        await base.Update_Where_Distinct_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c0"
SET "ContactName" = @p
FROM (
    SELECT DISTINCT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
) AS "c1"
WHERE "c0"."CustomerID" = "c1"."CustomerID"
""");
    }

    public override async Task Update_Where_using_navigation_set_null(bool async)
    {
        await base.Update_Where_using_navigation_set_null(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Orders" AS "o0"
SET "OrderDate" = NULL
FROM (
    SELECT "o"."OrderID"
    FROM "Orders" AS "o"
    LEFT JOIN "Customers" AS "c" ON "o"."CustomerID" = "c"."CustomerID"
    WHERE "c"."City" = 'Seattle'
) AS "s"
WHERE "o0"."OrderID" = "s"."OrderID"
""");
    }

    public override async Task Update_Where_using_navigation_2_set_constant(bool async)
    {
        await base.Update_Where_using_navigation_2_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='1'

UPDATE "Order Details" AS "o"
SET "Quantity" = CAST(@p AS INTEGER)
FROM "Orders" AS "o0"
LEFT JOIN "Customers" AS "c" ON "o0"."CustomerID" = "c"."CustomerID"
WHERE "o"."OrderID" = "o0"."OrderID" AND "c"."City" = 'Seattle'
""");
    }

    public override async Task Update_Where_SelectMany_set_null(bool async)
    {
        await base.Update_Where_SelectMany_set_null(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Orders" AS "o"
SET "OrderDate" = NULL
FROM "Customers" AS "c"
WHERE "c"."CustomerID" = "o"."CustomerID" AND "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_property_plus_constant(bool async)
    {
        await base.Update_Where_set_property_plus_constant(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customers" AS "c"
SET "ContactName" = COALESCE("c"."ContactName", '') || 'Abc'
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_property_plus_parameter(bool async)
    {
        await base.Update_Where_set_property_plus_parameter(async);

        AssertExecuteUpdateSql(
            """
@value='Abc' (Size = 3)

UPDATE "Customers" AS "c"
SET "ContactName" = COALESCE("c"."ContactName", '') || @value
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_property_plus_property(bool async)
    {
        await base.Update_Where_set_property_plus_property(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customers" AS "c"
SET "ContactName" = COALESCE("c"."ContactName", '') || "c"."CustomerID"
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_constant_using_ef_property(bool async)
    {
        await base.Update_Where_set_constant_using_ef_property(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_set_null(bool async)
    {
        await base.Update_Where_set_null(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customers" AS "c"
SET "ContactName" = NULL
WHERE "c"."CustomerID" LIKE 'F%'
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
@value='Abc' (Size = 3)
@p='Seattle' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @value,
    "City" = @p
WHERE "c"."CustomerID" LIKE 'F%'
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
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c1"
SET "ContactName" = @p
FROM (
    SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    UNION
    SELECT "c0"."CustomerID", "c0"."Address", "c0"."City", "c0"."CompanyName", "c0"."ContactName", "c0"."ContactTitle", "c0"."Country", "c0"."Fax", "c0"."Phone", "c0"."PostalCode", "c0"."Region"
    FROM "Customers" AS "c0"
    WHERE "c0"."CustomerID" LIKE 'A%'
) AS "u"
WHERE "c1"."CustomerID" = "u"."CustomerID"
""");
    }

    public override async Task Update_Concat_set_constant(bool async)
    {
        await base.Update_Concat_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c1"
SET "ContactName" = @p
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    UNION ALL
    SELECT "c0"."CustomerID"
    FROM "Customers" AS "c0"
    WHERE "c0"."CustomerID" LIKE 'A%'
) AS "u"
WHERE "c1"."CustomerID" = "u"."CustomerID"
""");
    }

    public override async Task Update_Except_set_constant(bool async)
    {
        await base.Update_Except_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c1"
SET "ContactName" = @p
FROM (
    SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    EXCEPT
    SELECT "c0"."CustomerID", "c0"."Address", "c0"."City", "c0"."CompanyName", "c0"."ContactName", "c0"."ContactTitle", "c0"."Country", "c0"."Fax", "c0"."Phone", "c0"."PostalCode", "c0"."Region"
    FROM "Customers" AS "c0"
    WHERE "c0"."CustomerID" LIKE 'A%'
) AS "e"
WHERE "c1"."CustomerID" = "e"."CustomerID"
""");
    }

    public override async Task Update_Intersect_set_constant(bool async)
    {
        await base.Update_Intersect_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c1"
SET "ContactName" = @p
FROM (
    SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" LIKE 'F%'
    INTERSECT
    SELECT "c0"."CustomerID", "c0"."Address", "c0"."City", "c0"."CompanyName", "c0"."ContactName", "c0"."ContactTitle", "c0"."Country", "c0"."Fax", "c0"."Phone", "c0"."PostalCode", "c0"."Region"
    FROM "Customers" AS "c0"
    WHERE "c0"."CustomerID" LIKE 'A%'
) AS "i"
WHERE "c1"."CustomerID" = "i"."CustomerID"
""");
    }

    public override async Task Update_with_join_set_constant(bool async)
    {
        await base.Update_with_join_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
FROM (
    SELECT "o"."CustomerID"
    FROM "Orders" AS "o"
    WHERE "o"."OrderID" < 10300
) AS "o0"
WHERE "c"."CustomerID" = "o0"."CustomerID" AND "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_with_LeftJoin(bool async)
    {
        await base.Update_with_LeftJoin(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c0"
SET "ContactName" = @p
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    LEFT JOIN (
        SELECT "o"."CustomerID"
        FROM "Orders" AS "o"
        WHERE "o"."OrderID" < 10300
    ) AS "o0" ON "c"."CustomerID" = "o0"."CustomerID"
    WHERE "c"."CustomerID" LIKE 'F%'
) AS "s"
WHERE "c0"."CustomerID" = "s"."CustomerID"
""");
    }

    public override async Task Update_with_LeftJoin_via_flattened_GroupJoin(bool async)
    {
        await base.Update_with_LeftJoin_via_flattened_GroupJoin(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c0"
SET "ContactName" = @p
FROM (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    LEFT JOIN (
        SELECT "o"."CustomerID"
        FROM "Orders" AS "o"
        WHERE "o"."OrderID" < 10300
    ) AS "o0" ON "c"."CustomerID" = "o0"."CustomerID"
    WHERE "c"."CustomerID" LIKE 'F%'
) AS "s"
WHERE "c0"."CustomerID" = "s"."CustomerID"
""");
    }

    public override async Task Update_with_RightJoin(bool async)
    {
        await base.Update_with_RightJoin(async);

        AssertExecuteUpdateSql(
            """
@p='2020-01-01T00:00:00.0000000Z' (Nullable = true) (DbType = DateTime)

UPDATE "Orders" AS "o0"
SET "OrderDate" = @p
FROM (
    SELECT "o"."OrderID"
    FROM "Orders" AS "o"
    RIGHT JOIN (
        SELECT "c"."CustomerID"
        FROM "Customers" AS "c"
        WHERE "c"."CustomerID" LIKE 'F%'
    ) AS "c0" ON "o"."CustomerID" = "c0"."CustomerID"
    WHERE "o"."OrderID" < 10300
) AS "s"
WHERE "o0"."OrderID" = "s"."OrderID"
""");
    }

    public override async Task Update_with_cross_join_set_constant(bool async)
    {
        await base.Update_with_cross_join_set_constant(async);

        AssertExecuteUpdateSql(
            """
@p='Updated' (Size = 7)

UPDATE "Customers" AS "c"
SET "ContactName" = @p
FROM (
    SELECT 1
    FROM "Orders" AS "o"
    WHERE "o"."OrderID" < 10300
) AS "o0"
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_with_cross_apply_set_constant(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Update_with_cross_apply_set_constant(async))).Message);

    public override async Task Update_with_outer_apply_set_constant(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Update_with_outer_apply_set_constant(async))).Message);

    [ConditionalTheory(Skip = "Issue#28886")]
    public override async Task Update_with_cross_join_left_join_set_constant(bool async)
    {
        await base.Update_with_cross_join_left_join_set_constant(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customers" AS "c"
SET "ContactName" = 'Updated'
FROM (
    SELECT "c0"."CustomerID", "c0"."Address", "c0"."City", "c0"."CompanyName", "c0"."ContactName", "c0"."ContactTitle", "c0"."Country", "c0"."Fax", "c0"."Phone", "c0"."PostalCode", "c0"."Region"
    FROM "Customers" AS "c0"
    WHERE "c0"."City" IS NOT NULL AND ("c0"."City" LIKE 'S%')
) AS "t"
LEFT JOIN (
    SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
    FROM "Orders" AS "o"
    WHERE "o"."OrderID" < 10300
) AS "t0" ON "c"."CustomerID" = "t0"."CustomerID"
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_with_cross_join_cross_apply_set_constant(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Update_with_cross_join_cross_apply_set_constant(async)))
            .Message);

    public override async Task Update_with_cross_join_outer_apply_set_constant(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Update_with_cross_join_outer_apply_set_constant(async)))
            .Message);

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
UPDATE "Orders" AS "o1"
SET "OrderDate" = NULL
FROM (
    SELECT "o0"."OrderID"
    FROM "Customers" AS "c"
    INNER JOIN (
        SELECT "o"."OrderID", "o"."CustomerID"
        FROM "Orders" AS "o"
        WHERE CAST(strftime('%Y', "o"."OrderDate") AS INTEGER) = 1997
    ) AS "o0" ON "c"."CustomerID" = "o0"."CustomerID"
    WHERE "c"."CustomerID" LIKE 'F%'
) AS "s"
WHERE "o1"."OrderID" = "s"."OrderID"
""");
    }

    public override async Task Update_Where_Join_set_property_from_joined_single_result_table(bool async)
    {
        await base.Update_Where_Join_set_property_from_joined_single_result_table(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customers" AS "c"
SET "City" = COALESCE(CAST(CAST(strftime('%Y', (
    SELECT "o"."OrderDate"
    FROM "Orders" AS "o"
    WHERE "c"."CustomerID" = "o"."CustomerID"
    ORDER BY "o"."OrderDate" DESC
    LIMIT 1)) AS INTEGER) AS TEXT), '')
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_Join_set_property_from_joined_table(bool async)
    {
        await base.Update_Where_Join_set_property_from_joined_table(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customers" AS "c"
SET "City" = "c1"."City"
FROM (
    SELECT "c0"."City"
    FROM "Customers" AS "c0"
    WHERE "c0"."CustomerID" = 'ALFKI'
) AS "c1"
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    public override async Task Update_Where_Join_set_property_from_joined_single_result_scalar(bool async)
    {
        await base.Update_Where_Join_set_property_from_joined_single_result_scalar(async);

        AssertExecuteUpdateSql(
            """
UPDATE "Customers" AS "c"
SET "City" = COALESCE(CAST(CAST(strftime('%Y', (
    SELECT "o"."OrderDate"
    FROM "Orders" AS "o"
    WHERE "c"."CustomerID" = "o"."CustomerID"
    ORDER BY "o"."OrderDate" DESC
    LIMIT 1)) AS INTEGER) AS TEXT), '')
WHERE "c"."CustomerID" LIKE 'F%'
""");
    }

    [ConditionalTheory(Skip = "Issue#28886")]
    public override async Task Update_with_two_inner_joins(bool async)
    {
        await base.Update_with_two_inner_joins(async);

        AssertExecuteUpdateSql(
            """

""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
