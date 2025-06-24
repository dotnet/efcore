// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class RelationshipsProjectionTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    protected RelationshipsContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>(),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_trunk_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.OptionalReferenceTrunk),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_trunk_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.CollectionTrunk),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_branch_required_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.RequiredReferenceBranch),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_branch_required_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.OptionalReferenceBranch),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_branch_optional_required(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.RequiredReferenceBranch),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_branch_optional_optional(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.OptionalReferenceBranch),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_branch_required_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_branch_optional_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name),
            queryTrackingBehavior: queryTrackingBehavior);

    #region Multiple

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_root_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Select(x => new { First = x, Second = x }),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_trunk_and_branch_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
                    {
                        Trunk1 = x.OptionalReferenceTrunk,
                        Branch1 = x.OptionalReferenceTrunk!.RequiredReferenceBranch,
                        Trunk2 = x.OptionalReferenceTrunk,
                        Branch2 = x.OptionalReferenceTrunk.RequiredReferenceBranch,
                    }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Trunk1, a.Trunk1);
                AssertEqual(e.Trunk2, a.Trunk2);
                AssertEqual(e.Branch1, a.Branch1);
                AssertEqual(e.Branch2, a.Branch2);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_trunk_and_trunk_duplicated(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .OrderBy(x => x.Id)
                .Select(
                    x => new
                    {
                        Trunk1 = x.RequiredReferenceTrunk,
                        Leaf1 = x.RequiredReferenceTrunk!.OptionalReferenceBranch!.RequiredReferenceLeaf,
                        Trunk2 = x.RequiredReferenceTrunk,
                        Leaf2 = x.RequiredReferenceTrunk.OptionalReferenceBranch.RequiredReferenceLeaf,
                    }),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.Trunk1, a.Trunk1);
                AssertEqual(e.Trunk2, a.Trunk2);
                AssertEqual(e.Leaf1, a.Leaf1);
                AssertEqual(e.Leaf2, a.Leaf2);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_leaf_trunk_root(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .Select(
                    x => new
                    {
                        x.RequiredReferenceTrunk.RequiredReferenceBranch.RequiredReferenceLeaf,
                        x.RequiredReferenceTrunk,
                        x
                    }),
            elementSorter: e => e.x.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.RequiredReferenceLeaf, a.RequiredReferenceLeaf);
                AssertEqual(e.RequiredReferenceTrunk, a.RequiredReferenceTrunk);
                AssertEqual(e.x, a.x);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_multiple_branch_leaf(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Select(
                x => new
                {
                    x.Id,
                    x.RequiredReferenceTrunk.RequiredReferenceBranch,
                    x.RequiredReferenceTrunk.RequiredReferenceBranch.OptionalReferenceLeaf,
                    x.RequiredReferenceTrunk.RequiredReferenceBranch.CollectionLeaf,
                    x.RequiredReferenceTrunk.CollectionBranch,
                    x.RequiredReferenceTrunk.RequiredReferenceBranch.OptionalReferenceLeaf!.Name
                }),
            elementSorter: e => e.Id,
            elementAsserter: (e, a) =>
            {
                Assert.Equal(e.Id, a.Id);
                AssertEqual(e.RequiredReferenceBranch, a.RequiredReferenceBranch);
                AssertEqual(e.OptionalReferenceLeaf, a.OptionalReferenceLeaf);
                AssertCollection(e.CollectionLeaf, a.CollectionLeaf, ordered: false);
                AssertCollection(e.CollectionBranch, a.CollectionBranch, ordered: false);
                Assert.Equal(e.Name, a.Name);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    #endregion Multiple

    #region Subquery

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRoot>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.RequiredReferenceTrunk)
                        .FirstOrDefault()!.RequiredReferenceBranch),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRoot>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.OptionalReferenceTrunk)
                        .FirstOrDefault()!.OptionalReferenceBranch),
            assertOrder: true,
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRoot>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.RequiredReferenceTrunk)
                        .FirstOrDefault()!.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRoot>()
                        .OrderBy(xx => xx.Id)
                        .Select(
                            xx => new
                            {
                                OuterBranch = x.RequiredReferenceTrunk.CollectionBranch,
                                xx.RequiredReferenceTrunk,
                                xx.RequiredReferenceTrunk.RequiredReferenceBranch,
                                xx.RequiredReferenceTrunk.Name,
                                OuterName = x.RequiredReferenceTrunk!.RequiredReferenceBranch.Name
                            }).FirstOrDefault()),
            assertOrder: true,
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.OuterBranch, a.OuterBranch);
                AssertEqual(e.RequiredReferenceTrunk, a.RequiredReferenceTrunk);
                AssertEqual(e.RequiredReferenceBranch, a.RequiredReferenceBranch);
                AssertEqual(e.Name, a.Name);
                AssertEqual(e.OuterName, a.OuterName);
            },
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRoot>()
                        .OrderBy(xx => xx.Id)
                        .Select(
                            xx => new
                            {
                                OuterBranchCollection = x.RequiredReferenceTrunk.CollectionBranch,
                                xx.RequiredReferenceTrunk,
                                xx.RequiredReferenceTrunk.RequiredReferenceBranch,
                                xx.RequiredReferenceTrunk.Name,
                                OuterName = x.RequiredReferenceTrunk.RequiredReferenceBranch.Name
                            }).FirstOrDefault()!.OuterBranchCollection),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a),
            queryTrackingBehavior: queryTrackingBehavior);

    #endregion Subquery

    #region SelectMany

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task SelectMany_trunk_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().SelectMany(x => x.CollectionTrunk),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task SelectMany_required_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().SelectMany(x => x.RequiredReferenceTrunk.CollectionBranch),
            queryTrackingBehavior: queryTrackingBehavior);

    [ConditionalTheory]
    [MemberData(nameof(AsyncAndTrackingData))]
    public virtual Task SelectMany_optional_trunk_reference_branch_collection(bool async, QueryTrackingBehavior queryTrackingBehavior)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .SelectMany(x => x.OptionalReferenceTrunk!.CollectionBranch),
            ss => ss.Set<RelationshipsRoot>()
                .SelectMany(x => x.OptionalReferenceTrunk.Maybe(xx => xx!.CollectionBranch) ?? new List<RelationshipsBranch>()),
            queryTrackingBehavior: queryTrackingBehavior);

    #endregion SelectMany
}
