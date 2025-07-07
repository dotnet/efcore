// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query.Translations.Operators;

public class BitwiseOperatorTranslationsCosmosTest : BitwiseOperatorTranslationsTestBase<BasicTypesQueryCosmosFixture>
{
    public BitwiseOperatorTranslationsCosmosTest(BasicTypesQueryCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Or(bool async)
        => AssertTranslationFailed(() => base.Or(async));

    public override async Task Or_over_boolean(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Or_over_boolean(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = 12) | (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task Or_multiple(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<InvalidOperationException>(() => base.Or_multiple(async));

            AssertSql();
        }
    }

    public override Task And(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.And(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] & c["Short"]) = 2)
""",
                    //
                    """
SELECT VALUE (c["Int"] & c["Short"])
FROM root c
""");
            });

    public override async Task And_over_boolean(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.And_over_boolean(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = 8) & (c["String"] = "Seattle"))
""");
        }
    }

    public override Task Xor(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Xor(a);

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

    public override Task Xor_over_boolean(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Xor_over_boolean(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] = c["Short"]) != (c["String"] = "Seattle"))
""");
            });

    public override Task Complement(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Complement(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE (~(c["Int"]) = -9)
""");
            });

    public override async Task And_or_over_boolean(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.And_or_over_boolean(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 12) & (c["Short"] = 12)) | (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task Or_with_logical_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Or_with_logical_or(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 12) | (c["Short"] = 12)) OR (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task And_with_logical_and(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.And_with_logical_and(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 8) & (c["Short"] = 8)) AND (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task Or_with_logical_and(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.Or_with_logical_and(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 8) | (c["Short"] = 9)) AND (c["String"] = "Seattle"))
""");
        }
    }

    public override async Task And_with_logical_or(bool async)
    {
        // Always throws for sync.
        if (async)
        {
            // Bitwise operators on booleans. Issue #13168.
            await Assert.ThrowsAsync<EqualException>(() => base.And_with_logical_or(async));

            AssertSql(
                """
SELECT VALUE c
FROM root c
WHERE (((c["Int"] = 12) & (c["Short"] = 12)) OR (c["String"] = "Seattle"))
""");
        }
    }

    public override Task Left_shift(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Left_shift(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] << 1) = 16)
""");
            });

    public override Task Right_shift(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Right_shift(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE ((c["Int"] >> 1) = 4)
""");
            });

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
