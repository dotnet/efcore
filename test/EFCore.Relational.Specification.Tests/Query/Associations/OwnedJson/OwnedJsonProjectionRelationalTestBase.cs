// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedJson;

public abstract class OwnedJsonProjectionRelationalTestBase<TFixture> : OwnedNavigationsProjectionTestBase<TFixture>
    where TFixture : OwnedJsonRelationalFixtureBase, new()
{
    public OwnedJsonProjectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Select_required_related_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(
            queryTrackingBehavior,
            () => base.Select_required_related_via_optional_navigation(queryTrackingBehavior));

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
