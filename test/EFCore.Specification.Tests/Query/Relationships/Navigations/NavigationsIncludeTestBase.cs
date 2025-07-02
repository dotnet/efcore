// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public abstract class NavigationsIncludeTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : NavigationsFixtureBase, new()
{
    protected RelationshipsContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_trunk_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Include(x => x.RequiredReferenceTrunk),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RelationshipsRoot>(x => x.RequiredReferenceTrunk)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_trunk_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Include(x => x.OptionalReferenceTrunk),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RelationshipsRoot>(x => x.OptionalReferenceTrunk!)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_trunk_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Include(x => x.CollectionTrunk),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RelationshipsRoot>(x => x.CollectionTrunk)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_trunk_required_optional_and_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>()
                .Include(x => x.RequiredReferenceTrunk)
                .Include(x => x.OptionalReferenceTrunk)
                .Include(x => x.CollectionTrunk),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRoot>(x => x.RequiredReferenceTrunk),
                new ExpectedInclude<RelationshipsRoot>(x => x.OptionalReferenceTrunk!),
                new ExpectedInclude<RelationshipsRoot>(x => x.CollectionTrunk)));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_required_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Include(x => x.RequiredReferenceTrunk.RequiredReferenceBranch),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRoot>(x => x.RequiredReferenceTrunk),
                new ExpectedInclude<RelationshipsTrunk>(x => x.RequiredReferenceBranch, "RequiredReferenceTrunk")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_required_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Include(x => x.RequiredReferenceTrunk.CollectionBranch),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRoot>(x => x.RequiredReferenceTrunk),
                new ExpectedInclude<RelationshipsTrunk>(x => x.CollectionBranch, "RequiredReferenceTrunk")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_optional_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Include(x => x.OptionalReferenceTrunk!.OptionalReferenceBranch),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRoot>(x => x.OptionalReferenceTrunk!),
                new ExpectedInclude<RelationshipsTrunk>(x => x.OptionalReferenceBranch!, "OptionalReferenceTrunk")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_optional_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Include(x => x.OptionalReferenceTrunk!.CollectionBranch),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRoot>(x => x.OptionalReferenceTrunk!),
                new ExpectedInclude<RelationshipsTrunk>(x => x.CollectionBranch, "OptionalReferenceTrunk")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Include_branch_collection_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRoot>().Include(x => x.CollectionTrunk).ThenInclude(x => x.CollectionBranch),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RelationshipsRoot>(x => x.CollectionTrunk!),
                new ExpectedInclude<RelationshipsTrunk>(x => x.CollectionBranch, "CollectionTrunk")));
}
