// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Include;

public abstract class NavigationIncludeTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : NavigationRelationshipsFixtureBase, new()
{
    protected RelationshipsContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_trunk_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Include(x => x.RequiredReferenceTrunk),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RelationshipsRootEntity>(x => x.RequiredReferenceTrunk)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_trunk_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Include(x => x.OptionalReferenceTrunk),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RelationshipsRootEntity>(x => x.OptionalReferenceTrunk!)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_trunk_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Include(x => x.CollectionTrunk),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RelationshipsRootEntity>(x => x.CollectionTrunk)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_trunk_required_optional_and_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>()
                .Include(x => x.RequiredReferenceTrunk)
                .Include(x => x.OptionalReferenceTrunk)
                .Include(x => x.CollectionTrunk),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRootEntity>(x => x.RequiredReferenceTrunk),
                new ExpectedInclude<RelationshipsRootEntity>(x => x.OptionalReferenceTrunk!),
                new ExpectedInclude<RelationshipsRootEntity>(x => x.CollectionTrunk)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_required_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Include(x => x.RequiredReferenceTrunk.RequiredReferenceBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRootEntity>(x => x.RequiredReferenceTrunk),
                new ExpectedInclude<RelationshipsTrunkEntity>(x => x.RequiredReferenceBranch, "RequiredReferenceTrunk")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_required_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Include(x => x.RequiredReferenceTrunk.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRootEntity>(x => x.RequiredReferenceTrunk),
                new ExpectedInclude<RelationshipsTrunkEntity>(x => x.CollectionBranch, "RequiredReferenceTrunk")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_optional_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Include(x => x.OptionalReferenceTrunk!.OptionalReferenceBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRootEntity>(x => x.OptionalReferenceTrunk!),
                new ExpectedInclude<RelationshipsTrunkEntity>(x => x.OptionalReferenceBranch!, "OptionalReferenceTrunk")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_optional_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Include(x => x.OptionalReferenceTrunk!.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRootEntity>(x => x.OptionalReferenceTrunk!),
                new ExpectedInclude<RelationshipsTrunkEntity>(x => x.CollectionBranch, "OptionalReferenceTrunk")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_collection_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().Include(x => x.CollectionTrunk).ThenInclude(x => x.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRootEntity>(x => x.CollectionTrunk!),
                new ExpectedInclude<RelationshipsTrunkEntity>(x => x.CollectionBranch, "CollectionTrunk")));
}
