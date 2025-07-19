// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public class OwnedNavigationsStructuralEqualityCosmosTest : OwnedNavigationsStructuralEqualityTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsStructuralEqualityCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Two_related(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Two_related(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Two_nested(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Two_nested(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Not_equals(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Not_equals(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override Task Related_with_inline_null(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Related_with_inline_null(async));

    public override Task Related_with_parameter_null(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Related_with_parameter_null(async));

    public override Task Nested_with_inline_null(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_with_inline_null(async));

    public override async Task Nested_with_inline(bool async)
    {
        if (async)
        {
            await base.Nested_with_inline(async);

            AssertSql();
        }
    }

    public override async Task Nested_with_parameter(bool async)
    {
        if (async)
        {
            await base.Nested_with_parameter(async);

            AssertSql();
        }
    }

    public override Task Two_nested_collections(bool async)
        => Fixture.NoSyncTest(
            async, async a =>
            {
                await base.Two_nested_collections(a);

                AssertSql(
                    """
SELECT VALUE c
FROM root c
WHERE false
""");
            });

    public override async Task Nested_collection_with_inline(bool async)
    {
        if (async)
        {
            await base.Nested_collection_with_inline(async);

            AssertSql();
        }
    }

    public override async Task Nested_collection_with_parameter(bool async)
    {
        if (async)
        {
            await base.Nested_collection_with_parameter(async);

            AssertSql();
        }
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
