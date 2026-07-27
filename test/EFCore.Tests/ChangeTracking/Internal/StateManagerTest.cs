// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class StateManagerTest
{
    [Fact]
    public void Can_get_existing_entry_if_entity_is_already_tracked_otherwise_new_entry()
    {
        var stateManager = CreateStateManager(BuildModel());
        var category = new Category { Id = 1, PrincipalId = 777 };

        var entry = stateManager.GetOrCreateEntry(category);

        stateManager.StartTracking(entry);

        var entry2 = stateManager.GetOrCreateEntry(category);

        Assert.Same(entry, entry2);
        Assert.Equal(EntityState.Detached, entry.EntityState);
    }

    [Fact]
    public void Identity_conflict_throws_for_primary_key()
    {
        using var context = new IdentityConflictContext();
        context.Attach(
            new SingleKey { Id = 77, AlternateId = 66 });

        Assert.Equal(
            CoreStrings.IdentityConflict("SingleKey", "{'Id'}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 77, AlternateId = 67 })).Message);
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Identity_conflict_can_be_resolved(bool copy)
    {
        using var context = new IdentityConflictContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor()
                : new IgnoringIdentityResolutionInterceptor());

        var entity = new SingleKey
        {
            Id = 77,
            AlternateId = 66,
            Value = "Existing"
        };
        context.Attach(entity);
        context.Attach(
            new SingleKey
            {
                Id = 77,
                AlternateId = 66,
                Value = "New"
            });

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(entity).State);
        Assert.Equal(copy ? "New" : "Existing", entity.Value);
    }

    [Fact]
    public void Resolving_identity_conflict_for_primary_key_cannot_change_alternate_key()
    {
        using var context = new IdentityConflictContext(new NaiveCopyingIdentityResolutionInterceptor());

        context.Attach(new SingleKey { Id = 77, AlternateId = 66 });

        Assert.Equal(
            CoreStrings.KeyReadOnly(nameof(SingleKey.AlternateId), nameof(SingleKey)),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 77, AlternateId = 67 })).Message);
    }

    [Fact]
    public void Resolving_identity_conflict_for_primary_key_throws_if_alternate_key_changes()
    {
        using var context = new IdentityConflictContext(new IgnoringIdentityResolutionInterceptor());

        context.Attach(new SingleKey { Id = 77, AlternateId = 66 });

        Assert.Equal(
            CoreStrings.IdentityConflict("SingleKey", "{'Id'}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 77, AlternateId = 67 })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_alternate_key()
    {
        using var context = new IdentityConflictContext();
        context.Attach(
            new SingleKey { Id = 77, AlternateId = 66 });

        Assert.Equal(
            CoreStrings.IdentityConflict("SingleKey", "{'AlternateId'}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 78, AlternateId = 66 })).Message);
    }

    [Fact]
    public void Resolving_identity_conflict_for_alternate_key_cannot_change_primary_key()
    {
        using var context = new IdentityConflictContext(new NaiveCopyingIdentityResolutionInterceptor());

        context.Attach(new SingleKey { Id = 77, AlternateId = 66 });
        Assert.Equal(
            CoreStrings.KeyReadOnly(nameof(SingleKey.Id), nameof(SingleKey)),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 78, AlternateId = 66 })).Message);
    }

    private class NaiveCopyingIdentityResolutionInterceptor : IIdentityResolutionInterceptor
    {
        public void UpdateTrackedInstance(IdentityResolutionInterceptionData interceptionData, EntityEntry existingEntry, object newEntity)
            => existingEntry.CurrentValues.SetValues(newEntity);
    }

    [Fact]
    public void Resolving_identity_conflict_for_alternate_key_throws_if_primary_key_changes()
    {
        using var context = new IdentityConflictContext(new IgnoringIdentityResolutionInterceptor());

        context.Attach(new SingleKey { Id = 77, AlternateId = 66 });
        Assert.Equal(
            CoreStrings.IdentityConflict("SingleKey", "{'AlternateId'}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 78, AlternateId = 66 })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_owned_primary_key()
    {
        using var context = new IdentityConflictContext();
        context.Attach(
            new SingleKey
            {
                Id = 77,
                AlternateId = 66,
                Owned = new SingleKeyOwned()
            });

        var duplicateOwned = new SingleKeyOwned();
        context.Entry(duplicateOwned).Property("SingleKeyId").CurrentValue = 77;

        Assert.Equal(
            CoreStrings.IdentityConflictOwned("SingleKeyOwned", "{'SingleKeyId'}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey
                {
                    Id = 78,
                    AlternateId = 67,
                    Owned = duplicateOwned
                })).Message);
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Identity_conflict_can_be_resolved_for_owned(bool copy)
    {
        using var context = new IdentityConflictContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor()
                : new IgnoringIdentityResolutionInterceptor());

        var owned = new SingleKeyOwned { Value = "Existing" };
        context.Attach(
            new SingleKey
            {
                Id = 77,
                AlternateId = 66,
                Owned = owned
            });

        var duplicateOwned = new SingleKeyOwned { Value = "New" };
        context.Entry(duplicateOwned).Property("SingleKeyId").CurrentValue = 77;

        context.Attach(
            new SingleKey
            {
                Id = 78,
                AlternateId = 67,
                Owned = duplicateOwned
            });

        Assert.Equal(3, context.ChangeTracker.Entries().Count());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(owned).State);
        Assert.Equal(copy ? "New" : "Existing", owned.Value);
    }

    [Fact]
    public void Identity_conflict_throws_for_composite_primary_key()
    {
        using var context = new IdentityConflictContext();
        context.Attach(
            new CompositeKey
            {
                Id1 = 77,
                Id2 = 78,
                AlternateId1 = 66,
                AlternateId2 = 67
            });

        Assert.Equal(
            CoreStrings.IdentityConflict("CompositeKey", "{'Id1', 'Id2'}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new CompositeKey
                {
                    Id1 = 77,
                    Id2 = 78,
                    AlternateId1 = 66,
                    AlternateId2 = 68
                })).Message);
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Identity_conflict_can_be_resolved_for_composite_primary_key(bool copy)
    {
        using var context = new IdentityConflictContext(
            copy
                ? new UpdatingIdentityResolutionInterceptor()
                : new IgnoringIdentityResolutionInterceptor());

        var entity = new CompositeKey
        {
            Id1 = 77,
            Id2 = 78,
            AlternateId1 = 66,
            AlternateId2 = 67,
            Value = "Existing"
        };
        context.Attach(entity);
        context.Attach(
            new CompositeKey
            {
                Id1 = 77,
                Id2 = 78,
                AlternateId1 = 66,
                AlternateId2 = 67,
                Value = "New"
            });

        Assert.Single(context.ChangeTracker.Entries());
        Assert.Equal(copy ? EntityState.Modified : EntityState.Unchanged, context.Entry(entity).State);
        Assert.Equal(copy ? "New" : "Existing", entity.Value);
    }

    [Fact]
    public void Identity_conflict_throws_for_composite_alternate_key()
    {
        using var context = new IdentityConflictContext();
        context.Attach(
            new CompositeKey
            {
                Id1 = 77,
                Id2 = 78,
                AlternateId1 = 66,
                AlternateId2 = 67
            });

        Assert.Equal(
            CoreStrings.IdentityConflict("CompositeKey", "{'AlternateId1', 'AlternateId2'}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new CompositeKey
                {
                    Id1 = 77,
                    Id2 = 79,
                    AlternateId1 = 66,
                    AlternateId2 = 67
                })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_owned_composite_primary_key()
    {
        using var context = new IdentityConflictContext();
        context.Attach(
            new CompositeKey
            {
                Id1 = 77,
                Id2 = 78,
                AlternateId1 = 66,
                AlternateId2 = 67,
                Owned = new CompositeKeyOwned()
            });

        var duplicateOwned = new CompositeKeyOwned();
        context.Entry(duplicateOwned).Property("CompositeKeyId1").CurrentValue = 77;
        context.Entry(duplicateOwned).Property("CompositeKeyId2").CurrentValue = 78;

        Assert.Equal(
            CoreStrings.IdentityConflictOwned("CompositeKeyOwned", "{'CompositeKeyId1', 'CompositeKeyId2'}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new CompositeKey
                {
                    Id1 = 177,
                    Id2 = 178,
                    AlternateId1 = 166,
                    AlternateId2 = 168,
                    Owned = duplicateOwned
                })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_primary_key_values_logged()
    {
        using var context = new SensitiveIdentityConflictContext();
        context.Attach(
            new SingleKey { Id = 77, AlternateId = 66 });

        Assert.Equal(
            CoreStrings.IdentityConflictSensitive("SingleKey", "{Id: 77}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 77, AlternateId = 67 })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_alternate_key_values_logged()
    {
        using var context = new SensitiveIdentityConflictContext();
        context.Attach(
            new SingleKey { Id = 77, AlternateId = 66 });

        Assert.Equal(
            CoreStrings.IdentityConflictSensitive("SingleKey", "{AlternateId: 66}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 78, AlternateId = 66 })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_owned_primary_keylogged()
    {
        using var context = new SensitiveIdentityConflictContext();
        context.Attach(
            new SingleKey
            {
                Id = 77,
                AlternateId = 66,
                Owned = new SingleKeyOwned()
            });

        var duplicateOwned = new SingleKeyOwned();
        context.Entry(duplicateOwned).Property("SingleKeyId").CurrentValue = 77;

        Assert.Equal(
            CoreStrings.IdentityConflictOwnedSensitive("SingleKeyOwned", "{SingleKeyId: 77}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey
                {
                    Id = 78,
                    AlternateId = 67,
                    Owned = duplicateOwned
                })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_composite_primary_key_values_logged()
    {
        using var context = new SensitiveIdentityConflictContext();
        context.Attach(
            new CompositeKey
            {
                Id1 = 77,
                Id2 = 78,
                AlternateId1 = 66,
                AlternateId2 = 67
            });

        Assert.Equal(
            CoreStrings.IdentityConflictSensitive("CompositeKey", "{Id1: 77, Id2: 78}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new CompositeKey
                {
                    Id1 = 77,
                    Id2 = 78,
                    AlternateId1 = 66,
                    AlternateId2 = 68
                })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_composite_alternate_key_values_logged()
    {
        using var context = new SensitiveIdentityConflictContext();
        context.Attach(
            new CompositeKey
            {
                Id1 = 77,
                Id2 = 78,
                AlternateId1 = 66,
                AlternateId2 = 67
            });

        Assert.Equal(
            CoreStrings.IdentityConflictSensitive("CompositeKey", "{AlternateId1: 66, AlternateId2: 67}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new CompositeKey
                {
                    Id1 = 77,
                    Id2 = 79,
                    AlternateId1 = 66,
                    AlternateId2 = 67
                })).Message);
    }

    [Fact]
    public void Identity_conflict_throws_for_owned_composite_primary_key_logged()
    {
        using var context = new SensitiveIdentityConflictContext();
        context.Attach(
            new CompositeKey
            {
                Id1 = 77,
                Id2 = 78,
                AlternateId1 = 66,
                AlternateId2 = 67,
                Owned = new CompositeKeyOwned()
            });

        var duplicateOwned = new CompositeKeyOwned();
        context.Entry(duplicateOwned).Property("CompositeKeyId1").CurrentValue = 77;
        context.Entry(duplicateOwned).Property("CompositeKeyId2").CurrentValue = 78;

        Assert.Equal(
            CoreStrings.IdentityConflictOwnedSensitive("CompositeKeyOwned", "{CompositeKeyId1: 77, CompositeKeyId2: 78}"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new CompositeKey
                {
                    Id1 = 177,
                    Id2 = 178,
                    AlternateId1 = 166,
                    AlternateId2 = 168,
                    Owned = duplicateOwned
                })).Message);
    }

    [Fact]
    public void Identity_null_throws_for_primary_key()
    {
        using var context = new IdentityConflictContext();
        Assert.Equal(
            CoreStrings.InvalidKeyValue("SingleKey", "Id"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = null, AlternateId = 67 })).Message);
    }

    [Fact]
    public void Identity_null_throws_for_alternate_key()
    {
        using var context = new IdentityConflictContext();
        Assert.Equal(
            CoreStrings.InvalidAlternateKeyValue("SingleKey", "AlternateId"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new SingleKey { Id = 77, AlternateId = null })).Message);
    }

    [Fact]
    public void Identity_null_throws_for_composite_primary_key()
    {
        using var context = new IdentityConflictContext();
        Assert.Equal(
            CoreStrings.InvalidKeyValue("CompositeKey", "Id2"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new CompositeKey
                {
                    Id1 = 77,
                    Id2 = null,
                    AlternateId1 = 66,
                    AlternateId2 = 68
                })).Message);
    }

    [Fact]
    public void Identity_null_throws_for_composite_alternate_key()
    {
        using var context = new IdentityConflictContext();
        Assert.Equal(
            CoreStrings.InvalidAlternateKeyValue("CompositeKey", "AlternateId2"),
            Assert.Throws<InvalidOperationException>(() => context.Attach(
                new CompositeKey
                {
                    Id1 = 77,
                    Id2 = 79,
                    AlternateId1 = 66,
                    AlternateId2 = null
                })).Message);
    }

    private class SensitiveIdentityConflictContext : IdentityConflictContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(IdentityConflictContext))
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(InMemoryFixture.DefaultSensitiveServiceProvider);
    }

    private class IdentityConflictContext(params IInterceptor[] interceptors) : DbContext
    {
        private readonly IInterceptor[] _interceptors = interceptors;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .AddInterceptors(_interceptors)
                .UseInMemoryDatabase(nameof(IdentityConflictContext))
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider);

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SingleKey>(b =>
            {
                b.HasKey(e => e.Id);
                b.HasAlternateKey(e => e.AlternateId);
                b.Property(e => e.Id).ValueGeneratedNever();
                b.Property(e => e.AlternateId).ValueGeneratedNever();
                b.OwnsOne(e => e.Owned);
            });

            modelBuilder.Entity<CompositeKey>(b =>
            {
                b.HasKey(e => new { e.Id1, e.Id2 });
                b.HasAlternateKey(e => new { e.AlternateId1, e.AlternateId2 });
                b.OwnsOne(e => e.Owned);
            });
        }
    }

    private class SingleKeyOwned
    {
        public string Value { get; set; }
    }

    private class CompositeKeyOwned
    {
        public string Value { get; set; }
    }

    private class SingleKey
    {
        public int? Id { get; set; }
        public int? AlternateId { get; set; }

        public string Value { get; set; }

        public SingleKeyOwned Owned { get; set; }
    }

    private class CompositeKey
    {
        public int? Id1 { get; set; }
        public int? Id2 { get; set; }

        public int? AlternateId1 { get; set; }
        public int? AlternateId2 { get; set; }

        public string Value { get; set; }

        public CompositeKeyOwned Owned { get; set; }
    }

    private class WidgetContext : DbContext
    {
        public DbSet<Widget> Widgets { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(WidgetContext) + Guid.NewGuid())
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider);
    }

    [Fact]
    public void StartTracking_is_no_op_if_entity_is_already_tracked()
    {
        var model = BuildModel();
        var categoryType = model.FindEntityType(typeof(Category));
        var stateManager = CreateStateManager(model);

        var category = new Category { Id = 77, PrincipalId = 777 };
        var snapshot = new Snapshot<int, string, int>(77, "Bjork", 777);

        var entry = stateManager.StartTrackingFromQuery(categoryType, category, snapshot);

        Assert.Same(entry, stateManager.StartTrackingFromQuery(categoryType, category, snapshot));
    }

    [Fact]
    public void StartTracking_throws_for_invalid_entity_key()
    {
        var model = BuildModel();
        var stateManager = CreateStateManager(model);

        var entry = stateManager.GetOrCreateEntry(
            new Dogegory { Id = null });

        Assert.Equal(
            CoreStrings.InvalidKeyValue("Dogegory", "Id"),
            Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(entry)).Message);
    }

    [Fact]
    public void StartTracking_throws_for_invalid_alternate_key()
    {
        var model = BuildModel();
        var stateManager = CreateStateManager(model);

        var entry = stateManager.GetOrCreateEntry(
            new Category { Id = 77, PrincipalId = null });

        Assert.Equal(
            CoreStrings.InvalidAlternateKeyValue("Category", "PrincipalId"),
            Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(entry)).Message);
    }

    [Fact]
    public void Can_get_existing_entry_even_if_state_not_yet_set()
    {
        var stateManager = CreateStateManager(BuildModel());
        var category = new Category { Id = 1 };

        var entry = stateManager.GetOrCreateEntry(category);
        var entry2 = stateManager.GetOrCreateEntry(category);

        Assert.Same(entry, entry2);
        Assert.Equal(EntityState.Detached, entry.EntityState);
    }

    [Fact]
    public void Can_stop_tracking_and_then_start_tracking_again()
    {
        var stateManager = CreateStateManager(BuildModel());
        var category = new Category { Id = 1, PrincipalId = 777 };

        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);
        entry.SetEntityState(EntityState.Detached);
        entry.SetEntityState(EntityState.Added);

        var entry2 = stateManager.GetOrCreateEntry(category);
        Assert.Same(entry, entry2);
    }

    [Fact]
    public void Can_stop_tracking_and_then_start_tracking_using_a_new_state_entry()
    {
        var stateManager = CreateStateManager(BuildModel());
        var category = new Category { Id = 1, PrincipalId = 777 };

        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);
        entry.SetEntityState(EntityState.Detached);

        var entry2 = stateManager.GetOrCreateEntry(category);
        Assert.NotSame(entry, entry2);

        entry2.SetEntityState(EntityState.Added);
    }

    [Fact]
    public void StopTracking_releases_reference_to_entry()
    {
        var stateManager = CreateStateManager(BuildModel());
        var category = new Category { Id = 1, PrincipalId = 777 };

        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);
        entry.SetEntityState(EntityState.Detached);

        var entry2 = stateManager.GetOrCreateEntry(category);
        entry2.SetEntityState(EntityState.Added);

        Assert.NotSame(entry, entry2);
        Assert.Equal(EntityState.Detached, entry.EntityState);
    }

    [Fact]
    public void Entry_for_untracked_entity_is_cached_while_the_entity_is_referenced()
    {
        var stateManager = CreateStateManager(BuildModel());
        var category = new Category { Id = 1, PrincipalId = 777 };

        // GetOrCreateEntry (e.g. via ctx.Entry(category)) caches an entry for the untracked entity so
        // that the same entry is returned and a subsequent Add/Attach acts on it. This must keep working
        // as long as the entity is referenced.
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Detached);

        Assert.Same(entry, stateManager.TryGetEntry(category));
    }

    [Fact]
    public void Entry_for_untracked_entity_does_not_keep_the_entity_alive()
    {
        var stateManager = CreateStateManager(BuildModel());

        // The detached-entity cache must not prevent the entity from being collected once nothing else
        // references it, even while the state manager (context) is still alive (issue #33557).
        var reference = CreateDetachedEntry(stateManager);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(reference.IsAlive);

        // Keep the state manager (which owns the detached-entity cache) alive across the collection;
        // otherwise the cache could be collected along with it and the entity would be released even
        // with a strong cache, making this test pass without actually exercising the weak reference.
        GC.KeepAlive(stateManager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateDetachedEntry(IStateManager stateManager)
    {
        var category = new Category { Id = 1, PrincipalId = 777 };
        stateManager.GetOrCreateEntry(category).SetEntityState(EntityState.Detached);
        return new WeakReference(category);
    }

    [Fact]
    public void Entry_for_untracked_entity_survives_collection_while_the_entity_is_referenced()
    {
        var stateManager = CreateStateManager(BuildModel());
        var category = new Category { Id = 1, PrincipalId = 777 };

        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Detached);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // While the entity is still referenced, the weak cache must not drop the entry: the same entry
        // must be returned so a subsequent Add/Attach acts on it (entry-stability contract, issue #33557).
        Assert.Same(entry, stateManager.TryGetEntry(category));
    }

    [Fact]
    public void Entry_for_untracked_shared_type_entity_does_not_keep_the_entity_alive()
    {
        var model = BuildModelWithSharedType();
        var stateManager = CreateStateManager(model);
        var entityType = model.FindEntityType("SharedCategoryA")!;

        // Shared-type entities are cached in a per-type sub-map; that sub-map's detached cache must also
        // hold entries weakly so the entity can be collected once nothing else references it (issue #33557).
        var reference = CreateDetachedSharedTypeEntry(stateManager, entityType);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(reference.IsAlive);

        GC.KeepAlive(stateManager);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateDetachedSharedTypeEntry(IStateManager stateManager, IEntityType entityType)
    {
        var entity = new Category { Id = 1, PrincipalId = 777 };
        stateManager.GetOrCreateEntry(entity, entityType).SetEntityState(EntityState.Detached);
        return new WeakReference(entity);
    }

    [Fact]
    public void Detaching_a_tracked_graph_does_not_retain_references_to_detached_entities()
    {
        using var context = new WidgetContext();

        // Reproduces the issue's pattern: add a graph, then detach it by iterating and setting each
        // entity to Detached (detaching the principal cascade-detaches the dependents, so the later
        // iterations call ctx.Entry(...) on already-detached entities, caching detached entries).
        // The context must not retain references to those entities once they are no longer referenced.
        var references = AddAndDetachGraph(context);

        Assert.Empty(context.ChangeTracker.Entries());

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.All(references, reference => Assert.False(reference.IsAlive));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference[] AddAndDetachGraph(WidgetContext context)
    {
        var root = new Widget { Id = -1 };
        var child = new Widget { Id = -2, ParentWidget = root };
        var grandChild = new Widget { Id = -3, ParentWidget = child };
        root.ChildWidgets = [child];
        child.ChildWidgets = [grandChild];

        context.Add(grandChild);

        foreach (var entity in new object[] { root, child, grandChild })
        {
            context.Entry(entity).State = EntityState.Detached;
        }

        return [new WeakReference(root), new WeakReference(child), new WeakReference(grandChild)];
    }

    [Fact]
    public void Can_add_an_equivalent_graph_after_detaching_a_graph_with_the_same_keys()
    {
        using var context = new WidgetContext();

        var first = BuildWidgetGraph();

        // Capture the first graph's instances up front: detaching cascades through the graph and
        // nulls the navigations, so they can't be reached from 'first' afterwards.
        var firstGraph = new object[] { first, first.ParentWidget!, first.ParentWidget!.ParentWidget! };

        context.Add(first);
        foreach (var entity in firstGraph)
        {
            context.Entry(entity).State = EntityState.Detached;
        }

        var second = BuildWidgetGraph();

        // Must not throw an identity conflict and must not pull the detached first graph back in.
        context.Add(second);

        var entries = context.ChangeTracker.Entries().ToList();
        Assert.Equal(3, entries.Count);
        Assert.All(entries, e => Assert.Equal(EntityState.Added, e.State));
        Assert.DoesNotContain(entries, e => firstGraph.Contains(e.Entity));

        context.SaveChanges();

        Assert.Equal(3, context.Set<Widget>().Count());

        // Builds a three-level chain (root -> child -> grandChild) with the same keys each time, and
        // returns the leaf so adding it pulls in the whole graph via its to-principal navigations.
        static Widget BuildWidgetGraph()
        {
            var root = new Widget { Id = -1 };
            var child = new Widget { Id = -2, ParentWidget = root };
            var grandChild = new Widget { Id = -3, ParentWidget = child };
            root.ChildWidgets = [child];
            child.ChildWidgets = [grandChild];
            return grandChild;
        }
    }

    [Fact]
    public void Throws_on_attempt_to_start_tracking_entity_with_null_key()
    {
        var stateManager = CreateStateManager(BuildModel());
        var entity = new Dogegory();

        var entry = stateManager.GetOrCreateEntry(entity);

        Assert.Equal(
            CoreStrings.InvalidKeyValue("Dogegory", "Id"),
            Assert.Throws<InvalidOperationException>(() => stateManager.StartTracking(entry)).Message);
    }

    [Fact]
    public void Throws_on_attempt_to_start_tracking_with_wrong_manager()
    {
        var model = BuildModel();
        var stateManager = CreateStateManager(model);
        var stateManager2 = CreateStateManager(model);

        var entry = stateManager.GetOrCreateEntry(new Category());

        Assert.Equal(
            CoreStrings.WrongStateManager(nameof(Category)),
            Assert.Throws<InvalidOperationException>(() => stateManager2.StartTracking(entry)).Message);
    }

    [Fact]
    public void Will_get_new_entry_if_another_entity_with_the_same_key_is_already_tracked()
    {
        var stateManager = CreateStateManager(BuildModel());

        Assert.NotSame(
            stateManager.GetOrCreateEntry(
                new Category { Id = 77 }),
            stateManager.GetOrCreateEntry(
                new Category { Id = 77 }));
    }

    [Fact]
    public void Can_get_all_entities()
    {
        var stateManager = CreateStateManager(BuildModel());

        var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
        var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

        stateManager.StartTracking(
                stateManager.GetOrCreateEntry(
                    new Category { Id = 77, PrincipalId = 777 }))
            .SetEntityState(EntityState.Unchanged);

        stateManager.StartTracking(
                stateManager.GetOrCreateEntry(
                    new Category { Id = 78, PrincipalId = 778 }))
            .SetEntityState(EntityState.Unchanged);

        stateManager.StartTracking(
                stateManager.GetOrCreateEntry(
                    new Product { Id = productId1 }))
            .SetEntityState(EntityState.Unchanged);

        stateManager.StartTracking(
                stateManager.GetOrCreateEntry(
                    new Product { Id = productId2 }))
            .SetEntityState(EntityState.Unchanged);

        Assert.Equal(4, stateManager.Entries.Count());

        Assert.Equal(
            new[] { 77, 78 },
            stateManager.Entries
                .Select(e => e.Entity)
                .OfType<Category>()
                .Select(e => e.Id)
                .OrderBy(k => k)
                .ToArray());

        Assert.Equal(
            new[] { productId2, productId1 },
            stateManager.Entries
                .Select(e => e.Entity)
                .OfType<Product>()
                .Select(e => e.Id)
                .OrderBy(k => k)
                .ToArray());
    }

    private class TestListener : INavigationFixer
    {
        public int ChangingCount;
        public int ChangedCount;
        public EntityState ChangingState;
        public EntityState ChangedState;

        public bool BeginDelayedFixup()
            => false;

        public void CompleteDelayedFixup()
        {
        }

        public void AbortDelayedFixup()
        {
        }

        public void NavigationReferenceChanged(
            InternalEntityEntry entry,
            INavigationBase navigationBase,
            object oldValue,
            object newValue)
        {
        }

        public void NavigationCollectionChanged(
            InternalEntityEntry entry,
            INavigationBase navigationBase,
            IEnumerable<object> added,
            IEnumerable<object> removed)
        {
        }

        public void TrackedFromQuery(InternalEntityEntry entry)
        {
        }

        public void KeyPropertyChanged(
            InternalEntityEntry entry,
            IProperty property,
            IEnumerable<IKey> containingPrincipalKeys,
            IEnumerable<IForeignKey> containingForeignKeys,
            object oldValue,
            object newValue)
        {
        }

        public void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
            ChangingCount++;
            ChangingState = newState;
        }

        public void StateChanged(InternalEntityEntry entry, EntityState oldState, bool fromQuery)
        {
            ChangedCount++;
            ChangedState = oldState;

            Assert.False(fromQuery);
        }

        public void FixupResolved(InternalEntityEntry entry, InternalEntityEntry duplicateEntry)
        {
        }
    }

    [Fact]
    public void DetectChanges_is_called_for_all_tracked_entities_and_returns_true_if_any_changes_detected()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var entry1 = stateManager.GetOrCreateEntry(
            new Category
            {
                Id = 77,
                Name = "Beverages",
                PrincipalId = 777
            });
        var entry2 = stateManager.GetOrCreateEntry(
            new Category
            {
                Id = 78,
                Name = "Foods",
                PrincipalId = 778
            });
        var entry3 = stateManager.GetOrCreateEntry(
            new Category
            {
                Id = 79,
                Name = "Stuff",
                PrincipalId = 779
            });

        entry1.SetEntityState(EntityState.Unchanged);
        entry2.SetEntityState(EntityState.Unchanged);
        entry3.SetEntityState(EntityState.Unchanged);

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        ((Category)entry1.Entity).Name = "Egarevebs";
        ((Category)entry2.Entity).Name = "Doofs";
        ((Category)entry3.Entity).Name = "Ffuts";

        changeDetector.DetectChanges(stateManager);

        Assert.Equal(EntityState.Modified, entry1.EntityState);
        Assert.Equal(EntityState.Modified, entry2.EntityState);
        Assert.Equal(EntityState.Modified, entry3.EntityState);
    }

    [Fact]
    public void AcceptAllChanges_processes_all_tracked_entities()
    {
        var stateManager = CreateStateManager(BuildModel());

        var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
        var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

        var entry1 = stateManager.GetOrCreateEntry(
            new Category { Id = 77, PrincipalId = 777 });
        var entry2 = stateManager.GetOrCreateEntry(
            new Category { Id = 78, PrincipalId = 778 });
        var entry3 = stateManager.GetOrCreateEntry(
            new Product { Id = productId1 });
        var entry4 = stateManager.GetOrCreateEntry(
            new Product { Id = productId2 });

        entry1.SetEntityState(EntityState.Added);
        entry2.SetEntityState(EntityState.Modified);
        entry3.SetEntityState(EntityState.Unchanged);
        entry4.SetEntityState(EntityState.Deleted);

        stateManager.AcceptAllChanges();

        Assert.Equal(3, stateManager.Entries.Count());
        Assert.Contains(entry1, stateManager.Entries);
        Assert.Contains(entry2, stateManager.Entries);
        Assert.Contains(entry3, stateManager.Entries);

        Assert.Equal(EntityState.Unchanged, entry1.EntityState);
        Assert.Equal(EntityState.Unchanged, entry2.EntityState);
        Assert.Equal(EntityState.Unchanged, entry3.EntityState);
        Assert.Equal(EntityState.Detached, entry4.EntityState);
    }

    [Fact]
    public void Can_get_all_dependent_entries()
    {
        var model = BuildModel();
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var categoryEntry1 = stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Category { Id = 1, PrincipalId = 77 }));
        var categoryEntry2 = stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Category { Id = 2, PrincipalId = 78 }));
        var categoryEntry3 = stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Category { Id = 3, PrincipalId = 79 }));
        var categoryEntry4 = stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Category { Id = 4, PrincipalId = 0 }));
        var productEntry1 = stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Product { Id = Guid.NewGuid(), DependentId = 77 }));
        var productEntry2 = stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Product { Id = Guid.NewGuid(), DependentId = 77 }));
        var productEntry3 = stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Product { Id = Guid.NewGuid(), DependentId = 78 }));
        var productEntry4 = stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Product { Id = Guid.NewGuid(), DependentId = 78 }));
        stateManager.StartTracking(
            stateManager.GetOrCreateEntry(
                new Product { Id = Guid.NewGuid(), DependentId = null }));

        var fk = model.FindEntityType(typeof(Product)).GetForeignKeys().Single();

        Assert.Equal(
            [productEntry1, productEntry2],
            stateManager.GetDependents(categoryEntry1, fk).ToArray());

        Assert.Equal(
            [productEntry3, productEntry4],
            stateManager.GetDependents(categoryEntry2, fk).ToArray());

        Assert.Empty(stateManager.GetDependents(categoryEntry3, fk).ToArray());
        Assert.Empty(stateManager.GetDependents(categoryEntry4, fk).ToArray());
    }

    [Fact]
    public void Does_not_throws_when_instance_of_unmapped_derived_type_is_used()
    {
        var model = BuildModel();
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(model);
        model = contextServices.GetRequiredService<IModel>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var entry = stateManager.GetOrCreateEntry(new SpecialProduct());

        Assert.Same(model.FindEntityType(typeof(Product)), entry.EntityType);
    }

    private static IStateManager CreateStateManager(IModel model)
        => InMemoryTestHelpers.Instance.CreateContextServices(model).GetRequiredService<IStateManager>();

    [Fact]
    public void CreateEntry_distinguishes_complex_properties_that_share_a_complex_type()
    {
        var model = BuildModelWithDuplicateComplexType();
        var stateManager = CreateStateManager(model);
        var entityType = model.FindEntityType(typeof(Place))!;

        var id = entityType.FindProperty(nameof(Place.Id))!;
        var note = entityType.FindProperty("Note")!;
        var origin = entityType.FindComplexProperty(nameof(Place.Origin))!;
        var destination = entityType.FindComplexProperty(nameof(Place.Destination))!;
        var originLatitude = origin.ComplexType.FindProperty(nameof(Coordinate.Latitude))!;
        var originLongitude = origin.ComplexType.FindProperty(nameof(Coordinate.Longitude))!;
        var destinationLatitude = destination.ComplexType.FindProperty(nameof(Coordinate.Latitude))!;
        var destinationLongitude = destination.ComplexType.FindProperty(nameof(Coordinate.Longitude))!;

        var entry = stateManager.CreateEntry(
            new Dictionary<IProperty, object>
            {
                { id, 1 },
                { note, "Somewhere" },
                { originLatitude, 11 },
                { originLongitude, 12 },
                { destinationLatitude, 21 },
                { destinationLongitude, 22 }
            },
            entityType);

        Assert.Equal("Somewhere", entry[note]);
        Assert.Equal(11, entry[originLatitude]);
        Assert.Equal(12, entry[originLongitude]);
        Assert.Equal(21, entry[destinationLatitude]);
        Assert.Equal(22, entry[destinationLongitude]);

        var place = (Place)entry.Entity;
        Assert.Equal(11, place.Origin.Latitude);
        Assert.Equal(12, place.Origin.Longitude);
        Assert.Equal(21, place.Destination.Latitude);
        Assert.Equal(22, place.Destination.Longitude);
    }

    [Fact]
    public void CreateEntry_uses_sentinel_for_unsupplied_value_type_properties()
    {
        var model = BuildModelWithDuplicateComplexType();
        var stateManager = CreateStateManager(model);
        var entityType = model.FindEntityType(typeof(Place))!;

        var id = entityType.FindProperty(nameof(Place.Id))!;
        var origin = entityType.FindComplexProperty(nameof(Place.Origin))!;
        var originLatitude = origin.ComplexType.FindProperty(nameof(Coordinate.Latitude))!;
        var originLongitude = origin.ComplexType.FindProperty(nameof(Coordinate.Longitude))!;

        // Supply only the key and one coordinate component. The unsupplied non-nullable value-type
        // properties must fall back to their sentinel (default) values rather than null, which would
        // otherwise cause an invalid cast during materialization.
        var entry = stateManager.CreateEntry(
            new Dictionary<IProperty, object> { { id, 1 }, { originLatitude, 11 } },
            entityType);

        Assert.Equal(1, entry[id]);
        Assert.Equal(11, entry[originLatitude]);
        Assert.Equal(originLongitude.Sentinel, entry[originLongitude]);

        var place = (Place)entry.Entity;
        Assert.Equal(11, place.Origin.Latitude);
        Assert.Equal(0, place.Origin.Longitude);
        Assert.Equal(0, place.Destination.Latitude);
        Assert.Equal(0, place.Destination.Longitude);
    }

    [Fact]
    public void CreateEntry_throws_for_property_from_another_entity_type()
    {
        var model = BuildModelWithDuplicateComplexType();
        var stateManager = CreateStateManager(model);
        var entityType = model.FindEntityType(typeof(Place))!;
        var otherEntityType = model.FindEntityType(typeof(Location))!;

        var id = entityType.FindProperty(nameof(Place.Id))!;
        var foreignProperty = otherEntityType.FindProperty(nameof(Location.Planet))!;

        Assert.Equal(
            CoreStrings.PropertyDoesNotBelong(
                foreignProperty.Name, otherEntityType.DisplayName(), entityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => stateManager.CreateEntry(
                    new Dictionary<IProperty, object>
                    {
                        { id, 1 },
                        { foreignProperty, "Mars" }
                    },
                    entityType)).Message);
    }

    [Fact]
    public void CreateEntry_throws_for_complex_property_from_another_entity_type()
    {
        var model = BuildModelWithDuplicateComplexType();
        var stateManager = CreateStateManager(model);
        var entityType = model.FindEntityType(typeof(Place))!;
        var otherEntityType = model.FindEntityType(typeof(Pin))!;

        var id = entityType.FindProperty(nameof(Place.Id))!;
        var otherCoordinate = otherEntityType.FindComplexProperty(nameof(Pin.At))!;
        var foreignLatitude = otherCoordinate.ComplexType.FindProperty(nameof(Coordinate.Latitude))!;

        Assert.Equal(
            CoreStrings.PropertyDoesNotBelong(
                foreignLatitude.Name, otherEntityType.DisplayName(), entityType.DisplayName()),
            Assert.Throws<InvalidOperationException>(
                () => stateManager.CreateEntry(
                    new Dictionary<IProperty, object>
                    {
                        { id, 1 },
                        { foreignLatitude, 11 }
                    },
                    entityType)).Message);
    }

    public class Widget
    {
        public int Id { get; set; }

        public int? ParentWidgetId { get; set; }
        public Widget ParentWidget { get; set; }

        public List<Widget> ChildWidgets { get; set; }
    }

    private class Category
    {
        public int Id { get; set; }
        public int? PrincipalId { get; set; }
        public string Name { get; set; }
    }

    private class Product
    {
        public Guid Id { get; set; }
        public int? DependentId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    private class SpecialProduct : Product;

    private class Dogegory
    {
        public string Id { get; set; }
    }

    private class Location
    {
        public int Id { get; set; }
        public string Planet { get; set; }
    }

    private class Place
    {
        public int Id { get; set; }
        public Coordinate Origin { get; set; } = null!;
        public Coordinate Destination { get; set; } = null!;
    }

    private class Pin
    {
        public int Id { get; set; }
        public Coordinate At { get; set; } = null!;
    }

    private class Coordinate
    {
        public int Latitude { get; set; }
        public int Longitude { get; set; }
    }

    private static IModel BuildModel()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<Product>().HasOne<Category>().WithOne()
            .HasForeignKey<Product>(e => e.DependentId)
            .HasPrincipalKey<Category>(e => e.PrincipalId);

        builder.Entity<Widget>()
            .HasOne(e => e.ParentWidget)
            .WithMany(e => e.ChildWidgets)
            .HasForeignKey(e => e.ParentWidgetId);

        builder.Entity<Category>();

        builder.Entity<Dogegory>();

        builder.Entity<Location>();

        return builder.Model.FinalizeModel();
    }

    private static IModel BuildModelWithDuplicateComplexType()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<Place>(b =>
        {
            b.Property<string>("Note");
            b.ComplexProperty(e => e.Origin);
            b.ComplexProperty(e => e.Destination);
        });

        builder.Entity<Pin>(b => b.ComplexProperty(e => e.At));

        builder.Entity<Location>();

        return builder.Model.FinalizeModel();
    }

    private static IModel BuildModelWithSharedType()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        // Reuse Category as the CLR type for two shared-type entities so the model has a shared-type
        // sub-map to exercise (Category is not used as a regular entity type in this model).
        builder.SharedTypeEntity<Category>("SharedCategoryA");
        builder.SharedTypeEntity<Category>("SharedCategoryB");

        return builder.Model.FinalizeModel();
    }
}
