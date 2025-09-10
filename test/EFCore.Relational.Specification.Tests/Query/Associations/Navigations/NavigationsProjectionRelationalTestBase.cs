// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsProjectionRelationalTestBase<TFixture>
    : NavigationsProjectionTestBase<TFixture>
    where TFixture : NavigationsRelationalFixtureBase, new()
{
    public NavigationsProjectionRelationalTestBase(TFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Traditional relational collections navigations projected from null instances are returned as empty collections rather than null.
    // This is in contrast to client evaluation behavior - and also the JSON collection behavior - where we get null instance (coalescing).
    public override Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated!.NestedCollection),
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalRelated!.NestedCollection ?? new List<NestedType>()),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
            queryTrackingBehavior: queryTrackingBehavior);

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
