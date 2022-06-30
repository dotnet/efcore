// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.EntityFrameworkCore.Internal;
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
    public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase propertyBase, bool setModified)
    {
        if (entry.EntityState == EntityState.Detached
            || propertyBase is IServiceProperty)
        {
            return;
        }

        if (propertyBase is IProperty property)
        {
            if (entry.EntityState != EntityState.Deleted)
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
            DetectNavigationChange(entry, navigation);
        }
    }

    private static void ThrowIfKeyChanged(InternalEntityEntry entry, IProperty property)
    {
        if (property.IsKey()
            && property.GetAfterSaveBehavior() == PropertySaveBehavior.Throw)
        {
            throw new InvalidOperationException(CoreStrings.KeyReadOnly(property.Name, entry.EntityType.DisplayName()));
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void PropertyChanging(InternalEntityEntry entry, IPropertyBase propertyBase)
    {
        if (entry.EntityState == EntityState.Detached
            || propertyBase is IServiceProperty)
        {
            return;
        }

        if (!entry.EntityType.UseEagerSnapshots())
        {
            if (propertyBase is IProperty asProperty
                && asProperty.GetOriginalValueIndex() != -1)
            {
                entry.EnsureOriginalValues();
            }

            if (propertyBase.GetRelationshipIndex() != -1)
            {
                entry.EnsureRelationshipSnapshot();
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
        OnDetectingChanges(stateManager);
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
        
        OnDetectedChanges(stateManager, changesFound);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void DetectChanges(InternalEntityEntry entry)
    {
        OnDetectingChanges(entry);
        OnDetectedChanges(entry, DetectChanges(entry, new HashSet<InternalEntityEntry> { entry }));
    }

    private bool DetectChanges(InternalEntityEntry entry, HashSet<InternalEntityEntry> visited)
    {
        var changesFound = false;

        if (entry.EntityState != EntityState.Detached)
        {
            foreach (var foreignKey in entry.EntityType.GetForeignKeys())
            {
                var principalEntry = entry.StateManager.FindPrincipal(entry, foreignKey);

                if (principalEntry != null
                    && !visited.Contains(principalEntry))
                {
                    visited.Add(principalEntry);

                    if (DetectChanges(principalEntry, visited))
                    {
                        changesFound = true;
                    }
                }
            }

            if (LocalDetectChanges(entry))
            {
                changesFound = true;
            }
        }

        return changesFound;
    }

    private bool LocalDetectChanges(InternalEntityEntry entry)
    {
        var changesFound = false;

        var entityType = entry.EntityType;
        if (entityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
        {
            return false;
        }

        foreach (var property in entityType.GetProperties())
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

        return changesFound;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public bool DetectValueChange(InternalEntityEntry entry, IProperty property)
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

    private void LogChangeDetected(InternalEntityEntry entry, IProperty property, object? original, object? current)
    {
        if (_loggingOptions.IsSensitiveDataLoggingEnabled)
        {
            _logger.PropertyChangeDetectedSensitive(entry, property, original, current);
        }
        else
        {
            _logger.PropertyChangeDetected(entry, property, original, current);
        }
    }

    private bool DetectKeyChange(InternalEntityEntry entry, IProperty property)
    {
        if (property.GetRelationshipIndex() < 0)
        {
            return false;
        }

        var snapshotValue = entry.GetRelationshipSnapshotValue(property);
        var currentValue = entry[property];

        var comparer = property.GetKeyValueComparer();

        // Note that mutation of a byte[] key is not supported or detected, but two different instances
        // of byte[] with the same content must be detected as equal.
        if (!comparer.Equals(currentValue, snapshotValue))
        {
            var keys = property.GetContainingKeys();
            var foreignKeys = property.GetContainingForeignKeys()
                .Where(fk => fk.DeclaringEntityType.IsAssignableFrom(entry.EntityType));

            if (_loggingOptions.IsSensitiveDataLoggingEnabled)
            {
                _logger.ForeignKeyChangeDetectedSensitive(entry, property, snapshotValue, currentValue);
            }
            else
            {
                _logger.ForeignKeyChangeDetected(entry, property, snapshotValue, currentValue);
            }

            entry.StateManager.InternalEntityEntryNotifier.KeyPropertyChanged(
                entry, property, keys, foreignKeys, snapshotValue, currentValue);

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

            var removed = new HashSet<object>(LegacyReferenceEqualityComparer.Instance);
            if (snapshotCollection != null)
            {
                foreach (var entity in snapshotCollection)
                {
                    removed.Add(entity);
                }
            }

            var added = new HashSet<object>(LegacyReferenceEqualityComparer.Instance);
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
    public virtual (EventHandler<DetectChangesEventArgs>? DetectingChanges,
        EventHandler<DetectedChangesEventArgs>? DetectedChanges) CaptureEvents()
        => (DetectingChanges, DetectedChanges);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void SetEvents(
        EventHandler<DetectChangesEventArgs>? detectingChanges,
        EventHandler<DetectedChangesEventArgs>? detectedChanges)
    {
        DetectingChanges = detectingChanges;
        DetectedChanges = detectedChanges;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<DetectChangesEventArgs>? DetectingChanges;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnDetectingChanges(InternalEntityEntry internalEntityEntry)
    {
        var @event = DetectingChanges;

        @event?.Invoke(
            internalEntityEntry.StateManager.Context.ChangeTracker, 
            new DetectChangesEventArgs(internalEntityEntry));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnDetectingChanges(IStateManager stateManager)
    {
        var @event = DetectingChanges;

        @event?.Invoke(
            stateManager.Context.ChangeTracker, 
            new DetectChangesEventArgs(null));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public event EventHandler<DetectedChangesEventArgs>? DetectedChanges;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnDetectedChanges(InternalEntityEntry internalEntityEntry, bool changesFound)
    {
        var @event = DetectedChanges;

        @event?.Invoke(
            internalEntityEntry.StateManager.Context.ChangeTracker, 
            new DetectedChangesEventArgs(internalEntityEntry, changesFound));
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void OnDetectedChanges(IStateManager stateManager, bool changesFound)
    {
        var @event = DetectedChanges;

        @event?.Invoke(
            stateManager.Context.ChangeTracker, 
            new DetectedChangesEventArgs(null, changesFound));
    }
    
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void ResetState()
    {
        DetectingChanges = null;
        DetectedChanges = null;
    }
}
