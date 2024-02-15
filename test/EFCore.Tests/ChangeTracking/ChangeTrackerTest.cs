// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking;

public class ChangeTrackerTest
{
    [ConditionalTheory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    public void Can_Add_with_identifying_relationships_dependent_first(int principalKeyValue, int dependentKeyValue)
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Add(new DependentGG { Id = dependentKeyValue, PrincipalGG = new PrincipalGG { Id = principalKeyValue } })
            .Entity;
        Assert.Equal(EntityState.Added, context.Entry(added1).State);
        Assert.Equal(EntityState.Added, context.Entry(added1.PrincipalGG!).State);

        var added2 = context.Add(new DependentNG { Id = dependentKeyValue, PrincipalNG = new PrincipalNG { Id = principalKeyValue } })
            .Entity;
        Assert.Equal(EntityState.Added, context.Entry(added2).State);
        Assert.Equal(EntityState.Added, context.Entry(added2.PrincipalNG!).State);

        var added3 = context.Add(new DependentNN { Id = dependentKeyValue, PrincipalNN = new PrincipalNN { Id = principalKeyValue } })
            .Entity;
        Assert.Equal(EntityState.Added, context.Entry(added3).State);
        Assert.Equal(EntityState.Added, context.Entry(added3.PrincipalNN!).State);

        var added4 = context.Add(new DependentGN { Id = dependentKeyValue, PrincipalGN = new PrincipalGN { Id = principalKeyValue } })
            .Entity;
        Assert.Equal(EntityState.Added, context.Entry(added4).State);
        Assert.Equal(EntityState.Added, context.Entry(added4.PrincipalGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalTheory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    public void Can_Add_with_identifying_relationships_principal_first(int principalKeyValue, int dependentKeyValue)
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Add(new PrincipalGG { Id = principalKeyValue, DependentGG = new DependentGG { Id = dependentKeyValue } })
            .Entity;
        Assert.Equal(EntityState.Added, context.Entry(added1).State);
        Assert.Equal(EntityState.Added, context.Entry(added1.DependentGG!).State);

        var added2 = context.Add(new PrincipalNG { Id = principalKeyValue, DependentNG = new DependentNG { Id = dependentKeyValue } })
            .Entity;
        Assert.Equal(EntityState.Added, context.Entry(added2).State);
        Assert.Equal(EntityState.Added, context.Entry(added2.DependentNG!).State);

        var added3 = context.Add(new PrincipalNN { Id = principalKeyValue, DependentNN = new DependentNN { Id = dependentKeyValue } })
            .Entity;
        Assert.Equal(EntityState.Added, context.Entry(added3).State);
        Assert.Equal(EntityState.Added, context.Entry(added3.DependentNN!).State);

        var added4 = context.Add(new PrincipalGN { Id = principalKeyValue, DependentGN = new DependentGN { Id = dependentKeyValue } })
            .Entity;
        Assert.Equal(EntityState.Added, context.Entry(added4).State);
        Assert.Equal(EntityState.Added, context.Entry(added4.DependentGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Can_Attach_with_identifying_relationships_dependent_first()
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Attach(new DependentGG { PrincipalGG = new PrincipalGG() }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added1).State);
        Assert.Equal(EntityState.Added, context.Entry(added1.PrincipalGG!).State);

        var added2 = context.Attach(new DependentNG { PrincipalNG = new PrincipalNG() }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added2.PrincipalNG!).State);

        var added3 = context.Attach(new DependentNN { PrincipalNN = new PrincipalNN() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added3).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added3.PrincipalNN!).State);

        var added4 = context.Attach(new DependentGN { PrincipalGN = new PrincipalGN() }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added4).State);
        Assert.Equal(EntityState.Added, context.Entry(added4.PrincipalGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Can_Attach_with_identifying_relationships_dependent_first_with_principal_keys_set()
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Attach(new DependentGG { PrincipalGG = new PrincipalGG { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added1.PrincipalGG!).State);

        var added2 = context.Attach(new DependentNG { PrincipalNG = new PrincipalNG { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added2.PrincipalNG!).State);

        var added3 = context.Attach(new DependentNN { PrincipalNN = new PrincipalNN { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added3).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added3.PrincipalNN!).State);

        var added4 = context.Attach(new DependentGN { PrincipalGN = new PrincipalGN { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added4).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added4.PrincipalGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Can_Attach_with_identifying_relationships_dependent_first_with_dependent_keys_set()
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Attach(new DependentGG { Id = 1, PrincipalGG = new PrincipalGG() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added1).State);
        Assert.Equal(EntityState.Added, context.Entry(added1.PrincipalGG!).State);

        var added2 = context.Attach(new DependentNG { Id = 1, PrincipalNG = new PrincipalNG() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added2.PrincipalNG!).State);

        var added3 = context.Attach(new DependentNN { Id = 1, PrincipalNN = new PrincipalNN() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added3).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added3.PrincipalNN!).State);

        var added4 = context.Attach(new DependentGN { Id = 1, PrincipalGN = new PrincipalGN() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added4).State);
        Assert.Equal(EntityState.Added, context.Entry(added4.PrincipalGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Can_Attach_with_identifying_relationships_dependent_first_with_all_keys_set()
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Attach(new DependentGG { Id = 1, PrincipalGG = new PrincipalGG { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added1.PrincipalGG!).State);

        var added2 = context.Attach(new DependentNG { Id = 1, PrincipalNG = new PrincipalNG { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added2.PrincipalNG!).State);

        var added3 = context.Attach(new DependentNN { Id = 1, PrincipalNN = new PrincipalNN { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added3).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added3.PrincipalNN!).State);

        var added4 = context.Attach(new DependentGN { Id = 1, PrincipalGN = new PrincipalGN { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added4).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added4.PrincipalGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Can_Attach_with_identifying_relationships_principal_first()
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Attach(new PrincipalGG { DependentGG = new DependentGG() }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added1).State);
        Assert.Equal(EntityState.Added, context.Entry(added1.DependentGG!).State);

        var added2 = context.Attach(new PrincipalNG { DependentNG = new DependentNG() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added2).State);
        Assert.Equal(EntityState.Added, context.Entry(added2.DependentNG!).State);

        var added3 = context.Attach(new PrincipalNN { DependentNN = new DependentNN() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added3).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added3.DependentNN!).State);

        var added4 = context.Attach(new PrincipalGN { DependentGN = new DependentGN() }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added4).State);
        Assert.Equal(EntityState.Added, context.Entry(added4.DependentGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Can_Attach_with_identifying_relationships_principal_first_with_principal_keys_set()
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Attach(new PrincipalGG { Id = 1, DependentGG = new DependentGG() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added1).State);
        Assert.Equal(EntityState.Added, context.Entry(added1.DependentGG!).State);

        var added2 = context.Attach(new PrincipalNG { Id = 1, DependentNG = new DependentNG() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added2).State);
        Assert.Equal(EntityState.Added, context.Entry(added2.DependentNG!).State);

        var added3 = context.Attach(new PrincipalNN { Id = 1, DependentNN = new DependentNN() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added3).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added3.DependentNN!).State);

        var added4 = context.Attach(new PrincipalGN { Id = 1, DependentGN = new DependentGN() }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added4).State);
        Assert.Equal(EntityState.Added, context.Entry(added4.DependentGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Can_Attach_with_identifying_relationships_principal_first_with_dependent_keys_set()
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Attach(new PrincipalGG { DependentGG = new DependentGG { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added1.DependentGG!).State);

        var added2 = context.Attach(new PrincipalNG { DependentNG = new DependentNG { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added2.DependentNG!).State);

        var added3 = context.Attach(new PrincipalNN { DependentNN = new DependentNN { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added3).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added3.DependentNN!).State);

        var added4 = context.Attach(new PrincipalGN { DependentGN = new DependentGN { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Added, context.Entry(added4).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added4.DependentGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Can_Attach_with_identifying_relationships_principal_first_with_all_keys_set()
    {
        using var context = new EarlyLearningCenter();

        var added1 = context.Attach(new PrincipalGG { Id = 1, DependentGG = new DependentGG { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added1.DependentGG!).State);

        var added2 = context.Attach(new PrincipalNG { Id = 1, DependentNG = new DependentNG { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added2.DependentNG!).State);

        var added3 = context.Attach(new PrincipalNN { Id = 1, DependentNN = new DependentNN { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added3).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added3.DependentNN!).State);

        var added4 = context.Attach(new PrincipalGN { Id = 1, DependentGN = new DependentGN { Id = 1 } }).Entity;
        Assert.Equal(EntityState.Unchanged, context.Entry(added4).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(added4.DependentGN!).State);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());
    }

    [ConditionalFact]
    public void Change_tracker_can_be_cleared()
    {
        Seed();

        using var context = new LikeAZooContext();

        var cats = context.Cats.ToList();
        var hats = context.Set<Hat>().ToList();

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(cats[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(hats[0]).State);
        Assert.NotNull(cats[0].EntityType);

        context.ChangeTracker.Clear();

        Assert.Null(cats[0].EntityType);
        Assert.Empty(context.ChangeTracker.Entries());
        Assert.Equal(EntityState.Detached, context.Entry(cats[0]).State);
        Assert.Equal(EntityState.Detached, context.Entry(hats[0]).State);

        var catsAgain = context.Cats.ToList();
        var hatsAgain = context.Set<Hat>().ToList();

        Assert.NotNull(catsAgain[0].EntityType);
        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(catsAgain[0]).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(hatsAgain[0]).State);

        Assert.Null(cats[0].EntityType);
        Assert.Equal(EntityState.Detached, context.Entry(cats[0]).State);
        Assert.Equal(EntityState.Detached, context.Entry(hats[0]).State);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Keys_generated_on_behalf_of_a_principal_are_not_saved(bool async)
    {
        using var context = new WeakHerosContext();

        var entity = new Weak { Id = Guid.NewGuid() };

        if (async)
        {
            await context.AddAsync(entity);
        }
        else
        {
            context.Add(entity);
        }

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(
            CoreStrings.UnknownKeyValue(nameof(Weak), nameof(Weak.HeroId)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => _ = async ? await context.SaveChangesAsync() : context.SaveChanges()))
            .Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Attached_owned_entity_without_owner_not_saved(bool async)
    {
        using var context = new WeakHerosContext();

        if (async)
        {
            await context.AddAsync(new Skinner());
        }
        else
        {
            context.Add(new Skinner());
        }

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(
            CoreStrings.SaveOwnedWithoutOwner(nameof(Skinner)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => _ = async ? await context.SaveChangesAsync() : context.SaveChanges()))
            .Message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Attached_owned_collection_entity_without_owner_not_saved(bool async)
    {
        using var context = new WeakHerosContext();

        if (async)
        {
            await context.AddAsync(new TheStreets());
        }
        else
        {
            context.Add(new TheStreets());
        }

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(
            CoreStrings.SaveOwnedWithoutOwner(nameof(TheStreets)),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                async () => _ = async ? await context.SaveChangesAsync() : context.SaveChanges()))
            .Message);
    }

    public class Hero
    {
        public Guid Id { get; set; }
        public ICollection<Weak> Weaks { get; } = new List<Weak>();
    }

    public class Weak
    {
        public Guid Id { get; set; }
        public Guid HeroId { get; set; }

        public Hero? Hero { get; set; }
    }

    public class Mike
    {
        public Guid Id { get; set; }
        public ICollection<TheStreets> TheStreets { get; } = new List<TheStreets>();
        public Skinner? TheHero { get; set; }
    }

    public class Skinner;

    public class TheStreets;

    public class WeakHerosContext : DbContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Weak>(
                b =>
                {
                    b.HasKey(e => new { e.Id, e.HeroId });
                    b.HasOne(e => e.Hero).WithMany(e => e.Weaks).HasForeignKey(e => e.HeroId);
                });

            modelBuilder.Entity<Mike>(
                b =>
                {
                    b.OwnsOne(e => e.TheHero);
                    b.OwnsMany(e => e.TheStreets);
                });
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(nameof(WeakHerosContext));
    }

    [ConditionalFact]
    public void DetectChanges_is_logged()
    {
        Seed();

        using var context = new LikeAZooContext();
        _loggerFactory.Log.Clear();

        context.SaveChanges();

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.DetectChangesStarting.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            CoreResources.LogDetectChangesStarting(new TestLogger<TestLoggingDefinitions>())
                .GenerateMessage(nameof(LikeAZooContext)), message);

        (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.DetectChangesCompleted.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            CoreResources.LogDetectChangesCompleted(new TestLogger<TestLoggingDefinitions>())
                .GenerateMessage(nameof(LikeAZooContext)), message);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Detect_property_change_is_logged(bool sensitive, bool callDetectChangesTwice)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        var cat = context.Cats.Find(1)!;

        _loggerFactory.Log.Clear();

        cat.Name = "Smoke-a-doke";

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogPropertyChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Cat), nameof(Cat.Name), "Smokey", "Smoke-a-doke", "{Id: 1}")
                : CoreResources.LogPropertyChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Cat), nameof(Cat.Name)),
            message);

        _loggerFactory.Log.Clear();

        cat.Name = "Little Artichoke";

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        Assert.Empty(_loggerFactory.Log.Where(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Detect_nested_property_change_is_logged(bool sensitive, bool callDetectChangesTwice)
    {
        var wocket = new Wocket { Id = 1, Name = "Gollum", Pocket = new() { Contents = "Handsies" } };

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        context.Database.EnsureDeleted();
        context.Add(wocket);
        context.SaveChanges();

        _loggerFactory.Log.Clear();

        wocket.Pocket.Contents = "Fishies";

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogPropertyChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Pocket), nameof(Pocket.Contents), "Handsies", "Fishies", "{Id: 1}")
                : CoreResources.LogPropertyChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Pocket), nameof(Pocket.Contents)),
            message);

        _loggerFactory.Log.Clear();

        wocket.Pocket.Contents = "String...or nothing!";

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        Assert.Empty(_loggerFactory.Log.Where(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id));
    }

    [ConditionalTheory] // Issue #21896
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Property_changes_on_Deleted_entities_are_not_continually_detected(bool sensitive, bool callDetectChangesTwice)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        var cat = context.Cats.Find(1)!;

        _loggerFactory.Log.Clear();

        context.Entry(cat).State = EntityState.Deleted;

        cat.Name = "Smoke-a-doke";

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        Assert.Empty(_loggerFactory.Log.Where(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id));

        _loggerFactory.Log.Clear();

        cat.Name = "Little Artichoke";

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        Assert.Empty(_loggerFactory.Log.Where(e => e.Id.Id == CoreEventId.PropertyChangeDetected.Id));
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Detect_foreign_key_property_change_is_logged(bool sensitive, bool callDetectChangesTwice)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

        _loggerFactory.Log.Clear();

        var hat = cat.Hats.Single(h => h.Id == 77);
        hat.CatId = 2;

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ForeignKeyChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogForeignKeyChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Hat), nameof(Hat.CatId), 1, 2, "{Id: 77}")
                : CoreResources.LogForeignKeyChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Hat), nameof(Hat.CatId)),
            message);

        _loggerFactory.Log.Clear();

        hat.CatId = 1;

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ForeignKeyChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogForeignKeyChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Hat), nameof(Hat.CatId), 2, 1, "{Id: 77}")
                : CoreResources.LogForeignKeyChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Hat), nameof(Hat.CatId)),
            message);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Detect_collection_change_is_logged(bool sensitive, bool callDetectChangesTwice)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);
        var hat = cat.Hats.Single(h => h.Id == 77);

        _loggerFactory.Log.Clear();

        cat.Hats.Clear();

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.CollectionChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogCollectionChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Hats), "{Id: 1}")
                : CoreResources.LogCollectionChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Hats)),
            message);

        _loggerFactory.Log.Clear();

        cat.Hats.Add(hat);

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.CollectionChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogCollectionChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Hats), "{Id: 1}")
                : CoreResources.LogCollectionChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Hats)),
            message);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Detect_skip_collection_change_is_logged(bool sensitive, bool callDetectChangesTwice)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        var cat = context.Cats.Include(e => e.Mats).Single(e => e.Id == 1);
        var mat = cat.Mats.Single(h => h.Id == 77);

        _loggerFactory.Log.Clear();

        cat.Mats.Clear();

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SkipCollectionChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogSkipCollectionChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Mats), "{Id: 1}")
                : CoreResources.LogSkipCollectionChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(0, 1, nameof(Cat), nameof(Cat.Mats)),
            message);

        _loggerFactory.Log.Clear();

        cat.Mats.Add(mat);

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SkipCollectionChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogSkipCollectionChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Mats), "{Id: 1}")
                : CoreResources.LogSkipCollectionChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(1, 0, nameof(Cat), nameof(Cat.Mats)),
            message);
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Detect_reference_change_is_logged(bool sensitive, bool callDetectChangesTwice)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);
        var hat = cat.Hats.Single(h => h.Id == 77);

        _loggerFactory.Log.Clear();

        hat.Cat = null;

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ReferenceChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogReferenceChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Hat), nameof(Hat.Cat), "{Id: 77}")
                : CoreResources.LogReferenceChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Hat), nameof(Hat.Cat)),
            message);

        _loggerFactory.Log.Clear();

        hat.Cat = cat;

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ReferenceChangeDetected.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogReferenceChangeDetectedSensitive(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Hat), nameof(Hat.Cat), "{Id: 77}")
                : CoreResources.LogReferenceChangeDetected(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(Hat), nameof(Hat.Cat)),
            message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Start_tracking_is_logged_from_query(bool sensitive)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        _loggerFactory.Log.Clear();
        context.Cats.Find(1);

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StartedTracking.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogStartedTrackingSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(LikeAZooContextSensitive), nameof(Cat), "{Id: 1}")
                : CoreResources.LogStartedTracking(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(LikeAZooContext), nameof(Cat)),
            message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Start_tracking_is_logged_from_attach(bool sensitive)
    {
        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        _loggerFactory.Log.Clear();
        context.Attach(new Hat(88));

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StartedTracking.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogStartedTrackingSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(LikeAZooContextSensitive), nameof(Hat), "{Id: 88}")
                : CoreResources.LogStartedTracking(new TestLogger<TestLoggingDefinitions>())
                    .GenerateMessage(nameof(LikeAZooContext), nameof(Hat)),
            message);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void State_change_is_logged(bool sensitive)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        var cat = context.Cats.Find(1)!;

        _loggerFactory.Log.Clear();

        context.Entry(cat).State = EntityState.Deleted;

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.StateChanged.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            sensitive
                ? CoreResources.LogStateChangedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Cat), "{Id: 1}", nameof(LikeAZooContextSensitive), EntityState.Unchanged, EntityState.Deleted)
                : CoreResources.LogStateChanged(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                    nameof(Cat), nameof(LikeAZooContext), EntityState.Unchanged, EntityState.Deleted),
            message);
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task Value_generation_is_logged(bool sensitive, bool async, bool temporary)
    {
        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        ResetValueGenerator(
            context,
            context.Model.FindEntityType(typeof(Hat))!.FindProperty(nameof(Hat.Id))!,
            temporary);

        _loggerFactory.Log.Clear();

        if (async)
        {
            context.Add(new Hat(0));
        }
        else
        {
            await context.AddAsync(new Hat(0));
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ValueGenerated.Id);
        Assert.Equal(LogLevel.Debug, level);

        if (temporary)
        {
            Assert.Equal(
                sensitive
                    ? CoreResources.LogTempValueGeneratedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(LikeAZooContextSensitive), 1, nameof(Hat), nameof(Hat.Id))
                    : CoreResources.LogTempValueGenerated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(LikeAZooContext), nameof(Hat), nameof(Hat.Id)),
                message);
        }
        else
        {
            Assert.Equal(
                sensitive
                    ? CoreResources.LogValueGeneratedSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(LikeAZooContextSensitive), 1, nameof(Hat), nameof(Hat.Id))
                    : CoreResources.LogValueGenerated(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(LikeAZooContext), nameof(Hat), nameof(Hat.Id)),
                message);
        }
    }

    private static void ResetValueGenerator(DbContext context, IProperty property, bool generateTemporaryValues)
    {
        var cache = context.GetService<IValueGeneratorCache>();

        var generator = (ResettableValueGenerator)cache.GetOrAdd(
            property,
            property.DeclaringType,
            (p, e) => new ResettableValueGenerator())!;

        generator.Reset(generateTemporaryValues);
    }

    private class ResettableValueGenerator : ValueGenerator<int>
    {
        private int _current;
        private bool _generatesTemporaryValues;

        public override bool GeneratesTemporaryValues
            => _generatesTemporaryValues;

        public override int Next(EntityEntry entry)
            => Interlocked.Increment(ref _current);

        public void Reset(bool generateTemporaryValues)
        {
            _generatesTemporaryValues = generateTemporaryValues;
            _current = 0;
        }
    }

    [ConditionalTheory]
    [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(false, CascadeTiming.OnSaveChanges, null)]
    [InlineData(false, CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(false, CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(false, CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(false, CascadeTiming.Immediate, null)]
    [InlineData(false, CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(false, CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(false, CascadeTiming.Never, CascadeTiming.Never)]
    [InlineData(false, CascadeTiming.Never, null)]
    [InlineData(false, null, CascadeTiming.OnSaveChanges)]
    [InlineData(false, null, CascadeTiming.Immediate)]
    [InlineData(false, null, CascadeTiming.Never)]
    [InlineData(false, null, null)]
    [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(true, CascadeTiming.OnSaveChanges, null)]
    [InlineData(true, CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(true, CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(true, CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(true, CascadeTiming.Immediate, null)]
    [InlineData(true, CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(true, CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(true, CascadeTiming.Never, CascadeTiming.Never)]
    [InlineData(true, CascadeTiming.Never, null)]
    [InlineData(true, null, CascadeTiming.OnSaveChanges)]
    [InlineData(true, null, CascadeTiming.Immediate)]
    [InlineData(true, null, CascadeTiming.Never)]
    [InlineData(true, null, null)]
    public void Cascade_delete_is_logged(
        bool sensitive,
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        if (cascadeDeleteTiming.HasValue)
        {
            context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming.Value;
        }

        if (deleteOrphansTiming.HasValue)
        {
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming.Value;
        }

        var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

        LogLevel? cascadeDeleteLevel = null;
        string? cascadeDeleteMessage = null;
        string? deleteOrphansMessage = null;

        void CaptureMessages()
        {
            (cascadeDeleteLevel, _, cascadeDeleteMessage, _, _) =
                _loggerFactory.Log.FirstOrDefault(e => e.Id.Id == CoreEventId.CascadeDelete.Id);
            (_, _, deleteOrphansMessage, _, _) =
                _loggerFactory.Log.FirstOrDefault(e => e.Id.Id == CoreEventId.CascadeDeleteOrphan.Id);
        }

        void ClearMessages()
            => _loggerFactory.Log.Clear();

        switch (cascadeDeleteTiming)
        {
            case CascadeTiming.Immediate:
            case null:
                ClearMessages();

                context.Entry(cat).State = EntityState.Deleted;

                CaptureMessages();

                context.SaveChanges();
                break;
            case CascadeTiming.OnSaveChanges:
                context.Entry(cat).State = EntityState.Deleted;

                ClearMessages();

                context.SaveChanges();

                CaptureMessages();
                break;
            case CascadeTiming.Never:
                ClearMessages();

                context.Entry(cat).State = EntityState.Deleted;

                Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

                CaptureMessages();
                break;
        }

        Assert.Null(deleteOrphansMessage);

        if (cascadeDeleteTiming == CascadeTiming.Never)
        {
            Assert.Null(cascadeDeleteMessage);
        }
        else
        {
            Assert.Equal(LogLevel.Debug, cascadeDeleteLevel);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogCascadeDeleteSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(Hat), "{Id: 77}", EntityState.Deleted, nameof(Cat), "{Id: 1}")
                    : CoreResources.LogCascadeDelete(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(Hat), EntityState.Deleted, nameof(Cat)),
                cascadeDeleteMessage);
        }
    }

    [ConditionalTheory]
    [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(false, CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(false, CascadeTiming.OnSaveChanges, null)]
    [InlineData(false, CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(false, CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(false, CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(false, CascadeTiming.Immediate, null)]
    [InlineData(false, CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(false, CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(false, CascadeTiming.Never, CascadeTiming.Never)]
    [InlineData(false, CascadeTiming.Never, null)]
    [InlineData(false, null, CascadeTiming.OnSaveChanges)]
    [InlineData(false, null, CascadeTiming.Immediate)]
    [InlineData(false, null, CascadeTiming.Never)]
    [InlineData(false, null, null)]
    [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(true, CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(true, CascadeTiming.OnSaveChanges, null)]
    [InlineData(true, CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(true, CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(true, CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(true, CascadeTiming.Immediate, null)]
    [InlineData(true, CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(true, CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(true, CascadeTiming.Never, CascadeTiming.Never)]
    [InlineData(true, CascadeTiming.Never, null)]
    [InlineData(true, null, CascadeTiming.OnSaveChanges)]
    [InlineData(true, null, CascadeTiming.Immediate)]
    [InlineData(true, null, CascadeTiming.Never)]
    [InlineData(true, null, null)]
    public void Cascade_delete_orphan_is_logged(
        bool sensitive,
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        Seed(sensitive);

        using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
        if (cascadeDeleteTiming.HasValue)
        {
            context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming.Value;
        }

        if (deleteOrphansTiming.HasValue)
        {
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming.Value;
        }

        var cat = context.Cats.Include(e => e.Hats).Single(e => e.Id == 1);

        LogLevel? deleteOrphansLevel = null;
        string? cascadeDeleteMessage = null;
        string? deleteOrphansMessage = null;

        void CaptureMessages()
        {
            (_, _, cascadeDeleteMessage, _, _) = _loggerFactory.Log.FirstOrDefault(e => e.Id.Id == CoreEventId.CascadeDelete.Id);
            (deleteOrphansLevel, _, deleteOrphansMessage, _, _) =
                _loggerFactory.Log.FirstOrDefault(e => e.Id.Id == CoreEventId.CascadeDeleteOrphan.Id);
        }

        void ClearMessages()
            => _loggerFactory.Log.Clear();

        switch (deleteOrphansTiming)
        {
            case CascadeTiming.Immediate:
            case null:
                ClearMessages();

                cat.Hats.Clear();
                context.ChangeTracker.DetectChanges();

                CaptureMessages();

                context.SaveChanges();
                break;
            case CascadeTiming.OnSaveChanges:
                cat.Hats.Clear();
                context.ChangeTracker.DetectChanges();

                ClearMessages();

                context.SaveChanges();

                CaptureMessages();
                break;
            case CascadeTiming.Never:
                ClearMessages();

                cat.Hats.Clear();
                context.ChangeTracker.DetectChanges();

                Assert.Throws<InvalidOperationException>(() => context.SaveChanges());

                CaptureMessages();
                break;
        }

        Assert.Null(cascadeDeleteMessage);

        if (deleteOrphansTiming == CascadeTiming.Never)
        {
            Assert.Null(deleteOrphansMessage);
        }
        else
        {
            Assert.Equal(LogLevel.Debug, deleteOrphansLevel);
            Assert.Equal(
                sensitive
                    ? CoreResources.LogCascadeDeleteOrphanSensitive(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(
                        nameof(Hat), "{Id: 77}", EntityState.Deleted, nameof(Cat))
                    : CoreResources.LogCascadeDeleteOrphan(new TestLogger<TestLoggingDefinitions>())
                        .GenerateMessage(nameof(Hat), EntityState.Deleted, nameof(Cat)),
                deleteOrphansMessage);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveChanges_is_logged(bool async)
    {
        Seed();

        using var context = new LikeAZooContext();
        var cat = context.Cats.Find(1)!;

        context.Entry(cat).State = EntityState.Deleted;

        _loggerFactory.Log.Clear();

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SaveChangesStarting.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            CoreResources.LogSaveChangesStarting(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(nameof(LikeAZooContext)),
            message);

        (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.SaveChangesCompleted.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            CoreResources.LogSaveChangesCompleted(new TestLogger<TestLoggingDefinitions>())
                .GenerateMessage(nameof(LikeAZooContext), 1), message);
    }

    [ConditionalFact]
    public void Context_Dispose_is_logged()
    {
        using (var context = new LikeAZooContext())
        {
            context.Cats.Find(1);

            _loggerFactory.Log.Clear();
        }

        var (level, _, message, _, _) = _loggerFactory.Log.Single(e => e.Id.Id == CoreEventId.ContextDisposed.Id);
        Assert.Equal(LogLevel.Debug, level);
        Assert.Equal(
            CoreResources.LogContextDisposed(new TestLogger<TestLoggingDefinitions>()).GenerateMessage(nameof(LikeAZooContext)),
            message);
    }

    [ConditionalFact]
    public void State_change_events_fire_from_query()
    {
        var tracking = new List<EntityTrackingEventArgs>();
        var tracked = new List<EntityTrackedEventArgs>();
        var changing = new List<EntityStateChangingEventArgs>();
        var changed = new List<EntityStateChangedEventArgs>();

        Seed(usePool: true);

        using (var scope = _poolProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

            RegisterEvents(context, tracking, tracked, changing, changed);

            Assert.Equal(2, context.Cats.OrderBy(e => e.Id).ToList().Count);

            Assert.Equal(2, tracking.Count);
            Assert.Equal(2, tracked.Count);
            Assert.Empty(changing);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 1, EntityState.Unchanged, tracking[0], tracked[0], fromQuery: true);
            AssertTrackedEvent(context, 2, EntityState.Unchanged, tracking[1], tracked[1], fromQuery: true);
        }

        using (var scope = _poolProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

            Assert.Equal(2, context.Cats.OrderBy(e => e.Id).ToList().Count);

            Assert.Equal(2, tracked.Count);
            Assert.Empty(changed);
        }
    }

    [ConditionalFact]
    public void State_change_events_fire_from_Attach()
    {
        var tracking = new List<EntityTrackingEventArgs>();
        var tracked = new List<EntityTrackedEventArgs>();
        var changing = new List<EntityStateChangingEventArgs>();
        var changed = new List<EntityStateChangedEventArgs>();

        using var scope = _poolProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterEvents(context, tracking, tracked, changing, changed);

        context.Attach(new Cat(1));

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Single(tracked);
        Assert.Empty(changed);

        AssertTrackedEvent(context, 1, EntityState.Unchanged, tracking[0], tracked[0], fromQuery: false);

        context.Entry(new Cat(2)).State = EntityState.Unchanged;

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(2, tracked.Count);
        Assert.Empty(changed);

        AssertTrackedEvent(context, 2, EntityState.Unchanged, tracking[1], tracked[1], fromQuery: false);
    }

    [ConditionalFact]
    public void State_change_events_fire_from_Add()
    {
        var tracking = new List<EntityTrackingEventArgs>();
        var tracked = new List<EntityTrackedEventArgs>();
        var changing = new List<EntityStateChangingEventArgs>();
        var changed = new List<EntityStateChangedEventArgs>();

        using var scope = _poolProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterEvents(context, tracking, tracked, changing, changed);

        context.Add(new Cat(1));

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Single(tracked);
        Assert.Empty(changed);

        AssertTrackedEvent(context, 1, EntityState.Added, tracking[0], tracked[0], fromQuery: false);

        context.Entry(new Cat(2)).State = EntityState.Added;

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(2, tracked.Count);
        Assert.Empty(changed);

        AssertTrackedEvent(context, 2, EntityState.Added, tracking[1], tracked[1], fromQuery: false);
    }

    [ConditionalFact]
    public void State_change_events_fire_from_Update()
    {
        var tracking = new List<EntityTrackingEventArgs>();
        var tracked = new List<EntityTrackedEventArgs>();
        var changing = new List<EntityStateChangingEventArgs>();
        var changed = new List<EntityStateChangedEventArgs>();

        using var scope = _poolProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterEvents(context, tracking, tracked, changing, changed);

        context.Update(new Cat(1));

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Single(tracked);
        Assert.Empty(changed);

        AssertTrackedEvent(context, 1, EntityState.Modified, tracking[0], tracked[0], fromQuery: false);

        context.Entry(new Cat(2)).State = EntityState.Modified;

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Equal(2, tracked.Count);
        Assert.Empty(changed);

        AssertTrackedEvent(context, 2, EntityState.Modified, tracking[1], tracked[1], fromQuery: false);
    }

    [ConditionalFact]
    public void State_change_events_fire_for_tracked_state_changes()
    {
        var tracking = new List<EntityTrackingEventArgs>();
        var tracked = new List<EntityTrackedEventArgs>();
        var changing = new List<EntityStateChangingEventArgs>();
        var changed = new List<EntityStateChangedEventArgs>();

        using (var scope = _poolProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

            RegisterEvents(context, tracking, tracked, changing, changed);

            context.AddRange(new Cat(1), new Cat(2));

            Assert.True(context.ChangeTracker.HasChanges());

            Assert.Equal(2, tracked.Count);
            Assert.Empty(changed);

            AssertTrackedEvent(context, 1, EntityState.Added, tracking[0], tracked[0], fromQuery: false);
            AssertTrackedEvent(context, 2, EntityState.Added, tracking[1], tracked[1], fromQuery: false);

            context.Entry(context.Cats.Find(1)!).State = EntityState.Unchanged;
            context.Entry(context.Cats.Find(2)!).State = EntityState.Modified;

            Assert.Equal(2, tracked.Count);
            Assert.Equal(2, changed.Count);

            Assert.True(context.ChangeTracker.HasChanges());

            AssertChangedEvent(context, 1, EntityState.Added, EntityState.Unchanged, changing[0], changed[0]);
            AssertChangedEvent(context, 2, EntityState.Added, EntityState.Modified, changing[1], changed[1]);

            context.Entry(context.Cats.Find(1)!).State = EntityState.Added;
            context.Entry(context.Cats.Find(2)!).State = EntityState.Deleted;

            Assert.Equal(2, tracked.Count);
            Assert.Equal(4, changed.Count);

            AssertChangedEvent(context, 1, EntityState.Unchanged, EntityState.Added, changing[2], changed[2]);
            AssertChangedEvent(context, 2, EntityState.Modified, EntityState.Deleted, changing[3], changed[3]);

            context.Remove(context.Cats.Find(1)!);
            context.Entry(context.Cats.Find(2)!).State = EntityState.Detached;

            Assert.False(context.ChangeTracker.HasChanges());

            Assert.Equal(2, tracked.Count);
            Assert.Equal(6, changed.Count);

            AssertChangedEvent(context, null, EntityState.Added, EntityState.Detached, changing[4], changed[4]);
            AssertChangedEvent(context, null, EntityState.Deleted, EntityState.Detached, changing[5], changed[5]);
        }

        using (var scope = _poolProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

            context.AddRange(new Cat(1), new Cat(2));

            context.Entry(context.Cats.Find(1)!).State = EntityState.Unchanged;
            context.Entry(context.Cats.Find(2)!).State = EntityState.Modified;

            context.Entry(context.Cats.Find(1)!).State = EntityState.Added;
            context.Entry(context.Cats.Find(2)!).State = EntityState.Deleted;

            context.Remove(context.Cats.Find(1)!);
            context.Entry(context.Cats.Find(2)!).State = EntityState.Detached;

            Assert.Equal(2, tracked.Count);
            Assert.Equal(6, changed.Count);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void State_change_events_fire_when_saving_changes(bool callDetectChangesTwice)
    {
        var tracking = new List<EntityTrackingEventArgs>();
        var tracked = new List<EntityTrackedEventArgs>();
        var changing = new List<EntityStateChangingEventArgs>();
        var changed = new List<EntityStateChangedEventArgs>();

        Seed(usePool: true);

        using var scope = _poolProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterEvents(context, tracking, tracked, changing, changed);

        var cat1 = context.Cats.Find(1)!;

        Assert.Single(tracked);
        Assert.Empty(changed);

        AssertTrackedEvent(context, 1, EntityState.Unchanged, tracking[0], tracked[0], fromQuery: true);

        context.Add(new Cat(3));
        cat1.Name = "Clippy";

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        Assert.Equal(2, tracked.Count);
        Assert.Single(changed);

        AssertTrackedEvent(context, 3, EntityState.Added, tracking[1], tracked[1], fromQuery: false);
        AssertChangedEvent(context, 1, EntityState.Unchanged, EntityState.Modified, changing[0], changed[0]);

        Assert.True(context.ChangeTracker.HasChanges());

        context.SaveChanges();

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Equal(2, tracked.Count);
        Assert.Equal(3, changed.Count);

        AssertChangedEvent(context, 1, EntityState.Modified, EntityState.Unchanged, changing[2], changed[2]);
        AssertChangedEvent(context, 3, EntityState.Added, EntityState.Unchanged, changing[1], changed[1]);

        context.Database.EnsureDeleted();
    }

    [ConditionalFact]
    public void State_change_events_fire_when_property_modified_flags_cause_state_change()
    {
        var tracking = new List<EntityTrackingEventArgs>();
        var tracked = new List<EntityTrackedEventArgs>();
        var changing = new List<EntityStateChangingEventArgs>();
        var changed = new List<EntityStateChangedEventArgs>();

        using var scope = _poolProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterEvents(context, tracking, tracked, changing, changed);

        var cat = context.Attach(
            new Cat(3) { Name = "Achilles" }).Entity;

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Single(tracked);
        Assert.Empty(changed);

        AssertTrackedEvent(context, 3, EntityState.Unchanged, tracking[0], tracked[0], fromQuery: false);

        context.Entry(cat).Property(e => e.Name).IsModified = true;

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Single(tracked);
        Assert.Single(changed);

        AssertChangedEvent(context, 3, EntityState.Unchanged, EntityState.Modified, changing[0], changed[0]);

        context.Entry(cat).Property(e => e.Name).IsModified = false;

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Single(tracked);
        Assert.Equal(2, changed.Count);

        AssertChangedEvent(context, 3, EntityState.Modified, EntityState.Unchanged, changing[1], changed[1]);
    }

    [ConditionalFact]
    public void State_change_events_are_limited_to_the_current_context()
    {
        var tracking1 = new List<EntityTrackingEventArgs>();
        var tracked1 = new List<EntityTrackedEventArgs>();
        var changing1 = new List<EntityStateChangingEventArgs>();
        var changed1 = new List<EntityStateChangedEventArgs>();
        var tracking2 = new List<EntityTrackingEventArgs>();
        var tracked2 = new List<EntityTrackedEventArgs>();
        var changing2 = new List<EntityStateChangingEventArgs>();
        var changed2 = new List<EntityStateChangedEventArgs>();

        Seed(usePool: true);

        using var scope = _poolProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterEvents(context, tracking1, tracked1, changing1, changed1);

        using (var scope2 = _poolProvider.CreateScope())
        {
            var context2 = scope2.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

            RegisterEvents(context2, tracking2, tracked2, changing2, changed2);

            Assert.Equal(2, context2.Cats.OrderBy(e => e.Id).ToList().Count);

            Assert.Equal(2, tracked2.Count);
            Assert.Empty(changed2);

            context2.Entry(context2.Cats.Find(1)!).State = EntityState.Modified;

            Assert.Equal(2, tracked2.Count);
            Assert.Single(changed2);

            Assert.Empty(tracked1);
            Assert.Empty(changed1);
        }

        Assert.Equal(2, context.Cats.OrderBy(e => e.Id).ToList().Count);

        Assert.Equal(2, tracked1.Count);
        Assert.Empty(changed1);

        context.Entry(context.Cats.Find(1)!).State = EntityState.Modified;

        Assert.Equal(2, tracked1.Count);
        Assert.Single(changed1);

        Assert.Equal(2, tracked2.Count);
        Assert.Single(changed2);

        context.Database.EnsureDeleted();
    }

    [ConditionalFact]
    public void DetectChanges_events_fire_for_no_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        context.AttachRange(new Cat(1), new Cat(2));

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);

        Assert.False(context.ChangeTracker.HasChanges());

        Assert.Single(detectingAll);
        Assert.Single(detectedAll);
        Assert.Equal(2, detectingEntity.Count);
        Assert.Equal(2, detectedEntity.Count);

        Assert.False(detectedAll[0].ChangesFound);
        Assert.False(detectedEntity[0].ChangesFound);
        Assert.False(detectedEntity[1].ChangesFound);
    }

    [ConditionalFact]
    public void DetectChanges_events_fire_for_property_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var cat = new Cat(1);
        context.AttachRange(cat, new Cat(2));
        cat.Name = "Alice";

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Single(detectingAll);
        Assert.Single(detectedAll);
        Assert.Equal(2, detectingEntity.Count);
        Assert.Equal(2, detectedEntity.Count);

        Assert.True(detectedAll[0].ChangesFound);
        Assert.True(detectedEntity[0].ChangesFound);
        Assert.False(detectedEntity[1].ChangesFound);
    }

    [ConditionalFact]
    public void DetectChanges_events_fire_for_fk_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        using var context = new EarlyLearningCenter();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var product = new Product { Category = new Category() };
        context.Attach(product);
        product.CategoryId = 2;

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Single(detectingAll);
        Assert.Single(detectedAll);
        Assert.Equal(2, detectingEntity.Count);
        Assert.Equal(2, detectedEntity.Count);

        Assert.True(detectedAll[0].ChangesFound);
        Assert.True(detectedEntity[0].ChangesFound);
        Assert.False(detectedEntity[1].ChangesFound);
    }

    [ConditionalFact]
    public void DetectChanges_events_fire_for_reference_navigation_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        using var context = new EarlyLearningCenter();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var product = new Product { Category = new Category() };
        context.Attach(product);
        product.Category = null;

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Single(detectingAll);
        Assert.Single(detectedAll);
        Assert.Equal(2, detectingEntity.Count);
        Assert.Equal(2, detectedEntity.Count);

        Assert.True(detectedAll[0].ChangesFound);
        Assert.True(detectedEntity[0].ChangesFound);
        Assert.False(detectedEntity[1].ChangesFound);
    }

    [ConditionalFact]
    public void DetectChanges_events_fire_for_collection_navigation_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        using var context = new EarlyLearningCenter();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var product = new Product { Category = new Category() };
        context.Attach(product);
        product.Category.Products.Clear();

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Single(detectingAll);
        Assert.Single(detectedAll);
        Assert.Equal(2, detectingEntity.Count);
        Assert.Equal(2, detectedEntity.Count);

        Assert.True(detectedAll[0].ChangesFound);
        Assert.False(detectedEntity[0].ChangesFound);
        Assert.True(detectedEntity[1].ChangesFound);
    }

    [ConditionalFact]
    public void DetectChanges_events_fire_for_skip_navigation_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var cat = new Cat(1) { Hats = { new Hat(2) } };
        context.Attach(cat);
        cat.Hats.Clear();

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);

        Assert.True(context.ChangeTracker.HasChanges());

        Assert.Single(detectingAll);
        Assert.Single(detectedAll);
        Assert.Equal(2, detectingEntity.Count);
        Assert.Equal(2, detectedEntity.Count);

        Assert.True(detectedAll[0].ChangesFound);
        Assert.True(detectedEntity[0].ChangesFound);
        Assert.False(detectedEntity[1].ChangesFound);
    }

    [ConditionalFact]
    public void Local_DetectChanges_events_fire_for_no_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var cat = new Cat(1);
        context.AttachRange(cat, new Cat(2));

        Assert.Empty(detectingEntity);
        Assert.Empty(detectedEntity);

        _ = context.Entry(cat);

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);
        Assert.Single(detectingEntity);
        Assert.Single(detectedEntity);

        AssertLocalDetectChangesEvent(changesFound: false, detectingEntity[0], detectedEntity[0]);
    }

    [ConditionalFact]
    public void Local_DetectChanges_events_fire_for_property_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var cat = new Cat(1);
        context.AttachRange(cat, new Cat(2));
        cat.Name = "Alice";

        Assert.Empty(detectingEntity);
        Assert.Empty(detectedEntity);

        _ = context.Entry(cat);

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);
        Assert.Single(detectingEntity);
        Assert.Single(detectedEntity);

        AssertLocalDetectChangesEvent(changesFound: true, detectingEntity[0], detectedEntity[0]);
    }

    [ConditionalFact]
    public void Local_DetectChanges_events_fire_for_fk_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        using var context = new EarlyLearningCenter();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var product = new Product { Category = new Category() };
        context.Attach(product);
        product.CategoryId = 2;

        Assert.Empty(detectingEntity);
        Assert.Empty(detectedEntity);

        _ = context.Entry(product);

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);
        Assert.Single(detectingEntity);
        Assert.Single(detectedEntity);

        AssertLocalDetectChangesEvent(changesFound: true, detectingEntity[0], detectedEntity[0]);
    }

    [ConditionalFact]
    public void Local_DetectChanges_events_fire_for_reference_navigation_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        using var context = new EarlyLearningCenter();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var product = new Product { Category = new Category() };
        context.Attach(product);
        product.Category = null;

        Assert.Empty(detectingEntity);
        Assert.Empty(detectedEntity);

        _ = context.Entry(product);

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);
        Assert.Equal(2, detectingEntity.Count);
        Assert.Equal(2, detectedEntity.Count);

        AssertLocalDetectChangesEvent(changesFound: false, detectingEntity[0], detectedEntity[0]);
        AssertLocalDetectChangesEvent(changesFound: true, detectingEntity[1], detectedEntity[1]);
    }

    [ConditionalFact]
    public void Local_DetectChanges_events_fire_for_collection_navigation_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        using var context = new EarlyLearningCenter();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var product = new Product { Category = new Category() };
        context.Attach(product);
        product.Category.Products.Clear();

        Assert.Empty(detectingEntity);
        Assert.Empty(detectedEntity);

        _ = context.Entry(product.Category);

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);
        Assert.Single(detectingEntity);
        Assert.Single(detectedEntity);

        AssertLocalDetectChangesEvent(changesFound: true, detectingEntity[0], detectedEntity[0]);
    }

    [ConditionalFact]
    public void Local_DetectChanges_events_fire_for_skip_navigation_change()
    {
        var detectingAll = new List<DetectChangesEventArgs>();
        var detectedAll = new List<DetectedChangesEventArgs>();
        var detectingEntity = new List<DetectEntityChangesEventArgs>();
        var detectedEntity = new List<DetectedEntityChangesEventArgs>();

        using var scope = _poolProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        RegisterDetectAllEvents(context, detectingAll, detectedAll);
        RegisterDetectEntityEvents(context, detectingEntity, detectedEntity);

        var cat = new Cat(1) { Hats = { new Hat(2) } };
        context.Attach(cat);
        cat.Hats.Clear();

        Assert.Empty(detectingEntity);
        Assert.Empty(detectedEntity);

        _ = context.Entry(cat);

        Assert.Empty(detectingAll);
        Assert.Empty(detectedAll);
        Assert.Single(detectingEntity);
        Assert.Single(detectedEntity);

        AssertLocalDetectChangesEvent(changesFound: true, detectingEntity[0], detectedEntity[0]);
    }

    [ConditionalFact] // Issue #26506
    public void DetectChanges_event_can_be_used_to_know_when_all_properties_have_changed()
    {
        using var scope = _poolProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>();

        var hat = new Hat(2) { Color = "Orange" };
        var cat1 = new Cat(1) { Hats = { hat } };
        var cat2 = new Cat(2);
        context.AttachRange(cat1, cat2);

        var stateChangedCalled = false;
        context.ChangeTracker.StateChanged += (s, a) =>
        {
            stateChangedCalled = true;
            Assert.Equal("Black", a.Entry.Property(nameof(Hat.Color)).CurrentValue);
            Assert.Equal(1, a.Entry.Property(nameof(Hat.CatId)).CurrentValue);
        };

        var detectedChangesCalled = false;
        context.ChangeTracker.DetectedEntityChanges += (s, a) =>
        {
            if (a.ChangesFound)
            {
                detectedChangesCalled = true;

                Assert.Equal("Black", a.Entry.Property(nameof(Hat.Color)).CurrentValue);
                Assert.Equal(2, a.Entry.Property(nameof(Hat.CatId)).CurrentValue);
            }
        };

        hat.Cat = cat2;
        hat.Color = "Black";

        context.ChangeTracker.DetectChanges();

        Assert.True(stateChangedCalled);
        Assert.True(detectedChangesCalled);

        Assert.Equal(2, hat.CatId);
    }

    private static void AssertTrackedEvent(
        LikeAZooContext context,
        int id,
        EntityState newState,
        EntityTrackingEventArgs tracking,
        EntityTrackedEventArgs tracked,
        bool fromQuery)
    {
        Assert.Same(tracking.Entry.Entity, tracked.Entry.Entity);
        Assert.Equal(newState, tracking.State);
        Assert.Equal(newState, tracked.Entry.State);
        Assert.Equal(fromQuery, tracking.FromQuery);
        Assert.Equal(fromQuery, tracked.FromQuery);
        Assert.Same(context.Cats.Find(id), tracked.Entry.Entity);
    }

    private static void AssertChangedEvent(
        LikeAZooContext context,
        int? id,
        EntityState oldState,
        EntityState newState,
        EntityStateChangingEventArgs changing,
        EntityStateChangedEventArgs changed)
    {
        Assert.Same(changing.Entry.Entity, changed.Entry.Entity);
        Assert.Equal(oldState, changing.OldState);
        Assert.Equal(newState, changing.NewState);
        Assert.Equal(oldState, changed.OldState);
        Assert.Equal(newState, changed.NewState);
        Assert.Equal(newState, changed.Entry.State);

        if (id != null)
        {
            Assert.Same(context.Cats.Find(id), changed.Entry.Entity);
        }
    }

    private static void AssertLocalDetectChangesEvent(
        bool changesFound,
        DetectEntityChangesEventArgs detectingEntity,
        DetectedEntityChangesEventArgs detectedEntity)
    {
        Assert.Same(detectingEntity.Entry.Entity, detectedEntity.Entry.Entity);
        Assert.Equal(changesFound, detectedEntity.ChangesFound);
    }

    private static void RegisterEvents(
        DbContext context,
        IList<EntityTrackingEventArgs> tracking,
        IList<EntityTrackedEventArgs> tracked,
        IList<EntityStateChangingEventArgs> changing,
        IList<EntityStateChangedEventArgs> changed)
    {
        context.ChangeTracker.Tracking += (s, e) =>
        {
            Assert.Same(context.ChangeTracker, s);
            tracking.Add(e);
        };

        context.ChangeTracker.Tracked += (s, e) =>
        {
            Assert.Same(context.ChangeTracker, s);
            tracked.Add(e);
        };

        context.ChangeTracker.StateChanging += (s, e) =>
        {
            Assert.Same(context.ChangeTracker, s);
            Assert.Equal(e.OldState, e.Entry.State);
            changing.Add(e);
        };

        context.ChangeTracker.StateChanged += (s, e) =>
        {
            Assert.Same(context.ChangeTracker, s);
            Assert.Equal(e.NewState, e.Entry.State);
            changed.Add(e);
        };
    }

    private static void RegisterDetectAllEvents(
        DbContext context,
        IList<DetectChangesEventArgs> detectingAll,
        IList<DetectedChangesEventArgs> detectedAll)
    {
        context.ChangeTracker.DetectingAllChanges += (s, e) =>
        {
            _ = context.ChangeTracker.Entries().ToList(); // Should not recursively call DetectChanges
            Assert.Same(context.ChangeTracker, s);
            detectingAll.Add(e);
        };

        context.ChangeTracker.DetectedAllChanges += (s, e) =>
        {
            _ = context.ChangeTracker.Entries().ToList(); // Should not recursively call DetectChanges
            Assert.Same(context.ChangeTracker, s);
            detectedAll.Add(e);
        };
    }

    private static void RegisterDetectEntityEvents(
        DbContext context,
        IList<DetectEntityChangesEventArgs> detectingEntity,
        IList<DetectedEntityChangesEventArgs> detectedEntity)
    {
        context.ChangeTracker.DetectingEntityChanges += (s, e) =>
        {
            _ = context.ChangeTracker.Entries().ToList(); // Should not recursively call DetectChanges
            Assert.Same(context.ChangeTracker, s);
            Assert.NotNull(e.Entry);
            detectingEntity.Add(e);
        };

        context.ChangeTracker.DetectedEntityChanges += (s, e) =>
        {
            _ = context.ChangeTracker.Entries().ToList(); // Should not recursively call DetectChanges
            Assert.Same(context.ChangeTracker, s);
            Assert.NotNull(e.Entry);
            detectedEntity.Add(e);
        };
    }

    private class Wocket
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public required Pocket Pocket { get; set; }
    }

    private class Pocket
    {
        public string? Contents { get; set; }
    }

    private class Cat(int id)
    {
        public IEntityType? EntityType { get; set; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; } = id;

        public string? Name { get; set; }

        public ICollection<Hat> Hats { get; } = new List<Hat>();

        public ICollection<Mat> Mats { get; } = new List<Mat>();
    }

    private class Hat(int id)
    {

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; } = id;

        public string? Color { get; set; }

        public int CatId { get; set; }
        public Cat? Cat { get; set; }
    }

    private class Mat(int id)
    {

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public int Id { get; private set; } = id;

        public ICollection<Cat> Cats { get; } = new List<Cat>();
    }

    private class CatMat
    {
        public int CatId { get; set; }
        public int MatId { get; set; }
    }

    private static readonly ListLoggerFactory _loggerFactory = new();

    private static readonly IServiceProvider _serviceProvider
        = InMemoryFixture.BuildServiceProvider(_loggerFactory);

    private static readonly IServiceProvider _sensitiveProvider
        = InMemoryFixture.BuildServiceProvider(_loggerFactory);

    private static readonly IServiceProvider _poolProvider
        = new ServiceCollection()
            .AddDbContextPool<LikeAZooContextPooled>(
                p => p.UseInMemoryDatabase(nameof(LikeAZooContextPooled))
                    .UseInternalServiceProvider(InMemoryFixture.BuildServiceProvider(_loggerFactory)))
            .BuildServiceProvider(validateScopes: true);

    private class LikeAZooContextPooled(DbContextOptions<LikeAZooContextPooled> options) : LikeAZooContext(options)
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
    }

    private class LikeAZooContext : DbContext
    {
        public LikeAZooContext()
        {
        }

        protected LikeAZooContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Cat> Cats
            => Set<Cat>();

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(LikeAZooContext));

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Cat>()
                .Property(e => e.Id)
                .HasValueGenerator((_, __) => new ResettableValueGenerator());

            modelBuilder
                .Entity<Hat>()
                .Property(e => e.Id)
                .HasValueGenerator((_, __) => new ResettableValueGenerator());

            modelBuilder.Entity<Mat>(
                b =>
                {
                    b.Property(e => e.Id).HasValueGenerator((_, __) => new ResettableValueGenerator());
                    b.HasMany(e => e.Cats)
                        .WithMany(e => e.Mats)
                        .UsingEntity<CatMat>(
                            ts => ts.HasOne<Cat>().WithMany(),
                            ts => ts.HasOne<Mat>().WithMany())
                        .HasKey(ts => new { ts.CatId, ts.MatId });
                });

            modelBuilder.Entity<Wocket>().ComplexProperty(e => e.Pocket);
        }
    }

    private class LikeAZooContextSensitive : LikeAZooContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(_sensitiveProvider)
                .UseInMemoryDatabase(nameof(LikeAZooContextSensitive));
    }

    private void Seed(bool sensitive = false, bool usePool = false)
    {
        void Seed(LikeAZooContext context)
        {
            context.Database.EnsureDeleted();

            var cat1 = new Cat(1) { Name = "Smokey" };
            var cat2 = new Cat(2) { Name = "Sid" };

            cat1.Hats.Add(new Hat(77) { Color = "Pine Green" });

            context.AddRange(cat1, cat2);

            var mat = new Mat(77);
            context.Add(mat);
            cat1.Mats.Add(mat);

            context.SaveChanges();
        }

        if (usePool)
        {
            using var scope = _poolProvider.CreateScope();
            Seed(scope.ServiceProvider.GetRequiredService<LikeAZooContextPooled>());
        }
        else
        {
            using var context = sensitive ? new LikeAZooContextSensitive() : new LikeAZooContext();
            Seed(context);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Can_remove_dependent_identifying_one_to_many(bool saveEntities)
    {
        using var context = new EarlyLearningCenter();
        var product = new Product();
        var order = new Order();
        var orderDetails = new OrderDetails { Order = order, Product = product };

        context.Add(orderDetails);
        if (saveEntities)
        {
            context.SaveChanges();
        }

        var expectedState = saveEntities ? EntityState.Unchanged : EntityState.Added;

        Assert.Equal(expectedState, context.Entry(product).State);
        Assert.Equal(expectedState, context.Entry(order).State);
        Assert.Equal(expectedState, context.Entry(orderDetails).State);

        Assert.Same(orderDetails, product.OrderDetails.Single());
        Assert.Same(orderDetails, order.OrderDetails.Single());

        order.OrderDetails.Remove(orderDetails);

        Assert.Equal(expectedState, context.Entry(product).State);
        Assert.Equal(expectedState, context.Entry(order).State);
        Assert.Equal(saveEntities ? EntityState.Deleted : EntityState.Detached, context.Entry(orderDetails).State);

        Assert.Empty(product.OrderDetails);
        Assert.Empty(order.OrderDetails);

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(order).State);
        Assert.Equal(EntityState.Detached, context.Entry(orderDetails).State);

        Assert.Empty(product.OrderDetails);
        Assert.Empty(order.OrderDetails);
    }

    [ConditionalFact] // Issue #26827
    public void Setting_dependent_to_null_for_client_cascaded_optional_is_not_overwritten_by_DetectChanges()
    {
        using var context = new EarlyLearningCenter();
        var bobby = context.Add(new Bobby { Buggy = new Buggy() }).Entity;

        context.SaveChanges();
        context.ChangeTracker.Clear();

        bobby = context.Set<Bobby>().Include(e => e.Buggy).Single(e => e.Id == bobby.Id);

        Assert.NotNull(bobby.Buggy);

        bobby.Buggy = null;

        Assert.Null(bobby.Buggy);

        context.ChangeTracker.DetectChanges();

        Assert.Null(bobby.Buggy);
    }

    [ConditionalFact]
    public void Keyless_type_negative_cases()
    {
        using var context = new EarlyLearningCenter();
        var whoAmI = new WhoAmI();

        Assert.Equal(
            CoreStrings.KeylessTypeTracked("WhoAmI"),
            Assert.Throws<InvalidOperationException>(() => context.Add(whoAmI)).Message);

        Assert.Equal(
            CoreStrings.KeylessTypeTracked("WhoAmI"),
            Assert.Throws<InvalidOperationException>(() => context.Remove(whoAmI)).Message);

        Assert.Equal(
            CoreStrings.KeylessTypeTracked("WhoAmI"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(whoAmI)).Message);

        Assert.Equal(
            CoreStrings.KeylessTypeTracked("WhoAmI"),
            Assert.Throws<InvalidOperationException>(() => context.Update(whoAmI)).Message);

        Assert.Equal(
            CoreStrings.InvalidSetKeylessOperation("WhoAmI"),
            Assert.Throws<InvalidOperationException>(() => context.Find<WhoAmI>(1)).Message);

        Assert.Equal(
            CoreStrings.InvalidSetKeylessOperation("WhoAmI"),
            Assert.Throws<InvalidOperationException>(() => context.Set<WhoAmI>().Local).Message);

        Assert.Equal(
            CoreStrings.KeylessTypeTracked("WhoAmI"),
            Assert.Throws<InvalidOperationException>(() => context.Entry(whoAmI)).Message);
    }

    [ConditionalFact]
    public void Can_get_all_entries()
    {
        using var context = new EarlyLearningCenter();
        var category = context.Add(new Category()).Entity;
        var product = context.Add(new Product()).Entity;

        Assert.Equal(
            new object[] { category, product },
            context.ChangeTracker.Entries().Select(e => e.Entity).OrderBy(e => e.GetType().Name));
    }

    [ConditionalFact]
    public void Can_get_all_entities_for_an_entity_of_a_given_type()
    {
        using var context = new EarlyLearningCenter();
        var category = context.Add(new Category()).Entity;
        var product = context.Add(new Product()).Entity;

        Assert.Equal(
            new object[] { product },
            context.ChangeTracker.Entries<Product>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));

        Assert.Equal(
            new object[] { category },
            context.ChangeTracker.Entries<Category>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));

        Assert.Equal(
            new object[] { category, product },
            context.ChangeTracker.Entries<object>().Select(e => e.Entity).OrderBy(e => e.GetType().Name));
    }

    [ConditionalFact]
    public void Can_get_Context()
    {
        using var context = new EarlyLearningCenter();
        Assert.Same(context, context.ChangeTracker.Context);
    }

    [ConditionalTheory] // Issue #17828
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void DetectChanges_reparents_even_when_immediate_cascade_enabled(bool delayCascade, bool callDetectChangesTwice)
    {
        using var context = new EarlyLearningCenter();

        // Construct initial state
        var parent1 = new Category { Id = 1 };
        var parent2 = new Category { Id = 2 };
        var child = new Product { Id = 3, Category = parent1 };

        context.AddRange(parent1, parent2, child);
        context.ChangeTracker.AcceptAllChanges();

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Unchanged, context.Entry(parent1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(child).State);

        if (delayCascade)
        {
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        }

        child.Category = parent2;

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        context.Remove(parent1);

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(EntityState.Deleted, context.Entry(parent1).State);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent2).State);
        Assert.Equal(EntityState.Modified, context.Entry(child).State);
    }

    [ConditionalTheory] // Issue #19203
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dependent_FKs_are_not_nulled_when_principal_is_detached(bool delayCascade, bool trackNewDependents)
    {
        using var context = new EarlyLearningCenter(new UpdatingIdentityResolutionInterceptor());

        var category = new OptionalCategory
        {
            Id = 1,
            Products =
            {
                new OptionalProduct { Id = 1 },
                new OptionalProduct { Id = 2 },
                new OptionalProduct { Id = 3 }
            }
        };

        context.Attach(category);

        var categoryEntry = context.Entry(category);
        var product0Entry = context.Entry(category.Products[0]);
        var product1Entry = context.Entry(category.Products[1]);
        var product2Entry = context.Entry(category.Products[2]);

        Assert.Equal(EntityState.Unchanged, categoryEntry.State);
        Assert.Equal(EntityState.Unchanged, product0Entry.State);
        Assert.Equal(EntityState.Unchanged, product1Entry.State);
        Assert.Equal(EntityState.Unchanged, product2Entry.State);

        if (delayCascade)
        {
            context.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
        }

        context.Entry(category).State = EntityState.Detached;

        Assert.Equal(EntityState.Detached, categoryEntry.State);

        Assert.Equal(EntityState.Unchanged, product0Entry.State);
        Assert.Equal(EntityState.Unchanged, product1Entry.State);
        Assert.Equal(EntityState.Unchanged, product2Entry.State);

        var newCategory = new OptionalCategory { Id = 1, };

        if (trackNewDependents)
        {
            newCategory.Products.AddRange(
                new OptionalProduct[]
                {
                    new() { Id = 1, CategoryId = category.Id },
                    new() { Id = 2, CategoryId = category.Id },
                    new() { Id = 3, CategoryId = category.Id }
                });
        }

        context.Update(newCategory);

        Assert.Equal(4, context.ChangeTracker.Entries().Count());

        categoryEntry = context.Entry(newCategory);
        product0Entry = context.Entry(newCategory.Products[0]);
        product1Entry = context.Entry(newCategory.Products[1]);
        product2Entry = context.Entry(newCategory.Products[2]);

        Assert.Equal(EntityState.Modified, categoryEntry.State);

        Assert.Equal(EntityState.Unchanged, product0Entry.State);
        Assert.Equal(EntityState.Unchanged, product1Entry.State);
        Assert.Equal(EntityState.Unchanged, product2Entry.State);

        Assert.Same(newCategory.Products[0], category.Products[0]);
        Assert.Same(newCategory.Products[1], category.Products[1]);
        Assert.Same(newCategory.Products[2], category.Products[2]);

        Assert.Same(newCategory, newCategory.Products[0].Category);
        Assert.Same(newCategory, newCategory.Products[1].Category);
        Assert.Same(newCategory, newCategory.Products[2].Category);

        Assert.Equal(newCategory.Id, product0Entry.Property("CategoryId").CurrentValue);
        Assert.Equal(newCategory.Id, product1Entry.Property("CategoryId").CurrentValue);
        Assert.Equal(newCategory.Id, product2Entry.Property("CategoryId").CurrentValue);
    }

    [ConditionalTheory] // Issues #16546 #25360; Change reverted in #27174.
    [InlineData(null, false, false, true, false, false)]
    [InlineData(null, true, false, true, false, false)]
    [InlineData(null, false, true, true, false, false)]
    [InlineData(null, true, false, false, true, false)]
    [InlineData(null, false, true, false, true, false)]
    [InlineData(null, true, false, true, true, false)]
    [InlineData(null, false, true, true, true, false)]
    [InlineData(CascadeTiming.Immediate, false, false, true, false, false)]
    [InlineData(CascadeTiming.Immediate, true, false, true, false, false)]
    [InlineData(CascadeTiming.Immediate, false, true, true, false, false)]
    [InlineData(CascadeTiming.Immediate, true, false, false, true, false)]
    [InlineData(CascadeTiming.Immediate, false, true, false, true, false)]
    [InlineData(CascadeTiming.Immediate, true, false, true, true, false)]
    [InlineData(CascadeTiming.Immediate, false, true, true, true, false)]
    [InlineData(CascadeTiming.OnSaveChanges, false, false, true, false, false)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, true, false, false)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, true, false, false)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, false, true, false)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, false, true, false)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, true, true, false)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, true, true, false)]
    [InlineData(CascadeTiming.Never, false, false, true, false, false)]
    [InlineData(CascadeTiming.Never, true, false, true, false, false)]
    [InlineData(CascadeTiming.Never, false, true, true, false, false)]
    [InlineData(CascadeTiming.Never, true, false, false, true, false)]
    [InlineData(CascadeTiming.Never, false, true, false, true, false)]
    [InlineData(CascadeTiming.Never, true, false, true, true, false)]
    [InlineData(CascadeTiming.Never, false, true, true, true, false)]
    [InlineData(null, false, false, true, false, true)]
    [InlineData(null, true, false, true, false, true)]
    [InlineData(null, false, true, true, false, true)]
    [InlineData(null, true, false, false, true, true)]
    [InlineData(null, false, true, false, true, true)]
    [InlineData(null, true, false, true, true, true)]
    [InlineData(null, false, true, true, true, true)]
    [InlineData(CascadeTiming.Immediate, false, false, true, false, true)]
    [InlineData(CascadeTiming.Immediate, true, false, true, false, true)]
    [InlineData(CascadeTiming.Immediate, false, true, true, false, true)]
    [InlineData(CascadeTiming.Immediate, true, false, false, true, true)]
    [InlineData(CascadeTiming.Immediate, false, true, false, true, true)]
    [InlineData(CascadeTiming.Immediate, true, false, true, true, true)]
    [InlineData(CascadeTiming.Immediate, false, true, true, true, true)]
    [InlineData(CascadeTiming.OnSaveChanges, false, false, true, false, true)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, true, false, true)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, true, false, true)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, false, true, true)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, false, true, true)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, true, true, true)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, true, true, true)]
    [InlineData(CascadeTiming.Never, false, false, true, false, true)]
    [InlineData(CascadeTiming.Never, true, false, true, false, true)]
    [InlineData(CascadeTiming.Never, false, true, true, false, true)]
    [InlineData(CascadeTiming.Never, true, false, false, true, true)]
    [InlineData(CascadeTiming.Never, false, true, false, true, true)]
    [InlineData(CascadeTiming.Never, true, false, true, true, true)]
    [InlineData(CascadeTiming.Never, false, true, true, true, true)]
    public void Optional_relationship_with_cascade_does_not_delete_orphans(
        CascadeTiming? orphanTiming,
        bool setProperty,
        bool setCurrentValue,
        bool useForeignKey,
        bool useNavigation,
        bool forceCascade)
    {
        Kontainer detachedContainer;
        using (var context = new KontainerContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new Kontainer
                {
                    Name = "C1",
                    Rooms = { new KontainerRoom { Number = 1, Troduct = new Troduct { Description = "Heavy Engine XT3" } } }
                }
            );

            context.SaveChanges();

            detachedContainer = context.Set<Kontainer>()
                .Include(container => container.Rooms)
                .ThenInclude(room => room.Troduct)
                .AsNoTracking()
                .Single();
        }

        using (var context = new KontainerContext())
        {
            var attachedContainer = context.Set<Kontainer>()
                .Include(container => container.Rooms)
                .ThenInclude(room => room.Troduct)
                .Single();

            var attachedRoom = attachedContainer.Rooms.Single();
            var attachedTroduct = attachedRoom.Troduct;

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedRoom).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);

            if (orphanTiming != null)
            {
                context.ChangeTracker.DeleteOrphansTiming = orphanTiming.Value;
            }

            if (setProperty)
            {
                if (useForeignKey)
                {
                    attachedRoom.TroductId = null;
                }

                if (useNavigation)
                {
                    attachedRoom.Troduct = null;
                }
            }
            else if (setCurrentValue)
            {
                if (useForeignKey)
                {
                    context.Entry(attachedRoom).Property(e => e.TroductId).CurrentValue = null;
                }

                if (useNavigation)
                {
                    context.Entry(attachedRoom).Reference(e => e.Troduct).CurrentValue = null;
                }
            }
            else
            {
                var detachedRoom = detachedContainer.Rooms.Single();
                detachedRoom.TroductId = null;
                context.Entry(attachedRoom).CurrentValues.SetValues(detachedRoom);
            }

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);
            Assert.Equal(EntityState.Modified, context.Entry(attachedRoom).State);

            if (forceCascade)
            {
                context.ChangeTracker.CascadeChanges();
            }

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);
            Assert.Equal(EntityState.Modified, context.Entry(attachedRoom).State);

            context.SaveChanges();

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedRoom).State);
        }
    }

    [ConditionalTheory] // Issues #16546 #25360; Change reverted in #27174.
    [InlineData(null, false, false, true, false, false)]
    [InlineData(null, true, false, true, false, false)]
    [InlineData(null, false, true, true, false, false)]
    [InlineData(null, true, false, false, true, false)]
    [InlineData(null, false, true, false, true, false)]
    [InlineData(null, true, false, true, true, false)]
    [InlineData(null, false, true, true, true, false)]
    [InlineData(CascadeTiming.Immediate, false, false, true, false, false)]
    [InlineData(CascadeTiming.Immediate, true, false, true, false, false)]
    [InlineData(CascadeTiming.Immediate, false, true, true, false, false)]
    [InlineData(CascadeTiming.Immediate, true, false, false, true, false)]
    [InlineData(CascadeTiming.Immediate, false, true, false, true, false)]
    [InlineData(CascadeTiming.Immediate, true, false, true, true, false)]
    [InlineData(CascadeTiming.Immediate, false, true, true, true, false)]
    [InlineData(CascadeTiming.OnSaveChanges, false, false, true, false, false)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, true, false, false)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, true, false, false)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, false, true, false)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, false, true, false)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, true, true, false)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, true, true, false)]
    [InlineData(CascadeTiming.Never, false, false, true, false, false)]
    [InlineData(CascadeTiming.Never, true, false, true, false, false)]
    [InlineData(CascadeTiming.Never, false, true, true, false, false)]
    [InlineData(CascadeTiming.Never, true, false, false, true, false)]
    [InlineData(CascadeTiming.Never, false, true, false, true, false)]
    [InlineData(CascadeTiming.Never, true, false, true, true, false)]
    [InlineData(CascadeTiming.Never, false, true, true, true, false)]
    [InlineData(null, false, false, true, false, true)]
    [InlineData(null, true, false, true, false, true)]
    [InlineData(null, false, true, true, false, true)]
    [InlineData(null, true, false, false, true, true)]
    [InlineData(null, false, true, false, true, true)]
    [InlineData(null, true, false, true, true, true)]
    [InlineData(null, false, true, true, true, true)]
    [InlineData(CascadeTiming.Immediate, false, false, true, false, true)]
    [InlineData(CascadeTiming.Immediate, true, false, true, false, true)]
    [InlineData(CascadeTiming.Immediate, false, true, true, false, true)]
    [InlineData(CascadeTiming.Immediate, true, false, false, true, true)]
    [InlineData(CascadeTiming.Immediate, false, true, false, true, true)]
    [InlineData(CascadeTiming.Immediate, true, false, true, true, true)]
    [InlineData(CascadeTiming.Immediate, false, true, true, true, true)]
    [InlineData(CascadeTiming.OnSaveChanges, false, false, true, false, true)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, true, false, true)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, true, false, true)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, false, true, true)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, false, true, true)]
    [InlineData(CascadeTiming.OnSaveChanges, true, false, true, true, true)]
    [InlineData(CascadeTiming.OnSaveChanges, false, true, true, true, true)]
    [InlineData(CascadeTiming.Never, false, false, true, false, true)]
    [InlineData(CascadeTiming.Never, true, false, true, false, true)]
    [InlineData(CascadeTiming.Never, false, true, true, false, true)]
    [InlineData(CascadeTiming.Never, true, false, false, true, true)]
    [InlineData(CascadeTiming.Never, false, true, false, true, true)]
    [InlineData(CascadeTiming.Never, true, false, true, true, true)]
    [InlineData(CascadeTiming.Never, false, true, true, true, true)]
    public void Optional_relationship_with_cascade_can_be_forced_to_delete_orphans(
        CascadeTiming? orphanTiming,
        bool setProperty,
        bool setCurrentValue,
        bool useForeignKey,
        bool useNavigation,
        bool forceCascade)
    {
        Kontainer detachedContainer;
        using (var context = new KontainerContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Add(
                new Kontainer
                {
                    Name = "C1",
                    Rooms = { new KontainerRoom { Number = 1, Troduct = new Troduct { Description = "Heavy Engine XT3" } } }
                }
            );

            context.SaveChanges();

            detachedContainer = context.Set<Kontainer>()
                .Include(container => container.Rooms)
                .ThenInclude(room => room.Troduct)
                .AsNoTracking()
                .Single();
        }

        using (var context = new KontainerContext())
        {
            var attachedContainer = context.Set<Kontainer>()
                .Include(container => container.Rooms)
                .ThenInclude(room => room.Troduct)
                .Single();

            var attachedRoom = attachedContainer.Rooms.Single();
            var attachedTroduct = attachedRoom.Troduct;

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedRoom).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);

            if (orphanTiming != null)
            {
                context.ChangeTracker.DeleteOrphansTiming = orphanTiming.Value;
            }

            if (setProperty)
            {
                if (useForeignKey)
                {
                    attachedRoom.TroductId = null;
                }

                if (useNavigation)
                {
                    attachedRoom.Troduct = null;
                }
            }
            else if (setCurrentValue)
            {
                if (useForeignKey)
                {
                    context.Entry(attachedRoom).Property(e => e.TroductId).CurrentValue = null;
                }

                if (useNavigation)
                {
                    context.Entry(attachedRoom).Reference(e => e.Troduct).CurrentValue = null;
                }
            }
            else
            {
                var detachedRoom = detachedContainer.Rooms.Single();
                detachedRoom.TroductId = null;
                context.Entry(attachedRoom).CurrentValues.SetValues(detachedRoom);
            }

            context.Entry(attachedRoom).GetInfrastructure()
                .HandleNullForeignKey(context.Entry(attachedRoom).Property(e => e.TroductId).Metadata);

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);

            if (orphanTiming is null or CascadeTiming.Immediate)
            {
                Assert.Equal(EntityState.Deleted, context.Entry(attachedRoom).State);
            }
            else
            {
                Assert.Equal(EntityState.Modified, context.Entry(attachedRoom).State);
            }

            if (forceCascade)
            {
                context.ChangeTracker.CascadeChanges();
            }

            Assert.Equal(3, context.ChangeTracker.Entries().Count());
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
            Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);

            if (orphanTiming == null
                || orphanTiming == CascadeTiming.Immediate
                || forceCascade)
            {
                Assert.Equal(EntityState.Deleted, context.Entry(attachedRoom).State);
            }
            else
            {
                Assert.Equal(EntityState.Modified, context.Entry(attachedRoom).State);
            }

            if (orphanTiming == CascadeTiming.Never
                && !forceCascade)
            {
                Assert.Equal(
                    CoreStrings.RelationshipConceptualNull(nameof(Troduct), nameof(KontainerRoom)),
                    Assert.Throws<InvalidOperationException>(
                        () => context.SaveChanges()).Message);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);
                Assert.Equal(EntityState.Modified, context.Entry(attachedRoom).State);
            }
            else
            {
                context.SaveChanges();
                Assert.Equal(2, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedContainer).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(attachedTroduct!).State);
                Assert.Equal(EntityState.Detached, context.Entry(attachedRoom).State);
            }
        }
    }

    private class Kontainer
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<KontainerRoom> Rooms { get; } = [];
    }

    private class KontainerRoom
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public int KontainerId { get; set; }
        public Kontainer? Kontainer { get; set; }
        public int? TroductId { get; set; }
        public Troduct? Troduct { get; set; }
    }

    private class Troduct
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public List<KontainerRoom> Rooms { get; } = [];
    }

    private class KontainerContext : DbContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<KontainerRoom>()
                .HasOne(room => room.Troduct)
                .WithMany(product => product.Rooms)
                .HasForeignKey(room => room.TroductId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(KontainerContext));
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Adding_derived_owned_throws(bool useAdd)
    {
        using var context = new EarlyLearningCenter();
        var dreams = new Dreams { Sweet = new Sweet { Id = 1 }, Are = new OfThis() };

        context.Entry(dreams.Sweet).State = EntityState.Unchanged;

        if (useAdd)
        {
            Assert.Equal(
                CoreStrings.TrackingTypeMismatch(nameof(OfThis), "Dreams.Are#AreMade"),
                Assert.Throws<InvalidOperationException>(() => context.Add(dreams)).Message);
        }
        else
        {
            Assert.Equal(
                CoreStrings.TrackingTypeMismatch(nameof(OfThis), "Dreams.Are#AreMade"),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        context.ChangeTracker.TrackGraph(
                            dreams, e =>
                            {
                                e.Entry.State = e.Entry.IsKeySet && !e.Entry.Metadata.IsOwned()
                                    ? EntityState.Unchanged
                                    : EntityState.Added;
                            })).Message);
        }
    }

    [ConditionalFact]
    public void Moving_derived_owned_to_non_derived_reference_throws()
    {
        using var context = new EarlyLearningCenter();
        var dreams = new Dreams { Sweet = new Sweet { Id = 1 }, OfThis = new OfThis() };

        context.Entry(dreams.Sweet).State = EntityState.Unchanged;
        context.Add(dreams);

        dreams.Are = dreams.OfThis;
        dreams.OfThis = null;

        Assert.Equal(
            CoreStrings.TrackingTypeMismatch(nameof(OfThis), "Dreams.Are#AreMade"),
            Assert.Throws<InvalidOperationException>(() => context.Entry(dreams)).Message);
    }

    [ConditionalFact] // Issue #1207
    public void Can_add_principal_and_then_identifying_dependents_with_key_generation()
    {
        using var context = new EarlyLearningCenter();
        var product1 = new Product
        {
            Details = new ProductDetails { Tag = new ProductDetailsTag { TagDetails = new ProductDetailsTagDetails() } }
        };
        var product2 = new Product
        {
            Details = new ProductDetails { Tag = new ProductDetailsTag { TagDetails = new ProductDetailsTagDetails() } }
        };

        context.Add(product1);
        context.Add(product1.Details);
        context.Add(product1.Details.Tag);
        context.Add(product1.Details.Tag.TagDetails);
        context.Add(product2);
        context.Add(product2.Details);
        context.Add(product2.Details.Tag);
        context.Add(product2.Details.Tag.TagDetails);

        AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
    }

    [ConditionalFact] // Issue #1207
    public void Can_add_identifying_dependents_and_then_principal_with_key_generation()
    {
        using var context = new EarlyLearningCenter();
        var tagDetails1 = new ProductDetailsTagDetails
        {
            Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
        };

        var tagDetails2 = new ProductDetailsTagDetails
        {
            Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
        };

        context.Add(tagDetails1);
        context.Add(tagDetails1.Tag);
        context.Add(tagDetails1.Tag.Details);
        context.Add(tagDetails1.Tag.Details.Product);
        context.Add(tagDetails2);
        context.Add(tagDetails2.Tag);
        context.Add(tagDetails2.Tag.Details);
        context.Add(tagDetails2.Tag.Details.Product);

        AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
    }

    [ConditionalFact] // Issue #1207
    public void Can_add_identifying_dependents_and_then_principal_interleaved_with_key_generation()
    {
        using var context = new EarlyLearningCenter();
        var tagDetails1 = new ProductDetailsTagDetails
        {
            Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
        };

        var tagDetails2 = new ProductDetailsTagDetails
        {
            Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
        };

        context.Add(tagDetails1);
        context.Add(tagDetails2);
        context.Add(tagDetails1.Tag);
        context.Add(tagDetails2.Tag);
        context.Add(tagDetails2.Tag.Details);
        context.Add(tagDetails1.Tag.Details);
        context.Add(tagDetails1.Tag.Details.Product);
        context.Add(tagDetails2.Tag.Details.Product);

        AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
    }

    [ConditionalFact] // Issue #1207
    public void Can_add_identifying_dependents_and_principal_starting_in_the_middle_with_key_generation()
    {
        using var context = new EarlyLearningCenter();
        var tagDetails1 = new ProductDetailsTagDetails
        {
            Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
        };

        var tagDetails2 = new ProductDetailsTagDetails
        {
            Tag = new ProductDetailsTag { Details = new ProductDetails { Product = new Product() } }
        };

        context.Add(tagDetails1.Tag);
        context.Add(tagDetails2.Tag);
        context.Add(tagDetails1);
        context.Add(tagDetails2);
        context.Add(tagDetails2.Tag.Details);
        context.Add(tagDetails1.Tag.Details);
        context.Add(tagDetails1.Tag.Details.Product);
        context.Add(tagDetails2.Tag.Details.Product);

        AssertProductAndDetailsFixedUp(context, tagDetails1, tagDetails2);
    }

    [ConditionalFact] // Issue #1207
    public void Can_add_principal_and_identifying_dependents_starting_in_the_middle_with_key_generation()
    {
        using var context = new EarlyLearningCenter();
        var product1 = new Product
        {
            Details = new ProductDetails { Tag = new ProductDetailsTag { TagDetails = new ProductDetailsTagDetails() } }
        };
        var product2 = new Product
        {
            Details = new ProductDetails { Tag = new ProductDetailsTag { TagDetails = new ProductDetailsTagDetails() } }
        };

        context.Add(product1.Details);
        context.Add(product2.Details);
        context.Add(product1.Details.Tag.TagDetails);
        context.Add(product1);
        context.Add(product1.Details.Tag);
        context.Add(product2.Details.Tag);
        context.Add(product2.Details.Tag.TagDetails);
        context.Add(product2);

        AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
    }

    [ConditionalTheory] // Issue #1207
    [InlineData(false)]
    [InlineData(true)]
    public void Can_add_identifying_dependents_and_principal_with_post_nav_fixup_with_key_generation(bool callDetectChangesTwice)
    {
        using var context = new EarlyLearningCenter();
        var product1 = new Product();
        var details1 = new ProductDetails();
        var tag1 = new ProductDetailsTag();
        var tagDetails1 = new ProductDetailsTagDetails();

        var product2 = new Product();
        var details2 = new ProductDetails();
        var tag2 = new ProductDetailsTag();
        var tagDetails2 = new ProductDetailsTagDetails();

        context.Add(product1);
        context.Add(tagDetails2);
        context.Add(details1);
        context.Add(tag2);
        context.Add(details2);
        context.Add(tag1);
        context.Add(tagDetails1);
        context.Add(product2);

        product1.Details = details1;
        details1.Tag = tag1;
        tag1.TagDetails = tagDetails1;

        product2.Details = details2;
        details2.Tag = tag2;
        tag2.TagDetails = tagDetails2;

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        AssertProductAndDetailsFixedUp(context, product1.Details.Tag.TagDetails, product2.Details.Tag.TagDetails);
    }

    [ConditionalTheory] // Issue #1207
    [InlineData(false)]
    [InlineData(true)]
    public void Can_add_identifying_dependents_and_principal_with_reverse_post_nav_fixup_with_key_generation(
        bool callDetectChangesTwice)
    {
        using var context = new EarlyLearningCenter();
        var product1 = new Product();
        var details1 = new ProductDetails();
        var tag1 = new ProductDetailsTag();
        var tagDetails1 = new ProductDetailsTagDetails();

        var product2 = new Product();
        var details2 = new ProductDetails();
        var tag2 = new ProductDetailsTag();
        var tagDetails2 = new ProductDetailsTagDetails();

        context.Add(product1);
        context.Add(tagDetails2);
        context.Add(details1);
        context.Add(tag2);
        context.Add(details2);
        context.Add(tag1);
        context.Add(tagDetails1);
        context.Add(product2);

        tagDetails1.Tag = tag1;
        tag1.Details = details1;
        details1.Product = product1;

        tagDetails2.Tag = tag2;
        tag2.Details = details2;
        details2.Product = product2;

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        AssertProductAndDetailsFixedUp(context, product1.Details!.Tag.TagDetails, product2.Details!.Tag.TagDetails);
    }

    private static void AssertProductAndDetailsFixedUp(
        DbContext context,
        ProductDetailsTagDetails tagDetails1,
        ProductDetailsTagDetails tagDetails2)
    {
        Assert.Equal(8, context.ChangeTracker.Entries().Count());

        Assert.Equal(EntityState.Added, context.Entry(tagDetails1).State);
        Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag!).State);
        Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag!.Details).State);
        Assert.Equal(EntityState.Added, context.Entry(tagDetails1.Tag.Details.Product).State);

        Assert.Equal(EntityState.Added, context.Entry(tagDetails2).State);
        Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag!).State);
        Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag!.Details).State);
        Assert.Equal(EntityState.Added, context.Entry(tagDetails2.Tag.Details.Product).State);

        Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Id);
        Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Details.Id);
        Assert.Equal(tagDetails1.Id, tagDetails1.Tag.Details.Product.Id);
        Assert.True(tagDetails1.Id > 0);

        Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Id);
        Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Details.Id);
        Assert.Equal(tagDetails2.Id, tagDetails2.Tag.Details.Product.Id);
        Assert.True(tagDetails2.Id > 0);

        Assert.Same(tagDetails1, tagDetails1.Tag.TagDetails);
        Assert.Same(tagDetails1.Tag, tagDetails1.Tag.Details.Tag);
        Assert.Same(tagDetails1.Tag.Details, tagDetails1.Tag.Details.Product.Details);

        Assert.Same(tagDetails2, tagDetails2.Tag.TagDetails);
        Assert.Same(tagDetails2.Tag, tagDetails2.Tag.Details.Tag);
        Assert.Same(tagDetails2.Tag.Details, tagDetails2.Tag.Details.Product.Details);

        var product1 = tagDetails1.Tag.Details.Product;
        Assert.Same(product1, product1.Details!.Product);
        Assert.Same(product1.Details, product1.Details.Tag.Details);
        Assert.Same(product1.Details.Tag, product1.Details.Tag.TagDetails.Tag);

        var product2 = tagDetails2.Tag.Details.Product;
        Assert.Same(product2, product2.Details!.Product);
        Assert.Same(product2.Details, product2.Details.Tag.Details);
        Assert.Same(product2.Details.Tag, product2.Details.Tag.TagDetails.Tag);
    }

    [ConditionalFact] // Issue #1207
    public void Can_add_identifying_one_to_many_via_principal_with_key_generation()
    {
        using var context = new EarlyLearningCenter();
        var product1 = new Product();
        var product2 = new Product();

        var order1 = new Order();
        var order2 = new Order();

        var orderDetails1a = new OrderDetails { Order = order1, Product = product1 };
        var orderDetails1b = new OrderDetails { Order = order1, Product = product2 };
        var orderDetails2a = new OrderDetails { Order = order2, Product = product1 };
        var orderDetails2b = new OrderDetails { Order = order2, Product = product2 };

        context.Add(product1);
        context.Add(order1);
        context.Add(orderDetails1a);
        context.Add(orderDetails1b);
        context.Add(product2);
        context.Add(order2);
        context.Add(orderDetails2a);
        context.Add(orderDetails2b);

        AssertOrderAndDetailsFixedUp(context, orderDetails1a, orderDetails1b, orderDetails2a, orderDetails2b);
    }

    [ConditionalFact] // Issue #1207
    public void Can_add_identifying_one_to_many_via_dependents_with_key_generation()
    {
        using var context = new EarlyLearningCenter();
        var product1 = new Product();
        var product2 = new Product();

        var order1 = new Order();
        var order2 = new Order();

        var orderDetails1a = new OrderDetails { Order = order1, Product = product1 };
        var orderDetails1b = new OrderDetails { Order = order1, Product = product2 };
        var orderDetails2a = new OrderDetails { Order = order2, Product = product1 };
        var orderDetails2b = new OrderDetails { Order = order2, Product = product2 };

        context.Add(orderDetails1a);
        context.Add(orderDetails2a);
        context.Add(orderDetails1b);
        context.Add(orderDetails2b);
        context.Add(order1);
        context.Add(product1);
        context.Add(order2);
        context.Add(product2);

        AssertOrderAndDetailsFixedUp(context, orderDetails1a, orderDetails1b, orderDetails2a, orderDetails2b);
    }

    private static void AssertOrderAndDetailsFixedUp(
        DbContext context,
        OrderDetails orderDetails1a,
        OrderDetails orderDetails1b,
        OrderDetails orderDetails2a,
        OrderDetails orderDetails2b)
    {
        Assert.Equal(8, context.ChangeTracker.Entries().Count());

        Assert.Equal(EntityState.Added, context.Entry(orderDetails1a).State);
        Assert.Equal(EntityState.Added, context.Entry(orderDetails1b).State);
        Assert.Equal(EntityState.Added, context.Entry(orderDetails1a.Order).State);
        Assert.Equal(EntityState.Added, context.Entry(orderDetails1b.Product).State);

        Assert.Equal(EntityState.Added, context.Entry(orderDetails2a).State);
        Assert.Equal(EntityState.Added, context.Entry(orderDetails2b).State);
        Assert.Equal(EntityState.Added, context.Entry(orderDetails2a.Order).State);
        Assert.Equal(EntityState.Added, context.Entry(orderDetails2b.Product).State);

        Assert.Equal(orderDetails1a.OrderId, orderDetails1a.Order.Id);
        Assert.Equal(orderDetails1b.OrderId, orderDetails1b.Order.Id);
        Assert.Equal(orderDetails1a.ProductId, orderDetails1a.Product.Id);
        Assert.Equal(orderDetails1b.ProductId, orderDetails1b.Product.Id);
        Assert.True(orderDetails1a.OrderId > 0);
        Assert.True(orderDetails1b.OrderId > 0);
        Assert.True(orderDetails1a.ProductId > 0);
        Assert.True(orderDetails1b.ProductId > 0);

        Assert.Equal(orderDetails2a.OrderId, orderDetails2a.Order.Id);
        Assert.Equal(orderDetails2b.OrderId, orderDetails2b.Order.Id);
        Assert.Equal(orderDetails2a.ProductId, orderDetails2a.Product.Id);
        Assert.Equal(orderDetails2b.ProductId, orderDetails2b.Product.Id);
        Assert.True(orderDetails2a.OrderId > 0);
        Assert.True(orderDetails2b.OrderId > 0);
        Assert.True(orderDetails2a.ProductId > 0);
        Assert.True(orderDetails2b.ProductId > 0);

        Assert.Same(orderDetails1a.Order, orderDetails1b.Order);
        Assert.Same(orderDetails2a.Order, orderDetails2b.Order);

        Assert.Same(orderDetails1a.Product, orderDetails2a.Product);
        Assert.Same(orderDetails1b.Product, orderDetails2b.Product);

        Assert.Equal(2, orderDetails1a.Order.OrderDetails.Count);
        Assert.Equal(2, orderDetails2a.Order.OrderDetails.Count);

        Assert.Contains(orderDetails1a, orderDetails1a.Order.OrderDetails);
        Assert.Contains(orderDetails1b, orderDetails1a.Order.OrderDetails);
        Assert.Contains(orderDetails2a, orderDetails2a.Order.OrderDetails);
        Assert.Contains(orderDetails2b, orderDetails2a.Order.OrderDetails);

        Assert.Equal(2, orderDetails1a.Product.OrderDetails.Count);
        Assert.Equal(2, orderDetails1b.Product.OrderDetails.Count);

        Assert.Contains(orderDetails1a, orderDetails1a.Product.OrderDetails);
        Assert.Contains(orderDetails2a, orderDetails1a.Product.OrderDetails);
        Assert.Contains(orderDetails1b, orderDetails1b.Product.OrderDetails);
        Assert.Contains(orderDetails2b, orderDetails1b.Product.OrderDetails);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Entries_calls_DetectChanges_by_default(bool useGenericOverload)
    {
        using var context = new EarlyLearningCenter();
        var entry = context.Attach(
            new Product { Id = 1, CategoryId = 66 });

        entry.Entity.CategoryId = 77;

        Assert.Equal(EntityState.Unchanged, entry.State);

        if (useGenericOverload)
        {
            context.ChangeTracker.Entries<Product>();
        }
        else
        {
            context.ChangeTracker.Entries();
        }

        Assert.Equal(EntityState.Modified, entry.State);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Auto_DetectChanges_for_Entries_can_be_switched_off(bool useGenericOverload)
    {
        using var context = new EarlyLearningCenter();
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var entry = context.Attach(
            new Product { Id = 1, CategoryId = 66 });

        entry.Entity.CategoryId = 77;

        Assert.Equal(EntityState.Unchanged, entry.State);

        if (useGenericOverload)
        {
            context.ChangeTracker.Entries<Product>();
        }
        else
        {
            context.ChangeTracker.Entries();
        }

        Assert.Equal(EntityState.Unchanged, entry.State);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicitly_calling_DetectChanges_works_even_if_auto_DetectChanges_is_switched_off(bool callDetectChangesTwice)
    {
        using var context = new EarlyLearningCenter();
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var entry = context.Attach(
            new Product { Id = 1, CategoryId = 66 });

        entry.Entity.CategoryId = 77;

        Assert.Equal(EntityState.Unchanged, entry.State);

        context.ChangeTracker.DetectChanges();

        if (callDetectChangesTwice)
        {
            context.ChangeTracker.DetectChanges();
        }

        Assert.Equal(EntityState.Modified, entry.State);
    }

    [ConditionalFact]
    public void Does_not_throw_when_instance_of_unmapped_derived_type_is_used()
    {
        using var context = new EarlyLearningCenter();
        Assert.Same(
            context.Model.FindEntityType(typeof(Product)),
            context.Add(new SpecialProduct()).Metadata);
    }

    [ConditionalFact]
    public void Shadow_properties_are_not_included_in_update_unless_value_explicitly_set()
    {
        int id;

        using (var context = new TheShadows())
        {
            var entry = context.Add(new Dark());

            Assert.NotEqual(0, id = entry.Property<int>("Id").CurrentValue);
            Assert.Equal(0, entry.Property<int>("SomeInt").CurrentValue);
            Assert.Null(entry.Property<string>("SomeString").CurrentValue);

            entry.Property<int>("SomeInt").CurrentValue = 77;
            entry.Property<string>("SomeString").CurrentValue = "Morden";

            context.SaveChanges();
        }

        AssertValuesSaved(id, 77, "Morden");

        using (var context = new TheShadows())
        {
            var entry = context.Entry(new Dark());
            entry.Property<int>("Id").CurrentValue = id;
            entry.State = EntityState.Modified;

            context.SaveChanges();
        }

        AssertValuesSaved(id, 77, "Morden");

        using (var context = new TheShadows())
        {
            var entry = context.Entry(new Dark());
            entry.Property<int>("Id").CurrentValue = id;
            entry.Property<int>("SomeInt").CurrentValue = 78;
            entry.Property<string>("SomeString").CurrentValue = "Mr";
            entry.State = EntityState.Modified;

            context.SaveChanges();
        }

        AssertValuesSaved(id, 78, "Mr");

        using (var context = new TheShadows())
        {
            var entry = context.Entry(new Dark());
            entry.Property<int>("Id").CurrentValue = id;
            entry.State = EntityState.Modified;
            entry.Property<int>("SomeInt").CurrentValue = 0;
            entry.Property<string?>("SomeString").CurrentValue = null;

            context.SaveChanges();
        }

        AssertValuesSaved(id, 0, null);
    }

    [ConditionalFact]
    public void Clearing_change_tracker_resets_local_view_count()
    {
        using var context = new LikeAZooContext();

        var originalCount = context.Cats.Local.Count;
        context.Cats.Add(new Cat(3));

        context.ChangeTracker.Clear();

        Assert.Equal(originalCount, context.Cats.Local.Count);
    }

    [ConditionalFact] // Issue #26448
    public void Stable_generated_values_do_not_force_Added_state()
    {
        using var context = new EarlyLearningCenter();

        Assert.Equal(EntityState.Added, context.Add(new Stable()).State);

        context.ChangeTracker.Clear();
        Assert.Equal(EntityState.Modified, context.Update(new Stable()).State);

        context.ChangeTracker.Clear();
        Assert.Equal(EntityState.Unchanged, context.Attach(new Stable()).State);

        context.ChangeTracker.Clear();
        Assert.Equal(EntityState.Deleted, context.Remove(new Stable()).State);
    }

    private static void AssertValuesSaved(int id, int someInt, string? someString)
    {
        using var context = new TheShadows();
        var entry = context.Entry(context.Set<Dark>().Single(e => EF.Property<int>(e, "Id") == id));

        Assert.Equal(id, entry.Property<int>("Id").CurrentValue);
        Assert.Equal(someInt, entry.Property<int>("SomeInt").CurrentValue);
        Assert.Equal(someString, entry.Property<string>("SomeString").CurrentValue);
    }

    private class TheShadows : DbContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Dark>(
                b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd();
                    b.Property<int>("SomeInt");
                    b.Property<string>("SomeString");
                });

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(TheShadows));
    }

    private class Dark;

    private class Category
    {
        public int Id { get; set; }

        public List<Product> Products { get; } = [];
    }

    private class Product
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ProductDetails? Details { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Local
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public List<OrderDetails> OrderDetails { get; } = [];
    }

    private class OptionalCategory
    {
        public int Id { get; set; }

        public List<OptionalProduct> Products { get; } = [];
    }

    private class OptionalProduct
    {
        public int Id { get; set; }

        public int? CategoryId { get; set; }
        public OptionalCategory? Category { get; set; }
    }

    private class SpecialProduct : Product;

    private class ProductDetails
    {
        public int Id { get; set; }

        public Product Product { get; set; } = null!;

        public ProductDetailsTag Tag { get; set; } = null!;
    }

    private class ProductDetailsTag
    {
        public int Id { get; set; }

        public ProductDetails Details { get; set; } = null!;

        public ProductDetailsTagDetails TagDetails { get; set; } = null!;
    }

    private class ProductDetailsTagDetails
    {
        public int Id { get; set; }

        public ProductDetailsTag? Tag { get; set; }
    }

    private class Order
    {
        public int Id { get; set; }

        // ReSharper disable once CollectionNeverUpdated.Local
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public List<OrderDetails> OrderDetails { get; } = [];
    }

    private class OrderDetails
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }

    private class Sweet
    {
        public int? Id { get; set; }
        public Dreams? Dreams { get; set; }
    }

    private class Dreams
    {
        public Sweet? Sweet { get; set; }
        public AreMade? Are { get; set; }
        public AreMade? Made { get; set; }
        public OfThis? OfThis { get; set; }
    }

    private class AreMade;

    private class OfThis : AreMade;

    private class WhoAmI
    {
        public string? ToDisagree { get; set; }
    }

    private class PrincipalGG
    {
        public int Id { get; set; }
        public DependentGG? DependentGG { get; set; }
    }

    private class DependentGG
    {
        public int Id { get; set; }
        public PrincipalGG? PrincipalGG { get; set; }
    }

    private class PrincipalNN
    {
        public int Id { get; set; }
        public DependentNN? DependentNN { get; set; }
    }

    private class DependentNN
    {
        public int Id { get; set; }
        public PrincipalNN? PrincipalNN { get; set; }
    }

    private class PrincipalNG
    {
        public int Id { get; set; }
        public DependentNG? DependentNG { get; set; }
    }

    private class DependentNG
    {
        public int Id { get; set; }
        public PrincipalNG? PrincipalNG { get; set; }
    }

    private class PrincipalGN
    {
        public int Id { get; set; }
        public DependentGN? DependentGN { get; set; }
    }

    private class DependentGN
    {
        public int Id { get; set; }
        public PrincipalGN? PrincipalGN { get; set; }
    }

    public class Bobby
    {
        public int Id { get; set; }
        public Buggy? Buggy { get; set; }
    }

    public class Buggy
    {
        public int Id { get; set; }
    }

    private class Stable
    {
        public Guid Id { get; set; }
    }

    private class TenantIdGenerator : ValueGenerator<Guid>
    {
        public override Guid Next(EntityEntry entry)
            => Guid.Parse("98D06A82-C691-4988-EA39-08D98E2C8D8F");

        public override bool GeneratesTemporaryValues
            => false;

        public override bool GeneratesStableValues
            => true;
    }

    private class EarlyLearningCenter(params IInterceptor[] interceptors) : DbContext
    {
        private readonly IInterceptor[] _interceptors = interceptors;
        private readonly IServiceProvider _serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sweet>().OwnsOne(
                e => e.Dreams, b =>
                {
                    b.WithOwner(e => e.Sweet);
                    b.OwnsOne(e => e.Are);
                    b.OwnsOne(e => e.Made);
                    b.OwnsOne(e => e.OfThis);
                });

            modelBuilder.Entity<WhoAmI>().HasNoKey();

            modelBuilder
                .Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);

            modelBuilder
                .Entity<ProductDetailsTag>().HasOne(e => e.TagDetails).WithOne(e => e.Tag)
                .HasForeignKey<ProductDetailsTagDetails>(e => e.Id);

            modelBuilder
                .Entity<ProductDetails>().HasOne(e => e.Tag).WithOne(e => e.Details)
                .HasForeignKey<ProductDetailsTag>(e => e.Id);

            modelBuilder
                .Entity<Product>().HasOne(e => e.Details).WithOne(e => e.Product)
                .HasForeignKey<ProductDetails>(e => e.Id);

            modelBuilder.Entity<OrderDetails>(
                b =>
                {
                    b.HasKey(
                        e => new { e.OrderId, e.ProductId });
                    b.HasOne(e => e.Order).WithMany(e => e.OrderDetails).HasForeignKey(e => e.OrderId);
                    b.HasOne(e => e.Product).WithMany(e => e.OrderDetails).HasForeignKey(e => e.ProductId);
                });

            modelBuilder.Entity<OptionalProduct>();

            modelBuilder.Entity<PrincipalNN>(
                b =>
                {
                    b.HasOne(e => e.DependentNN)
                        .WithOne(e => e.PrincipalNN)
                        .HasForeignKey<DependentNN>(e => e.Id);

                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<DependentNN>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<PrincipalGG>(
                b =>
                {
                    b.HasOne(e => e.DependentGG)
                        .WithOne(e => e.PrincipalGG)
                        .HasForeignKey<DependentGG>(e => e.Id);

                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<DependentGG>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<PrincipalNG>(
                b =>
                {
                    b.HasOne(e => e.DependentNG)
                        .WithOne(e => e.PrincipalNG)
                        .HasForeignKey<DependentNG>(e => e.Id);

                    b.Property(e => e.Id).ValueGeneratedNever();
                });

            modelBuilder.Entity<DependentNG>().Property(e => e.Id).HasValueGenerator<DummyValueGenerator>();

            modelBuilder.Entity<PrincipalGN>(
                b =>
                {
                    b.HasOne(e => e.DependentGN)
                        .WithOne(e => e.PrincipalGN)
                        .HasForeignKey<DependentGN>(e => e.Id);

                    b.Property(e => e.Id).ValueGeneratedOnAdd();
                });

            modelBuilder.Entity<DependentGN>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<Buggy>(
                entity =>
                {
                    entity.Property<int?>("BobbyId");
                    entity.HasOne<Bobby>()
                        .WithOne(p => p.Buggy)
                        .HasForeignKey<Buggy>("BobbyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity<Stable>()
                .Property(e => e.Id)
                .HasValueGenerator<TenantIdGenerator>();
        }

        private class DummyValueGenerator : ValueGenerator<int>
        {
            private static int _value;

            public override int Next(EntityEntry entry)
                => _value++;

            public override bool GeneratesTemporaryValues
                => false;
        }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .AddInterceptors(_interceptors)
                .UseInternalServiceProvider(_serviceProvider)
                .UseInMemoryDatabase(nameof(EarlyLearningCenter));
    }
}
