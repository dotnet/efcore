// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Projection;

public abstract class ReferenceProjectionTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    protected RelationshipsContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_root(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_trunk_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.OptionalReferenceTrunk),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_trunk_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_branch_required_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.RequiredReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_branch_required_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.OptionalReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_branch_optional_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.RequiredReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_branch_optional_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.OptionalReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_root_duplicated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Select(x => new { First = x, Second = x }),
            elementSorter: e => e.First.Id,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.First, a.First);
                AssertEqual(e.Second, a.Second);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_trunk_and_branch_duplicated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_trunk_and_trunk_duplicated(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_leaf_trunk_root(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
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
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_root_set_required_trunk_FirstOrDefault_branch(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRootEntity>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.RequiredReferenceTrunk)
                        .FirstOrDefault()!.RequiredReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Select_subquery_root_set_optional_trunk_FirstOrDefault_branch(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .OrderBy(x => x.Id)
                .Select(
                    x => ss.Set<RelationshipsRootEntity>()
                        .OrderBy(xx => xx.Id)
                        .Select(xx => xx.OptionalReferenceTrunk)
                        .FirstOrDefault()!.OptionalReferenceBranch),
            assertOrder: true);
}
