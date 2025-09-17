// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.BulkUpdates;

namespace Microsoft.EntityFrameworkCore.Query.Associations;

public abstract class AssociationsBulkUpdateTestBase<TFixture>(TFixture fixture) : BulkUpdatesTestBase<TFixture>(fixture)
    where TFixture : AssociationsQueryFixtureBase, new()
{
    #region Delete

    [ConditionalFact]
    public virtual async Task Delete_entity_with_associations()
    {
        // Make sure foreign key constraints don't get in the way
        var deletableEntity = Fixture.Data.RootEntities.Where(e => !Fixture.Data.RootReferencingEntities.Any(re => re.Root == e)).First();

        await AssertDelete(
             ss => ss.Set<RootEntity>().Where(e => e.Name == deletableEntity.Name),
             rowsAffectedCount: 1);
    }

    // Should always fail (since the association is required), but (at least for now) may fail in different ways depending on the
    // association mapping type.
    [ConditionalFact]
    public virtual Task Delete_required_association()
        => AssertDelete(
            ss => ss.Set<RootEntity>().Select(c => c.RequiredRelated),
            rowsAffectedCount: 0);

    [ConditionalFact]
    public virtual Task Delete_optional_association()
        => AssertDelete(
            ss => ss.Set<RootEntity>().Select(c => c.OptionalRelated),
            rowsAffectedCount: 0);

    #endregion Delete

    #region Update properties

    [ConditionalFact]
    public virtual Task Update_property_inside_association()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            e => e,
            s => s.SetProperty(c => c.RequiredRelated.String, "foo_updated"),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_property_inside_association_with_special_chars()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(c => c.RequiredRelated.String == "{ this may/look:like JSON but it [isn't]: ממש ממש לאéèéè }"),
            e => e,
            s => s.SetProperty(c => c.RequiredRelated.String, c => "{ Some other/JSON:like text though it [isn't]: ממש ממש לאéèéè }"),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_property_inside_nested()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            e => e,
            s => s.SetProperty(c => c.RequiredRelated.RequiredNested.String, "foo_updated"),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_property_on_projected_association()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Select(c => c.RequiredRelated),
            a => a,
            s => s.SetProperty(c => c.String, "foo_updated"),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_property_on_projected_association_with_OrderBy_Skip()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Select(c => c.RequiredRelated).OrderBy(a => a.String).Skip(1),
            a => a,
            s => s.SetProperty(c => c.String, "foo_updated"),
            rowsAffectedCount: 3);

    [ConditionalFact]
    public virtual async Task Update_association_with_null_required_property()
    {
        using var context = Fixture.CreateContext();

        var invalidRelated = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredRelated;
        var originalValue = invalidRelated.String;
        invalidRelated.String = null!;

        await Assert.ThrowsAnyAsync<Exception>(() =>
            context.Set<RootEntity>().ExecuteUpdateAsync(s => s.SetProperty(x => x.RequiredRelated, invalidRelated)));

        // Make sure no update actually occurred in the database
        using (Fixture.ListLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(originalValue, (await context.Set<RootEntity>().SingleAsync(e => e.Id == 1)).RequiredRelated.String);
        }
    }

    #endregion Update properties

    #region Update association

    [ConditionalFact]
    public virtual Task Update_association_to_parameter()
    {
        var newRelated = new RelatedType
        {
            Id = 1000,
            Name = "Updated related name",
            Int = 80,
            String = "Updated nested string",
            Ints = [1, 2, 3],

            RequiredNested = new NestedType
            {
                Id = 1000,
                Name = "Updated nested name",
                Int = 80,
                String = "Updated nested string",
                Ints = [1, 2, 3]
            },
            OptionalNested = null,
            NestedCollection = []
        };

        return AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated, newRelated),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_nested_association_to_parameter()
    {
        var newNested = new NestedType
        {
            Id = 1000,
            Name = "Updated nested name",
            Int = 80,
            String = "Updated nested string",
            Ints = [1, 2, 4]
        };

        return AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated.RequiredNested, newNested),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_association_to_another_association()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.OptionalRelated, x => x.RequiredRelated),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_nested_association_to_another_nested_association()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated.OptionalNested, x => x.RequiredRelated.RequiredNested),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_association_to_inline()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(
                x => x.RequiredRelated,
                new RelatedType
                {
                    Id = 1000,
                    Name = "Updated related name",
                    Int = 70,
                    String = "Updated related string",
                    Ints = [1, 2, 4],

                    RequiredNested = new NestedType
                    {
                        Id = 1000,
                        Name = "Updated nested name",
                        Int = 80,
                        String = "Updated nested string",
                        Ints = [1, 2, 4]
                    },
                    OptionalNested = null,
                    NestedCollection = []
                }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_association_to_inline_with_lambda()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(
                x => x.RequiredRelated,
                x => new RelatedType
                {
                    Id = 1000,
                    Name = "Updated related name",
                    Int = 70,
                    String = "Updated related string",
                    Ints = new() { 1, 2, 4 },

                    RequiredNested = new NestedType
                    {
                        Id = 1000,
                        Name = "Updated nested name",
                        Int = 80,
                        String = "Updated nested string",
                        Ints = new() { 1, 2, 4 }
                    },
                    OptionalNested = null,
                    NestedCollection = new List<NestedType>()
                }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_nested_association_to_inline_with_lambda()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(
                x => x.RequiredRelated.RequiredNested,
                x => new NestedType
                {
                    Id = 1000,
                    Name = "Updated nested name",
                    Int = 80,
                    String = "Updated nested string",
                    Ints = new() { 1, 2, 4 }
                }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_association_to_null()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.OptionalRelated, (RelatedType?)null),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_association_to_null_with_lambda()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.OptionalRelated, x => null),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_association_to_null_parameter()
    {
        var nullRelated = (RelatedType?)null;

        return AssertUpdate(
                ss => ss.Set<RootEntity>(),
                c => c,
                s => s.SetProperty(x => x.OptionalRelated, nullRelated),
                rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual async Task Update_association_with_null_required_nested_association()
    {
        using var context = Fixture.CreateContext();

        var invalidRelated = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredRelated;
        var originalNested = invalidRelated.RequiredNested;
        invalidRelated.RequiredNested = null!;

        await Assert.ThrowsAnyAsync<Exception>(() =>
            context.Set<RootEntity>().ExecuteUpdateAsync(s => s.SetProperty(x => x.RequiredRelated, invalidRelated)));

        // Make sure no update actually occurred in the database
        using (Fixture.ListLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(
                originalNested.String,
                (await context.Set<RootEntity>().SingleAsync(e => e.Id == 1)).RequiredRelated.RequiredNested.String);
        }
    }

    #endregion Update association

    #region Update collection

    [ConditionalFact]
    public virtual Task Update_collection_to_parameter()
    {
        List<RelatedType> collection =
        [
            new()
            {
                Id = 1000,
                Name = "Updated related name1",
                Int = 80,
                String = "Updated related string1",
                Ints = [1, 2, 4],

                RequiredNested = new()
                {
                    Id = 1000,
                    Name = "Updated nested name1",
                    Int = 80,
                    String = "Updated nested string1",
                    Ints = [1, 2, 4]
                },
                OptionalNested = null,
                NestedCollection = []
            },
            new()
            {
                Id = 1001,
                Name = "Updated related name2",
                Int = 81,
                String = "Updated related string2",
                Ints = [1, 2, 4],

                RequiredNested = new()
                {
                    Id = 1001,
                    Name = "Updated nested name2",
                    Int = 81,
                    String = "Updated nested string2",
                    Ints = [1, 2, 4]
                },
                OptionalNested = null,
                NestedCollection = []
            }
        ];

        return AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RelatedCollection, collection),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_nested_collection_to_parameter()
    {
        List<NestedType> collection =
        [
            new()
            {
                Id = 1000,
                Name = "Updated nested name1",
                Int = 80,
                String = "Updated nested string1",
                Ints = [1, 2, 4]
            },
            new()
            {
                Id = 1001,
                Name = "Updated nested name2",
                Int = 81,
                String = "Updated nested string2",
                Ints = [1, 2, 4]
            },
        ];

        return AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated.NestedCollection, collection),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_nested_collection_to_inline_with_lambda()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(
                x => x.RequiredRelated.NestedCollection,
                x => new List<NestedType>
                {
                    new()
                    {
                        Id = 1000,
                        Name = "Updated nested name1",
                        Int = 80,
                        String = "Updated nested string1",
                        Ints = new() { 1, 2, 4 }
                    },
                    new()
                    {
                        Id = 1001,
                        Name = "Updated nested name2",
                        Int = 81,
                        String = "Updated nested string2",
                        Ints = new() { 1, 2, 4 }
                    }
                }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_collection_referencing_the_original_collection()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection.Count >= 2),
            c => c,
            s => s.SetProperty(
                e => e.RequiredRelated.NestedCollection,
                e => new List<NestedType> { e.RequiredRelated.NestedCollection[1], e.RequiredRelated.NestedCollection[0]}),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_nested_collection_to_another_nested_collection()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(e => e.OptionalRelated != null),
            c => c,
            s => s.SetProperty(
                x => x.RequiredRelated.NestedCollection,
                x => x.OptionalRelated!.NestedCollection),
            rowsAffectedCount: 6);

    [ConditionalFact]
    public virtual async Task Update_inside_structural_collection()
    {
        var nested = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredRelated.NestedCollection[1];
        nested.String += " Updated";

        await AssertUpdate(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.NestedCollection.Count >= 2),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated.NestedCollection[1], nested),
            rowsAffectedCount: 7);
    }

    #endregion Update collection

    #region Update primitive collection

    [ConditionalFact]
    public virtual Task Update_primitive_collection_to_constant()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated.Ints, x => new List<int> { 1, 2, 4 }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual async Task Update_primitive_collection_to_parameter()
    {
        List<int> ints = [1, 2, 4];

        await AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated.Ints, x => ints),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual async Task Update_primitive_collection_to_another_collection()
    {
        List<int> ints = [1, 2, 4];

        await AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated.OptionalNested!.Ints, x => x.RequiredRelated.RequiredNested.Ints),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_inside_primitive_collection()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredRelated.Ints.Count >= 2),
            c => c,
            s => s.SetProperty(x => x.RequiredRelated.Ints[1], 99),
            rowsAffectedCount: 7);

    #endregion Update primitive collection

    #region Multiple updates

    [ConditionalFact]
    public virtual Task Update_multiple_properties_inside_same_association()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            e => e,
            s => s
                .SetProperty(c => c.RequiredRelated.String, "foo_updated")
                .SetProperty(c => c.RequiredRelated.Int, 20),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_multiple_properties_inside_associations_and_on_entity_type()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(c => c.OptionalRelated != null),
            e => e,
            s => s
                .SetProperty(c => c.Name, c => c.Name + "Modified")
                .SetProperty(c => c.RequiredRelated.String, c => c.OptionalRelated!.String)
                .SetProperty(c => c.OptionalRelated!.RequiredNested.String, "foo_updated"),
            rowsAffectedCount: 6);

    [ConditionalFact]
    public virtual Task Update_multiple_projected_associations_via_anonymous_type()
        => AssertUpdate(
            ss => ss.Set<RootEntity>()
                .Where(c => c.OptionalRelated != null)
                .Select(c => new
                {
                    c.RequiredRelated,
                    c.OptionalRelated,
                    RootEntity = c
                }),
            x => x.RootEntity,
            s => s
                .SetProperty(c => c.RequiredRelated.String, c => c.OptionalRelated!.String)
                .SetProperty(c => c.OptionalRelated!.String, "foo_updated"),
            rowsAffectedCount: 6);

    #endregion Multiple updates

    protected static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected static async Task AssertTranslationFailedWithDetails(string details, Func<Task> query)
        => Assert.Contains(
            CoreStrings.NonQueryTranslationFailedWithDetails("", details)[21..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query)).Message);
}
