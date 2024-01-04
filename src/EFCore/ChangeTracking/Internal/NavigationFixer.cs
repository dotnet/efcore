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
public class NavigationFixer : INavigationFixer
{
    private IList<(
        InternalEntityEntry Entry,
        InternalEntityEntry OtherEntry,
        ISkipNavigation SkipNavigation,
        bool FromQuery,
        bool SetModified)>? _danglingJoinEntities;

    private readonly IEntityGraphAttacher _attacher;
    private readonly IEntityMaterializerSource _entityMaterializerSource;
    private bool _inFixup;
    private bool _inAttachGraph;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public NavigationFixer(
        IEntityGraphAttacher attacher,
        IEntityMaterializerSource entityMaterializerSource)
    {
        _attacher = attacher;
        _entityMaterializerSource = entityMaterializerSource;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool BeginDelayedFixup()
    {
        if (_inAttachGraph)
        {
            return false;
        }

        if (_danglingJoinEntities != null
            && _danglingJoinEntities.Any())
        {
            throw new InvalidOperationException(CoreStrings.InvalidDbContext);
        }

        _inAttachGraph = true;

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void CompleteDelayedFixup()
    {
        _inAttachGraph = false;
        if (_danglingJoinEntities != null
            && _danglingJoinEntities.Any())
        {
            var dangles = _danglingJoinEntities.ToList();
            _danglingJoinEntities.Clear();
            foreach (var arguments in dangles)
            {
                FindOrCreateJoinEntry(arguments);
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void AbortDelayedFixup()
    {
        _inAttachGraph = false;
        _danglingJoinEntities?.Clear();
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
        object? oldValue,
        object? newValue)
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

        var delayingFixup = BeginDelayedFixup();
        try
        {
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
                                (InternalEntityEntry?)stateManager.GetDependents(newTargetEntry, foreignKey).FirstOrDefault();
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
                        UndeleteDependent(entry, newTargetEntry);
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
                        && (entry.EntityType.GetNavigations().All(
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
                    UndeleteDependent(entry, newTargetEntry);
                    SetNavigation(newTargetEntry, inverse, entry, fromQuery: false);
                }
            }

            entry.SetIsLoaded(navigation, loaded: newValue != null);
        }
        finally
        {
            if (delayingFixup)
            {
                CompleteDelayedFixup();
            }
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
                entry.EntityState == EntityState.Added && !navigation.IsOnDependent ? EntityState.Added : EntityState.Modified,
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
                var delayingFixup = BeginDelayedFixup();
                try
                {
                    if (navigationBase is ISkipNavigation skipNavigation)
                    {
                        var joinEntry = FindJoinEntry(entry, oldTargetEntry, skipNavigation);

                        joinEntry?.SetEntityState(
                            joinEntry.EntityState != EntityState.Added
                                ? EntityState.Deleted
                                : EntityState.Detached);

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
                    if (delayingFixup)
                    {
                        CompleteDelayedFixup();
                    }
                }
            }
        }

        foreach (var newValue in added)
        {
            var newTargetEntry = stateManager.GetOrCreateEntry(newValue, targetEntityType);
            if (newTargetEntry.EntityState != EntityState.Detached)
            {
                var delayingFixup = BeginDelayedFixup();
                try
                {
                    if (navigationBase is ISkipNavigation skipNavigation)
                    {
                        FindOrCreateJoinEntry(
                            (entry, newTargetEntry, skipNavigation, FromQuery: false, SetModified: true));

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
                        UndeleteDependent(newTargetEntry, entry);

                        // Set the inverse navigation to point to this principal
                        SetNavigation(newTargetEntry, inverse, entry, fromQuery: false);
                    }
                }
                finally
                {
                    if (delayingFixup)
                    {
                        CompleteDelayedFixup();
                    }
                }
            }
            else
            {
                stateManager.RecordReferencedUntrackedEntity(newValue, navigationBase, entry);

                _attacher.AttachGraph(
                    newTargetEntry,
                    EntityState.Added,
                    entry.EntityState == EntityState.Added ? EntityState.Added : EntityState.Modified,
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
        object? oldValue,
        object? newValue)
    {
        if (entry.EntityState == EntityState.Detached)
        {
            return;
        }

        var delayingFixup = BeginDelayedFixup();
        try
        {
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
                                = (InternalEntityEntry?)stateManager
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

                if (newValue == null
                    && foreignKey is { IsRequired: true, DeleteBehavior: DeleteBehavior.Cascade or DeleteBehavior.ClientCascade })
                {
                    entry.HandleNullForeignKey(property);
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
                        if (dependentEntry.EntityState == EntityState.Deleted)
                        {
                            continue;
                        }

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
            if (delayingFixup)
            {
                CompleteDelayedFixup();
            }
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
    public virtual void TrackedFromQuery(InternalEntityEntry entry)
        => InitialFixup(entry, null, fromQuery: true);

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
        var delayingFixup = BeginDelayedFixup();
        try
        {
            if (oldState == EntityState.Detached)
            {
                InitialFixup(entry, null, fromQuery);
            }
            else if (oldState is EntityState.Deleted or EntityState.Added
                     && entry.EntityState == EntityState.Detached)
            {
                DeleteFixup(entry);
            }
        }
        finally
        {
            if (delayingFixup)
            {
                CompleteDelayedFixup();
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void FixupResolved(
        InternalEntityEntry entry,
        InternalEntityEntry duplicateEntry)
    {
        var delayingFixup = BeginDelayedFixup();
        try
        {
            InitialFixup(entry, duplicateEntry, fromQuery: false);
        }
        finally
        {
            if (delayingFixup)
            {
                CompleteDelayedFixup();
            }
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

            foreach (var skipNavigation in foreignKey.GetReferencingSkipNavigations())
            {
                Check.DebugAssert(
                    skipNavigation.IsCollection,
                    "Issue #21673. Non-collection skip navigations not supported.");

                if (StringComparer.Ordinal.Compare(skipNavigation.Name, skipNavigation.Inverse.Name) < 0)
                {
                    // Only do this once for any given pair of skip navigations
                    continue;
                }

                var principal = stateManager.FindPrincipal(entry, foreignKey);
                if (principal != null
                    && principal.EntityState != EntityState.Deleted)
                {
                    var otherPrincipal = stateManager.FindPrincipal(entry, skipNavigation.Inverse.ForeignKey);
                    if (otherPrincipal != null
                        && otherPrincipal.EntityState != EntityState.Deleted)
                    {
                        RemoveFromCollection(principal, skipNavigation, otherPrincipal);
                        RemoveFromCollection(otherPrincipal, skipNavigation.Inverse, principal);
                    }
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
                if (foreignKey.DeleteBehavior != DeleteBehavior.ClientNoAction)
                {
                    ConditionallyNullForeignKeyProperties(dependentEntry, entry, foreignKey);
                }

                // TODO: Don't fixup deleted entries, #26074
                if (dependentToPrincipal != null
                    && !IsAmbiguous(dependentEntry)
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
        InternalEntityEntry? duplicateEntry,
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
                if (CanOverrideCurrentValue(entry, navigation, principalEntry, fromQuery)
                    && !IsAmbiguous(principalEntry))
                {
                    SetNavigation(entry, navigation, principalEntry, fromQuery);
                    ToDependentFixup(entry, principalEntry, foreignKey, fromQuery);
                }
            }

            FixupSkipNavigations(entry, foreignKey, fromQuery);
        }

        foreach (var foreignKey in entityType.GetReferencingForeignKeys())
        {
            if (foreignKey.DeclaringEntityType.FindPrimaryKey() != null)
            {
                var dependents = stateManager.GetDependents(entry, foreignKey);
                if (foreignKey.IsUnique)
                {
                    var dependentEntry = (InternalEntityEntry?)dependents.FirstOrDefault();
                    if (dependentEntry != null
                        && dependentEntry.EntityState != EntityState.Deleted)
                    {
                        var toDependent = foreignKey.PrincipalToDependent;
                        if (CanOverrideCurrentValue(entry, toDependent, dependentEntry, fromQuery)
                            && (!fromQuery || CanOverrideCurrentValue(dependentEntry, foreignKey.DependentToPrincipal, entry, fromQuery))
                            && !IsAmbiguous(dependentEntry))
                        {
                            SetNavigation(entry, toDependent, dependentEntry, fromQuery);
                            SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, entry, fromQuery);
                        }
                    }
                }
                else
                {
                    foreach (InternalEntityEntry dependentEntry in dependents)
                    {
                        if (dependentEntry.EntityState != EntityState.Deleted
                            && !IsAmbiguous(dependentEntry)
                            && (!fromQuery || CanOverrideCurrentValue(dependentEntry, foreignKey.DependentToPrincipal, entry, fromQuery)))
                        {
                            SetNavigation(dependentEntry, foreignKey.DependentToPrincipal, entry, fromQuery);
                            AddToCollection(entry, foreignKey.PrincipalToDependent, dependentEntry, fromQuery);

                            foreach (var skipNavigation in foreignKey.GetReferencingSkipNavigations())
                            {
                                Check.DebugAssert(
                                    skipNavigation.IsCollection,
                                    "Issue #21673. Non-collection skip navigations not supported.");

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

                        navigationValue = duplicateEntry?[principalToDependent];
                        if (navigationValue != null)
                        {
                            if (principalToDependent.IsCollection)
                            {
                                foreach (var dependentEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                                {
                                    var dependentEntry = stateManager.TryGetEntry(dependentEntity, foreignKey.DeclaringEntityType);
                                    if (dependentEntry == null
                                        || dependentEntry.EntityState == EntityState.Detached)
                                    {
                                        // If dependents in collection are not yet tracked, then save them away so that
                                        // when we start tracking them we can come back and fixup this principal to them
                                        stateManager.RecordReferencedUntrackedEntity(dependentEntity, principalToDependent, entry);
                                    }
                                }
                            }
                            else
                            {
                                var dependentEntry = stateManager.TryGetEntry(navigationValue, principalToDependent.TargetEntityType);
                                if (dependentEntry == null
                                    || dependentEntry.EntityState == EntityState.Detached)
                                {
                                    // If dependent is not yet tracked, then save it away so that
                                    // when we start tracking it we can come back and fixup this principal to it
                                    stateManager.RecordReferencedUntrackedEntity(navigationValue, principalToDependent, entry);
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
                            FindOrCreateJoinEntry((entry, otherEntry, skipNavigation, fromQuery, setModified));

                            Check.DebugAssert(
                                skipNavigation.Inverse.IsCollection,
                                "Issue #21673. Non-collection skip navigations not supported.");

                            AddToCollection(otherEntry, skipNavigation.Inverse, entry, fromQuery);
                        }

                        entry.AddToCollectionSnapshot(skipNavigation, otherEntity);
                    }
                }

                navigationValue = duplicateEntry?[skipNavigation];
                if (navigationValue != null)
                {
                    foreach (var otherEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                    {
                        var otherEntry = stateManager.TryGetEntry(otherEntity, skipNavigation.Inverse.DeclaringEntityType);
                        if (otherEntry == null
                            || otherEntry.EntityState == EntityState.Detached)
                        {
                            // If dependents in collection are not yet tracked, then save them away so that
                            // when we start tracking them we can come back and fixup this principal to them
                            stateManager.RecordReferencedUntrackedEntity(otherEntity, skipNavigation, entry);
                        }
                    }
                }
            }

            // If the entity was previously referenced while it was still untracked, go back and do the fixup
            // that we would have done then now that the entity is tracked.
            foreach (var (navigationBase, internalEntityEntry) in stateManager.GetRecordedReferrers(entry.Entity, clear: true))
            {
                DelayedFixup(internalEntityEntry, navigationBase, entry, fromQuery);
            }
        }
    }

    private static bool IsAmbiguous(InternalEntityEntry dependentEntry)
        => dependentEntry.EntityState is EntityState.Detached or EntityState.Deleted
            && (dependentEntry.SharedIdentityEntry != null
                || dependentEntry.EntityType.HasSharedClrType
                && dependentEntry.StateManager.TryGetEntry(dependentEntry.Entity, throwOnNonUniqueness: false) != dependentEntry);

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
                FindOrCreateJoinEntry((entry, referencedEntry, skipNavigation, fromQuery, setModified));

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
                        if (entry.CollectionContains(navigation, referencedEntry.Entity))
                        {
                            FixupToDependent(entry, referencedEntry, navigation.ForeignKey, setModified, fromQuery);
                        }
                    }
                    else
                    {
                        FixupToDependent(
                            entry,
                            referencedEntry,
                            navigation.ForeignKey,
                            referencedEntry.Entity == navigationValue && setModified,
                            fromQuery);
                    }
                }
                else
                {
                    FixupToPrincipal(
                        entry,
                        referencedEntry,
                        navigation.ForeignKey,
                        referencedEntry.Entity == navigationValue && setModified,
                        fromQuery);

                    FixupSkipNavigations(entry, navigation.ForeignKey, fromQuery);
                }
            }
        }
    }

    private void FixupSkipNavigations(InternalEntityEntry entry, IForeignKey foreignKey, bool fromQuery)
    {
        foreach (var skipNavigation in foreignKey.GetReferencingSkipNavigations())
        {
            var leftEntry = entry.StateManager.FindPrincipal(entry, foreignKey);
            if (leftEntry != null)
            {
                var rightEntry = entry.StateManager.FindPrincipal(entry, skipNavigation.Inverse.ForeignKey);
                if (rightEntry != null)
                {
                    AddToCollection(leftEntry, skipNavigation, rightEntry, fromQuery);
                    AddToCollection(rightEntry, skipNavigation.Inverse, leftEntry, fromQuery);
                }
            }
        }
    }

    private void FindOrCreateJoinEntry(
        (InternalEntityEntry Entry,
            InternalEntityEntry OtherEntry,
            ISkipNavigation SkipNavigation,
            bool FromQuery,
            bool SetModified) arguments)
    {
        var joinEntry = FindJoinEntry(arguments.Entry, arguments.OtherEntry, arguments.SkipNavigation);
        if (joinEntry != null)
        {
            SetForeignKeyProperties(
                joinEntry, arguments.Entry, arguments.SkipNavigation.ForeignKey, arguments.SetModified, arguments.FromQuery);
            SetForeignKeyProperties(
                joinEntry, arguments.OtherEntry, arguments.SkipNavigation.Inverse.ForeignKey, arguments.SetModified,
                arguments.FromQuery);
        }
        else if (!_inAttachGraph)
        {
            var joinEntityType = arguments.SkipNavigation.JoinEntityType;
            var joinEntity = joinEntityType.GetOrCreateEmptyMaterializer(_entityMaterializerSource)
                (new MaterializationContext(ValueBuffer.Empty, arguments.Entry.Context));

            joinEntry = arguments.Entry.StateManager.GetOrCreateEntry(joinEntity, joinEntityType);

            SetForeignKeyProperties(
                joinEntry, arguments.Entry, arguments.SkipNavigation.ForeignKey, arguments.SetModified, arguments.FromQuery);
            SetNavigation(joinEntry, arguments.SkipNavigation.ForeignKey.DependentToPrincipal, arguments.Entry, arguments.FromQuery);
            SetForeignKeyProperties(
                joinEntry, arguments.OtherEntry, arguments.SkipNavigation.Inverse.ForeignKey, arguments.SetModified,
                arguments.FromQuery);
            SetNavigation(joinEntry, arguments.SkipNavigation.Inverse.ForeignKey.DependentToPrincipal, arguments.OtherEntry, arguments.FromQuery);

            joinEntry.SetEntityState(
                arguments.SetModified
                || arguments.Entry.EntityState == EntityState.Added
                || arguments.OtherEntry.EntityState == EntityState.Added
                    ? EntityState.Added
                    : EntityState.Unchanged);
        }
        else
        {
            _danglingJoinEntities ??=
                new List<(
                    InternalEntityEntry Entry,
                    InternalEntityEntry OtherEntry,
                    ISkipNavigation SkipNavigation,
                    bool FromQuery,
                    bool SetModified)>();

            _danglingJoinEntities.Add(arguments);
        }
    }

    private static InternalEntityEntry? FindJoinEntry(
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
            out InternalEntityEntry? joinEntry)
        {
            var key = joinEntityType.FindKey(new[] { firstForeignKey.Properties[0], secondForeignKey.Properties[0] });
            if (key != null)
            {
                joinEntry = entry.StateManager.TryGetEntry(
                    key,
                    new[]
                    {
                        firstEntry[firstForeignKey.PrincipalKey.Properties[0]], secondEntry[secondForeignKey.PrincipalKey.Properties[0]]
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
            out InternalEntityEntry? joinEntry)
        {
            var firstForeignKeyProperties = firstForeignKey.Properties;
            var secondForeignKeyProperties = secondForeignKey.Properties;

            var key = joinEntityType.FindKey(firstForeignKeyProperties.Concat(secondForeignKeyProperties).ToList());
            if (key != null)
            {
                var keyValues = new object?[firstForeignKeyProperties.Count + secondForeignKeyProperties.Count];
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
                    : (InternalEntityEntry?)dependentEntry.StateManager
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
                dependentEntry.PropagateValue(principalEntry, principalProperty, dependentProperty, fromQuery, setModified);

                dependentEntry.StateManager.UpdateDependentMap(dependentEntry, foreignKey);
                dependentEntry.SetRelationshipSnapshotValue(dependentProperty, principalValue);
            }
        }
    }

    private static void UndeleteDependent(
        InternalEntityEntry dependentEntry,
        InternalEntityEntry principalEntry)
    {
        if (dependentEntry.EntityState == EntityState.Deleted
            && principalEntry.EntityState is EntityState.Unchanged or EntityState.Modified)
        {
            dependentEntry.SetEntityState(EntityState.Modified);
        }
    }

    private static bool PrincipalValueEqualsDependentValue(
        IProperty principalProperty,
        object? dependentValue,
        object? principalValue)
        => principalProperty.GetKeyValueComparer().Equals(dependentValue, principalValue);

    private void ConditionallyNullForeignKeyProperties(
        InternalEntityEntry dependentEntry,
        InternalEntityEntry? principalEntry,
        IForeignKey foreignKey)
    {
        var currentPrincipal = dependentEntry.StateManager.FindPrincipal(dependentEntry, foreignKey);
        if (currentPrincipal != null
            && currentPrincipal != principalEntry)
        {
            return;
        }

        var hasOnlyKeyProperties = true;
        foreignKey.GetPropertiesWithMinimalOverlapIfPossible(out var dependentProperties, out var principalProperties);

        if (principalEntry != null
            && principalEntry.EntityState != EntityState.Detached)
        {
            for (var i = 0; i < dependentProperties.Count; i++)
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

        for (var i = 0; i < dependentProperties.Count; i++)
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
            try
            {
                _inFixup = true;
                switch (dependentEntry.EntityState)
                {
                    case EntityState.Added:
                        dependentEntry.SetEntityState(EntityState.Detached);
                        DeleteFixup(dependentEntry);
                        break;
                    case EntityState.Unchanged:
                    case EntityState.Modified:
                        dependentEntry.SetEntityState(
                            dependentEntry.SharedIdentityEntry != null ? EntityState.Detached : EntityState.Deleted);
                        DeleteFixup(dependentEntry);
                        break;
                }
            }
            finally
            {
                _inFixup = false;
            }
        }
    }

    private static bool CanOverrideCurrentValue(
        InternalEntityEntry entry,
        INavigationBase? navigation,
        InternalEntityEntry value,
        bool fromQuery)
    {
        var existingValue = navigation == null ? null : entry[navigation];
        if (existingValue == null
            || existingValue == value.Entity)
        {
            return true;
        }

        if (!fromQuery)
        {
            return false;
        }

        var existingEntry = entry.StateManager.TryGetEntry(existingValue, throwOnNonUniqueness: false);
        if (existingEntry == null)
        {
            return true;
        }

        SetForeignKeyProperties(entry, existingEntry, ((INavigation)navigation!).ForeignKey, setModified: true, fromQuery);

        return false;
    }

    private void SetNavigation(InternalEntityEntry entry, INavigationBase? navigation, InternalEntityEntry? value, bool fromQuery)
    {
        if (navigation != null)
        {
            _inFixup = true;
            var entity = value?.Entity;
            try
            {
                entry.SetProperty(navigation, entity, fromQuery);
            }
            finally
            {
                _inFixup = false;
            }

            entry.SetRelationshipSnapshotValue(navigation, entity);
        }
    }

    private void AddToCollection(InternalEntityEntry entry, INavigationBase? navigation, InternalEntityEntry value, bool fromQuery)
    {
        if (navigation != null)
        {
            _inFixup = true;
            try
            {
                if (entry.AddToCollection(navigation, value.Entity, fromQuery))
                {
                    entry.AddToCollectionSnapshot(navigation, value.Entity);
                }
            }
            finally
            {
                _inFixup = false;
            }
        }
    }

    private void RemoveFromCollection(InternalEntityEntry entry, INavigationBase navigation, InternalEntityEntry value)
    {
        _inFixup = true;
        try
        {
            if (entry.RemoveFromCollection(navigation, value.Entity))
            {
                entry.RemoveFromCollectionSnapshot(navigation, value.Entity);
            }
        }
        finally
        {
            _inFixup = false;
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
