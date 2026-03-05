// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexJson;

public abstract class ComplexJsonCollectionRelationalTestBase<TFixture> : ComplexPropertiesCollectionTestBase<TFixture>
    where TFixture : ComplexJsonRelationalFixtureBase, new()
{
    public ComplexJsonCollectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Distinct_projected(QueryTrackingBehavior queryTrackingBehavior)
    {
        // #36421 - support projecting out complex JSON types after Distinct
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Distinct_projected(queryTrackingBehavior));

        Assert.Equal(RelationalStrings.InsufficientInformationToIdentifyElementOfCollectionJoin, exception.Message);
    }

    public override async Task Distinct_over_projected_filtered_nested_collection()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(base.Distinct_over_projected_filtered_nested_collection);

        Assert.Equal(RelationalStrings.DistinctOnCollectionNotSupported, exception.Message);
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
