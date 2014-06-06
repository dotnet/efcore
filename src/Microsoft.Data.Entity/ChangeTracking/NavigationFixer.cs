// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        public virtual void StateChanging(StateEntry entry, EntityState newState)
        {
        }

        public virtual void StateChanged(StateEntry entry, EntityState oldState)
        {
            Check.NotNull(entry, "entry");
            Check.IsDefined(oldState, "oldState");

            // TODO: Lots to do here; for now just fixup references when entity begins tracking
            if (oldState != EntityState.Unknown)
            {
                return;
            }

            // Handle case where the new entity is the dependent
            foreach (var foreignKey in entry.EntityType.ForeignKeys)
            {
                var principalEntry = _stateManager.GetPrincipal(entry, foreignKey);
                if (principalEntry != null)
                {
                    DoFixup(foreignKey, principalEntry, new[] { entry });
                }
            }

            // Handle case where the new entity is the principal
            foreach (var foreignKey in _stateManager.Model.EntityTypes.SelectMany(
                e => e.ForeignKeys.Where(f => f.ReferencedEntityType == entry.EntityType)))
            {
                var dependents = _stateManager.GetDependents(entry, foreignKey).ToArray();

                if (dependents.Length > 0)
                {
                    DoFixup(foreignKey, entry, dependents);
                }
            }
        }

        private void DoFixup(IForeignKey foreignKey, StateEntry principalEntry, StateEntry[] dependentEntries)
        {
            foreach (var navigation in _stateManager.Model.GetNavigations(foreignKey))
            {
                var accessor = _accessorSource.GetAccessor(navigation);

                if (navigation.PointsToPrincipal)
                {
                    foreach (var dependent in dependentEntries)
                    {
                        accessor.Setter.SetClrValue(dependent.Entity, principalEntry.Entity);
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
                }
            }
        }
    }
}
