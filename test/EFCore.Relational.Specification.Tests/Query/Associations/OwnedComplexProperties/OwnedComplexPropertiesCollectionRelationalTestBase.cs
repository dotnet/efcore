// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public abstract class OwnedComplexPropertiesCollectionRelationalTestBase<TFixture> : OwnedComplexPropertiesCollectionTestBase<TFixture>
    where TFixture : OwnedComplexPropertiesRelationalFixtureBase, new()
{
    public OwnedComplexPropertiesCollectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // The owned associate collection is unordered, but the nested collection is a JSON complex collection, which is ordered
    public override Task Index_on_nested_collection()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection[0].Int == 8),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection.Count > 0
                && e.RequiredAssociate.NestedCollection[0].Int == 8));

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(base.Distinct_over_projected_filtered_nested_collection);

        Assert.Equal(RelationalStrings.DistinctOnCollectionNotSupported, exception.Message);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
