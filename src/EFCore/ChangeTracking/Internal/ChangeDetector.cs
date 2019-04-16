// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ChangeDetector : IChangeDetector
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> _logger;
        private readonly ILoggingOptions _loggingOptions;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string SkipDetectChangesAnnotation = "ChangeDetector.SkipDetectChanges";

        private bool _suspended;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ChangeDetector(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
            [NotNull] ILoggingOptions loggingOptions)
        {
            _logger = logger;
            _loggingOptions = loggingOptions;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Suspend() => _suspended = true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Resume() => _suspended = false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void PropertyChanged(InternalEntityEntry entry, IPropertyBase propertyBase, bool setModified)
        {
            if (_suspended
                || entry.EntityState == EntityState.Detached
                || propertyBase is IServiceProperty)
            {
                return;
            }

            if (propertyBase is IProperty property)
            {
                entry.SetPropertyModified(property, setModified);

                if (property.GetRelationshipIndex() != -1)
                {
                    DetectKeyChange(entry, property);
                }
            }
            else if (propertyBase.GetRelationshipIndex() != -1
                     && propertyBase is INavigation navigation)
            {
                DetectNavigationChange(entry, navigation);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void PropertyChanging(InternalEntityEntry entry, IPropertyBase propertyBase)
        {
            if (_suspended
                || entry.EntityState == EntityState.Detached
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
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void DetectChanges(IStateManager stateManager)
        {
            _logger.DetectChangesStarting(stateManager.Context);

            foreach (var entry in stateManager.Entries.Where(
                e => e.EntityState != EntityState.Detached
                     && e.EntityType.GetChangeTrackingStrategy() == ChangeTrackingStrategy.Snapshot).ToList())
            {
                // State might change while detecting changes on other entries
                if (entry.EntityState != EntityState.Detached)
                {
                    DetectChanges(entry);
                }
            }

            _logger.DetectChangesCompleted(stateManager.Context);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void DetectChanges(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            foreach (var property in entityType.GetProperties())
            {
                if (property.GetOriginalValueIndex() >= 0
                    && !entry.IsModified(property)
                    && !entry.IsConceptualNull(property))
                {
                    var current = entry[property];
                    var original = entry.GetOriginalValue(property);

                    var comparer = property.GetValueComparer() ?? property.FindMapping()?.Comparer;

                    if (comparer == null)
                    {
                        if (!Equals(current, original))
                        {
                            LogChangeDetected(entry, property, original, current);
                            entry.SetPropertyModified(property);
                        }
                    }
                    else
                    {
                        if (!comparer.Equals(current, original))
                        {
                            LogChangeDetected(entry, property, original, current);
                            entry.SetPropertyModified(property);
                        }
                    }
                }
            }

            foreach (var property in entityType.GetProperties())
            {
                DetectKeyChange(entry, property);
            }

            if (entry.HasRelationshipSnapshot)
            {
                foreach (var navigation in entityType.GetNavigations())
                {
                    DetectNavigationChange(entry, navigation);
                }
            }
        }

        private void LogChangeDetected(InternalEntityEntry entry, IProperty property, object original, object current)
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

        private void DetectKeyChange(InternalEntityEntry entry, IProperty property)
        {
            if (property.GetRelationshipIndex() >= 0)
            {
                var snapshotValue = entry.GetRelationshipSnapshotValue(property);
                var currentValue = entry[property];

                var comparer = property.GetKeyValueComparer()
                               ?? property.FindMapping()?.KeyComparer;

                // Note that mutation of a byte[] key is not supported or detected, but two different instances
                // of byte[] with the same content must be detected as equal.
                if (!(comparer?.Equals(currentValue, snapshotValue)
                      ?? StructuralComparisons.StructuralEqualityComparer.Equals(currentValue, snapshotValue)))
                {
                    var keys = property.GetContainingKeys().ToList();
                    var foreignKeys = property.GetContainingForeignKeys()
                        .Where(fk => fk.DeclaringEntityType.IsAssignableFrom(entry.EntityType)).ToList();

                    if (_loggingOptions.IsSensitiveDataLoggingEnabled)
                    {
                        _logger.ForeignKeyChangeDetectedSensitive(entry, property, snapshotValue, currentValue);
                    }
                    else
                    {
                        _logger.ForeignKeyChangeDetected(entry, property, snapshotValue, currentValue);
                    }

                    entry.StateManager.InternalEntityEntryNotifier.KeyPropertyChanged(entry, property, keys, foreignKeys, snapshotValue, currentValue);
                }
            }
        }

        private void DetectNavigationChange(InternalEntityEntry entry, INavigation navigation)
        {
            var snapshotValue = entry.GetRelationshipSnapshotValue(navigation);
            var currentValue = entry[navigation];
            var stateManager = entry.StateManager;

            if (navigation.IsCollection())
            {
                var snapshotCollection = (IEnumerable)snapshotValue;
                var currentCollection = (IEnumerable)currentValue;

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
                        _logger.CollectionChangeDetectedSensitive(entry, navigation, added, removed);
                    }
                    else
                    {
                        _logger.CollectionChangeDetected(entry, navigation, added, removed);
                    }

                    stateManager.InternalEntityEntryNotifier.NavigationCollectionChanged(entry, navigation, added, removed);
                }
            }
            else if (!ReferenceEquals(currentValue, snapshotValue)
                     && (!navigation.ForeignKey.IsOwnership
                         || !navigation.IsDependentToPrincipal()))
            {
                if (_loggingOptions.IsSensitiveDataLoggingEnabled)
                {
                    _logger.ReferenceChangeDetectedSensitive(entry, navigation, snapshotValue, currentValue);
                }
                else
                {
                    _logger.ReferenceChangeDetected(entry, navigation, snapshotValue, currentValue);
                }

                stateManager.InternalEntityEntryNotifier.NavigationReferenceChanged(entry, navigation, snapshotValue, currentValue);
            }
        }
    }
}
