// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public class NorthwindBulkUpdatesSqliteTest : NorthwindBulkUpdatesTestBase<NorthwindBulkUpdatesSqliteFixture<NoopModelCustomizer>>
{
    public NorthwindBulkUpdatesSqliteTest(NorthwindBulkUpdatesSqliteFixture<NoopModelCustomizer> fixture)
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
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE ""o"".""OrderID"" < 10300");
    }

    public override async Task Delete_Where_parameter(bool async)
    {
        await base.Delete_Where_parameter(async);

        AssertSql(
            @"@__quantity_0='1' (Nullable = true) (DbType = Int16)

DELETE FROM ""Order Details"" AS ""o""
WHERE ""o"".""Quantity"" = @__quantity_0",
                //
                @"DELETE FROM ""Order Details"" AS ""o""
WHERE 0");
    }

    public override async Task Delete_Where_OrderBy(bool async)
    {
        await base.Delete_Where_OrderBy(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Order Details"" AS ""o0""
    WHERE ""o0"".""OrderID"" < 10300 AND ""o0"".""OrderID"" = ""o"".""OrderID"" AND ""o0"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_OrderBy_Skip(bool async)
    {
        await base.Delete_Where_OrderBy_Skip(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        ORDER BY ""o0"".""OrderID""
        LIMIT -1 OFFSET @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_OrderBy_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        ORDER BY ""o0"".""OrderID""
        LIMIT @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_OrderBy_Skip_Take(bool async)
    {
        await base.Delete_Where_OrderBy_Skip_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        ORDER BY ""o0"".""OrderID""
        LIMIT @__p_0 OFFSET @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_Skip(bool async)
    {
        await base.Delete_Where_Skip(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        LIMIT -1 OFFSET @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_Take(bool async)
    {
        await base.Delete_Where_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        LIMIT @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_Skip_Take(bool async)
    {
        await base.Delete_Where_Skip_Take(async);

        AssertSql(
            @"@__p_0='100'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10300
        LIMIT @__p_0 OFFSET @__p_0
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_predicate_with_group_by_aggregate(bool async)
    {
        await base.Delete_Where_predicate_with_group_by_aggregate(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE ""o"".""OrderID"" < (
    SELECT (
        SELECT ""o1"".""OrderID""
        FROM ""Orders"" AS ""o1""
        WHERE ""o0"".""CustomerID"" = ""o1"".""CustomerID"" OR (""o0"".""CustomerID"" IS NULL AND ""o1"".""CustomerID"" IS NULL)
        LIMIT 1)
    FROM ""Orders"" AS ""o0""
    GROUP BY ""o0"".""CustomerID""
    HAVING COUNT(*) > 11
    LIMIT 1)");
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
@__p_2='5'
@__p_1='20'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""t"".""OrderID"", ""t"".""ProductID"", ""t"".""Discount"", ""t"".""Quantity"", ""t"".""UnitPrice""
        FROM (
            SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
            FROM ""Order Details"" AS ""o0""
            WHERE ""o0"".""OrderID"" < 10300
            LIMIT @__p_0 OFFSET @__p_0
        ) AS ""t""
        LIMIT @__p_2 OFFSET @__p_1
    ) AS ""t0""
    WHERE ""t0"".""OrderID"" = ""o"".""OrderID"" AND ""t0"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_Distinct(bool async)
    {
        await base.Delete_Where_Distinct(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE ""o"".""OrderID"" < 10300");
    }

    public override async Task Delete_SelectMany(bool async)
    {
        await base.Delete_SelectMany(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Orders"" AS ""o0""
    INNER JOIN ""Order Details"" AS ""o1"" ON ""o0"".""OrderID"" = ""o1"".""OrderID""
    WHERE ""o0"".""OrderID"" < 10250 AND ""o1"".""OrderID"" = ""o"".""OrderID"" AND ""o1"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_SelectMany_subquery(bool async)
    {
        await base.Delete_SelectMany_subquery(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Orders"" AS ""o0""
    INNER JOIN (
        SELECT ""o1"".""OrderID"", ""o1"".""ProductID"", ""o1"".""Discount"", ""o1"".""Quantity"", ""o1"".""UnitPrice""
        FROM ""Order Details"" AS ""o1""
        WHERE ""o1"".""ProductID"" > 0
    ) AS ""t"" ON ""o0"".""OrderID"" = ""t"".""OrderID""
    WHERE ""o0"".""OrderID"" < 10250 AND ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_using_navigation(bool async)
    {
        await base.Delete_Where_using_navigation(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Order Details"" AS ""o0""
    INNER JOIN ""Orders"" AS ""o1"" ON ""o0"".""OrderID"" = ""o1"".""OrderID""
    WHERE CAST(strftime('%Y', ""o1"".""OrderDate"") AS INTEGER) = 2000 AND ""o0"".""OrderID"" = ""o"".""OrderID"" AND ""o0"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Where_using_navigation_2(bool async)
    {
        await base.Delete_Where_using_navigation_2(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Order Details"" AS ""o0""
    INNER JOIN ""Orders"" AS ""o1"" ON ""o0"".""OrderID"" = ""o1"".""OrderID""
    LEFT JOIN ""Customers"" AS ""c"" ON ""o1"".""CustomerID"" = ""c"".""CustomerID""
    WHERE ""c"".""CustomerID"" IS NOT NULL AND (""c"".""CustomerID"" LIKE 'F%') AND ""o0"".""OrderID"" = ""o"".""OrderID"" AND ""o0"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Union(bool async)
    {
        await base.Delete_Union(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10250
        UNION
        SELECT ""o1"".""OrderID"", ""o1"".""ProductID"", ""o1"".""Discount"", ""o1"".""Quantity"", ""o1"".""UnitPrice""
        FROM ""Order Details"" AS ""o1""
        WHERE ""o1"".""OrderID"" > 11250
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Concat(bool async)
    {
        await base.Delete_Concat(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10250
        UNION ALL
        SELECT ""o1"".""OrderID"", ""o1"".""ProductID"", ""o1"".""Discount"", ""o1"".""Quantity"", ""o1"".""UnitPrice""
        FROM ""Order Details"" AS ""o1""
        WHERE ""o1"".""OrderID"" > 11250
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Intersect(bool async)
    {
        await base.Delete_Intersect(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10250
        INTERSECT
        SELECT ""o1"".""OrderID"", ""o1"".""ProductID"", ""o1"".""Discount"", ""o1"".""Quantity"", ""o1"".""UnitPrice""
        FROM ""Order Details"" AS ""o1""
        WHERE ""o1"".""OrderID"" > 11250
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_Except(bool async)
    {
        await base.Delete_Except(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""o0"".""OrderID"", ""o0"".""ProductID"", ""o0"".""Discount"", ""o0"".""Quantity"", ""o0"".""UnitPrice""
        FROM ""Order Details"" AS ""o0""
        WHERE ""o0"".""OrderID"" < 10250
        EXCEPT
        SELECT ""o1"".""OrderID"", ""o1"".""ProductID"", ""o1"".""Discount"", ""o1"".""Quantity"", ""o1"".""UnitPrice""
        FROM ""Order Details"" AS ""o1""
        WHERE ""o1"".""OrderID"" > 11250
    ) AS ""t""
    WHERE ""t"".""OrderID"" = ""o"".""OrderID"" AND ""t"".""ProductID"" = ""o"".""ProductID"")");
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
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT ""OrderID"", ""ProductID"", ""UnitPrice"", ""Quantity"", ""Discount""
        FROM ""Order Details""
        WHERE ""OrderID"" < 10300
    ) AS ""m""
    WHERE ""m"".""OrderID"" = ""o"".""OrderID"" AND ""m"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_with_join(bool async)
    {
        await base.Delete_with_join(async);

        AssertSql(
            @"@__p_1='100'
@__p_0='0'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Order Details"" AS ""o0""
    INNER JOIN (
        SELECT ""o1"".""OrderID"", ""o1"".""CustomerID"", ""o1"".""EmployeeID"", ""o1"".""OrderDate""
        FROM ""Orders"" AS ""o1""
        WHERE ""o1"".""OrderID"" < 10300
        ORDER BY ""o1"".""OrderID""
        LIMIT @__p_1 OFFSET @__p_0
    ) AS ""t"" ON ""o0"".""OrderID"" = ""t"".""OrderID""
    WHERE ""o0"".""OrderID"" = ""o"".""OrderID"" AND ""o0"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_with_left_join(bool async)
    {
        await base.Delete_with_left_join(async);

        AssertSql(
            @"@__p_1='100'
@__p_0='0'

DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Order Details"" AS ""o0""
    LEFT JOIN (
        SELECT ""o1"".""OrderID"", ""o1"".""CustomerID"", ""o1"".""EmployeeID"", ""o1"".""OrderDate""
        FROM ""Orders"" AS ""o1""
        WHERE ""o1"".""OrderID"" < 10300
        ORDER BY ""o1"".""OrderID""
        LIMIT @__p_1 OFFSET @__p_0
    ) AS ""t"" ON ""o0"".""OrderID"" = ""t"".""OrderID""
    WHERE ""o0"".""OrderID"" < 10276 AND ""o0"".""OrderID"" = ""o"".""OrderID"" AND ""o0"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_with_cross_join(bool async)
    {
        await base.Delete_with_cross_join(async);

        AssertSql(
            @"DELETE FROM ""Order Details"" AS ""o""
WHERE EXISTS (
    SELECT 1
    FROM ""Order Details"" AS ""o0""
    CROSS JOIN (
        SELECT ""o1"".""OrderID"", ""o1"".""CustomerID"", ""o1"".""EmployeeID"", ""o1"".""OrderDate""
        FROM ""Orders"" AS ""o1""
        WHERE ""o1"".""OrderID"" < 10300
        ORDER BY ""o1"".""OrderID""
        LIMIT 100 OFFSET 0
    ) AS ""t""
    WHERE ""o0"".""OrderID"" < 10276 AND ""o0"".""OrderID"" = ""o"".""OrderID"" AND ""o0"".""ProductID"" = ""o"".""ProductID"")");
    }

    public override async Task Delete_with_cross_apply(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Delete_with_cross_apply(async))).Message);

    public override async Task Delete_with_outer_apply(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(() => base.Delete_with_outer_apply(async))).Message);

    public override async Task Update_where_constant(bool async)
    {
        await base.Update_where_constant(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = 'Updated'
WHERE ""c"".""CustomerID"" LIKE 'F%'");
    }

    public override async Task Update_where_parameter_in_predicate(bool async)
    {
        await base.Update_where_parameter_in_predicate(async);

        AssertExecuteUpdateSql(
            @"@__customer_0='ALFKI' (Size = 5)

UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = 'Updated'
WHERE ""c"".""CustomerID"" = @__customer_0",
                //
                @"@__customer_0='ALFKI' (Size = 5)

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = @__customer_0",
                //
                @"SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE 0",
                //
                @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = 'Updated'
WHERE 0");
    }

    public override async Task Update_where_parameter(bool async)
    {
        await base.Update_where_parameter(async);

        AssertExecuteUpdateSql(
            @"@__value_0='Abc' (Size = 3)

UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = @__value_0
WHERE ""c"".""CustomerID"" LIKE 'F%'");
    }

    [ConditionalTheory(Skip = "Issue#28661")]
    public override async Task Update_where_take_constant(bool async)
    {
        await base.Update_where_take_constant(async);

        AssertExecuteUpdateSql(
            @"@__p_0='4'

UPDATE TOP(@__p_0) [c]
    SET [c].[ContactName] = N'Updated'
FROM [Customers] AS [c]
WHERE [c].[CustomerID] LIKE N'F%'");
    }

    public override async Task Update_where_group_by_aggregate_constant(bool async)
    {
        await base.Update_where_group_by_aggregate_constant(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = 'Updated'
WHERE ""c"".""CustomerID"" = (
    SELECT ""o"".""CustomerID""
    FROM ""Orders"" AS ""o""
    GROUP BY ""o"".""CustomerID""
    HAVING COUNT(*) > 11
    LIMIT 1)");
    }

    public override async Task Update_where_group_by_first_constant(bool async)
    {
        await base.Update_where_group_by_first_constant(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = 'Updated'
WHERE ""c"".""CustomerID"" = (
    SELECT (
        SELECT ""o0"".""CustomerID""
        FROM ""Orders"" AS ""o0""
        WHERE ""o"".""CustomerID"" = ""o0"".""CustomerID"" OR (""o"".""CustomerID"" IS NULL AND ""o0"".""CustomerID"" IS NULL)
        LIMIT 1)
    FROM ""Orders"" AS ""o""
    GROUP BY ""o"".""CustomerID""
    HAVING COUNT(*) > 11
    LIMIT 1)");
    }

    public override async Task Update_where_group_by_first_constant_2(bool async)
    {
        await base.Update_where_group_by_first_constant_2(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_group_by_first_constant_3(bool async)
    {
        await base.Update_where_group_by_first_constant_3(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_where_distinct_constant(bool async)
    {
        await base.Update_where_distinct_constant(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = 'Updated'
WHERE ""c"".""CustomerID"" LIKE 'F%'");
    }

    public override async Task Update_where_using_navigation(bool async)
    {
        await base.Update_where_using_navigation(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Orders"" AS ""o""
    SET ""OrderDate"" = NULL
FROM ""Customers"" AS ""c""
WHERE ""o"".""CustomerID"" = ""c"".""CustomerID"" AND ""c"".""City"" = 'Seattle'");
    }

    public override async Task Update_where_using_navigation_2(bool async)
    {
        await base.Update_where_using_navigation_2(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Order Details"" AS ""o""
    SET ""Quantity"" = CAST(1 AS INTEGER)
FROM ""Orders"" AS ""o0""
LEFT JOIN ""Customers"" AS ""c"" ON ""o0"".""CustomerID"" = ""c"".""CustomerID""
WHERE ""o"".""OrderID"" = ""o0"".""OrderID"" AND ""c"".""City"" = 'Seattle'");
    }

    public override async Task Update_where_select_many(bool async)
    {
        await base.Update_where_select_many(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Orders"" AS ""o""
    SET ""OrderDate"" = NULL
FROM ""Customers"" AS ""c""
WHERE ""c"".""CustomerID"" = ""o"".""CustomerID"" AND (""c"".""CustomerID"" LIKE 'F%')");
    }

    public override async Task Update_where_using_property_plus_constant(bool async)
    {
        await base.Update_where_using_property_plus_constant(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = COALESCE(""c"".""ContactName"", '') || 'Abc'
WHERE ""c"".""CustomerID"" LIKE 'F%'");
    }

    public override async Task Update_where_using_property_plus_parameter(bool async)
    {
        await base.Update_where_using_property_plus_parameter(async);

        AssertExecuteUpdateSql(
            @"@__value_0='Abc' (Size = 3)

UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = COALESCE(""c"".""ContactName"", '') || @__value_0
WHERE ""c"".""CustomerID"" LIKE 'F%'");
    }

    public override async Task Update_where_using_property_plus_property(bool async)
    {
        await base.Update_where_using_property_plus_property(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = COALESCE(""c"".""ContactName"", '') || ""c"".""CustomerID""
WHERE ""c"".""CustomerID"" LIKE 'F%'");
    }

    public override async Task Update_where_constant_using_ef_property(bool async)
    {
        await base.Update_where_constant_using_ef_property(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = 'Updated'
WHERE ""c"".""CustomerID"" LIKE 'F%'");
    }

    public override async Task Update_where_null(bool async)
    {
        await base.Update_where_null(async);

        AssertExecuteUpdateSql(
            @"UPDATE ""Customers"" AS ""c""
    SET ""ContactName"" = NULL
WHERE ""c"".""CustomerID"" LIKE 'F%'");
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

    public override async Task Update_where_multi_property_update(bool async)
    {
        await base.Update_where_multi_property_update(async);

        AssertExecuteUpdateSql(
            @"@__value_0='Abc' (Size = 3)

UPDATE ""Customers"" AS ""c""
    SET ""City"" = 'Seattle',
    ""ContactName"" = @__value_0
WHERE ""c"".""CustomerID"" LIKE 'F%'");
    }

    public override async Task Update_with_invalid_lambda_in_set_property_throws(bool async)
    {
        await base.Update_with_invalid_lambda_in_set_property_throws(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_multiple_entity_update(bool async)
    {
        await base.Update_multiple_entity_update(async);

        AssertExecuteUpdateSql();
    }

    public override async Task Update_unmapped_property(bool async)
    {
        await base.Update_unmapped_property(async);

        AssertExecuteUpdateSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertExecuteUpdateSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected, forUpdate: true);
}
