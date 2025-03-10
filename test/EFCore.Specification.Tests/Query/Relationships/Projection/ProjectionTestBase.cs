// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public abstract class ProjectionTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    protected RelationshipsContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_trunk_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.CollectionTrunk),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_branch_required_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_branch_optional_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_multiple_branch_leaf(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Select(
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
                AssertCollection(e.CollectionLeaf, a.CollectionLeaf, ordered: true);
                AssertCollection(e.CollectionBranch, a.CollectionBranch, ordered: true);
                Assert.Equal(e.Name, a.Name);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_root_set_trunk_FirstOrDefault_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRootEntity>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.RequiredReferenceTrunk)
                        .FirstOrDefault()!.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_root_set_complex_projection_including_references_to_outer_FirstOrDefault(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRootEntity>()
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_root_set_complex_projection_FirstOrDefault_project_reference_to_outer(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRootEntity>()
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
            elementAsserter: (e, a) => AssertCollection(e, a));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_trunk_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .SelectMany(x => x.CollectionTrunk));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_required_trunk_reference_branch_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .SelectMany(x => x.RequiredReferenceTrunk.CollectionBranch));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task SelectMany_optional_trunk_reference_branch_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .SelectMany(x => x.OptionalReferenceTrunk!.CollectionBranch),
            ss => ss.Set<RelationshipsRootEntity>()
                .SelectMany(x => x.OptionalReferenceTrunk.Maybe(xx => xx!.CollectionBranch) ?? new List<RelationshipsBranchEntity>()));
}
