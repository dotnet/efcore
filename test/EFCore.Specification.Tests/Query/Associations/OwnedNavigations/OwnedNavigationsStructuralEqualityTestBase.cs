// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedNavigations;

public abstract class OwnedNavigationsStructuralEqualityTestBase<TFixture>(TFixture fixture)
    : AssociationsStructuralEqualityTestBase<TFixture>(fixture)
    where TFixture : OwnedNavigationsFixtureBase, new()
{
    public override Task Two_associates()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate == e.OptionalAssociate),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    public override Task Two_nested_associates()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate == e.OptionalAssociate!.RequiredNestedAssociate),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    public override Task Not_equals()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate != e.OptionalAssociate),
            ss => ss.Set<RootEntity>().Where(e => false), // TODO: unclear, this should be true
            assertEmpty: true);

    // #36400
    public override Task Nested_associate_with_inline()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_associate_with_inline());

    // #36400
    public override Task Nested_associate_with_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_associate_with_parameter());

    public override Task Two_nested_collections()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection == e.OptionalAssociate!.NestedCollection),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    // #36400
    public override Task Nested_collection_with_inline()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_inline());

    // #36400
    public override Task Nested_collection_with_parameter()
        => Assert.ThrowsAsync<InvalidOperationException>(() => base.Nested_collection_with_parameter());
}
