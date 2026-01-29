// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsStructuralEqualityTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Two_associates()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate == e.OptionalAssociate),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.Equals(
                    e.OptionalAssociate))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Two_nested_associates()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate == e.OptionalAssociate!.RequiredNestedAssociate),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.RequiredNestedAssociate.Equals(
                    e.OptionalAssociate!.RequiredNestedAssociate))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Not_equals()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate != e.OptionalAssociate),
            ss => ss.Set<RootEntity>()
                .Where(e => !e.RequiredAssociate.Equals(
                    e.OptionalAssociate))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Associate_with_inline_null()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalAssociate == null));

    [ConditionalFact]
    public virtual async Task Associate_with_parameter_null()
    {
        AssociateType? related = null;

        await AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalAssociate == related));
    }

    [ConditionalFact]
    public virtual Task Nested_associate_with_inline_null()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.OptionalNestedAssociate == null));

    [ConditionalFact]
    public virtual Task Optional_associate_nested_associate_with_inline_null()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalAssociate!.OptionalNestedAssociate == null));

    [ConditionalFact]
    public virtual Task Optional_associate_nested_associate_with_inline_not_null()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalAssociate!.OptionalNestedAssociate != null));

    [ConditionalFact]
    public virtual Task Nested_associate_with_inline()
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
                    }))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual async Task Nested_associate_with_parameter()
    {
        var nested = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredAssociate.RequiredNestedAssociate;

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.RequiredNestedAssociate == nested),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.RequiredNestedAssociate
                    .Equals(nested))); // TODO: Rewrite equality to Equals for the entire test suite for complex
    }

    [ConditionalFact]
    public virtual Task Two_nested_collections()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection == e.OptionalAssociate!.NestedCollection),
            ss => ss.Set<RootEntity>().Where(e
                => e.OptionalAssociate != null
                && e.RequiredAssociate.NestedCollection.SequenceEqual(
                    e.OptionalAssociate!.NestedCollection))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Nested_collection_with_inline()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.NestedCollection
                    == new List<NestedAssociateType>
                    {
                        new()
                        {
                            Id = 1002,
                            Name = "Root1_RequiredRelated_NestedCollection_1",
                            Int = 8,
                            String = "foo",
                            Ints = new() { 1, 2, 3 }
                        },
                        new()
                        {
                            Id = 1003,
                            Name = "Root1_RequiredRelated_NestedCollection_2",
                            Int = 8,
                            String = "foo",
                            Ints = new() { 1, 2, 3 }
                        }
                    }),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.NestedCollection.SequenceEqual(
                    new List<NestedAssociateType>
                    {
                        new()
                        {
                            Id = 1002,
                            Name = "Root1_RequiredRelated_NestedCollection_1",
                            Int = 8,
                            String = "foo",
                            Ints = new() { 1, 2, 3 }
                        },
                        new()
                        {
                            Id = 1003,
                            Name = "Root1_RequiredRelated_NestedCollection_2",
                            Int = 8,
                            String = "foo",
                            Ints = new() { 1, 2, 3 }
                        }
                    }))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual async Task Nested_collection_with_parameter()
    {
        var nestedCollection = new List<NestedAssociateType>
        {
            new()
            {
                Id = 1002,
                Name = "Root1_RequiredRelated_NestedCollection_1",
                Int = 8,
                String = "foo",
                Ints = new() { 1, 2, 3 }
            },
            new()
            {
                Id = 1003,
                Name = "Root1_RequiredRelated_NestedCollection_2",
                Int = 8,
                String = "foo",
                Ints = new() { 1, 2, 3 }
            }
        };

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection == nestedCollection),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredAssociate.NestedCollection.SequenceEqual(
                    nestedCollection))); // TODO: Rewrite equality to Equals for the entire test suite for complex
    }

    #region Contains

    [ConditionalFact]
    public virtual Task Contains_with_inline()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RequiredAssociate.NestedCollection.Contains(
                new NestedAssociateType
                {
                    Id = 1002,
                    Name = "Root1_RequiredAssociate_NestedCollection_1",
                    Int = 8,
                    String = "foo",
                    Ints = new() { 1, 2, 3 }
                })));

    [ConditionalFact]
    public virtual async Task Contains_with_parameter()
    {
        var nested = new NestedAssociateType
        {
            Id = 1002,
            Name = "Root1_RequiredAssociate_NestedCollection_1",
            Int = 8,
            String = "foo",
            Ints = [1, 2, 3]
        };

        await AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection.Contains(nested)));
    }

    [ConditionalFact]
    public virtual async Task Contains_with_operators_composed_on_the_collection()
    {
        var collection = Fixture.Data.RootEntities.Single(e => e.Name == "Root3_With_different_values").RequiredAssociate.NestedCollection;

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(
                e => e.RequiredAssociate.NestedCollection.Where(n => n.Int > collection[0].Int).Contains(collection[1])));
    }

    [ConditionalFact]
    public virtual async Task Contains_with_nested_and_composed_operators()
    {
        var collection = Fixture.Data.RootEntities.Single(e => e.Name == "Root3_With_different_values").AssociateCollection;

        await AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(e => e.AssociateCollection.Where(r => r.Id > collection[0].Id).Contains(collection[1])));
    }

    #endregion Contains

    // TODO: Equality on subquery
}
