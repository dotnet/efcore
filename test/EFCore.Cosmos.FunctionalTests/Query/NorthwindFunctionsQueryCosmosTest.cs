// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Azure.Cosmos;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindFunctionsQueryCosmosTest : NorthwindFunctionsQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindFunctionsQueryCosmosTest(
        NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override Task Client_evaluation_of_uncorrelated_method_call(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Client_evaluation_of_uncorrelated_method_call(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (((c["$type"] = "OrderDetail") AND (c["UnitPrice"] < 7.0)) AND (10 < c["ProductID"]))
""");
            });

    public override async Task Order_by_length_twice(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Unsupported ORDER BY clause. Issue #27037.
            await Assert.ThrowsAsync<CosmosException>(() => base.Order_by_length_twice(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
ORDER BY LENGTH(c["id"]), c["id"]
""");
        }
    }

    public override async Task Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Order_by_length_twice_followed_by_projection_of_naked_collection_navigation(async));

        AssertSql();
    }

    public override Task Sum_over_round_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection(async));

    public override Task Sum_over_round_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_round_works_correctly_in_projection_2(async));

    public override Task Sum_over_truncate_works_correctly_in_projection(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection(async));

    public override Task Sum_over_truncate_works_correctly_in_projection_2(bool async)
        => AssertTranslationFailed(() => base.Sum_over_truncate_works_correctly_in_projection_2(async));

    public override async Task Where_functions_nested(bool async)
    {
        // Cosmos client evaluation. Issue #17246.
        await AssertTranslationFailed(() => base.Where_functions_nested(async));

        AssertSql();
    }

    public override Task Static_equals_nullable_datetime_compared_to_non_nullable(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_equals_nullable_datetime_compared_to_non_nullable(a);

                AssertSql(
                    """
@arg='1996-07-04T00:00:00'

SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND (c["OrderDate"] = @arg))
""");
            });

    public override Task Static_equals_int_compared_to_long(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Static_equals_int_compared_to_long(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["$type"] = "Order") AND false)
""");
            });

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
