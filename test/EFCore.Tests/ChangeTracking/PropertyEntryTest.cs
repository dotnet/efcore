// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
#pragma warning disable CS0414 // Field is assigned but its value is never used

using System.ComponentModel;
using System.Runtime.CompilerServices;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class PropertyEntryTest
{
    [ConditionalFact]
    public void Setting_IsModified_should_not_be_dependent_on_other_properties()
    {
        Guid id;

        using (var context = new UserContext())
        {
            id = context.Add(
                new User { Name = "A", LongName = "B" }).Entity.Id;

            context.SaveChanges();
        }

        using (var context = new UserContext())
        {
            var user = context.Attach(
                new User
                {
                    Id = id,
                    Name = "NewA",
                    LongName = "NewB"
                }).Entity;

            context.Entry(user).Property(x => x.Name).IsModified = false;
            context.Entry(user).Property(x => x.LongName).IsModified = true;

            Assert.False(context.Entry(user).Property(x => x.Name).IsModified);
            Assert.True(context.Entry(user).Property(x => x.LongName).IsModified);

            context.SaveChanges();
        }

        using (var context = new UserContext())
        {
            var user = context.Find<User>(id)!;

            Assert.Equal("A", user.Name);
            Assert.Equal("NewB", user.LongName);
        }
    }

    [ConditionalFact]
    public void SetValues_with_IsModified_can_mark_a_set_of_values_as_changed()
    {
        Guid id;

        using (var context = new UserContext())
        {
            id = context.Add(
                new User { Name = "A", LongName = "B" }).Entity.Id;

            context.SaveChanges();
        }

        using (var context = new UserContext())
        {
            var disconnectedEntity = new User { Id = id, LongName = "NewLongName" };
            var trackedEntity = context.Find<User>(id)!;

            Assert.Equal("A", trackedEntity.Name);
            Assert.Equal("B", trackedEntity.LongName);

            var entry = context.Entry(trackedEntity);

            entry.CurrentValues.SetValues(disconnectedEntity);

            Assert.Null(trackedEntity.Name);
            Assert.Equal("NewLongName", trackedEntity.LongName);

            Assert.False(entry.Property(e => e.Id).IsModified);
            Assert.True(entry.Property(e => e.Name).IsModified);
            Assert.True(entry.Property(e => e.LongName).IsModified);

            var internalEntry = entry.GetInfrastructure();

            Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.Id).Metadata));
            Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.Name).Metadata));
            Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.LongName).Metadata));

            foreach (var property in entry.Properties)
            {
                property.IsModified = property.Metadata.Name == "LongName";
            }

            Assert.False(entry.Property(e => e.Id).IsModified);
            Assert.False(entry.Property(e => e.Name).IsModified);
            Assert.True(entry.Property(e => e.LongName).IsModified);

            Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.Id).Metadata));
            Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.Name).Metadata));
            Assert.False(internalEntry.IsConceptualNull(entry.Property(e => e.LongName).Metadata));

            context.SaveChanges();
        }
    }

    private class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string LongName { get; set; } = null!;
    }

    private class UserContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(GetType().FullName!);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<User>(
                b =>
                {
                    b.Property(e => e.Name).IsRequired();
                    b.Property(e => e.LongName).IsRequired();
                });
    }

    [ConditionalFact]
    public void Setting_IsModified_is_not_reset_by_OriginalValues()
    {
        Guid id;
        using (var context = new UserContext())
        {
            id = context.Add(
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "A",
                    LongName = "B"
                }).Entity.Id;

            context.SaveChanges();
        }

        using (var context = new UserContext())
        {
            var user = context.Update(
                new User { Id = id }).Entity;

            user.Name = "A2";
            user.LongName = "B2";

            context.Entry(user).Property(x => x.Name).IsModified = false;
            Assert.False(context.Entry(user).Property(x => x.Name).IsModified);

            context.SaveChanges();
        }

        using (var context = new UserContext())
        {
            var user = context.Find<User>(id)!;

            Assert.Equal("A", user.Name);
            Assert.Equal("B2", user.LongName);
        }
    }

    [ConditionalFact]
    public void Can_get_name()
        => Can_get_name_helper<Wotty>();

    [ConditionalFact]
    public void Can_get_name_with_object_field()
        => Can_get_name_helper<ObjectWotty>();

    private void Can_get_name_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entry = context
            .Entry(
                new TWotty
                {
                    Id = 1,
                    Primate = "Monkey",
                    RequiredPrimate = "Tarsier"
                })
            .GetInfrastructure();

        entry.SetEntityState(EntityState.Unchanged);

        Assert.Equal("Primate", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_current_value()
        => Can_get_current_value_helper<Wotty>();

    [ConditionalFact]
    public void Can_get_current_value_with_object_field()
        => Can_get_current_value_helper<ObjectWotty>();

    private void Can_get_current_value_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entry = context
            .Entry(
                new TWotty
                {
                    Id = 1,
                    Primate = "Monkey",
                    RequiredPrimate = "Tarsier"
                })
            .GetInfrastructure();

        entry.SetEntityState(EntityState.Unchanged);

        Assert.Equal("Monkey", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).CurrentValue);
        Assert.Equal("Tarsier", new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).CurrentValue);
    }

    [ConditionalFact]
    public void Can_set_current_value()
        => Can_set_current_value_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_current_value_with_object_field()
        => Can_set_current_value_helper<ObjectWotty>();

    private void Can_set_current_value_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty
        {
            Id = 1,
            Primate = "Monkey",
            RequiredPrimate = "Tarsier"
        };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(EntityState.Unchanged);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).CurrentValue = "Chimp";
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).CurrentValue = "Bushbaby";

        Assert.Equal("Chimp", entity.Primate);
        Assert.Equal("Bushbaby", entity.RequiredPrimate);

        context.ChangeTracker.DetectChanges();

        Assert.Equal("Chimp", entity.Primate);
        Assert.Equal("Bushbaby", entity.RequiredPrimate);
    }

    [ConditionalFact]
    public void Can_set_current_value_to_null()
        => Can_set_current_value_to_null_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_current_value_to_null_with_object_field()
        => Can_set_current_value_to_null_helper<ObjectWotty>();

    private void Can_set_current_value_to_null_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty
        {
            Id = 1,
            Primate = "Monkey",
            RequiredPrimate = "Tarsier"
        };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(EntityState.Unchanged);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).CurrentValue = null;
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).CurrentValue = null;

        Assert.Null(entity.Primate);
        Assert.Null(entity.RequiredPrimate);

        context.ChangeTracker.DetectChanges();

        Assert.Null(entity.Primate);
        Assert.Null(entity.RequiredPrimate);
    }

    [ConditionalFact]
    public void Can_set_and_get_original_value()
        => Can_set_and_get_original_value_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_and_get_original_value_with_object_field()
        => Can_set_and_get_original_value_helper<ObjectWotty>();

    private void Can_set_and_get_original_value_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty
        {
            Id = 1,
            Primate = "Monkey",
            RequiredPrimate = "Tarsier"
        };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(EntityState.Unchanged);

        Assert.Equal("Monkey", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Equal("Tarsier", new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = "Chimp";
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue = "Bushbaby";

        Assert.Equal("Chimp", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Equal("Monkey", entity.Primate);

        Assert.Equal("Bushbaby", new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue);
        Assert.Equal("Tarsier", entity.RequiredPrimate);

        context.ChangeTracker.DetectChanges();

        Assert.Equal("Chimp", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Equal("Monkey", entity.Primate);

        Assert.Equal("Bushbaby", new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue);
        Assert.Equal("Tarsier", entity.RequiredPrimate);
    }

    [ConditionalFact]
    public void Can_set_and_get_original_value_starting_null()
        => Can_set_and_get_original_value_starting_null_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_and_get_original_value_starting_null_with_object_field()
        => Can_set_and_get_original_value_starting_null_helper<ObjectWotty>();

    private void Can_set_and_get_original_value_starting_null_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty { Id = 1 };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(EntityState.Unchanged);

        Assert.Null(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Null(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = "Chimp";
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue = "Bushbaby";

        Assert.Equal("Chimp", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Null(entity.Primate);

        Assert.Equal("Bushbaby", new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue);
        Assert.Null(entity.RequiredPrimate);

        context.ChangeTracker.DetectChanges();

        Assert.Equal("Chimp", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Null(entity.Primate);

        Assert.Equal("Bushbaby", new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue);
        Assert.Null(entity.RequiredPrimate);
    }

    [ConditionalFact]
    public void Can_set_original_value_to_null()
        => Can_set_original_value_to_null_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_original_value_to_null_with_object_field()
        => Can_set_original_value_to_null_helper<ObjectWotty>();

    private void Can_set_original_value_to_null_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty
        {
            Id = 1,
            Primate = "Monkey",
            RequiredPrimate = "Tarsier"
        };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(EntityState.Unchanged);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = null;
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue = null;

        Assert.Null(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Null(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue);

        context.ChangeTracker.DetectChanges();

        Assert.Null(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Null(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).OriginalValue);
    }

    [ConditionalFact]
    public void Can_set_and_clear_modified_on_Modified_entity()
        => Can_set_and_clear_modified_on_Modified_entity_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_and_clear_modified_on_Modified_entity_with_object_field()
        => Can_set_and_clear_modified_on_Modified_entity_helper<ObjectWotty>();

    private void Can_set_and_clear_modified_on_Modified_entity_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty { Id = 1 };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(EntityState.Modified);

        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        context.ChangeTracker.DetectChanges();

        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified = false;
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified = false;

        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        context.ChangeTracker.DetectChanges();

        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified = true;
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified = true;

        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        context.ChangeTracker.DetectChanges();

        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Deleted)]
    public void Can_set_and_clear_modified_on_Added_or_Deleted_entity(EntityState initialState)
        => Can_set_and_clear_modified_on_Added_or_Deleted_entity_helper<Wotty>(initialState);

    [ConditionalTheory]
    [InlineData(EntityState.Added)]
    [InlineData(EntityState.Deleted)]
    public void Can_set_and_clear_modified_on_Added_or_Deleted_entity_with_object_field(EntityState initialState)
        => Can_set_and_clear_modified_on_Added_or_Deleted_entity_helper<ObjectWotty>(initialState);

    private void Can_set_and_clear_modified_on_Added_or_Deleted_entity_helper<TWotty>(EntityState initialState)
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty { Id = 1 };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(initialState);

        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified = true;
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified = true;
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        context.ChangeTracker.DetectChanges();
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified = false;
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified = false;
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        context.ChangeTracker.DetectChanges();
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);
    }

    [ConditionalTheory]
    [InlineData(EntityState.Detached)]
    [InlineData(EntityState.Unchanged)]
    public void Can_set_and_clear_modified_on_Unchanged_or_Detached_entity(EntityState initialState)
        => Can_set_and_clear_modified_on_Unchanged_or_Detached_entity_helper<Wotty>(initialState);

    [ConditionalTheory]
    [InlineData(EntityState.Detached)]
    [InlineData(EntityState.Unchanged)]
    public void Can_set_and_clear_modified_on_Unchanged_or_Detached_entity_with_object_field(EntityState initialState)
        => Can_set_and_clear_modified_on_Unchanged_or_Detached_entity_helper<ObjectWotty>(initialState);

    private void Can_set_and_clear_modified_on_Unchanged_or_Detached_entity_helper<TWotty>(EntityState initialState)
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty { Id = 1 };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(initialState);

        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified = true;
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified = true;
        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        context.ChangeTracker.DetectChanges();
        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.True(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified = false;
        new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified = false;
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);

        context.ChangeTracker.DetectChanges();
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
        Assert.False(new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!).IsModified);
    }

    [ConditionalFact]
    public void Can_reject_changes_when_clearing_modified_flag()
        => Can_reject_changes_when_clearing_modified_flag_helper<Wotty>();

    [ConditionalFact]
    public void Can_reject_changes_when_clearing_modified_flag_with_object_field()
        => Can_reject_changes_when_clearing_modified_flag_helper<ObjectWotty>();

    private void Can_reject_changes_when_clearing_modified_flag_helper<TWotty>()
        where TWotty : IWotty, new()
    {
        using var context = new PrimateContext();
        var entity = new TWotty
        {
            Id = 1,
            Primate = "Monkey",
            Marmate = "Bovril",
            RequiredPrimate = "Tarsier"
        };
        var entry = context.Entry(entity).GetInfrastructure();
        entry.SetEntityState(EntityState.Unchanged);

        var primateEntry =
            new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!) { OriginalValue = "Chimp", IsModified = true };

        var marmateEntry =
            new PropertyEntry(entry, entry.EntityType.FindProperty("Marmate")!) { OriginalValue = "Marmite", IsModified = true };

        var requiredEntry =
            new PropertyEntry(entry, entry.EntityType.FindProperty("RequiredPrimate")!) { OriginalValue = "Bushbaby", IsModified = true };

        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.Equal("Monkey", entity.Primate);
        Assert.Equal("Bovril", entity.Marmate);
        Assert.Equal("Tarsier", entity.RequiredPrimate);

        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.Equal("Monkey", entity.Primate);
        Assert.Equal("Bovril", entity.Marmate);
        Assert.Equal("Tarsier", entity.RequiredPrimate);

        primateEntry.IsModified = false;

        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.Equal("Chimp", entity.Primate);
        Assert.Equal("Bovril", entity.Marmate);
        Assert.Equal("Tarsier", entity.RequiredPrimate);

        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.Equal("Chimp", entity.Primate);
        Assert.Equal("Bovril", entity.Marmate);
        Assert.Equal("Tarsier", entity.RequiredPrimate);

        marmateEntry.IsModified = false;

        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.Equal("Chimp", entity.Primate);
        Assert.Equal("Marmite", entity.Marmate);
        Assert.Equal("Tarsier", entity.RequiredPrimate);

        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.Equal("Chimp", entity.Primate);
        Assert.Equal("Marmite", entity.Marmate);
        Assert.Equal("Tarsier", entity.RequiredPrimate);

        requiredEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.Equal("Chimp", entity.Primate);
        Assert.Equal("Marmite", entity.Marmate);
        Assert.Equal("Bushbaby", entity.RequiredPrimate);

        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.Equal("Chimp", entity.Primate);
        Assert.Equal("Marmite", entity.Marmate);
        Assert.Equal("Bushbaby", entity.RequiredPrimate);
    }

    [ConditionalFact]
    public void Can_get_name_generic()
        => Can_get_name_generic_helper<Wotty>();

    [ConditionalFact]
    public void Can_get_name_generic_with_object_field()
        => Can_get_name_generic_helper<ObjectWotty>();

    private void Can_get_name_generic_helper<TWotty>()
        where TWotty : class, IWotty, new()
    {
        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            new TWotty { Id = 1, Primate = "Monkey" });

        Assert.Equal("Primate", new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_current_value_generic()
        => Can_get_current_value_generic_helper<Wotty>();

    [ConditionalFact]
    public void Can_get_current_value_generic_with_object_field()
        => Can_get_current_value_generic_helper<ObjectWotty>();

    private void Can_get_current_value_generic_helper<TWotty>()
        where TWotty : class, IWotty, new()
    {
        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            new TWotty { Id = 1, Primate = "Monkey" });

        Assert.Equal("Monkey", new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).CurrentValue);
    }

    [ConditionalFact]
    public void Can_set_current_value_generic()
        => Can_set_current_value_generic_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_current_value_generic_with_object_field()
        => Can_set_current_value_generic_helper<ObjectWotty>();

    private void Can_set_current_value_generic_helper<TWotty>()
        where TWotty : class, IWotty, new()
    {
        var entity = new TWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).CurrentValue = "Chimp";

        Assert.Equal("Chimp", entity.Primate);
    }

    [ConditionalFact]
    public void Can_set_current_value_to_null_generic()
        => Can_set_current_value_to_null_generic_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_current_value_to_null_generic_with_object_field()
        => Can_set_current_value_to_null_generic_helper<ObjectWotty>();

    private void Can_set_current_value_to_null_generic_helper<TWotty>()
        where TWotty : class, IWotty, new()
    {
        var entity = new TWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        new PropertyEntry<Wotty, string?>(entry, entry.EntityType.FindProperty("Primate")!).CurrentValue = null;

        Assert.Null(entity.Primate);
    }

    [ConditionalFact]
    public void Can_set_and_get_original_value_generic()
        => Can_set_and_get_original_value_generic_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_and_get_original_value_generic_with_object_field()
        => Can_set_and_get_original_value_generic_helper<ObjectWotty>();

    private void Can_set_and_get_original_value_generic_helper<TWotty>()
        where TWotty : class, IWotty, new()
    {
        var entity = new TWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        Assert.Equal("Monkey", new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);

        new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = "Chimp";

        Assert.Equal("Chimp", new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Equal("Monkey", entity.Primate);
    }

    [ConditionalFact]
    public void Can_set_original_value_to_null_generic()
        => Can_set_original_value_to_null_generic_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_original_value_to_null_generic_with_object_field()
        => Can_set_original_value_to_null_generic_helper<ObjectWotty>();

    private void Can_set_original_value_to_null_generic_helper<TWotty>()
        where TWotty : class, IWotty, new()
    {
        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            new TWotty { Id = 1, Primate = "Monkey" });

        new PropertyEntry<Wotty, string?>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = null;

        Assert.Null(new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
    }

    [ConditionalFact]
    public void Can_set_and_clear_modified_generic()
        => Can_set_and_clear_modified_generic_helper<Wotty>();

    [ConditionalFact]
    public void Can_set_and_clear_modified_generic_with_object_field()
        => Can_set_and_clear_modified_generic_helper<ObjectWotty>();

    private void Can_set_and_clear_modified_generic_helper<TWotty>()
        where TWotty : class, IWotty, new()
    {
        var entity = new TWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        Assert.False(new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).IsModified);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified = true;

        Assert.True(new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).IsModified);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).IsModified = false;

        Assert.False(new PropertyEntry<Wotty, string>(entry, entry.EntityType.FindProperty("Primate")!).IsModified);
    }

    [ConditionalFact]
    public void Can_set_and_get_original_value_notifying_entities()
    {
        var entity = new NotifyingWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        Assert.Equal("Monkey", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = "Chimp";

        Assert.Equal("Chimp", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Equal("Monkey", entity.Primate);
    }

    [ConditionalFact]
    public void Can_set_original_value_to_null_notifying_entities()
    {
        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            new NotifyingWotty { Id = 1, Primate = "Monkey" });

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = null;

        Assert.Null(new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
    }

    [ConditionalFact]
    public void Can_set_and_get_original_value_generic_notifying_entities()
    {
        var entity = new NotifyingWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        Assert.Equal("Monkey", new PropertyEntry<NotifyingWotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);

        new PropertyEntry<NotifyingWotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = "Chimp";

        Assert.Equal("Chimp", new PropertyEntry<NotifyingWotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Equal("Monkey", entity.Primate);
    }

    [ConditionalFact]
    public void Can_set_original_value_to_null_generic_notifying_entities()
    {
        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            new NotifyingWotty { Id = 1, Primate = "Monkey" });

        new PropertyEntry<NotifyingWotty, string?>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = null;

        Assert.Null(new PropertyEntry<NotifyingWotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
    }

    [ConditionalFact]
    public void Can_set_and_get_concurrency_token_original_value_full_notification_entities()
    {
        var entity = new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        Assert.Equal("Monkey", new PropertyEntry(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue);

        new PropertyEntry(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue = "Chimp";

        Assert.Equal("Chimp", new PropertyEntry(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue);
        Assert.Equal("Monkey", entity.ConcurrentPrimate);
    }

    [ConditionalFact]
    public void Can_set_concurrency_token_original_value_to_null_full_notification_entities()
    {
        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" });

        new PropertyEntry(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue = null;

        Assert.Null(new PropertyEntry(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue);
    }

    [ConditionalFact]
    public void Can_set_and_get_concurrency_token_original_value_generic_full_notification_entities()
    {
        var entity = new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        Assert.Equal(
            "Monkey",
            new PropertyEntry<FullyNotifyingWotty, string>(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue);

        new PropertyEntry<FullyNotifyingWotty, string>(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue = "Chimp";

        Assert.Equal(
            "Chimp",
            new PropertyEntry<FullyNotifyingWotty, string>(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue);
        Assert.Equal("Monkey", entity.ConcurrentPrimate);
    }

    [ConditionalFact]
    public void Can_set_concurrency_token_original_value_to_null_generic_full_notification_entities()
    {
        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" });

        new PropertyEntry<FullyNotifyingWotty, string?>(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue = null;

        Assert.Null(
            new PropertyEntry<FullyNotifyingWotty, string>(entry, entry.EntityType.FindProperty("ConcurrentPrimate")!).OriginalValue);
    }

    [ConditionalFact]
    public void Cannot_set_or_get_original_value_when_not_tracked()
    {
        var entity = new FullyNotifyingWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        var propertyEntry = new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!);

        Assert.Equal(
            CoreStrings.OriginalValueNotTracked("Primate", "FullyNotifyingWotty"),
            Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue).Message);

        Assert.Equal(
            CoreStrings.OriginalValueNotTracked("Primate", "FullyNotifyingWotty"),
            Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = "Chimp").Message);
    }

    [ConditionalFact]
    public void Cannot_set_or_get_original_value_when_not_tracked_generic()
    {
        var entity = new FullyNotifyingWotty { Id = 1, ConcurrentPrimate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(),
            EntityState.Unchanged,
            entity);

        var propertyEntry = new PropertyEntry<FullyNotifyingWotty, string>(entry, entry.EntityType.FindProperty("Primate")!);

        Assert.Equal(
            CoreStrings.OriginalValueNotTracked("Primate", "FullyNotifyingWotty"),
            Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue).Message);

        Assert.Equal(
            CoreStrings.OriginalValueNotTracked("Primate", "FullyNotifyingWotty"),
            Assert.Throws<InvalidOperationException>(() => propertyEntry.OriginalValue = "Chimp").Message);
    }

    [ConditionalFact]
    public void Can_set_or_get_original_value_when_property_explicitly_marked_to_be_tracked()
    {
        var entity = new FullyNotifyingWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues),
            EntityState.Unchanged,
            entity);

        Assert.Equal("Monkey", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);

        new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = "Chimp";

        Assert.Equal("Chimp", new PropertyEntry(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Equal("Monkey", entity.Primate);
    }

    [ConditionalFact]
    public void Can_set_or_get_original_value_when_property_explicitly_marked_to_be_tracked_generic()
    {
        var entity = new FullyNotifyingWotty { Id = 1, Primate = "Monkey" };

        var entry = InMemoryTestHelpers.Instance.CreateInternalEntry(
            BuildModel(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues),
            EntityState.Unchanged,
            entity);

        Assert.Equal(
            "Monkey", new PropertyEntry<FullyNotifyingWotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);

        new PropertyEntry<FullyNotifyingWotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue = "Chimp";

        Assert.Equal(
            "Chimp", new PropertyEntry<FullyNotifyingWotty, string>(entry, entry.EntityType.FindProperty("Primate")!).OriginalValue);
        Assert.Equal("Monkey", entity.Primate);
    }

    [ConditionalFact]
    public void Can_get_name_for_complex_property()
    {
        using var context = new YogurtContext();
        var entry = context.Entry(
            new Yogurt
            {
                Culture = new Culture
                {
                    License = new License
                    {
                        Charge = 1.0m,
                        Tag = new Tag { Text = "Ta1" },
                        Title = "Ti1",
                        Tog = new Tog { Text = "To1" }
                    },
                    Manufacturer = new Manufacturer
                    {
                        Name = "M1",
                        Rating = 7,
                        Tag = new Tag { Text = "Ta2" },
                        Tog = new Tog { Text = "To2" }
                    },
                    Rating = 8,
                    Species = "S1",
                    Validation = false
                },
                Milk = new Milk
                {
                    License = new License
                    {
                        Charge = 1.0m,
                        Tag = new Tag { Text = "Ta1" },
                        Title = "Ti1",
                        Tog = new Tog { Text = "To1" }
                    },
                    Manufacturer = new Manufacturer
                    {
                        Name = "M1",
                        Rating = 7,
                        Tag = new Tag { Text = "Ta2" },
                        Tog = new Tog { Text = "To2" }
                    },
                    Rating = 8,
                    Species = "S1",
                    Validation = false
                },
                FieldCulture = new FieldCulture
                {
                    License = new FieldLicense
                    {
                        Charge = 1.0m,
                        Tag = new FieldTag { Text = "Ta1" },
                        Title = "Ti1",
                        Tog = new FieldTog { Text = "To1" }
                    },
                    Manufacturer = new FieldManufacturer
                    {
                        Name = "M1",
                        Rating = 7,
                        Tag = new FieldTag { Text = "Ta2" },
                        Tog = new FieldTog { Text = "To2" }
                    },
                    Rating = 8,
                    Species = "S1",
                    Validation = false
                },
                FieldMilk = new FieldMilk
                {
                    License = new FieldLicense
                    {
                        Charge = 1.0m,
                        Tag = new FieldTag { Text = "Ta1" },
                        Title = "Ti1",
                        Tog = new FieldTog { Text = "To1" }
                    },
                    Manufacturer = new FieldManufacturer
                    {
                        Name = "M1",
                        Rating = 7,
                        Tag = new FieldTag { Text = "Ta2" },
                        Tog = new FieldTog { Text = "To2" }
                    },
                    Rating = 8,
                    Species = "S1",
                    Validation = false
                }
            });

        entry.State = EntityState.Unchanged;

        var cultureEntry = entry.ComplexProperty(e => e.Culture);
        var cultureManufacturerEntry = cultureEntry.ComplexProperty(e => e.Manufacturer);
        var cultureLicenseEntry = cultureEntry.ComplexProperty(e => e.License);
        var cultureManTogEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var cultureManTagEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var cultureLicTogEntry = cultureLicenseEntry.ComplexProperty(e => e.Tog);
        var cultureLicTagEntry = cultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal("Culture", cultureEntry.Metadata.Name);
        Assert.Equal("Rating", cultureEntry.Property(e => e.Rating).Metadata.Name);
        Assert.Equal("Manufacturer", cultureManufacturerEntry.Metadata.Name);
        Assert.Equal("Name", cultureManufacturerEntry.Property(e => e.Name).Metadata.Name);
        Assert.Equal("License", cultureLicenseEntry.Metadata.Name);
        Assert.Equal("Charge", cultureLicenseEntry.Property(e => e.Charge).Metadata.Name);
        Assert.Equal("Tog", cultureManTogEntry.Metadata.Name);
        Assert.Equal("Text", cultureManTogEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tag", cultureManTagEntry.Metadata.Name);
        Assert.Equal("Text", cultureManTagEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tog", cultureLicTogEntry.Metadata.Name);
        Assert.Equal("Text", cultureLicTogEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tag", cultureLicTagEntry.Metadata.Name);
        Assert.Equal("Text", cultureLicTagEntry.Property(e => e.Text).Metadata.Name);

        var milkEntry = entry.ComplexProperty(e => e.Milk);
        var milkManufacturerEntry = milkEntry.ComplexProperty(e => e.Manufacturer);
        var milkLicenseEntry = milkEntry.ComplexProperty(e => e.License);
        var milkManTogEntry = milkManufacturerEntry.ComplexProperty(e => e.Tog);
        var milkManTagEntry = milkManufacturerEntry.ComplexProperty(e => e.Tag);
        var milkLicTogEntry = milkLicenseEntry.ComplexProperty(e => e.Tog);
        var milkLicTagEntry = milkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal("Milk", milkEntry.Metadata.Name);
        Assert.Equal("Rating", milkEntry.Property(e => e.Rating).Metadata.Name);
        Assert.Equal("Manufacturer", milkManufacturerEntry.Metadata.Name);
        Assert.Equal("Name", milkManufacturerEntry.Property(e => e.Name).Metadata.Name);
        Assert.Equal("License", milkLicenseEntry.Metadata.Name);
        Assert.Equal("Charge", milkLicenseEntry.Property(e => e.Charge).Metadata.Name);
        Assert.Equal("Tog", milkManTogEntry.Metadata.Name);
        Assert.Equal("Text", milkManTogEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tag", milkManTagEntry.Metadata.Name);
        Assert.Equal("Text", milkManTagEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tog", milkLicTogEntry.Metadata.Name);
        Assert.Equal("Text", milkLicTogEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tag", milkLicTagEntry.Metadata.Name);
        Assert.Equal("Text", milkLicTagEntry.Property(e => e.Text).Metadata.Name);

        var fieldCultureEntry = entry.ComplexProperty(e => e.FieldCulture);
        var fieldCultureManufacturerEntry = fieldCultureEntry.ComplexProperty(e => e.Manufacturer);
        var fieldCultureLicenseEntry = fieldCultureEntry.ComplexProperty(e => e.License);
        var fieldCultureManTogEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldCultureManTagEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldCultureLicTogEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldCultureLicTagEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal("FieldCulture", fieldCultureEntry.Metadata.Name);
        Assert.Equal("Rating", fieldCultureEntry.Property(e => e.Rating).Metadata.Name);
        Assert.Equal("Manufacturer", fieldCultureManufacturerEntry.Metadata.Name);
        Assert.Equal("Name", fieldCultureManufacturerEntry.Property(e => e.Name).Metadata.Name);
        Assert.Equal("License", fieldCultureLicenseEntry.Metadata.Name);
        Assert.Equal("Charge", fieldCultureLicenseEntry.Property(e => e.Charge).Metadata.Name);
        Assert.Equal("Tog", fieldCultureManTogEntry.Metadata.Name);
        Assert.Equal("Text", fieldCultureManTogEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tag", fieldCultureManTagEntry.Metadata.Name);
        Assert.Equal("Text", fieldCultureManTagEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tog", fieldCultureLicTogEntry.Metadata.Name);
        Assert.Equal("Text", fieldCultureLicTogEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tag", fieldCultureLicTagEntry.Metadata.Name);
        Assert.Equal("Text", fieldCultureLicTagEntry.Property(e => e.Text).Metadata.Name);

        var fieldMilkEntry = entry.ComplexProperty(e => e.FieldMilk);
        var fieldMilkManufacturerEntry = fieldMilkEntry.ComplexProperty(e => e.Manufacturer);
        var fieldMilkLicenseEntry = fieldMilkEntry.ComplexProperty(e => e.License);
        var fieldMilkManTogEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldMilkManTagEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldMilkLicTogEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldMilkLicTagEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal("FieldMilk", fieldMilkEntry.Metadata.Name);
        Assert.Equal("Rating", fieldMilkEntry.Property(e => e.Rating).Metadata.Name);
        Assert.Equal("Manufacturer", fieldMilkManufacturerEntry.Metadata.Name);
        Assert.Equal("Name", fieldMilkManufacturerEntry.Property(e => e.Name).Metadata.Name);
        Assert.Equal("License", fieldMilkLicenseEntry.Metadata.Name);
        Assert.Equal("Charge", fieldMilkLicenseEntry.Property(e => e.Charge).Metadata.Name);
        Assert.Equal("Tog", fieldMilkManTogEntry.Metadata.Name);
        Assert.Equal("Text", fieldMilkManTogEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tag", fieldMilkManTagEntry.Metadata.Name);
        Assert.Equal("Text", fieldMilkManTagEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tog", fieldMilkLicTogEntry.Metadata.Name);
        Assert.Equal("Text", fieldMilkLicTogEntry.Property(e => e.Text).Metadata.Name);
        Assert.Equal("Tag", fieldMilkLicTagEntry.Metadata.Name);
        Assert.Equal("Text", fieldMilkLicTagEntry.Property(e => e.Text).Metadata.Name);
    }

    [ConditionalFact]
    public void Can_get_current_value_for_complex_property()
    {
        using var context = new YogurtContext();
        var yogurt = new Yogurt
        {
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldCulture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldMilk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

        var entry = context.Entry(yogurt);
        entry.State = EntityState.Unchanged;

        var cultureEntry = entry.ComplexProperty(e => e.Culture);
        var cultureManufacturerEntry = cultureEntry.ComplexProperty(e => e.Manufacturer);
        var cultureLicenseEntry = cultureEntry.ComplexProperty(e => e.License);
        var cultureManTogEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var cultureManTagEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var cultureLicTogEntry = cultureLicenseEntry.ComplexProperty(e => e.Tog);
        var cultureLicTagEntry = cultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(yogurt.Culture, cultureEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.Rating, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Species, cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.Culture.Subspecies, cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.Culture.Validation, cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer, cultureManufacturerEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Name, cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Rating, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tog, cultureManTogEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tog.Text, cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tag, cultureManTagEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tag.Text, cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License, cultureLicenseEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.License.Title, cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Charge, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tog, cultureLicTogEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tog.Text, cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tag, cultureLicTagEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tag.Text, cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var milkEntry = entry.ComplexProperty(e => e.Milk);
        var milkManufacturerEntry = milkEntry.ComplexProperty(e => e.Manufacturer);
        var milkLicenseEntry = milkEntry.ComplexProperty(e => e.License);
        var milkManTogEntry = milkManufacturerEntry.ComplexProperty(e => e.Tog);
        var milkManTagEntry = milkManufacturerEntry.ComplexProperty(e => e.Tag);
        var milkLicTogEntry = milkLicenseEntry.ComplexProperty(e => e.Tog);
        var milkLicTagEntry = milkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(yogurt.Milk, milkEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.Rating, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Species, milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.Milk.Subspecies, milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.Milk.Validation, milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer, milkManufacturerEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Name, milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Rating, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tog, milkManTogEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tog.Text, milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tag, milkManTagEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tag.Text, milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License, milkLicenseEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.License.Title, milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Charge, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tog, milkLicTogEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tog.Text, milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tag, milkLicTagEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tag.Text, milkLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldCultureEntry = entry.ComplexProperty(e => e.FieldCulture);
        var fieldCultureManufacturerEntry = fieldCultureEntry.ComplexProperty(e => e.Manufacturer);
        var fieldCultureLicenseEntry = fieldCultureEntry.ComplexProperty(e => e.License);
        var fieldCultureManTogEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldCultureManTagEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldCultureLicTogEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldCultureLicTagEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(yogurt.FieldCulture, fieldCultureEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Rating, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Species, fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Subspecies, fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Validation, fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer, fieldCultureManufacturerEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Name, fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Rating, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tog, fieldCultureManTogEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tog.Text, fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tag, fieldCultureManTagEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tag.Text, fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License, fieldCultureLicenseEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Title, fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Charge, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tog, fieldCultureLicTogEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tog.Text, fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tag, fieldCultureLicTagEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tag.Text, fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldMilkEntry = entry.ComplexProperty(e => e.FieldMilk);
        var fieldMilkManufacturerEntry = fieldMilkEntry.ComplexProperty(e => e.Manufacturer);
        var fieldMilkLicenseEntry = fieldMilkEntry.ComplexProperty(e => e.License);
        var fieldMilkManTogEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldMilkManTagEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldMilkLicTogEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldMilkLicTagEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(yogurt.FieldMilk, fieldMilkEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Rating, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Species, fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Subspecies, fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Validation, fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer, fieldMilkManufacturerEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Name, fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Rating, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tog, fieldMilkManTogEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tog.Text, fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tag, fieldMilkManTagEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tag.Text, fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License, fieldMilkLicenseEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Title, fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Charge, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tog, fieldMilkLicTogEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tog.Text, fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tag, fieldMilkLicTagEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tag.Text, fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);
    }

    [ConditionalFact]
    public void Can_set_current_value_for_complex_property()
    {
        using var context = new YogurtContext();
        var yogurt = new Yogurt
        {
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldCulture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldMilk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

        var entry = context.Entry(yogurt);
        entry.State = EntityState.Unchanged;

        var cultureEntry = entry.ComplexProperty(e => e.Culture);
        var cultureManufacturerEntry = cultureEntry.ComplexProperty(e => e.Manufacturer);
        var cultureLicenseEntry = cultureEntry.ComplexProperty(e => e.License);
        var cultureManTogEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var cultureManTagEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var cultureLicTogEntry = cultureLicenseEntry.ComplexProperty(e => e.Tog);
        var cultureLicTagEntry = cultureLicenseEntry.ComplexProperty(e => e.Tag);

        cultureEntry.Property(e => e.Rating).CurrentValue = 11;
        cultureEntry.Property(e => e.Species).CurrentValue = "XY";
        cultureEntry.Property(e => e.Subspecies).CurrentValue = "Z";
        cultureEntry.Property(e => e.Validation).CurrentValue = true;
        cultureManufacturerEntry.Property(e => e.Name).CurrentValue = "Nom";
        cultureManufacturerEntry.Property(e => e.Rating).CurrentValue = 9;
        cultureManTogEntry.Property(e => e.Text).CurrentValue = "Tog1";
        cultureManTagEntry.Property(e => e.Text).CurrentValue = "Tag1";
        cultureLicenseEntry.Property(e => e.Title).CurrentValue = "Title";
        cultureLicenseEntry.Property(e => e.Charge).CurrentValue = 11.0m;
        cultureLicTogEntry.Property(e => e.Text).CurrentValue = "Tog2";
        cultureLicTagEntry.Property(e => e.Text).CurrentValue = "Tag2";

        Assert.Equal(yogurt.Culture, cultureEntry.CurrentValue);
        Assert.Equal(11, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("XY", cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("Z", cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.True(cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer, cultureManufacturerEntry.CurrentValue);
        Assert.Equal("Nom", cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(9, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tog, cultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1", cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tag, cultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1", cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License, cultureLicenseEntry.CurrentValue);
        Assert.Equal("Title", cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(11.0m, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tog, cultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2", cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tag, cultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2", cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var milkEntry = entry.ComplexProperty(e => e.Milk);
        var milkManufacturerEntry = milkEntry.ComplexProperty(e => e.Manufacturer);
        var milkLicenseEntry = milkEntry.ComplexProperty(e => e.License);
        var milkManTogEntry = milkManufacturerEntry.ComplexProperty(e => e.Tog);
        var milkManTagEntry = milkManufacturerEntry.ComplexProperty(e => e.Tag);
        var milkLicTogEntry = milkLicenseEntry.ComplexProperty(e => e.Tog);
        var milkLicTagEntry = milkLicenseEntry.ComplexProperty(e => e.Tag);

        milkEntry.Property(e => e.Rating).CurrentValue = 11;
        milkEntry.Property(e => e.Species).CurrentValue = "XY";
        milkEntry.Property(e => e.Subspecies).CurrentValue = "Z";
        milkEntry.Property(e => e.Validation).CurrentValue = true;
        milkManufacturerEntry.Property(e => e.Name).CurrentValue = "Nom";
        milkManufacturerEntry.Property(e => e.Rating).CurrentValue = 9;
        milkManTogEntry.Property(e => e.Text).CurrentValue = "Tog1";
        milkManTagEntry.Property(e => e.Text).CurrentValue = "Tag1";
        milkLicenseEntry.Property(e => e.Title).CurrentValue = "Title";
        milkLicenseEntry.Property(e => e.Charge).CurrentValue = 11.0m;
        milkLicTogEntry.Property(e => e.Text).CurrentValue = "Tog2";
        milkLicTagEntry.Property(e => e.Text).CurrentValue = "Tag2";

        Assert.Equal(yogurt.Milk, milkEntry.CurrentValue);
        Assert.Equal(11, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("XY", milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("Z", milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.True(milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer, milkManufacturerEntry.CurrentValue);
        Assert.Equal("Nom", milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(9, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tog, milkManTogEntry.CurrentValue);
        Assert.Equal("Tog1", milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tag, milkManTagEntry.CurrentValue);
        Assert.Equal("Tag1", milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License, milkLicenseEntry.CurrentValue);
        Assert.Equal("Title", milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(11.0m, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tog, milkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2", milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tag, milkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2", milkLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldCultureEntry = entry.ComplexProperty(e => e.FieldCulture);
        var fieldCultureManufacturerEntry = fieldCultureEntry.ComplexProperty(e => e.Manufacturer);
        var fieldCultureLicenseEntry = fieldCultureEntry.ComplexProperty(e => e.License);
        var fieldCultureManTogEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldCultureManTagEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldCultureLicTogEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldCultureLicTagEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tag);

        fieldCultureEntry.Property(e => e.Rating).CurrentValue = 11;
        fieldCultureEntry.Property(e => e.Species).CurrentValue = "XY";
        fieldCultureEntry.Property(e => e.Subspecies).CurrentValue = "Z";
        fieldCultureEntry.Property(e => e.Validation).CurrentValue = true;
        fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue = "Nom";
        fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue = 9;
        fieldCultureManTogEntry.Property(e => e.Text).CurrentValue = "Tog1";
        fieldCultureManTagEntry.Property(e => e.Text).CurrentValue = "Tag1";
        fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue = "Title";
        fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue = 11.0m;
        fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue = "Tog2";
        fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue = "Tag2";

        Assert.Equal(yogurt.FieldCulture, fieldCultureEntry.CurrentValue);
        Assert.Equal(11, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("XY", fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("Z", fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.True(fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer, fieldCultureManufacturerEntry.CurrentValue);
        Assert.Equal("Nom", fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(9, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tog, fieldCultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1", fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tag, fieldCultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1", fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License, fieldCultureLicenseEntry.CurrentValue);
        Assert.Equal("Title", fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(11.0m, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tog, fieldCultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2", fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tag, fieldCultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2", fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldMilkEntry = entry.ComplexProperty(e => e.FieldMilk);
        var fieldMilkManufacturerEntry = fieldMilkEntry.ComplexProperty(e => e.Manufacturer);
        var fieldMilkLicenseEntry = fieldMilkEntry.ComplexProperty(e => e.License);
        var fieldMilkManTogEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldMilkManTagEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldMilkLicTogEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldMilkLicTagEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tag);

        fieldMilkEntry.Property(e => e.Rating).CurrentValue = 11;
        fieldMilkEntry.Property(e => e.Species).CurrentValue = "XY";
        fieldMilkEntry.Property(e => e.Subspecies).CurrentValue = "Z";
        fieldMilkEntry.Property(e => e.Validation).CurrentValue = true;
        fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue = "Nom";
        fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue = 9;
        fieldMilkManTogEntry.Property(e => e.Text).CurrentValue = "Tog1";
        fieldMilkManTagEntry.Property(e => e.Text).CurrentValue = "Tag1";
        fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue = "Title";
        fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue = 11.0m;
        fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue = "Tog2";
        fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue = "Tag2";

        Assert.Equal(yogurt.FieldMilk, fieldMilkEntry.CurrentValue);
        Assert.Equal(11, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("XY", fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("Z", fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.True(fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer, fieldMilkManufacturerEntry.CurrentValue);
        Assert.Equal("Nom", fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(9, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tog, fieldMilkManTogEntry.CurrentValue);
        Assert.Equal("Tog1", fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tag, fieldMilkManTagEntry.CurrentValue);
        Assert.Equal("Tag1", fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License, fieldMilkLicenseEntry.CurrentValue);
        Assert.Equal("Title", fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(11.0m, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tog, fieldMilkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2", fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tag, fieldMilkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2", fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);

        context.ChangeTracker.DetectChanges();

        Assert.Equal(yogurt.Culture, cultureEntry.CurrentValue);
        Assert.Equal(11, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("XY", cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("Z", cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.True(cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer, cultureManufacturerEntry.CurrentValue);
        Assert.Equal("Nom", cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(9, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tog, cultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1", cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tag, cultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1", cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License, cultureLicenseEntry.CurrentValue);
        Assert.Equal("Title", cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(11.0m, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tog, cultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2", cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tag, cultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2", cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(yogurt.Milk, milkEntry.CurrentValue);
        Assert.Equal(11, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("XY", milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("Z", milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.True(milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer, milkManufacturerEntry.CurrentValue);
        Assert.Equal("Nom", milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(9, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tog, milkManTogEntry.CurrentValue);
        Assert.Equal("Tog1", milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tag, milkManTagEntry.CurrentValue);
        Assert.Equal("Tag1", milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License, milkLicenseEntry.CurrentValue);
        Assert.Equal("Title", milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(11.0m, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tog, milkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2", milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tag, milkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2", milkLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(yogurt.FieldCulture, fieldCultureEntry.CurrentValue);
        Assert.Equal(11, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("XY", fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("Z", fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.True(fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer, fieldCultureManufacturerEntry.CurrentValue);
        Assert.Equal("Nom", fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(9, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tog, fieldCultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1", fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tag, fieldCultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1", fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License, fieldCultureLicenseEntry.CurrentValue);
        Assert.Equal("Title", fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(11.0m, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tog, fieldCultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2", fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tag, fieldCultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2", fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(yogurt.FieldMilk, fieldMilkEntry.CurrentValue);
        Assert.Equal(11, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("XY", fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("Z", fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.True(fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer, fieldMilkManufacturerEntry.CurrentValue);
        Assert.Equal("Nom", fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(9, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tog, fieldMilkManTogEntry.CurrentValue);
        Assert.Equal("Tog1", fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tag, fieldMilkManTagEntry.CurrentValue);
        Assert.Equal("Tag1", fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License, fieldMilkLicenseEntry.CurrentValue);
        Assert.Equal("Title", fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(11.0m, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tog, fieldMilkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2", fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tag, fieldMilkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2", fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);
    }

    [ConditionalFact]
    public void Can_set_current_value_for_complex_property_using_complex_type()
    {
        using var context = new YogurtContext();
        var yogurt = new Yogurt
        {
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldCulture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldMilk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

        var entry = context.Entry(yogurt);
        entry.State = EntityState.Unchanged;

        var cultureEntry = entry.ComplexProperty(e => e.Culture);
        var cultureManufacturerEntry = cultureEntry.ComplexProperty(e => e.Manufacturer);
        var cultureLicenseEntry = cultureEntry.ComplexProperty(e => e.License);
        var cultureManTogEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var cultureManTagEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var cultureLicTogEntry = cultureLicenseEntry.ComplexProperty(e => e.Tog);
        var cultureLicTagEntry = cultureLicenseEntry.ComplexProperty(e => e.Tag);

        cultureManTagEntry.CurrentValue = new Tag { Text = "Tag1a" };
        cultureManTogEntry.CurrentValue = new Tog { Text = "Tog1a" };
        cultureLicTagEntry.CurrentValue = new Tag { Text = "Tag2a" };
        cultureLicTogEntry.CurrentValue = new Tog { Text = "Tog2a" };

        Assert.Equal(yogurt.Culture, cultureEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.Rating, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Species, cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.Culture.Subspecies, cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.Culture.Validation, cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer, cultureManufacturerEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Name, cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Rating, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tog, cultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1a", cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tag, cultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1a", cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License, cultureLicenseEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.License.Title, cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Charge, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tog, cultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2a", cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tag, cultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2a", cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        cultureManufacturerEntry.CurrentValue = new Manufacturer
        {
            Name = "NameB",
            Rating = -7,
            Tag = new Tag { Text = "Tag1b" },
            Tog = new Tog { Text = "Tog1b" }
        };

        cultureLicenseEntry.CurrentValue = new License
        {
            Charge = -1.0m,
            Title = "TitleB",
            Tag = new Tag { Text = "Tag2b" },
            Tog = new Tog { Text = "Tog2b" }
        };

        Assert.Equal(yogurt.Culture, cultureEntry.CurrentValue);
        Assert.Equal(yogurt.Culture.Rating, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Species, cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.Culture.Subspecies, cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.Culture.Validation, cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer, cultureManufacturerEntry.CurrentValue);
        Assert.Equal("NameB", cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-7, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tog, cultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1b", cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tag, cultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1b", cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License, cultureLicenseEntry.CurrentValue);
        Assert.Equal("TitleB", cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-1.0m, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tog, cultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2b", cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tag, cultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2b", cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        cultureEntry.CurrentValue = new Culture
        {
            License = new License
            {
                Charge = -2.0m,
                Title = "TitleC",
                Tag = new Tag { Text = "Tag2c" },
                Tog = new Tog { Text = "Tog2c" }
            },
            Manufacturer = new Manufacturer
            {
                Name = "NameC",
                Rating = -8,
                Tag = new Tag { Text = "Tag1c" },
                Tog = new Tog { Text = "Tog1c" }
            },
            Rating = -77,
            Species = "SpC",
            Subspecies = "SpS",
            Validation = null
        };

        Assert.Equal(yogurt.Culture, cultureEntry.CurrentValue);
        Assert.Equal(-77, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("SpC", cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("SpS", cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer, cultureManufacturerEntry.CurrentValue);
        Assert.Equal("NameC", cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-8, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tog, cultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1c", cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tag, cultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1c", cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License, cultureLicenseEntry.CurrentValue);
        Assert.Equal("TitleC", cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-2.0m, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tog, cultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2c", cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tag, cultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2c", cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var milkEntry = entry.ComplexProperty(e => e.Milk);
        var milkManufacturerEntry = milkEntry.ComplexProperty(e => e.Manufacturer);
        var milkLicenseEntry = milkEntry.ComplexProperty(e => e.License);
        var milkManTogEntry = milkManufacturerEntry.ComplexProperty(e => e.Tog);
        var milkManTagEntry = milkManufacturerEntry.ComplexProperty(e => e.Tag);
        var milkLicTogEntry = milkLicenseEntry.ComplexProperty(e => e.Tog);
        var milkLicTagEntry = milkLicenseEntry.ComplexProperty(e => e.Tag);

        milkManTagEntry.CurrentValue = new Tag { Text = "Tag1a" };
        milkManTogEntry.CurrentValue = new Tog { Text = "Tog1a" };
        milkLicTagEntry.CurrentValue = new Tag { Text = "Tag2a" };
        milkLicTogEntry.CurrentValue = new Tog { Text = "Tog2a" };

        Assert.Equal(yogurt.Milk, milkEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.Rating, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Species, milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.Milk.Subspecies, milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.Milk.Validation, milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer, milkManufacturerEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Name, milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Rating, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tog, milkManTogEntry.CurrentValue);
        Assert.Equal("Tog1a", milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tag, milkManTagEntry.CurrentValue);
        Assert.Equal("Tag1a", milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License, milkLicenseEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.License.Title, milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Charge, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tog, milkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2a", milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tag, milkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2a", milkLicTagEntry.Property(e => e.Text).CurrentValue);

        milkManufacturerEntry.CurrentValue = new Manufacturer
        {
            Name = "NameB",
            Rating = -7,
            Tag = new Tag { Text = "Tag1b" },
            Tog = new Tog { Text = "Tog1b" }
        };

        milkLicenseEntry.CurrentValue = new License
        {
            Charge = -1.0m,
            Title = "TitleB",
            Tag = new Tag { Text = "Tag2b" },
            Tog = new Tog { Text = "Tog2b" }
        };

        Assert.Equal(yogurt.Milk, milkEntry.CurrentValue);
        Assert.Equal(yogurt.Milk.Rating, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Species, milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.Milk.Subspecies, milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.Milk.Validation, milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer, milkManufacturerEntry.CurrentValue);
        Assert.Equal("NameB", milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-7, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tog, milkManTogEntry.CurrentValue);
        Assert.Equal("Tog1b", milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tag, milkManTagEntry.CurrentValue);
        Assert.Equal("Tag1b", milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License, milkLicenseEntry.CurrentValue);
        Assert.Equal("TitleB", milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-1.0m, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tog, milkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2b", milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tag, milkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2b", milkLicTagEntry.Property(e => e.Text).CurrentValue);

        milkEntry.CurrentValue = new Milk
        {
            License = new License
            {
                Charge = -2.0m,
                Title = "TitleC",
                Tag = new Tag { Text = "Tag2c" },
                Tog = new Tog { Text = "Tog2c" }
            },
            Manufacturer = new Manufacturer
            {
                Name = "NameC",
                Rating = -8,
                Tag = new Tag { Text = "Tag1c" },
                Tog = new Tog { Text = "Tog1c" }
            },
            Rating = -77,
            Species = "SpC",
            Subspecies = "SpS",
            Validation = null
        };

        Assert.Equal(yogurt.Milk, milkEntry.CurrentValue);
        Assert.Equal(-77, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("SpC", milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("SpS", milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer, milkManufacturerEntry.CurrentValue);
        Assert.Equal("NameC", milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-8, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tog, milkManTogEntry.CurrentValue);
        Assert.Equal("Tog1c", milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tag, milkManTagEntry.CurrentValue);
        Assert.Equal("Tag1c", milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License, milkLicenseEntry.CurrentValue);
        Assert.Equal("TitleC", milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-2.0m, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tog, milkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2c", milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tag, milkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2c", milkLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldCultureEntry = entry.ComplexProperty(e => e.FieldCulture);
        var fieldCultureManufacturerEntry = fieldCultureEntry.ComplexProperty(e => e.Manufacturer);
        var fieldCultureLicenseEntry = fieldCultureEntry.ComplexProperty(e => e.License);
        var fieldCultureManTogEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldCultureManTagEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldCultureLicTogEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldCultureLicTagEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tag);

        fieldCultureManTagEntry.CurrentValue = new FieldTag { Text = "Tag1a" };
        fieldCultureManTogEntry.CurrentValue = new FieldTog { Text = "Tog1a" };
        fieldCultureLicTagEntry.CurrentValue = new FieldTag { Text = "Tag2a" };
        fieldCultureLicTogEntry.CurrentValue = new FieldTog { Text = "Tog2a" };

        Assert.Equal(yogurt.FieldCulture, fieldCultureEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Rating, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Species, fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Subspecies, fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Validation, fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer, fieldCultureManufacturerEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Name, fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Rating, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tog, fieldCultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1a", fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tag, fieldCultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1a", fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License, fieldCultureLicenseEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Title, fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Charge, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tog, fieldCultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2a", fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tag, fieldCultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2a", fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        fieldCultureManufacturerEntry.CurrentValue = new FieldManufacturer
        {
            Name = "NameB",
            Rating = -7,
            Tag = new FieldTag { Text = "Tag1b" },
            Tog = new FieldTog { Text = "Tog1b" }
        };

        fieldCultureLicenseEntry.CurrentValue = new FieldLicense
        {
            Charge = -1.0m,
            Title = "TitleB",
            Tag = new FieldTag { Text = "Tag2b" },
            Tog = new FieldTog { Text = "Tog2b" }
        };

        Assert.Equal(yogurt.FieldCulture, fieldCultureEntry.CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Rating, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Species, fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Subspecies, fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Validation, fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer, fieldCultureManufacturerEntry.CurrentValue);
        Assert.Equal("NameB", fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-7, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tog, fieldCultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1b", fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tag, fieldCultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1b", fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License, fieldCultureLicenseEntry.CurrentValue);
        Assert.Equal("TitleB", fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-1.0m, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tog, fieldCultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2b", fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tag, fieldCultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2b", fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        fieldCultureEntry.CurrentValue = new FieldCulture
        {
            License = new FieldLicense
            {
                Charge = -2.0m,
                Title = "TitleC",
                Tag = new FieldTag { Text = "Tag2c" },
                Tog = new FieldTog { Text = "Tog2c" }
            },
            Manufacturer = new FieldManufacturer
            {
                Name = "NameC",
                Rating = -8,
                Tag = new FieldTag { Text = "Tag1c" },
                Tog = new FieldTog { Text = "Tog1c" }
            },
            Rating = -77,
            Species = "SpC",
            Subspecies = "SpS",
            Validation = null
        };

        Assert.Equal(yogurt.FieldCulture, fieldCultureEntry.CurrentValue);
        Assert.Equal(-77, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("SpC", fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("SpS", fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer, fieldCultureManufacturerEntry.CurrentValue);
        Assert.Equal("NameC", fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-8, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tog, fieldCultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1c", fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tag, fieldCultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1c", fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License, fieldCultureLicenseEntry.CurrentValue);
        Assert.Equal("TitleC", fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-2.0m, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tog, fieldCultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2c", fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tag, fieldCultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2c", fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldMilkEntry = entry.ComplexProperty(e => e.FieldMilk);
        var fieldMilkManufacturerEntry = fieldMilkEntry.ComplexProperty(e => e.Manufacturer);
        var fieldMilkLicenseEntry = fieldMilkEntry.ComplexProperty(e => e.License);
        var fieldMilkManTogEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldMilkManTagEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldMilkLicTogEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldMilkLicTagEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tag);

        fieldMilkManTagEntry.CurrentValue = new FieldTag { Text = "Tag1a" };
        fieldMilkManTogEntry.CurrentValue = new FieldTog { Text = "Tog1a" };
        fieldMilkLicTagEntry.CurrentValue = new FieldTag { Text = "Tag2a" };
        fieldMilkLicTogEntry.CurrentValue = new FieldTog { Text = "Tog2a" };

        Assert.Equal(yogurt.FieldMilk, fieldMilkEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Rating, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Species, fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Subspecies, fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Validation, fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer, fieldMilkManufacturerEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Name, fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Rating, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tog, fieldMilkManTogEntry.CurrentValue);
        Assert.Equal("Tog1a", fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tag, fieldMilkManTagEntry.CurrentValue);
        Assert.Equal("Tag1a", fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License, fieldMilkLicenseEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Title, fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Charge, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tog, fieldMilkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2a", fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tag, fieldMilkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2a", fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);

        fieldMilkManufacturerEntry.CurrentValue = new FieldManufacturer
        {
            Name = "NameB",
            Rating = -7,
            Tag = new FieldTag { Text = "Tag1b" },
            Tog = new FieldTog { Text = "Tog1b" }
        };

        fieldMilkLicenseEntry.CurrentValue = new FieldLicense
        {
            Charge = -1.0m,
            Title = "TitleB",
            Tag = new FieldTag { Text = "Tag2b" },
            Tog = new FieldTog { Text = "Tog2b" }
        };

        Assert.Equal(yogurt.FieldMilk, fieldMilkEntry.CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Rating, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Species, fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Subspecies, fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Validation, fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer, fieldMilkManufacturerEntry.CurrentValue);
        Assert.Equal("NameB", fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-7, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tog, fieldMilkManTogEntry.CurrentValue);
        Assert.Equal("Tog1b", fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tag, fieldMilkManTagEntry.CurrentValue);
        Assert.Equal("Tag1b", fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License, fieldMilkLicenseEntry.CurrentValue);
        Assert.Equal("TitleB", fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-1.0m, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tog, fieldMilkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2b", fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tag, fieldMilkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2b", fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);

        fieldMilkEntry.CurrentValue = new FieldMilk
        {
            License = new FieldLicense
            {
                Charge = -2.0m,
                Title = "TitleC",
                Tag = new FieldTag { Text = "Tag2c" },
                Tog = new FieldTog { Text = "Tog2c" }
            },
            Manufacturer = new FieldManufacturer
            {
                Name = "NameC",
                Rating = -8,
                Tag = new FieldTag { Text = "Tag1c" },
                Tog = new FieldTog { Text = "Tog1c" }
            },
            Rating = -77,
            Species = "SpC",
            Subspecies = "SpS",
            Validation = null
        };

        Assert.Equal(yogurt.FieldMilk, fieldMilkEntry.CurrentValue);
        Assert.Equal(-77, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("SpC", fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("SpS", fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer, fieldMilkManufacturerEntry.CurrentValue);
        Assert.Equal("NameC", fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-8, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tog, fieldMilkManTogEntry.CurrentValue);
        Assert.Equal("Tog1c", fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tag, fieldMilkManTagEntry.CurrentValue);
        Assert.Equal("Tag1c", fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License, fieldMilkLicenseEntry.CurrentValue);
        Assert.Equal("TitleC", fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-2.0m, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tog, fieldMilkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2c", fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tag, fieldMilkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2c", fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);

        context.ChangeTracker.DetectChanges();

        Assert.Equal(yogurt.Culture, cultureEntry.CurrentValue);
        Assert.Equal(-77, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("SpC", cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("SpS", cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer, cultureManufacturerEntry.CurrentValue);
        Assert.Equal("NameC", cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-8, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tog, cultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1c", cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.Manufacturer.Tag, cultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1c", cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License, cultureLicenseEntry.CurrentValue);
        Assert.Equal("TitleC", cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-2.0m, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tog, cultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2c", cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Culture.License.Tag, cultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2c", cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(yogurt.Milk, milkEntry.CurrentValue);
        Assert.Equal(-77, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("SpC", milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("SpS", milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer, milkManufacturerEntry.CurrentValue);
        Assert.Equal("NameC", milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-8, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tog, milkManTogEntry.CurrentValue);
        Assert.Equal("Tog1c", milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.Manufacturer.Tag, milkManTagEntry.CurrentValue);
        Assert.Equal("Tag1c", milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License, milkLicenseEntry.CurrentValue);
        Assert.Equal("TitleC", milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-2.0m, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tog, milkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2c", milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.Milk.License.Tag, milkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2c", milkLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(yogurt.FieldCulture, fieldCultureEntry.CurrentValue);
        Assert.Equal(-77, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("SpC", fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("SpS", fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer, fieldCultureManufacturerEntry.CurrentValue);
        Assert.Equal("NameC", fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-8, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tog, fieldCultureManTogEntry.CurrentValue);
        Assert.Equal("Tog1c", fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.Manufacturer.Tag, fieldCultureManTagEntry.CurrentValue);
        Assert.Equal("Tag1c", fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License, fieldCultureLicenseEntry.CurrentValue);
        Assert.Equal("TitleC", fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-2.0m, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tog, fieldCultureLicTogEntry.CurrentValue);
        Assert.Equal("Tog2c", fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldCulture.License.Tag, fieldCultureLicTagEntry.CurrentValue);
        Assert.Equal("Tag2c", fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(yogurt.FieldMilk, fieldMilkEntry.CurrentValue);
        Assert.Equal(-77, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("SpC", fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Equal("SpS", fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer, fieldMilkManufacturerEntry.CurrentValue);
        Assert.Equal("NameC", fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(-8, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tog, fieldMilkManTogEntry.CurrentValue);
        Assert.Equal("Tog1c", fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.Manufacturer.Tag, fieldMilkManTagEntry.CurrentValue);
        Assert.Equal("Tag1c", fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License, fieldMilkLicenseEntry.CurrentValue);
        Assert.Equal("TitleC", fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(-2.0m, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tog, fieldMilkLicTogEntry.CurrentValue);
        Assert.Equal("Tog2c", fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal(yogurt.FieldMilk.License.Tag, fieldMilkLicTagEntry.CurrentValue);
        Assert.Equal("Tag2c", fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);
    }

    [ConditionalFact]
    public void Can_set_current_value_for_property_of_complex_type_to_null()
    {
        using var context = new YogurtContext();
        var yogurt = new Yogurt
        {
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldCulture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldMilk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

        var entry = context.Entry(yogurt);
        entry.State = EntityState.Unchanged;

        var cultureEntry = entry.ComplexProperty(e => e.Culture);
        var cultureManufacturerEntry = cultureEntry.ComplexProperty(e => e.Manufacturer);
        var cultureLicenseEntry = cultureEntry.ComplexProperty(e => e.License);
        var cultureManTogEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var cultureManTagEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var cultureLicTogEntry = cultureLicenseEntry.ComplexProperty(e => e.Tog);
        var cultureLicTagEntry = cultureLicenseEntry.ComplexProperty(e => e.Tag);

        cultureEntry.Property(e => e.Subspecies).CurrentValue = null;
        cultureEntry.Property(e => e.Validation).CurrentValue = null;
        cultureManufacturerEntry.Property(e => e.Name).CurrentValue = null;
        cultureManTogEntry.Property(e => e.Text).CurrentValue = null;
        cultureManTagEntry.Property(e => e.Text).CurrentValue = null;
        cultureLicTogEntry.Property(e => e.Text).CurrentValue = null;
        cultureLicTagEntry.Property(e => e.Text).CurrentValue = null;

        Assert.Null(cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Null(cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Null(cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var milkEntry = entry.ComplexProperty(e => e.Milk);
        var milkManufacturerEntry = milkEntry.ComplexProperty(e => e.Manufacturer);
        var milkLicenseEntry = milkEntry.ComplexProperty(e => e.License);
        var milkManTogEntry = milkManufacturerEntry.ComplexProperty(e => e.Tog);
        var milkManTagEntry = milkManufacturerEntry.ComplexProperty(e => e.Tag);
        var milkLicTogEntry = milkLicenseEntry.ComplexProperty(e => e.Tog);
        var milkLicTagEntry = milkLicenseEntry.ComplexProperty(e => e.Tag);

        milkEntry.Property(e => e.Subspecies).CurrentValue = null;
        milkEntry.Property(e => e.Validation).CurrentValue = null;
        milkManufacturerEntry.Property(e => e.Name).CurrentValue = null;
        milkManTogEntry.Property(e => e.Text).CurrentValue = null;
        milkManTagEntry.Property(e => e.Text).CurrentValue = null;
        milkLicTogEntry.Property(e => e.Text).CurrentValue = null;
        milkLicTagEntry.Property(e => e.Text).CurrentValue = null;

        Assert.Null(milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Null(milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Null(milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(milkLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldCultureEntry = entry.ComplexProperty(e => e.FieldCulture);
        var fieldCultureManufacturerEntry = fieldCultureEntry.ComplexProperty(e => e.Manufacturer);
        var fieldCultureLicenseEntry = fieldCultureEntry.ComplexProperty(e => e.License);
        var fieldCultureManTogEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldCultureManTagEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldCultureLicTogEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldCultureLicTagEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tag);

        fieldCultureEntry.Property(e => e.Subspecies).CurrentValue = null;
        fieldCultureEntry.Property(e => e.Validation).CurrentValue = null;
        fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue = null;
        fieldCultureManTogEntry.Property(e => e.Text).CurrentValue = null;
        fieldCultureManTagEntry.Property(e => e.Text).CurrentValue = null;
        fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue = null;
        fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue = null;

        Assert.Null(fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Null(fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Null(fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldMilkEntry = entry.ComplexProperty(e => e.FieldMilk);
        var fieldMilkManufacturerEntry = fieldMilkEntry.ComplexProperty(e => e.Manufacturer);
        var fieldMilkLicenseEntry = fieldMilkEntry.ComplexProperty(e => e.License);
        var fieldMilkManTogEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldMilkManTagEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldMilkLicTogEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldMilkLicTagEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tag);

        fieldMilkEntry.Property(e => e.Subspecies).CurrentValue = null;
        fieldMilkEntry.Property(e => e.Validation).CurrentValue = null;
        fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue = null;
        fieldMilkManTogEntry.Property(e => e.Text).CurrentValue = null;
        fieldMilkManTagEntry.Property(e => e.Text).CurrentValue = null;
        fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue = null;
        fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue = null;

        Assert.Null(fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Null(fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Null(fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);

        context.ChangeTracker.DetectChanges();

        Assert.Null(cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Null(cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Null(cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Null(milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Null(milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Null(milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(milkLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Null(fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Null(fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Null(fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Null(fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.Null(fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Null(fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Null(fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Null(fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);
    }

    [ConditionalFact]
    public void Can_set_and_get_original_value_for_complex_property()
    {
        using var context = new YogurtContext();
        var yogurt = new Yogurt
        {
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldCulture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldMilk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

        var entry = context.Entry(yogurt);
        entry.State = EntityState.Unchanged;

        var cultureEntry = entry.ComplexProperty(e => e.Culture);
        var cultureManufacturerEntry = cultureEntry.ComplexProperty(e => e.Manufacturer);
        var cultureLicenseEntry = cultureEntry.ComplexProperty(e => e.License);
        var cultureManTogEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var cultureManTagEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var cultureLicTogEntry = cultureLicenseEntry.ComplexProperty(e => e.Tog);
        var cultureLicTagEntry = cultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(8, cultureEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("S1", cultureEntry.Property(e => e.Species).OriginalValue);
        Assert.Null(cultureEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.False(cultureEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("M1", cultureManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(7, cultureManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("To2", cultureManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ta2", cultureManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ti1", cultureLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(1.0m, cultureLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("To1", cultureLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ta1", cultureLicTagEntry.Property(e => e.Text).OriginalValue);

        cultureEntry.Property(e => e.Rating).OriginalValue = 11;
        cultureEntry.Property(e => e.Species).OriginalValue = "XY";
        cultureEntry.Property(e => e.Subspecies).OriginalValue = "Z";
        cultureEntry.Property(e => e.Validation).OriginalValue = true;
        cultureManufacturerEntry.Property(e => e.Name).OriginalValue = "Nom";
        cultureManufacturerEntry.Property(e => e.Rating).OriginalValue = 9;
        cultureManTogEntry.Property(e => e.Text).OriginalValue = "Tog1";
        cultureManTagEntry.Property(e => e.Text).OriginalValue = "Tag1";
        cultureLicenseEntry.Property(e => e.Title).OriginalValue = "Title";
        cultureLicenseEntry.Property(e => e.Charge).OriginalValue = 11.0m;
        cultureLicTogEntry.Property(e => e.Text).OriginalValue = "Tog2";
        cultureLicTagEntry.Property(e => e.Text).OriginalValue = "Tag2";

        Assert.Equal(11, cultureEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("XY", cultureEntry.Property(e => e.Species).OriginalValue);
        Assert.Equal("Z", cultureEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.True(cultureEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("Nom", cultureManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(9, cultureManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("Tog1", cultureManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag1", cultureManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Title", cultureLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(11.0m, cultureLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("Tog2", cultureLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag2", cultureLicTagEntry.Property(e => e.Text).OriginalValue);

        Assert.Equal(8, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("S1", cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Null(cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.False(cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal("M1", cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(7, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("To2", cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta2", cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ti1", cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(1.0m, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal("To1", cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta1", cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var milkEntry = entry.ComplexProperty(e => e.Milk);
        var milkManufacturerEntry = milkEntry.ComplexProperty(e => e.Manufacturer);
        var milkLicenseEntry = milkEntry.ComplexProperty(e => e.License);
        var milkManTogEntry = milkManufacturerEntry.ComplexProperty(e => e.Tog);
        var milkManTagEntry = milkManufacturerEntry.ComplexProperty(e => e.Tag);
        var milkLicTogEntry = milkLicenseEntry.ComplexProperty(e => e.Tog);
        var milkLicTagEntry = milkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(8, milkEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("S1", milkEntry.Property(e => e.Species).OriginalValue);
        Assert.Null(milkEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.False(milkEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("M1", milkManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(7, milkManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("To2", milkManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ta2", milkManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ti1", milkLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(1.0m, milkLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("To1", milkLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ta1", milkLicTagEntry.Property(e => e.Text).OriginalValue);

        milkEntry.Property(e => e.Rating).OriginalValue = 11;
        milkEntry.Property(e => e.Species).OriginalValue = "XY";
        milkEntry.Property(e => e.Subspecies).OriginalValue = "Z";
        milkEntry.Property(e => e.Validation).OriginalValue = true;
        milkManufacturerEntry.Property(e => e.Name).OriginalValue = "Nom";
        milkManufacturerEntry.Property(e => e.Rating).OriginalValue = 9;
        milkManTogEntry.Property(e => e.Text).OriginalValue = "Tog1";
        milkManTagEntry.Property(e => e.Text).OriginalValue = "Tag1";
        milkLicenseEntry.Property(e => e.Title).OriginalValue = "Title";
        milkLicenseEntry.Property(e => e.Charge).OriginalValue = 11.0m;
        milkLicTogEntry.Property(e => e.Text).OriginalValue = "Tog2";
        milkLicTagEntry.Property(e => e.Text).OriginalValue = "Tag2";

        Assert.Equal(11, milkEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("XY", milkEntry.Property(e => e.Species).OriginalValue);
        Assert.Equal("Z", milkEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.True(milkEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("Nom", milkManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(9, milkManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("Tog1", milkManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag1", milkManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Title", milkLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(11.0m, milkLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("Tog2", milkLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag2", milkLicTagEntry.Property(e => e.Text).OriginalValue);

        Assert.Equal(8, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("S1", milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Null(milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.False(milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal("M1", milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(7, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("To2", milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta2", milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ti1", milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(1.0m, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal("To1", milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta1", milkLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldCultureEntry = entry.ComplexProperty(e => e.FieldCulture);
        var fieldCultureManufacturerEntry = fieldCultureEntry.ComplexProperty(e => e.Manufacturer);
        var fieldCultureLicenseEntry = fieldCultureEntry.ComplexProperty(e => e.License);
        var fieldCultureManTogEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldCultureManTagEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldCultureLicTogEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldCultureLicTagEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(8, fieldCultureEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("S1", fieldCultureEntry.Property(e => e.Species).OriginalValue);
        Assert.Null(fieldCultureEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("M1", fieldCultureManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(7, fieldCultureManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("To2", fieldCultureManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ta2", fieldCultureManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ti1", fieldCultureLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(1.0m, fieldCultureLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("To1", fieldCultureLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ta1", fieldCultureLicTagEntry.Property(e => e.Text).OriginalValue);

        fieldCultureEntry.Property(e => e.Rating).OriginalValue = 11;
        fieldCultureEntry.Property(e => e.Species).OriginalValue = "XY";
        fieldCultureEntry.Property(e => e.Subspecies).OriginalValue = "Z";
        fieldCultureEntry.Property(e => e.Validation).OriginalValue = true;
        fieldCultureManufacturerEntry.Property(e => e.Name).OriginalValue = "Nom";
        fieldCultureManufacturerEntry.Property(e => e.Rating).OriginalValue = 9;
        fieldCultureManTogEntry.Property(e => e.Text).OriginalValue = "Tog1";
        fieldCultureManTagEntry.Property(e => e.Text).OriginalValue = "Tag1";
        fieldCultureLicenseEntry.Property(e => e.Title).OriginalValue = "Title";
        fieldCultureLicenseEntry.Property(e => e.Charge).OriginalValue = 11.0m;
        fieldCultureLicTogEntry.Property(e => e.Text).OriginalValue = "Tog2";
        fieldCultureLicTagEntry.Property(e => e.Text).OriginalValue = "Tag2";

        Assert.Equal(11, fieldCultureEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("XY", fieldCultureEntry.Property(e => e.Species).OriginalValue);
        Assert.Equal("Z", fieldCultureEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.True(fieldCultureEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("Nom", fieldCultureManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(9, fieldCultureManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("Tog1", fieldCultureManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag1", fieldCultureManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Title", fieldCultureLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(11.0m, fieldCultureLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("Tog2", fieldCultureLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag2", fieldCultureLicTagEntry.Property(e => e.Text).OriginalValue);

        Assert.Equal(8, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("S1", fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Null(fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal("M1", fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(7, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("To2", fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta2", fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ti1", fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(1.0m, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal("To1", fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta1", fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        var fieldMilkEntry = entry.ComplexProperty(e => e.FieldMilk);
        var fieldMilkManufacturerEntry = fieldMilkEntry.ComplexProperty(e => e.Manufacturer);
        var fieldMilkLicenseEntry = fieldMilkEntry.ComplexProperty(e => e.License);
        var fieldMilkManTogEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldMilkManTagEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldMilkLicTogEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldMilkLicTagEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(8, fieldMilkEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("S1", fieldMilkEntry.Property(e => e.Species).OriginalValue);
        Assert.Null(fieldMilkEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("M1", fieldMilkManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(7, fieldMilkManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("To2", fieldMilkManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ta2", fieldMilkManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ti1", fieldMilkLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(1.0m, fieldMilkLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("To1", fieldMilkLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Ta1", fieldMilkLicTagEntry.Property(e => e.Text).OriginalValue);

        fieldMilkEntry.Property(e => e.Rating).OriginalValue = 11;
        fieldMilkEntry.Property(e => e.Species).OriginalValue = "XY";
        fieldMilkEntry.Property(e => e.Subspecies).OriginalValue = "Z";
        fieldMilkEntry.Property(e => e.Validation).OriginalValue = true;
        fieldMilkManufacturerEntry.Property(e => e.Name).OriginalValue = "Nom";
        fieldMilkManufacturerEntry.Property(e => e.Rating).OriginalValue = 9;
        fieldMilkManTogEntry.Property(e => e.Text).OriginalValue = "Tog1";
        fieldMilkManTagEntry.Property(e => e.Text).OriginalValue = "Tag1";
        fieldMilkLicenseEntry.Property(e => e.Title).OriginalValue = "Title";
        fieldMilkLicenseEntry.Property(e => e.Charge).OriginalValue = 11.0m;
        fieldMilkLicTogEntry.Property(e => e.Text).OriginalValue = "Tog2";
        fieldMilkLicTagEntry.Property(e => e.Text).OriginalValue = "Tag2";

        Assert.Equal(11, fieldMilkEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("XY", fieldMilkEntry.Property(e => e.Species).OriginalValue);
        Assert.Equal("Z", fieldMilkEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.True(fieldMilkEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("Nom", fieldMilkManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(9, fieldMilkManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("Tog1", fieldMilkManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag1", fieldMilkManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Title", fieldMilkLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(11.0m, fieldMilkLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("Tog2", fieldMilkLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag2", fieldMilkLicTagEntry.Property(e => e.Text).OriginalValue);

        Assert.Equal(8, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("S1", fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Null(fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal("M1", fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(7, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("To2", fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta2", fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ti1", fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(1.0m, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal("To1", fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta1", fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);

        context.ChangeTracker.DetectChanges();

        Assert.Equal(11, cultureEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("XY", cultureEntry.Property(e => e.Species).OriginalValue);
        Assert.Equal("Z", cultureEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.True(cultureEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("Nom", cultureManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(9, cultureManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("Tog1", cultureManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag1", cultureManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Title", cultureLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(11.0m, cultureLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("Tog2", cultureLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag2", cultureLicTagEntry.Property(e => e.Text).OriginalValue);

        Assert.Equal(8, cultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("S1", cultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Null(cultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.False(cultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal("M1", cultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(7, cultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("To2", cultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta2", cultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ti1", cultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(1.0m, cultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal("To1", cultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta1", cultureLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(11, milkEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("XY", milkEntry.Property(e => e.Species).OriginalValue);
        Assert.Equal("Z", milkEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.True(milkEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("Nom", milkManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(9, milkManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("Tog1", milkManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag1", milkManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Title", milkLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(11.0m, milkLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("Tog2", milkLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag2", milkLicTagEntry.Property(e => e.Text).OriginalValue);

        Assert.Equal(8, milkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("S1", milkEntry.Property(e => e.Species).CurrentValue);
        Assert.Null(milkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.False(milkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal("M1", milkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(7, milkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("To2", milkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta2", milkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ti1", milkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(1.0m, milkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal("To1", milkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta1", milkLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(11, fieldCultureEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("XY", fieldCultureEntry.Property(e => e.Species).OriginalValue);
        Assert.Equal("Z", fieldCultureEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.True(fieldCultureEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("Nom", fieldCultureManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(9, fieldCultureManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("Tog1", fieldCultureManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag1", fieldCultureManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Title", fieldCultureLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(11.0m, fieldCultureLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("Tog2", fieldCultureLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag2", fieldCultureLicTagEntry.Property(e => e.Text).OriginalValue);

        Assert.Equal(8, fieldCultureEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("S1", fieldCultureEntry.Property(e => e.Species).CurrentValue);
        Assert.Null(fieldCultureEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal("M1", fieldCultureManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(7, fieldCultureManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("To2", fieldCultureManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta2", fieldCultureManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ti1", fieldCultureLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(1.0m, fieldCultureLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal("To1", fieldCultureLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta1", fieldCultureLicTagEntry.Property(e => e.Text).CurrentValue);

        Assert.Equal(11, fieldMilkEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("XY", fieldMilkEntry.Property(e => e.Species).OriginalValue);
        Assert.Equal("Z", fieldMilkEntry.Property(e => e.Subspecies).OriginalValue);
        Assert.True(fieldMilkEntry.Property(e => e.Validation).OriginalValue);
        Assert.Equal("Nom", fieldMilkManufacturerEntry.Property(e => e.Name).OriginalValue);
        Assert.Equal(9, fieldMilkManufacturerEntry.Property(e => e.Rating).OriginalValue);
        Assert.Equal("Tog1", fieldMilkManTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag1", fieldMilkManTagEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Title", fieldMilkLicenseEntry.Property(e => e.Title).OriginalValue);
        Assert.Equal(11.0m, fieldMilkLicenseEntry.Property(e => e.Charge).OriginalValue);
        Assert.Equal("Tog2", fieldMilkLicTogEntry.Property(e => e.Text).OriginalValue);
        Assert.Equal("Tag2", fieldMilkLicTagEntry.Property(e => e.Text).OriginalValue);

        Assert.Equal(8, fieldMilkEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("S1", fieldMilkEntry.Property(e => e.Species).CurrentValue);
        Assert.Null(fieldMilkEntry.Property(e => e.Subspecies).CurrentValue);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).CurrentValue);
        Assert.Equal("M1", fieldMilkManufacturerEntry.Property(e => e.Name).CurrentValue);
        Assert.Equal(7, fieldMilkManufacturerEntry.Property(e => e.Rating).CurrentValue);
        Assert.Equal("To2", fieldMilkManTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta2", fieldMilkManTagEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ti1", fieldMilkLicenseEntry.Property(e => e.Title).CurrentValue);
        Assert.Equal(1.0m, fieldMilkLicenseEntry.Property(e => e.Charge).CurrentValue);
        Assert.Equal("To1", fieldMilkLicTogEntry.Property(e => e.Text).CurrentValue);
        Assert.Equal("Ta1", fieldMilkLicTagEntry.Property(e => e.Text).CurrentValue);
    }

    [ConditionalFact]
    public void Can_set_and_clear_modified_on_Modified_entity_for_complex_property()
    {
        using var context = new YogurtContext();
        var yogurt = new Yogurt
        {
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldCulture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldMilk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

        var entry = context.Entry(yogurt);
        entry.State = EntityState.Unchanged;

        var cultureEntry = entry.ComplexProperty(e => e.Culture);
        var cultureManufacturerEntry = cultureEntry.ComplexProperty(e => e.Manufacturer);
        var cultureLicenseEntry = cultureEntry.ComplexProperty(e => e.License);
        var cultureManTogEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var cultureManTagEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var cultureLicTogEntry = cultureLicenseEntry.ComplexProperty(e => e.Tog);
        var cultureLicTagEntry = cultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureLicTagEntry.Property(e => e.Text).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(cultureLicTagEntry.IsModified);
        Assert.True(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureLicTagEntry.Property(e => e.Text).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureManufacturerEntry.Property(e => e.Name).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.True(cultureManufacturerEntry.IsModified);
        Assert.True(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureManufacturerEntry.Property(e => e.Name).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureEntry.Property(e => e.Subspecies).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.True(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureEntry.Property(e => e.Subspecies).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        var milkEntry = entry.ComplexProperty(e => e.Milk);
        var milkManufacturerEntry = milkEntry.ComplexProperty(e => e.Manufacturer);
        var milkLicenseEntry = milkEntry.ComplexProperty(e => e.License);
        var milkManTogEntry = milkManufacturerEntry.ComplexProperty(e => e.Tog);
        var milkManTagEntry = milkManufacturerEntry.ComplexProperty(e => e.Tag);
        var milkLicTogEntry = milkLicenseEntry.ComplexProperty(e => e.Tog);
        var milkLicTagEntry = milkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkLicTagEntry.Property(e => e.Text).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(milkLicTagEntry.IsModified);
        Assert.True(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkLicTagEntry.Property(e => e.Text).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkManufacturerEntry.Property(e => e.Name).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.True(milkManufacturerEntry.IsModified);
        Assert.True(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkManufacturerEntry.Property(e => e.Name).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkEntry.Property(e => e.Subspecies).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.True(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkEntry.Property(e => e.Subspecies).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        var fieldCultureEntry = entry.ComplexProperty(e => e.FieldCulture);
        var fieldCultureManufacturerEntry = fieldCultureEntry.ComplexProperty(e => e.Manufacturer);
        var fieldCultureLicenseEntry = fieldCultureEntry.ComplexProperty(e => e.License);
        var fieldCultureManTogEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldCultureManTagEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldCultureLicTogEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldCultureLicTagEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureLicTagEntry.Property(e => e.Text).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldCultureLicTagEntry.IsModified);
        Assert.True(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureLicTagEntry.Property(e => e.Text).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureManufacturerEntry.Property(e => e.Name).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.True(fieldCultureManufacturerEntry.IsModified);
        Assert.True(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureManufacturerEntry.Property(e => e.Name).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureEntry.Property(e => e.Subspecies).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.True(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureEntry.Property(e => e.Subspecies).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        var fieldMilkEntry = entry.ComplexProperty(e => e.FieldMilk);
        var fieldMilkManufacturerEntry = fieldMilkEntry.ComplexProperty(e => e.Manufacturer);
        var fieldMilkLicenseEntry = fieldMilkEntry.ComplexProperty(e => e.License);
        var fieldMilkManTogEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldMilkManTagEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldMilkLicTogEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldMilkLicTagEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkLicTagEntry.Property(e => e.Text).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldMilkLicTagEntry.IsModified);
        Assert.True(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkLicTagEntry.Property(e => e.Text).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkManufacturerEntry.Property(e => e.Name).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.True(fieldMilkManufacturerEntry.IsModified);
        Assert.True(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkManufacturerEntry.Property(e => e.Name).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkEntry.Property(e => e.Subspecies).IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.True(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkEntry.Property(e => e.Subspecies).IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);
    }

    [ConditionalFact]
    public void Can_set_and_clear_modified_on_Modified_using_complex_type()
    {
        using var context = new YogurtContext();
        var yogurt = new Yogurt
        {
            Culture = new Culture
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            Milk = new Milk
            {
                License = new License
                {
                    Charge = 1.0m,
                    Tag = new Tag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new Tog { Text = "To1" }
                },
                Manufacturer = new Manufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new Tag { Text = "Ta2" },
                    Tog = new Tog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldCulture = new FieldCulture
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            },
            FieldMilk = new FieldMilk
            {
                License = new FieldLicense
                {
                    Charge = 1.0m,
                    Tag = new FieldTag { Text = "Ta1" },
                    Title = "Ti1",
                    Tog = new FieldTog { Text = "To1" }
                },
                Manufacturer = new FieldManufacturer
                {
                    Name = "M1",
                    Rating = 7,
                    Tag = new FieldTag { Text = "Ta2" },
                    Tog = new FieldTog { Text = "To2" }
                },
                Rating = 8,
                Species = "S1",
                Validation = false
            }
        };

        var entry = context.Entry(yogurt);
        entry.State = EntityState.Unchanged;

        var cultureEntry = entry.ComplexProperty(e => e.Culture);
        var cultureManufacturerEntry = cultureEntry.ComplexProperty(e => e.Manufacturer);
        var cultureLicenseEntry = cultureEntry.ComplexProperty(e => e.License);
        var cultureManTogEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var cultureManTagEntry = cultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var cultureLicTogEntry = cultureLicenseEntry.ComplexProperty(e => e.Tog);
        var cultureLicTagEntry = cultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureLicTagEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(cultureLicTagEntry.IsModified);
        Assert.True(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureLicTagEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureManufacturerEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.True(cultureManufacturerEntry.IsModified);
        Assert.True(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.True(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.True(cultureManTogEntry.IsModified);
        Assert.True(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.True(cultureManTagEntry.IsModified);
        Assert.True(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureManufacturerEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(cultureEntry.IsModified);
        Assert.True(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.True(cultureEntry.Property(e => e.Species).IsModified);
        Assert.True(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.True(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.True(cultureManufacturerEntry.IsModified);
        Assert.True(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.True(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.True(cultureManTogEntry.IsModified);
        Assert.True(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.True(cultureManTagEntry.IsModified);
        Assert.True(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(cultureLicenseEntry.IsModified);
        Assert.True(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.True(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.True(cultureLicTogEntry.IsModified);
        Assert.True(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(cultureLicTagEntry.IsModified);
        Assert.True(cultureLicTagEntry.Property(e => e.Text).IsModified);

        cultureEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(cultureEntry.IsModified);
        Assert.False(cultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureEntry.Property(e => e.Species).IsModified);
        Assert.False(cultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(cultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(cultureManufacturerEntry.IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(cultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(cultureManTogEntry.IsModified);
        Assert.False(cultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureManTagEntry.IsModified);
        Assert.False(cultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicenseEntry.IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(cultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(cultureLicTogEntry.IsModified);
        Assert.False(cultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(cultureLicTagEntry.IsModified);
        Assert.False(cultureLicTagEntry.Property(e => e.Text).IsModified);

        var milkEntry = entry.ComplexProperty(e => e.Milk);
        var milkManufacturerEntry = milkEntry.ComplexProperty(e => e.Manufacturer);
        var milkLicenseEntry = milkEntry.ComplexProperty(e => e.License);
        var milkManTogEntry = milkManufacturerEntry.ComplexProperty(e => e.Tog);
        var milkManTagEntry = milkManufacturerEntry.ComplexProperty(e => e.Tag);
        var milkLicTogEntry = milkLicenseEntry.ComplexProperty(e => e.Tog);
        var milkLicTagEntry = milkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkLicTagEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(milkLicTagEntry.IsModified);
        Assert.True(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkLicTagEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkManufacturerEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.True(milkManufacturerEntry.IsModified);
        Assert.True(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.True(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.True(milkManTogEntry.IsModified);
        Assert.True(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.True(milkManTagEntry.IsModified);
        Assert.True(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkManufacturerEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(milkEntry.IsModified);
        Assert.True(milkEntry.Property(e => e.Rating).IsModified);
        Assert.True(milkEntry.Property(e => e.Species).IsModified);
        Assert.True(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.True(milkEntry.Property(e => e.Validation).IsModified);
        Assert.True(milkManufacturerEntry.IsModified);
        Assert.True(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.True(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.True(milkManTogEntry.IsModified);
        Assert.True(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.True(milkManTagEntry.IsModified);
        Assert.True(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(milkLicenseEntry.IsModified);
        Assert.True(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.True(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.True(milkLicTogEntry.IsModified);
        Assert.True(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(milkLicTagEntry.IsModified);
        Assert.True(milkLicTagEntry.Property(e => e.Text).IsModified);

        milkEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(milkEntry.IsModified);
        Assert.False(milkEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkEntry.Property(e => e.Species).IsModified);
        Assert.False(milkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(milkEntry.Property(e => e.Validation).IsModified);
        Assert.False(milkManufacturerEntry.IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(milkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(milkManTogEntry.IsModified);
        Assert.False(milkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkManTagEntry.IsModified);
        Assert.False(milkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicenseEntry.IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(milkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(milkLicTogEntry.IsModified);
        Assert.False(milkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(milkLicTagEntry.IsModified);
        Assert.False(milkLicTagEntry.Property(e => e.Text).IsModified);

        var fieldCultureEntry = entry.ComplexProperty(e => e.FieldCulture);
        var fieldCultureManufacturerEntry = fieldCultureEntry.ComplexProperty(e => e.Manufacturer);
        var fieldCultureLicenseEntry = fieldCultureEntry.ComplexProperty(e => e.License);
        var fieldCultureManTogEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldCultureManTagEntry = fieldCultureManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldCultureLicTogEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldCultureLicTagEntry = fieldCultureLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureLicTagEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldCultureLicTagEntry.IsModified);
        Assert.True(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureLicTagEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureManufacturerEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.True(fieldCultureManufacturerEntry.IsModified);
        Assert.True(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.True(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.True(fieldCultureManTogEntry.IsModified);
        Assert.True(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldCultureManTagEntry.IsModified);
        Assert.True(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureManufacturerEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldCultureEntry.IsModified);
        Assert.True(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.True(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.True(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.True(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.True(fieldCultureManufacturerEntry.IsModified);
        Assert.True(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.True(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.True(fieldCultureManTogEntry.IsModified);
        Assert.True(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldCultureManTagEntry.IsModified);
        Assert.True(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldCultureLicenseEntry.IsModified);
        Assert.True(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.True(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.True(fieldCultureLicTogEntry.IsModified);
        Assert.True(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldCultureLicTagEntry.IsModified);
        Assert.True(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        fieldCultureEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldCultureEntry.IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldCultureEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldCultureManufacturerEntry.IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldCultureManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldCultureManTogEntry.IsModified);
        Assert.False(fieldCultureManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureManTagEntry.IsModified);
        Assert.False(fieldCultureManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicenseEntry.IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldCultureLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldCultureLicTogEntry.IsModified);
        Assert.False(fieldCultureLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldCultureLicTagEntry.IsModified);
        Assert.False(fieldCultureLicTagEntry.Property(e => e.Text).IsModified);

        var fieldMilkEntry = entry.ComplexProperty(e => e.FieldMilk);
        var fieldMilkManufacturerEntry = fieldMilkEntry.ComplexProperty(e => e.Manufacturer);
        var fieldMilkLicenseEntry = fieldMilkEntry.ComplexProperty(e => e.License);
        var fieldMilkManTogEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tog);
        var fieldMilkManTagEntry = fieldMilkManufacturerEntry.ComplexProperty(e => e.Tag);
        var fieldMilkLicTogEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tog);
        var fieldMilkLicTagEntry = fieldMilkLicenseEntry.ComplexProperty(e => e.Tag);

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkLicTagEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldMilkLicTagEntry.IsModified);
        Assert.True(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkLicTagEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkManufacturerEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.True(fieldMilkManufacturerEntry.IsModified);
        Assert.True(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.True(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.True(fieldMilkManTogEntry.IsModified);
        Assert.True(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldMilkManTagEntry.IsModified);
        Assert.True(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkManufacturerEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkEntry.IsModified = true;

        Assert.Equal(EntityState.Modified, entry.State);
        Assert.True(fieldMilkEntry.IsModified);
        Assert.True(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.True(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.True(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.True(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.True(fieldMilkManufacturerEntry.IsModified);
        Assert.True(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.True(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.True(fieldMilkManTogEntry.IsModified);
        Assert.True(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldMilkManTagEntry.IsModified);
        Assert.True(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldMilkLicenseEntry.IsModified);
        Assert.True(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.True(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.True(fieldMilkLicTogEntry.IsModified);
        Assert.True(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.True(fieldMilkLicTagEntry.IsModified);
        Assert.True(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);

        fieldMilkEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entry.State);
        Assert.False(fieldMilkEntry.IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Species).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Subspecies).IsModified);
        Assert.False(fieldMilkEntry.Property(e => e.Validation).IsModified);
        Assert.False(fieldMilkManufacturerEntry.IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Name).IsModified);
        Assert.False(fieldMilkManufacturerEntry.Property(e => e.Rating).IsModified);
        Assert.False(fieldMilkManTogEntry.IsModified);
        Assert.False(fieldMilkManTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkManTagEntry.IsModified);
        Assert.False(fieldMilkManTagEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicenseEntry.IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Title).IsModified);
        Assert.False(fieldMilkLicenseEntry.Property(e => e.Charge).IsModified);
        Assert.False(fieldMilkLicTogEntry.IsModified);
        Assert.False(fieldMilkLicTogEntry.Property(e => e.Text).IsModified);
        Assert.False(fieldMilkLicTagEntry.IsModified);
        Assert.False(fieldMilkLicTagEntry.Property(e => e.Text).IsModified);
    }

    private interface IWotty
    {
        int Id { get; set; }
        string? Primate { get; set; }
        string RequiredPrimate { get; set; }
        string? Marmate { get; set; }
    }

    private class ObjectWotty : IWotty
    {
        private object? _id;
        private object? _primate;
        private object? _requiredPrimate;
        private object? _marmate;

        public int Id
        {
            get => (int)_id!;
            set => _id = value;
        }

        public string? Primate
        {
            get => (string)_primate!;
            set => _primate = value;
        }

        public string RequiredPrimate
        {
            get => (string)_requiredPrimate!;
            set => _requiredPrimate = value;
        }

        public string? Marmate
        {
            get => (string)_marmate!;
            set => _marmate = value;
        }
    }

    private class Wotty : IWotty
    {
        public int Id { get; set; }
        public string? Primate { get; set; }
        public string RequiredPrimate { get; set; } = null!;
        public string? Marmate { get; set; }
    }

    private class FullyNotifyingWotty : HasChangedAndChanging
    {
        private int _id;
        private string? _primate;
        private string? _concurrentprimate;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    OnPropertyChanging();
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Primate
        {
            get => _primate;
            set
            {
                if (_primate != value)
                {
                    OnPropertyChanging();
                    _primate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? ConcurrentPrimate
        {
            get => _concurrentprimate;
            set
            {
                if (_concurrentprimate != value)
                {
                    OnPropertyChanging();
                    _concurrentprimate = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    private class NotifyingWotty : HasChanged
    {
        private int _id;
        private string? _primate;

        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Primate
        {
            get => _primate;
            set
            {
                if (_primate != value)
                {
                    _primate = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    private abstract class HasChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private abstract class HasChangedAndChanging : HasChanged, INotifyPropertyChanging
    {
        public event PropertyChangingEventHandler? PropertyChanging;

        protected void OnPropertyChanging([CallerMemberName] string propertyName = "")
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    public static IModel BuildModel(
        ChangeTrackingStrategy fullNotificationStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications,
        ModelBuilder builder = null!,
        bool finalize = true)
    {
        builder ??= InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.HasChangeTrackingStrategy(fullNotificationStrategy);

        builder.Entity<Wotty>(
            b =>
            {
                b.Property(e => e.RequiredPrimate).IsRequired();
                b.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            });

        builder.Entity<ObjectWotty>(
            b =>
            {
                b.Property(e => e.RequiredPrimate).IsRequired();
                b.HasChangeTrackingStrategy(ChangeTrackingStrategy.Snapshot);
            });

        builder.Entity<NotifyingWotty>(
            b => b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications));

        builder.Entity<FullyNotifyingWotty>(
            b =>
            {
                b.HasChangeTrackingStrategy(fullNotificationStrategy);
                b.Property(e => e.ConcurrentPrimate).IsConcurrencyToken();
            });

        return finalize ? builder.Model.FinalizeModel() : (IModel)builder.Model;
    }

    private class Yogurt
    {
        public Guid Id { get; set; }
        public Culture Culture { get; set; }
        public Milk Milk { get; set; } = null!;
        public FieldCulture FieldCulture;
        public FieldMilk FieldMilk = null!;
    }

    private class YogurtContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(GetType().FullName!);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Yogurt>(
                b =>
                {
                    b.ComplexProperty(
                        e => e.Culture, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });

                    b.ComplexProperty(
                        e => e.Milk, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });

                    b.ComplexProperty(
                        e => e.FieldCulture, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });

                    b.ComplexProperty(
                        e => e.FieldMilk, b =>
                        {
                            b.ComplexProperty(
                                e => e.License, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                            b.ComplexProperty(
                                e => e.Manufacturer, b =>
                                {
                                    b.ComplexProperty(e => e.Tag);
                                    b.ComplexProperty(e => e.Tog);
                                });
                        });
                });
    }

    private class PrimateContext(ChangeTrackingStrategy fullNotificationStrategy = ChangeTrackingStrategy.ChangingAndChangedNotifications) : DbContext
    {
        private readonly ChangeTrackingStrategy _fullNotificationStrategy = fullNotificationStrategy;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(GetType().FullName!);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => BuildModel(_fullNotificationStrategy, modelBuilder, finalize: false);
    }

    private struct Culture
    {
        public string Species { get; set; }
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public License License { get; set; }
    }

    private class Milk
    {
        public string Species { get; set; } = null!;
        public string? Subspecies { get; set; }
        public int Rating { get; set; }
        public bool? Validation { get; set; }
        public Manufacturer Manufacturer { get; set; } = null!;
        public License License { get; set; }
    }

    private class Manufacturer
    {
        public string? Name { get; set; }
        public int Rating { get; set; }
        public Tag Tag { get; set; } = null!;
        public Tog Tog { get; set; }
    }

    private struct License
    {
        public string Title { get; set; }
        public decimal Charge { get; set; }
        public Tag Tag { get; set; }
        public Tog Tog { get; set; }
    }

    private class Tag
    {
        public string? Text { get; set; }
    }

    private struct Tog
    {
        public string? Text { get; set; }
    }

    private struct FieldCulture
    {
        public string Species;
        public string? Subspecies;
        public int Rating;
        public bool? Validation;
        public FieldManufacturer Manufacturer;
        public FieldLicense License;
    }

    private class FieldMilk
    {
        public string Species = null!;
        public string? Subspecies;
        public int Rating;
        public bool? Validation;
        public FieldManufacturer Manufacturer = null!;
        public FieldLicense License;
    }

    private class FieldManufacturer
    {
        public string? Name;
        public int Rating;
        public FieldTag Tag = null!;
        public FieldTog Tog;
    }

    private struct FieldLicense
    {
        public string Title;
        public decimal Charge;
        public FieldTag Tag;
        public FieldTog Tog;
    }

    private class FieldTag
    {
        public string? Text;
    }

    private struct FieldTog
    {
        public string? Text;
    }
}
