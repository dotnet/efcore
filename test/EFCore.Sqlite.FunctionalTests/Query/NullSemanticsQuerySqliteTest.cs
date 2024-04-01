// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NullSemanticsQuerySqliteTest : NullSemanticsQueryTestBase<NullSemanticsQuerySqliteFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public NullSemanticsQuerySqliteTest(NullSemanticsQuerySqliteFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Null_semantics_contains_non_nullable_item_with_non_nullable_subquery(bool async)
    {
        await base.Null_semantics_contains_non_nullable_item_with_non_nullable_subquery(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."StringA" IN (
    SELECT "e0"."StringA"
    FROM "Entities2" AS "e0"
)
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."StringA" NOT IN (
    SELECT "e0"."StringA"
    FROM "Entities2" AS "e0"
)
""");
    }

    public override async Task Null_semantics_contains_nullable_item_with_non_nullable_subquery(bool async)
    {
        await base.Null_semantics_contains_nullable_item_with_non_nullable_subquery(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableStringA" IN (
    SELECT "e0"."StringA"
    FROM "Entities2" AS "e0"
)
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."NullableStringA" NOT IN (
    SELECT "e0"."StringA"
    FROM "Entities2" AS "e0"
) OR "e"."NullableStringA" IS NULL
""");
    }

    public override async Task Null_semantics_contains_non_nullable_item_with_nullable_subquery(bool async)
    {
        await base.Null_semantics_contains_non_nullable_item_with_nullable_subquery(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE "e"."StringA" IN (
    SELECT "e0"."NullableStringA"
    FROM "Entities2" AS "e0"
)
""",
            //
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT (COALESCE("e"."StringA" IN (
    SELECT "e0"."NullableStringA"
    FROM "Entities2" AS "e0"
), 0))
""");
    }

    public override async Task Null_semantics_contains_nullable_item_with_nullable_subquery(bool async)
    {
        await base.Null_semantics_contains_nullable_item_with_nullable_subquery(async);

        AssertSql(
            """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE EXISTS (
    SELECT 1
    FROM "Entities2" AS "e0"
    WHERE "e0"."NullableStringA" = "e"."NullableStringB" OR ("e0"."NullableStringA" IS NULL AND "e"."NullableStringB" IS NULL))
""",
                //
                """
SELECT "e"."Id"
FROM "Entities1" AS "e"
WHERE NOT EXISTS (
    SELECT 1
    FROM "Entities2" AS "e0"
    WHERE "e0"."NullableStringA" = "e"."NullableStringB" OR ("e0"."NullableStringA" IS NULL AND "e"."NullableStringB" IS NULL))
""");
    }

    public override async Task Bool_equal_nullable_bool_HasValue(bool async)
    {
        await base.Bool_equal_nullable_bool_HasValue(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" IS NOT NULL
""",
            //
            """
@__prm_0='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @__prm_0 = ("e"."NullableBoolA" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."BoolB" = ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Bool_equal_nullable_bool_compared_to_null(bool async)
    {
        await base.Bool_equal_nullable_bool_compared_to_null(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" IS NULL
""",
            //
            """
@__prm_0='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @__prm_0 = ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Bool_not_equal_nullable_bool_HasValue(bool async)
    {
        await base.Bool_not_equal_nullable_bool_HasValue(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" IS NULL
""",
            //
            """
@__prm_0='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @__prm_0 <> ("e"."NullableBoolA" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."BoolB" <> ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Bool_not_equal_nullable_int_HasValue(bool async)
    {
        await base.Bool_not_equal_nullable_int_HasValue(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableIntA" IS NULL
""",
            //
            """
@__prm_0='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @__prm_0 <> ("e"."NullableIntA" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."BoolB" <> ("e"."NullableIntA" IS NOT NULL)
""");
    }

    public override async Task Bool_not_equal_nullable_bool_compared_to_null(bool async)
    {
        await base.Bool_not_equal_nullable_bool_compared_to_null(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."NullableBoolA" IS NOT NULL
""",
            //
            """
@__prm_0='False'

SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE @__prm_0 <> ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Bool_logical_operation_with_nullable_bool_HasValue(bool async)
    {
        await base.Bool_logical_operation_with_nullable_bool_HasValue(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE 0
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE "e"."BoolB" | ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    public override async Task Comparison_compared_to_null_check_on_bool(bool async)
    {
        await base.Comparison_compared_to_null_check_on_bool(async);

        AssertSql(
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE ("e"."IntA" = "e"."IntB") <> ("e"."NullableBoolA" IS NOT NULL)
""",
            //
            """
SELECT "e"."Id", "e"."BoolA", "e"."BoolB", "e"."BoolC", "e"."IntA", "e"."IntB", "e"."IntC", "e"."NullableBoolA", "e"."NullableBoolB", "e"."NullableBoolC", "e"."NullableIntA", "e"."NullableIntB", "e"."NullableIntC", "e"."NullableStringA", "e"."NullableStringB", "e"."NullableStringC", "e"."StringA", "e"."StringB", "e"."StringC"
FROM "Entities1" AS "e"
WHERE ("e"."IntA" <> "e"."IntB") = ("e"."NullableBoolA" IS NOT NULL)
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override NullSemanticsContext CreateContext(bool useRelationalNulls = false)
    {
        var options = new DbContextOptionsBuilder(Fixture.CreateOptions());
        if (useRelationalNulls)
        {
            new SqliteDbContextOptionsBuilder(options).UseRelationalNulls();
        }

        var context = new NullSemanticsContext(options.Options);

        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        return context;
    }
}
