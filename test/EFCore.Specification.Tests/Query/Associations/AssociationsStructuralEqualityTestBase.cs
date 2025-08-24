// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsStructuralEqualityTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Two_related()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated == e.OptionalRelated),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.Equals(
                    e.OptionalRelated))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Two_nested()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested == e.OptionalRelated!.RequiredNested),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.RequiredNested.Equals(
                    e.OptionalRelated!.RequiredNested))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Not_equals()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated != e.OptionalRelated),
            ss => ss.Set<RootEntity>()
                .Where(e => !e.RequiredRelated.Equals(
                    e.OptionalRelated))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Related_with_inline_null()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalRelated == null));

    [ConditionalFact]
    public virtual async Task Related_with_parameter_null()
    {
        RelatedType? related = null;

        await AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.OptionalRelated == related));
    }

    [ConditionalFact]
    public virtual Task Nested_with_inline_null()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.OptionalNested == null));

    [ConditionalFact]
    public virtual Task Nested_with_inline()
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
                    }))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual async Task Nested_with_parameter()
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
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.RequiredNested
                    .Equals(nested))); // TODO: Rewrite equality to Equals for the entire test suite for complex
    }

    [ConditionalFact]
    public virtual Task Two_nested_collections()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection == e.OptionalRelated!.NestedCollection),
            ss => ss.Set<RootEntity>().Where(e
                => e.OptionalRelated != null
                && e.RequiredRelated.NestedCollection.SequenceEqual(
                    e.OptionalRelated!.NestedCollection))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Nested_collection_with_inline()
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
                    }))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual async Task Nested_collection_with_parameter()
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
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.NestedCollection.SequenceEqual(
                    nestedCollection))); // TODO: Rewrite equality to Equals for the entire test suite for complex
    }

    #region Contains

    [ConditionalFact]
    public virtual Task Contains_with_inline()
        => AssertQuery(ss => ss.Set<RootEntity>().Where(e =>
            e.RequiredRelated.NestedCollection.Contains(
                new NestedType
                {
                    Id = 1002,
                    Name = "Root1_RequiredRelated_NestedCollection_1",
                    Int = 8,
                    String = "foo"
                })));

    [ConditionalFact]
    public virtual async Task Contains_with_parameter()
    {
        var nested = new NestedType
        {
            Id = 1002,
            Name = "Root1_RequiredRelated_NestedCollection_1",
            Int = 8,
            String = "foo"
        };

        await AssertQuery(ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection.Contains(nested)));
    }

    [ConditionalFact]
    public virtual async Task Contains_with_operators_composed_on_the_collection()
    {
        var collection = Fixture.Data.RootEntities.Single(e => e.Name == "Root3_With_different_values").RequiredRelated.NestedCollection;

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(
                e => e.RequiredRelated.NestedCollection.Where(n => n.Int > collection[0].Int).Contains(collection[1])));
    }

    [ConditionalFact]
    public virtual async Task Contains_with_nested_and_composed_operators()
    {
        var collection = Fixture.Data.RootEntities.Single(e => e.Name == "Root3_With_different_values").RelatedCollection;

        await AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(e => e.RelatedCollection.Where(r => r.Id > collection[0].Id).Contains(collection[1])));
    }

    #endregion Contains

    // TODO: Equality on subquery
}
