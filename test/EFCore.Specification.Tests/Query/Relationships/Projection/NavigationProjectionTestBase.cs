// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public abstract class NavigationProjectionTestBase<TFixture>(TFixture fixture)
    : ProjectionTestBase<TFixture>(fixture)
        where TFixture : NavigationRelationshipsFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_everything_using_joins(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<RelationshipsRootEntity>()
                  join t in ss.Set<RelationshipsTrunkEntity>() on r.Id equals t.Id
                  join b in ss.Set<RelationshipsBranchEntity>() on t.Id equals b.Id
                  join l in ss.Set<RelationshipsLeafEntity>() on b.Id equals l.Id
                  select new { r, t, b, l },
            elementSorter: e => e.r.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.r, a.r);
                AssertEqual(e.t, a.t);
                AssertEqual(e.b, a.b);
                AssertEqual(e.l, a.l);
            });
}
