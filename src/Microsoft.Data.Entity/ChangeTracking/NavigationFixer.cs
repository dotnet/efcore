// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class NavigationFixer : IEntityStateListener
    {
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
                DoFixup(stateManager, foreignKey, stateManager.GetPrincipal(entry, foreignKey), new[] { entry });
            }

            // Handle case where the new entity is the principal
            foreach (var foreignKey in stateManager.Model.EntityTypes.SelectMany(
                e => e.ForeignKeys.Where(f => f.PrincipalType == entry.EntityType)))
            {
                var dependents = stateManager.GetDependents(entry, foreignKey).ToArray();

                DoFixup(stateManager, foreignKey, entry, dependents);
            }
        }

        private static void DoFixup(
            StateManager stateManager, IForeignKey foreignKey, StateEntry principalEntry, StateEntry[] dependentEntries)
        {
            foreach (var navigation in stateManager.Model.GetNavigations(foreignKey))
            {
                foreach (var dependent in dependentEntries)
                {
                    if (navigation.EntityType == principalEntry.EntityType)
                    {
                        navigation.SetOrAddEntity(principalEntry.Entity, dependent.Entity);
                    }
                    else
                    {
                        navigation.SetOrAddEntity(dependent.Entity, principalEntry.Entity);
                    }
                }
            }
        }
    }
}
