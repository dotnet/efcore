// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

/// <summary>
/// Tests for using navigations in projection - mostly to test shaper code around
/// </summary>
public abstract class RelationshipsInProjectionQueryTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    protected RelationshipsContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_root(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_trunk_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.OptionalReferenceTrunk),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_trunk_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_trunk_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.CollectionTrunk),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_branch_required_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.RequiredReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_branch_required_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.OptionalReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_branch_required_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_branch_optional_required(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.RequiredReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_branch_optional_optional(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.OptionalReferenceBranch),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_branch_optional_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name));
}
