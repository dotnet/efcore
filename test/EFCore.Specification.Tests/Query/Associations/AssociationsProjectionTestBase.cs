// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsProjectionTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_root(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>(),
            queryTrackingBehavior: queryTrackingBehavior);

    #region Scalar properties

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_scalar_property_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.RequiredAssociate.String),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_property_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate!.String),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_value_type_property_on_null_associate_throws(QueryTrackingBehavior queryTrackingBehavior)
        // We have an entity with OptionalRelated null, so projecting a value type property from that throws
        // "Nullable object must have a value"
        => Assert.ThrowsAsync<InvalidOperationException>(() =>
            AssertQuery(
                ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate!.Int),
                queryTrackingBehavior: queryTrackingBehavior));

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_nullable_value_type_property_on_null_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => (int?)x.OptionalAssociate!.Int),
            queryTrackingBehavior: queryTrackingBehavior);

    #endregion Scalar properties

    #region Structural properties

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.RequiredAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_required_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.RequiredAssociate.RequiredNestedAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_optional_nested_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.RequiredAssociate.OptionalNestedAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_required_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate!.RequiredNestedAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_optional_nested_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => x.OptionalAssociate!.OptionalNestedAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_required_associate_via_optional_navigation(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootReferencingEntity>().Select(e => e.Root!.RequiredAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_unmapped_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(e => e.RequiredAssociate.Unmapped),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_untranslatable_method_on_associate_scalar_property(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(e => UntranslatableMethod(e.RequiredAssociate.Int)),
            queryTrackingBehavior: queryTrackingBehavior);

    private static int UntranslatableMethod(int i) => i + 1;

    #endregion Structural properties

    #region Structural collection properties

    // Note we order via the Id (server-side) to ensure the collections come back in deterministic order,
    // otherwise it's difficult/unreliable to compare client-side.

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.AssociateCollection),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.RequiredAssociate.NestedCollection),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().OrderBy(e => e.Id).Select(x => x.OptionalAssociate!.NestedCollection),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: r => r.Id),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task SelectMany_associate_collection(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().SelectMany(x => x.AssociateCollection),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task SelectMany_nested_collection_on_required_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().SelectMany(x => x.RequiredAssociate.NestedCollection),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task SelectMany_nested_collection_on_optional_associate(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .SelectMany(x => x.OptionalAssociate!.NestedCollection),
            ss => ss.Set<RootEntity>()
                .SelectMany(x => x.OptionalAssociate.Maybe(xx => xx!.NestedCollection) ?? new List<NestedAssociateType>()),
            queryTrackingBehavior: queryTrackingBehavior);

    #endregion Structural collection properties

    #region Multiple

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_root_duplicated(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => new { First = x, Second = x }),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))] // #37551
    public virtual Task Select_associate_and_target_to_index_based_binding_via_closure(QueryTrackingBehavior queryTrackingBehavior)
    {
        var obj = new object();
        return AssertQuery(
            ss => ss.Set<RootEntity>().Select(x => new { Obj = obj, x.Id, x.RequiredAssociate }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Same(e.Obj, a.Obj);
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.RequiredAssociate, a.RequiredAssociate);
            },
            queryTrackingBehavior: queryTrackingBehavior);
    }

    // [ConditionalTheory]
    // [MemberData(nameof(TrackingData))]
    // public virtual Task Select_trunk_and_branch_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
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
    // [MemberData(nameof(TrackingData))]
    // public virtual Task Select_trunk_and_trunk_duplicated(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
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
    // [MemberData(nameof(TrackingData))]
    // public virtual Task Select_leaf_trunk_root(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
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
    // [MemberData(nameof(TrackingData))]
    // public virtual Task Select_multiple_branch_leaf(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
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

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_subquery_required_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .Select(x => ss.Set<RootEntity>()
                .OrderBy(e => e.Id)
                .Select(e => e.RequiredAssociate)
                .FirstOrDefault()!.RequiredNestedAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory, MemberData(nameof(TrackingData))]
    public virtual Task Select_subquery_optional_related_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .Select(x => ss.Set<RootEntity>()
                .OrderBy(e => e.Id)
                .Select(e => e.OptionalAssociate)
                .FirstOrDefault()!.RequiredNestedAssociate),
            queryTrackingBehavior: queryTrackingBehavior);

    // [ConditionalTheory]
    // [MemberData(nameof(TrackingData))]
    // public virtual Task Select_subquery_root_set_trunk_FirstOrDefault_collection(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
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
    // [MemberData(nameof(TrackingData))]
    // public virtual Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
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
    // [MemberData(nameof(TrackingData))]
    // public virtual Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(QueryTrackingBehavior queryTrackingBehavior)
    //     => AssertQuery(
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
}
