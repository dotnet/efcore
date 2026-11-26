// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindCommonTableQuerySqliteTest : NorthwindCommonTableQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindCommonTableQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Embedded_query_in_Any(bool async)
    {
        await base.Embedded_query_in_Any(async);

        AssertSql(
            """
WITH "londonCustomers" AS (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."City" = 'London'
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "londonCustomers" AS "c0"
    WHERE "c0"."CustomerID" = "o"."CustomerID")
""");
    }

    public override async Task Embedded_query_referenced_twice(bool async)
    {
        await base.Embedded_query_referenced_twice(async);

        AssertSql(
            """
WITH "londonCustomers" AS (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."City" = 'London'
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE "o"."CustomerID" IN (
    SELECT "c0"."CustomerID"
    FROM "londonCustomers" AS "c0"
) AND (
    SELECT COUNT(*)
    FROM "londonCustomers" AS "c2") > 1
""");
    }

    public override async Task Embedded_query_referenced_twice_with_different_columns(bool async)
    {
        await base.Embedded_query_referenced_twice_with_different_columns(async);

        AssertSql(
            """
WITH "londonCustomers" AS (
    SELECT "c"."CustomerID", "c"."ContactTitle"
    FROM "Customers" AS "c"
    WHERE "c"."City" = 'London'
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE "o"."CustomerID" IN (
    SELECT "c0"."CustomerID"
    FROM "londonCustomers" AS "c0"
) AND EXISTS (
    SELECT 1
    FROM "londonCustomers" AS "c2"
    WHERE "c2"."ContactTitle" = 'Sales Representative')
""");
    }

    public override async Task Embedded_query_with_parameter(bool async)
    {
        await base.Embedded_query_with_parameter(async);

        AssertSql(
            """
@city='London' (Size = 6)

WITH "cityCustomers" AS (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."City" = @city
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "cityCustomers" AS "c0"
    WHERE "c0"."CustomerID" = "o"."CustomerID")
""");
    }

    public override async Task Embedded_query_named_like_table(bool async)
    {
        await base.Embedded_query_named_like_table(async);

        AssertSql(
            """
WITH "customers1" AS (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."City" = 'London'
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "customers1" AS "c0"
    WHERE "c0"."CustomerID" = "o"."CustomerID")
""");
    }

    public override async Task Embedded_queries_with_names_differing_in_case(bool async)
    {
        await base.Embedded_queries_with_names_differing_in_case(async);

        AssertSql(
            """
WITH "query" AS (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."City" = 'London'
),
"Query1" AS (
    SELECT "c1"."CustomerID"
    FROM "Customers" AS "c1"
    WHERE "c1"."ContactTitle" = 'Owner'
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "query" AS "c0"
    WHERE "c0"."CustomerID" = "o"."CustomerID") OR EXISTS (
    SELECT 1
    FROM "Query1" AS "c2"
    WHERE "c2"."CustomerID" = "o"."CustomerID")
""");
    }

    public override async Task Embedded_query_with_Take(bool async)
    {
        await base.Embedded_query_with_Take(async);

        AssertSql(
            """
WITH "latestOrders" AS (
    SELECT "o"."CustomerID"
    FROM "Orders" AS "o"
    ORDER BY "o"."OrderID" DESC
    LIMIT 5
)
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "latestOrders" AS "o0"
    WHERE "o0"."CustomerID" = "c"."CustomerID")
""");
    }

    public override async Task Embedded_query_referencing_embedded_query(bool async)
    {
        await base.Embedded_query_referencing_embedded_query(async);

        AssertSql(
            """
WITH "londonCustomers" AS (
    SELECT "c0"."CustomerID"
    FROM "Customers" AS "c0"
    WHERE "c0"."City" = 'London'
),
"lateLondonOrders" AS (
    SELECT "o"."CustomerID"
    FROM "Orders" AS "o"
    WHERE "o"."OrderID" > 11000 AND EXISTS (
        SELECT 1
        FROM "londonCustomers" AS "c1"
        WHERE "c1"."CustomerID" = "o"."CustomerID")
)
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."Country" = 'UK' AND EXISTS (
    SELECT 1
    FROM "lateLondonOrders" AS "o0"
    WHERE "o0"."CustomerID" = "c"."CustomerID")
""");
    }

    public override async Task Embedded_query_compared_to_navigation(bool async)
    {
        await base.Embedded_query_compared_to_navigation(async);

        AssertSql(
            """
WITH "londonCustomers" AS (
    SELECT "c0"."CustomerID"
    FROM "Customers" AS "c0"
    WHERE "c0"."City" = 'London'
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
LEFT JOIN "Customers" AS "c" ON "o"."CustomerID" = "c"."CustomerID"
WHERE "c"."CustomerID" IN (
    SELECT "c1"."CustomerID"
    FROM "londonCustomers" AS "c1"
)
""");
    }

    public override async Task Embedded_query_with_projection(bool async)
    {
        await base.Embedded_query_with_projection(async);

        AssertSql(
            """
WITH "customers1" AS (
    SELECT "c"."CustomerID", upper("c"."City") AS "Upper"
    FROM "Customers" AS "c"
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "customers1" AS "c0"
    WHERE "c0"."CustomerID" = "o"."CustomerID" AND "c0"."Upper" = 'LONDON')
""");
    }

    public override async Task Embedded_query_in_collection_projection(bool async)
    {
        await base.Embedded_query_in_collection_projection(async);

        AssertSql(
            """
WITH "lateOrders" AS (
    SELECT "o"."OrderID", "o"."CustomerID"
    FROM "Orders" AS "o"
    WHERE "o"."OrderID" > 11000
)
SELECT "c"."CustomerID", "o0"."OrderID"
FROM "Customers" AS "c"
LEFT JOIN "lateOrders" AS "o0" ON "c"."CustomerID" = "o0"."CustomerID"
WHERE "c"."City" = 'London'
ORDER BY "c"."CustomerID"
""");
    }

    public override async Task Embedded_query_in_SelectMany(bool async)
    {
        await base.Embedded_query_in_SelectMany(async);

        AssertSql(
            """
WITH "lateOrders" AS (
    SELECT "o"."OrderID", "o"."CustomerID"
    FROM "Orders" AS "o"
    WHERE "o"."OrderID" > 11000
)
SELECT "c"."CustomerID", "o0"."OrderID"
FROM "Customers" AS "c"
INNER JOIN "lateOrders" AS "o0" ON "c"."CustomerID" = "o0"."CustomerID"
WHERE "c"."City" = 'London'
""");
    }

    public override async Task Embedded_query_in_Join_over_required_foreign_key(bool async)
    {
        await base.Embedded_query_in_Join_over_required_foreign_key(async);

        AssertSql(
            """
WITH "lateOrders" AS (
    SELECT "o0"."OrderID"
    FROM "Orders" AS "o0"
    WHERE "o0"."OrderID" > 11070
)
SELECT "p"."ProductID", "p"."Discontinued", "p"."ProductName", "p"."SupplierID", "p"."UnitPrice", "p"."UnitsInStock"
FROM "Products" AS "p"
WHERE "p"."ProductID" IN (
    SELECT "o"."ProductID"
    FROM "Order Details" AS "o"
    INNER JOIN "lateOrders" AS "o1" ON "o"."OrderID" = "o1"."OrderID"
)
""");
    }

    public override async Task Embedded_query_in_both_sides_of_set_operation(bool async)
    {
        await base.Embedded_query_in_both_sides_of_set_operation(async);

        AssertSql(
            """
WITH "londonCustomers" AS (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."City" = 'London'
)
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderID" < 10300 AND EXISTS (
    SELECT 1
    FROM "londonCustomers" AS "c0"
    WHERE "c0"."CustomerID" = "o"."CustomerID")
UNION ALL
SELECT "o0"."OrderID", "o0"."CustomerID", "o0"."EmployeeID", "o0"."OrderDate"
FROM "Orders" AS "o0"
WHERE "o0"."OrderID" > 11000 AND EXISTS (
    SELECT 1
    FROM "londonCustomers" AS "c2"
    WHERE "c2"."CustomerID" = "o0"."CustomerID")
""");
    }

    public override async Task Embedded_DbSet_is_not_a_common_table(bool async)
    {
        await base.Embedded_DbSet_is_not_a_common_table(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE EXISTS (
    SELECT 1
    FROM "Customers" AS "c"
    WHERE "c"."CustomerID" = "o"."CustomerID" AND "c"."City" = 'London')
""");
    }

    public override async Task Embedded_grouping_query_in_Any(bool async)
    {
        await base.Embedded_grouping_query_in_Any(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "Orders" AS "o"
    GROUP BY "o"."CustomerID"
    HAVING "o"."CustomerID" = "c"."CustomerID" AND COUNT(*) > 20)
""");
    }

    public override async Task Embedded_grouping_query_in_projection(bool async)
    {
        await base.Embedded_grouping_query_in_projection(async);

        AssertSql(
            """
SELECT "c"."CustomerID", COALESCE((
    SELECT COUNT(*)
    FROM "Orders" AS "o"
    GROUP BY "o"."CustomerID"
    HAVING "o"."CustomerID" = "c"."CustomerID"
    LIMIT 1), 0) AS "Count"
FROM "Customers" AS "c"
""");
    }

    public override async Task Embedded_query_named_like_table_in_raw_sql()
    {
        await base.Embedded_query_named_like_table_in_raw_sql();

        AssertSql(
            """
WITH "orders1" AS (
    SELECT "c"."CustomerID"
    FROM "Customers" AS "c"
    WHERE "c"."City" = 'London'
)
SELECT "m"."CustomerID"
FROM (
    SELECT * FROM "Customers" WHERE "CustomerID" IN (SELECT "CustomerID" FROM "Orders" WHERE "OrderID" < 10300)
) AS "m"
WHERE EXISTS (
    SELECT 1
    FROM "orders1" AS "c0"
    WHERE "c0"."CustomerID" = "m"."CustomerID")
ORDER BY "m"."CustomerID"
""",
            //
            """
SELECT "s"."CustomerID"
FROM (
    SELECT DISTINCT "o"."CustomerID"
    FROM "Orders" AS "o"
    LEFT JOIN "Customers" AS "c" ON "o"."CustomerID" = "c"."CustomerID"
    WHERE "o"."OrderID" < 10300 AND "c"."City" = 'London'
) AS "s"
ORDER BY "s"."CustomerID"
""");
    }

    public override async Task Embedded_in_memory_query_is_not_a_common_table(bool async)
    {
        await base.Embedded_in_memory_query_is_not_a_common_table(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."CustomerID" IN ('ALFKI', 'ANATR')
""");
    }

    public override async Task Embedded_query_in_split_collection_projection(bool async)
    {
        await base.Embedded_query_in_split_collection_projection(async);

        AssertSql(
            """
WITH "lateOrders" AS (
    SELECT "o"."CustomerID"
    FROM "Orders" AS "o"
    WHERE "o"."OrderID" > 11000
)
SELECT "c"."CustomerID"
FROM "Customers" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "lateOrders" AS "o0"
    WHERE "o0"."CustomerID" = "c"."CustomerID")
ORDER BY "c"."CustomerID"
""",
            //
            """
WITH "lateOrders" AS (
    SELECT "o5"."OrderID", "o5"."CustomerID"
    FROM "Orders" AS "o5"
    WHERE "o5"."OrderID" > 11000
)
SELECT "o6"."OrderID", "c"."CustomerID"
FROM "Customers" AS "c"
INNER JOIN "lateOrders" AS "o6" ON "c"."CustomerID" = "o6"."CustomerID"
WHERE EXISTS (
    SELECT 1
    FROM "lateOrders" AS "o0"
    WHERE "o0"."CustomerID" = "c"."CustomerID")
ORDER BY "c"."CustomerID"
""");
    }

    public override async Task Embedded_ordered_query_with_ThenBy(bool async)
    {
        await base.Embedded_ordered_query_with_ThenBy(async);

        AssertSql(
            """
SELECT COALESCE((
    SELECT "o"."OrderID"
    FROM "Orders" AS "o"
    WHERE "o"."OrderID" > 11000 AND "o"."CustomerID" = "c"."CustomerID"
    ORDER BY "o"."CustomerID", "o"."OrderID"
    LIMIT 1), 0)
FROM "Customers" AS "c"
WHERE "c"."City" = 'London'
""");
    }

    public override async Task Embedded_query_with_entity_in_anonymous_type_is_not_a_common_table(bool async)
    {
        await base.Embedded_query_with_entity_in_anonymous_type_is_not_a_common_table(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "Orders" AS "o"
    LEFT JOIN "Customers" AS "c0" ON "o"."CustomerID" = "c0"."CustomerID"
    WHERE "o"."OrderID" > 11050 AND "c0"."CustomerID" = "c"."CustomerID" AND (
        SELECT COUNT(*)
        FROM "Orders" AS "o0"
        WHERE "c0"."CustomerID" IS NOT NULL AND "c0"."CustomerID" = "o0"."CustomerID") > 10)
""");
    }

    public override async Task Embedded_query_with_entity_in_dto_is_not_a_common_table(bool async)
    {
        await base.Embedded_query_with_entity_in_dto_is_not_a_common_table(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "Orders" AS "o"
    LEFT JOIN "Customers" AS "c0" ON "o"."CustomerID" = "c0"."CustomerID"
    WHERE "o"."OrderID" > 11000 AND ("c0"."City" = "c"."City" OR ("c0"."City" IS NULL AND "c"."City" IS NULL)))
""");
    }

    public override async Task Embedded_query_with_entity_from_subquery(bool async)
    {
        await base.Embedded_query_with_entity_from_subquery(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "Orders" AS "o"
    INNER JOIN "Products" AS "p" ON (
        SELECT "o0"."ProductID"
        FROM "Order Details" AS "o0"
        WHERE "o"."OrderID" = "o0"."OrderID"
        ORDER BY "o0"."ProductID"
        LIMIT 1) = "p"."ProductID"
    WHERE "o"."OrderID" > 11000 AND "p"."ProductName" = 'Chai' AND "o"."OrderID" > (
        SELECT COUNT(*)
        FROM "Orders" AS "o1"
        WHERE "c"."CustomerID" = "o1"."CustomerID"))
""");
    }

    public override async Task Embedded_query_with_collection_navigation(bool async)
    {
        await base.Embedded_query_with_collection_navigation(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE EXISTS (
    SELECT 1
    FROM "Customers" AS "c0"
    WHERE "c0"."City" = 'London' AND "c0"."CustomerID" = "c"."CustomerID" AND (
        SELECT COUNT(*)
        FROM "Orders" AS "o"
        WHERE "c0"."CustomerID" = "o"."CustomerID") > 5)
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
