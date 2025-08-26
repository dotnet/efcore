// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedTableSplitting;

public abstract class OwnedTableSplittingProjectionRelationalTestBase<TFixture>
    : OwnedNavigationsProjectionTestBase<TFixture>
    where TFixture : OwnedTableSplittingRelationalFixtureBase, new()
{
    public OwnedTableSplittingProjectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Select_required_related_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(
            queryTrackingBehavior,
            () => base.Select_required_related_via_optional_navigation(queryTrackingBehavior));

    // Traditional relational collections navigations can't be compared reliably.
    // The failure below is because collections on from null instances are returned as empty collections rather than null; but
    // even disregarding that, elements in the collection don't preserve ordering and so can't be compared reliably.
    public override Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(
            queryTrackingBehavior,
            () => AssertQuery(
                ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated!.NestedCollection),
                ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated!.NestedCollection ?? new List<NestedType>()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
                queryTrackingBehavior: queryTrackingBehavior));

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
