// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public abstract class ComplexProjectionTestBase<TFixture>(TFixture fixture)
    : ReferenceProjectionTestBase<TFixture>(fixture)
        where TFixture : ComplexRelationshipsFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_everything(bool async)
        => AssertQuery(
            async,
            ss => from r in ss.Set<RelationshipsRootEntity>()
                  join t in ss.Set<RelationshipsTrunkEntity>() on r.Id equals t.Id
                  select new { r, t },
            elementSorter: e => e.r.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.r, a.r);
                AssertEqual(e.t, a.t);
            });

    [ConditionalTheory(Skip = "issue #31412")]
    public override Task Select_branch_required_optional(bool async)
        => base.Select_branch_required_optional(async);

    [ConditionalTheory(Skip = "issue #31376")]
    public override Task Select_branch_optional_optional(bool async)
        => base.Select_branch_optional_optional(async);

    [ConditionalTheory(Skip = "issue #31412")]
    public override Task Select_branch_optional_required(bool async)
        => base.Select_branch_optional_required(async);

    public override Task Select_root_duplicated(bool async)
        => base.Select_root_duplicated(async);

    [ConditionalTheory(Skip = "issue #31412")]
    public override Task Select_trunk_and_branch_duplicated(bool async)
        => base.Select_trunk_and_branch_duplicated(async);

    [ConditionalTheory(Skip = "issue #31412")]
    public override Task Select_trunk_and_trunk_duplicated(bool async)
        => base.Select_trunk_and_trunk_duplicated(async);

    [ConditionalTheory(Skip = "issue #31237")]
    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async);

    [ConditionalTheory(Skip = "issue #31412")]
    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async);
}
