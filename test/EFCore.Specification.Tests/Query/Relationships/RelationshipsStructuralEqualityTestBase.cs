// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public abstract class RelationshipsStructuralEqualityTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : RelationshipsQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Two_related()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated == e.OptionalRelated),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.Equals(e.OptionalRelated))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Two_nested()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested == e.OptionalRelated!.RequiredNested),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested.Equals(e.OptionalRelated!.RequiredNested))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Not_equals()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated != e.OptionalRelated),
            ss => ss.Set<RootEntity>().Where(e => !e.RequiredRelated.Equals(e.OptionalRelated))); // TODO: Rewrite equality to Equals for the entire test suite for complex

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
                .Where(e => e.RequiredRelated.RequiredNested == new NestedType { Id = 1000, Name = "Root1_RequiredRelated_RequiredNested", Int = 8, String = "foo" }),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.RequiredNested.Equals(new NestedType { Id = 1000, Name = "Root1_RequiredRelated_RequiredNested", Int = 8, String = "foo" }))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual async Task Nested_with_parameter()
    {
        var nested = new NestedType { Id = 1000, Name = "Root1_RequiredRelated_RequiredNested", Int = 8, String = "foo" };

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested == nested),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.RequiredNested.Equals(nested))); // TODO: Rewrite equality to Equals for the entire test suite for complex
    }

    [ConditionalFact]
    public virtual Task Two_nested_collections()
        => AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection == e.OptionalRelated!.NestedCollection),
            ss => ss.Set<RootEntity>().Where(e => e.OptionalRelated != null && e.RequiredRelated.NestedCollection.SequenceEqual(e.OptionalRelated!.NestedCollection))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual Task Nested_collection_with_inline()
        => AssertQuery(
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.NestedCollection == new List<NestedType>
                {
                    new() { Id = 1002, Name = "Root1_RequiredRelated_NestedCollection_1", Int = 8, String = "foo" },
                    new() { Id = 1003, Name = "Root1_RequiredRelated_NestedCollection_2", Int = 8, String = "foo" }
                }),
            ss => ss.Set<RootEntity>()
                .Where(e => e.RequiredRelated.NestedCollection.SequenceEqual(new List<NestedType>
                {
                    new() { Id = 1002, Name = "Root1_RequiredRelated_NestedCollection_1", Int = 8, String = "foo" },
                    new() { Id = 1003, Name = "Root1_RequiredRelated_NestedCollection_2", Int = 8, String = "foo" }
                }))); // TODO: Rewrite equality to Equals for the entire test suite for complex

    [ConditionalFact]
    public virtual async Task Nested_collection_with_parameter()
    {
        var nestedCollection = new List<NestedType>
        {
            new() { Id = 1002, Name = "Root1_RequiredRelated_NestedCollection_1", Int = 8, String = "foo" },
            new() { Id = 1003, Name = "Root1_RequiredRelated_NestedCollection_2", Int = 8, String = "foo" }
        };

        await AssertQuery(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection == nestedCollection),
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection.SequenceEqual(nestedCollection))); // TODO: Rewrite equality to Equals for the entire test suite for complex
    }

    // TODO: Equality on subquery
}
