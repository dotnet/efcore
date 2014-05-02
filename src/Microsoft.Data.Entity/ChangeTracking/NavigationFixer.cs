// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

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
        private readonly ClrCollectionAccessorSource _collectionAccessorSource;
        private readonly ClrPropertySetterSource _setterSource;

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
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] ClrPropertySetterSource setterSource)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(collectionAccessorSource, "collectionAccessorSource");
            Check.NotNull(setterSource, "setterSource");

            _stateManager = stateManager;
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
