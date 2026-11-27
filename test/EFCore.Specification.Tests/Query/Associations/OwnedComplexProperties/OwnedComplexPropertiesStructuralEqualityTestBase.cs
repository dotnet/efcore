// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.OwnedComplexProperties;

public abstract class OwnedComplexPropertiesStructuralEqualityTestBase<TFixture>(TFixture fixture)
    : AssociationsStructuralEqualityTestBase<TFixture>(fixture)
    where TFixture : OwnedComplexPropertiesFixtureBase, new()
{
    public override Task Two_associates()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate == e.OptionalAssociate),
            ss => ss.Set<RootEntity>().Where(e => false), // Owned entities are never equal
            assertEmpty: true);

    public override Task Not_equals()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate != e.OptionalAssociate),
            ss => ss.Set<RootEntity>().Where(e => false), // TODO: unclear, this should be true
            assertEmpty: true);

    // The nested associates are complex types, so they're compared by value

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
                            Ints = new List<int>
                            {
                                1,
                                2,
                                3
                            }
                        },
                        new()
                        {
                            Id = 1003,
                            Name = "Root1_RequiredAssociate_NestedCollection_2",
                            Int = 8,
                            String = "foo",
                            Ints = new List<int>
                            {
                                1,
                                2,
                                3
                            }
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
                            Ints = new List<int>
                            {
                                1,
                                2,
                                3
                            }
                        },
                        new()
                        {
                            Id = 1003,
                            Name = "Root1_RequiredAssociate_NestedCollection_2",
                            Int = 8,
                            String = "foo",
                            Ints = new List<int>
                            {
                                1,
                                2,
                                3
                            }
                        }
                    })));

    public override async Task Nested_collection_with_parameter()
    {
        var nestedCollection = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredAssociate.NestedCollection;

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection == nestedCollection),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection.SequenceEqual(nestedCollection)));
    }
}
