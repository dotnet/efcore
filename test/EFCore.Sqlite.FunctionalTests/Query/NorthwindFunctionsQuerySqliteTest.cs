// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindFunctionsQuerySqliteTest : NorthwindFunctionsQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindFunctionsQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Client_evaluation_of_uncorrelated_method_call(bool async)
    {
        await base.Client_evaluation_of_uncorrelated_method_call(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."ProductID", "o"."Discount", "o"."Quantity", "o"."UnitPrice"
FROM "Order Details" AS "o"
WHERE ef_compare("o"."UnitPrice", '7.0') < 0 AND 10 < "o"."ProductID"
""");
    }

    public override async Task Order_by_length_twice(bool async)
    {
        await base.Order_by_length_twice(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
ORDER BY length("c"."CustomerID"), "c"."CustomerID"
""");
    }

    public override async Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
    {
        await base.Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Customers" AS "c"
LEFT JOIN "Orders" AS "o" ON "c"."CustomerID" = "o"."CustomerID"
ORDER BY length("c"."CustomerID"), "c"."CustomerID"
""");
    }

    public override async Task Where_functions_nested(bool async)
    {
        await base.Where_functions_nested(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE pow(CAST(length("c"."CustomerID") AS REAL), 2.0) = 25.0
""");
    }

    public override Task Sum_over_round_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection(async));

    public override Task Sum_over_round_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection_2(async));

    public override Task Sum_over_truncate_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection(async));

    public override Task Sum_over_truncate_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection_2(async));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
