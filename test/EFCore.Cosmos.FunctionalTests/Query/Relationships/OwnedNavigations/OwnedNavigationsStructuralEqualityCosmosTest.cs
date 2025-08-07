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

    public override async Task Two_related()
    {
        await base.Two_related();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Two_nested()
    {
        await base.Two_nested();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Not_equals()
    {
        await base.Not_equals();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override Task Related_with_inline_null()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Related_with_inline_null());

    public override Task Related_with_parameter_null()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Related_with_parameter_null());

    public override Task Nested_with_inline_null()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_with_inline_null());

    public override async Task Nested_with_inline()
    {
        await base.Nested_with_inline();

        AssertSql();
    }

    public override async Task Nested_with_parameter()
    {
        await base.Nested_with_parameter();

        AssertSql();
    }

    public override async Task Two_nested_collections()
    {
        await base.Two_nested_collections();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Nested_collection_with_inline()
    {
        await base.Nested_collection_with_inline();

        AssertSql();
    }

    public override async Task Nested_collection_with_parameter()
    {
        await base.Nested_collection_with_parameter();

        AssertSql();
    }

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
