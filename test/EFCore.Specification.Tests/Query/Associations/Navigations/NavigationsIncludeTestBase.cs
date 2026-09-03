// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.Navigations;

public abstract class NavigationsIncludeTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : NavigationsFixtureBase, new()
{
    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.RequiredAssociate),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.RequiredAssociate)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.OptionalAssociate),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.OptionalAssociate!)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.AssociateCollection),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<RootEntity>(x => x.AssociateCollection)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_required_optional_and_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>()
                .Include(x => x.RequiredAssociate)
                .Include(x => x.OptionalAssociate)
                .Include(x => x.AssociateCollection),
            assertOrder: true,
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.RequiredAssociate),
                new ExpectedInclude<RootEntity>(x => x.OptionalAssociate!),
                new ExpectedInclude<RootEntity>(x => x.AssociateCollection)));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.RequiredAssociate.RequiredNestedAssociate),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.RequiredAssociate),
                new ExpectedInclude<AssociateType>(x => x.RequiredNestedAssociate, "RequiredRelated")));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.OptionalAssociate!.OptionalNestedAssociate),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.OptionalAssociate!),
                new ExpectedInclude<AssociateType>(x => x.OptionalNestedAssociate!, "OptionalRelated")));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.RequiredAssociate.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.RequiredAssociate),
                new ExpectedInclude<AssociateType>(x => x.NestedCollection, "RequiredRelated")));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_collection_on_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.OptionalAssociate!.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.OptionalAssociate!),
                new ExpectedInclude<AssociateType>(x => x.NestedCollection, "OptionalRelated")));

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual Task Include_nested_collection_on_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Include(x => x.AssociateCollection).ThenInclude(x => x.NestedCollection),
            elementAsserter: (e, a) => AssertInclude(
                e,
                a,
                new ExpectedInclude<RootEntity>(x => x.AssociateCollection),
                new ExpectedInclude<AssociateType>(x => x.NestedCollection, "RelatedCollection")));
}
