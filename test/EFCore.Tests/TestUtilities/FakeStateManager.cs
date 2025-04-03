// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class FakeStateManager : IStateManager
{
    public IEnumerable<InternalEntityEntry> InternalEntries { get; set; }
    public bool SaveChangesCalled { get; set; }
    public bool SaveChangesAsyncCalled { get; set; }

    public void ResetState()
    {
    }

    public Task ResetStateAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public void Unsubscribe(bool resetting)
    {
    }

    public (EventHandler<EntityTrackingEventArgs> Tracking, EventHandler<EntityTrackedEventArgs> Tracked,
        EventHandler<EntityStateChangingEventArgs> StateChanging, EventHandler<EntityStateChangedEventArgs> StateChanged) CaptureEvents()
        => (null, null, null, null);

    public void SetEvents(
        EventHandler<EntityTrackingEventArgs> tracking,
        EventHandler<EntityTrackedEventArgs> tracked,
        EventHandler<EntityStateChangingEventArgs> stateChanging,
        EventHandler<EntityStateChangedEventArgs> stateChanged)
    {
    }

    public int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SaveChangesCalled = true;
        return 1;
    }

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new())
    {
        SaveChangesAsyncCalled = true;
        return Task.FromResult(1);
    }

    public InternalEntityEntry TryGetExistingEntry(object entity, IKey key)
        => throw new NotImplementedException();

    public IEnumerable<InternalEntityEntry> Entries
        => InternalEntries ?? Enumerable.Empty<InternalEntityEntry>();

    public IEnumerable<InternalEntityEntry> GetEntriesForState(
        bool added = false,
        bool modified = false,
        bool deleted = false,
        bool unchanged = false,
        bool returnSharedIdentity = false)
        => throw new NotImplementedException();

    public int GetCountForState(
        bool added = false,
        bool modified = false,
        bool deleted = false,
        bool unchanged = false,
        bool countSharedIdentity = false)
        => throw new NotImplementedException();

    public int ChangedCount { get; set; }

    public int Count
        => throw new NotImplementedException();

    public IDiagnosticsLogger<DbLoggerCategory.Update> UpdateLogger { get; }

    public void Clear(bool resetting)
        => throw new NotImplementedException();

    public bool SavingChanges
        => throw new NotImplementedException();

    public IEnumerable<TEntity> GetNonDeletedEntities<TEntity>()
        where TEntity : class
        => throw new NotImplementedException();

    public IEntityFinder CreateEntityFinder(IEntityType entityType)
        => throw new NotImplementedException();

    public void UpdateIdentityMap(InternalEntityEntry entry, IKey principalKey)
        => throw new NotImplementedException();

    public void UpdateDependentMap(InternalEntityEntry entry, IForeignKey foreignKey)
        => throw new NotImplementedException();

    public IEnumerable<InternalEntityEntry> GetEntries(IKey key)
        => throw new NotImplementedException();

    public IEnumerable<IUpdateEntry> GetDependents(IUpdateEntry principalEntry, IForeignKey foreignKey)
        => throw new NotImplementedException();

    public IEnumerable<IUpdateEntry> GetDependents(IReadOnlyList<object> keyValues, IForeignKey foreignKey)
        => throw new NotImplementedException();

    public IEnumerable<IUpdateEntry> GetDependentsUsingRelationshipSnapshot(
        IUpdateEntry principalEntry,
        IForeignKey foreignKey)
        => throw new NotImplementedException();

    public IEnumerable<IUpdateEntry> GetDependentsFromNavigation(IUpdateEntry principalEntry, IForeignKey foreignKey)
        => throw new NotImplementedException();

    public IList<IUpdateEntry> GetEntriesToSave(bool cascadeChanges)
        => Enumerable.Empty<IUpdateEntry>().ToList();

    public virtual void AcceptAllChanges()
        => throw new NotImplementedException();

    public StateManagerDependencies Dependencies { get; }
    public CascadeTiming DeleteOrphansTiming { get; set; }
    public CascadeTiming CascadeDeleteTiming { get; set; }

    public InternalEntityEntry GetOrCreateEntry(object entity)
        => throw new NotImplementedException();

    public InternalEntityEntry GetOrCreateEntry(object entity, IEntityType entityType)
        => throw new NotImplementedException();

    public InternalEntityEntry CreateEntry(IDictionary<string, object> values, IEntityType entityType)
        => throw new NotImplementedException();

    public InternalEntityEntry StartTrackingFromQuery(
        IEntityType baseEntityType,
        object entity,
        in ISnapshot snapshot)
        => throw new NotImplementedException();

    public InternalEntityEntry TryGetEntry(IKey key, IReadOnlyList<object> keyValues)
        => throw new NotImplementedException();

    public InternalEntityEntry TryGetEntryTyped<TKey>(IKey key, TKey keyValue)
        => throw new NotImplementedException();

    public InternalEntityEntry TryGetEntry(IKey key, object[] keyValues)
        => throw new NotImplementedException();

    public InternalEntityEntry TryGetEntry(IKey key, in ValueBuffer valueBuffer, bool throwOnNullKey)
        => throw new NotImplementedException();

    public InternalEntityEntry TryGetEntry(object entity, bool throwOnNonUniqueness = true)
        => throw new NotImplementedException();

    public InternalEntityEntry TryGetEntry(object entity, IEntityType type, bool throwOnTypeMismatch = true)
        => throw new NotImplementedException();

    public IInternalEntityEntryNotifier InternalEntityEntryNotifier
        => throw new NotImplementedException();

    public void ChangingState(InternalEntityEntry entry, EntityState newState)
        => throw new NotImplementedException();

    public IValueGenerationManager ValueGenerationManager
        => throw new NotImplementedException();

    public IEntityMaterializerSource EntityMaterializerSource { get; }

    public InternalEntityEntry StartTracking(InternalEntityEntry entry)
        => throw new NotImplementedException();

    public void StopTracking(InternalEntityEntry entry, EntityState oldState)
        => throw new NotImplementedException();

    public void RecordReferencedUntrackedEntity(
        object referencedEntity,
        INavigationBase navigation,
        InternalEntityEntry referencedFromEntry)
        => throw new NotImplementedException();

    public void UpdateReferencedUntrackedEntity(
        object referencedEntity,
        object newReferencedEntity,
        INavigationBase navigation,
        InternalEntityEntry referencedFromEntry)
        => throw new NotImplementedException();

    public bool ResolveToExistingEntry(
        InternalEntityEntry newEntry,
        INavigationBase navigation,
        InternalEntityEntry referencedFromEntry)
        => throw new NotImplementedException();

    public IEnumerable<Tuple<INavigationBase, InternalEntityEntry>> GetRecordedReferrers(object referencedEntity, bool clear)
        => throw new NotImplementedException();

    public void BeginAttachGraph()
        => throw new NotImplementedException();

    public void CompleteAttachGraph()
        => throw new NotImplementedException();

    public void AbortAttachGraph()
        => throw new NotImplementedException();

    public InternalEntityEntry FindPrincipal(InternalEntityEntry entityEntry, IForeignKey foreignKey)
        => throw new NotImplementedException();

    public InternalEntityEntry FindPrincipalUsingPreStoreGeneratedValues(InternalEntityEntry entityEntry, IForeignKey foreignKey)
        => throw new NotImplementedException();

    public InternalEntityEntry FindPrincipalUsingRelationshipSnapshot(InternalEntityEntry entityEntry, IForeignKey foreignKey)
        => throw new NotImplementedException();

    public DbContext Context
        => new(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase("D")
                .Options);

    public IModel Model
        => throw new NotImplementedException();

    public event EventHandler<EntityTrackingEventArgs> Tracking;

    public void OnTracking(InternalEntityEntry internalEntityEntry, EntityState state, bool fromQuery)
        => Tracking?.Invoke(null, null);

    public event EventHandler<EntityTrackedEventArgs> Tracked;

    public void OnTracked(InternalEntityEntry internalEntityEntry, bool fromQuery)
        => Tracked?.Invoke(null, null);

    public event EventHandler<EntityStateChangingEventArgs> StateChanging;

    public void OnStateChanging(InternalEntityEntry internalEntityEntry, EntityState newState)
        => StateChanging?.Invoke(null, null);

    public event EventHandler<EntityStateChangedEventArgs> StateChanged;

    public void OnStateChanged(InternalEntityEntry internalEntityEntry, EntityState oldState)
        => StateChanged?.Invoke(null, null);

    public bool SensitiveLoggingEnabled { get; }

    public void CascadeChanges(bool force)
        => throw new NotImplementedException();

    public void CascadeDelete(InternalEntityEntry entry, bool force, IEnumerable<IForeignKey> foreignKeys = null)
        => throw new NotImplementedException();

    public InternalEntityEntry TryGetEntry(IKey key, object[] keyValues, bool throwOnNullKey, out bool hasNullKey)
        => throw new NotImplementedException();
}
