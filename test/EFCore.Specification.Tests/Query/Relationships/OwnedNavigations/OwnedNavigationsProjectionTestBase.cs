// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public abstract class OwnedNavigationsProjectionTestBase<TFixture>(TFixture fixture)
    : RelationshipsProjectionTestBase<TFixture>(fixture)
        where TFixture : OwnedNavigationsFixtureBase, new()
{
    public override Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_root(async, queryTrackingBehavior);

    public override Task Select_trunk_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_optional(async, queryTrackingBehavior));

    public override Task Select_trunk_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_required(async, queryTrackingBehavior));

    public override Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_collection(async, queryTrackingBehavior));

    public override Task Select_branch_required_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_required_required(async, queryTrackingBehavior));

    public override Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_required_optional(async, queryTrackingBehavior));

    public override Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_optional_required(async, queryTrackingBehavior));

    public override Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_optional_optional(async, queryTrackingBehavior));

    public override Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_required_collection(async, queryTrackingBehavior));

    public override Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_branch_optional_collection(async, queryTrackingBehavior));

    #region Multiple

    public override Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => base.Select_root_duplicated(async, queryTrackingBehavior);

    public override Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_and_branch_duplicated(async, queryTrackingBehavior));

    public override Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_trunk_and_trunk_duplicated(async, queryTrackingBehavior));

    public override Task Select_leaf_trunk_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_leaf_trunk_root(async, queryTrackingBehavior));

    public override Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_multiple_branch_leaf(async, queryTrackingBehavior));

    #endregion Multiple

    #region Subquery

    public override Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_required_trunk_FirstOrDefault_branch(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_trunk_FirstOrDefault_collection(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(async, queryTrackingBehavior));

    public override Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(async, queryTrackingBehavior));

    #endregion Subquery

    #region SelectMany

    public override Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.SelectMany_trunk_collection(async, queryTrackingBehavior));

    public override Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.SelectMany_required_trunk_reference_branch_collection(async, queryTrackingBehavior));

    public override Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertCantTrackOwned(queryTrackingBehavior, () => base.SelectMany_optional_trunk_reference_branch_collection(async, queryTrackingBehavior));

    #endregion SelectMany

    private async Task AssertCantTrackOwned(QueryTrackingBehavior queryTrackingBehavior, Func<Task> test)
    {
        if (queryTrackingBehavior is QueryTrackingBehavior.TrackAll)
        {
            var message = (await Assert.ThrowsAsync<InvalidOperationException>(test)).Message;

            Assert.Equal(CoreStrings.OwnedEntitiesCannotBeTrackedWithoutTheirOwner, message);

            return;
        }

        await test();
    }
}
