// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsCollectionRelationalTestBase<TFixture> : OwnedNavigationsCollectionTestBase<TFixture>
    where TFixture : OwnedNavigationsRelationalFixtureBase, new()
{
    public OwnedNavigationsCollectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Distinct_over_projected_nested_collection()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(base.Distinct_over_projected_nested_collection);

        Assert.Equal(RelationalStrings.DistinctOnCollectionNotSupported, exception.Message);
    }

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(base.Distinct_over_projected_filtered_nested_collection);

        Assert.Equal(RelationalStrings.DistinctOnCollectionNotSupported, exception.Message);
    }

    public void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
