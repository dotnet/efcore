// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class ChangeDetector : IChangeDetector
{
    private readonly IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> _logger;
    private readonly ILoggingOptions _loggingOptions;
    private bool _inCascadeDelete;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public ChangeDetector(
        IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
        ILoggingOptions loggingOptions)
    {
        _logger = logger;
        _loggingOptions = loggingOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void PropertyChanged(IInternalEntry entry, IPropertyBase propertyBase, bool setModified)
    {
        if (entry.EntityState is EntityState.Detached
            || propertyBase is IServiceProperty)
        {
            return;
        }

        if (propertyBase is IProperty property)
        {
            if (entry.EntityState is not EntityState.Deleted)
            {
                entry.SetPropertyModified(property, setModified);
            }
            else
            {
                ThrowIfKeyChanged(entry, property);
            }

            DetectKeyChange(entry, property);
        }
        else if (propertyBase.GetRelationshipIndex() != -1
                 && propertyBase is INavigationBase navigation)
        {
            DetectNavigationChange(entry as InternalEntityEntry ?? throw new UnreachableException("Complex type entry with a navigation"), navigation);
        }
    }

    private static void ThrowIfKeyChanged(IInternalEntry entry, IProperty property)
    {
        if (property.IsKey()
            && property.GetAfterSaveBehavior() == PropertySaveBehavior.Throw)
        {
            throw new InvalidOperationException(CoreStrings.KeyReadOnly(property.Name, entry.StructuralType.DisplayName()));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void PropertyChanging(IInternalEntry entry, IPropertyBase propertyBase)
    {
        if (entry.EntityState == EntityState.Detached
            || propertyBase is IServiceProperty)
        {
            return;
        }

        if (!entry.StructuralType.UseEagerSnapshots())
        {
            if (propertyBase is IProperty asProperty
                && asProperty.GetOriginalValueIndex() != -1)
            {
                entry.EnsureOriginalValues();
            }

            if (propertyBase.GetRelationshipIndex() != -1)
            {
                var entityEntry = entry as InternalEntityEntry ?? throw new UnreachableException("Complex type entry with a navigation");
                entityEntry.EnsureRelationshipSnapshot();
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void DetectChanges(IStateManager stateManager)
    {
        if (_inCascadeDelete)
        {
            return;
        }

        try
        {
            _inCascadeDelete = true;

            OnDetectingAllChanges(stateManager);
            var changesFound = false;

            _logger.DetectChangesStarting(stateManager.Context);

            foreach (var entry in stateManager.ToList()) // Might be too big, but usually _all_ entities are using Snapshot tracking
            {
                switch (entry.EntityState)
                {
                    case EntityState.Detached:
                        break;
                    case EntityState.Deleted:
                        if (entry.SharedIdentityEntry != null)
                        {
                            continue;
                        }

                        goto default;
                    default:
                        if (LocalDetectChanges(entry))
                        {
                            changesFound = true;
                        }

                        break;
                }
            }

            _logger.DetectChangesCompleted(stateManager.Context);

            OnDetectedAllChanges(stateManager, changesFound);
        }
        finally
        {
            _inCascadeDelete = false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void DetectChanges(InternalEntityEntry entry)
    {
        if (_inCascadeDelete)
        {
            return;
        }

        try
        {
            _inCascadeDelete = true;
            DetectChanges(entry, [entry]);
        }
        finally
        {
            _inCascadeDelete = false;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void DetectChanges(InternalComplexEntry entry)
    {
        if (entry.EntityState == EntityState.Detached)
        {
            return;
        }

        LocalDetectChanges(entry);
    }

    private void DetectChanges(InternalEntityEntry entry, HashSet<InternalEntityEntry> visited)
    {
        if (entry.EntityState == EntityState.Detached)
        {
            return;
        }

        foreach (var foreignKey in entry.EntityType.GetForeignKeys())
        {
            var principalEntry = entry.StateManager.FindPrincipal(entry, foreignKey);

            if (principalEntry != null
                && !visited.Contains(principalEntry))
            {
                visited.Add(principalEntry);

                DetectChanges(principalEntry, visited);
            }
        }

        LocalDetectChanges(entry);
    }

    private bool LocalDetectChanges(InternalEntityEntry entry)
    {
        var changesFound = false;

        var entityType = entry.EntityType;
        if (entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
        {
            return false;
        }

        OnDetectingEntityChanges(entry);

        changesFound |= LocalDetectChanges((InternalEntryBase)entry);

        if (entry.HasRelationshipSnapshot)
        {
            foreach (var navigation in entityType.GetNavigations())
            {
                if (DetectNavigationChange(entry, navigation))
                {
                    changesFound = true;
                }
            }

            foreach (var navigation in entityType.GetSkipNavigations())
            {
                if (DetectNavigationChange(entry, navigation))
                {
                    changesFound = true;
                }
            }
        }

        OnDetectedEntityChanges(entry, changesFound);

        return changesFound;
    }

    private bool LocalDetectChanges(InternalEntryBase entry)
    {
        var changesFound = false;
        foreach (var property in entry.StructuralType.GetFlattenedProperties())
        {
            if (property.GetOriginalValueIndex() >= 0
                && !entry.IsModified(property)
                && !entry.IsConceptualNull(property))
            {
                if (DetectValueChange(entry, property))
                {
                    changesFound = true;
                }
            }

            if (DetectKeyChange(entry, property))
            {
                changesFound = true;
            }
        }

        foreach (var complexProperty in entry.StructuralType.GetComplexProperties())
        {
            if (complexProperty.IsCollection)
            {
                if (DetectComplexCollectionChanges(entry, complexProperty))
                {
                    changesFound = true;
                }
            }
        }

        return changesFound;
    }

    private bool DetectComplexCollectionChanges(InternalEntryBase entry, IComplexProperty complexProperty)
    {
        Check.DebugAssert(complexProperty.IsCollection, $"Expected {complexProperty.Name} to be a collection.");

        if (!entry.HasOriginalValuesSnapshot || complexProperty.GetOriginalValueIndex() < 0)
        {
            foreach (var complexEntry in entry.GetComplexCollectionEntries(complexProperty))
            {
                if (complexEntry != null)
                {
                    LocalDetectChanges(complexEntry);
                }
            }

            return false;
        }

        Check.DebugAssert(!complexProperty.ComplexType.ClrType.IsValueType, $"Expected {complexProperty.Name} to be a collection of reference types.");

        var changesFound = false;
        var originalEntries = new Dictionary<object, InternalComplexEntry>(ReferenceEqualityComparer.Instance);
        var currentCollection = (IList?)entry[complexProperty];
        var originalCollection = (IList?)entry.GetOriginalValue(complexProperty);

        entry.EnsureComplexCollectionEntriesCapacity(complexProperty, currentCollection?.Count ?? 0, originalCollection?.Count ?? 0, trim: false);
        var originalNulls = new HashSet<int>();
        var removed = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
        if (originalCollection != null
            && entry.EntityState != EntityState.Added)
        {
            var originalEntriesList = entry.GetComplexCollectionOriginalEntries(complexProperty);
            Check.DebugAssert(originalEntriesList.Count == originalCollection.Count,
                $"Expected original entries list count {originalEntriesList.Count} to be equal to original collection count {originalCollection.Count}.");

            for (var i = 0; i < originalEntriesList.Count; i++)
            {
                var element = originalCollection[i];
                if (element == null)
                {
                    originalNulls.Add(i);
                    continue;
                }

                removed[element] = i;

                var originalEntry = originalEntriesList[i];
                if (originalEntry == null)
                {
                    continue;
                }

                Check.DebugAssert(originalEntry.OriginalOrdinal == i, $"Expected original ordinal {originalEntry.OriginalOrdinal} to be equal to {i}.");

                originalEntries[element] = originalEntry;
            }
        }

        var currentNulls = new HashSet<int>();
        var added = new List<int>();
        var currentEntries = entry.GetComplexCollectionEntries(complexProperty);
        if (entry.EntityState != EntityState.Added
            && entry.EntityState != EntityState.Deleted
            && currentCollection != null)
        {
            for (var i = 0; i < currentCollection.Count; i++)
            {
                var element = currentCollection[i];
                if (element == null)
                {
                    currentNulls.Add(i);
                    continue;
                }

                if (removed.TryGetAndRemove(element, out int originalOrdinal))
                {
                    // Existing instance found, fix ordinal and check for changes.
                    var originalEntry = originalEntries[element];

                    Check.DebugAssert(originalEntry.OriginalOrdinal == originalOrdinal,
                        $"The OriginalOrdinal for entry of the element at original ordinal {originalOrdinal} is {originalEntry.OriginalOrdinal}.");
                    if (originalEntry.Ordinal != i)
                    {
                        if (originalEntry.EntityState == EntityState.Detached)
                        {
                            originalEntry.Ordinal = i;
                            originalEntry.SetEntityState(EntityState.Unchanged);
                        }
                        else
                        {
                            Check.DebugAssert(originalEntry.Ordinal > i || currentNulls.Contains(originalEntry.Ordinal),
                                $"Expected the entry that was previously at {originalEntry.Ordinal} to have been encountered at an ordinal before {i}.");
                            entry.MoveComplexCollectionEntry(complexProperty, originalEntry.Ordinal, i);
                        }

                        changesFound = true;
                    }

                    if (LocalDetectChanges(originalEntry))
                    {
                        if (originalEntry.EntityState == EntityState.Unchanged)
                        {
                            originalEntry.SetEntityState(EntityState.Modified);
                            changesFound = true;
                        }
                    }
                }
                else
                {
                    // The instance was not found in the original collection, so it could be a replacement or an addition.
                    var currentEntry = entry.GetComplexCollectionEntry(complexProperty, i);
                    if (originalNulls.Remove(currentEntry.OriginalOrdinal))
                    {
                        var nullEntry = entry.GetComplexCollectionOriginalEntry(complexProperty, currentEntry.OriginalOrdinal);
                        if (nullEntry != currentEntry)
                        {
                            currentEntry.SetEntityState(EntityState.Detached);
                        }

                        if (nullEntry.Ordinal == -1)
                        {
                            nullEntry.Ordinal = i;
                        }
                        else
                        {
                            entry.MoveComplexCollectionEntry(complexProperty, nullEntry.Ordinal, i);
                        }

                        nullEntry.SetEntityState(EntityState.Modified);
                        changesFound = true;
                    }
                    else
                    {
                        added.Add(i);
                        if (currentEntry.EntityState is not EntityState.Detached and not EntityState.Added)
                        {
                            // If the element was newly added then there should be a null entry at some ordinal, otherwise it will be treated as a replacement.
                            var nullEntryOrdinal = -1;
                            for (var j = i + 1; j < currentEntries.Count; j++)
                            {
                                if (currentEntries[j] == null)
                                {
                                    nullEntryOrdinal = j;
                                    break;
                                }
                            }

                            if (nullEntryOrdinal != -1)
                            {
                                entry.MoveComplexCollectionEntry(complexProperty, nullEntryOrdinal, i);
                            }
                        }
                    }
                }
            }
        }

        // Try to match up the added entries with the original nulls or removed elements.
        foreach (var addedOrdinal in added)
        {
            var currentEntry = entry.GetComplexCollectionEntry(complexProperty, addedOrdinal);
            if (currentEntry.EntityState is EntityState.Detached or EntityState.Added)
            {
                if (originalNulls.Count > 0)
                {
                    var originalOrdinal = originalNulls.First();
                    originalNulls.Remove(originalOrdinal);
                    currentEntry.OriginalOrdinal = originalOrdinal;
                    currentEntry.SetEntityState(EntityState.Modified);
                }
                else if (removed.Count > 0)
                {
                    var (removedElement, originalOrdinal) = removed.First();
                    removed.Remove(removedElement);
                    currentEntry.OriginalOrdinal = originalOrdinal;
                    currentEntry.SetEntityState(EntityState.Unchanged);
                }
                else
                {
                    currentEntry.SetEntityState(EntityState.Added);
                    continue;
                }
            }
            else if (!originalNulls.Remove(currentEntry.OriginalOrdinal))
            {
                var removedElement = removed.FirstOrDefault(r => r.Value == currentEntry.OriginalOrdinal);
                if (removedElement.Key != null)
                {
                    removed.Remove(removedElement.Key);
                }
                else
                {
                    Check.DebugAssert(false,
                        $"Expected the entry at {addedOrdinal} to have been removed or matched with a null entry. Current state: {currentEntry.EntityState}.");
                }
            }

            if (currentEntry.EntityState == EntityState.Unchanged)
            {
                if (LocalDetectChanges(currentEntry))
                {
                    currentEntry.SetEntityState(EntityState.Modified);
                    changesFound = true;
                }
            }
        }

        if (entry.EntityState != EntityState.Added
            && removed.Count > 0)
        {
            foreach (var (removedElement, originalOrdinal) in removed)
            {
                var originalEntry = originalEntries[removedElement];
                Check.DebugAssert(originalEntry.OriginalOrdinal == originalOrdinal,
                    $"The OriginalOrdinal for entry of the element at original ordinal {originalOrdinal} is {originalEntry.OriginalOrdinal}.");
                var newCurrentOrdinal = originalEntry.Ordinal;
                if (originalEntry.EntityState is EntityState.Unchanged or EntityState.Detached)
                {
                    // Try to match removed elements with nulls to mark the entry as modified
                    if (!currentNulls.Remove(newCurrentOrdinal))
                    {
                        if (currentNulls.Count > 0)
                        {
                            newCurrentOrdinal = currentNulls.First();
                            currentNulls.Remove(newCurrentOrdinal);
                        }
                        else
                        {
                            newCurrentOrdinal = -1;
                        }
                    }
                }
                else
                {
                    newCurrentOrdinal = -1;
                }

                if (newCurrentOrdinal == -1
                    || newCurrentOrdinal >= (currentCollection?.Count ?? 0))
                {
                    // If the are no unmatched nulls left, treat the original entry as deleted
                    originalEntry.SetEntityState(EntityState.Deleted);
                }
                else
                {
                    var existingEntry = entry.GetComplexCollectionEntry(complexProperty, newCurrentOrdinal);
                    if (existingEntry != originalEntry)
                    {
                        existingEntry.SetEntityState(EntityState.Detached);
                    }

                    if (originalEntry.EntityState is EntityState.Deleted or EntityState.Detached)
                    {
                        originalEntry.Ordinal = newCurrentOrdinal;
                        originalEntry.SetEntityState(EntityState.Modified);
                    }
                    else
                    {
                        originalEntry.SetEntityState(EntityState.Modified);
                        entry.MoveComplexCollectionEntry(complexProperty, originalEntry.Ordinal, newCurrentOrdinal);
                    }
                }
            }
            changesFound = true;
        }

        if (originalNulls.Count > 0)
        {
            // If there are any unmatched original nulls left, they should be treated as deleted.
            foreach (var originalNull in originalNulls)
            {
                var nullEntry = entry.GetComplexCollectionOriginalEntry(complexProperty, originalNull);

                Check.DebugAssert(nullEntry.EntityState is EntityState.Unchanged or EntityState.Detached,
                    $"Expected null entry at {originalNull} to be unchanged or detached.");
                nullEntry.SetEntityState(EntityState.Deleted);
                changesFound = true;
            }
        }

        if (currentNulls.Count > 0)
        {
            // If there are any unmatched current nulls left, they should be treated as added.
            foreach (var currentNull in currentNulls)
            {
                var nullEntry = entry.GetComplexCollectionEntry(complexProperty, currentNull);

                Check.DebugAssert(nullEntry.EntityState is EntityState.Unchanged or EntityState.Detached,
                    $"Expected null entry at {currentNull} to be unchanged or detached.");
                nullEntry.SetEntityState(EntityState.Added);
                changesFound = true;
            }
        }

        // Trim excess entries
        entry.EnsureComplexCollectionEntriesCapacity(complexProperty, currentCollection?.Count ?? 0, originalCollection?.Count ?? 0, trim: true);

        if (changesFound)
        {
            entry.SetPropertyModified(complexProperty, true);
        }

        return changesFound;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool DetectValueChange(IInternalEntry entry, IProperty property)
    {
        var current = entry[property];
        var original = entry.GetOriginalValue(property);

        if (!property.GetValueComparer().Equals(current, original))
        {
            if (entry.EntityState == EntityState.Deleted)
            {
                ThrowIfKeyChanged(entry, property);
            }
            else
            {
                LogChangeDetected(entry, property, original, current);
                entry.SetPropertyModified(property);
                return true;
            }
        }

        return false;
    }

    private void LogChangeDetected(IInternalEntry entry, IProperty property, object? original, object? current)
    {
        if (_loggingOptions.IsSensitiveDataLoggingEnabled)
        {
            if (entry is InternalEntityEntry entityEntry)
            {
                _logger.PropertyChangeDetectedSensitive(entityEntry, property, original, current);
            }
            else
            {
                _logger.ComplexTypePropertyChangeDetectedSensitive((InternalComplexEntry)entry, property, original, current);
            }
        }
        else
        {
            if (entry is InternalEntityEntry entityEntry)
            {
                _logger.PropertyChangeDetected(entityEntry, property, original, current);
            }
            else
            {
                _logger.ComplexTypePropertyChangeDetected((InternalComplexEntry)entry, property, original, current);
            }
        }
    }

    private bool DetectKeyChange(IInternalEntry entry, IProperty property)
    {
        if (property.GetRelationshipIndex() < 0)
        {
            return false;
        }

        var entityEntry = entry as InternalEntityEntry ?? throw new UnreachableException("Complex type entry with a navigation");
        var snapshotValue = entityEntry.GetRelationshipSnapshotValue(property);
        var currentValue = entityEntry[property];

        var comparer = property.GetKeyValueComparer();

        // Note that mutation of a byte[] key is not supported or detected, but two different instances
        // of byte[] with the same content must be detected as equal.
        if (!comparer.Equals(currentValue, snapshotValue))
        {
            var keys = property.GetContainingKeys();
            var foreignKeys = property.GetContainingForeignKeys()
                .Where(fk => fk.DeclaringEntityType.IsAssignableFrom(entityEntry.EntityType));

            if (_loggingOptions.IsSensitiveDataLoggingEnabled)
            {
                _logger.ForeignKeyChangeDetectedSensitive(entityEntry, property, snapshotValue, currentValue);
            }
            else
            {
                _logger.ForeignKeyChangeDetected(entityEntry, property, snapshotValue, currentValue);
            }

            entityEntry.StateManager.InternalEntityEntryNotifier.KeyPropertyChanged(
                entityEntry, property, keys, foreignKeys, snapshotValue, currentValue);

            return true;
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool DetectNavigationChange(InternalEntityEntry entry, INavigationBase navigationBase)
    {
        var snapshotValue = entry.GetRelationshipSnapshotValue(navigationBase);
        var currentValue = entry[navigationBase];
        var stateManager = entry.StateManager;

        if (navigationBase.IsCollection)
        {
            var snapshotCollection = (IEnumerable?)snapshotValue;
            var currentCollection = (IEnumerable?)currentValue;

            var removed = new HashSet<object>(ReferenceEqualityComparer.Instance);
            if (snapshotCollection != null)
            {
                foreach (var entity in snapshotCollection)
                {
                    removed.Add(entity);
                }
            }

            var added = new HashSet<object>(ReferenceEqualityComparer.Instance);
            if (currentCollection != null)
            {
                foreach (var entity in currentCollection)
                {
                    if (!removed.Remove(entity))
                    {
                        added.Add(entity);
                    }
                }
            }

            if (added.Count > 0
                || removed.Count > 0)
            {
                if (_loggingOptions.IsSensitiveDataLoggingEnabled)
                {
                    if (navigationBase is INavigation navigation)
                    {
                        _logger.CollectionChangeDetectedSensitive(entry, navigation, added, removed);
                    }
                    else if (navigationBase is ISkipNavigation skipNavigation)
                    {
                        _logger.SkipCollectionChangeDetectedSensitive(entry, skipNavigation, added, removed);
                    }
                }
                else
                {
                    if (navigationBase is INavigation navigation)
                    {
                        _logger.CollectionChangeDetected(entry, navigation, added, removed);
                    }
                    else if (navigationBase is ISkipNavigation skipNavigation)
                    {
                        _logger.SkipCollectionChangeDetected(entry, skipNavigation, added, removed);
                    }
                }

                stateManager.InternalEntityEntryNotifier.NavigationCollectionChanged(entry, navigationBase, added, removed);

                return true;
            }

            return false;
        }

        if (!ReferenceEquals(currentValue, snapshotValue))
        {
            Check.DebugAssert(navigationBase is INavigation, "Issue #21673. Non-collection skip navigations not supported.");

            var navigation = (INavigation)navigationBase;
            if (_loggingOptions.IsSensitiveDataLoggingEnabled)
            {
                _logger.ReferenceChangeDetectedSensitive(entry, navigation, snapshotValue, currentValue);
            }
            else
            {
                _logger.ReferenceChangeDetected(entry, navigation, snapshotValue, currentValue);
            }

            stateManager.InternalEntityEntryNotifier.NavigationReferenceChanged(entry, navigation, snapshotValue, currentValue);

            return true;
        }

        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (EventHandler<DetectChangesEventArgs>? DetectingAllChanges,
        EventHandler<DetectedChangesEventArgs>? DetectedAllChanges,
        EventHandler<DetectEntityChangesEventArgs>? DetectingEntityChanges,
        EventHandler<DetectedEntityChangesEventArgs>? DetectedEntityChanges) CaptureEvents()
        => (DetectingAllChanges, DetectedAllChanges, DetectingEntityChanges, DetectedEntityChanges);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetEvents(
        EventHandler<DetectChangesEventArgs>? detectingAllChanges,
        EventHandler<DetectedChangesEventArgs>? detectedAllChanges,
        EventHandler<DetectEntityChangesEventArgs>? detectingEntityChanges,
        EventHandler<DetectedEntityChangesEventArgs>? detectedEntityChanges)
    {
        DetectingAllChanges = detectingAllChanges;
        DetectedAllChanges = detectedAllChanges;
        DetectingEntityChanges = detectingEntityChanges;
        DetectedEntityChanges = detectedEntityChanges;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<DetectEntityChangesEventArgs>? DetectingEntityChanges;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnDetectingEntityChanges(InternalEntityEntry internalEntityEntry)
    {
        var @event = DetectingEntityChanges;
        if (@event != null)
        {
            var changeTracker = internalEntityEntry.StateManager.Context.ChangeTracker;
            var detectChangesEnabled = changeTracker.AutoDetectChangesEnabled;
            try
            {
                changeTracker.AutoDetectChangesEnabled = false;
                @event(changeTracker, new DetectEntityChangesEventArgs(internalEntityEntry));
            }
            finally
            {
                changeTracker.AutoDetectChangesEnabled = detectChangesEnabled;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<DetectChangesEventArgs>? DetectingAllChanges;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnDetectingAllChanges(IStateManager stateManager)
    {
        var @event = DetectingAllChanges;

        if (@event != null)
        {
            var changeTracker = stateManager.Context.ChangeTracker;
            var detectChangesEnabled = changeTracker.AutoDetectChangesEnabled;
            try
            {
                changeTracker.AutoDetectChangesEnabled = false;
                @event(changeTracker, new DetectChangesEventArgs());
            }
            finally
            {
                changeTracker.AutoDetectChangesEnabled = detectChangesEnabled;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<DetectedEntityChangesEventArgs>? DetectedEntityChanges;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnDetectedEntityChanges(InternalEntityEntry internalEntityEntry, bool changesFound)
    {
        var @event = DetectedEntityChanges;

        if (@event != null)
        {
            var changeTracker = internalEntityEntry.StateManager.Context.ChangeTracker;
            var detectChangesEnabled = changeTracker.AutoDetectChangesEnabled;
            try
            {
                changeTracker.AutoDetectChangesEnabled = false;
                @event(changeTracker, new DetectedEntityChangesEventArgs(internalEntityEntry, changesFound));
            }
            finally
            {
                changeTracker.AutoDetectChangesEnabled = detectChangesEnabled;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<DetectedChangesEventArgs>? DetectedAllChanges;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnDetectedAllChanges(IStateManager stateManager, bool changesFound)
    {
        var @event = DetectedAllChanges;

        if (@event != null)
        {
            var changeTracker = stateManager.Context.ChangeTracker;
            var detectChangesEnabled = changeTracker.AutoDetectChangesEnabled;
            try
            {
                changeTracker.AutoDetectChangesEnabled = false;
                @event(changeTracker, new DetectedChangesEventArgs(changesFound));
            }
            finally
            {
                changeTracker.AutoDetectChangesEnabled = detectChangesEnabled;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ResetState()
    {
        DetectingEntityChanges = null;
        DetectedEntityChanges = null;
        DetectingAllChanges = null;
        DetectedAllChanges = null;
    }
}
