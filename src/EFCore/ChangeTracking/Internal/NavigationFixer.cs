// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        private bool _inFixup;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NavigationFixer(
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IEntityGraphAttacher attacher)
        {
            _changeDetector = changeDetector;
            _attacher = attacher;
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

                                    if (ReferenceEquals(victimDependentEntry[navigation], newTargetEntry.Entity))
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
                    }

                    if (inverse != null)
                    {
                        var collectionAccessor = inverse.IsCollection() ? inverse.GetCollectionAccessor() : null;

                        // Set the inverse reference or add the entity to the inverse collection
                        if (newTargetEntry != null)
                        {
                            SetReferenceOrAddToCollection(newTargetEntry, inverse, collectionAccessor, entry.Entity);
                        }

                        // Remove the entity from the old collection, or null the old inverse unless it was already
                        // changed to point to something else
                        if (oldTargetEntry != null)
                        {
                            if (collectionAccessor != null)
                            {
                                RemoveFromCollection(oldTargetEntry, inverse, collectionAccessor, entry.Entity);
                            }
                            else if (ReferenceEquals(oldTargetEntry[inverse], entry.Entity))
                            {
                                SetNavigation(oldTargetEntry, inverse, null);
                            }
                        }
                    }
                }
                else
                {
                    Debug.Assert(foreignKey.IsUnique);

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

                        SetNavigation(newTargetEntry, inverse, entry.Entity);
                    }

                    if (oldTargetEntry != null)
                    {
                        // Null the FK properties on the old dependent, unless they have already been changed
                        ConditionallyNullForeignKeyProperties(oldTargetEntry, entry, foreignKey);

                        // Clear the inverse reference, unless it has already been changed
                        if (inverse != null
                            && ReferenceEquals(oldTargetEntry[inverse], entry.Entity)
                            && (!oldTargetEntry.EntityType.HasDelegatedIdentity()
                                || entry.EntityType.GetNavigations().All(n =>
                                    n == navigation || !ReferenceEquals(oldTargetEntry.Entity, entry[n]))))
                        {
                            SetNavigation(oldTargetEntry, inverse, null);
                        }
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
                var targetEntry = targetEntityType.HasDelegatedIdentity()
                    ? stateManager.GetOrCreateEntry(newValue, targetEntityType)
                    : stateManager.GetOrCreateEntry(newValue);
                _attacher.AttachGraph(targetEntry, EntityState.Added);
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
            var collectionAccessor = navigation.GetCollectionAccessor();

            foreach (var oldValue in removed)
            {
                var oldTargetEntry = stateManager.TryGetEntry(oldValue);

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
                            && ReferenceEquals(oldTargetEntry[inverse], entry.Entity))
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
                var newTargetEntry = stateManager.GetOrCreateEntry(newValue);

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
                            RemoveFromCollection(oldPrincipalEntry, navigation, collectionAccessor, newValue);
                        }

                        // Set the FK properties on added dependents to match this principal
                        SetForeignKeyProperties(newTargetEntry, entry, foreignKey, setModified: true);

                        // Set the inverse navigation to point to this principal
                        SetNavigation(newTargetEntry, inverse, entry.Entity);
                    }
                    finally
                    {
                        _inFixup = false;
                    }
                }
                else
                {
                    stateManager.RecordReferencedUntrackedEntity(newValue, navigation, entry);
                    _attacher.AttachGraph(newTargetEntry, EntityState.Added);
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
                        var collectionAccessor = principalToDependent.IsCollection() ? principalToDependent.GetCollectionAccessor() : null;

                        if (oldPrincipalEntry != null)
                        {
                            // Remove this entity from the principal collection that it was previously part of,
                            // or null the navigation for a 1:1 unless that reference was already changed.
                            if (collectionAccessor != null)
                            {
                                RemoveFromCollection(oldPrincipalEntry, principalToDependent, collectionAccessor, entry.Entity);
                            }
                            else if (ReferenceEquals(oldPrincipalEntry[principalToDependent], entry.Entity))
                            {
                                SetNavigation(oldPrincipalEntry, principalToDependent, null);
                            }
                        }

                        if (newPrincipalEntry != null)
                        {
                            // Add this entity to the collection of the new principal, or set the navigation for a 1:1
                            SetReferenceOrAddToCollection(newPrincipalEntry, principalToDependent, collectionAccessor, entry.Entity);
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

                                    if (ReferenceEquals(targetDependentEntry[dependentToPrincipal], newPrincipalEntry.Entity))
                                    {
                                        SetNavigation(targetDependentEntry, dependentToPrincipal, null);
                                    }
                                }
                            }

                            SetNavigation(entry, dependentToPrincipal, newPrincipalEntry.Entity);
                        }
                        else if (oldPrincipalEntry != null
                                 && ReferenceEquals(entry[dependentToPrincipal], oldPrincipalEntry.Entity))
                        {
                            SetNavigation(entry, dependentToPrincipal, null);
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
                    if (principalEntry != null)
                    {
                        if (principalToDependent.IsCollection())
                        {
                            RemoveFromCollection(
                                principalEntry,
                                principalToDependent,
                                principalToDependent.GetCollectionAccessor(),
                                entry.Entity);
                        }
                        else if (principalEntry[principalToDependent] == entry.Entity)
                        {
                            SetNavigation(principalEntry, principalToDependent, null);
                        }
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
            var entityType = entry.EntityType;
            var stateManager = entry.StateManager;

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                if (handledForeignKeys == null
                    || !handledForeignKeys.Contains(foreignKey))
                {
                    var principalEntry = stateManager.GetPrincipal(entry, foreignKey);
                    if (principalEntry != null)
                    {
                        // Set navigation to principal based on FK properties
                        SetNavigation(entry, foreignKey.DependentToPrincipal, principalEntry.Entity);

                        // Add this entity to principal's collection, or set inverse for 1:1
                        var principalToDependent = foreignKey.PrincipalToDependent;
                        if (principalToDependent != null)
                        {
                            if (!principalToDependent.IsCollection())
                            {
                                var oldDependent = principalEntry[principalToDependent];
                                if (oldDependent != null
                                    && !ReferenceEquals(entry.Entity, oldDependent))
                                {
                                    var oldDependentEntry = stateManager.TryGetEntry(oldDependent);
                                    if (oldDependentEntry != null
                                        && oldDependentEntry.EntityState != EntityState.Detached)
                                    {
                                        ConditionallyNullForeignKeyProperties(oldDependentEntry, null, foreignKey);
                                        SetNavigation(principalEntry, principalToDependent, null);
                                    }
                                }
                            }

                            SetReferenceOrAddToCollection(
                                principalEntry,
                                principalToDependent,
                                principalToDependent.IsCollection() ? principalToDependent.GetCollectionAccessor() : null,
                                entry.Entity);
                        }
                    }
                }
            }

            foreach (var foreignKey in entityType.GetReferencingForeignKeys())
            {
                if (handledForeignKeys == null
                    || !handledForeignKeys.Contains(foreignKey))
                {
                    var dependents = stateManager.GetDependents(entry, foreignKey).ToList();
                    if (dependents.Any())
                    {
                        var dependentToPrincipal = foreignKey.DependentToPrincipal;
                        var principalToDependent = foreignKey.PrincipalToDependent;

                        if (foreignKey.IsUnique)
                        {
                            var dependentEntry = dependents.First();

                            // Set navigations to and from principal entity that is indicated by FK
                            SetNavigation(entry, principalToDependent, dependentEntry.Entity);
                            SetNavigation(dependentEntry, dependentToPrincipal, entry.Entity);
                        }
                        else
                        {
                            var collectionAccessor = principalToDependent?.GetCollectionAccessor();

                            foreach (var dependentEntry in dependents)
                            {
                                var dependentEntity = dependentEntry.Entity;

                                // Add to collection on principal indicated by FK and set inverse navigation
                                AddToCollection(entry, principalToDependent, collectionAccessor, dependentEntity);

                                SetNavigation(dependentEntry, dependentToPrincipal, entry.Entity);
                            }
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
                    var principalToDependent = foreignKey.PrincipalToDependent;
                    if (principalToDependent != null)
                    {
                        var navigationValue = entry[principalToDependent];
                        if (navigationValue != null)
                        {
                            if (principalToDependent.IsCollection())
                            {
                                var dependents = ((IEnumerable)navigationValue).Cast<object>();
                                foreach (var dependentEntity in dependents)
                                {
                                    var dependentEntry = stateManager.TryGetEntry(dependentEntity);
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
                foreach (var danglerEntry in stateManager.GetRecordedReferers(entry.Entity, clear: true))
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
                        if (navigation.GetCollectionAccessor().Contains(entry.Entity, referencedEntry.Entity))
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

            SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, principalEntry.Entity);
        }

        private void FixupToPrincipal(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey,
            bool setModified)
        {
            SetForeignKeyProperties(dependentEntry, principalEntry, foreignKey, setModified);

            var inverse = foreignKey.PrincipalToDependent;
            if (inverse != null)
            {
                SetReferenceOrAddToCollection(
                    principalEntry,
                    inverse,
                    inverse.IsCollection() ? inverse.GetCollectionAccessor() : null,
                    dependentEntry.Entity);
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
                var principalValue = principalEntry[principalProperties[i]];
                var dependentProperty = dependentProperties[i];

                if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                    dependentEntry[dependentProperty],
                    principalValue)
                    || (dependentEntry.IsConceptualNull(dependentProperty)
                        && principalValue != null))
                {
                    dependentEntry.SetProperty(dependentProperty, principalValue, setModified);
                    dependentEntry.StateManager.UpdateDependentMap(dependentEntry, foreignKey);
                    dependentEntry.SetRelationshipSnapshotValue(dependentProperty, principalValue);
                }
            }
        }

        private static void ConditionallyNullForeignKeyProperties(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey)
        {
            var principalProperties = foreignKey.PrincipalKey.Properties;
            var dependentProperties = foreignKey.Properties;
            var hasNonKeyProperties = false;

            if (principalEntry != null
                && principalEntry.EntityState != EntityState.Detached)
            {
                for (var i = 0; i < foreignKey.Properties.Count; i++)
                {
                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(
                        principalEntry[principalProperties[i]],
                        dependentEntry[dependentProperties[i]]))
                    {
                        return;
                    }

                    if (!dependentProperties[i].IsKey())
                    {
                        hasNonKeyProperties = true;
                    }
                }
            }

            for (var i = 0; i < foreignKey.Properties.Count; i++)
            {
                if (!hasNonKeyProperties
                    || !dependentProperties[i].IsKey())
                {
                    dependentEntry[dependentProperties[i]] = null;
                    dependentEntry.StateManager.UpdateDependentMap(dependentEntry, foreignKey);
                    dependentEntry.SetRelationshipSnapshotValue(dependentProperties[i], null);
                }
            }

            if (foreignKey.IsRequired
                && !hasNonKeyProperties
                && dependentEntry.EntityState != EntityState.Detached)
            {
                switch (dependentEntry.EntityState)
                {
                    case EntityState.Added:
                        dependentEntry.SetEntityState(EntityState.Detached);
                        break;
                    case EntityState.Unchanged:
                    case EntityState.Modified:
                        dependentEntry.SetEntityState(EntityState.Deleted);
                        break;
                }
            }
        }

        private void SetNavigation(InternalEntityEntry entry, INavigation navigation, object value)
        {
            if (navigation != null)
            {
                _changeDetector.Suspend();
                try
                {
                    entry[navigation] = value;
                }
                finally
                {
                    _changeDetector.Resume();
                }
                entry.SetRelationshipSnapshotValue(navigation, value);
            }
        }

        private void AddToCollection(
            InternalEntityEntry entry,
            INavigation navigation,
            IClrCollectionAccessor collectionAccessor,
            object value)
        {
            if (navigation != null)
            {
                _changeDetector.Suspend();
                try
                {
                    if (collectionAccessor.Add(entry.Entity, value))
                    {
                        entry.AddToCollectionSnapshot(navigation, value);
                    }
                }
                finally
                {
                    _changeDetector.Resume();
                }
            }
        }

        private void RemoveFromCollection(
            InternalEntityEntry entry,
            INavigation navigation,
            IClrCollectionAccessor collectionAccessor,
            object value)
        {
            _changeDetector.Suspend();
            try
            {
                collectionAccessor.Remove(entry.Entity, value);
            }
            finally
            {
                _changeDetector.Resume();
            }
            entry.RemoveFromCollectionSnapshot(navigation, value);
        }

        private void SetReferenceOrAddToCollection(
            InternalEntityEntry entry,
            INavigation navigation,
            IClrCollectionAccessor collectionAccessor,
            object value)
        {
            if (collectionAccessor != null)
            {
                AddToCollection(entry, navigation, collectionAccessor, value);
            }
            else
            {
                SetNavigation(entry, navigation, value);
            }
        }
    }
}
