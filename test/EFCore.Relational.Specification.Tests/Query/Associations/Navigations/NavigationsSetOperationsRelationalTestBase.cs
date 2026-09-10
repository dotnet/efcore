// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsSetOperationsRelationalTestBase<TFixture> : NavigationsSetOperationsTestBase<TFixture>
    where TFixture : NavigationsRelationalFixtureBase, new()
{
    public NavigationsSetOperationsRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Over_associate_collection_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        // #33485, #34849
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Over_associate_collection_projected(queryTrackingBehavior));

        Assert.Equal(
            RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin,
            exception.Message);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
