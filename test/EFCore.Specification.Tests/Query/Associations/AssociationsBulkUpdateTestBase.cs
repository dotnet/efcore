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
    public virtual Task Delete_required_associate()
        => AssertDelete(
            ss => ss.Set<RootEntity>().Select(c => c.RequiredAssociate),
            rowsAffectedCount: 0);

    [ConditionalFact]
    public virtual Task Delete_optional_associate()
        => AssertDelete(
            ss => ss.Set<RootEntity>().Select(c => c.OptionalAssociate),
            rowsAffectedCount: 0);

    #endregion Delete

    #region Update properties

    [ConditionalFact]
    public virtual Task Update_property_inside_associate()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            e => e,
            s => s.SetProperty(c => c.RequiredAssociate.String, "foo_updated"),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_property_inside_associate_with_special_chars()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(c => c.RequiredAssociate.String == "{ this may/look:like JSON but it [isn't]: ממש ממש לאéèéè }"),
            e => e,
            s => s.SetProperty(c => c.RequiredAssociate.String, c => "{ Some other/JSON:like text though it [isn't]: ממש ממש לאéèéè }"),
            rowsAffectedCount: 1);

    [ConditionalFact]
    public virtual Task Update_property_inside_nested_associate()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            e => e,
            s => s.SetProperty(c => c.RequiredAssociate.RequiredNestedAssociate.String, "foo_updated"),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_property_on_projected_associate()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Select(c => c.RequiredAssociate),
            a => a,
            s => s.SetProperty(c => c.String, "foo_updated"),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_property_on_projected_associate_with_OrderBy_Skip()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Select(c => c.RequiredAssociate).OrderBy(a => a.String).Skip(1),
            a => a,
            s => s.SetProperty(c => c.String, "foo_updated"),
            rowsAffectedCount: 3);

    [ConditionalFact]
    public virtual async Task Update_associate_with_null_required_property()
    {
        using var context = Fixture.CreateContext();

        var invalidAssociate = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredAssociate;
        var originalValue = invalidAssociate.String;
        invalidAssociate.String = null!;

        await Assert.ThrowsAnyAsync<Exception>(() =>
            context.Set<RootEntity>().ExecuteUpdateAsync(s => s.SetProperty(x => x.RequiredAssociate, invalidAssociate)));

        // Make sure no update actually occurred in the database
        using (Fixture.ListLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(originalValue, (await context.Set<RootEntity>().SingleAsync(e => e.Id == 1)).RequiredAssociate.String);
        }
    }

    #endregion Update properties

    #region Update associate

    [ConditionalFact]
    public virtual Task Update_associate_to_parameter()
    {
        var newAssociate = new AssociateType
        {
            Id = 1000,
            Name = "Updated associate name",
            Int = 80,
            String = "Updated nested string",
            Ints = [1, 2, 3],

            RequiredNestedAssociate = new NestedAssociateType
            {
                Id = 1000,
                Name = "Updated nested name",
                Int = 80,
                String = "Updated nested string",
                Ints = [1, 2, 3]
            },
            OptionalNestedAssociate = null,
            NestedCollection = []
        };

        return AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredAssociate, newAssociate),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_nested_associate_to_parameter()
    {
        var newNested = new NestedAssociateType
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
            s => s.SetProperty(x => x.RequiredAssociate.RequiredNestedAssociate, newNested),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_associate_to_another_associate()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.OptionalAssociate, x => x.RequiredAssociate),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_nested_associate_to_another_nested_associate()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredAssociate.OptionalNestedAssociate, x => x.RequiredAssociate.RequiredNestedAssociate),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_associate_to_inline()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(
                x => x.RequiredAssociate,
                new AssociateType
                {
                    Id = 1000,
                    Name = "Updated associate name",
                    Int = 70,
                    String = "Updated associate string",
                    Ints = [1, 2, 4],

                    RequiredNestedAssociate = new NestedAssociateType
                    {
                        Id = 1000,
                        Name = "Updated nested name",
                        Int = 80,
                        String = "Updated nested string",
                        Ints = [1, 2, 4]
                    },
                    OptionalNestedAssociate = null,
                    NestedCollection = []
                }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_associate_to_inline_with_lambda()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(
                x => x.RequiredAssociate,
                x => new AssociateType
                {
                    Id = 1000,
                    Name = "Updated associate name",
                    Int = 70,
                    String = "Updated associate string",
                    Ints = new() { 1, 2, 4 },

                    RequiredNestedAssociate = new NestedAssociateType
                    {
                        Id = 1000,
                        Name = "Updated nested name",
                        Int = 80,
                        String = "Updated nested string",
                        Ints = new() { 1, 2, 4 }
                    },
                    OptionalNestedAssociate = null,
                    NestedCollection = new List<NestedAssociateType>()
                }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_nested_associate_to_inline_with_lambda()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(
                x => x.RequiredAssociate.RequiredNestedAssociate,
                x => new NestedAssociateType
                {
                    Id = 1000,
                    Name = "Updated nested name",
                    Int = 80,
                    String = "Updated nested string",
                    Ints = new() { 1, 2, 4 }
                }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_associate_to_null()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.OptionalAssociate, (AssociateType?)null),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_associate_to_null_with_lambda()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.OptionalAssociate, x => null),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_associate_to_null_parameter()
    {
        var nullAssociate = (AssociateType?)null;

        return AssertUpdate(
                ss => ss.Set<RootEntity>(),
                c => c,
                s => s.SetProperty(x => x.OptionalAssociate, nullAssociate),
                rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual async Task Update_required_nested_associate_to_null()
    {
        using var context = Fixture.CreateContext();

        var invalidAssociate = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredAssociate;
        var originalNested = invalidAssociate.RequiredNestedAssociate;
        invalidAssociate.RequiredNestedAssociate = null!;

        await Assert.ThrowsAnyAsync<Exception>(() =>
            context.Set<RootEntity>().ExecuteUpdateAsync(s => s.SetProperty(x => x.RequiredAssociate, invalidAssociate)));

        // Make sure no update actually occurred in the database
        using (Fixture.ListLoggerFactory.SuspendRecordingEvents())
        {
            Assert.Equal(
                originalNested.String,
                (await context.Set<RootEntity>().SingleAsync(e => e.Id == 1)).RequiredAssociate.RequiredNestedAssociate.String);
        }
    }

    #endregion Update associate

    #region Update collection

    [ConditionalFact]
    public virtual Task Update_collection_to_parameter()
    {
        List<AssociateType> collection =
        [
            new()
            {
                Id = 1000,
                Name = "Updated associate name1",
                Int = 80,
                String = "Updated associate string1",
                Ints = [1, 2, 4],

                RequiredNestedAssociate = new()
                {
                    Id = 1000,
                    Name = "Updated nested name1",
                    Int = 80,
                    String = "Updated nested string1",
                    Ints = [1, 2, 4]
                },
                OptionalNestedAssociate = null,
                NestedCollection = []
            },
            new()
            {
                Id = 1001,
                Name = "Updated associate name2",
                Int = 81,
                String = "Updated associate string2",
                Ints = [1, 2, 4],

                RequiredNestedAssociate = new()
                {
                    Id = 1001,
                    Name = "Updated nested name2",
                    Int = 81,
                    String = "Updated nested string2",
                    Ints = [1, 2, 4]
                },
                OptionalNestedAssociate = null,
                NestedCollection = []
            }
        ];

        return AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.AssociateCollection, collection),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_nested_collection_to_parameter()
    {
        List<NestedAssociateType> collection =
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
            s => s.SetProperty(x => x.RequiredAssociate.NestedCollection, collection),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_nested_collection_to_inline_with_lambda()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(
                x => x.RequiredAssociate.NestedCollection,
                x => new List<NestedAssociateType>
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
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection.Count >= 2),
            c => c,
            s => s.SetProperty(
                e => e.RequiredAssociate.NestedCollection,
                e => new List<NestedAssociateType> { e.RequiredAssociate.NestedCollection[1], e.RequiredAssociate.NestedCollection[0]}),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_nested_collection_to_another_nested_collection()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(e => e.OptionalAssociate != null),
            c => c,
            s => s.SetProperty(
                x => x.RequiredAssociate.NestedCollection,
                x => x.OptionalAssociate!.NestedCollection),
            rowsAffectedCount: 6);

    [ConditionalFact]
    public virtual async Task Update_inside_structural_collection()
    {
        var nested = Fixture.Data.RootEntities.Single(e => e.Id == 1).RequiredAssociate.NestedCollection[1];
        nested.String += " Updated";

        await AssertUpdate(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.NestedCollection.Count >= 2),
            c => c,
            s => s.SetProperty(x => x.RequiredAssociate.NestedCollection[1], nested),
            rowsAffectedCount: 7);
    }

    #endregion Update collection

    #region Update primitive collection

    [ConditionalFact]
    public virtual Task Update_primitive_collection_to_constant()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredAssociate.Ints, x => new List<int> { 1, 2, 4 }),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual async Task Update_primitive_collection_to_parameter()
    {
        List<int> ints = [1, 2, 4];

        await AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredAssociate.Ints, x => ints),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual async Task Update_primitive_collection_to_another_collection()
    {
        List<int> ints = [1, 2, 4];

        await AssertUpdate(
            ss => ss.Set<RootEntity>(),
            c => c,
            s => s.SetProperty(x => x.RequiredAssociate.OptionalNestedAssociate!.Ints, x => x.RequiredAssociate.RequiredNestedAssociate.Ints),
            rowsAffectedCount: 7);
    }

    [ConditionalFact]
    public virtual Task Update_inside_primitive_collection()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(e => e.RequiredAssociate.Ints.Count >= 2),
            c => c,
            s => s.SetProperty(x => x.RequiredAssociate.Ints[1], 99),
            rowsAffectedCount: 7);

    #endregion Update primitive collection

    #region Multiple updates

    [ConditionalFact]
    public virtual Task Update_multiple_properties_inside_same_associate()
        => AssertUpdate(
            ss => ss.Set<RootEntity>(),
            e => e,
            s => s
                .SetProperty(c => c.RequiredAssociate.String, "foo_updated")
                .SetProperty(c => c.RequiredAssociate.Int, 20),
            rowsAffectedCount: 7);

    [ConditionalFact]
    public virtual Task Update_multiple_properties_inside_associates_and_on_entity_type()
        => AssertUpdate(
            ss => ss.Set<RootEntity>().Where(c => c.OptionalAssociate != null),
            e => e,
            s => s
                .SetProperty(c => c.Name, c => c.Name + "Modified")
                .SetProperty(c => c.RequiredAssociate.String, c => c.OptionalAssociate!.String)
                .SetProperty(c => c.OptionalAssociate!.RequiredNestedAssociate.String, "foo_updated"),
            rowsAffectedCount: 6);

    [ConditionalFact]
    public virtual Task Update_multiple_projected_associates_via_anonymous_type()
        => AssertUpdate(
            ss => ss.Set<RootEntity>()
                .Where(c => c.OptionalAssociate != null)
                .Select(c => new
                {
                    c.RequiredAssociate,
                    c.OptionalAssociate,
                    RootEntity = c
                }),
            x => x.RootEntity,
            s => s
                .SetProperty(c => c.RequiredAssociate.String, c => c.OptionalAssociate!.String)
                .SetProperty(c => c.OptionalAssociate!.String, "foo_updated"),
            rowsAffectedCount: 6);

    #endregion Multiple updates

    protected static async Task AssertTranslationFailed(Func<Task> query)
        => Assert.Contains(
            CoreStrings.TranslationFailed("")[48..],
            (await Assert.ThrowsAsync<InvalidOperationException>(query))
            .Message);

    protected static async Task AssertTranslationFailedWithDetails(string details, Func<Task> query)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(query);
        Assert.StartsWith(CoreStrings.NonQueryTranslationFailed("")[0..^1], exception.Message);
        var innerException = Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal(details, innerException.Message);
    }
}
