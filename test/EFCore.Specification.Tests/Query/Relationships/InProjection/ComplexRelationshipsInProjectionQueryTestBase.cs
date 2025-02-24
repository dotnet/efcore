// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public abstract class ComplexRelationshipsInProjectionQueryTestBase<TFixture>(TFixture fixture)
    : RelationshipsInProjectionQueryTestBase<TFixture>(fixture)
        where TFixture : ComplexRelationshipsQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_everything(bool async)
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
    public override Task Project_branch_required_optional(bool async)
        => base.Project_branch_required_optional(async);

    [ConditionalTheory(Skip = "issue #31237")]
    public override Task Project_branch_required_collection(bool async)
        => base.Project_branch_required_collection(async);

    [ConditionalTheory(Skip = "issue #31376")]
    public override Task Project_branch_optional_optional(bool async)
        => base.Project_branch_optional_optional(async);

    [ConditionalTheory(Skip = "issue #31412")]
    public override Task Project_branch_optional_required(bool async)
        => base.Project_branch_optional_required(async);

    [ConditionalTheory(Skip = "issue #31237")]
    public override Task Project_branch_optional_collection(bool async)
        => base.Project_branch_optional_collection(async);
}
