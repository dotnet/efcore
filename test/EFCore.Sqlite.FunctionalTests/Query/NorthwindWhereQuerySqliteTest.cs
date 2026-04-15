// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindWhereQuerySqliteTest : NorthwindWhereQueryRelationalTestBase<NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindWhereQuerySqliteTest(NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Where_ternary_boolean_condition_negated(bool async)
    {
        await base.Where_ternary_boolean_condition_negated(async);

        AssertSql(
            """
SELECT "p"."ProductID", "p"."Discontinued", "p"."ProductName", "p"."SupplierID", "p"."UnitPrice", "p"."UnitsInStock"
FROM "Products" AS "p"
WHERE CASE
    WHEN "p"."UnitsInStock" >= 20 THEN 1
    ELSE 0
END
""");
    }

    public override async Task<string> Where_simple_closure(bool async)
    {
        var queryString = await base.Where_simple_closure(async);

        AssertSql(
            """
@city='London' (Size = 6)

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."City" = @city
""");

        Assert.Equal(
            """
.param set @city 'London'

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."City" = @city
""",
            queryString, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);

        return null;
    }

    public override async Task Decimal_cast_to_double_works(bool async)
    {
        await base.Decimal_cast_to_double_works(async);

        AssertSql(
            """
SELECT "p"."ProductID", "p"."Discontinued", "p"."ProductName", "p"."SupplierID", "p"."UnitPrice", "p"."UnitsInStock"
FROM "Products" AS "p"
WHERE CAST("p"."UnitPrice" AS REAL) > 100.0
""");
    }

    public override async Task Where_compare_constructed_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_not_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_multi_value_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
