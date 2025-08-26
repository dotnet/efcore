// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsProjectionTestBase<TFixture>(TFixture fixture) : AssociationsProjectionTestBase<TFixture>(fixture)
    where TFixture : OwnedNavigationsFixtureBase, new()
{
    #region Non-collection

    public override Task Select_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_related(queryTrackingBehavior));

    public override Task Select_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_optional_related(queryTrackingBehavior));

    public override Task Select_required_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_required_nested_on_required_related(queryTrackingBehavior));

    public override Task Select_optional_nested_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_optional_nested_on_required_related(queryTrackingBehavior));

    public override Task Select_required_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_required_nested_on_optional_related(queryTrackingBehavior));

    public override Task Select_optional_nested_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_optional_nested_on_optional_related(queryTrackingBehavior));

    #endregion Non-collection

    #region Collection

    public override Task Select_related_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_related_collection(queryTrackingBehavior));

    public override Task Select_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_nested_collection_on_required_related(queryTrackingBehavior));

    public override Task Select_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.Select_nested_collection_on_optional_related(queryTrackingBehavior));

    public override Task SelectMany_related_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(queryTrackingBehavior, () => base.SelectMany_related_collection(queryTrackingBehavior));

    public override Task SelectMany_nested_collection_on_required_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(
            queryTrackingBehavior, () => base.SelectMany_nested_collection_on_required_related(queryTrackingBehavior));

    public override Task SelectMany_nested_collection_on_optional_related(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(
            queryTrackingBehavior, () => base.SelectMany_nested_collection_on_optional_related(queryTrackingBehavior));

    #endregion Collection

    #region Multiple

    // [ConditionalTheory]
    // [MemberData(nameof(AsyncAndTrackingData))]
    // public virtual Task Select_trunk_and_branch_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>()
    //             .OrderBy(x => x.Id)
    //             .Select(
    //                 x => new
    //                 {
    //                     Trunk1 = x.OptionalTrunk,
    //                     Branch1 = x.OptionalTrunk!.RequiredBranch,
    //                     Trunk2 = x.OptionalTrunk,
    //                     Branch2 = x.OptionalTrunk.RequiredBranch,
    //                 }),
    //         assertOrder: true,
    //         elementAsserter: (e, a) =>
    //         {
    //             AssertEqual(e.Trunk1, a.Trunk1);
    //             AssertEqual(e.Trunk2, a.Trunk2);
    //             AssertEqual(e.Branch1, a.Branch1);
    //             AssertEqual(e.Branch2, a.Branch2);
    //         },
    //         queryTrackingBehavior: queryTrackingBehavior);

    // [ConditionalTheory]
    // [MemberData(nameof(AsyncAndTrackingData))]
    // public virtual Task Select_trunk_and_trunk_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>()
    //             .OrderBy(x => x.Id)
    //             .Select(
    //                 x => new
    //                 {
    //                     Trunk1 = x.RequiredTrunk,
    //                     Leaf1 = x.RequiredTrunk!.OptionalBranch!.RequiredLeaf,
    //                     Trunk2 = x.RequiredTrunk,
    //                     Leaf2 = x.RequiredTrunk.OptionalBranch.RequiredLeaf,
    //                 }),
    //         assertOrder: true,
    //         elementAsserter: (e, a) =>
    //         {
    //             AssertEqual(e.Trunk1, a.Trunk1);
    //             AssertEqual(e.Trunk2, a.Trunk2);
    //             AssertEqual(e.Leaf1, a.Leaf1);
    //             AssertEqual(e.Leaf2, a.Leaf2);
    //         },
    //         queryTrackingBehavior: queryTrackingBehavior);

    // [ConditionalTheory]
    // [MemberData(nameof(AsyncAndTrackingData))]
    // public virtual Task Select_leaf_trunk_root(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>()
    //             .Select(
    //                 x => new
    //                 {
    //                     RequiredReferenceLeaf=x.RequiredTrunk.RequiredBranch.RequiredLeaf,
    //                     RequiredReferenceTrunk= x.RequiredTrunk,
    //                     x
    //                 }),
    //         elementSorter: e => e.x.Id,
    //         elementAsserter: (e, a) =>
    //         {
    //             AssertEqual(e.RequiredReferenceLeaf, a.RequiredReferenceLeaf);
    //             AssertEqual(e.RequiredReferenceTrunk, a.RequiredReferenceTrunk);
    //             AssertEqual(e.x, a.x);
    //         },
    //         queryTrackingBehavior: queryTrackingBehavior);

    // [ConditionalTheory]
    // [MemberData(nameof(AsyncAndTrackingData))]
    // public virtual Task Select_multiple_branch_leaf(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Select(
    //             x => new
    //             {
    //                 x.Id,
    //                 RequiredReferenceBranch= x.RequiredTrunk.RequiredBranch,
    //                 OptionalReferenceLeaf= x.RequiredTrunk.RequiredBranch.OptionalLeaf,
    //                 CollectionLeaf=x.RequiredTrunk.RequiredBranch.Leaves,
    //                 CollectionBranch= x.RequiredTrunk.Branches,
    //                 x.RequiredTrunk.RequiredBranch.OptionalLeaf!.Name
    //             }),
    //         elementSorter: e => e.Id,
    //         elementAsserter: (e, a) =>
    //         {
    //             Assert.Equal(e.Id, a.Id);
    //             AssertEqual(e.RequiredReferenceBranch, a.RequiredReferenceBranch);
    //             AssertEqual<Leaf>(e.OptionalReferenceLeaf, a.OptionalReferenceLeaf);
    //             AssertCollection(e.CollectionLeaf, a.CollectionLeaf, ordered: false);
    //             AssertCollection(e.CollectionBranch, a.CollectionBranch, ordered: false);
    //             Assert.Equal(e.Name, a.Name);
    //         },
    //         queryTrackingBehavior: queryTrackingBehavior);

    #endregion Multiple

    #region Subquery

    public override Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(
            queryTrackingBehavior, () => base.Select_subquery_required_related_FirstOrDefault(queryTrackingBehavior));

    public override Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertOwnedTrackingQuery(
            queryTrackingBehavior, () => base.Select_subquery_optional_related_FirstOrDefault(queryTrackingBehavior));

    // [ConditionalTheory]
    // [MemberData(nameof(AsyncAndTrackingData))]
    // public virtual Task Select_subquery_root_set_trunk_FirstOrDefault_collection(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>()
    //             .OrderBy(x => x.Id)
    //             .Select(
    //                 x => ss.Set<RootEntity>()
    //                     .OrderBy(xx => xx.Id)
    //                     .Select(xx => xx.RequiredTrunk)
    //                     .FirstOrDefault()!.Branches),
    //         assertOrder: true,
    //         elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee),
    //         queryTrackingBehavior: queryTrackingBehavior);

    // [ConditionalTheory]
    // [MemberData(nameof(AsyncAndTrackingData))]
    // public virtual Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>()
    //             .OrderBy(x => x.Id)
    //             .Select(
    //                 x => ss.Set<RootEntity>()
    //                     .OrderBy(xx => xx.Id)
    //                     .Select(
    //                         xx => new
    //                         {
    //                             OuterBranch = x.RequiredTrunk.Branches,
    //                             RequiredReferenceTrunk= xx.RequiredTrunk,
    //                             RequiredReferenceBranch=xx.RequiredTrunk.RequiredBranch,
    //                             xx.RequiredTrunk.Name,
    //                             OuterName = x.RequiredTrunk!.RequiredBranch.Name
    //                         }).FirstOrDefault()),
    //         assertOrder: true,
    //         elementAsserter: (e, a) =>
    //         {
    //             AssertCollection(e.OuterBranch, a.OuterBranch);
    //             AssertEqual(e.RequiredReferenceTrunk, a.RequiredReferenceTrunk);
    //             AssertEqual(e.RequiredReferenceBranch, a.RequiredReferenceBranch);
    //             AssertEqual(e.Name, a.Name);
    //             AssertEqual(e.OuterName, a.OuterName);
    //         },
    //         queryTrackingBehavior: queryTrackingBehavior);

    // [ConditionalTheory]
    // [MemberData(nameof(AsyncAndTrackingData))]
    // public virtual Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>()
    //             .OrderBy(x => x.Id)
    //             .Select(
    //                 x => ss.Set<RootEntity>()
    //                     .OrderBy(xx => xx.Id)
    //                     .Select(
    //                         xx => new
    //                         {
    //                             OuterBranchCollection = x.RequiredTrunk.Branches,
    //                             xx.RequiredTrunk,
    //                             xx.RequiredTrunk.RequiredBranch,
    //                             xx.RequiredTrunk.Name,
    //                             OuterName = x.RequiredTrunk.RequiredBranch.Name
    //                         }).FirstOrDefault()!.OuterBranchCollection),
    //         assertOrder: true,
    //         elementAsserter: (e, a) => AssertCollection(e, a),
    //         queryTrackingBehavior: queryTrackingBehavior);

    #endregion Subquery

    protected virtual async Task AssertOwnedTrackingQuery(QueryTrackingBehavior queryTrackingBehavior, Func<Task> test)
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
