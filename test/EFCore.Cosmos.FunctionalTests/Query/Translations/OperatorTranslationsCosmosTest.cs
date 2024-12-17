// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class OperatorTranslationsCosmosTest : OperatorTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public OperatorTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Bitwise

    public override Task Bitwise_or(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Bitwise_xor(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] ^ c["Short"]) = 1)
""",
                    //
                    """
SELECT VALUE (c["Int"] ^ c["Short"])
FROM root c
""");
            });

    public override async Task Bitwise_or_over_boolean(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Bitwise_or_over_boolean(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = 12) | (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task Bitwise_or_multiple(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Bitwise_or_multiple(async));

            AssertSql();
        }
    }

    public override Task Bitwise_and(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Bitwise_xor(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] ^ c["Short"]) = 1)
""",
                    //
                    """
SELECT VALUE (c["Int"] ^ c["Short"])
FROM root c
""");
            });

    public override async Task Bitwise_and_over_boolean(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Bitwise_and_over_boolean(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = 8) & (c["String"] = "Seattle"))
""");
        }
    }

    public override Task Bitwise_xor(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Bitwise_xor(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] ^ c["Short"]) = 1)
""",
                    //
                    """
SELECT VALUE (c["Int"] ^ c["Short"])
FROM root c
""");
            });

    public override Task Bitwise_xor_over_boolean(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Bitwise_xor_over_boolean(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = c["Short"]) != (c["String"] = "Seattle"))
""");
            });

    public override Task Bitwise_complement(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Bitwise_complement(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (~(c["Int"]) = -9)
""");
            });

    public override async Task Bitwise_and_or_over_boolean(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Bitwise_and_or_over_boolean(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 12) & (c["Short"] = 12)) | (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task Bitwise_or_with_logical_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Bitwise_or_with_logical_or(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 12) | (c["Short"] = 12)) OR (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task Bitwise_and_with_logical_and(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Bitwise_and_with_logical_and(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 8) & (c["Short"] = 8)) AND (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task Bitwise_or_with_logical_and(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Bitwise_or_with_logical_and(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 8) | (c["Short"] = 9)) AND (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task Bitwise_and_with_logical_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Bitwise_and_with_logical_or(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 12) & (c["Short"] = 12)) OR (c["String"] = "Seattle"))
""");
        }
    }

    #endregion Bitwise

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
