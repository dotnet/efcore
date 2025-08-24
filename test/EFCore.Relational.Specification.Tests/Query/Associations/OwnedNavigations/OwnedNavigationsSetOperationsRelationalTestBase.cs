// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsSetOperationsRelationalTestBase<TFixture> : OwnedNavigationsSetOperationsTestBase<TFixture>
    where TFixture : OwnedNavigationsRelationalFixtureBase, new()
{
    public OwnedNavigationsSetOperationsRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task On_related_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        // #33485, #34849
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.On_related_projected(queryTrackingBehavior));

        Assert.Equal(
            RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin,
            exception.Message);
    }

    public override async Task Over_different_collection_properties()
    {
        // In owned navigation, different properties are modeled as different structural types even if they share the same CLR type.
        // As a result, their model definitions might differ (e.g. shadow properties) and we don't currently support set operations over them.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(base.Over_different_collection_properties);

        Assert.Equal(
            RelationalStrings.SetOperationOverDifferentStructuralTypes(
                "RootEntity.RequiredRelated#RelatedType.NestedCollection#NestedType",
                "RootEntity.OptionalRelated#RelatedType.NestedCollection#NestedType"),
            exception.Message);
    }

    public void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
