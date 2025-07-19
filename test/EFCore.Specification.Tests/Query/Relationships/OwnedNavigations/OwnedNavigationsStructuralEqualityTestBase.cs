// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;

public abstract class OwnedNavigationsStructuralEqualityTestBase<TFixture>(TFixture fixture)
    : RelationshipsStructuralEqualityTestBase<TFixture>(fixture)
    where TFixture : OwnedNavigationsFixtureBase, new()
{
    public override Task Two_related(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated == e.OptionalRelated),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    public override Task Two_nested(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested == e.OptionalRelated!.RequiredNested),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    public override Task Not_equals(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated != e.OptionalRelated),
            ss => ss.Set<RootEntity>().Where(e => false), // TODO: unclear, this should be true
            assertEmpty: true);

    // #36400
    public override Task Nested_with_inline(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_with_inline(async));

    // #36400
    public override Task Nested_with_parameter(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_with_parameter(async));

    public override Task Two_nested_collections(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection == e.OptionalRelated!.NestedCollection),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    // #36400
    public override Task Nested_collection_with_inline(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_inline(async));

    // #36400
    public override Task Nested_collection_with_parameter(bool async)
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_parameter(async));
}
