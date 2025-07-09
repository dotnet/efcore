// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.ComplexTableSplitting;

public abstract class ComplexTableSplittingProjectionRelationalTestBase<TFixture>(TFixture fixture)
    : RelationshipsProjectionTestBase<TFixture>(fixture)
    where TFixture : ComplexTableSplittingRelationalFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_everything(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => from r in ss.Set<RelationshipsRoot>()
                  join t in ss.Set<RelationshipsTrunk>() on r.Id equals t.Id
                  select new { r, t },
            elementSorter: e => e.r.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.r, a.r);
                AssertEqual(e.t, a.t);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_trunk_collection(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex types projected via optional navigations, #31412")]
    public override Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_branch_required_optional(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex types projected via optional navigations, #31412")]
    public override Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_branch_optional_optional(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_branch_required_collection(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_branch_optional_collection(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex types projected via optional navigations, #31412")]
    public override Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_branch_optional_required(async, queryTrackingBehavior);

    #region Multiple

    [ConditionalTheory(Skip = "Complex types projected via optional navigations, #31412")]
    public override Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_trunk_and_branch_duplicated(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex types projected via optional navigations, #31412")]
    public override Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_trunk_and_trunk_duplicated(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_multiple_branch_leaf(async, queryTrackingBehavior);

    #endregion Multiple

    #region Subquery

    [ConditionalTheory(Skip = "Complex types projected via optional navigations, #31412")]
    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex types projected via optional navigations, #31412")]
    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async, queryTrackingBehavior);

    #endregion Subquery

    #region SelectMany

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.SelectMany_trunk_collection(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.SelectMany_required_trunk_reference_branch_collection(async, queryTrackingBehavior);

    [ConditionalTheory(Skip = "Complex collections not yet supported, #31237")]
    public override Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.SelectMany_optional_trunk_reference_branch_collection(async, queryTrackingBehavior);

    #endregion SelectMany
}
