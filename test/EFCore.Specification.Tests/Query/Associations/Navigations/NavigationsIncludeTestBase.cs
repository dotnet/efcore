// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsIncludeTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : NavigationsFixtureBase, new()
{
    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.RequiredRelated),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.RequiredRelated)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.OptionalRelated),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.OptionalRelated!)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.RelatedCollection),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.RelatedCollection)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_required_optional_and_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>()
                .Include(x => x.RequiredRelated)
                .Include(x => x.OptionalRelated)
                .Include(x => x.RelatedCollection),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.RequiredRelated),
                new ExpectedInclude<RootEntity>(x => x.OptionalRelated!),
                new ExpectedInclude<RootEntity>(x => x.RelatedCollection)));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.RequiredRelated.RequiredNested),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.RequiredRelated),
                new ExpectedInclude<RelatedType>(x => x.RequiredNested, "RequiredRelated")));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.OptionalRelated!.OptionalNested),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.OptionalRelated!),
                new ExpectedInclude<RelatedType>(x => x.OptionalNested!, "OptionalRelated")));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.RequiredRelated.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.RequiredRelated),
                new ExpectedInclude<RelatedType>(x => x.NestedCollection, "RequiredRelated")));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_collection_on_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.OptionalRelated!.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.OptionalRelated!),
                new ExpectedInclude<RelatedType>(x => x.NestedCollection, "OptionalRelated")));

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_collection_on_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.RelatedCollection).ThenInclude(x => x.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.RelatedCollection),
                new ExpectedInclude<RelatedType>(x => x.NestedCollection, "RelatedCollection")));
}
