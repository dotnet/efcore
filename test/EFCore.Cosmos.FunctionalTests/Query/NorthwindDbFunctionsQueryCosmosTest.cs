// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindDbFunctionsQueryCosmosTest : NorthwindDbFunctionsQueryTestBase<NorthwindQueryCosmosFixture<NoopModelCustomizer>>
{
    public NorthwindDbFunctionsQueryCosmosTest(
        NorthwindQueryCosmosFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    public override async Task Like_all_literals(bool async)
    {
        await AssertTranslationFailed(() => base.Like_all_literals(async));

        AssertSql();
    }

    public override async Task Like_all_literals_with_escape(bool async)
    {
        await AssertTranslationFailed(() => base.Like_all_literals_with_escape(async));

        AssertSql();
    }

    public override async Task Like_literal(bool async)
    {
        await AssertTranslationFailed(() => base.Like_literal(async));

        AssertSql();
    }

    public override async Task Like_literal_with_escape(bool async)
    {
        await AssertTranslationFailed(() => base.Like_literal_with_escape(async));

        AssertSql();
    }

    public override async Task Like_identity(bool async)
    {
        await AssertTranslationFailed(() => base.Like_identity(async));

        AssertSql();
    }

    public override Task Random_return_less_than_1(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Random_return_less_than_1(async);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (RAND() < 1.0))
""");
            });

    public override Task Random_return_greater_than_0(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Random_return_greater_than_0(async);

                AssertSql(
                    """
SELECT COUNT(1) AS c
FROM root c
WHERE ((c["Discriminator"] = "Order") AND (RAND() >= 0.0))
""");
            });

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
