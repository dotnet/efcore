// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class NavigationFixer : IEntityStateListener
    {
        private readonly ClrCollectionAccessorSource _collectionAccessorSource;
        private readonly ClrPropertySetterSource _setterSource;

        public NavigationFixer(
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource, [NotNull] ClrPropertySetterSource setterSource)
        {
            Check.NotNull(collectionAccessorSource, "collectionAccessorSource");
            Check.NotNull(setterSource, "setterSource");

            _collectionAccessorSource = collectionAccessorSource;
            _setterSource = setterSource;
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

            var stateManager = entry.StateManager;

            // Handle case where the new entity is the dependent
            foreach (var foreignKey in entry.EntityType.ForeignKeys)
            {
                var principalEntry = stateManager.GetPrincipal(entry, foreignKey);
                if (principalEntry != null)
                {
                    DoFixup(stateManager, foreignKey, principalEntry, new[] { entry });
                }
            }

            // Handle case where the new entity is the principal
            foreach (var foreignKey in stateManager.Model.EntityTypes.SelectMany(
                e => e.ForeignKeys.Where(f => f.ReferencedEntityType == entry.EntityType)))
            {
                var dependents = stateManager.GetDependents(entry, foreignKey).ToArray();

                if (dependents.Length > 0)
                {
                    DoFixup(stateManager, foreignKey, entry, dependents);
                }
            }
        }

        private void DoFixup(
            StateManager stateManager, IForeignKey foreignKey, StateEntry principalEntry, StateEntry[] dependentEntries)
        {
            foreach (var navigation in stateManager.Model.GetNavigations(foreignKey))
            {
                if (navigation.EntityType == principalEntry.EntityType)
                {
                    var accessor = _collectionAccessorSource.GetAccessor(navigation);

                    foreach (var dependent in dependentEntries)
                    {
                        if (!accessor.Contains(principalEntry.Entity, dependent.Entity))
                        {
                            accessor.Add(principalEntry.Entity, dependent.Entity);
                        }
                    }
                }
                else
                {
                    var accessor = _setterSource.GetAccessor(navigation);

                    foreach (var dependent in dependentEntries)
                    {
                        accessor.SetClrValue(dependent.Entity, principalEntry.Entity);
                    }
                }
            }
        }
    }
}
