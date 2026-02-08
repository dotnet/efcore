// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public class OwnedNavigationsStructuralEqualityCosmosTest : OwnedNavigationsStructuralEqualityTestBase<OwnedNavigationsCosmosFixture>
{
    public OwnedNavigationsStructuralEqualityCosmosTest(OwnedNavigationsCosmosFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Two_associates()
    {
        await base.Two_associates();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE false
""");
    }

    public override async Task Two_nested_associates()
    {
        await base.Two_nested_associates();

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

    public override async Task Associate_with_inline_null()
    {
        await base.Associate_with_inline_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["OptionalAssociate"] = null)
""");
    }

    public override Task Associate_with_parameter_null()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Associate_with_parameter_null());

    public override async Task Nested_associate_with_inline_null()
    {
        await base.Nested_associate_with_inline_null();

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE (c["RequiredAssociate"]["OptionalNestedAssociate"] = null)
""");
    }

    public override async Task Nested_associate_with_inline()
    {
        await base.Nested_associate_with_inline();

        AssertSql();
    }

    public override async Task Nested_associate_with_parameter()
    {
        await base.Nested_associate_with_parameter();

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

    #region Contains

    [ConditionalFact]
    public async Task Contains_with_inline_null()
    {
        await AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RequiredAssociate.NestedCollection.Contains(null!)), assertEmpty: true);

        AssertSql(
            """
SELECT VALUE c
FROM root c
WHERE EXISTS (
    SELECT 1
    FROM n IN c["RequiredAssociate"]["NestedCollection"]
    WHERE false)
""");
    }

    public override async Task Contains_with_inline()
    {
        // No backing field could be found for property 'RootEntity.RequiredRelated#RelatedType.NestedCollection#NestedType.RelatedTypeRootEntityId' and the property does not have a getter.
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Contains_with_inline());

        AssertSql();
    }

    public override async Task Contains_with_parameter()
    {
        await AssertTranslationFailed(base.Contains_with_parameter);

        AssertSql();
    }

    public override async Task Contains_with_operators_composed_on_the_collection()
    {
        await AssertTranslationFailed(base.Contains_with_operators_composed_on_the_collection);

        AssertSql();
    }

    public override async Task Contains_with_nested_and_composed_operators()
    {
        await AssertTranslationFailed(base.Contains_with_nested_and_composed_operators);

        AssertSql();
    }

    #endregion Contains

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
