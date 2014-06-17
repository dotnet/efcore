// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class NavigationFixer : IEntityStateListener
    {
        private readonly StateManager _stateManager;
        private readonly NavigationAccessorSource _accessorSource;
        private bool _inFixup;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected NavigationFixer()
        {
        }

        public NavigationFixer(
            [NotNull] StateManager stateManager,
            [NotNull] NavigationAccessorSource accessorSource)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(accessorSource, "accessorSource");

            _stateManager = stateManager;
            _accessorSource = accessorSource;
        }

        public virtual void ForeignKeyPropertyChanged(StateEntry entry, IProperty property, object oldValue, object newValue)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(property, "property");

            PerformFixup(() => ForeignKeyPropertyChangedAction(entry, property, oldValue, newValue));
        }

        private void ForeignKeyPropertyChangedAction(StateEntry entry, IProperty property, object oldValue, object newValue)
        {
            foreach (var foreignKey in entry.EntityType.ForeignKeys.Where(p => p.Properties.Contains(property)).Distinct())
            {
                var navigations = _stateManager.Model.GetNavigations(foreignKey).ToArray();

                var oldPrincipalEntry = _stateManager.GetPrincipal(entry, foreignKey, useForeignKeySnapshot: true);
                if (oldPrincipalEntry != null)
                {
                    Unfixup(navigations, oldPrincipalEntry, entry);
                }

                var principalEntry = _stateManager.GetPrincipal(entry, foreignKey, useForeignKeySnapshot: false);
                if (principalEntry != null)
                {
                    if (foreignKey.IsUnique)
                    {
                        var oldDependents = _stateManager.GetDependents(principalEntry, foreignKey).Where(e => e != entry).ToArray();

                        // TODO: Decide how to handle case where multiple values found (negative case)
                        if (oldDependents.Length > 0)
                        {
                            StealReference(foreignKey, oldDependents[0]);
                        }
                    }

                    DoFixup(navigations, principalEntry, new[] { entry });
                }
            }
        }

        public virtual void NavigationReferenceChanged(StateEntry entry, INavigation navigation, object oldValue, object newValue)
        {
            Check.NotNull(entry, "entry");
            Check.NotNull(navigation, "navigation");

            PerformFixup(() => NavigationReferenceChangedAction(entry, navigation, oldValue, newValue));
        }

        private void NavigationReferenceChangedAction(StateEntry entry, INavigation navigation, object oldValue, object newValue)
        {
            if (navigation.PointsToPrincipal)
            {
                var dependentProperties = navigation.ForeignKey.Properties;

                if (newValue != null)
                {
                    // TODO: What if the principal is not yet being tracked?
                    var newPrincipalEntry = _stateManager.GetOrCreateEntry(newValue);

                    var principalProperties = navigation.ForeignKey.ReferencedProperties;
                    Contract.Assert(principalProperties.Count == dependentProperties.Count);

                    for (var i = 0; i < dependentProperties.Count; i++)
                    {
                        // TODO: Consider nullable/non-nullable assignment issues
                        entry[dependentProperties[i]] = newPrincipalEntry[principalProperties[i]];
                    }
                }
                else
                {
                    foreach (var dependentProperty in dependentProperties)
                    {
                        // TODO: Conceptual nulls
                        entry[dependentProperty] = null;
                    }
                }
            }
        }

        public virtual void StateChanging(StateEntry entry, EntityState newState)
        {
        }

        public virtual void StateChanged(StateEntry entry, EntityState oldState)
        {
            Check.NotNull(entry, "entry");
            Check.IsDefined(oldState, "oldState");

            if (oldState != EntityState.Unknown)
            {
                return;
            }

            PerformFixup(() => InitialFixup(entry, oldState));
        }

        private void InitialFixup(StateEntry entry, EntityState oldState)
        {
            var entityType = entry.EntityType;

            // Handle case where the new entity is the dependent
            foreach (var foreignKey in entityType.ForeignKeys)
            {
                var principalEntry = _stateManager.GetPrincipal(entry, foreignKey, useForeignKeySnapshot: false);
                if (principalEntry != null)
                {
                    DoFixup(foreignKey, principalEntry, new[] { entry });
                }
            }

            // Handle case where the new entity is the principal
            foreach (var foreignKey in _stateManager.Model.EntityTypes.SelectMany(
                e => e.ForeignKeys.Where(f => f.ReferencedEntityType == entityType)))
            {
                var dependents = _stateManager.GetDependents(entry, foreignKey).ToArray();

                if (dependents.Length > 0)
                {
                    DoFixup(foreignKey, entry, dependents);
                }
            }
        }

        private void PerformFixup(Action fixupAction)
        {
            if (_inFixup)
            {
                return;
            }

            try
            {
                _inFixup = true;

                fixupAction();
            }
            finally
            {
                _inFixup = false;
            }
        }

        private void DoFixup(IForeignKey foreignKey, StateEntry principalEntry, StateEntry[] dependentEntries)
        {
            DoFixup(_stateManager.Model.GetNavigations(foreignKey).ToArray(), principalEntry, dependentEntries);
        }

        private void DoFixup(IEnumerable<INavigation> navigations, StateEntry principalEntry, StateEntry[] dependentEntries)
        {
            foreach (var navigation in navigations)
            {
                var accessor = _accessorSource.GetAccessor(navigation);

                if (navigation.PointsToPrincipal)
                {
                    foreach (var dependent in dependentEntries)
                    {
                        accessor.Setter.SetClrValue(dependent.Entity, principalEntry.Entity);
                        dependent.RelationshipsSnapshot.TakeSnapshot(navigation);
                    }
                }
                else
                {
                    var collectionAccessor = accessor as CollectionNavigationAccessor;
                    if (collectionAccessor != null)
                    {
                        foreach (var dependent in dependentEntries)
                        {
                            if (!collectionAccessor.Collection.Contains(principalEntry.Entity, dependent.Entity))
                            {
                                collectionAccessor.Collection.Add(principalEntry.Entity, dependent.Entity);
                            }
                        }
                    }
                    else
                    {
                        // TODO: Decide how to handle case where multiple values match non-collection nav prop
                        accessor.Setter.SetClrValue(principalEntry.Entity, dependentEntries.Single().Entity);
                    }
                    principalEntry.RelationshipsSnapshot.TakeSnapshot(navigation);
                }
            }
        }

        private void Unfixup(IEnumerable<INavigation> navigations, StateEntry oldPrincipalEntry, StateEntry dependentEntry)
        {
            foreach (var navigation in navigations)
            {
                var accessor = _accessorSource.GetAccessor(navigation);

                if (navigation.PointsToPrincipal)
                {
                    accessor.Setter.SetClrValue(dependentEntry.Entity, null);
                    dependentEntry.RelationshipsSnapshot.TakeSnapshot(navigation);
                }
                else
                {
                    var collectionAccessor = accessor as CollectionNavigationAccessor;
                    if (collectionAccessor != null)
                    {
                        if (collectionAccessor.Collection.Contains(oldPrincipalEntry.Entity, dependentEntry.Entity))
                        {
                            collectionAccessor.Collection.Remove(oldPrincipalEntry.Entity, dependentEntry.Entity);
                        }
                    }
                    else
                    {
                        accessor.Setter.SetClrValue(oldPrincipalEntry.Entity, null);
                    }
                    oldPrincipalEntry.RelationshipsSnapshot.TakeSnapshot(navigation);
                }
            }
        }

        private void StealReference(IForeignKey foreignKey, StateEntry dependentEntry)
        {
            foreach (var navigation in dependentEntry.EntityType.Navigations.Where(n => n.ForeignKey == foreignKey))
            {
                if (navigation.PointsToPrincipal)
                {
                    _accessorSource.GetAccessor(navigation).Setter.SetClrValue(dependentEntry.Entity, null);
                    dependentEntry.RelationshipsSnapshot.TakeSnapshot(navigation);
                }
            }

            var nullableProperties = foreignKey.Properties.Where(p => p.IsNullable).ToArray();
            if (nullableProperties.Length > 0)
            {
                foreach (var property in nullableProperties)
                {
                    dependentEntry[property] = null;
                }
            }
            else
            {
                // TODO: Handle conceptual null
            }
        }
    }
}
