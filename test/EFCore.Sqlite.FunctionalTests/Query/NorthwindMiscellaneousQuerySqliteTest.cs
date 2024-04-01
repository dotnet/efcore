// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Internal;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindMiscellaneousQuerySqliteTest : NorthwindMiscellaneousQueryRelationalTestBase<
    NorthwindQuerySqliteFixture<NoopModelCustomizer>>
{
    public NorthwindMiscellaneousQuerySqliteTest(
        NorthwindQuerySqliteFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Query_expression_with_to_string_and_contains(bool async)
    {
        await base.Query_expression_with_to_string_and_contains(async);

        AssertSql(
            """
SELECT "o"."CustomerID"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL AND "o"."EmployeeID" IS NOT NULL AND instr(CAST("o"."EmployeeID" AS TEXT), '7') > 0
""");
    }

    public override async Task Take_Skip(bool async)
    {
        await base.Take_Skip(async);

        AssertSql(
            """
@__p_0='10'
@__p_1='5'

SELECT "c0"."CustomerID", "c0"."Address", "c0"."City", "c0"."CompanyName", "c0"."ContactName", "c0"."ContactTitle", "c0"."Country", "c0"."Fax", "c0"."Phone", "c0"."PostalCode", "c0"."Region"
FROM (
    SELECT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
    FROM "Customers" AS "c"
    ORDER BY "c"."ContactName"
    LIMIT @__p_0
) AS "c0"
ORDER BY "c0"."ContactName"
LIMIT -1 OFFSET @__p_1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Select_datetime_millisecond_component_composed(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.AddYears(1).Millisecond));

        AssertSql(
            """
SELECT (CAST(strftime('%f', "o"."OrderDate", CAST(1 AS TEXT) || ' years') AS REAL) * 1000.0) % 1000.0
FROM "Orders" AS "o"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Select_datetime_TimeOfDay_component_composed(bool async)
    {
        await AssertQueryScalar(
            async,
            ss => ss.Set<Order>().Select(o => o.OrderDate.Value.AddYears(1).TimeOfDay));

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%H:%M:%f', "o"."OrderDate", CAST(1 AS TEXT) || ' years'), '0'), '.')
FROM "Orders" AS "o"
""");
    }

    public override async Task Select_expression_date_add_year(bool async)
    {
        await base.Select_expression_date_add_year(async);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", CAST(1 AS TEXT) || ' years'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_month(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>()
                .Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddMonths(1) }),
            e => e.OrderDate,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.OrderDate.HasValue, a.OrderDate.HasValue);
                if (e.OrderDate.HasValue && a.OrderDate.HasValue)
                {
                    // difference between how Sqlite and everyone else add months
                    // e.g. when adding 1 month to Jan 31st, we get March 2/3 on Sqlite and Feb 28th/29ths for everyone else
                    // see notes on issue #25851 for more details
                    var diff = (e.OrderDate - a.OrderDate).Value;
                    Assert.True(diff.Days is >= -3 and <= 0);
                    Assert.Equal(0, diff.Hours);
                    Assert.Equal(0, diff.Minutes);
                    Assert.Equal(0, diff.Seconds);
                    Assert.Equal(0, diff.Milliseconds);
                    Assert.Equal(0, diff.Microseconds);
                }
            });

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", CAST(1 AS TEXT) || ' months'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_hour(bool async)
    {
        await base.Select_expression_datetime_add_hour(async);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", CAST(1.0 AS TEXT) || ' hours'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_minute(bool async)
    {
        await base.Select_expression_datetime_add_minute(async);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", CAST(1.0 AS TEXT) || ' minutes'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_second(bool async)
    {
        await base.Select_expression_datetime_add_second(async);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", CAST(1.0 AS TEXT) || ' seconds'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Select_expression_datetime_add_ticks(bool async)
    {
        // modifying the original scenario - Sqlite gives inaccurate results for values of granularity less than 1 second
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new Order { OrderDate = o.OrderDate.Value.AddTicks(10 * TimeSpan.TicksPerSecond) }),
            e => e.OrderDate);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", CAST(100000000 / 10000000 AS TEXT) || ' seconds'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Select_expression_date_add_milliseconds_above_the_range(bool async)
    {
        await base.Select_expression_date_add_milliseconds_above_the_range(async);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", CAST(1000000000000.0 / 1000.0 AS TEXT) || ' seconds'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Select_expression_date_add_milliseconds_below_the_range(bool async)
    {
        await base.Select_expression_date_add_milliseconds_below_the_range(async);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", CAST(-1000000000000.0 / 1000.0 AS TEXT) || ' seconds'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Select_expression_date_add_milliseconds_large_number_divided(bool async)
    {
        await base.Select_expression_date_add_milliseconds_large_number_divided(async);

        AssertSql(
            """
@__millisecondsPerDay_0='86400000'

SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', "o"."OrderDate", COALESCE(CAST(CAST(CAST((CAST(strftime('%f', "o"."OrderDate") AS REAL) * 1000.0) % 1000.0 AS INTEGER) / @__millisecondsPerDay_0 AS REAL) AS TEXT), '') || ' days', COALESCE(CAST(CAST(CAST((CAST(strftime('%f', "o"."OrderDate") AS REAL) * 1000.0) % 1000.0 AS INTEGER) % @__millisecondsPerDay_0 AS REAL) / 1000.0 AS TEXT), '') || ' seconds'), '0'), '.') AS "OrderDate"
FROM "Orders" AS "o"
WHERE "o"."OrderDate" IS NOT NULL
""");
    }

    public override async Task Add_minutes_on_constant_value(bool async)
    {
        await base.Add_minutes_on_constant_value(async);

        AssertSql(
            """
SELECT rtrim(rtrim(strftime('%Y-%m-%d %H:%M:%f', '1900-01-01 00:00:00', CAST(CAST("o"."OrderID" % 25 AS REAL) AS TEXT) || ' minutes'), '0'), '.') AS "Test"
FROM "Orders" AS "o"
WHERE "o"."OrderID" < 10500
ORDER BY "o"."OrderID"
""");
    }

    public override async Task Select_distinct_long_count(bool async)
    {
        await base.Select_distinct_long_count(async);

        AssertSql(
            """
SELECT COUNT(*)
FROM (
    SELECT DISTINCT "c"."CustomerID", "c"."Address", "c"."City", "c"."CompanyName", "c"."ContactName", "c"."ContactTitle", "c"."Country", "c"."Fax", "c"."Phone", "c"."PostalCode", "c"."Region"
    FROM "Customers" AS "c"
) AS "c0"
""");
    }

    public override async Task Select_orderBy_skip_long_count(bool async)
    {
        await base.Select_orderBy_skip_long_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT 1
    FROM "Customers" AS "c"
    ORDER BY "c"."Country"
    LIMIT -1 OFFSET @__p_0
) AS "c0"
""");
    }

    public override async Task Select_orderBy_take_long_count(bool async)
    {
        await base.Select_orderBy_take_long_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT 1
    FROM "Customers" AS "c"
    ORDER BY "c"."Country"
    LIMIT @__p_0
) AS "c0"
""");
    }

    public override async Task Select_skip_long_count(bool async)
    {
        await base.Select_skip_long_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT 1
    FROM "Customers" AS "c"
    LIMIT -1 OFFSET @__p_0
) AS "c0"
""");
    }

    public override async Task Select_take_long_count(bool async)
    {
        await base.Select_take_long_count(async);

        AssertSql(
            """
@__p_0='7'

SELECT COUNT(*)
FROM (
    SELECT 1
    FROM "Customers" AS "c"
    LIMIT @__p_0
) AS "c0"
""");
    }

    public override Task Complex_nested_query_doesnt_try_binding_to_grandparent_when_parent_returns_complex_result(bool async)
        => null;

    public override Task SelectMany_correlated_subquery_hard(bool async)
        => null;

    public override async Task Concat_string_int(bool async)
    {
        await base.Concat_string_int(async);

        AssertSql(
            """
SELECT CAST("o"."OrderID" AS TEXT) || COALESCE("o"."CustomerID", '')
FROM "Orders" AS "o"
""");
    }

    public override async Task Concat_int_string(bool async)
    {
        await base.Concat_int_string(async);

        AssertSql(
            """
SELECT COALESCE("o"."CustomerID", '') || CAST("o"."OrderID" AS TEXT)
FROM "Orders" AS "o"
""");
    }

    public override async Task Concat_parameter_string_int(bool async)
    {
        await base.Concat_parameter_string_int(async);

        AssertSql(
            """
@__parameter_0='-' (Size = 1)

SELECT @__parameter_0 || CAST("o"."OrderID" AS TEXT)
FROM "Orders" AS "o"
""");
    }

    public override async Task Concat_constant_string_int(bool async)
    {
        await base.Concat_constant_string_int(async);

        AssertSql(
            """
SELECT '-' || CAST("o"."OrderID" AS TEXT)
FROM "Orders" AS "o"
""");
    }

    public override async Task Client_code_using_instance_method_throws(bool async)
        => Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodInstance(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQuerySqliteTest",
                "InstanceMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_method_throws(async))).Message);

    public override async Task Client_code_using_instance_in_static_method(bool async)
        => Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQuerySqliteTest",
                "StaticMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_static_method(async))).Message);

    public override async Task Client_code_using_instance_in_anonymous_type(bool async)
        => Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInTree(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQuerySqliteTest"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_anonymous_type(async))).Message);

    public override async Task Client_code_unknown_method(bool async)
        => await AssertTranslationFailedWithDetails(
            () => base.Client_code_unknown_method(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryTestBase<Microsoft.EntityFrameworkCore.Query.NorthwindQuerySqliteFixture<Microsoft.EntityFrameworkCore.TestUtilities.NoopModelCustomizer>>",
                nameof(UnknownMethod)));

    public override async Task Entity_equality_through_subquery_composite_key(bool async)
        => Assert.Equal(
            CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported("==", nameof(OrderDetail)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Entity_equality_through_subquery_composite_key(async))).Message);

    public override async Task DefaultIfEmpty_in_subquery_nested_filter_order_comparison(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.DefaultIfEmpty_in_subquery_nested_filter_order_comparison(async))).Message);

    public override async Task Select_subquery_recursive_trivial(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_subquery_recursive_trivial(async))).Message);

    public override async Task Select_correlated_subquery_ordered(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Select_correlated_subquery_ordered(async))).Message);

    public override async Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(
        bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_distinct_without_default_identifiers_projecting_columns_with_navigation(async)))
            .Message);

    public override async Task Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(bool async)
        => Assert.Equal(
            SqliteStrings.ApplyNotSupported,
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Correlated_collection_with_distinct_without_default_identifiers_projecting_columns(async))).Message);

    public override Task Max_on_empty_sequence_throws(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Max_on_empty_sequence_throws(async));

    [ConditionalFact]
    public async Task Single_Predicate_Cancellation()
        => await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () =>
                await Single_Predicate_Cancellation_test(Fixture.TestSqlLoggerFactory.CancelQuery()));

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
