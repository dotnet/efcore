// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsStructuralEqualityTestBase<TFixture>(TFixture fixture)
    : AssociationsStructuralEqualityTestBase<TFixture>(fixture)
    where TFixture : OwnedNavigationsFixtureBase, new()
{
    public override Task Two_related()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated == e.OptionalRelated),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    public override Task Two_nested()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested == e.OptionalRelated!.RequiredNested),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    public override Task Not_equals()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated != e.OptionalRelated),
            ss => ss.Set<RootEntity>().Where(e => false), // TODO: unclear, this should be true
            assertEmpty: true);

    // #36400
    public override Task Nested_with_inline()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_with_inline());

    // #36400
    public override Task Nested_with_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_with_parameter());

    public override Task Two_nested_collections()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection == e.OptionalRelated!.NestedCollection),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    // #36400
    public override Task Nested_collection_with_inline()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_inline());

    // #36400
    public override Task Nested_collection_with_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_parameter());
}
