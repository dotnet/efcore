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

    public override Task Two_related()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated == e.OptionalRelated),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.Equals(e.OptionalRelated)));

    public override Task Two_nested()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested == e.OptionalRelated!.RequiredNested),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested.Equals(e.OptionalRelated!.RequiredNested)));

    public override Task Not_equals()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated != e.OptionalRelated),
            ss => ss.Set<RootEntity>().Where(e => !e.RequiredRelated.Equals(e.OptionalRelated)));

    public override Task Nested_with_inline()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.RequiredNested
                    == new NestedType
                    {
                        Id = 1000,
                        Name = "Root1_RequiredRelated_RequiredNested",
                        Int = 8,
                        String = "foo"
                    }),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.RequiredNested.Equals(
                    new NestedType
                    {
                        Id = 1000,
                        Name = "Root1_RequiredRelated_RequiredNested",
                        Int = 8,
                        String = "foo"
                    })));

    public override async Task Nested_with_parameter()
    {
        var nested = new NestedType
        {
            Id = 1000,
            Name = "Root1_RequiredRelated_RequiredNested",
            Int = 8,
            String = "foo"
        };

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested == nested),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested.Equals(nested)));
    }

    public override Task Two_nested_collections()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection == e.OptionalRelated!.NestedCollection),
            ss => ss.Set<RootEntity>().Where(e
                => e.OptionalRelated != null && e.RequiredRelated.NestedCollection.SequenceEqual(e.OptionalRelated!.NestedCollection)));

    public override Task Nested_collection_with_inline()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.NestedCollection
                    == new List<NestedType>
                    {
                        new()
                        {
                            Id = 1002,
                            Name = "Root1_RequiredRelated_NestedCollection_1",
                            Int = 8,
                            String = "foo"
                        },
                        new()
                        {
                            Id = 1003,
                            Name = "Root1_RequiredRelated_NestedCollection_2",
                            Int = 8,
                            String = "foo"
                        }
                    }),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.NestedCollection.SequenceEqual(
                    new List<NestedType>
                    {
                        new()
                        {
                            Id = 1002,
                            Name = "Root1_RequiredRelated_NestedCollection_1",
                            Int = 8,
                            String = "foo"
                        },
                        new()
                        {
                            Id = 1003,
                            Name = "Root1_RequiredRelated_NestedCollection_2",
                            Int = 8,
                            String = "foo"
                        }
                    })));

    public override async Task Nested_collection_with_parameter()
    {
        var nestedCollection = new List<NestedType>
        {
            new()
            {
                Id = 1002,
                Name = "Root1_RequiredRelated_NestedCollection_1",
                Int = 8,
                String = "foo"
            },
            new()
            {
                Id = 1003,
                Name = "Root1_RequiredRelated_NestedCollection_2",
                Int = 8,
                String = "foo"
            }
        };

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection == nestedCollection),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection.SequenceEqual(nestedCollection)));
    }

    #region Value types

    [ConditionalFact]
    public virtual Task Nullable_value_type_with_null()
        => AssertQuery(ss => ss.Set<ValueRootEntity>().Where(e => e.OptionalRelated == null));

    #endregion Value types
}
