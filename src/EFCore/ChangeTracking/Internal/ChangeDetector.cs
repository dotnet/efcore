// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class ChangeDetector : IChangeDetector
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> _logger;
        private readonly ILoggingOptions _loggingOptions;
        private bool _suspended;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ChangeDetector(
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
            [NotNull] ILoggingOptions loggingOptions)
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
        public virtual void Suspend()
            => _suspended = true;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Resume()
            => _suspended = false;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void DetectChanges(IStateManager stateManager)
        {
            _logger.DetectChangesStarting(stateManager.Context);

            foreach (var entry in stateManager.ToList()) // Might be too big, but usually _all_ entities are using Snapshot tracking
            {
                if (entry.EntityState != EntityState.Detached)
                {
                    LocalDetectChanges(entry);
                }
            }

            _logger.DetectChangesCompleted(stateManager.Context);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void DetectChanges(InternalEntityEntry entry)
            => DetectChanges(entry, new HashSet<InternalEntityEntry> { entry });

        private void DetectChanges(InternalEntityEntry entry, HashSet<InternalEntityEntry> visited)
        {
            if (entry.EntityState != EntityState.Detached)
            {
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
        }

        private void LocalDetectChanges(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;

            if (entry.EntityType.GetChangeTrackingStrategy() != ChangeTrackingStrategy.Snapshot)
            {
                return;
            }

            foreach (var property in entityType.GetProperties())
            {
                if (property.GetOriginalValueIndex() >= 0
                    && !entry.IsModified(property)
                    && !entry.IsConceptualNull(property))
                {
                    var current = entry[property];
                    var original = entry.GetOriginalValue(property);

                    var comparer = property.GetValueComparer();

                    if (comparer == null)
                    {
                        if (!Equals(current, original))
                        {
                            SetPropertyModified();
                        }
                    }
                    else if (!comparer.Equals(current, original))
                    {
                        SetPropertyModified();
                    }

                    void SetPropertyModified()
                    {
                        if (entry.EntityState == EntityState.Deleted)
                        {
                            ThrowIfKeyChanged(entry, property);
                        }
                        else
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

                foreach (var navigation in entityType.GetSkipNavigations())
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
            if (property.GetRelationshipIndex() < 0)
            {
                return;
            }

            var snapshotValue = entry.GetRelationshipSnapshotValue(property);
            var currentValue = entry[property];

            var comparer = property.GetKeyValueComparer();

            // Note that mutation of a byte[] key is not supported or detected, but two different instances
            // of byte[] with the same content must be detected as equal.
            if (!(comparer?.Equals(currentValue, snapshotValue)
                ?? StructuralComparisons.StructuralEqualityComparer.Equals(currentValue, snapshotValue)))
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
            }
        }

        private void DetectNavigationChange(InternalEntityEntry entry, INavigationBase navigationBase)
        {
            var snapshotValue = entry.GetRelationshipSnapshotValue(navigationBase);
            var currentValue = entry[navigationBase];
            var stateManager = entry.StateManager;

            if (navigationBase.IsCollection)
            {
                var snapshotCollection = (IEnumerable)snapshotValue;
                var currentCollection = (IEnumerable)currentValue;

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
                }
            }
            else if (!ReferenceEquals(currentValue, snapshotValue))
            {
                Check.DebugAssert(navigationBase is INavigation, "Issue #21673. Non-collection skip navigations not supported.");

                var navigation = (INavigation)navigationBase;
                if (!navigation.ForeignKey.IsOwnership
                    || !navigation.IsOnDependent)
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
}
