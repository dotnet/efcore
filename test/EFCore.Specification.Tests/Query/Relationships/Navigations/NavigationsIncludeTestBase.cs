// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.Navigations;

public abstract class NavigationsIncludeTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : NavigationsFixtureBase, new()
{
    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_trunk_required(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Include(x => x.RequiredTrunk),
    //         elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.RequiredTrunk)));

    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_trunk_optional(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Include(x => x.OptionalTrunk),
    //         elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.OptionalTrunk!)));

    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_trunk_collection(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Include(x => x.CollectionTrunk),
    //         elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.CollectionTrunk)));

    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_trunk_required_optional_and_collection(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>()
    //             .Include(x => x.RequiredTrunk)
    //             .Include(x => x.OptionalTrunk)
    //             .Include(x => x.CollectionTrunk),
    //         assertOrder: true,
    //         elementAsserter: (e, a) => AssertInclude(
    //             e,
    //             a,
    //             new ExpectedInclude<RootEntity>(x => x.RequiredTrunk),
    //             new ExpectedInclude<RootEntity>(x => x.OptionalTrunk!),
    //             new ExpectedInclude<RootEntity>(x => x.CollectionTrunk)));

    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_branch_required_required(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Include(x => x.RequiredTrunk.RequiredBranch),
    //         elementAsserter: (e, a) => AssertInclude(
    //             e,
    //             a,
    //             new ExpectedInclude<RootEntity>(x => x.RequiredTrunk),
    //             new ExpectedInclude<Trunk>(x => x.RequiredBranch, "RequiredReferenceTrunk")));

    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_branch_required_collection(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Include(x => x.RequiredTrunk.Branches),
    //         elementAsserter: (e, a) => AssertInclude(
    //             e,
    //             a,
    //             new ExpectedInclude<RootEntity>(x => x.RequiredTrunk),
    //             new ExpectedInclude<Trunk>(x => x.Branches, "RequiredReferenceTrunk")));

    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_branch_optional_optional(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Include(x => x.OptionalTrunk!.OptionalBranch),
    //         elementAsserter: (e, a) => AssertInclude(
    //             e,
    //             a,
    //             new ExpectedInclude<RootEntity>(x => x.OptionalTrunk!),
    //             new ExpectedInclude<Trunk>(x => x.OptionalBranch!, "OptionalReferenceTrunk")));

    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_branch_optional_collection(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Include(x => x.OptionalTrunk!.Branches),
    //         elementAsserter: (e, a) => AssertInclude(
    //             e,
    //             a,
    //             new ExpectedInclude<RootEntity>(x => x.OptionalTrunk!),
    //             new ExpectedInclude<Trunk>(x => x.Branches, "OptionalReferenceTrunk")));

    // [ConditionalTheory]
    // [MemberData(nameof(IsAsyncData))]
    // public virtual Task Include_branch_collection_collection(bool async)
    //     => AssertQuery(
    //         async,
    //         ss => ss.Set<RootEntity>().Include(x => x.CollectionTrunk).ThenInclude(x => x.Branches),
    //         elementAsserter: (e, a) => AssertInclude(
    //             e,
    //             a,
    //             new ExpectedInclude<RootEntity>(x => x.CollectionTrunk!),
    //             new ExpectedInclude<Trunk>(x => x.Branches, "CollectionTrunk")));
}
