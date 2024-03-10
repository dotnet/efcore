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

    public override Task Where_datetimeoffset_now_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_now_component(async));

    public override Task Where_datetimeoffset_utcnow_component(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_utcnow_component(async));

    public override Task Where_datetimeoffset_utcnow(bool async)
        => AssertTranslationFailed(() => base.Where_datetimeoffset_utcnow_component(async));

    public override async Task<string> Where_simple_closure(bool async)
    {
        var queryString = await base.Where_simple_closure(async);

        AssertSql(
            """
@__city_0='London' (Size = 6)

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE "c"."City" = @__city_0
""");

        Assert.Equal(
            @".param set @__city_0 'London'

SELECT ""c"".""CustomerID"", ""c"".""Address"", ""c"".""City"", ""c"".""CompanyName"", ""c"".""ContactName"", ""c"".""ContactTitle"", ""c"".""Country"", ""c"".""Fax"", ""c"".""Phone"", ""c"".""PostalCode"", ""c"".""Region""
FROM ""Customers"" AS ""c""
WHERE ""c"".""City"" = @__city_0", queryString, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);

        return null;
    }

    public override async Task Where_datetime_now(bool async)
    {
        await base.Where_datetime_now(async);

        AssertSql(
            """
@__myDatetime_0='2015-04-10T00:00:00.0000000' (DbType = DateTime)

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime'), '0'), '.') <> @__myDatetime_0
""");
    }

    public override async Task Where_datetime_utcnow(bool async)
    {
        await base.Where_datetime_utcnow(async);

        AssertSql(
            """
@__myDatetime_0='2015-04-10T00:00:00.0000000' (DbType = DateTime)

SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now'), '0'), '.') <> @__myDatetime_0
""");
    }

    public override async Task Where_datetime_today(bool async)
    {
        await base.Where_datetime_today(async);

        AssertSql(
            """
SELECT "e"."EmployeeID", "e"."City", "e"."Country", "e"."FirstName", "e"."ReportsTo", "e"."Title"
FROM "Employees" AS "e"
WHERE rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime', 'start of day'), '0'), '.') = rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime', 'start of day'), '0'), '.')
""");
    }

    public override async Task Where_datetime_date_component(bool async)
    {
        await base.Where_datetime_date_component(async);

        AssertSql(
            """
@__myDatetime_0='1998-05-04T00:00:00.0000000' (DbType = DateTime)

SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", 'start of day'), '0'), '.') = @__myDatetime_0
""");
    }

    public override async Task Where_datetime_year_component(bool async)
    {
        await base.Where_datetime_year_component(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE CAST(strftime('%Y', "o"."OrderDate") AS INTEGER) = 1998
""");
    }

    public override async Task Where_datetime_month_component(bool async)
    {
        await base.Where_datetime_month_component(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE CAST(strftime('%m', "o"."OrderDate") AS INTEGER) = 4
""");
    }

    public override async Task Where_datetime_dayOfYear_component(bool async)
    {
        await base.Where_datetime_dayOfYear_component(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE CAST(strftime('%j', "o"."OrderDate") AS INTEGER) = 68
""");
    }

    public override async Task Where_datetime_day_component(bool async)
    {
        await base.Where_datetime_day_component(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE CAST(strftime('%d', "o"."OrderDate") AS INTEGER) = 4
""");
    }

    public override async Task Where_datetime_hour_component(bool async)
    {
        await base.Where_datetime_hour_component(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE CAST(strftime('%H', "o"."OrderDate") AS INTEGER) = 0
""");
    }

    public override async Task Where_datetime_minute_component(bool async)
    {
        await base.Where_datetime_minute_component(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE CAST(strftime('%M', "o"."OrderDate") AS INTEGER) = 0
""");
    }

    public override async Task Where_datetime_second_component(bool async)
    {
        await base.Where_datetime_second_component(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE CAST(strftime('%S', "o"."OrderDate") AS INTEGER) = 0
""");
    }

    public override async Task Where_datetime_millisecond_component(bool async)
    {
        await base.Where_datetime_millisecond_component(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE (CAST(strftime('%f', "o"."OrderDate") AS REAL) * 1000.0) % 1000.0 = 0.0
""");
    }

    public override async Task Where_string_length(bool async)
    {
        await base.Where_string_length(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE length("c"."City") = 6
""");
    }

    public override async Task Where_string_indexof(bool async)
    {
        await base.Where_string_indexof(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE instr("c"."City", 'Sea') - 1 <> -1 OR "c"."City" IS NULL
""");
    }

    public override async Task Where_string_replace(bool async)
    {
        await base.Where_string_replace(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE replace("c"."City", 'Sea', 'Rea') = 'Reattle'
""");
    }

    public override async Task Where_string_substring(bool async)
    {
        await base.Where_string_substring(async);

        AssertSql(
            """
SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
FROM "Customers" AS "c"
WHERE substr("c"."City", 1 + 1, 2) = 'ea'
""");
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

    public override async Task Like_with_non_string_column_using_ToString(bool async)
    {
        await base.Like_with_non_string_column_using_ToString(async);

        AssertSql(
            """
SELECT "o"."OrderID", "o"."CustomerID", "o"."EmployeeID", "o"."OrderDate"
FROM "Orders" AS "o"
WHERE CAST("o"."OrderID" AS TEXT) LIKE '%20%'
""");
    }

    public override async Task Where_bitwise_xor(bool async)
    {
        // Cannot eval 'where (([c].CustomerID == \"ALFKI\") ^ True)'. Issue #16645.
        await AssertTranslationFailed(() => base.Where_bitwise_xor(async));

        AssertSql();
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
