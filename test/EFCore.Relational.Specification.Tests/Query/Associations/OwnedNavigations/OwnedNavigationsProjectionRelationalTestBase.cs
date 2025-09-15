// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsProjectionRelationalTestBase<TFixture>
    : OwnedNavigationsProjectionTestBase<TFixture>
    where TFixture : OwnedNavigationsRelationalFixtureBase, new()
{
    public OwnedNavigationsProjectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override Task Select_required_related_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(
            queryTrackingBehavior,
            () => base.Select_required_related_via_optional_navigation(queryTrackingBehavior));

    // Traditional relational collections navigations projected from null instances are returned as empty collections rather than null.
    // This is in contrast to client evaluation behavior - and also the JSON collection behavior - where we get null instance (coalescing).
    public override Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => queryTrackingBehavior is QueryTrackingBehavior.TrackAll
            ? base.Select_nested_collection_on_optional_related(queryTrackingBehavior)
            : AssertQuery(
                ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated!.NestedCollection),
                ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated!.NestedCollection ?? new List<NestedType>()),
                assertOrder: true,
                elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
                queryTrackingBehavior: queryTrackingBehavior);

    public void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
