// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships.InProjection;

public abstract class JsonRelationshipsInProjectionQueryTestBase<TFixture>(TFixture fixture)
    : RelationshipsInProjectionQueryTestBase<TFixture>(fixture)
        where TFixture : JsonRelationshipsQueryFixtureBase, new()
{
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_branch_collection_element_using_indexer_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RelationshipsRootEntity>().OrderBy(x => x.Id).Select(x => x.RequiredReferenceTrunk.CollectionBranch),
            assertOrder: true,
            elementAsserter: (e, a) => AssertCollection(e, a, elementSorter: ee => ee.Name));


}
