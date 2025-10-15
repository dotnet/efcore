// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public abstract class ComplexPropertiesStructuralEqualityTestBase<TFixture>(TFixture fixture)
    : AssociationsStructuralEqualityTestBase<TFixture>(fixture)
    where TFixture : ComplexPropertiesFixtureBase, new()
{
    // The below overrides are in order to add the client-side by-value comparison (Equals instead of ==), to account for the
    // by-value server-side behavior of complex properties.
    // TODO: Ideally do this rewriting automatically via a visitor

    public override Task Two_associates()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate == e.OptionalAssociate),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Equals(e.OptionalAssociate)));

    public override Task Two_nested_associates()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate == e.OptionalAssociate!.RequiredNestedAssociate),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate.Equals(e.OptionalAssociate!.RequiredNestedAssociate)));

    public override Task Not_equals()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate != e.OptionalAssociate),
            ss => ss.Set<RootEntity>().Where(e => !e.RequiredAssociate.Equals(e.OptionalAssociate)));

    public override Task Nested_associate_with_inline()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.RequiredNestedAssociate
                    == new NestedAssociateType
                    {
                        Id = 1000,
                        Name = "Root1_RequiredAssociate_RequiredNestedAssociate",
                        Int = 8,
                        String = "foo",
                        Ints = new() { 1, 2, 3 }
                    }),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.RequiredNestedAssociate.Equals(
                    new NestedAssociateType
                    {
                        Id = 1000,
                        Name = "Root1_RequiredAssociate_RequiredNestedAssociate",
                        Int = 8,
                        String = "foo",
                        Ints = new() { 1, 2, 3 }
                    })));

    public override async Task Nested_associate_with_parameter()
    {
        var nested = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredAssociate.RequiredNestedAssociate;

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate == nested),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate.Equals(nested)));
    }

    public override Task Two_nested_collections()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection == e.OptionalAssociate!.NestedCollection),
            ss => ss.Set<RootEntity>().Where(e
                => e.OptionalAssociate != null && e.RequiredAssociate.NestedCollection.SequenceEqual(e.OptionalAssociate!.NestedCollection)));

    public override Task Nested_collection_with_inline()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.NestedCollection
                    == new List<NestedAssociateType>
                    {
                        new()
                        {
                            Id = 1002,
                            Name = "Root1_RequiredAssociate_NestedCollection_1",
                            Int = 8,
                            String = "foo",
                            Ints = new List<int> { 1, 2, 3 }
                        },
                        new()
                        {
                            Id = 1003,
                            Name = "Root1_RequiredAssociate_NestedCollection_2",
                            Int = 8,
                            String = "foo",
                            Ints = new List<int> { 1, 2, 3 }
                        }
                    }),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.NestedCollection.SequenceEqual(
                    new List<NestedAssociateType>
                    {
                        new()
                        {
                            Id = 1002,
                            Name = "Root1_RequiredAssociate_NestedCollection_1",
                            Int = 8,
                            String = "foo",
                            Ints = new List<int> { 1, 2, 3 }
                        },
                        new()
                        {
                            Id = 1003,
                            Name = "Root1_RequiredAssociate_NestedCollection_2",
                            Int = 8,
                            String = "foo",
                            Ints = new List<int> { 1, 2, 3 }
                        }
                    })));

    public override async Task Nested_collection_with_parameter()
    {
        var nestedCollection = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredAssociate.NestedCollection;

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection == nestedCollection),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection.SequenceEqual(nestedCollection)));
    }

    #region Value types

    [ConditionalFact]
    public virtual Task Nullable_value_type_with_null()
        => AssertQuery(ss => ss.Set<ValueRootEntity>().Where(e => e.OptionalAssociate == null));

    #endregion Value types
}
