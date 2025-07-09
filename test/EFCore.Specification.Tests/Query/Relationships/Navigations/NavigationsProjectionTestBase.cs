// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public abstract class NavigationsProjectionTestBase<TFixture>(TFixture fixture)
    : RelationshipsProjectionTestBase<TFixture>(fixture)
        where TFixture : NavigationsFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_everything(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => from r in ss.Set<RelationshipsRoot>()
                  join t in ss.Set<RelationshipsTrunk>() on r.Id equals t.Id
                  join b in ss.Set<RelationshipsBranch>() on t.Id equals b.Id
                  join l in ss.Set<RelationshipsLeaf>() on b.Id equals l.Id
                  select new { r, t, b, l },
            elementSorter: e => e.r.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.r, a.r);
                AssertEqual(e.t, a.t);
                AssertEqual(e.b, a.b);
                AssertEqual(e.l, a.l);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_everything_using_joins(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => from r in ss.Set<RelationshipsRoot>()
                  join t in ss.Set<RelationshipsTrunk>() on r.Id equals t.Id
                  join b in ss.Set<RelationshipsBranch>() on t.Id equals b.Id
                  join l in ss.Set<RelationshipsLeaf>() on b.Id equals l.Id
                  select new { r, t, b, l },
            elementSorter: e => e.r.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.r, a.r);
                AssertEqual(e.t, a.t);
                AssertEqual(e.b, a.b);
                AssertEqual(e.l, a.l);
            },
            queryTrackingBehavior: queryTrackingBehavior);
}
