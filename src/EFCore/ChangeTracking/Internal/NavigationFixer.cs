// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NavigationFixer : INavigationFixer
    {
        private readonly IChangeDetector _changeDetector;
        private readonly IEntityGraphAttacher _attacher;
        private readonly bool _sensitiveLoggingEnabled;
        private bool _inFixup;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NavigationFixer(
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IEntityGraphAttacher attacher,
            [NotNull] ILoggingOptions loggingOptions)
        {
            _changeDetector = changeDetector;
            _attacher = attacher;
            if (loggingOptions.IsSensitiveDataLoggingEnabled)
            {
                _sensitiveLoggingEnabled = true;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void NavigationReferenceChanged(InternalEntityEntry entry, INavigation navigation, object oldValue, object newValue)
        {
            if (_inFixup)
            {
                return;
            }

            var foreignKey = navigation.ForeignKey;
            var stateManager = entry.StateManager;
            var inverse = navigation.FindInverse();
            var targetEntityType = navigation.GetTargetType();

            var oldTargetEntry = oldValue == null ? null : stateManager.TryGetEntry(oldValue, targetEntityType);
            if (oldTargetEntry?.EntityState == EntityState.Detached)
            {
                oldTargetEntry = null;
            }

            var newTargetEntry = newValue == null ? null : stateManager.TryGetEntry(newValue, targetEntityType);
            if (newTargetEntry?.EntityState == EntityState.Detached)
            {
                newTargetEntry = null;
            }

            try
            {
                _inFixup = true;

                if (navigation.IsDependentToPrincipal())
                {
                    if (newValue != null)
                    {
                        if (newTargetEntry != null)
                        {
                            if (foreignKey.IsUnique)
                            {
                                // Navigation points to principal. Find the dependent that previously pointed to that principal and
                                // null out its FKs and navigation property. A.k.a. reference stealing.
                                // However, if the FK has already been changed or the reference is already set to point
                                // to something else, then don't change it.
                                var victimDependentEntry = stateManager.GetDependents(newTargetEntry, foreignKey).FirstOrDefault();
                                if (victimDependentEntry != null
                                    && victimDependentEntry != entry)
                                {
                                    ConditionallyNullForeignKeyProperties(victimDependentEntry, newTargetEntry, foreignKey);

                                    if (ReferenceEquals(victimDependentEntry[navigation], newTargetEntry.Entity)
                                        && victimDependentEntry.StateManager
                                            .TryGetEntry(victimDependentEntry.Entity, targetEntityType) != null)
                                    {
                                        SetNavigation(victimDependentEntry, navigation, null);
                                    }
                                }
                            }

                            // Set the FK properties to reflect the change to the navigation.
                            SetForeignKeyProperties(entry, newTargetEntry, foreignKey, setModified: true);
                        }
                    }
                    else
                    {
                        // Null the FK properties to reflect that the navigation has been nulled out.
                        ConditionallyNullForeignKeyProperties(entry, oldTargetEntry, foreignKey);
                        entry.SetRelationshipSnapshotValue(navigation, null);
                    }

                    if (inverse != null)
                    {
                        // Set the inverse reference or add the entity to the inverse collection
                        if (newTargetEntry != null)
                        {
                            SetReferenceOrAddToCollection(newTargetEntry, inverse, entry);
                        }

                        // Remove the entity from the old collection, or null the old inverse unless it was already
                        // changed to point to something else
                        if (oldTargetEntry != null
                            && oldTargetEntry.EntityState != EntityState.Deleted)
                        {
                            ResetReferenceOrRemoveCollection(oldTargetEntry, inverse, entry);
                        }
                    }
                }
                else
                {
                    Debug.Assert(foreignKey.IsUnique);

                    if (oldTargetEntry != null)
                    {
                        // Null the FK properties on the old dependent, unless they have already been changed
                        ConditionallyNullForeignKeyProperties(oldTargetEntry, entry, foreignKey);

                        // Clear the inverse reference, unless it has already been changed
                        if (inverse != null
                            && ReferenceEquals(oldTargetEntry[inverse], entry.Entity)
                            && (!oldTargetEntry.EntityType.HasDefiningNavigation()
                                || entry.EntityType.GetNavigations().All(
                                    n => n == navigation
                                         || !ReferenceEquals(oldTargetEntry.Entity, entry[n]))))
                        {
                            SetNavigation(oldTargetEntry, inverse, null);
                        }
                    }

                    if (newTargetEntry != null)
                    {
                        // Navigation points to dependent and is 1:1. Find the principal that previously pointed to that
                        // dependent and null out its navigation property. A.k.a. reference stealing.
                        // However, if the reference is already set to point to something else, then don't change it.
                        var victimPrincipalEntry = stateManager.GetPrincipal(newTargetEntry, foreignKey);
                        if (victimPrincipalEntry != null
                            && victimPrincipalEntry != entry
                            && ReferenceEquals(victimPrincipalEntry[navigation], newTargetEntry.Entity))
                        {
                            SetNavigation(victimPrincipalEntry, navigation, null);
                        }

                        SetForeignKeyProperties(newTargetEntry, entry, foreignKey, setModified: true);

                        SetNavigation(newTargetEntry, inverse, entry);
                    }
                }

                if (newValue == null)
                {
                    entry.SetIsLoaded(navigation, loaded: false);
                }
            }
            finally
            {
                _inFixup = false;
            }

            if (newValue != null
                && newTargetEntry == null)
            {
                stateManager.RecordReferencedUntrackedEntity(newValue, navigation, entry);
                entry.SetRelationshipSnapshotValue(navigation, newValue);
                newTargetEntry = targetEntityType.HasDefiningNavigation()
                    ? stateManager.GetOrCreateEntry(newValue, targetEntityType)
                    : stateManager.GetOrCreateEntry(newValue);
                _attacher.AttachGraph(newTargetEntry, EntityState.Added, forceStateWhenUnknownKey: false);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void NavigationCollectionChanged(
            InternalEntityEntry entry,
            INavigation navigation,
            IEnumerable<object> added,
            IEnumerable<object> removed)
        {
            if (_inFixup)
            {
                return;
            }

            var foreignKey = navigation.ForeignKey;
            var stateManager = entry.StateManager;
            var inverse = navigation.FindInverse();
            var targetEntityType = navigation.GetTargetType();

            foreach (var oldValue in removed)
            {
                var oldTargetEntry = stateManager.TryGetEntry(oldValue, targetEntityType);

                if (oldTargetEntry != null
                    && oldTargetEntry.EntityState != EntityState.Detached)
                {
                    try
                    {
                        _inFixup = true;

                        // Null FKs and navigations of dependents that have been removed, unless they
                        // have already been changed.
                        ConditionallyNullForeignKeyProperties(oldTargetEntry, entry, foreignKey);

                        if (inverse != null
                            && ReferenceEquals(oldTargetEntry[inverse], entry.Entity)
                            && oldTargetEntry.StateManager.TryGetEntry(oldTargetEntry.Entity, targetEntityType) != null)
                        {
                            SetNavigation(oldTargetEntry, inverse, null);
                        }

                        entry.RemoveFromCollectionSnapshot(navigation, oldValue);
                    }
                    finally
                    {
                        _inFixup = false;
                    }
                }
            }

            foreach (var newValue in added)
            {
                var newTargetEntry = targetEntityType.HasDefiningNavigation()
                    ? stateManager.GetOrCreateEntry(newValue, targetEntityType)
                    : stateManager.GetOrCreateEntry(newValue);

                if (newTargetEntry.EntityState != EntityState.Detached)
                {
                    try
                    {
                        _inFixup = true;

                        // For a dependent added to the collection, remove it from the collection of
                        // the principal entity that it was previously part of
                        var oldPrincipalEntry = stateManager.GetPrincipalUsingRelationshipSnapshot(newTargetEntry, foreignKey);
                        if (oldPrincipalEntry != null
                            && oldPrincipalEntry != entry)
                        {
                            RemoveFromCollection(oldPrincipalEntry, navigation, newTargetEntry);
                        }

                        // Set the FK properties on added dependents to match this principal
                        SetForeignKeyProperties(newTargetEntry, entry, foreignKey, setModified: true);

                        // Set the inverse navigation to point to this principal
                        SetNavigation(newTargetEntry, inverse, entry);
                    }
                    finally
                    {
                        _inFixup = false;
                    }
                }
                else
                {
                    stateManager.RecordReferencedUntrackedEntity(newValue, navigation, entry);
                    _attacher.AttachGraph(newTargetEntry, EntityState.Added, forceStateWhenUnknownKey: false);
                }

                entry.AddToCollectionSnapshot(navigation, newValue);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void KeyPropertyChanged(
            InternalEntityEntry entry,
            IProperty property,
            IReadOnlyList<IKey> containingPrincipalKeys,
            IReadOnlyList<IForeignKey> containingForeignKeys,
            object oldValue,
            object newValue)
        {
            if (entry.EntityState == EntityState.Detached)
            {
                return;
            }

            try
            {
                _inFixup = true;

                var stateManager = entry.StateManager;

                foreach (var foreignKey in containingForeignKeys)
                {
                    var newPrincipalEntry = stateManager.GetPrincipal(entry, foreignKey)
                                            ?? stateManager.GetPrincipalUsingPreStoreGeneratedValues(entry, foreignKey);
                    var oldPrincipalEntry = stateManager.GetPrincipalUsingRelationshipSnapshot(entry, foreignKey);

                    var principalToDependent = foreignKey.PrincipalToDependent;
                    if (principalToDependent != null)
                    {
                        if (oldPrincipalEntry != null
                            && oldPrincipalEntry.EntityState != EntityState.Deleted)
                        {
                            // Remove this entity from the principal collection that it was previously part of,
                            // or null the navigation for a 1:1 unless that reference was already changed.
                            ResetReferenceOrRemoveCollection(oldPrincipalEntry, principalToDependent, entry);
                        }

                        if (newPrincipalEntry != null
                            && !entry.IsConceptualNull(property))
                        {
                            // Add this entity to the collection of the new principal, or set the navigation for a 1:1
                            SetReferenceOrAddToCollection(newPrincipalEntry, principalToDependent, entry);
                        }
                    }

                    var dependentToPrincipal = foreignKey.DependentToPrincipal;
                    if (dependentToPrincipal != null)
                    {
                        if (newPrincipalEntry != null)
                        {
                            if (foreignKey.IsUnique)
                            {
                                // Dependent has been changed to point to a new principal.
                                // Find the dependent that previously pointed to the new principal and null out its FKs
                                // and navigation property. A.k.a. reference stealing.
                                // However, if the FK has already been changed or the reference is already set to point
                                // to something else, then don't change it.
                                var targetDependentEntry
                                    = stateManager.GetDependentsUsingRelationshipSnapshot(newPrincipalEntry, foreignKey).FirstOrDefault();

                                if (targetDependentEntry != null
                                    && targetDependentEntry != entry)
                                {
                                    ConditionallyNullForeignKeyProperties(targetDependentEntry, newPrincipalEntry, foreignKey);

                                    if (ReferenceEquals(targetDependentEntry[dependentToPrincipal], newPrincipalEntry.Entity)
                                        && targetDependentEntry.StateManager.TryGetEntry(targetDependentEntry.Entity, foreignKey.DeclaringEntityType) != null)
                                    {
                                        SetNavigation(targetDependentEntry, dependentToPrincipal, null);
                                    }
                                }
                            }

                            if (!entry.IsConceptualNull(property))
                            {
                                SetNavigation(entry, dependentToPrincipal, newPrincipalEntry);
                            }
                        }
                        else if (oldPrincipalEntry != null)
                        {
                            if (ReferenceEquals(entry[dependentToPrincipal], oldPrincipalEntry.Entity)
                                && entry.StateManager.TryGetEntry(entry.Entity, foreignKey.DeclaringEntityType) != null)
                            {
                                SetNavigation(entry, dependentToPrincipal, null);
                            }
                        }
                        else
                        {
                            if (entry[dependentToPrincipal] == null
                                && entry.StateManager.TryGetEntry(entry.Entity, foreignKey.DeclaringEntityType) != null)
                            {
                                // FK has changed but navigation is still null
                                entry.SetIsLoaded(dependentToPrincipal, false);
                            }
                        }
                    }

                    stateManager.UpdateDependentMap(entry, foreignKey);
                }

                foreach (var key in containingPrincipalKeys)
                {
                    stateManager.UpdateIdentityMap(entry, key);

                    // Propagate principal key values into FKs
                    foreach (var foreignKey in key.GetReferencingForeignKeys())
                    {
                        // Fix up dependents that have been added by propagating through different foreign key
                        foreach (var dependentEntry in stateManager.GetDependents(entry, foreignKey).ToList())
                        {
                            var principalToDependent = foreignKey.PrincipalToDependent;
                            if (principalToDependent != null)
                            {
                                if (!entry.IsConceptualNull(property))
                                {
                                    // Add this entity to the collection of the new principal, or set the navigation for a 1:1
                                    SetReferenceOrAddToCollection(entry, principalToDependent, dependentEntry);
                                }
                            }

                            var dependentToPrincipal = foreignKey.DependentToPrincipal;
                            if (dependentToPrincipal != null)
                            {
                                if (!entry.IsConceptualNull(property))
                                {
                                    SetNavigation(dependentEntry, dependentToPrincipal, entry);
                                }
                            }
                        }

                        foreach (var dependentEntry in stateManager.GetDependentsUsingRelationshipSnapshot(entry, foreignKey).ToList())
                        {
                            SetForeignKeyProperties(dependentEntry, entry, foreignKey, setModified: true);
                        }
                    }
                }

                entry.SetRelationshipSnapshotValue(property, newValue);
            }
            finally
            {
                _inFixup = false;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void TrackedFromQuery(
            InternalEntityEntry entry,
            ISet<IForeignKey> handledForeignKeys)
        {
            try
            {
                _inFixup = true;

                InitialFixup(entry, handledForeignKeys, fromQuery: true);
            }
            finally
            {
                _inFixup = false;
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void StateChanged(
            InternalEntityEntry entry,
            EntityState oldState,
            bool fromQuery)
        {
            if (fromQuery || _inFixup)
            {
                return;
            }

            try
            {
                _inFixup = true;

                if (oldState == EntityState.Detached)
                {
                    InitialFixup(entry, null, fromQuery: false);
                }
                else if (entry.EntityState == EntityState.Detached
                         && oldState == EntityState.Deleted)
                {
                    DeleteFixup(entry);
                }
            }
            finally
            {
                _inFixup = false;
            }
        }

        private void DeleteFixup(InternalEntityEntry entry)
        {
            var entityType = entry.EntityType;
            var stateManager = entry.StateManager;

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var principalToDependent = foreignKey.PrincipalToDependent;
                if (principalToDependent != null)
                {
                    var principalEntry = stateManager.GetPrincipal(entry, foreignKey);
                    if (principalEntry != null
                        && principalEntry.EntityState != EntityState.Deleted)
                    {
                        ResetReferenceOrRemoveCollection(principalEntry, principalToDependent, entry);
                    }
                }
            }

            foreach (var foreignKey in entityType.GetReferencingForeignKeys())
            {
                var dependentToPrincipal = foreignKey.DependentToPrincipal;
                if (dependentToPrincipal != null)
                {
                    var dependentEntries = stateManager.GetDependents(entry, foreignKey);
                    foreach (var dependentEntry in dependentEntries)
                    {
                        if (dependentEntry[dependentToPrincipal] == entry.Entity)
                        {
                            SetNavigation(dependentEntry, dependentToPrincipal, null);
                        }
                    }
                }
            }
        }

        private void InitialFixup(
            InternalEntityEntry entry,
            ISet<IForeignKey> handledForeignKeys,
            bool fromQuery)
        {
            var entityType = (EntityType)entry.EntityType;
            var stateManager = entry.StateManager;
            IForeignKey conflictingPrincipalForeignKey = null;
            var matchingPrincipal = false;

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                if (handledForeignKeys?.Contains(foreignKey) != true)
                {
                    var principalEntry = stateManager.GetPrincipal(entry, foreignKey);
                    if (principalEntry != null)
                    {
                        if (!foreignKey.PrincipalEntityType.IsAssignableFrom(principalEntry.EntityType))
                        {
                            conflictingPrincipalForeignKey = foreignKey;
                        }
                        else
                        {
                            matchingPrincipal = true;

                            // Set navigation to principal based on FK properties
                            SetNavigation(entry, foreignKey.DependentToPrincipal, principalEntry);

                            // Add this entity to principal's collection, or set inverse for 1:1
                            ToDependentFixup(entry, principalEntry, foreignKey);
                        }
                    }
                }
            }

            if (!matchingPrincipal
                && conflictingPrincipalForeignKey != null)
            {
                var principalEntry = stateManager.GetPrincipal(entry, conflictingPrincipalForeignKey);

                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IncompatiblePrincipalEntrySensitive(
                            entry.BuildCurrentValuesString(conflictingPrincipalForeignKey.Properties),
                            entityType.DisplayName(),
                            entry.BuildOriginalValuesString(entityType.FindPrimaryKey().Properties),
                            principalEntry.EntityType.DisplayName(),
                            conflictingPrincipalForeignKey.PrincipalEntityType.DisplayName()));
                }

                throw new InvalidOperationException(
                    CoreStrings.IncompatiblePrincipalEntry(
                        Property.Format(conflictingPrincipalForeignKey.Properties),
                        entityType.DisplayName(),
                        principalEntry.EntityType.DisplayName(),
                        conflictingPrincipalForeignKey.PrincipalEntityType.DisplayName()));
            }

            foreach (var foreignKey in entityType.GetReferencingForeignKeys())
            {
                if (!foreignKey.DeclaringEntityType.IsQueryType
                    && handledForeignKeys?.Contains(foreignKey) != true)
                {
                    var dependents = stateManager.GetDependents(entry, foreignKey);
                    if (foreignKey.IsUnique)
                    {
                        var dependentEntry = dependents.FirstOrDefault();
                        if (dependentEntry != null)
                        {
                            // Set navigations to and from principal entity that is indicated by FK
                            SetNavigation(entry, foreignKey.PrincipalToDependent, dependentEntry);
                            SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, entry);
                        }
                    }
                    else
                    {
                        foreach (var dependentEntry in dependents)
                        {
                            // Add to collection on principal indicated by FK and set inverse navigation
                            AddToCollection(entry, foreignKey.PrincipalToDependent, dependentEntry);

                            SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, entry);
                        }
                    }
                }
            }

            // If the new state is from a query then we are going to assume that the FK value is the source of
            // truth and not attempt to ascertain relationships from navigation properties
            if (!fromQuery)
            {
                var setModified = entry.EntityState != EntityState.Unchanged
                                  && entry.EntityState != EntityState.Modified;

                foreach (var foreignKey in entityType.GetReferencingForeignKeys())
                {
                    if (!foreignKey.DeclaringEntityType.IsQueryType)
                    {
                        var principalToDependent = foreignKey.PrincipalToDependent;
                        if (principalToDependent != null)
                        {
                            var navigationValue = entry[principalToDependent];
                            if (navigationValue != null)
                            {
                                if (principalToDependent.IsCollection())
                                {
                                    var dependents = ((IEnumerable)navigationValue).Cast<object>().ToList();
                                    foreach (var dependentEntity in dependents)
                                    {
                                        var dependentEntry = stateManager.TryGetEntry(dependentEntity, foreignKey.DeclaringEntityType);
                                        if (dependentEntry == null
                                            || dependentEntry.EntityState == EntityState.Detached)
                                        {
                                            // If dependents in collection are not yet tracked, then save them away so that
                                            // when we start tracking them we can come back and fixup this principal to them
                                            stateManager.RecordReferencedUntrackedEntity(dependentEntity, principalToDependent, entry);
                                        }
                                        else
                                        {
                                            FixupToDependent(entry, dependentEntry, foreignKey, setModified);
                                        }
                                    }
                                }
                                else
                                {
                                    var targetEntityType = principalToDependent.GetTargetType();
                                    var dependentEntry = stateManager.TryGetEntry(navigationValue, targetEntityType);
                                    if (dependentEntry == null
                                        || dependentEntry.EntityState == EntityState.Detached)
                                    {
                                        // If dependent is not yet tracked, then save it away so that
                                        // when we start tracking it we can come back and fixup this principal to it
                                        stateManager.RecordReferencedUntrackedEntity(navigationValue, principalToDependent, entry);
                                    }
                                    else
                                    {
                                        FixupToDependent(entry, dependentEntry, foreignKey, setModified);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    var dependentToPrincipal = foreignKey.DependentToPrincipal;
                    if (dependentToPrincipal != null)
                    {
                        var navigationValue = entry[dependentToPrincipal];
                        if (navigationValue != null)
                        {
                            var targetEntityType = dependentToPrincipal.GetTargetType();
                            var principalEntry = stateManager.TryGetEntry(navigationValue, targetEntityType);
                            if (principalEntry == null
                                || principalEntry.EntityState == EntityState.Detached)
                            {
                                // If principal is not yet tracked, then save it away so that
                                // when we start tracking it we can come back and fixup this dependent to it
                                stateManager.RecordReferencedUntrackedEntity(navigationValue, dependentToPrincipal, entry);
                            }
                            else
                            {
                                FixupToPrincipal(entry, principalEntry, foreignKey, setModified);
                            }
                        }
                    }
                }

                // If the entity was previously referenced while it was still untracked, go back and do the fixup
                // that we would have done then now that the entity is tracked.
                foreach (var danglerEntry in stateManager.GetRecordedReferrers(entry.Entity, clear: true))
                {
                    DelayedFixup(danglerEntry.Item2, danglerEntry.Item1, entry);
                }
            }
        }

        private void DelayedFixup(InternalEntityEntry entry, INavigation navigation, InternalEntityEntry referencedEntry)
        {
            var navigationValue = entry[navigation];

            if (navigationValue != null)
            {
                var setModified = referencedEntry.EntityState != EntityState.Unchanged
                                  && referencedEntry.EntityState != EntityState.Modified;

                if (!navigation.IsDependentToPrincipal())
                {
                    if (navigation.IsCollection())
                    {
                        if (entry.CollectionContains(navigation, referencedEntry))
                        {
                            FixupToDependent(entry, referencedEntry, navigation.ForeignKey, setModified);
                        }
                    }
                    else if (referencedEntry.Entity == navigationValue)
                    {
                        FixupToDependent(entry, referencedEntry, navigation.ForeignKey, setModified);
                    }
                }
                else if (referencedEntry.Entity == navigationValue)
                {
                    FixupToPrincipal(entry, referencedEntry, navigation.ForeignKey, setModified);
                }
            }
        }

        private void FixupToDependent(
            InternalEntityEntry principalEntry,
            InternalEntityEntry dependentEntry,
            IForeignKey foreignKey,
            bool setModified)
        {
            SetForeignKeyProperties(dependentEntry, principalEntry, foreignKey, setModified);

            SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, principalEntry);
        }

        private void FixupToPrincipal(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey,
            bool setModified)
        {
            SetForeignKeyProperties(dependentEntry, principalEntry, foreignKey, setModified);

            ToDependentFixup(dependentEntry, principalEntry, foreignKey);
        }

        private void ToDependentFixup(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey)
        {
            var principalToDependent = foreignKey.PrincipalToDependent;
            if (foreignKey.IsUnique)
            {
                var oldDependent = principalToDependent == null ? null : principalEntry[principalToDependent];
                var oldDependentEntry = oldDependent != null
                                        && !ReferenceEquals(dependentEntry.Entity, oldDependent)
                    ? dependentEntry.StateManager.TryGetEntry(oldDependent, foreignKey.DeclaringEntityType)
                    : dependentEntry.StateManager.GetDependentsUsingRelationshipSnapshot(principalEntry, foreignKey)
                        .FirstOrDefault();

                if (oldDependentEntry != null
                    && !ReferenceEquals(dependentEntry.Entity, oldDependentEntry.Entity)
                    && oldDependentEntry.EntityState != EntityState.Detached)
                {
                    ConditionallyNullForeignKeyProperties(oldDependentEntry, principalEntry, foreignKey);

                    var dependentToPrincipal = foreignKey.DependentToPrincipal;
                    if (dependentToPrincipal != null
                        && ReferenceEquals(oldDependentEntry[dependentToPrincipal], principalEntry.Entity)
                        && oldDependentEntry.StateManager.TryGetEntry(oldDependentEntry.Entity, foreignKey.DeclaringEntityType) != null)
                    {
                        SetNavigation(oldDependentEntry, dependentToPrincipal, null);
                    }
                }
            }

            if (principalToDependent != null)
            {
                SetReferenceOrAddToCollection(
                    principalEntry,
                    principalToDependent,
                    dependentEntry);
            }
        }

        private static void SetForeignKeyProperties(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey,
            bool setModified)
        {
            var principalProperties = foreignKey.PrincipalKey.Properties;
            var dependentProperties = foreignKey.Properties;

            for (var i = 0; i < foreignKey.Properties.Count; i++)
            {
                var principalProperty = principalProperties[i];
                var dependentProperty = dependentProperties[i];
                var principalValue = principalEntry[principalProperty];
                var dependentValue = dependentEntry[dependentProperty];

                if (!PrincipalValueEqualsDependentValue(principalProperty, dependentValue, principalValue)
                    || (dependentEntry.IsConceptualNull(dependentProperty)
                        && principalValue != null))
                {
                    if (principalEntry.HasTemporaryValue(principalProperty))
                    {
                        dependentEntry.SetTemporaryValue(dependentProperty, principalValue, setModified);
                    }
                    else
                    {
                        dependentEntry.SetProperty(dependentProperty, principalValue, setModified);
                    }
                    dependentEntry.StateManager.UpdateDependentMap(dependentEntry, foreignKey);
                    dependentEntry.SetRelationshipSnapshotValue(dependentProperty, principalValue);
                }
            }
        }

        private static bool PrincipalValueEqualsDependentValue(
            IProperty principalProperty,
            object dependentValue,
            object principalValue)
            => (principalProperty.GetKeyValueComparer()
                ?? principalProperty.FindMapping()?.KeyComparer)
               ?.Equals(dependentValue, principalValue)
               ?? StructuralComparisons.StructuralEqualityComparer.Equals(
                   dependentValue,
                   principalValue);

        private void ConditionallyNullForeignKeyProperties(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey)
        {
            var principalProperties = foreignKey.PrincipalKey.Properties;
            var dependentProperties = foreignKey.Properties;
            var hasOnlyKeyProperties = true;

            if (principalEntry != null
                && principalEntry.EntityState != EntityState.Detached)
            {
                for (var i = 0; i < foreignKey.Properties.Count; i++)
                {
                    if (!PrincipalValueEqualsDependentValue(
                        principalProperties[i],
                        dependentEntry[dependentProperties[i]],
                        principalEntry[principalProperties[i]]))
                    {
                        return;
                    }

                    if (!dependentProperties[i].IsKey())
                    {
                        hasOnlyKeyProperties = false;
                    }
                }
            }

            for (var i = 0; i < foreignKey.Properties.Count; i++)
            {
                if (!dependentProperties[i].IsKey())
                {
                    dependentEntry[dependentProperties[i]] = null;
                    dependentEntry.StateManager.UpdateDependentMap(dependentEntry, foreignKey);
                    dependentEntry.SetRelationshipSnapshotValue(dependentProperties[i], null);
                }
            }

            if (foreignKey.IsRequired
                && hasOnlyKeyProperties
                && dependentEntry.EntityState != EntityState.Detached)
            {
                switch (dependentEntry.EntityState)
                {
                    case EntityState.Added:
                        dependentEntry.SetEntityState(EntityState.Detached);
                        DeleteFixup(dependentEntry);
                        break;
                    case EntityState.Unchanged:
                    case EntityState.Modified:
                        dependentEntry.SetEntityState(EntityState.Deleted);
                        DeleteFixup(dependentEntry);
                        break;
                }
            }
        }

        private void SetNavigation(InternalEntityEntry entry, INavigation navigation, InternalEntityEntry value)
        {
            if (navigation != null)
            {
                _changeDetector.Suspend();
                var entity = value?.Entity;
                try
                {
                    entry[navigation] = entity;
                }
                finally
                {
                    _changeDetector.Resume();
                }

                entry.SetRelationshipSnapshotValue(navigation, entity);
            }
        }

        private void AddToCollection(InternalEntityEntry entry, INavigation navigation, InternalEntityEntry value)
        {
            if (navigation != null)
            {
                _changeDetector.Suspend();
                try
                {
                    if (entry.AddToCollection(navigation, value))
                    {
                        entry.AddToCollectionSnapshot(navigation, value.Entity);
                    }
                }
                finally
                {
                    _changeDetector.Resume();
                }
            }
        }

        private void RemoveFromCollection(InternalEntityEntry entry, INavigation navigation, InternalEntityEntry value)
        {
            _changeDetector.Suspend();
            try
            {
                entry.RemoveFromCollection(navigation, value);
            }
            finally
            {
                _changeDetector.Resume();
            }

            entry.RemoveFromCollectionSnapshot(navigation, value.Entity);
        }

        private void SetReferenceOrAddToCollection(
            InternalEntityEntry entry,
            INavigation navigation,
            InternalEntityEntry value)
        {
            if (navigation.IsCollection())
            {
                AddToCollection(entry, navigation, value);
            }
            else
            {
                SetNavigation(entry, navigation, value);
            }
        }

        private void ResetReferenceOrRemoveCollection(
            InternalEntityEntry entry,
            INavigation navigation,
            InternalEntityEntry value)
        {
            if (navigation.IsCollection())
            {
                RemoveFromCollection(entry, navigation, value);
            }
            else if (ReferenceEquals(entry[navigation], value.Entity))
            {
                SetNavigation(entry, navigation, null);
            }
        }
    }
}
