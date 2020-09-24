// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
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
    public class NavigationFixer : INavigationFixer
    {
        private readonly IChangeDetector _changeDetector;
        private readonly IEntityGraphAttacher _attacher;
        private bool _inFixup;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public NavigationFixer(
            [NotNull] IChangeDetector changeDetector,
            [NotNull] IEntityGraphAttacher attacher)
        {
            _changeDetector = changeDetector;
            _attacher = attacher;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void NavigationReferenceChanged(
            InternalEntityEntry entry,
            INavigationBase navigationBase,
            object oldValue,
            object newValue)
        {
            if (_inFixup)
            {
                return;
            }

            Check.DebugAssert(navigationBase is INavigation, "Issue #21673. Non-collection skip navigations not supported.");

            var navigation = (INavigation)navigationBase;
            var foreignKey = navigation.ForeignKey;
            var stateManager = entry.StateManager;
            var inverse = navigation.Inverse;
            var targetEntityType = navigation.TargetEntityType;

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

                if (navigation.IsOnDependent)
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
                                var victimDependentEntry =
                                    (InternalEntityEntry)stateManager.GetDependents(newTargetEntry, foreignKey).FirstOrDefault();
                                if (victimDependentEntry != null
                                    && victimDependentEntry != entry)
                                {
                                    ConditionallyNullForeignKeyProperties(victimDependentEntry, newTargetEntry, foreignKey);

                                    if (ReferenceEquals(victimDependentEntry[navigation], newTargetEntry.Entity)
                                        && victimDependentEntry.StateManager
                                            .TryGetEntry(victimDependentEntry.Entity, navigation.DeclaringEntityType)
                                        != null)
                                    {
                                        SetNavigation(victimDependentEntry, navigation, null, fromQuery: false);
                                    }
                                }
                            }

                            // Set the FK properties to reflect the change to the navigation.
                            SetForeignKeyProperties(entry, newTargetEntry, foreignKey, setModified: true, fromQuery: false);
                            entry.SetRelationshipSnapshotValue(navigation, newValue);
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
                            SetReferenceOrAddToCollection(newTargetEntry, inverse, entry, fromQuery: false);
                        }

                        // Remove the entity from the old collection, or null the old inverse unless it was already
                        // changed to point to something else
                        if (oldTargetEntry != null
                            && oldTargetEntry.EntityState != EntityState.Deleted)
                        {
                            ResetReferenceOrRemoveCollection(oldTargetEntry, inverse, entry, fromQuery: false);
                        }
                    }
                }
                else
                {
                    Check.DebugAssert(foreignKey.IsUnique, $"foreignKey {foreignKey} is not unique");

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
                            SetNavigation(oldTargetEntry, inverse, null, fromQuery: false);
                        }
                    }

                    if (newTargetEntry != null)
                    {
                        // Navigation points to dependent and is 1:1. Find the principal that previously pointed to that
                        // dependent and null out its navigation property. A.k.a. reference stealing.
                        // However, if the reference is already set to point to something else, then don't change it.
                        var victimPrincipalEntry = stateManager.FindPrincipal(newTargetEntry, foreignKey);
                        if (victimPrincipalEntry != null
                            && victimPrincipalEntry != entry
                            && ReferenceEquals(victimPrincipalEntry[navigation], newTargetEntry.Entity))
                        {
                            SetNavigation(victimPrincipalEntry, navigation, null, fromQuery: false);
                        }

                        SetForeignKeyProperties(newTargetEntry, entry, foreignKey, setModified: true, fromQuery: false);

                        SetNavigation(newTargetEntry, inverse, entry, fromQuery: false);
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

                newTargetEntry = stateManager.GetOrCreateEntry(newValue, targetEntityType);

                _attacher.AttachGraph(
                    newTargetEntry,
                    EntityState.Added,
                    EntityState.Modified,
                    forceStateWhenUnknownKey: false);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void NavigationCollectionChanged(
            InternalEntityEntry entry,
            INavigationBase navigationBase,
            IEnumerable<object> added,
            IEnumerable<object> removed)
        {
            if (_inFixup)
            {
                return;
            }

            var stateManager = entry.StateManager;
            var inverse = navigationBase.Inverse;
            var targetEntityType = navigationBase.TargetEntityType;

            foreach (var oldValue in removed)
            {
                var oldTargetEntry = stateManager.TryGetEntry(oldValue, targetEntityType);

                if (oldTargetEntry != null
                    && oldTargetEntry.EntityState != EntityState.Detached)
                {
                    try
                    {
                        _inFixup = true;

                        if (navigationBase is ISkipNavigation skipNavigation)
                        {
                            FindJoinEntry(entry, oldTargetEntry, skipNavigation)?.SetEntityState(EntityState.Deleted);

                            Check.DebugAssert(
                                skipNavigation.Inverse.IsCollection,
                                "Issue #21673. Non-collection skip navigations not supported.");

                            RemoveFromCollection(oldTargetEntry, skipNavigation.Inverse, entry);
                        }
                        else
                        {
                            var foreignKey = ((INavigation)navigationBase).ForeignKey;

                            // Null FKs and navigations of dependents that have been removed, unless they
                            // have already been changed.
                            ConditionallyNullForeignKeyProperties(oldTargetEntry, entry, foreignKey);

                            if (inverse != null
                                && ReferenceEquals(oldTargetEntry[inverse], entry.Entity)
                                && (!foreignKey.IsOwnership
                                    || (oldTargetEntry.EntityState != EntityState.Deleted
                                        && oldTargetEntry.EntityState != EntityState.Detached)))
                            {
                                SetNavigation(oldTargetEntry, inverse, null, fromQuery: false);
                            }
                        }

                        entry.RemoveFromCollectionSnapshot(navigationBase, oldValue);
                    }
                    finally
                    {
                        _inFixup = false;
                    }
                }
            }

            foreach (var newValue in added)
            {
                var newTargetEntry = stateManager.GetOrCreateEntry(newValue, targetEntityType);
                if (newTargetEntry.EntityState != EntityState.Detached)
                {
                    try
                    {
                        _inFixup = true;

                        if (navigationBase is ISkipNavigation skipNavigation)
                        {
                            FindOrCreateJoinEntry(entry, newTargetEntry, skipNavigation, fromQuery: false, setModified: true);

                            Check.DebugAssert(
                                skipNavigation.Inverse.IsCollection,
                                "Issue #21673. Non-collection skip navigations not supported.");

                            AddToCollection(newTargetEntry, skipNavigation.Inverse, entry, fromQuery: false);
                        }
                        else
                        {
                            var foreignKey = ((INavigation)navigationBase).ForeignKey;

                            // For a dependent added to the collection, remove it from the collection of
                            // the principal entity that it was previously part of
                            var oldPrincipalEntry = stateManager.FindPrincipalUsingRelationshipSnapshot(newTargetEntry, foreignKey);
                            if (oldPrincipalEntry != null
                                && oldPrincipalEntry != entry)
                            {
                                RemoveFromCollection(oldPrincipalEntry, navigationBase, newTargetEntry);
                            }

                            // Set the FK properties on added dependents to match this principal
                            SetForeignKeyProperties(newTargetEntry, entry, foreignKey, setModified: true, fromQuery: false);

                            // Set the inverse navigation to point to this principal
                            SetNavigation(newTargetEntry, inverse, entry, fromQuery: false);
                        }
                    }
                    finally
                    {
                        _inFixup = false;
                    }
                }
                else
                {
                    stateManager.RecordReferencedUntrackedEntity(newValue, navigationBase, entry);

                    _attacher.AttachGraph(
                        newTargetEntry,
                        EntityState.Added,
                        EntityState.Modified,
                        forceStateWhenUnknownKey: false);
                }

                entry.AddToCollectionSnapshot(navigationBase, newValue);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void KeyPropertyChanged(
            InternalEntityEntry entry,
            IProperty property,
            IEnumerable<IKey> containingPrincipalKeys,
            IEnumerable<IForeignKey> containingForeignKeys,
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
                    var newPrincipalEntry = stateManager.FindPrincipal(entry, foreignKey)
                        ?? stateManager.FindPrincipalUsingPreStoreGeneratedValues(entry, foreignKey);
                    var oldPrincipalEntry = stateManager.FindPrincipalUsingRelationshipSnapshot(entry, foreignKey);

                    var principalToDependent = foreignKey.PrincipalToDependent;
                    if (principalToDependent != null)
                    {
                        if (oldPrincipalEntry != null
                            && oldPrincipalEntry.EntityState != EntityState.Deleted)
                        {
                            // Remove this entity from the principal collection that it was previously part of,
                            // or null the navigation for a 1:1 unless that reference was already changed.
                            ResetReferenceOrRemoveCollection(oldPrincipalEntry, principalToDependent, entry, fromQuery: false);
                        }

                        if (newPrincipalEntry != null
                            && !entry.IsConceptualNull(property))
                        {
                            // Add this entity to the collection of the new principal, or set the navigation for a 1:1
                            SetReferenceOrAddToCollection(newPrincipalEntry, principalToDependent, entry, fromQuery: false);
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
                                    = (InternalEntityEntry)stateManager
                                        .GetDependentsUsingRelationshipSnapshot(newPrincipalEntry, foreignKey).FirstOrDefault();

                                if (targetDependentEntry != null
                                    && targetDependentEntry != entry)
                                {
                                    ConditionallyNullForeignKeyProperties(targetDependentEntry, newPrincipalEntry, foreignKey);

                                    if (ReferenceEquals(targetDependentEntry[dependentToPrincipal], newPrincipalEntry.Entity)
                                        && targetDependentEntry.StateManager.TryGetEntry(
                                            targetDependentEntry.Entity, foreignKey.DeclaringEntityType)
                                        != null)
                                    {
                                        SetNavigation(targetDependentEntry, dependentToPrincipal, null, fromQuery: false);
                                    }
                                }
                            }

                            if (!entry.IsConceptualNull(property))
                            {
                                SetNavigation(entry, dependentToPrincipal, newPrincipalEntry, fromQuery: false);
                            }
                        }
                        else if (oldPrincipalEntry != null)
                        {
                            if (ReferenceEquals(entry[dependentToPrincipal], oldPrincipalEntry.Entity)
                                && entry.StateManager.TryGetEntry(entry.Entity, foreignKey.DeclaringEntityType) != null)
                            {
                                SetNavigation(entry, dependentToPrincipal, null, fromQuery: false);
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
                        foreach (InternalEntityEntry dependentEntry in stateManager
                            .GetDependentsUsingRelationshipSnapshot(entry, foreignKey).ToList())
                        {
                            SetForeignKeyProperties(dependentEntry, entry, foreignKey, setModified: true, fromQuery: false);
                        }

                        if (foreignKey.IsOwnership)
                        {
                            continue;
                        }

                        // Fix up dependents that have been added by propagating through different foreign key
                        foreach (InternalEntityEntry dependentEntry in stateManager.GetDependents(entry, foreignKey).ToList())
                        {
                            var principalToDependent = foreignKey.PrincipalToDependent;
                            if (principalToDependent != null)
                            {
                                if (!entry.IsConceptualNull(property))
                                {
                                    // Add this entity to the collection of the new principal, or set the navigation for a 1:1
                                    SetReferenceOrAddToCollection(entry, principalToDependent, dependentEntry, fromQuery: false);
                                }
                            }

                            var dependentToPrincipal = foreignKey.DependentToPrincipal;
                            if (dependentToPrincipal != null)
                            {
                                if (!entry.IsConceptualNull(property))
                                {
                                    SetNavigation(dependentEntry, dependentToPrincipal, entry, fromQuery: false);
                                }
                            }
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
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void TrackedFromQuery(
            InternalEntityEntry entry)
        {
            try
            {
                _inFixup = true;

                InitialFixup(entry, fromQuery: true);
            }
            finally
            {
                _inFixup = false;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                    InitialFixup(entry, fromQuery: false);
                }
                else if (oldState == EntityState.Deleted
                    && entry.EntityState == EntityState.Detached)
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
                    var principalEntry = stateManager.FindPrincipal(entry, foreignKey);
                    if (principalEntry != null
                        && principalEntry.EntityState != EntityState.Deleted)
                    {
                        ResetReferenceOrRemoveCollection(principalEntry, principalToDependent, entry, fromQuery: false);
                    }
                }
            }

            foreach (var foreignKey in entityType.GetReferencingForeignKeys())
            {
                var dependentToPrincipal = foreignKey.DependentToPrincipal;
                if (dependentToPrincipal == null
                    && !foreignKey.IsOwnership)
                {
                    continue;
                }

                var dependentEntries = stateManager.GetDependents(entry, foreignKey);
                foreach (InternalEntityEntry dependentEntry in dependentEntries.ToList())
                {
                    if (foreignKey.IsOwnership)
                    {
                        ConditionallyNullForeignKeyProperties(dependentEntry, entry, foreignKey);
                    }

                    if (dependentToPrincipal != null
                        && (!foreignKey.IsOwnership
                            || (entry.EntityState != EntityState.Deleted
                                && entry.EntityState != EntityState.Detached))
                        && dependentEntry[dependentToPrincipal] == entry.Entity)
                    {
                        SetNavigation(dependentEntry, dependentToPrincipal, null, fromQuery: false);
                    }
                }
            }

            foreach (var skipNavigation in entityType.GetSkipNavigations())
            {
                var navigationValue = entry[skipNavigation];
                if (navigationValue != null)
                {
                    Check.DebugAssert(skipNavigation.IsCollection, "Issue #21673. Non-collection skip navigations not supported.");

                    var others = ((IEnumerable)navigationValue).Cast<object>().ToList();
                    foreach (var otherEntity in others)
                    {
                        var otherEntry = stateManager.TryGetEntry(otherEntity, skipNavigation.Inverse.DeclaringEntityType);
                        if (otherEntry != null
                            && otherEntry.EntityState != EntityState.Deleted)
                        {
                            Check.DebugAssert(
                                skipNavigation.Inverse.IsCollection,
                                "Issue #21673. Non-collection skip navigations not supported.");

                            RemoveFromCollection(otherEntry, skipNavigation.Inverse, entry);
                        }
                    }
                }
            }
        }

        private void InitialFixup(
            InternalEntityEntry entry,
            bool fromQuery)
        {
            var entityType = entry.EntityType;
            var stateManager = entry.StateManager;

            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                var principalEntry = stateManager.FindPrincipal(entry, foreignKey);
                if (principalEntry != null)
                {
                    var navigation = foreignKey.DependentToPrincipal;
                    var existingPrincipal = navigation == null ? null : entry[navigation];
                    if (existingPrincipal == null
                        || existingPrincipal == principalEntry.Entity)
                    {
                        // Set navigation to principal based on FK properties
                        SetNavigation(entry, navigation, principalEntry, fromQuery);

                        // Add this entity to principal's collection, or set inverse for 1:1
                        ToDependentFixup(entry, principalEntry, foreignKey, fromQuery);
                    }
                }

                foreach (var skipNavigation in foreignKey.GetReferencingSkipNavigations())
                {
                    var leftEntry = stateManager.FindPrincipal(entry, foreignKey);
                    if (leftEntry != null)
                    {
                        var rightEntry = stateManager.FindPrincipal(entry, skipNavigation.Inverse.ForeignKey);
                        if (rightEntry != null)
                        {
                            AddToCollection(leftEntry, skipNavigation, rightEntry, fromQuery);
                            AddToCollection(rightEntry, skipNavigation.Inverse, leftEntry, fromQuery);
                        }
                    }
                }
            }

            foreach (var foreignKey in entityType.GetReferencingForeignKeys())
            {
                if (foreignKey.DeclaringEntityType.FindPrimaryKey() != null)
                {
                    var dependents = stateManager.GetDependents(entry, foreignKey);
                    if (foreignKey.IsUnique)
                    {
                        var dependentEntry = (InternalEntityEntry)dependents.FirstOrDefault();
                        if (dependentEntry != null)
                        {
                            if ((!foreignKey.IsOwnership
                                    || (dependentEntry.EntityState != EntityState.Deleted
                                        && dependentEntry.EntityState != EntityState.Detached))
                                && (foreignKey.PrincipalToDependent == null
                                    || entry[foreignKey.PrincipalToDependent] == null
                                    || entry[foreignKey.PrincipalToDependent] == dependentEntry.Entity))
                            {
                                // Set navigations to and from principal entity that is indicated by FK
                                SetNavigation(entry, foreignKey.PrincipalToDependent, dependentEntry, fromQuery);
                                SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, entry, fromQuery);
                            }
                        }
                    }
                    else
                    {
                        foreach (InternalEntityEntry dependentEntry in dependents)
                        {
                            if ((!foreignKey.IsOwnership
                                    || (dependentEntry.EntityState != EntityState.Deleted
                                        && dependentEntry.EntityState != EntityState.Detached))
                                && (!fromQuery
                                    || foreignKey.DependentToPrincipal == null
                                    || dependentEntry.GetCurrentValue(foreignKey.DependentToPrincipal) == null))
                            {
                                // Add to collection on principal indicated by FK and set inverse navigation
                                AddToCollection(entry, foreignKey.PrincipalToDependent, dependentEntry, fromQuery);
                                SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, entry, fromQuery);
                            }

                            foreach (var skipNavigation in foreignKey.GetReferencingSkipNavigations())
                            {
                                var otherEntry = stateManager.FindPrincipal(dependentEntry, skipNavigation.Inverse.ForeignKey);
                                if (otherEntry != null)
                                {
                                    AddToCollection(otherEntry, skipNavigation.Inverse, entry, fromQuery);
                                    AddToCollection(entry, skipNavigation, otherEntry, fromQuery);
                                }
                            }
                        }
                    }
                }
            }

            // If the new state is from a query then we are going to assume that the FK value is the source of
            // truth and not attempt to ascertain relationships from navigation properties
            if (!fromQuery)
            {
                var setModified = entry.EntityState != EntityState.Unchanged;

                foreach (var foreignKey in entityType.GetReferencingForeignKeys())
                {
                    if (foreignKey.DeclaringEntityType.FindPrimaryKey() != null)
                    {
                        var principalToDependent = foreignKey.PrincipalToDependent;
                        if (principalToDependent != null)
                        {
                            var navigationValue = entry[principalToDependent];
                            if (navigationValue != null)
                            {
                                if (principalToDependent.IsCollection)
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
                                            FixupToDependent(entry, dependentEntry, foreignKey, setModified, fromQuery);
                                        }
                                    }
                                }
                                else
                                {
                                    var targetEntityType = principalToDependent.TargetEntityType;
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
                                        FixupToDependent(entry, dependentEntry, foreignKey, setModified, fromQuery);
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
                            var targetEntityType = dependentToPrincipal.TargetEntityType;
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
                                FixupToPrincipal(entry, principalEntry, foreignKey, setModified, fromQuery);
                            }
                        }
                    }
                }

                foreach (var skipNavigation in entityType.GetSkipNavigations())
                {
                    var navigationValue = entry[skipNavigation];
                    if (navigationValue != null)
                    {
                        Check.DebugAssert(skipNavigation.IsCollection, "Issue #21673. Non-collection skip navigations not supported.");
                        var others = ((IEnumerable)navigationValue).Cast<object>().ToList();
                        foreach (var otherEntity in others)
                        {
                            var otherEntry = stateManager.TryGetEntry(otherEntity, skipNavigation.Inverse.DeclaringEntityType);
                            if (otherEntry == null
                                || otherEntry.EntityState == EntityState.Detached)
                            {
                                // If dependents in collection are not yet tracked, then save them away so that
                                // when we start tracking them we can come back and fixup this principal to them
                                stateManager.RecordReferencedUntrackedEntity(otherEntity, skipNavigation, entry);
                            }
                            else
                            {
                                FindOrCreateJoinEntry(entry, otherEntry, skipNavigation, fromQuery, setModified);

                                Check.DebugAssert(
                                    skipNavigation.Inverse.IsCollection,
                                    "Issue #21673. Non-collection skip navigations not supported.");

                                AddToCollection(otherEntry, skipNavigation.Inverse, entry, fromQuery);
                            }
                        }
                    }
                }

                // If the entity was previously referenced while it was still untracked, go back and do the fixup
                // that we would have done then now that the entity is tracked.
                foreach (var danglerEntry in stateManager.GetRecordedReferrers(entry.Entity, clear: true))
                {
                    DelayedFixup(danglerEntry.Item2, danglerEntry.Item1, entry, fromQuery);
                }
            }
        }

        private void DelayedFixup(
            InternalEntityEntry entry,
            INavigationBase navigationBase,
            InternalEntityEntry referencedEntry,
            bool fromQuery)
        {
            var navigationValue = entry[navigationBase];

            if (navigationValue != null)
            {
                var setModified = referencedEntry.EntityState != EntityState.Unchanged;
                if (navigationBase is ISkipNavigation skipNavigation)
                {
                    FindOrCreateJoinEntry(entry, referencedEntry, skipNavigation, fromQuery, setModified);

                    Check.DebugAssert(
                        skipNavigation.Inverse.IsCollection,
                        "Issue #21673. Non-collection skip navigations not supported.");

                    AddToCollection(referencedEntry, skipNavigation.Inverse, entry, fromQuery);
                }
                else
                {
                    var navigation = (INavigation)navigationBase;

                    if (!navigation.IsOnDependent)
                    {
                        if (navigation.IsCollection)
                        {
                            if (entry.CollectionContains(navigation, referencedEntry))
                            {
                                FixupToDependent(entry, referencedEntry, navigation.ForeignKey, setModified, fromQuery);
                            }
                        }
                        else if (referencedEntry.Entity == navigationValue)
                        {
                            FixupToDependent(entry, referencedEntry, navigation.ForeignKey, setModified, fromQuery);
                        }
                    }
                    else if (referencedEntry.Entity == navigationValue)
                    {
                        FixupToPrincipal(entry, referencedEntry, navigation.ForeignKey, setModified, fromQuery);
                    }
                }
            }
        }

        private InternalEntityEntry FindOrCreateJoinEntry(
            InternalEntityEntry entry,
            InternalEntityEntry otherEntry,
            ISkipNavigation skipNavigation,
            bool fromQuery,
            bool setModified)
        {
            var joinEntry = FindJoinEntry(entry, otherEntry, skipNavigation);

            if (joinEntry == null)
            {
                var joinEntityType = skipNavigation.JoinEntityType;
                var joinEntity = joinEntityType.GetInstanceFactory()(
                    new MaterializationContext(ValueBuffer.Empty, entry.StateManager.Context));

                joinEntry = entry.StateManager.GetOrCreateEntry(joinEntity, joinEntityType);
            }

            SetForeignKeyProperties(joinEntry, entry, skipNavigation.ForeignKey, setModified, fromQuery);
            SetForeignKeyProperties(joinEntry, otherEntry, skipNavigation.Inverse.ForeignKey, setModified, fromQuery);

            if (joinEntry.EntityState == EntityState.Detached)
            {
                try
                {
                    _inFixup = false;

                    joinEntry.SetEntityState(
                        setModified
                        || entry.EntityState == EntityState.Added
                        || otherEntry.EntityState == EntityState.Added
                            ? EntityState.Added
                            : EntityState.Unchanged);
                }
                finally
                {
                    _inFixup = true;
                }
            }

            return joinEntry;
        }

        private static InternalEntityEntry FindJoinEntry(
            InternalEntityEntry entry,
            InternalEntityEntry otherEntry,
            ISkipNavigation skipNavigation)
        {
            var joinEntityType = skipNavigation.JoinEntityType;
            var foreignKey = skipNavigation.ForeignKey;
            var otherForeignKey = skipNavigation.Inverse.ForeignKey;

            // TODO: Perf - avoid looking up the join table key every time. See #21901

            if (foreignKey.Properties.Count == 1
                && otherForeignKey.Properties.Count == 1)
            {
                if (TryFind(entry, otherEntry, foreignKey, otherForeignKey, out var joinEntry))
                {
                    return joinEntry;
                }

                if (TryFind(otherEntry, entry, otherForeignKey, foreignKey, out joinEntry))
                {
                    return joinEntry;
                }
            }
            else
            {
                if (TryFindComposite(entry, otherEntry, foreignKey, otherForeignKey, out var joinEntry))
                {
                    return joinEntry;
                }

                if (TryFindComposite(otherEntry, entry, otherForeignKey, foreignKey, out joinEntry))
                {
                    return joinEntry;
                }
            }

            // Perf - see #21900

            var keyValues = foreignKey.PrincipalKey.Properties.Select(p => entry[p])
                .Concat(otherForeignKey.PrincipalKey.Properties.Select(p => otherEntry[p]))
                .ToList();

            var keyProperties = foreignKey.Properties.Concat(otherForeignKey.Properties).ToList();
            var keyComparers = keyProperties.Select(e => e.GetKeyValueComparer()).ToList();
            var propertiesCount = keyComparers.Count;

            foreach (var candidate in entry.StateManager.Entries)
            {
                if (candidate.EntityType == joinEntityType
                    && KeysEqual(candidate))
                {
                    return candidate;
                }
            }

            return null;

            bool KeysEqual(InternalEntityEntry candidate)
            {
                for (var i = 0; i < propertiesCount; i++)
                {
                    if (!keyComparers[i].Equals(keyValues[i], candidate[keyProperties[i]]))
                    {
                        return false;
                    }
                }

                return true;
            }

            bool TryFind(
                InternalEntityEntry firstEntry,
                InternalEntityEntry secondEntry,
                IForeignKey firstForeignKey,
                IForeignKey secondForeignKey,
                out InternalEntityEntry joinEntry)
            {
                var key = joinEntityType.FindKey(new[] { firstForeignKey.Properties[0], secondForeignKey.Properties[0] });
                if (key != null)
                {
                    joinEntry = entry.StateManager.TryGetEntry(
                        key,
                        new[]
                        {
                            firstEntry[firstForeignKey.PrincipalKey.Properties[0]],
                            secondEntry[secondForeignKey.PrincipalKey.Properties[0]]
                        });
                    return true;
                }

                joinEntry = null;
                return false;
            }

            bool TryFindComposite(
                InternalEntityEntry firstEntry,
                InternalEntityEntry secondEntry,
                IForeignKey firstForeignKey,
                IForeignKey secondForeignKey,
                out InternalEntityEntry joinEntry)
            {
                var firstForeignKeyProperties = firstForeignKey.Properties;
                var secondForeignKeyProperties = secondForeignKey.Properties;

                var key = joinEntityType.FindKey(firstForeignKeyProperties.Concat(secondForeignKeyProperties).ToList());
                if (key != null)
                {
                    var keyValues = new object[firstForeignKeyProperties.Count + secondForeignKeyProperties.Count];
                    var index = 0;

                    foreach (var keyProperty in firstForeignKey.PrincipalKey.Properties)
                    {
                        keyValues[index++] = firstEntry[keyProperty];
                    }

                    foreach (var keyProperty in secondForeignKey.PrincipalKey.Properties)
                    {
                        keyValues[index++] = secondEntry[keyProperty];
                    }

                    joinEntry = entry.StateManager.TryGetEntry(key, keyValues);
                    return true;
                }

                joinEntry = null;
                return false;
            }
        }

        private void FixupToDependent(
            InternalEntityEntry principalEntry,
            InternalEntityEntry dependentEntry,
            IForeignKey foreignKey,
            bool setModified,
            bool fromQuery)
        {
            SetForeignKeyProperties(dependentEntry, principalEntry, foreignKey, setModified, fromQuery);

            SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, principalEntry, fromQuery);
        }

        private void FixupToPrincipal(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey,
            bool setModified,
            bool fromQuery)
        {
            SetForeignKeyProperties(dependentEntry, principalEntry, foreignKey, setModified, fromQuery);

            ToDependentFixup(dependentEntry, principalEntry, foreignKey, fromQuery);
        }

        private void ToDependentFixup(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey,
            bool fromQuery)
        {
            var principalToDependent = foreignKey.PrincipalToDependent;
            if (foreignKey.IsUnique)
            {
                var oldDependent = principalToDependent == null ? null : principalEntry[principalToDependent];
                var oldDependentEntry = oldDependent != null
                    && !ReferenceEquals(dependentEntry.Entity, oldDependent)
                        ? dependentEntry.StateManager.TryGetEntry(oldDependent, foreignKey.DeclaringEntityType)
                        : (InternalEntityEntry)dependentEntry.StateManager
                            .GetDependentsUsingRelationshipSnapshot(principalEntry, foreignKey)
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
                        SetNavigation(oldDependentEntry, dependentToPrincipal, null, fromQuery);
                    }
                }
            }

            if (principalToDependent != null)
            {
                SetReferenceOrAddToCollection(
                    principalEntry,
                    principalToDependent,
                    dependentEntry,
                    fromQuery);
            }
        }

        private static void SetForeignKeyProperties(
            InternalEntityEntry dependentEntry,
            InternalEntityEntry principalEntry,
            IForeignKey foreignKey,
            bool setModified,
            bool fromQuery)
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
                        dependentEntry.SetProperty(dependentProperty, principalValue, fromQuery, setModified);
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
            => (principalProperty.GetKeyValueComparer())
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

            var currentPrincipal = dependentEntry.StateManager.FindPrincipal(dependentEntry, foreignKey);
            if (currentPrincipal != null
                && currentPrincipal != principalEntry)
            {
                return;
            }

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

        private void SetNavigation(InternalEntityEntry entry, INavigationBase navigation, InternalEntityEntry value, bool fromQuery)
        {
            if (navigation != null)
            {
                _changeDetector.Suspend();
                var entity = value?.Entity;
                try
                {
                    entry.SetProperty(navigation, entity, fromQuery);
                }
                finally
                {
                    _changeDetector.Resume();
                }

                entry.SetRelationshipSnapshotValue(navigation, entity);
            }
        }

        private void AddToCollection(InternalEntityEntry entry, INavigationBase navigation, InternalEntityEntry value, bool fromQuery)
        {
            if (navigation != null)
            {
                _changeDetector.Suspend();
                try
                {
                    if (entry.AddToCollection(navigation, value, fromQuery))
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

        private void RemoveFromCollection(InternalEntityEntry entry, INavigationBase navigation, InternalEntityEntry value)
        {
            _changeDetector.Suspend();
            try
            {
                if (entry.RemoveFromCollection(navigation, value))
                {
                    entry.RemoveFromCollectionSnapshot(navigation, value.Entity);
                }
            }
            finally
            {
                _changeDetector.Resume();
            }
        }

        private void SetReferenceOrAddToCollection(
            InternalEntityEntry entry,
            INavigationBase navigation,
            InternalEntityEntry value,
            bool fromQuery)
        {
            if (navigation.IsCollection)
            {
                AddToCollection(entry, navigation, value, fromQuery);
            }
            else
            {
                SetNavigation(entry, navigation, value, fromQuery);
            }
        }

        private void ResetReferenceOrRemoveCollection(
            InternalEntityEntry entry,
            INavigationBase navigation,
            InternalEntityEntry value,
            bool fromQuery)
        {
            if (navigation.IsCollection)
            {
                RemoveFromCollection(entry, navigation, value);
            }
            else if (ReferenceEquals(entry[navigation], value.Entity))
            {
                SetNavigation(entry, navigation, null, fromQuery);
            }
        }
    }
}
